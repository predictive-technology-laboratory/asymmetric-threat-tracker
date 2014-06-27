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
using LAIR.ResourceAPIs.PostgreSQL;
using Npgsql;
using PostGIS = LAIR.ResourceAPIs.PostGIS;

namespace PTL.ATT
{
    internal class AreaBoundingBoxes
    {
        internal enum Relationship
        {
            /// <summary>
            /// Bounding box overlaps the area border
            /// </summary>
            Overlaps,

            /// <summary>
            /// Bounding box is entirely within the area border
            /// </summary>
            Within
        }

        internal class Columns
        {
            [Reflector.Insert]
            internal const string BoundingBox = "bounding_box";
            internal const string Id = "id";
            internal const string Relationship = "relationship";

            internal static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
        }

        internal static string GetTableName(Area area)
        {
            return "area_bounding_boxes_" + area.Id;
        }

        private static string CreateTable(Area area)
        {
            string tableName = GetTableName(area);

            if (!DB.Connection.TableExists(tableName))
                DB.Connection.ExecuteNonQuery(
                    "CREATE TABLE " + tableName + " (" +
                    Columns.BoundingBox + " GEOMETRY(POLYGON," + area.Shapefile.SRID + ")," +
                    Columns.Id + " SERIAL PRIMARY KEY," +
                    Columns.Relationship + " VARCHAR);" +
                    "CREATE INDEX ON " + tableName + " USING GIST (" + Columns.BoundingBox + ");" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.Relationship + ");");

            return tableName;
        }

        internal static void Create(Area area, int pointContainmentBoundingBoxSize)
        {
            string tableName = CreateTable(area);

            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT min(st_xmin(" + ShapefileGeometry.Columns.Geometry + "))," +
                                                                "max(st_xmax(" + ShapefileGeometry.Columns.Geometry + "))," +
                                                                "min(st_ymin(" + ShapefileGeometry.Columns.Geometry + "))," +
                                                                "max(st_ymax(" + ShapefileGeometry.Columns.Geometry + ")) " +
                                                         "FROM " + area.Shapefile.GeometryTable + " ");

            NpgsqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            double minX = Convert.ToDouble(reader[0]);
            double maxX = Convert.ToDouble(reader[1]);
            double minY = Convert.ToDouble(reader[2]);
            double maxY = Convert.ToDouble(reader[3]);
            reader.Close();

            List<PostGIS.Polygon> pointContainmentBoundingBoxes = new List<PostGIS.Polygon>();
            for (double x = minX; x <= maxX; x += pointContainmentBoundingBoxSize)
                for (double y = minY; y <= maxY; y += pointContainmentBoundingBoxSize)
                    pointContainmentBoundingBoxes.Add(new PostGIS.Polygon(new PostGIS.Point[]{
                        new PostGIS.Point(x, y, area.Shapefile.SRID),
                        new PostGIS.Point(x, y + pointContainmentBoundingBoxSize, area.Shapefile.SRID),
                        new PostGIS.Point(x + pointContainmentBoundingBoxSize, y + pointContainmentBoundingBoxSize, area.Shapefile.SRID),
                        new PostGIS.Point(x + pointContainmentBoundingBoxSize, y, area.Shapefile.SRID),
                        new PostGIS.Point(x, y, area.Shapefile.SRID)}, area.Shapefile.SRID));

            StringBuilder cmdText = new StringBuilder();
            int batchNum = 0;
            foreach (PostGIS.Polygon boundingBox in pointContainmentBoundingBoxes)
            {
                cmdText.Append((cmdText.Length == 0 ? "INSERT INTO " + tableName + " (" + Columns.Insert + ") VALUES " : ",") + "(" + boundingBox.StGeometryFromText + ")");
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

            cmd.CommandText = "UPDATE " + tableName + " " +
                              "SET " + Columns.Relationship + "='" + Relationship.Overlaps + "' " +
                              "WHERE EXISTS(" + 
                                             "SELECT 1 " +
                                             "FROM " + area.Shapefile.GeometryTable + " " +
                                             "WHERE st_overlaps(" + tableName + "." + Columns.BoundingBox + "," + area.Shapefile.GeometryTable + "." + ShapefileGeometry.Columns.Geometry + ")" +
                                           ")";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "UPDATE " + tableName + " " +
                              "SET " + Columns.Relationship + "='" + Relationship.Within + "' " +
                              "WHERE EXISTS(" + 
                                             "SELECT 1 " +
                                             "FROM " + area.Shapefile.GeometryTable + " " +
                                             "WHERE st_within(" + tableName + "." + Columns.BoundingBox + "," + area.Shapefile.GeometryTable + "." + ShapefileGeometry.Columns.Geometry + ")" +
                                           ")";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "DELETE FROM " + tableName + " WHERE " + Columns.Relationship + " IS NULL";
            cmd.ExecuteNonQuery();

            DB.Connection.Return(cmd.Connection);
        }
    }
}
