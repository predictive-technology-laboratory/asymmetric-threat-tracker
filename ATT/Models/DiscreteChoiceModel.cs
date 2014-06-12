#region copyright
// Copyright 2013-2014 The Rector & Visitors of the University of Virginia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using System.Reflection;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using LAIR.ResourceAPIs.PostgreSQL;
using LAIR.Collections.Generic;
using LAIR.MachineLearning;
using LAIR.Extensions;
using NpgsqlTypes;
using System.Threading;
using System.Drawing;
using PTL.ATT.Evaluation;
using PTL.ATT.Smoothers;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace PTL.ATT.Models
{
    [Serializable]
    public abstract class DiscreteChoiceModel : IDiscreteChoiceModel
    {
        #region static members
        public const string Table = "model";

        public class Columns
        {
            public const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Model = "model";
            [Reflector.Insert]
            public const string PredictionAreaId = "prediction_area_id";
            [Reflector.Insert]
            public const string TrainingAreaId = "training_area_id";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return Reflector.GetSelectColumns(Table, typeof(Columns)); } }
        }

        [ConnectionPool.CreateTable(typeof(Area))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Id + " SERIAL PRIMARY KEY," +
                   Columns.Model + " BYTEA," +
                   Columns.PredictionAreaId + " INT REFERENCES " + Area.Table + " ON DELETE RESTRICT," +
                   Columns.TrainingAreaId + " INT REFERENCES " + Area.Table + " ON DELETE RESTRICT);";
        }

        public static IEnumerable<DiscreteChoiceModel> GetAll(bool excludeThoseCopiedForPredictions)
        {
            List<DiscreteChoiceModel> models = new List<DiscreteChoiceModel>();
            BinaryFormatter bf = new BinaryFormatter();
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Table);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                DiscreteChoiceModel model = bf.Deserialize(new MemoryStream(reader[Table + "_" + Columns.Model] as byte[])) as DiscreteChoiceModel;
                if (!excludeThoseCopiedForPredictions || (!model.HasMadePredictions && !model.IsMakingAPrediction))
                    models.Add(model);
            }

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return models;
        }

        public static List<DiscreteChoiceModel> GetForArea(Area area, bool excludeThoseCopiedForPredictions)
        {
            List<DiscreteChoiceModel> models = new List<DiscreteChoiceModel>();
            foreach (DiscreteChoiceModel model in GetAll(excludeThoseCopiedForPredictions))
                if (model.TrainingArea.Id == area.Id)
                    models.Add(model);

            return models;
        }

        #region evaluation
        public const string OptimalSeriesName = "Optimal";

        public static void Evaluate(Prediction prediction, int plotWidth, int plotHeight)
        {
            List<Incident> newIncidents = new List<Incident>();
            DateTime newIncidentsStart = prediction.PredictionStartTime;
            if (prediction.MostRecentlyEvaluatedIncidentTime >= newIncidentsStart)
                newIncidentsStart = prediction.MostRecentlyEvaluatedIncidentTime + new TimeSpan(0, 0, 0, 0, 1);

            foreach (Incident i in Incident.Get(newIncidentsStart, prediction.PredictionEndTime, prediction.PredictionArea, prediction.Model.IncidentTypes.ToArray()))
                newIncidents.Add(i);

            if (newIncidents.Count == 0)
                return;

            List<Incident> oldIncidents = new List<Incident>();
            DateTime oldIncidentsEnd = newIncidentsStart - new TimeSpan(0, 0, 0, 0, 1);
            if (oldIncidentsEnd >= prediction.PredictionStartTime)
                foreach (Incident i in Incident.Get(prediction.PredictionStartTime, oldIncidentsEnd, prediction.PredictionArea, prediction.Model.IncidentTypes.ToArray()))
                    oldIncidents.Add(i);

            IEnumerable<Incident> incidents = oldIncidents.Union(newIncidents);

            DiscreteChoiceModel model = prediction.Model;

            long sliceTicks = -1;
            if (model is TimeSliceDCM)
                sliceTicks = (model as TimeSliceDCM).TimeSliceTicks;

            prediction.AssessmentPlots.Clear();
            int sliceNum = 1;
            Dictionary<long, Dictionary<string, int>> sliceLocationTrueCount = SurveillancePlot.GetSliceLocationTrueCount(incidents, prediction);
            Dictionary<long, Dictionary<string, List<double>>> sliceLocationThreats = SurveillancePlot.GetSliceLocationThreats(prediction);
            foreach (long slice in sliceLocationTrueCount.Keys.OrderBy(slice => slice))
            {
                string slicePlotTitle = prediction.Name;
                if (sliceTicks > 0)
                {
                    DateTime sliceStart = new DateTime(slice * sliceTicks);
                    DateTime sliceEnd = sliceStart + new TimeSpan(sliceTicks - 1);
                    slicePlotTitle += Environment.NewLine +
                                      sliceStart.ToShortDateString() + " " + sliceStart.ToShortTimeString() + " - " +
                                      Environment.NewLine +
                                      sliceEnd.ToShortDateString() + " " + sliceEnd.ToShortTimeString();
                }

                Dictionary<string, List<PointF>> seriesPoints = new Dictionary<string, List<PointF>>();
                seriesPoints.Add("Slice " + sliceNum++, SurveillancePlot.GetSurveillancePlotPoints(sliceLocationTrueCount[slice], sliceLocationThreats[slice], true, true));
                seriesPoints.Add(OptimalSeriesName, SurveillancePlot.GetOptimalSurveillancePlotPoints(sliceLocationTrueCount[slice], sliceLocationThreats[slice], true, true));
                prediction.AssessmentPlots.Add(new SurveillancePlot(slicePlotTitle, seriesPoints, plotHeight, plotWidth, Plot.Format.JPEG, 2));
            }

            if (sliceLocationTrueCount.Count > 1)
            {
                Dictionary<string, int> overallLocationTrueCount = SurveillancePlot.GetOverallLocationTrueCount(incidents, prediction);
                Dictionary<string, List<double>> overallLocationThreats = SurveillancePlot.GetOverallLocationThreats(prediction);
                Dictionary<string, List<PointF>> seriesPoints = new Dictionary<string, List<PointF>>();
                seriesPoints.Add("Overall", SurveillancePlot.GetSurveillancePlotPoints(overallLocationTrueCount, overallLocationThreats, true, true));
                seriesPoints.Add(OptimalSeriesName, SurveillancePlot.GetOptimalSurveillancePlotPoints(overallLocationTrueCount, overallLocationThreats, true, true));
                prediction.AssessmentPlots.Add(new SurveillancePlot(prediction.Name, seriesPoints, plotHeight, plotWidth, Plot.Format.JPEG, 2));
            }

            prediction.MostRecentlyEvaluatedIncidentTime = incidents.Max(i => i.Time);
            prediction.ReleaseLazyLoadedData();
        }

        public static Plot EvaluateAggregate(IEnumerable<Prediction> predictions, int plotWidth, int plotHeight, string seriesName, string plotTitle)
        {
            Dictionary<string, int> aggregateLocationTrueCount = new Dictionary<string, int>();
            Dictionary<string, List<double>> aggregateLocationThreats = new Dictionary<string, List<double>>();
            foreach (Prediction prediction in predictions)
            {
                IEnumerable<Incident> incidents = Incident.Get(prediction.PredictionStartTime, prediction.PredictionEndTime, prediction.PredictionArea, prediction.Model.IncidentTypes.ToArray());
                Dictionary<string, int> locationTrueCount = SurveillancePlot.GetOverallLocationTrueCount(incidents, prediction);
                foreach (string location in locationTrueCount.Keys)
                    aggregateLocationTrueCount.Add(prediction.Id + "-" + location, locationTrueCount[location]);

                Dictionary<string, List<double>> locationThreats = SurveillancePlot.GetOverallLocationThreats(prediction);
                foreach (string location in locationThreats.Keys)
                    aggregateLocationThreats.Add(prediction.Id + "-" + location, locationThreats[location]);

                prediction.ReleaseLazyLoadedData();
            }

            Dictionary<string, List<PointF>> seriesPoints = new Dictionary<string, List<PointF>>();
            seriesPoints.Add(seriesName, SurveillancePlot.GetSurveillancePlotPoints(aggregateLocationTrueCount, aggregateLocationThreats, true, true));
            seriesPoints.Add(OptimalSeriesName, SurveillancePlot.GetOptimalSurveillancePlotPoints(aggregateLocationTrueCount, aggregateLocationThreats, true, true));

            return new SurveillancePlot(plotTitle, seriesPoints, plotHeight, plotWidth, Plot.Format.JPEG, 2);
        }
        #endregion
        #endregion

        private int _id;
        private string _name;
        private int _pointSpacing;
        private Set<string> _incidentTypes;
        private Area _trainingArea;
        private DateTime _trainingStart;
        private DateTime _trainingEnd;
        private List<Smoother> _smoothers;
        private string _modelDirectory;
        private Area _predictionArea;

        #region properties
        public int Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                Update();
            }
        }

        public int PointSpacing
        {
            get { return _pointSpacing; }
            set
            {
                _pointSpacing = value;
                Update();
            }
        }

        public Set<string> IncidentTypes
        {
            get { return _incidentTypes; }
            set
            {
                _incidentTypes = value;
                Update();
            }
        }

        public Area TrainingArea
        {
            get { return _trainingArea; }
            set
            {
                _trainingArea = value;
                Update();
            }
        }

        public DateTime TrainingStart
        {
            get { return _trainingStart; }
            set
            {
                _trainingStart = value;
                Update();
            }
        }

        public DateTime TrainingEnd
        {
            get { return _trainingEnd; }
            set
            {
                _trainingEnd = value;
                Update();
            }
        }

        public List<Smoother> Smoothers
        {
            get { return _smoothers; }
            set
            {
                _smoothers = value;
                Update();
            }
        }

        public string ModelDirectory
        {
            get { return _modelDirectory; }
        }

        public Area PredictionArea
        {
            get { return _predictionArea; }
            set
            {
                _predictionArea = value;
                Update();
            }
        }

        public bool HasMadePredictions
        {
            get { return Prediction.GetForModel(this, true).Count > 0;}
        }

        public bool IsMakingAPrediction
        {
            get { return Prediction.GetForModel(this, false).Any(p => !p.Done); }
        }
        #endregion

        protected DiscreteChoiceModel()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            string trainingAreaId = _trainingArea == null ? "NULL" : _trainingArea.Id.ToString();
            string predictionAreaId = _predictionArea == null ? "NULL" : _predictionArea.Id.ToString();
            _id = Convert.ToInt32(DB.Connection.ExecuteScalar("INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES (@bytes," + predictionAreaId + "," + trainingAreaId + ") RETURNING " + Columns.Id, new Parameter("bytes", NpgsqlDbType.Bytea, ms.ToArray())));

            _modelDirectory = Path.Combine(Configuration.ModelsDirectory, _id.ToString());

            if (Directory.Exists(_modelDirectory))
                Directory.Delete(_modelDirectory, true);

            Directory.CreateDirectory(_modelDirectory);

            Update();
        }

        protected DiscreteChoiceModel(string name,
                                      int pointSpacing,
                                      IEnumerable<string> incidentTypes,
                                      Area trainingArea,
                                      DateTime trainingStart,
                                      DateTime trainingEnd,
                                      IEnumerable<Smoother> smoothers)
            : this()
        {
            _name = name;
            _pointSpacing = pointSpacing;
            _incidentTypes = new Set<string>(incidentTypes.ToArray());
            _trainingArea = trainingArea;
            _trainingStart = trainingStart;
            _trainingEnd = trainingEnd;
            _smoothers = new List<Smoother>(smoothers);
        }

        public Prediction Run(Area predictionArea, DateTime startTime, DateTime endTime, string predictionName, bool newRun)
        {
            DiscreteChoiceModel modelCopy = null;
            Prediction prediction = null;
            try
            {
                modelCopy = Copy();
                modelCopy.PredictionArea = predictionArea;
                prediction = new Prediction(modelCopy, newRun, predictionName, predictionArea, startTime, endTime, true);
                modelCopy.Run(prediction);
                prediction.Done = true;

                return prediction;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("An error occurred while running prediction:  " + ex.Message + Environment.NewLine +
                                      ex.StackTrace);

                try { prediction.Delete(); }
                catch (Exception ex2) { Console.Out.WriteLine("Failed to delete prediction:  " + ex2.Message); }

                try { modelCopy.Delete(); }
                catch (Exception ex2) { Console.Out.WriteLine("Failed to delete model:  " + ex2.Message); }

                throw ex;
            }
        }

        protected abstract void Run(Prediction prediction);

        public abstract string GetPointIdForLog(int id, DateTime time);

        /// <summary>
        /// Reads the point log for this prediction. The key is the point ID, which is mapped to two lists of tuples. The first
        /// list contains the label confidence scores and the second list contains the feature ID values.
        /// </summary>
        /// <param name="pointPredictionLogPath">Path to point prediction log</param>
        /// <param name="pointIds">Point IDs to read log for, or null for all points.</param>
        /// <returns></returns>
        public abstract Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<string, double>>>> ReadPointPredictionLog(string pointPredictionLogPath, Set<string> pointIds = null);

        /// <summary>
        /// Writes the point log for this prediction.
        /// </summary>
        /// <param name="pointIdLabelsFeatureValues">The key is the point ID, which is mapped to two lists of tuples. The first
        /// list contains the label confidence scores and the second list contains the feature ID values.</param>
        /// <param name="pointPredictionLogPath">Path to point prediction log</param>
        public abstract void WritePointPredictionLog(Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<string, double>>>> pointIdLabelsFeatureValues, string pointPredictionLogPath);

        public abstract string GetDetails(Prediction prediction);

        public virtual string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return (indentLevel > 0 ? Environment.NewLine : "") + indent + "Type:  " + GetType() + Environment.NewLine +
                   indent + "ID:  " + _id + Environment.NewLine +
                   indent + "Name:  " + _name + Environment.NewLine +
                   indent + "Point spacing:  " + _pointSpacing + Environment.NewLine +
                   indent + "Incident types:  " + _incidentTypes.Concatenate(",") + Environment.NewLine +
                   indent + "Training area:  " + TrainingArea.GetDetails(indentLevel + 1) + Environment.NewLine +
                   indent + "Training start:  " + _trainingStart.ToShortDateString() + " " + _trainingStart.ToShortTimeString() + Environment.NewLine +
                   indent + "Training end:  " + _trainingEnd.ToShortDateString() + " " + _trainingEnd.ToShortTimeString() + Environment.NewLine +
                   indent + "Smoothers:  " + _smoothers.Select(s => s.GetSmoothingDetails()).Concatenate(", ") + Environment.NewLine +
                   (_modelDirectory == "" ? "" : indent + "Model directory:  " + _modelDirectory);
        }

        public void Update()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            string trainingAreaId = _trainingArea == null ? "NULL" : _trainingArea.Id.ToString();
            string predictionAreaId = _predictionArea == null ? "NULL" : _predictionArea.Id.ToString();
            DB.Connection.ExecuteNonQuery("UPDATE " + Table + " SET " +
                                          Columns.Model + "=@bytes," +
                                          Columns.PredictionAreaId + "=" + predictionAreaId + "," +
                                          Columns.TrainingAreaId + "=" + trainingAreaId + " " +
                                          "WHERE " + Columns.Id + "=" + _id, new Parameter("bytes", NpgsqlDbType.Bytea, ms.ToArray()));
        }

        public void Delete()
        {
            if (HasMadePredictions)
                throw new Exception("Cannot delete model without first deleting predictions");

            try
            {
                if (Directory.Exists(_modelDirectory))
                    Directory.Delete(_modelDirectory, true);
            }
            catch (Exception ex) { Console.Out.WriteLine("Failed to delete model directory:  " + ex.Message); }

            try { DB.Connection.ExecuteNonQuery("DELETE FROM " + Table + " WHERE " + Columns.Id + "=" + _id); }
            catch (Exception ex) { Console.Out.WriteLine("Failed to delete model from table:  " + ex.Message); }
        }

        public abstract DiscreteChoiceModel Copy();

        public abstract void UpdateFeatureIdsFrom(DiscreteChoiceModel original);

        protected void Smooth(Prediction prediction)
        {
            foreach (Smoother smoother in _smoothers)
            {
                Console.Out.WriteLine("Smoothing prediction with " + smoother.GetType().FullName);
                smoother.Apply(prediction);
            }
        }
    }
}