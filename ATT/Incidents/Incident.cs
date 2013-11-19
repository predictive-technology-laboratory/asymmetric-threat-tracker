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
using NpgsqlTypes;
using LAIR.ResourceAPIs.PostgreSQL;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using System.Reflection;

namespace PTL.ATT.Incidents
{
    public class Incident
    {
        #region static members
        internal class Columns
        {
            [Reflector.Insert, Reflector.Select(true)]
            internal const string AreaId = "area_id";
            [Reflector.Select(true)]
            internal const string Id = "id";
            [Reflector.Insert]
            internal const string Location = "location";
            [Reflector.Insert, Reflector.Select(true)]
            internal const string Simulated = "simulated";
            [Reflector.Insert, Reflector.Select(true)]
            internal const string Time = "time";
            [Reflector.Insert, Reflector.Select(true)]
            internal const string Type = "type";
            [Reflector.Select(false)]
            internal static string StX(int srid) { return "st_x(" + GetTableName(srid) + "." + Location + ") as " + X(srid); }
            internal static string X(int srid) { return GetTableName(srid) + "_x"; }
            [Reflector.Select(false)]
            internal static string StY(int srid) { return "st_y(" + GetTableName(srid) + "." + Location + ") as " + Y(srid); }
            internal static string Y(int srid) { return GetTableName(srid) + "_y"; }
            [Reflector.Select(false)]
            internal static string StSRID(int srid) { return "st_srid(" + GetTableName(srid) + "." + Location + ") as " + SRID(srid); }
            internal static string SRID(int srid) { return GetTableName(srid) + "_srid"; }

            internal static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            internal static string Select(int srid) { return Reflector.GetSelectColumns(GetTableName(srid), typeof(Columns)); }
        }

        internal static string GetTableName(int srid)
        {
            return "incident_" + srid;
        }

        internal static string CreateTable(int srid)
        {
            string tableName = GetTableName(srid);

            if (!DB.Connection.TableExists(tableName))
                DB.Connection.ExecuteNonQuery(
                    "CREATE TABLE " + tableName + " (" +
                    Columns.AreaId + " INTEGER REFERENCES " + Area.Table + " ON DELETE CASCADE," + 
                    Columns.Id + " SERIAL PRIMARY KEY," +
                    Columns.Location + " GEOMETRY(POINT," + srid + ")," +
                    Columns.Simulated + " BOOLEAN," +
                    Columns.Time + " TIMESTAMP," +
                    Columns.Type + " VARCHAR);" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.AreaId + ");" +
                    "CREATE INDEX ON " + tableName + " USING GIST (" + Columns.Location + ");" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.Simulated + ");" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.Time + ");" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.Type + ");");

            return tableName;
        }

        internal static void VacuumTable(int srid)
        {
            DB.Connection.ExecuteNonQuery("VACUUM ANALYZE " + GetTableName(srid));
        }

        internal static string GetValue(int areaId, string location, bool simulated, string time, string type)
        {
            return areaId + "," + location + "," + simulated + "," + time + ",'" + type + "'";
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

            NpgsqlCommand cmd = DB.Connection.NewCommand(null);

            string tableName = CreateTable(area.SRID);

            cmd.CommandText = "BEGIN";
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
                PostGIS.Point location = new PostGIS.Point(x, y, area.SRID);

                cmdText.Append("INSERT INTO " + tableName + " (" + Columns.Insert + ") VALUES (" + GetValue(area.Id, location.StGeometryFromText, true, "@date" + numInBatch, type) + ");");
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

            VacuumTable(area.SRID);
        }

        public static void Clear(Area area)
        {
            DB.Connection.ExecuteNonQuery("DELETE FROM " + GetTableName(area.SRID) + " WHERE " + Columns.AreaId + "=" + area.Id);
            VacuumTable(area.SRID);
        }

        public static void ClearSimulated(Area area)
        {
            DB.Connection.ExecuteNonQuery("DELETE FROM " + GetTableName(area.SRID) + " WHERE " + Columns.AreaId + "=" + area.Id + " AND " + Columns.Simulated + "=" + true);
            VacuumTable(area.SRID);
        }

        public static IEnumerable<string> GetUniqueTypes(DateTime start, DateTime end, Area area)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT DISTINCT " + Columns.Type + " " +
                                                         "FROM " + GetTableName(area.SRID) + " " +
                                                         "WHERE " + Columns.Time + " >= @start AND " + Columns.Time + " <= @end AND " + Columns.AreaId + "=" + area.Id,
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

        public static IEnumerable<Incident> Get(DateTime start, DateTime end, Area area, params string[] types)
        {
            string typesCondition = null;
            if (types != null && types.Length > 0)
            {
                foreach (string type in types)
                    typesCondition = (typesCondition == null ? "(" : typesCondition + " OR ") + Columns.Type + "='" + type + "'";

                typesCondition += ")";
            }

            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select(area.SRID) + " " +
                                                         "FROM " + GetTableName(area.SRID) + " " +
                                                         "WHERE " + (typesCondition == null ? "" : typesCondition + " AND ") +
                                                                    Columns.AreaId + "=" + area.Id + " AND " +
                                                                    Columns.Time + " >= @start AND " +
                                                                    Columns.Time + " <= @end", new Parameter("start", NpgsqlDbType.Timestamp, start), new Parameter("end", NpgsqlDbType.Timestamp, end));
            List<Incident> incidents = new List<Incident>();
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                incidents.Add(new Incident(reader, area.SRID));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return incidents;
        }

        public static int Count(DateTime start, DateTime end, Area area, params string[] types)
        {
            string typesCondition = null;
            if (types != null)
                foreach (string type in types)
                    typesCondition = (typesCondition == null ? "" : typesCondition + " OR ") + Columns.Type + "='" + type + "'";

            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT COUNT(*) " + 
                                                         "FROM " + GetTableName(area.SRID) + " " +
                                                         "WHERE " + (typesCondition == null ? "" : typesCondition + " AND ") +
                                                                    Columns.AreaId + "=" + area.Id + " AND " + 
                                                                    Columns.Time + " >= @start AND " +
                                                                    Columns.Time + " <= @end", new Parameter("start", NpgsqlDbType.Timestamp, start), new Parameter("end", NpgsqlDbType.Timestamp, end));
            int count = Convert.ToInt32(cmd.ExecuteScalar());

            DB.Connection.Return(cmd.Connection);

            return count;
        }
        #endregion

        private int _areaId;
        private int _id;
        private PostGIS.Point _location;
        private bool _simulated;
        private DateTime _time;
        private string _type;

        public int AreaId
        {
            get { return _areaId; }
        }

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

        protected Incident(NpgsqlDataReader reader, int srid)
        {
            string tableName = GetTableName(srid);

            _areaId = Convert.ToInt32(reader[tableName + "_" + Columns.AreaId]);
            _id = Convert.ToInt32(reader[tableName + "_" + Columns.Id]);
            _location = new PostGIS.Point(Convert.ToDouble(reader[Columns.X(srid)]), Convert.ToDouble(reader[Columns.Y(srid)]), Convert.ToInt32(reader[Columns.SRID(srid)]));
            _simulated = Convert.ToBoolean(reader[tableName + "_" + Columns.Simulated]);
            _time = Convert.ToDateTime(reader[tableName + "_" + Columns.Time]);
            _type = Convert.ToString(reader[tableName + "_" + Columns.Type]);
        }

        public override string ToString()
        {
            return _type + " (" + _time + ")";
        }
    }
}
