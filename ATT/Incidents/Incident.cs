#region copyright
//    Copyright 2013 Matthew S. Gerber (gerber.matthew@gmail.com)
//
//    This file is part of the Asymmetric Threat Tracker (ATT).
//
//    The ATT is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    The ATT is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with the ATT.  If not, see <http://www.gnu.org/licenses/>.
#endregion
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using LAIR.ResourceAPIs.PostgreSQL;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using System.Reflection;

namespace PTL.ATT.Incidents
{
    public class Incident
    {
        #region static members
        public const string Table = "incident";

        public class Columns
        {
            [Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert]
            public const string Location = "location";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Simulated = "simulated";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Time = "time";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Type = "type";
            [Reflector.Select(false)]
            public const string StX = "st_x(" + Table + "." + Location + ") as " + X;
            public const string X = Table + "_x";
            [Reflector.Select(false)]
            public const string StY = "st_y(" + Table + "." + Location + ") as " + Y;
            public const string Y = Table + "_y";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return Reflector.GetSelectColumns(Table, typeof(Columns)); } }
        }

        [ConnectionPool.CreateTable]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Id + " SERIAL PRIMARY KEY," +
                   Columns.Location + " GEOMETRY(POINT," + Configuration.PostgisSRID + ")," +
                   Columns.Simulated + " BOOLEAN," +
                   Columns.Time + " TIMESTAMP," +
                   Columns.Type + " VARCHAR);" + 
                   (connection.TableExists(Table) ? "" :
                   "CREATE INDEX ON " + Table + " USING GIST (" + Columns.Location + ");" +
                   "CREATE INDEX ON " + Table + " (" + Columns.Simulated + ");" +
                   "CREATE INDEX ON " + Table + " (" + Columns.Time + ");" +
                   "CREATE INDEX ON " + Table + " (" + Columns.Type + ");");
        }

        internal static void VacuumTable()
        {
            DB.Connection.ExecuteNonQuery("VACUUM ANALYZE " + Table);
        }

        internal static string GetValue(string location, bool simulated, string time, string type)
        {
            return location + "," + simulated + "," + time + ",'" + type + "'";
        }

        public static void Simulate(Area area, string[] incidentTypes, DateTime startDate, DateTime endDate, int n)
        {
            double minX = area.BoundingBox.MinX;
            double maxX = area.BoundingBox.MaxX;
            double minY = area.BoundingBox.MinY;
            double maxY = area.BoundingBox.MaxY;
            double xRange = maxX - minX;
            double yRange = maxY - minY;

            int dayRange = ((int)(endDate.Subtract(startDate).TotalDays)) + 1;

            NpgsqlCommand cmd = DB.Connection.NewCommand("BEGIN");
            cmd.ExecuteNonQuery();

            StringBuilder cmdText = new StringBuilder();
            int numInBatch = 0;
            int batchSize = 500;
            Random r = new Random();
            List<Parameter> param = new List<Parameter>(batchSize);
            for (int i = 0; i < n; ++i)
            {
                DateTime date = startDate.AddDays(r.Next(dayRange));
                double x = minX + r.NextDouble() * xRange;
                double y = minY + r.NextDouble() * yRange;
                string type = incidentTypes[r.Next(incidentTypes.Length)];
                PostGIS.Point location = new PostGIS.Point(x, y, Configuration.PostgisSRID);

                cmdText.Append("INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES (" + GetValue(location.StGeometryFromText, true, "@date" + numInBatch, type) + ");");
                param.Add(new Parameter("date" + numInBatch, NpgsqlDbType.Timestamp, date));

                if (++numInBatch >= batchSize)
                {
                    cmd.CommandText = cmdText.ToString();
                    ConnectionPool.AddParameters(cmd, param);
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    cmdText.Clear();
                    param.Clear();
                    numInBatch = 0;
                }
            }

            if (numInBatch > 0)
            {
                cmd.CommandText = cmdText.ToString();
                ConnectionPool.AddParameters(cmd, param);
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                cmdText.Clear();
                param.Clear();
                numInBatch = 0;
            }

            cmd.CommandText = "COMMIT";
            cmd.ExecuteNonQuery();

            DB.Connection.Return(cmd.Connection);

            VacuumTable();
        }

        public static void Clear()
        {
            DB.Connection.ExecuteNonQuery("DELETE FROM " + Table);
            VacuumTable();
        }

        public static void ClearSimulated()
        {
            DB.Connection.ExecuteNonQuery("DELETE FROM " + Table + " WHERE " + Columns.Simulated + "=" + true);
            VacuumTable();
        }

        public static IEnumerable<string> GetUniqueTypes(DateTime start, DateTime end)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT DISTINCT " + Columns.Type + " " +
                                                         "FROM " + Table + " " +
                                                         "WHERE " + Columns.Time + " >= @start AND " + Columns.Time + " <= @end",
                                                         new Parameter("start", NpgsqlDbType.Timestamp, start),
                                                         new Parameter("end", NpgsqlDbType.Timestamp, end));

            List<string> types = new List<string>();
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                types.Add(Convert.ToString(reader[Columns.Type]));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return types;
        }

        public static IEnumerable<Incident> Get(DateTime start, DateTime end, params string[] types)
        {
            string typesCondition = null;
            if (types != null)
                foreach (string type in types)
                    typesCondition = (typesCondition == null ? "" : typesCondition + " OR ") + Columns.Type + "='" + type + "'";

            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " " +
                                                         "FROM " + Table + " " +
                                                         "WHERE " + (typesCondition == null ? "" : typesCondition + " AND ") +
                                                                    Columns.Time + " >= @start AND " +
                                                                    Columns.Time + " <= @end", new Parameter("start", NpgsqlDbType.Timestamp, start), new Parameter("end", NpgsqlDbType.Timestamp, end));
            List<Incident> incidents = new List<Incident>();
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                incidents.Add(new Incident(reader));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return incidents;
        }

        public static int Count(DateTime start, DateTime end, params string[] types)
        {
            string typesCondition = null;
            if (types != null)
                foreach (string type in types)
                    typesCondition = (typesCondition == null ? "" : typesCondition + " OR ") + Columns.Type + "='" + type + "'";

            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT COUNT(*) " + 
                                                         "FROM " + Table + " " +
                                                         "WHERE " + (typesCondition == null ? "" : typesCondition + " AND ") +
                                                                    Columns.Time + " >= @start AND " +
                                                                    Columns.Time + " <= @end", new Parameter("start", NpgsqlDbType.Timestamp, start), new Parameter("end", NpgsqlDbType.Timestamp, end));
            int count = Convert.ToInt32(cmd.ExecuteScalar());

            DB.Connection.Return(cmd.Connection);

            return count;
        }
        #endregion

        private int _id;
        private PostGIS.Point _location;
        private bool _simulated;
        private DateTime _time;
        private string _type;

        public int Id
        {
            get { return _id; }
        }

        public PostGIS.Point Location
        {
            get { return _location; }
        }

        public bool Simulated
        {
            get { return _simulated; }
        }

        public DateTime Time
        {
            get { return _time; }
        }

        public string Type
        {
            get { return _type; }
        }

        protected Incident(NpgsqlDataReader reader)
        {
            _id = Convert.ToInt32(reader[Table + "_" + Columns.Id]);
            _location = new PostGIS.Point(Convert.ToDouble(reader[Columns.X]), Convert.ToDouble(reader[Columns.Y]), Configuration.PostgisSRID);
            _simulated = Convert.ToBoolean(reader[Table + "_" + Columns.Simulated]);
            _time = Convert.ToDateTime(reader[Table + "_" + Columns.Time]);
            _type = Convert.ToString(reader[Table + "_" + Columns.Type]);
        }

        public override string ToString()
        {
            return _type + " (" + _time + ")";
        }
    }
}
