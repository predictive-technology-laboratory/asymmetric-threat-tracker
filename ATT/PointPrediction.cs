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
using LAIR.MachineLearning;
using LAIR.ResourceAPIs.PostgreSQL;
using NpgsqlTypes;
using System.Threading;
using LAIR.Collections.Generic;
using LAIR.ResourceAPIs.PostGIS;

namespace PTL.ATT
{
    public class PointPrediction
    {
        public class Columns
        {
            [Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Labels = "labels";
            [Reflector.Insert, Reflector.Select(true)]
            public const string PointId = "point_id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string ThreatScores = "threat_scores";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Time = "time";
            [Reflector.Insert, Reflector.Select(true)]
            public const string TotalThreat = "total_threat";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select(string table) { return Reflector.GetSelectColumns(table, typeof(Columns)); }
            public static string JoinPoint(string predictionPointTable, string pointTable) { return pointTable + " JOIN " + predictionPointTable + " ON " + pointTable + "." + Point.Columns.Id + "=" + predictionPointTable + "." + PointId; }
        }

        public const string NullLabel = "NULL";

        internal static string GetTableName(Prediction prediction)
        {
            return "point_prediction_" + prediction.Id;
        }

        internal static string CreateTable(Prediction prediction)
        {
            string table = GetTableName(prediction);

            DB.Connection.ExecuteNonQuery(
                "CREATE TABLE " + table + " (" +
                 Columns.Id + " SERIAL PRIMARY KEY," +
                 Columns.Labels + " VARCHAR []," +
                 Columns.PointId + " INT REFERENCES " + Point.GetTableName(prediction) + " ON DELETE CASCADE," +
                 Columns.ThreatScores + " DOUBLE PRECISION []," +
                 Columns.Time + " TIMESTAMP," +
                 Columns.TotalThreat + " DOUBLE PRECISION);" +
                 "CREATE INDEX ON " + table + " (" + Columns.Labels + ");" +
                 "CREATE INDEX ON " + table + " (" + Columns.PointId + ");" +
                 "CREATE INDEX ON " + table + " (" + Columns.ThreatScores + ");" +
                 "CREATE INDEX ON " + table + " (" + Columns.Time + ");");

            return table;
        }

        internal static void DeleteTable(Prediction prediction)
        {
            DB.Connection.ExecuteNonQuery("DROP TABLE " + GetTableName(prediction) + " CASCADE");
        }

        public static void VacuumTable(Prediction prediction)
        {
            DB.Connection.ExecuteNonQuery("VACUUM ANALYZE " + GetTableName(prediction));
        }

        public static string GetValue(int pointId, string timeParameterName, IEnumerable<KeyValuePair<string, double>> incidentScore, double totalThreat)
        {
            string labels, scores;
            GetLabelsScoresSQL(incidentScore, out labels, out scores);

            return "(" + labels + "," + pointId + "," + scores + "," + timeParameterName + "," + totalThreat + ")";
        }

        public static void GetLabelsScoresSQL(IEnumerable<KeyValuePair<string, double>> incidentScores, out string labels, out string scores)
        {
            StringBuilder labelsBuilder = new StringBuilder("'{");
            StringBuilder scoresBuilder = new StringBuilder("'{");
            int i = 0;
            foreach (KeyValuePair<string, double> incidentScore in incidentScores.OrderBy(kvp => kvp.Key))
                if (incidentScore.Key != PointPrediction.NullLabel)
                {
                    labelsBuilder.Append((i == 0 ? "" : ",") + "\"" + incidentScore.Key + "\"");
                    scoresBuilder.Append((i == 0 ? "" : ",") + incidentScore.Value);
                    ++i;
                }

            labelsBuilder.Append("}'");
            scoresBuilder.Append("}'");

            labels = labelsBuilder.ToString();
            scores = scoresBuilder.ToString();
        }

        internal static void Insert(IEnumerable<Tuple<string, Parameter>> valueParameters, Prediction prediction, bool vacuum)
        {
            List<Tuple<string, Parameter>> valueParametersList = valueParameters.ToList();
            Set<Thread> threads = new Set<Thread>();
            for (int start = 0; start < Configuration.ProcessorCount; ++start)
            {
                Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                    {
                        NpgsqlCommand cmd = DB.Connection.NewCommand(null);
                        StringBuilder cmdText = new StringBuilder();
                        int pointNum = 0;
                        int pointsPerBatch = 5000;
                        int core = (int)o;
                        string table = GetTableName(prediction);
                        for (int j = 0; j + core < valueParametersList.Count; j += Configuration.ProcessorCount)
                        {
                            Tuple<string, Parameter> valueParameter = valueParametersList[j + core];
                            cmdText.Append((cmdText.Length == 0 ? "INSERT INTO " + table + " (" + Columns.Insert + ") VALUES " : ",") + valueParameter.Item1);
                            ConnectionPool.AddParameters(cmd, valueParameter.Item2);

                            if ((++pointNum % pointsPerBatch) == 0)
                            {
                                cmd.CommandText = cmdText.ToString();
                                cmd.ExecuteNonQuery();
                                cmdText.Clear();
                                cmd.Parameters.Clear();
                            }

                        }

                        if (cmdText.Length > 0)
                        {
                            cmd.CommandText = cmdText.ToString();
                            cmd.ExecuteNonQuery();
                            cmdText.Clear();
                            cmd.Parameters.Clear();
                        }

                        DB.Connection.Return(cmd.Connection);
                    }));

                t.Start(start);
                threads.Add(t);
            }

            foreach (Thread t in threads)
                t.Join();

            if (vacuum)
                VacuumTable(prediction);
        }

        public static IEnumerable<PointPrediction> GetWithin(Polygon polygon, Prediction prediction)
        {
            string table = GetTableName(prediction);
            string pointTable = Point.GetTableName(prediction);
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select(table) + " " +
                                                         "FROM " + Columns.JoinPoint(table, pointTable) + " " +
                                                         "WHERE st_intersects(" + pointTable + "." + Point.Columns.Location + "," + polygon.StGeometryFromText + ")");

            List<PointPrediction> predictions = new List<PointPrediction>();
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                predictions.Add(new PointPrediction(reader, table));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return predictions;
        }

        internal static void UpdateThreatScores(IEnumerable<PointPrediction> pointPredictions, Prediction prediction)
        {
            List<PointPrediction> pointPredictionsList = pointPredictions.ToList();
            Set<Thread> threads = new Set<Thread>();
            for (int i = 0; i < Configuration.ProcessorCount; ++i)
            {
                Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                    {
                        int core = (int)o;

                        int pointsPerBatch = 1000;
                        int pointNum = 0;
                        NpgsqlCommand cmd = DB.Connection.NewCommand("");
                        StringBuilder cmdText = new StringBuilder();
                        string table = GetTableName(prediction);
                        for (int j = 0; j + core < pointPredictionsList.Count; j += Configuration.ProcessorCount)
                        {
                            PointPrediction pointPrediction = pointPredictionsList[j + core];
                            string labels, scores;
                            GetLabelsScoresSQL(pointPrediction.IncidentScore, out labels, out scores);

                            cmdText.Append("UPDATE " + table + " " +
                                           "SET " + Columns.Labels + "=" + labels + "," +
                                                    Columns.ThreatScores + "=" + scores + "," +
                                                    Columns.TotalThreat + "=" + pointPrediction.IncidentScore.Values.Sum() + " " +
                                           "WHERE " + Columns.Id + "=" + pointPrediction.Id + ";");

                            if (++pointNum >= pointsPerBatch)
                            {
                                cmd.CommandText = cmdText.ToString();
                                cmd.ExecuteNonQuery();
                                pointNum = 0;
                                cmdText.Clear();
                            }

                        }

                        if (pointNum > 0)
                        {
                            cmd.CommandText = cmdText.ToString();
                            cmd.ExecuteNonQuery();
                        }

                        DB.Connection.Return(cmd.Connection);
                    }));

                t.Start(i);
                threads.Add(t);
            }

            foreach (Thread t in threads)
                t.Join();
        }

        private int _id;
        private int _pointId;
        private Dictionary<string, double> _incidentScore;
        private DateTime _time;
        private double _totalThreat;

        public int Id
        {
            get { return _id; }
        }

        public int PointId
        {
            get { return _pointId; }
        }

        public Dictionary<string, double> IncidentScore
        {
            get { return _incidentScore; }
            set { _incidentScore = value; }
        }

        public DateTime Time
        {
            get { return _time; }
        }

        public double TotalThreat
        {
            get { return _totalThreat; }
            set { _totalThreat = value; }
        }

        internal PointPrediction(NpgsqlDataReader reader, string table)
        {
            Construct(reader, table);
        }

        private void Construct(NpgsqlDataReader reader, string table)
        {
            _id = Convert.ToInt32(reader[table + "_" + Columns.Id]);
            _pointId = Convert.ToInt32(reader[table + "_" + Columns.PointId]);
            _time = Convert.ToDateTime(reader[table + "_" + Columns.Time]);
            _totalThreat = Convert.ToDouble(reader[table + "_" + Columns.TotalThreat]);

            string[] labels = reader[table + "_" + Columns.Labels] as string[];
            double[] threatScores = reader[table + "_" + Columns.ThreatScores] as double[];

            if (labels == null || threatScores == null)
                throw new NullReferenceException("Failed to get labels / threat scores from database");

            _incidentScore = new Dictionary<string, double>();
            for (int i = 0; i < labels.Length; ++i)
                _incidentScore.Add(labels[i], threatScores[i]);
        }
    }
}
