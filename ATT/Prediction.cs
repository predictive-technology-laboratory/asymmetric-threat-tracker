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
using NpgsqlTypes;
using Npgsql;
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
using PTL.ATT.Exceptions;

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
            [Reflector.Insert, Reflector.Select(true)]
            public const string Done = "done";
            [Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string ModelId = "model_id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string MostRecentlyEvaluatedIncidentTime = "most_recently_evaluated_incident_time";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Name = "name";
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

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return Reflector.GetSelectColumns(Table, typeof(Columns)); } }
        }

        [ConnectionPool.CreateTable(typeof(DiscreteChoiceModel))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Analysis + " VARCHAR," +
                   Columns.AssessmentPlots + " BYTEA," +
                   Columns.Done + " BOOLEAN," +
                   Columns.Id + " SERIAL PRIMARY KEY," +
                   Columns.ModelId + " INT REFERENCES " + DiscreteChoiceModel.Table + " ON DELETE RESTRICT," + // must delete predictions with Prediction.Delete (to clean up some tables)
                   Columns.MostRecentlyEvaluatedIncidentTime + " TIMESTAMP," +
                   Columns.Name + " VARCHAR," +
                   Columns.PredictionAreaId + " INT REFERENCES " + Area.Table + " ON DELETE RESTRICT," + // must delete predictions with Prediction.Delete (to clean up some tables)
                   Columns.PredictionEndTime + " TIMESTAMP," +
                   Columns.PredictionStartTime + " TIMESTAMP," +
                   Columns.RunId + " INT," +
                   Columns.Smoothing + " VARCHAR);" +
                   (connection.TableExists(Table) ? "" :
                   "CREATE INDEX ON " + Table + " (" + Columns.Done + ");" +
                   "CREATE INDEX ON " + Table + " (" + Columns.ModelId + ");" +
                   "CREATE INDEX ON " + Table + " (" + Columns.PredictionAreaId + ");" +
                   "CREATE INDEX ON " + Table + " (" + Columns.RunId + ");");
        }

        internal static int Create(NpgsqlConnection connection, int modelId, bool newRun, string name, int predictionAreaId, DateTime predictionStartTime, DateTime predictionEndTime, bool vacuum)
        {
            int runId = Convert.ToInt32(DB.Connection.ExecuteScalar("SELECT CASE WHEN COUNT(*) > 0 THEN MAX(" + Columns.RunId + ") ELSE -1 END FROM " + Table));
            runId += newRun ? 1 : 0;

            NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES (FALSE," + 
                                                                                                              modelId + "," +
                                                                                                              "@most_recently_evaluated_incident_time," +
                                                                                                              "'" + name + "'," +
                                                                                                              predictionAreaId + "," +
                                                                                                              "@prediction_end_time," +
                                                                                                              "@prediction_start_time," +
                                                                                                              runId + ") RETURNING " + Columns.Id, connection);

            ConnectionPool.AddParameters(cmd, new Parameter("most_recently_evaluated_incident_time", NpgsqlDbType.Timestamp, DateTime.MinValue),
                                              new Parameter("prediction_start_time", NpgsqlDbType.Timestamp, predictionStartTime),
                                              new Parameter("prediction_end_time", NpgsqlDbType.Timestamp, predictionEndTime));

            int id = Convert.ToInt32(cmd.ExecuteScalar());

            if (vacuum)
                VacuumTable();

            return id;
        }

        public static void VacuumTable()
        {
            DB.Connection.ExecuteNonQuery("VACUUM ANALYZE " + Table);
        }

        public static List<Prediction> GetAll()
        {
            List<Prediction> predictions = new List<Prediction>();
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Table + " WHERE " + Columns.Done + "=TRUE");
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                predictions.Add(new Prediction(reader));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return predictions;
        }

        public static List<Prediction> GetForModel(DiscreteChoiceModel model)
        {
            List<Prediction> predictions = new List<Prediction>();
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Table + " WHERE " + Columns.Done + "=TRUE AND " + Columns.ModelId + "=" + model.Id);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                predictions.Add(new Prediction(reader));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return predictions;
        }

        public static List<Prediction> GetForArea(Area area)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " " + 
                                                         "FROM " + Table + " JOIN " + DiscreteChoiceModel.Table + " ON " + DiscreteChoiceModel.Table + "." + DiscreteChoiceModel.Columns.Id + "=" + Prediction.Table + "." + Columns.ModelId + " " +  
                                                         "WHERE " + Columns.Done + "=TRUE AND (" + DiscreteChoiceModel.Columns.TrainingAreaId + "=" + area.Id + " OR " + Columns.PredictionAreaId + "=" + area.Id + ")");

            List<Prediction> predictions = new List<Prediction>();
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                predictions.Add(new Prediction(reader));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return predictions;
        }

        private bool _done;
        private int _id;
        private int _runId;
        private int _modelId;
        private string _name;
        private int _predictionAreaId;
        private Area _predictionArea;
        private DateTime _predictionStartTime;
        private DateTime _predictionEndTime;
        private DateTime _mostRecentlyEvaluatedIncidentTime;
        private List<Plot> _assessmentPlots;
        private string _modelDetails;
        private List<Point> _points;
        private List<PointPrediction> _pointPredictions;
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

        public bool Done
        {
            get { return _done; }
            set
            {
                _done = value;

                DB.Connection.ExecuteNonQuery("UPDATE " + Table + " SET " + Columns.Done + "=" + _done + " WHERE " + Columns.Id + "=" + _id);
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

        public string PointPredictionLogPath
        {
            get { return Path.Combine(Model.ModelDirectory, "point_predictions.gz"); }
        }

        public int ModelId
        {
            get { return _modelId; }
            set
            {
                _modelId = value;
                DB.Connection.ExecuteNonQuery("UPDATE " + Table + " SET " + Columns.ModelId + "=" + _modelId + " WHERE " + Columns.Id + "=" + _id);
            }
        }

        public DiscreteChoiceModel Model
        {
            get { return DiscreteChoiceModel.Instantiate(_modelId); }
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

        public int PredictionAreaId
        {
            get { return _predictionAreaId; }
        }

        public Area PredictionArea
        {
            get
            {
                lock (this)
                {
                    if (_predictionArea == null)
                        _predictionArea = new Area(_predictionAreaId);
                }

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

        public List<Point> Points
        {
            get
            {
                lock (this)
                {
                    if (_points == null)
                    {
                        _points = new List<Point>();
                        string pointTable = Point.GetTableName(_id);
                        NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Point.Columns.Select(pointTable) + " FROM " + pointTable);
                        NpgsqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            _points.Add(new Point(reader, pointTable));

                        reader.Close();
                        DB.Connection.Return(cmd.Connection);
                    }
                }

                return _points;
            }
        }

        public List<PointPrediction> PointPredictions
        {
            get
            {
                lock (this)
                {
                    if (_pointPredictions == null)
                    {
                        _pointPredictions = new List<PointPrediction>();
                        string pointPredictionTable = PointPrediction.GetTableName(_id);
                        NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + PointPrediction.Columns.Select(pointPredictionTable) + " FROM " + pointPredictionTable);
                        NpgsqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            _pointPredictions.Add(new PointPrediction(reader, pointPredictionTable));

                        reader.Close();
                        DB.Connection.Return(cmd.Connection);
                    }
                }

                return _pointPredictions;
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
            _done = Convert.ToBoolean(reader[Table + "_" + Columns.Done]);
            _id = Convert.ToInt32(reader[Table + "_" + Columns.Id]);
            _runId = Convert.ToInt32(reader[Table + "_" + Columns.RunId]);
            _modelId = Convert.ToInt32(reader[Table + "_" + Columns.ModelId]);
            _name = Convert.ToString(reader[Table + "_" + Columns.Name]);
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

        public void Delete()
        {
            ReleaseLazyLoadedData();

            try { DB.Connection.ExecuteNonQuery("DELETE FROM " + Table + " WHERE " + Columns.Id + "=" + _id); }
            catch (Exception ex) { Console.Out.WriteLine("Failed to delete prediction from table:  " + ex.Message); }

            try { Point.DeleteTable(_id); }
            catch (Exception ex) { Console.Out.WriteLine("Failed to delete point table:  " + ex.Message); }

            try { PointPrediction.DeleteTable(_id); }
            catch (Exception ex) { Console.Out.WriteLine("Failed to delete point prediction table:  " + ex.Message); }

            try { Model.Delete(); }
            catch (Exception ex) { Console.Out.WriteLine("Failed to delete model for prediction:  " + ex.Message); }
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

            Prediction copiedPrediction = null;

            try
            {
                copiedPrediction = new Prediction(Create(cmd.Connection, Model.Copy(), newRun, newName, _predictionAreaId, _predictionStartTime, _predictionEndTime, false));

                string copiedPointTable = Point.CreateTable(copiedPrediction.Id, PredictionArea.SRID);
                cmd.CommandText = "INSERT INTO " + copiedPointTable + " (" + Point.Columns.Insert + ") " +
                                  "SELECT " + Point.Columns.Insert + " " +
                                  "FROM " + Point.GetTableName(_id) + " " +
                                  "ORDER BY " + Point.Columns.Id + " ASC";
                cmd.ExecuteNonQuery();

                string copiedPointPredictionTable = PointPrediction.CreateTable(copiedPrediction.Id);
                cmd.CommandText = "INSERT INTO " + copiedPointPredictionTable + " (" + PointPrediction.Columns.Insert + ") " +
                                  "SELECT " + PointPrediction.Columns.Insert + " " +
                                  "FROM " + PointPrediction.GetTableName(_id) + " " +
                                  "ORDER BY " + PointPrediction.Columns.Id + " ASC";
                cmd.ExecuteNonQuery();

                foreach (string path in Directory.GetFiles(Model.ModelDirectory))
                    File.Copy(path, Path.Combine(copiedPrediction.Model.ModelDirectory, Path.GetFileName(path)));

                copiedPrediction.Smoothing = _smoothing;
                copiedPrediction.Done = true;

                copiedPrediction.Model.UpdateFeatureIdsFrom(Model);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Failed to copy prediction:  " + ex.Message);

                if(copiedPrediction != null)
                    try { copiedPrediction.Delete(); }
                    catch (Exception ex2) { Console.Out.WriteLine("Failed to delete copied prediction:  " + ex2.Message); }

                throw ex;
            }
            finally
            {
                DB.Connection.Return(cmd.Connection);

                if (vacuum)
                {
                    try { VacuumTable(); }
                    catch (Exception ex) { Console.Out.WriteLine("Failed to vacuum prediction table:  " + ex.Message); }
                    try { Point.VacuumTable(copiedPrediction.Id); }
                    catch (Exception ex) { Console.Out.WriteLine("Failed to vacuum point table for prediction \"" + copiedPrediction.Id + "\":  " + ex.Message); }
                    try { PointPrediction.VacuumTable(copiedPrediction.Id); }
                    catch (Exception ex) { Console.Out.WriteLine("Failed to vacuum point prediction table for prediction \"" + copiedPrediction.Id + "\":  " + ex.Message); }
                }
            }

            return copiedPrediction.Id;
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
        /// Releases all data that was lazy-loaded into memory (e.g., points). Often this data can be large and needs to be cleaned up.
        /// </summary>
        public void ReleaseLazyLoadedData()
        {
            _predictionArea = null;
            _points = null;
            _pointPredictions = null;
        }
    }
}
