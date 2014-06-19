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
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using LAIR.ResourceAPIs.PostgreSQL;
using NpgsqlTypes;
using LAIR.Collections.Generic;
using LAIR.MachineLearning;

namespace PTL.ATT
{
    public class Point : ClassifiableEntity
    {
        public class Columns
        {
            [Reflector.Insert]
            public const string Core = "core";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string IncidentType = "incident_type";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Location = "location";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Time = "time";

            public static string StX(string table) { return "st_x(" + table + "." + Location + ") as " + X(table); }
            public static string X(string table) { return table + "_x"; }
            public static string StY(string table) { return "st_y(" + table + "." + Location + ") as " + Y(table); }
            public static string Y(string table) { return table + "_y"; }
            public static string StSRID(string table) { return "st_srid(" + table + "." + Location + ") as " + SRID(table); }
            public static string SRID(string table) { return table + "_srid"; }

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select(string table) { return Reflector.GetSelectColumns(table, new string[] { StX(table), StY(table), StSRID(table) }, typeof(Columns)); }
        }

        public static string GetTableName(int predictionId)
        {
            return "point_" + predictionId;
        }

        internal static string CreateTable(int predictionId, int srid)
        {
            string table = GetTableName(predictionId);

            DB.Connection.ExecuteNonQuery(
                "CREATE TABLE " + table + " (" +
                Columns.Core + " INTEGER," +
                Columns.Id + " SERIAL PRIMARY KEY," +
                Columns.IncidentType + " VARCHAR," +
                Columns.Location + " GEOMETRY(GEOMETRY," + srid + ")," +
                Columns.Time + " TIMESTAMP);" +
                "CREATE INDEX ON " + table + " (" + Columns.Core + ");" +
                "CREATE INDEX ON " + table + " (" + Columns.IncidentType + ");" +
                "CREATE INDEX ON " + table + " USING GIST (" + Columns.Location + ");");

            return table;
        }

        internal static void DeleteTable(int predictionId)
        {
            DB.Connection.ExecuteNonQuery("DROP TABLE " + GetTableName(predictionId) + " CASCADE");
        }

        public static void VacuumTable(int predictionId)
        {
            DB.Connection.ExecuteNonQuery("VACUUM ANALYZE " + GetTableName(predictionId));
        }

        internal static List<int> Insert(NpgsqlConnection connection,
                                         IEnumerable<Tuple<PostGIS.Point, string, DateTime>> points,
                                         int predictionId,
                                         Area area,
                                         bool onlyInsertPointsInArea,
                                         bool vacuum)
        {
            NpgsqlCommand cmd = new NpgsqlCommand(null, connection);
            cmd.CommandTimeout = Configuration.PostgresCommandTimeout;

            string insertIntoTable = GetTableName(predictionId);

            if (onlyInsertPointsInArea)
            {
                cmd.CommandText = "CREATE TABLE temp AS SELECT * FROM " + insertIntoTable + " WHERE FALSE;" +
                                  "ALTER TABLE temp ALTER COLUMN " + Columns.Id + " SET DEFAULT nextval('" + insertIntoTable + "_" + Columns.Id + "_seq');";
                cmd.ExecuteNonQuery();

                insertIntoTable = "temp";
            }

            List<int> ids = new List<int>();
            StringBuilder pointValues = new StringBuilder();
            int pointNum = 0;
            int pointsPerBatch = 1000;
            foreach (Tuple<PostGIS.Point, string, DateTime> pointIncidentTime in points)
            {
                PostGIS.Point point = pointIncidentTime.Item1;
                string incidentType = pointIncidentTime.Item2;
                DateTime time = pointIncidentTime.Item3;

                if (point.SRID != area.SRID)
                    throw new Exception("Area SRID (" + area.SRID + ") does not match point SRID (" + point.SRID);

                pointValues.Append((pointValues.Length > 0 ? "," : "") + "(" + (pointNum % Configuration.ProcessorCount) + ",DEFAULT,'" + incidentType + "',st_geometryfromtext('POINT(" + point.X + " " + point.Y + ")'," + point.SRID + "),@time_" + pointNum + ")");
                ConnectionPool.AddParameters(cmd, new Parameter("time_" + pointNum, NpgsqlDbType.Timestamp, time));

                if ((++pointNum % pointsPerBatch) == 0)
                {
                    cmd.CommandText = "INSERT INTO " + insertIntoTable + " (" + Columns.Insert + ") VALUES " + pointValues + " RETURNING " + Columns.Id;

                    if (onlyInsertPointsInArea)
                        cmd.ExecuteNonQuery();
                    else
                    {
                        NpgsqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                            ids.Add(Convert.ToInt32(reader[0]));

                        reader.Close();
                    }

                    pointValues.Clear();
                    cmd.Parameters.Clear();
                }
            }

            if (pointValues.Length > 0)
            {
                cmd.CommandText = "INSERT INTO " + insertIntoTable + " (" + Columns.Insert + ") VALUES " + pointValues + " RETURNING " + Columns.Id;

                if (onlyInsertPointsInArea)
                    cmd.ExecuteNonQuery();
                else
                {
                    NpgsqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                        ids.Add(Convert.ToInt32(reader[0]));

                    reader.Close();
                }

                cmd.Parameters.Clear();
            }

            if (onlyInsertPointsInArea)
            {
                string areaGeometryTable = AreaGeometry.GetTableName(area.SRID);
                string areaBoundingBoxesTable = AreaBoundingBoxes.GetTableName(area.SRID);

                cmd.CommandText = "CREATE INDEX ON temp USING GIST (" + Columns.Location + ")";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "INSERT INTO " + GetTableName(predictionId) + " (" + Columns.Insert + ") " +
                                  "SELECT * " +
                                  "FROM temp " +
                                  "WHERE EXISTS (SELECT 1 " +
                                                "FROM " + areaGeometryTable + "," + areaBoundingBoxesTable + " " +
                                                "WHERE " + areaGeometryTable + "." + AreaGeometry.Columns.AreaId + "=" + area.Id + " AND " +
                                                           areaBoundingBoxesTable + "." + AreaBoundingBoxes.Columns.AreaId + "=" + area.Id + " AND " +
                                                           "(" +
                                                             "(" +
                                                                areaBoundingBoxesTable + "." + AreaBoundingBoxes.Columns.Relationship + "='" + AreaBoundingBoxes.Relationship.Within + "' AND " +
                                                                "st_intersects(temp." + Columns.Location + "," + areaBoundingBoxesTable + "." + AreaBoundingBoxes.Columns.BoundingBox + ")" +
                                                             ") " +
                                                             "OR " +
                                                             "(" +
                                                                areaBoundingBoxesTable + "." + AreaBoundingBoxes.Columns.Relationship + "='" + AreaBoundingBoxes.Relationship.Overlaps + "' AND " +
                                                                "st_intersects(temp." + Columns.Location + "," + areaBoundingBoxesTable + "." + AreaBoundingBoxes.Columns.BoundingBox + ") AND " +
                                                                "st_intersects(temp." + Columns.Location + "," + areaGeometryTable + "." + AreaGeometry.Columns.Geometry + ")" +
                                                             ")" +
                                                           ")" +
                                                ") " +
                                  "RETURNING " + Columns.Id + ";" +
                                  "DROP TABLE temp;";

                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    ids.Add(Convert.ToInt32(reader[0]));

                reader.Close();
            }

            if (vacuum)
                VacuumTable(predictionId);

            return ids;
        }

        private int _id;
        private string _incidentType;
        private PostGIS.Point _location;
        private DateTime _time;

        public int Id
        {
            get { return _id; }
        }

        public string IncidentType
        {
            get { return _incidentType; }
        }

        public PostGIS.Point Location
        {
            get { return _location; }
        }

        public DateTime Time
        {
            get { return _time; }
            set { _time = value; }
        }

        internal Point(NpgsqlDataReader reader, string table)
        {
            Construct(reader, table);
        }

        internal Point(int id, string incidentType, PostGIS.Point location, DateTime time)
        {
            Construct(id, incidentType, location, time);
        }

        private void Construct(NpgsqlDataReader reader, string table)
        {
            Construct(Convert.ToInt32(reader[table + "_" + Columns.Id]),
                      Convert.ToString(reader[table + "_" + Columns.IncidentType]),
                      new PostGIS.Point(Convert.ToDouble(reader[Columns.X(table)]), Convert.ToDouble(reader[Columns.Y(table)]), Convert.ToInt32(reader[Columns.SRID(table)])),
                      Convert.ToDateTime(reader[table + "_" + Columns.Time]));
        }

        private void Construct(int id, string incidentType, PostGIS.Point location, DateTime time)
        {
            _id = id;
            _incidentType = incidentType;
            _location = location;
            _time = time;
        }
    }
}
