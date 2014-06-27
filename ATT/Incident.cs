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
using NpgsqlTypes;
using LAIR.ResourceAPIs.PostgreSQL;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using System.Reflection;
using LAIR.Collections.Generic;

namespace PTL.ATT
{
    public class Incident
    {
        #region static members
        public class Columns
        {
            [Reflector.Insert, Reflector.Select(true)]
            public const string AreaId = "area_id";
            [Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert]
            public const string Location = "location";
            [Reflector.Insert, Reflector.Select(true)]
            public const string NativeId = "native_id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Simulated = "simulated";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Time = "time";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Type = "type";

            public static string StX(Area area) { return "st_x(" + GetTableName(area) + "." + Location + ") as " + X(area); }
            public static string X(Area area) { return GetTableName(area) + "_x"; }
            public static string StY(Area area) { return "st_y(" + GetTableName(area) + "." + Location + ") as " + Y(area); }
            public static string Y(Area area) { return GetTableName(area) + "_y"; }
            public static string StSRID(Area area) { return "st_srid(" + GetTableName(area) + "." + Location + ") as " + SRID(area); }
            public static string SRID(Area area) { return GetTableName(area) + "_srid"; }

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select(Area area) { return Reflector.GetSelectColumns(GetTableName(area), new string[] { StX(area), StY(area), StSRID(area) }, typeof(Columns)); }
        }

        public static string GetTableName(Area area)
        {
            string tableName = "incident_" + area.Shapefile.SRID;
            string nativeIdSeqName = tableName + "_native_id_seq";
            if (!DB.Connection.TableExists(tableName))
                DB.Connection.ExecuteNonQuery(
                    "CREATE SEQUENCE " + nativeIdSeqName + ";" +
                    "CREATE TABLE " + tableName + " (" +
                    Columns.AreaId + " INTEGER REFERENCES " + Area.Table + " ON DELETE CASCADE," +
                    Columns.Id + " SERIAL PRIMARY KEY," +
                    Columns.Location + " GEOMETRY(POINT," + area.Shapefile.SRID + ")," +
                    Columns.NativeId + " INT DEFAULT nextval('" + nativeIdSeqName + "')," +
                    Columns.Simulated + " BOOLEAN," +
                    Columns.Time + " TIMESTAMP," +
                    Columns.Type + " VARCHAR," +
                    "UNIQUE (" + Columns.AreaId + "," + Columns.NativeId + "));" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.AreaId + ");" +
                    "CREATE INDEX ON " + tableName + " USING GIST (" + Columns.Location + ");" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.NativeId + ");" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.Simulated + ");" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.Time + ");" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.Type + ");");

            return tableName;
        }

        internal static void VacuumTable(Area area)
        {
            DB.Connection.ExecuteNonQuery("VACUUM ANALYZE " + GetTableName(area));
        }

        public static string GetValue(Area area, int nativeId, PostGIS.Point location, bool simulated, string time, string type)
        {
            return area.Id + "," + (location.SRID == area.Shapefile.SRID ? location.StGeometryFromText : "st_transform(" + location.StGeometryFromText + "," + area.Shapefile.SRID + ")") + "," + nativeId + "," + simulated + "," + time + ",'" + Util.Escape(type) + "'";
        }

        public static void Clear(Area area)
        {
            if (DB.Connection.TableExists(GetTableName(area)))
            {
                DB.Connection.ExecuteNonQuery("DELETE FROM " + GetTableName(area) + " WHERE " + Columns.AreaId + "=" + area.Id);
                VacuumTable(area);
            }
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

            string tableName = GetTableName(area);

            cmd.CommandText = "BEGIN";
            cmd.ExecuteNonQuery();

            StringBuilder cmdText = new StringBuilder();
            int numInBatch = 0;
            int batchSize = 500;
            Random r = new Random();
            List<Parameter> param = new List<Parameter>(batchSize);
            int nativeIdStart = Convert.ToInt32(DB.Connection.ExecuteScalar("SELECT max(" + Incident.Columns.NativeId + ") FROM " + Incident.GetTableName(area) + " WHERE " + Incident.Columns.AreaId + "=" + area.Id)) + 1;
            int nativeIdEnd = nativeIdStart + n - 1;
            for (int nativeId = nativeIdStart; nativeId <= nativeIdEnd; ++nativeId)
            {
                DateTime date = startDate.AddDays(r.Next(dayRange));
                double x = minX + r.NextDouble() * xRange;
                double y = minY + r.NextDouble() * yRange;
                string type = incidentTypes[r.Next(incidentTypes.Length)];
                PostGIS.Point location = new PostGIS.Point(x, y, area.Shapefile.SRID);

                cmdText.Append("INSERT INTO " + tableName + " (" + Columns.Insert + ") VALUES (" + GetValue(area, nativeId, location, true, "@date" + numInBatch, type) + ");");
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

            VacuumTable(area);
        }

        public static void ClearSimulated(Area area)
        {
            if (DB.Connection.TableExists(GetTableName(area)))
            {
                DB.Connection.ExecuteNonQuery("DELETE FROM " + GetTableName(area) + " WHERE " + Columns.AreaId + "=" + area.Id + " AND " + Columns.Simulated + "=" + true);
                VacuumTable(area);
            }
        }

        public static IEnumerable<string> GetUniqueTypes(DateTime start, DateTime end, Area area)
        {
            List<string> types = new List<string>();

            if (DB.Connection.TableExists(GetTableName(area)))
            {
                NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT DISTINCT " + Columns.Type + " " +
                                                             "FROM " + GetTableName(area) + " " +
                                                             "WHERE " + Columns.Time + " >= @start AND " + Columns.Time + " <= @end AND " + Columns.AreaId + "=" + area.Id,
                                                             new Parameter("start", NpgsqlDbType.Timestamp, start),
                                                             new Parameter("end", NpgsqlDbType.Timestamp, end));


                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    types.Add(Convert.ToString(reader[Columns.Type]));

                reader.Close();
                DB.Connection.Return(cmd.Connection);
            }

            return types;
        }

        public static IEnumerable<Incident> Get(DateTime start, DateTime end, Area area, params string[] types)
        {
            List<Incident> incidents = new List<Incident>();

            if (DB.Connection.TableExists(GetTableName(area)))
            {
                string typesCondition = null;
                if (types != null && types.Length > 0)
                {
                    foreach (string type in types)
                        typesCondition = (typesCondition == null ? "(" : typesCondition + " OR ") + Columns.Type + "='" + type + "'";

                    typesCondition += ")";
                }

                NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select(area) + " " +
                                                             "FROM " + GetTableName(area) + " " +
                                                             "WHERE " + (typesCondition == null ? "" : typesCondition + " AND ") +
                                                                        Columns.AreaId + "=" + area.Id + " AND " +
                                                                        Columns.Time + " >= @start AND " +
                                                                        Columns.Time + " <= @end", new Parameter("start", NpgsqlDbType.Timestamp, start), new Parameter("end", NpgsqlDbType.Timestamp, end));

                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    incidents.Add(new Incident(reader, area));

                reader.Close();
                DB.Connection.Return(cmd.Connection);
            }

            return incidents;
        }

        public static int Count(DateTime start, DateTime end, Area area, params string[] types)
        {
            if (DB.Connection.TableExists(GetTableName(area)))
            {
                string typesCondition = null;
                if (types != null && types.Length > 0)
                {
                    foreach (string type in types)
                        typesCondition = (typesCondition == null ? "(" : typesCondition + " OR ") + Columns.Type + "='" + type + "'";

                    typesCondition += ")";
                }

                NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT COUNT(*) " +
                                                             "FROM " + GetTableName(area) + " " +
                                                             "WHERE " + (typesCondition == null ? "" : typesCondition + " AND ") +
                                                                        Columns.AreaId + "=" + area.Id + " AND " +
                                                                        Columns.Time + " >= @start AND " +
                                                                        Columns.Time + " <= @end", new Parameter("start", NpgsqlDbType.Timestamp, start), new Parameter("end", NpgsqlDbType.Timestamp, end));
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                DB.Connection.Return(cmd.Connection);

                return count;
            }
            else
                return 0;
        }

        public static Incident GetFirst(Area area)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select(area) + " FROM " + GetTableName(area) + " ORDER BY " + Columns.Time + " ASC LIMIT 1");
            NpgsqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
                return new Incident(reader, area);
            else
                return null;
        }

        public static Incident GetLast(Area area)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select(area) + " FROM " + GetTableName(area) + " ORDER BY " + Columns.Time + " DESC LIMIT 1");
            NpgsqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
                return new Incident(reader, area);
            else
                return null;
        }

        public static Set<int> GetNativeIds(Area area)
        {
            Set<int> nativeIds = new Set<int>();
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.NativeId + " FROM " + GetTableName(area) + " WHERE " + Columns.AreaId + "=" + area.Id);
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    nativeIds.Add(Convert.ToInt32(reader[0]));

                reader.Close();
            }

            DB.Connection.Return(cmd.Connection);

            return nativeIds;
        }
        #endregion

        private int _areaId;
        private int _id;
        private PostGIS.Point _location;
        private bool _simulated;
        private DateTime _time;
        private string _type;
        private int _nativeId;

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

        public int NativeId
        {
            get { return _nativeId; }
        }

        protected Incident(NpgsqlDataReader reader, Area area)
        {
            string tableName = GetTableName(area);

            _areaId = Convert.ToInt32(reader[tableName + "_" + Columns.AreaId]);
            _id = Convert.ToInt32(reader[tableName + "_" + Columns.Id]);
            _location = new PostGIS.Point(Convert.ToDouble(reader[Columns.X(area)]), Convert.ToDouble(reader[Columns.Y(area)]), Convert.ToInt32(reader[Columns.SRID(area)]));
            _simulated = Convert.ToBoolean(reader[tableName + "_" + Columns.Simulated]);
            _time = Convert.ToDateTime(reader[tableName + "_" + Columns.Time]);
            _type = Convert.ToString(reader[tableName + "_" + Columns.Type]);
            _nativeId = Convert.ToInt32(reader[tableName + "_" + Columns.NativeId]);
        }

        public override string ToString()
        {
            return _type + " (" + _time + ")";
        }
    }
}
