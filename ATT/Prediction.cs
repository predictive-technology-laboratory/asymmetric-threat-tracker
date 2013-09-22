#region copyright
// Copyright 2013 
// Predictive Technology Laboratory 
// predictivetech@virginia.edu
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
using NpgsqlTypes;
using Npgsql;
using PTL.ATT.ShapeFiles;
using LAIR.ResourceAPIs.PostgreSQL;
using LAIR.Collections.Generic;
using PTL.ATT.Classifiers;
using LAIR.MachineLearning;
using PTL.ATT.Models;
using PTL.ATT.Evaluation;
using LAIR.Extensions;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using LAIR.XML;
using System.IO.Compression;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace PTL.ATT
{
    public class Prediction : IComparable<Prediction>
    {
        public const string Table = "prediction";

        public class Columns
        {
            [Reflector.Select(true)]
            public const string Analysis = "analysis";
            [Reflector.Select(true)]
            public const string AssessmentPlots = "assessment_plots";
            [Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string IncidentTypes = "incident_types";
            [Reflector.Select(true)]
            public const string ModelDirectory = "model_directory";
            [Reflector.Insert, Reflector.Select(true)]
            public const string ModelId = "model_id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string MostRecentlyEvaluatedIncidentTime = "most_recently_evaluated_incident_time";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Name = "name";
            [Reflector.Insert, Reflector.Select(true)]
            public const string PointSpacing = "point_spacing";
            [Reflector.Insert, Reflector.Select(true)]
            public const string PredictionAreaId = "prediction_area_id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string PredictionEndTime = "prediction_end_time";
            [Reflector.Insert, Reflector.Select(true)]
            public const string PredictionStartTime = "prediction_start_time";
            [Reflector.Insert, Reflector.Select(true)]
            public const string RunId = "run_id";
            [Reflector.Select(true)]
            public const string Smoothing = "smoothing";
            [Reflector.Insert, Reflector.Select(true)]
            public const string TrainingAreaId = "training_area_id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string TrainingEndTime = "training_end_time";
            [Reflector.Insert, Reflector.Select(true)]
            public const string TrainingStartTime = "training_start_time";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return Reflector.GetSelectColumns(Table, typeof(Columns)); } }
        }

        [ConnectionPool.CreateTable(typeof(DiscreteChoiceModel))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Analysis + " VARCHAR," +
                   Columns.AssessmentPlots + " BYTEA," +
                   Columns.Id + " SERIAL PRIMARY KEY," +
                   Columns.IncidentTypes + " VARCHAR[]," +
                   Columns.ModelDirectory + " VARCHAR," +
                   Columns.ModelId + " INT REFERENCES " + DiscreteChoiceModel.Table + " ON DELETE RESTRICT," +
                   Columns.MostRecentlyEvaluatedIncidentTime + " TIMESTAMP," +
                   Columns.Name + " VARCHAR," +
                   Columns.PointSpacing + " INT," +
                   Columns.PredictionAreaId + " INT REFERENCES " + Area.Table + " ON DELETE CASCADE," +
                   Columns.PredictionEndTime + " TIMESTAMP," +
                   Columns.PredictionStartTime + " TIMESTAMP," +
                   Columns.RunId + " INT," +
                   Columns.Smoothing + " VARCHAR," +
                   Columns.TrainingAreaId + " INT REFERENCES " + Area.Table + " ON DELETE CASCADE," +
                   Columns.TrainingEndTime + " TIMESTAMP," +
                   Columns.TrainingStartTime + " TIMESTAMP);" +
                   (connection.TableExists(Table) ? "" :
                   "CREATE INDEX ON " + Table + " (" + Columns.ModelId + ");" +
                   "CREATE INDEX ON " + Table + " (" + Columns.PredictionAreaId + ");" +
                   "CREATE INDEX ON " + Table + " (" + Columns.RunId + ");" +
                   "CREATE INDEX ON " + Table + " (" + Columns.TrainingAreaId + ");");
        }

        internal static int Create(NpgsqlConnection connection, int modelId, bool newRun, IEnumerable<string> incidentTypes, int trainingAreaId, DateTime trainingStartTime, DateTime trainingEndTime, string name, int pointSpacing, int predictionAreaId, DateTime predictionStartTime, DateTime predictionEndTime, bool vacuum)
        {
            int runId = Convert.ToInt32(DB.Connection.ExecuteScalar("SELECT CASE WHEN COUNT(*) > 0 THEN MAX(" + Columns.RunId + ") ELSE -1 END FROM " + Table));
            runId += newRun ? 1 : 0;

            NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES ('{" + incidentTypes.Select(i => "\"" + i + "\"").Concatenate(",") + "}'," +
                                                                                                              modelId + "," +
                                                                                                              "@most_recently_evaluated_incident_time," +
                                                                                                              "'" + name + "'," +
                                                                                                              pointSpacing + "," +
                                                                                                              predictionAreaId + "," +
                                                                                                              "@prediction_end_time," +
                                                                                                              "@prediction_start_time," +
                                                                                                              runId + "," +
                                                                                                              trainingAreaId + "," +
                                                                                                              "@training_end_time," +
                                                                                                              "@training_start_time) RETURNING " + Columns.Id, connection);

            ConnectionPool.AddParameters(cmd, new Parameter("most_recently_evaluated_incident_time", NpgsqlDbType.Timestamp, DateTime.MinValue),
                                              new Parameter("training_start_time", NpgsqlDbType.Timestamp, trainingStartTime),
                                              new Parameter("training_end_time", NpgsqlDbType.Timestamp, trainingEndTime),
                                              new Parameter("prediction_start_time", NpgsqlDbType.Timestamp, predictionStartTime),
                                              new Parameter("prediction_end_time", NpgsqlDbType.Timestamp, predictionEndTime));

            int id = Convert.ToInt32(cmd.ExecuteScalar());

            string modelDirectory = Path.Combine(Configuration.ModelsDirectory, id.ToString());
            if (Directory.Exists(modelDirectory))
                throw new Exception("Model directory \"" + modelDirectory + "\" for prediction already exists. This should not be possible unless the ATT database was manually truncated without also deleting all model directories within \"" + Configuration.ModelsDirectory + "\"...do this.");
            else
                Directory.CreateDirectory(modelDirectory);

            cmd.CommandText = "UPDATE " + Table + " " +
                              "SET " + Columns.ModelDirectory + "='" + modelDirectory + "' " +
                              "WHERE " + Columns.Id + "=" + id;

            cmd.ExecuteNonQuery();

            if (vacuum)
                VacuumTable();

            return id;
        }

        public static void VacuumTable()
        {
            DB.Connection.ExecuteNonQuery("VACUUM ANALYZE " + Table);
        }

        public static IEnumerable<Prediction> GetAvailable(int modelId = -1)
        {
            List<Prediction> predictions = new List<Prediction>();
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Table + (modelId == -1 ? "" : " WHERE " + Columns.ModelId + "=" + modelId));
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                predictions.Add(new Prediction(reader));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return predictions;
        }

        private int _id;
        private int _runId;
        private string _modelDirectory;
        private int _modelId;
        private Set<string> _incidentTypes;
        private int _trainingAreaId;
        private Area _trainingArea;
        private DateTime _trainingStartTime;
        private DateTime _trainingEndTime;
        private string _name;
        private int _pointSpacing;
        private int _predictionAreaId;
        private Area _predictionArea;
        private DateTime _predictionStartTime;
        private DateTime _predictionEndTime;
        private DateTime _mostRecentlyEvaluatedIncidentTime;
        private List<Plot> _assessmentPlots;
        private string _modelDetails;
        private List<Point> _points;
        private List<PointPrediction> _pointPredictions;
        private List<Feature> _selectedFeatures;
        private string _smoothing;

        public string Smoothing
        {
            get { return _smoothing; }
            set
            {
                if (_smoothing == value)
                    return;

                _smoothing = value;
                
                DB.Connection.ExecuteNonQuery("UPDATE " + Table + " SET " + Columns.Smoothing + "='" + _smoothing + "' WHERE " + Columns.Id + "=" + _id);
            }
        }

        public int Id
        {
            get { return _id; }
        }

        public int RunId
        {
            get { return _runId; }
            set
            {
                if (_runId == value)
                    return;

                _runId = value;

                DB.Connection.ExecuteNonQuery("UPDATE " + Table + " SET " + Columns.RunId + "=" + _runId + " WHERE " + Columns.Id + "=" + _id);
            }
        }

        public string ModelDirectory
        {
            get { return _modelDirectory; }
        }

        public string PointPredictionLogPath
        {
            get { return Path.Combine(_modelDirectory, "point_predictions.gz"); }
        }

        public int ModelId
        {
            get { return _modelId; }
        }

        public DiscreteChoiceModel Model
        {
            get { return DiscreteChoiceModel.Instantiate(_modelId); }
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
            get
            {
                if (_trainingArea == null)
                    _trainingArea = new Area(_trainingAreaId);

                return _trainingArea;
            }
        }

        public DateTime TrainingStartTime
        {
            get { return _trainingStartTime; }
        }

        public DateTime TrainingEndTime
        {
            get { return _trainingEndTime; }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                value = value.Trim();

                if (value == "")
                    throw new Exception("Invalid new prediction name. Cannot be blank.");

                if (_name == value)
                    return;

                bool updateEvaluation = false;
                foreach (Plot assessmentPlot in _assessmentPlots)
                    if (assessmentPlot.Title.Contains(_name))
                    {
                        assessmentPlot.Title = assessmentPlot.Title.Replace(_name, value);
                        updateEvaluation = true;
                    }

                if (updateEvaluation)
                    UpdateEvaluation();

                _name = value;

                DB.Connection.ExecuteNonQuery("UPDATE " + Table + " SET " + Columns.Name + "='" + _name + "' WHERE " + Columns.Id + "=" + _id);
            }
        }

        public int PointSpacing
        {
            get { return _pointSpacing; }
        }

        public int PredictionAreaId
        {
            get { return _predictionAreaId; }
        }

        public Area PredictionArea
        {
            get
            {
                if (_predictionArea == null)
                    _predictionArea = new Area(_predictionAreaId);

                return _predictionArea;
            }
        }

        public DateTime PredictionStartTime
        {
            get { return _predictionStartTime; }
        }

        public DateTime PredictionEndTime
        {
            get { return _predictionEndTime; }
        }

        public DateTime MostRecentlyEvaluatedIncidentTime
        {
            get { return _mostRecentlyEvaluatedIncidentTime; }
            set { _mostRecentlyEvaluatedIncidentTime = value; }
        }

        public List<Plot> AssessmentPlots
        {
            get { return _assessmentPlots; }
            set { _assessmentPlots = value; }
        }

        public string ModelDetails
        {
            get { return _modelDetails; }
            set
            {
                if (_modelDetails == value)
                    return;

                _modelDetails = value;

                DB.Connection.ExecuteNonQuery("UPDATE " + Table + " SET " + Columns.Analysis + "='" + _modelDetails + "' WHERE " + Columns.Id + "=" + _id);
            }
        }

        public IEnumerable<Point> Points
        {
            get
            {
                if (_points == null)
                {
                    _points = new List<Point>();
                    string pointTable = Point.GetTable(_id, false);
                    NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Point.Columns.Select(pointTable) + " FROM " + pointTable);
                    NpgsqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                        _points.Add(new Point(reader, pointTable));

                    reader.Close();
                    DB.Connection.Return(cmd.Connection);
                }

                return _points;
            }
        }

        public IEnumerable<PointPrediction> PointPredictions
        {
            get
            {
                if (_pointPredictions == null)
                {
                    _pointPredictions = new List<PointPrediction>();
                    string pointPredictionTable = PointPrediction.GetTable(_id, false);
                    NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + PointPrediction.Columns.Select(pointPredictionTable) + " FROM " + pointPredictionTable);
                    NpgsqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                        _pointPredictions.Add(new PointPrediction(reader, pointPredictionTable));

                    reader.Close();
                    DB.Connection.Return(cmd.Connection);
                }

                return _pointPredictions;
            }
        }

        public IEnumerable<Feature> SelectedFeatures
        {
            get
            {
                if (_selectedFeatures == null)
                {
                    _selectedFeatures = new List<Feature>();

                    NpgsqlCommand cmd = DB.Connection.NewCommand(null);
                    try
                    {
                        cmd.CommandText = "SELECT " + Feature.Columns.Select + " FROM " + Feature.Table + " WHERE " + Feature.Columns.PredictionId + "=" + _id + " ORDER BY " + Feature.Columns.Id;
                        NpgsqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            _selectedFeatures.Add(new Feature(reader));

                        reader.Close();
                    }
                    finally { DB.Connection.Return(cmd.Connection); }
                }

                return _selectedFeatures;
            }
            set
            {
                _selectedFeatures = null;

                NpgsqlCommand cmd = DB.Connection.NewCommand(null);
                try
                {
                    cmd.CommandText = "DELETE FROM " + Feature.Table + " WHERE " + Feature.Columns.PredictionId + "=" + _id;
                    cmd.ExecuteNonQuery();

                    foreach (Feature feature in value.OrderBy(f => f.Id))
                        Feature.Create(cmd.Connection, feature.Description, feature.EnumType, feature.EnumValue, _id, feature.ResourceId, false);

                    Feature.VacuumTable();
                }
                finally { DB.Connection.Return(cmd.Connection); }
            }
        }

        public Prediction(int id)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Table + " WHERE " + Columns.Id + "=" + id);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            Construct(reader);
            reader.Close();
            DB.Connection.Return(cmd.Connection);
        }

        private Prediction(NpgsqlDataReader reader)
        {
            Construct(reader);
        }

        private void Construct(NpgsqlDataReader reader)
        {
            _id = Convert.ToInt32(reader[Table + "_" + Columns.Id]);
            _runId = Convert.ToInt32(reader[Table + "_" + Columns.RunId]);
            _modelDirectory = Convert.ToString(reader[Table + "_" + Columns.ModelDirectory]);
            _modelId = Convert.ToInt32(reader[Table + "_" + Columns.ModelId]);
            _incidentTypes = new Set<string>(reader[Table + "_" + Columns.IncidentTypes] as string[]);
            _trainingAreaId = Convert.ToInt32(reader[Table + "_" + Columns.TrainingAreaId]);
            _trainingStartTime = Convert.ToDateTime(reader[Table + "_" + Columns.TrainingStartTime]);
            _trainingEndTime = Convert.ToDateTime(reader[Table + "_" + Columns.TrainingEndTime]);
            _name = Convert.ToString(reader[Table + "_" + Columns.Name]);
            _pointSpacing = Convert.ToInt32(reader[Table + "_" + Columns.PointSpacing]);
            _predictionAreaId = Convert.ToInt32(reader[Table + "_" + Columns.PredictionAreaId]);
            _predictionStartTime = Convert.ToDateTime(reader[Table + "_" + Columns.PredictionStartTime]);
            _predictionEndTime = Convert.ToDateTime(reader[Table + "_" + Columns.PredictionEndTime]);
            _mostRecentlyEvaluatedIncidentTime = Convert.ToDateTime(reader[Table + "_" + Columns.MostRecentlyEvaluatedIncidentTime]);
            _modelDetails = Convert.ToString(reader[Table + "_" + Columns.Analysis]);
            _smoothing = Convert.ToString(reader[Table + "_" + Columns.Smoothing]);

            _assessmentPlots = new List<Plot>();
            if (!(reader[Table + "_" + Columns.AssessmentPlots] is DBNull))
            {
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream plotsBytes = new MemoryStream(reader[Table + "_" + Columns.AssessmentPlots] as byte[]);
                plotsBytes.Position = 0;
                _assessmentPlots = bf.Deserialize(plotsBytes) as List<Plot>;
            }
        }

        public override string ToString()
        {
            return _name;
        }
        internal void DeletePoints()
        {
            DB.Connection.ExecuteNonQuery("DELETE FROM " + Point.GetTable(_id, false));
            ReleaseLazyLoadedData();
        }

        public void Delete()
        {
            ReleaseLazyLoadedData();

            DB.Connection.ExecuteNonQuery("DELETE FROM " + Table + " WHERE " + Columns.Id + "=" + _id);

            try { Point.DeleteTable(_id); }
            catch (Exception) { }

            try { PointPrediction.DeleteTable(_id); }
            catch (Exception) { }

            if (Directory.Exists(_modelDirectory))
                Directory.Delete(_modelDirectory, true);

            Model.RaisePredictionDeleted(this);
        }

        public void UpdateEvaluation()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, _assessmentPlots);

            DB.Connection.ExecuteNonQuery("UPDATE " + Table + " " +
                                          "SET " + Columns.AssessmentPlots + "=@" + Columns.AssessmentPlots + "," +
                                                   Columns.MostRecentlyEvaluatedIncidentTime + "=@" + Columns.MostRecentlyEvaluatedIncidentTime + " " +
                                          "WHERE " + Columns.Id + "=" + _id,
                                          new Parameter(Columns.AssessmentPlots, NpgsqlDbType.Bytea, ms.ToArray()),
                                          new Parameter(Columns.MostRecentlyEvaluatedIncidentTime, NpgsqlDbType.Timestamp, _mostRecentlyEvaluatedIncidentTime));
        }

        public int Copy(string newName, bool newRun, bool vacuum)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand(null);

            int copyId = Create(cmd.Connection, _modelId, newRun, _incidentTypes, _trainingAreaId, _trainingStartTime, _trainingEndTime, newName, _pointSpacing, _predictionAreaId, _predictionStartTime, _predictionEndTime, false);

            try
            {
                string selectColumns = Point.Columns.GetInsertWithout(new Set<string>(new string[] { Point.Columns.Id }));

                string copiedPointTable = Point.GetTable(copyId, true);
                cmd.CommandText = "INSERT INTO " + copiedPointTable + "(" + selectColumns + ") " +
                                  "SELECT " + selectColumns + " " +
                                  "FROM " + Point.GetTable(_id, false) + " " +
                                  "ORDER BY " + Point.Columns.Id + " ASC " +
                                  "RETURNING " + Point.Columns.Id;

                List<int> oldPointIds = PointPredictions.Select(p => p.PointId).OrderBy(pointId => pointId).ToList();
                Dictionary<int, int> oldPointIdNewPointId = new Dictionary<int, int>(oldPointIds.Count);
                NpgsqlDataReader reader = cmd.ExecuteReader();
                int pointNum = 0;
                while (reader.Read())
                    oldPointIdNewPointId.Add(oldPointIds[pointNum++], Convert.ToInt32(reader[Point.Columns.Id]));
                reader.Close();

                if (pointNum != oldPointIds.Count)
                    throw new Exception("Mismatch between number of point predictions and number of new point IDs");

                List<Tuple<string, Parameter>> copiedPointPredictionValues = new List<Tuple<string, Parameter>>(oldPointIdNewPointId.Count);
                foreach (PointPrediction pointPrediction in PointPredictions)
                {
                    string labels = "'{" + pointPrediction.IncidentScore.Keys.Where(l => l != PointPrediction.NullLabel).Select(l => "\"" + l + "\"").Concatenate(",") + "}'";
                    string threats = "'{" + pointPrediction.IncidentScore.Keys.Where(l => l != PointPrediction.NullLabel).Select(l => pointPrediction.IncidentScore[l].ToString()).Concatenate(",") + "}'";
                    double totalThreat = pointPrediction.IncidentScore.Keys.Where(l => l != PointPrediction.NullLabel).Sum(l => pointPrediction.IncidentScore[l]);
                    string timeParameterName = "@time_" + pointPrediction.Id;
                    Parameter timeParameter = new Parameter(timeParameterName, NpgsqlDbType.Timestamp, pointPrediction.Time);
                    copiedPointPredictionValues.Add(new Tuple<string, Parameter>("(" + labels + "," + oldPointIdNewPointId[pointPrediction.PointId] + "," + threats + "," + timeParameterName + "," + totalThreat + ")", timeParameter));
                }

                PointPrediction.Insert(copiedPointPredictionValues, copyId, true);

                Prediction copy = new Prediction(copyId);
                copy.Smoothing = _smoothing;
                copy.SelectedFeatures = SelectedFeatures;

                Dictionary<int, int> oldNewFeatureId = new Dictionary<int, int>();
                foreach (Tuple<int, int> oldNew in SelectedFeatures.Zip(copy.SelectedFeatures, new Func<Feature, Feature, Tuple<int, int>>((f1, f2) => new Tuple<int, int>(f1.Id, f2.Id))))
                    oldNewFeatureId.Add(oldNew.Item1, oldNew.Item2);

                foreach (string path in Directory.GetFiles(_modelDirectory))
                    if (path == PointPredictionLogPath)
                    {
                        Dictionary<int, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> originalLog = ReadPointPredictionLog();
                        Dictionary<int, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> copiedLog = new Dictionary<int, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>>(originalLog.Count);
                        foreach (int originalPointId in originalLog.Keys)
                        {
                            List<Tuple<int, double>> copiedFeatureValues = new List<Tuple<int, double>>(originalLog[originalPointId].Item2.Count);
                            foreach (Tuple<int, double> originalFeatureValue in originalLog[originalPointId].Item2)
                                copiedFeatureValues.Add(new Tuple<int, double>(oldNewFeatureId[originalFeatureValue.Item1], originalFeatureValue.Item2));

                            copiedLog.Add(oldPointIdNewPointId[originalPointId], new Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>(originalLog[originalPointId].Item1, copiedFeatureValues));
                        }

                        copy.WritePointPredictionLog(copiedLog);
                    }
                    else
                        File.Copy(path, Path.Combine(copy.ModelDirectory, Path.GetFileName(path)));

                Model.ChangeFeatureIds(copy, oldNewFeatureId);
            }
            catch (Exception ex)
            {
                try { new Prediction(copyId).Delete(); }
                catch (Exception) { }

                throw ex;
            }
            finally
            {
                DB.Connection.Return(cmd.Connection);

                if (vacuum)
                {
                    try { VacuumTable(); }
                    catch (Exception) { }
                    try { Point.VacuumTable(copyId); }
                    catch (Exception) { }
                    try { PointPrediction.VacuumTable(copyId); }
                    catch (Exception) { }
                }
            }

            return copyId;
        }

        public string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return (indentLevel > 0 ? Environment.NewLine : "") + "ID:  " + _id + Environment.NewLine +
                   indent + "Name:  " + _name + Environment.NewLine +
                   indent + "Run:  " + _runId + Environment.NewLine +
                   indent + "Model:  " + Model.GetDetails(indentLevel + 1) + Environment.NewLine +
                   indent + "Model directory:  " + _modelDirectory + Environment.NewLine +
                   indent + "Incident types:  " + _incidentTypes.Concatenate(",") + Environment.NewLine +
                   indent + "Point spacing:  " + _pointSpacing + Environment.NewLine +
                   indent + "Training start:  " + _trainingStartTime.ToShortDateString() + " " + _trainingStartTime.ToShortTimeString() + Environment.NewLine +
                   indent + "Training end:  " + _trainingEndTime.ToShortDateString() + " " + _trainingEndTime.ToShortTimeString() + Environment.NewLine +
                   indent + "Prediction start:  " + _predictionStartTime.ToShortDateString() + " " + _predictionStartTime.ToShortTimeString() + Environment.NewLine +
                   indent + "Prediction end:  " + _predictionEndTime.ToShortDateString() + " " + _predictionEndTime.ToShortTimeString() + Environment.NewLine +
                   indent + "Smoothing:  " + _smoothing + Environment.NewLine +
                   indent + "Time of most recently evaluated incident:  " + (_mostRecentlyEvaluatedIncidentTime == DateTime.MinValue ? "Never" : _mostRecentlyEvaluatedIncidentTime.ToShortDateString() + " " + _mostRecentlyEvaluatedIncidentTime.ToShortTimeString());
        }

        public int CompareTo(Prediction other)
        {
            return _id.CompareTo(other.Id);
        }

        /// <summary>
        /// Reads the point log for this prediction. The key is the point ID, which is mapped to two lists of tuples. The first
        /// list contains the label confidence scores and the second list contains the feature ID values.
        /// </summary>
        /// <param name="pointIds">Point IDs to read log for, or null for all points.</param>
        /// <returns></returns>
        public Dictionary<int, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> ReadPointPredictionLog(Set<int> pointIds = null)
        {
            Dictionary<int, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> log = new Dictionary<int, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>>();

            StreamReader pointPredictionLog = new StreamReader(new GZipStream(new FileStream(PointPredictionLogPath, FileMode.Open, FileAccess.Read), CompressionMode.Decompress));
            string line;
            while ((line = pointPredictionLog.ReadLine()) != null)
            {
                int pointId = int.Parse(line.Substring(0, line.IndexOf(' ')));

                if (pointIds == null || pointIds.Contains(pointId))
                {
                    XmlParser pointP = new XmlParser(line.Substring(line.IndexOf(' ') + 1));

                    List<Tuple<string, double>> labelConfidences = new List<Tuple<string, double>>();
                    XmlParser labelsP = new XmlParser(pointP.OuterXML("ls"));
                    string labelXML;
                    while ((labelXML = labelsP.OuterXML("l")) != null)
                    {
                        XmlParser labelP = new XmlParser(labelXML);
                        double confidence = double.Parse(labelP.AttributeValue("l", "c"));
                        string label = labelP.ElementText("l");
                        labelConfidences.Add(new Tuple<string, double>(label, confidence));
                    }

                    List<Tuple<int, double>> featureValues = new List<Tuple<int, double>>();
                    XmlParser featureValuesP = new XmlParser(pointP.OuterXML("fvs"));
                    string featureValueXML;
                    while ((featureValueXML = featureValuesP.OuterXML("fv")) != null)
                    {
                        XmlParser featureValueP = new XmlParser(featureValueXML);
                        featureValues.Add(new Tuple<int, double>(int.Parse(featureValueP.AttributeValue("fv", "id")), double.Parse(featureValueP.ElementText("fv"))));
                    }

                    log.Add(pointId, new Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>(labelConfidences, featureValues));

                    if (pointIds != null)
                    {
                        pointIds.Remove(pointId);
                        if (pointIds.Count == 0)
                            break;
                    }
                }
            }

            pointPredictionLog.Close();

            return log;
        }

        /// <summary>
        /// Writes the point log for this prediction.
        /// </summary>
        /// <param name="pointIdLabelsFeatureValues">The key is the point ID, which is mapped to two lists of tuples. The first
        /// list contains the label confidence scores and the second list contains the feature ID values.</param>
        public void WritePointPredictionLog(Dictionary<int, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> pointIdLabelsFeatureValues)
        {
            StreamWriter pointPredictionLog = new StreamWriter(new GZipStream(new FileStream(PointPredictionLogPath, FileMode.Create, FileAccess.Write), CompressionMode.Compress));
            foreach (int pointId in pointIdLabelsFeatureValues.Keys.OrderBy(k => k))
            {
                pointPredictionLog.Write(pointId + " <p><ls>");
                foreach (Tuple<string, double> labelConfidence in pointIdLabelsFeatureValues[pointId].Item1)
                    pointPredictionLog.Write("<l c=\"" + labelConfidence.Item2 + "\"><![CDATA[" + labelConfidence.Item1 + "]]></l>");

                pointPredictionLog.Write("</ls><fvs>");
                foreach (Tuple<int, double> featureIdValue in pointIdLabelsFeatureValues[pointId].Item2)
                    pointPredictionLog.Write("<fv id=\"" + featureIdValue.Item1 + "\">" + featureIdValue.Item2 + "</fv>");

                pointPredictionLog.WriteLine("</fvs></p>");
            }

            pointPredictionLog.Close();
        }

        /// <summary>
        /// Releases all data that was lazy-loaded into memory (e.g., points). Often this data can be large and needs to be cleaned up.
        /// </summary>
        public void ReleaseLazyLoadedData()
        {
            _trainingArea = _predictionArea = null;
            _points = null;
            _pointPredictions = null;
            _selectedFeatures = null;
        }
    }
}
