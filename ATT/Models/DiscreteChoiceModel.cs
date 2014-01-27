#region copyright
// Copyright 2013 Matthew S. Gerber (gerber.matthew@gmail.com)
// 
// This file is part of the Asymmetric Threat Tracker (ATT).
// 
// The ATT is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// The ATT is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with the ATT.  If not, see <http://www.gnu.org/licenses/>.
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
using PTL.ATT.Exceptions;

namespace PTL.ATT.Models
{
    public abstract class DiscreteChoiceModel : IDiscreteChoiceModel
    {
        #region static members
        public const string Table = "discrete_choice_model";
        public const string OptimalSeriesName = "Optimal";

        private static Dictionary<int, DiscreteChoiceModel> _modelCache;

        public class Columns
        {
            [Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string IncidentTypes = "incident_types";
            [Reflector.Select(true)]
            public const string ModelDirectory = "model_directory";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Name = "name";
            [Reflector.Insert, Reflector.Select(true)]
            public const string PointSpacing = "point_spacing";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Smoothers = "smoothers";
            [Reflector.Insert, Reflector.Select(true)]
            public const string TrainingAreaId = "training_area_id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string TrainingEnd = "training_end";
            [Reflector.Insert, Reflector.Select(true)]
            public const string TrainingStart = "training_start";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Type = "type";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return Reflector.GetSelectColumns(Table, typeof(Columns)); } }
        }

        static DiscreteChoiceModel()
        {
            _modelCache = new Dictionary<int, DiscreteChoiceModel>();
        }

        [ConnectionPool.CreateTable(typeof(Area))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Id + " SERIAL PRIMARY KEY," +
                   Columns.IncidentTypes + " VARCHAR[] DEFAULT '{}'," +
                   Columns.ModelDirectory + " VARCHAR," +
                   Columns.Name + " VARCHAR," +
                   Columns.PointSpacing + " INT," +
                   Columns.Smoothers + " BYTEA[]," +
                   Columns.TrainingAreaId + " INT REFERENCES " + Area.Table + " ON DELETE RESTRICT," +
                   Columns.TrainingEnd + " TIMESTAMP," +
                   Columns.TrainingStart + " TIMESTAMP," +
                   Columns.Type + " VARCHAR);";
        }

        protected static int Create(NpgsqlConnection connection,
                                    string name,
                                    int pointSpacing,
                                    Type type,
                                    Area trainingArea,
                                    DateTime trainingStart,
                                    DateTime trainingEnd,
                                    IEnumerable<string> incidentTypes,
                                    IEnumerable<Smoother> smoothers)
        {
            NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES ('{" + incidentTypes.Select(i => "\"" + i + "\"").Concatenate(",") + "}'," +
                                                                                                              "'" + name + "'," +
                                                                                                              pointSpacing + "," +
                                                                                                              "ARRAY[" + smoothers.Select((s, i) => "@smoother_" + i).Concatenate(",") + "]::bytea[]," +
                                                                                                              trainingArea.Id + "," +
                                                                                                              "@training_end," +
                                                                                                              "@training_start,'" + type + "') RETURNING " + Columns.Id, connection);



            ConnectionPool.AddParameters(cmd, new Parameter("training_start", NpgsqlDbType.Timestamp, trainingStart),
                                              new Parameter("training_end", NpgsqlDbType.Timestamp, trainingEnd));

            BinaryFormatter bf = new BinaryFormatter();
            int smootherNum = 0;
            foreach (Smoother smoother in smoothers)
            {
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, smoother);
                ConnectionPool.AddParameters(cmd, new Parameter("smoother_" + smootherNum++, NpgsqlDbType.Bytea, ms.ToArray()));
            }

            int dcmId = Convert.ToInt32(cmd.ExecuteScalar());

            string modelDirectory = Path.Combine(Configuration.ModelsDirectory, dcmId.ToString());

            if (Directory.Exists(modelDirectory))
                Directory.Delete(modelDirectory);

            Directory.CreateDirectory(modelDirectory);

            cmd.CommandText = "UPDATE " + Table + " SET " + Columns.ModelDirectory + "='" + modelDirectory + "' WHERE " + Columns.Id + "=" + dcmId;
            cmd.ExecuteNonQuery();

            return dcmId;
        }

        public static IEnumerable<DiscreteChoiceModel> GetAll(bool excludeThoseCopiedForPredictions)
        {
            List<DiscreteChoiceModel> models = new List<DiscreteChoiceModel>();
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Table);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                DiscreteChoiceModel model = Instantiate(reader);
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
                if (model.TrainingAreaId == area.Id)
                    models.Add(model);

            return models;
        }

        public static DiscreteChoiceModel Instantiate(int id)
        {
            DiscreteChoiceModel m;
            if (!_modelCache.TryGetValue(id, out m))
            {
                NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Table + " WHERE " + Columns.Id + "=" + id);
                NpgsqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                m = Instantiate(reader);
                reader.Close();
                DB.Connection.Return(cmd.Connection);
            }

            return m;
        }

        protected static DiscreteChoiceModel Instantiate(NpgsqlDataReader reader)
        {
            int id = Convert.ToInt32(reader[Table + "_" + Columns.Id]);

            DiscreteChoiceModel m;
            if (!_modelCache.TryGetValue(id, out m))
            {
                Type type = Type.GetType(Convert.ToString(reader[Table + "_" + Columns.Type]));
                ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { id.GetType() }, null);

                m = constructor.Invoke(new object[] { id }) as DiscreteChoiceModel;
                if (m == null)
                    throw new NullReferenceException("Failed to construct model");

                _modelCache.Add(m.Id, m);
            }

            return m;
        }

        #region evaluation
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
            prediction.UpdateEvaluation();
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
        private int _trainingAreaId;
        private DateTime _trainingStart;
        private DateTime _trainingEnd;
        private List<Smoother> _smoothers;
        private string _modelDirectory;

        #region properties
        public int Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public int PointSpacing
        {
            get { return _pointSpacing; }
        }

        public Set<string> IncidentTypes
        {
            get { return _incidentTypes; }
        }

        public int TrainingAreaId
        {
            get { return _trainingAreaId; }
        }

        public Area TrainingArea
        {
            get { return new Area(_trainingAreaId); }
        }

        public DateTime TrainingStart
        {
            get { return _trainingStart; }
        }

        public DateTime TrainingEnd
        {
            get { return _trainingEnd; }
        }

        public List<Smoother> Smoothers
        {
            get { return _smoothers; }
        }

        public List<Prediction> Predictions
        {
            get { return Prediction.GetForModel(this); }
        }

        public bool HasMadePredictions
        {
            get
            {
                return Convert.ToBoolean(DB.Connection.ExecuteScalar("SELECT COUNT(*) > 0 " +
                                                                     "FROM " + Prediction.Table + " " +
                                                                     "WHERE " + Prediction.Table + "." + Prediction.Columns.ModelId + "=" + _id + " AND " +
                                                                                Prediction.Table + "." + Prediction.Columns.Done + "=TRUE"));
            }
        }

        public bool IsMakingAPrediction
        {
            get
            {
                return Convert.ToBoolean(DB.Connection.ExecuteScalar("SELECT COUNT(*) > 0 " +
                                                                     "FROM " + Prediction.Table + " " +
                                                                     "WHERE " + Prediction.Table + "." + Prediction.Columns.ModelId + "=" + _id + " AND " +
                                                                                Prediction.Table + "." + Prediction.Columns.Done + "=FALSE"));
            }
        }

        public string ModelDirectory
        {
            get { return _modelDirectory; }
        }
        #endregion

        protected virtual void Construct(NpgsqlDataReader reader)
        {
            _id = Convert.ToInt32(reader[Table + "_" + Columns.Id]);
            _name = Convert.ToString(reader[Table + "_" + Columns.Name]);
            _pointSpacing = Convert.ToInt32(reader[Table + "_" + Columns.PointSpacing]);
            _incidentTypes = new Set<string>(reader[Table + "_" + Columns.IncidentTypes] as string[]);
            _trainingAreaId = Convert.ToInt32(reader[Table + "_" + Columns.TrainingAreaId]);
            _trainingStart = Convert.ToDateTime(reader[Table + "_" + Columns.TrainingStart]);
            _trainingEnd = Convert.ToDateTime(reader[Table + "_" + Columns.TrainingEnd]);
            _modelDirectory = Convert.ToString(reader[Table + "_" + Columns.ModelDirectory]);

            BinaryFormatter bf = new BinaryFormatter();

            _smoothers = new List<Smoother>();

            IEnumerable<byte[]> smootherByteArrays = reader[Table + "_" + Columns.Smoothers] as IEnumerable<byte[]>;
            if (smootherByteArrays == null)
                throw new NullReferenceException("Failed to get smoother byte arrays");

            foreach (byte[] smootherBytes in smootherByteArrays)
            {
                MemoryStream ms = new MemoryStream(smootherBytes);
                ms.Position = 0;
                _smoothers.Add(bf.Deserialize(ms) as Smoother);
            }
        }

        public int Run(Area predictionArea, DateTime startTime, DateTime endTime, string predictionName, bool newRun)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand(null);

            DiscreteChoiceModel modelCopy = null;
            Prediction prediction = null;
            try
            {
                modelCopy = DiscreteChoiceModel.Instantiate(Copy());
                prediction = new Prediction(Prediction.Create(cmd.Connection, modelCopy.Id, newRun, predictionName, predictionArea.Id, startTime, endTime, true));
                modelCopy.Run(prediction);
                prediction.Done = true;

                return prediction.Id;
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
            finally
            {
                DB.Connection.Return(cmd.Connection);
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
        public abstract Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> ReadPointPredictionLog(string pointPredictionLogPath, Set<string> pointIds = null);

        /// <summary>
        /// Writes the point log for this prediction.
        /// </summary>
        /// <param name="pointIdLabelsFeatureValues">The key is the point ID, which is mapped to two lists of tuples. The first
        /// list contains the label confidence scores and the second list contains the feature ID values.</param>
        /// <param name="pointPredictionLogPath">Path to point prediction log</param>
        public abstract void WritePointPredictionLog(Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> pointIdLabelsFeatureValues, string pointPredictionLogPath);

        public abstract string GetDetails(Prediction prediction);

        public virtual string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return (indentLevel > 0 ? Environment.NewLine : "") + indent + "Type:  " + GetType() + Environment.NewLine +
                   indent + "ID:  " + _id + Environment.NewLine +
                   indent + "Name:  " + _name + Environment.NewLine +
                   (_modelDirectory == "" ? "" : indent + "Model directory:  " + _modelDirectory + Environment.NewLine) +
                   indent + "Incident types:  " + _incidentTypes.Concatenate(",") + Environment.NewLine +
                   indent + "Training start:  " + _trainingStart.ToShortDateString() + " " + _trainingStart.ToShortTimeString() + Environment.NewLine +
                   indent + "Training end:  " + _trainingEnd.ToShortDateString() + " " + _trainingEnd.ToShortTimeString() + Environment.NewLine +
                   indent + "Training area:  " + TrainingArea.GetDetails(indentLevel + 1) + Environment.NewLine +
                   indent + "Point spacing:  " + _pointSpacing + Environment.NewLine +
                   indent + "Smoothers:  " + _smoothers.Select(s => s.GetSmoothingDetails()).Concatenate(", ");
        }

        public void Update(string name, int pointSpacing, Area trainingArea, DateTime trainingStart, DateTime trainingEnd, IEnumerable<string> incidentTypes, IEnumerable<Smoother> smoothers)
        {
            _incidentTypes = new Set<string>(incidentTypes.ToArray());
            _name = name;
            _pointSpacing = pointSpacing;
            _trainingAreaId = trainingArea.Id;
            _trainingEnd = trainingEnd;
            _trainingStart = trainingStart;
            _smoothers = smoothers.ToList();

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            List<Parameter> parameters = new List<Parameter>();
            parameters.Add(new Parameter("training_end", NpgsqlDbType.Timestamp, trainingEnd));
            parameters.Add(new Parameter("training_start", NpgsqlDbType.Timestamp, trainingStart));

            int smootherNum = 0;
            foreach (Smoother smoother in smoothers)
            {
                ms = new MemoryStream();
                bf.Serialize(ms, smoother);
                parameters.Add(new Parameter("smoother_" + smootherNum++, NpgsqlDbType.Bytea, ms.ToArray()));
            }

            DB.Connection.ExecuteScalar("UPDATE " + Table + " SET " +
                                        Columns.IncidentTypes + "='{" + incidentTypes.Concatenate(",") + "}'," +
                                        Columns.Name + "='" + name + "'," +
                                        Columns.PointSpacing + "=" + pointSpacing + "," +
                                        Columns.TrainingAreaId + "=" + trainingArea.Id + "," +
                                        Columns.TrainingEnd + "=@training_end," +
                                        Columns.TrainingStart + "=@training_start," +
                                        Columns.Smoothers + "=ARRAY[" + smoothers.Select((s, i) => "@smoother_" + i).Concatenate(",") + "]::bytea[] " +
                                        "WHERE " + Columns.Id + "=" + _id, parameters.ToArray());

            _modelCache.Remove(_id);
        }

        public abstract int Copy();

        public abstract void UpdateFeatureIdsFrom(DiscreteChoiceModel original);

        protected void Smooth(Prediction prediction)
        {
            foreach (Smoother smoother in _smoothers)
            {
                Console.Out.WriteLine("Smoothing prediction with " + smoother.GetType().FullName);

                smoother.Apply(prediction);
            }
        }

        public void DeletePredictions()
        {
            foreach (Prediction prediction in Prediction.GetForModel(this))
                prediction.Delete();
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

            _modelCache.Remove(_id);
        }
    }
}