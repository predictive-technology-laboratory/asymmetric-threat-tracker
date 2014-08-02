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

namespace PTL.ATT
{
    [Serializable]
    public class Prediction : IComparable<Prediction>
    {
        public const string Table = "prediction";

        public class Columns
        {
            [Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string ModelId = "model_id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Prediction = "prediction";
            [Reflector.Insert, Reflector.Select(true)]
            public const string PredictionAreaId = "prediction_area_id";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return Reflector.GetSelectColumns(Table, typeof(Columns)); } }
        }

        [ConnectionPool.CreateTable(typeof(DiscreteChoiceModel))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Id + " SERIAL PRIMARY KEY," +
                   Columns.ModelId + " INT REFERENCES " + DiscreteChoiceModel.Table + " ON DELETE RESTRICT," + // must delete predictions with Prediction.Delete (to clean up some tables)
                   Columns.Prediction + " BYTEA," +
                   Columns.PredictionAreaId + " INT REFERENCES " + Area.Table + " ON DELETE RESTRICT);" + // must delete predictions with Prediction.Delete (to clean up some tables)
                   (connection.TableExists(Table) ? "" :
                   "CREATE INDEX ON " + Table + " (" + Columns.ModelId + ");" +
                   "CREATE INDEX ON " + Table + " (" + Columns.PredictionAreaId + ");");
        }

        public static void VacuumTable()
        {
            DB.Connection.ExecuteNonQuery("VACUUM ANALYZE " + Table);
        }

        public static List<Prediction> GetAll(bool onlyFinishedPredictions)
        {
            List<Prediction> predictions = new List<Prediction>();
            BinaryFormatter bf = new BinaryFormatter();
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Table);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Prediction prediction = bf.Deserialize(new MemoryStream(reader[Table + "_" + Columns.Prediction] as byte[])) as Prediction;
                if (!onlyFinishedPredictions || prediction.Done)
                    predictions.Add(prediction);
            }

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return predictions;
        }

        public static List<Prediction> GetForModel(DiscreteChoiceModel model, bool onlyFinishedPredictions)
        {
            return GetAll(onlyFinishedPredictions).Where(p => p.Model.Id == model.Id).ToList();
        }

        public static List<Prediction> GetForArea(Area area, bool onlyFinishedPredictions)
        {
            return GetAll(onlyFinishedPredictions).Where(p => p.Model.TrainingArea.Id == area.Id || p.Model.PredictionArea.Id == area.Id).ToList();
        }

        public static int MaxRunId
        {
            get
            {
                List<Prediction> predictions = GetAll(false);
                if (predictions.Count == 0)
                    return 0;
                else
                    return predictions.Max(p => p.RunId);
            }
        }

        private DiscreteChoiceModel _model;
        private int _runId;
        private string _name;
        private Area _predictionArea;
        private int _predictionPointSpacing;
        private DateTime _predictionStartTime;
        private DateTime _predictionEndTime;
        private List<Plot> _assessmentPlots;
        private int _id;
        private bool _done;
        private DateTime _mostRecentlyEvaluatedIncidentTime;
        private string _modelDetails;
        private string _smoothingDetails;
        [NonSerialized]
        private List<Point> _points;
        [NonSerialized]
        private List<PointPrediction> _pointPredictions;

        public string SmoothingDetails
        {
            get { return _smoothingDetails; }
            set
            {
                if (_smoothingDetails == value)
                    return;

                _smoothingDetails = value;
                Update();
            }
        }

        public bool Done
        {
            get { return _done; }
            set
            {
                _done = value;
                Update();
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
                Update();
            }
        }

        public string PointPredictionLogPath
        {
            get { return Path.Combine(Model.ModelDirectory, "point_predictions.gz"); }
        }

        public DiscreteChoiceModel Model
        {
            get { return _model; }
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

                foreach (Plot assessmentPlot in _assessmentPlots)
                    if (assessmentPlot.Title.Contains(_name))
                        assessmentPlot.Title = assessmentPlot.Title.Replace(_name, value);

                _name = value;

                Update();
            }
        }

        public Area PredictionArea
        {
            get { return _predictionArea; }
        }

        public int PredictionPointSpacing
        {
            get { return _predictionPointSpacing; }
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
            set
            {
                _mostRecentlyEvaluatedIncidentTime = value;
                Update();
            }
        }

        public List<Plot> AssessmentPlots
        {
            get { return _assessmentPlots; }
            set
            {
                _assessmentPlots = value;
                Update();
            }
        }

        public string ModelDetails
        {
            get { return _modelDetails; }
            set
            {
                if (_modelDetails == value)
                    return;

                _modelDetails = value;
                Update();
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
                        string pointTable = Point.GetTableName(this);
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
                        string pointPredictionTable = PointPrediction.GetTableName(this);
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

        internal Prediction(DiscreteChoiceModel model, bool newRun, string name, Area predictionArea, int predictionPointSpacing, DateTime predictionStartTime, DateTime predictionEndTime, bool vacuum)
        {
            _model = model;
            _runId = MaxRunId + (newRun ? 1 : 0);
            _name = name;
            _predictionArea = predictionArea;
            _predictionPointSpacing = predictionPointSpacing;
            _predictionStartTime = predictionStartTime;
            _predictionEndTime = predictionEndTime;
            _assessmentPlots = new List<Plot>();
            _done = false;
            _mostRecentlyEvaluatedIncidentTime = DateTime.MinValue;
            _modelDetails = _smoothingDetails = null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            _id = Convert.ToInt32(DB.Connection.ExecuteScalar("INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES (" + _model.Id + ",@bytes," + _predictionArea.Id + ") RETURNING " + Columns.Id, new Parameter("bytes", NpgsqlDbType.Bytea, ms.ToArray())));

            if (vacuum)
                VacuumTable();
        }

        public void Update()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            DB.Connection.ExecuteNonQuery("UPDATE " + Table + " " +
                                          "SET " + Columns.ModelId + "=" + _model.Id + "," +
                                                   Columns.Prediction + "=@bytes" + "," +
                                                   Columns.PredictionAreaId + "=" + _predictionArea.Id + " " +
                                          "WHERE " + Columns.Id + "=" + _id,
                                          new Parameter("bytes", NpgsqlDbType.Bytea, ms.ToArray()));
        }

        public void Delete()
        {
            ReleaseAllLazyLoadedData();

            try { DB.Connection.ExecuteNonQuery("DELETE FROM " + Table + " WHERE " + Columns.Id + "=" + _id); }
            catch (Exception ex) { Console.Out.WriteLine("Failed to delete prediction from table:  " + ex.Message); }

            try { Point.DeleteTable(this); }
            catch (Exception ex) { Console.Out.WriteLine("Failed to delete point table:  " + ex.Message); }

            try { PointPrediction.DeleteTable(this); }
            catch (Exception ex) { Console.Out.WriteLine("Failed to delete point prediction table:  " + ex.Message); }

            try { Model.Delete(); }
            catch (Exception ex) { Console.Out.WriteLine("Failed to delete model for prediction:  " + ex.Message); }

            VacuumTable();
        }

        public override string ToString()
        {
            return _name;
        }

        public Prediction Copy(string newName, bool newRun, bool vacuum)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand(null);

            Prediction copiedPrediction = null;

            try
            {
                copiedPrediction = new Prediction(Model.Copy(), newRun, newName, _predictionArea, _predictionPointSpacing, _predictionStartTime, _predictionEndTime, false);

                string copiedPointTable = Point.CreateTable(copiedPrediction, PredictionArea.Shapefile.SRID);
                cmd.CommandText = "INSERT INTO " + copiedPointTable + " (" + Point.Columns.Insert + ") " +
                                  "SELECT " + Point.Columns.Insert + " " +
                                  "FROM " + Point.GetTableName(this) + " " +
                                  "ORDER BY " + Point.Columns.Id + " ASC";
                cmd.ExecuteNonQuery();

                string copiedPointPredictionTable = PointPrediction.CreateTable(copiedPrediction);
                cmd.CommandText = "INSERT INTO " + copiedPointPredictionTable + " (" + PointPrediction.Columns.Insert + ") " +
                                  "SELECT " + PointPrediction.Columns.Insert + " " +
                                  "FROM " + PointPrediction.GetTableName(this) + " " +
                                  "ORDER BY " + PointPrediction.Columns.Id + " ASC";
                cmd.ExecuteNonQuery();

                foreach (string path in Directory.GetFiles(Model.ModelDirectory))
                    File.Copy(path, Path.Combine(copiedPrediction.Model.ModelDirectory, Path.GetFileName(path)));

                copiedPrediction.SmoothingDetails = _smoothingDetails;
                copiedPrediction.Done = true;
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
                    try { Point.VacuumTable(copiedPrediction); }
                    catch (Exception ex) { Console.Out.WriteLine("Failed to vacuum point table for prediction \"" + copiedPrediction.Id + "\":  " + ex.Message); }
                    try { PointPrediction.VacuumTable(copiedPrediction); }
                    catch (Exception ex) { Console.Out.WriteLine("Failed to vacuum point prediction table for prediction \"" + copiedPrediction.Id + "\":  " + ex.Message); }
                }
            }

            return copiedPrediction;
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
                   indent + "Prediction area:  " + _predictionArea.GetDetails(indentLevel + 1) + Environment.NewLine + 
                   indent + "Prediction point spacing:  " + _predictionPointSpacing + Environment.NewLine + 
                   indent + "Prediction start:  " + _predictionStartTime.ToShortDateString() + " " + _predictionStartTime.ToShortTimeString() + Environment.NewLine +
                   indent + "Prediction end:  " + _predictionEndTime.ToShortDateString() + " " + _predictionEndTime.ToShortTimeString() + Environment.NewLine +
                   indent + "Smoothing:  " + _smoothingDetails + Environment.NewLine +
                   indent + "Time of most recently evaluated incident:  " + (_mostRecentlyEvaluatedIncidentTime == DateTime.MinValue ? "Never" : _mostRecentlyEvaluatedIncidentTime.ToShortDateString() + " " + _mostRecentlyEvaluatedIncidentTime.ToShortTimeString());
        }

        public int CompareTo(Prediction other)
        {
            return _id.CompareTo(other.Id);
        }     

        public void ReleasePoints()
        {
            _points = null;
            _pointPredictions = null;
        }

        /// <summary>
        /// Releases all data that was lazy-loaded into memory (e.g., points). Often this data can be large and needs to be cleaned up periodically.
        /// </summary>
        public void ReleaseAllLazyLoadedData()
        {
            ReleasePoints();
        }
    }
}
