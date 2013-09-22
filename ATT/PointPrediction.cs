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
        private static Set<string> _tables;
        private static object _staticLockObject = new object();

        public static string GetTable(int predictionId, bool create)
        {
            lock (_staticLockObject)
            {
                if (_tables == null)
                    _tables = new Set<string>(DB.Connection.GetTables().Where(t => t.StartsWith("point_prediction_")).ToArray());

                string table = "point_prediction_" + predictionId;
                if (!_tables.Contains(table))
                {
                    if (!create)
                        return null;

                    CreateTable(table, predictionId);
                }

                return table;
            }
        }

        internal static void DeleteTable(int predictionId)
        {
            string table = GetTable(predictionId, false);
            if (table != null)
            {
                try { DB.Connection.ExecuteNonQuery("DROP TABLE " + table + " CASCADE"); }
                finally { lock (_staticLockObject) { _tables.Remove(table); } }
            }
        }

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

        private static void CreateTable(string name, int predictionId)
        {
            DB.Connection.ExecuteNonQuery(
                   "CREATE TABLE IF NOT EXISTS " + name + " (" +
                   Columns.Id + " SERIAL PRIMARY KEY," +
                   Columns.Labels + " VARCHAR []," +
                   Columns.PointId + " INT REFERENCES " + Point.GetTable(predictionId, true) + " ON DELETE CASCADE," +
                   Columns.ThreatScores + " DOUBLE PRECISION []," +
                   Columns.Time + " TIMESTAMP," +
                   Columns.TotalThreat + " DOUBLE PRECISION);" +
                   "CREATE INDEX ON " + name + " (" + Columns.Labels + ");" +
                   "CREATE INDEX ON " + name + " (" + Columns.PointId + ");" +
                   "CREATE INDEX ON " + name + " (" + Columns.ThreatScores + ");" +
                   "CREATE INDEX ON " + name + " (" + Columns.Time + ");");

            lock (_tables) { _tables.Add(name); }
        }

        public static void VacuumTable(int predictionId)
        {
            DB.Connection.ExecuteNonQuery("VACUUM ANALYZE " + GetTable(predictionId, false));
        }

        public const string NullLabel = "NULL";

        internal static void Insert(IEnumerable<Tuple<string, Parameter>> valueParameters, int predictionId, bool vacuum)
        {
            Set<Thread> threads = new Set<Thread>();
            for (int start = 0; start < Configuration.ProcessorCount; ++start)
            {
                Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                    {
                        NpgsqlCommand cmd = DB.Connection.NewCommand(null);
                        StringBuilder cmdText = new StringBuilder();
                        int pointNum = 0;
                        int pointsPerBatch = 5000;
                        int skip = (int)o;
                        string table = GetTable(predictionId, true);
                        foreach (Tuple<string, Parameter> valueParameter in valueParameters)
                            if (skip-- <= 0)
                            {
                                cmdText.Append((cmdText.Length == 0 ? "INSERT INTO " + table + " (" + Columns.Insert + ") VALUES " : ",") + valueParameter.Item1);
                                ConnectionPool.AddParameters(cmd, valueParameter.Item2);

                                if ((++pointNum % pointsPerBatch) == 0)
                                {
                                    cmd.CommandText = cmdText.ToString();
                                    cmd.ExecuteNonQuery();
                                    cmdText.Clear();
                                    cmd.Parameters.Clear();
                                }

                                skip = Configuration.ProcessorCount - 1;
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
                VacuumTable(predictionId);
        }

        public static IEnumerable<PointPrediction> GetWithin(Polygon polygon, int predictionId)
        {
            string table = GetTable(predictionId, false);
            string pointTable = Point.GetTable(predictionId, false);
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

        internal static void UpdateThreatScores(IEnumerable<PointPrediction> pointPredictions, int predictionId)
        {
            Set<Thread> threads = new Set<Thread>();
            for (int i = 0; i < Configuration.ProcessorCount; ++i)
            {
                Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                    {
                        int skip = (int)o;

                        int pointsPerBatch = 1000;
                        int pointNum = 0;
                        NpgsqlCommand cmd = DB.Connection.NewCommand("");
                        StringBuilder cmdText = new StringBuilder();
                        StringBuilder labels = new StringBuilder();
                        StringBuilder scores = new StringBuilder();
                        string table = GetTable(predictionId, false);
                        foreach (PointPrediction pointPrediction in pointPredictions)
                            if (skip-- <= 0)
                            {
                                labels.Clear();
                                scores.Clear();
                                double totalThreat = 0;
                                foreach (string incident in pointPrediction.IncidentScore.Keys)
                                {
                                    labels.Append((labels.Length == 0 ? "'{" : ",") + "\"" + incident + "\"");
                                    scores.Append((scores.Length == 0 ? "'{" : ",") + pointPrediction.IncidentScore[incident]);
                                    totalThreat += pointPrediction.IncidentScore[incident];
                                }

                                labels.Append("}'");
                                scores.Append("}'");

                                cmdText.Append("UPDATE " + table + " " +
                                               "SET " + Columns.Labels + "=" + labels + "," +
                                                        Columns.ThreatScores + "=" + scores + "," +
                                                        Columns.TotalThreat + "=" + totalThreat + " " +
                                               "WHERE " + Columns.Id + "=" + pointPrediction.Id + ";");

                                if (++pointNum >= pointsPerBatch)
                                {
                                    cmd.CommandText = cmdText.ToString();
                                    cmd.ExecuteNonQuery();
                                    pointNum = 0;
                                    cmdText.Clear();
                                }

                                skip = Configuration.ProcessorCount - 1;
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

        internal PointPrediction(int id, string table)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select(table) + " FROM " + table + " WHERE " + Columns.Id + "=" + id);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            Construct(reader, table);
            reader.Close();
            DB.Connection.Return(cmd.Connection);
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
            _incidentScore = new Dictionary<string, double>();
            for (int i = 0; i < labels.Length; ++i)
                _incidentScore.Add(labels[i], threatScores[i]);
        }
    }
}
