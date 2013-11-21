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
using LAIR.ResourceAPIs.PostgreSQL;
using Npgsql;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using PTL.ATT.ShapeFiles;

namespace PTL.ATT
{
    internal class AreaBoundingBoxes
    {
        public enum Relationship
        {
            Overlaps,
            Within
        }
            
        public const string Table = "area_bounding_boxes";

        public class Columns
        {
            [Reflector.Insert, Reflector.Select(true)]
            public const string AreaId = "area_id";
            [Reflector.Insert]
            public const string BoundingBox = "bounding_box";
            public const string Id = "id";
            public const string Relationship = "relationship";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return Reflector.GetSelectColumns(Table, typeof(Columns)); } }
        }

        [ConnectionPool.CreateTable(typeof(Area))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                Columns.AreaId + " INT REFERENCES " + Area.Table + " ON DELETE CASCADE," +
                Columns.BoundingBox + " GEOMETRY(POLYGON," + Configuration.PostgisSRID + ")," +
                Columns.Id + " SERIAL PRIMARY KEY," +
                Columns.Relationship + " VARCHAR);" +
                (connection.TableExists(Table) ? "" :
                "CREATE INDEX ON " + Table + " (" + Columns.AreaId + ");" +
                "CREATE INDEX ON " + Table + " USING GIST (" + Columns.BoundingBox + ");" +
                "CREATE INDEX ON " + Table + " (" + Columns.Relationship + ");");
        }

        private static IEnumerable<PostGIS.Geometry> GetCandidateBoundingBoxes(double minX, double maxX, double minY, double maxY, double boxSize)
        {
            for (double x = minX; x <= maxX; x += boxSize)
                for (double y = minY; y <= maxY; y += boxSize)
                    yield return new PostGIS.Polygon(new PostGIS.Point[]{
                        new PostGIS.Point(x, y, Configuration.PostgisSRID),
                        new PostGIS.Point(x, y + boxSize, Configuration.PostgisSRID),
                        new PostGIS.Point(x + boxSize, y + boxSize, Configuration.PostgisSRID),
                        new PostGIS.Point(x + boxSize, y, Configuration.PostgisSRID),
                        new PostGIS.Point(x, y, Configuration.PostgisSRID)}, Configuration.PostgisSRID);
        }

        internal static void Create(NpgsqlConnection connection, int areaId, ShapeFile shapeFile, double boxSize)
        {
            NpgsqlCommand cmd = new NpgsqlCommand("SELECT min(st_xmin(" + ShapeFileGeometry.Columns.Geometry + "))," +
                                                         "max(st_xmax(" + ShapeFileGeometry.Columns.Geometry + "))," +
                                                         "min(st_ymin(" + ShapeFileGeometry.Columns.Geometry + "))," +
                                                         "max(st_ymax(" + ShapeFileGeometry.Columns.Geometry + ")) " +
                                                  "FROM " + ShapeFileGeometry.Columns.JoinShapeFile + " " +
                                                  "WHERE " + ShapeFile.Table + "." + ShapeFile.Columns.Id + "=" + shapeFile.Id + " " +
                                                  "GROUP BY " + ShapeFile.Table + "." + ShapeFile.Columns.Id, connection);
            cmd.CommandTimeout = Configuration.PostgresCommandTimeout;

            NpgsqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            double minX = Convert.ToDouble(reader[0]);
            double maxX = Convert.ToDouble(reader[1]);
            double minY = Convert.ToDouble(reader[2]);
            double maxY = Convert.ToDouble(reader[3]);
            reader.Close();

            StringBuilder cmdText = new StringBuilder();
            int batchNum = 0;
            foreach (PostGIS.Geometry geometry in GetCandidateBoundingBoxes(minX, maxX, minY, maxY, boxSize))
            {
                cmdText.Append((cmdText.Length == 0 ? "INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES " : ",") + "(" + areaId + "," + geometry.StGeometryFromText + ")");
                if (++batchNum >= 1000)
                {
                    cmd.CommandText = cmdText.ToString();
                    cmd.ExecuteNonQuery();
                    cmdText.Clear();
                    batchNum = 0;
                }
            }

            if (batchNum > 0)
            {
                cmd.CommandText = cmdText.ToString();
                cmd.ExecuteNonQuery();
                cmdText.Clear();
                batchNum = 0;
            }

            cmd.CommandText = "UPDATE " + Table + " " +
                              "SET " + Columns.Relationship + "='" + Relationship.Overlaps + "' " +
                              "FROM " + AreaGeometry.Table + " " +
                              "WHERE " + AreaGeometry.Table + "." + AreaGeometry.Columns.AreaId + "=" + areaId + " AND " +
                                        "st_overlaps(" + Table + "." + Columns.BoundingBox + "," + AreaGeometry.Table + "." + AreaGeometry.Columns.Geometry + ")";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "UPDATE " + Table + " " +
                              "SET " + Columns.Relationship + "='" + Relationship.Within + "' " +
                              "FROM " + AreaGeometry.Table + " " +
                              "WHERE " + AreaGeometry.Table + "." + AreaGeometry.Columns.AreaId + "=" + areaId + " AND " +
                                        "st_within(" + Table + "." + Columns.BoundingBox + "," + AreaGeometry.Table + "." + AreaGeometry.Columns.Geometry + ")";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "DELETE FROM " + Table + " WHERE " + Columns.Relationship + " IS NULL";
            cmd.ExecuteNonQuery();
        }
    }
}
