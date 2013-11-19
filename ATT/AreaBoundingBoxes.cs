#region copyright
// Copyright 2013 Matthew S. Gerber (gerber.matthew@gmail.com)
// 
// This file is part of the Asymmetric Threat Tracker (ATT).
// 
// The ATT is free software: you can redistribute it and/or modify
// it under the terms of the GNU General internal License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// The ATT is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General internal License for more details.
// 
// You should have received a copy of the GNU General internal License
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
            internal const string AreaId = "area_id";
            [Reflector.Insert]
            internal const string BoundingBox = "bounding_box";
            internal const string Id = "id";
            internal const string Relationship = "relationship";

            internal static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
        }

        internal static string GetTableName(int srid)
        {
            return "area_bounding_boxes_" + srid;
        }

        private static string CreateTable(int srid)
        {
            string tableName = GetTableName(srid);

            if (!DB.Connection.TableExists(tableName))
                DB.Connection.ExecuteNonQuery(
                    "CREATE TABLE " + tableName + " (" +
                    Columns.AreaId + " INTEGER REFERENCES " + Area.Table + " ON DELETE CASCADE," + 
                    Columns.BoundingBox + " GEOMETRY(POLYGON," + srid + ")," +
                    Columns.Id + " SERIAL PRIMARY KEY," +
                    Columns.Relationship + " VARCHAR);" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.AreaId + ");" +
                    "CREATE INDEX ON " + tableName + " USING GIST (" + Columns.BoundingBox + ");" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.Relationship + ");");

            return tableName;
        }

        private static IEnumerable<PostGIS.Geometry> GetCandidateBoundingBoxes(double minX, double maxX, double minY, double maxY, double boxSize, int srid)
        {
            for (double x = minX; x <= maxX; x += boxSize)
                for (double y = minY; y <= maxY; y += boxSize)
                    yield return new PostGIS.Polygon(new PostGIS.Point[]{
                        new PostGIS.Point(x, y, srid),
                        new PostGIS.Point(x, y + boxSize, srid),
                        new PostGIS.Point(x + boxSize, y + boxSize, srid),
                        new PostGIS.Point(x + boxSize, y, srid),
                        new PostGIS.Point(x, y, srid)}, srid);
        }

        internal static void Create(int areaId, int srid, double boxSize)
        {
            string tableName = CreateTable(srid);

            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT min(st_xmin(" + ShapeFileGeometry.Columns.Geometry + "))," +
                                                                "max(st_xmax(" + ShapeFileGeometry.Columns.Geometry + "))," +
                                                                "min(st_ymin(" + ShapeFileGeometry.Columns.Geometry + "))," +
                                                                "max(st_ymax(" + ShapeFileGeometry.Columns.Geometry + ")) " +
                                                         "FROM " + AreaGeometry.GetTableName(srid) + " " +
                                                         "WHERE " + AreaGeometry.Columns.AreaId + "=" + areaId);

            NpgsqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            double minX = Convert.ToDouble(reader[0]);
            double maxX = Convert.ToDouble(reader[1]);
            double minY = Convert.ToDouble(reader[2]);
            double maxY = Convert.ToDouble(reader[3]);
            reader.Close();

            StringBuilder cmdText = new StringBuilder();
            int batchNum = 0;
            foreach (PostGIS.Geometry geometry in GetCandidateBoundingBoxes(minX, maxX, minY, maxY, boxSize, srid))
            {
                cmdText.Append((cmdText.Length == 0 ? "INSERT INTO " + tableName + " (" + Columns.Insert + ") VALUES " : ",") + "(" + areaId + "," + geometry.StGeometryFromText + ")");
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

            string areaGeometryTable = AreaGeometry.GetTableName(areaId);

            cmd.CommandText = "UPDATE " + tableName + " " +
                              "SET " + Columns.Relationship + "='" + Relationship.Overlaps + "' " +
                              "WHERE " + Columns.AreaId + "=" + areaId + " AND " + 
                                     "EXISTS(SELECT 1 " +
                                            "FROM " + areaGeometryTable + " " +
                                            "WHERE st_overlaps(" + tableName + "." + Columns.BoundingBox + "," + areaGeometryTable + "." + AreaGeometry.Columns.Geometry + ")" +
                                            ")";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "UPDATE " + tableName + " " +
                              "SET " + Columns.Relationship + "='" + Relationship.Within + "' " +
                              "WHERE " + Columns.AreaId + "=" + areaId + " AND " + 
                                     "EXISTS(SELECT 1 " +
                                            "FROM " + areaGeometryTable + " " +
                                            "WHERE st_overlaps(" + tableName + "." + Columns.BoundingBox + "," + areaGeometryTable + "." + AreaGeometry.Columns.Geometry + ")" +
                                            ")";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "DELETE FROM " + tableName + " WHERE " + Columns.AreaId + "=" + areaId + " AND " + Columns.Relationship + " IS NULL";
            cmd.ExecuteNonQuery();

            DB.Connection.Return(cmd.Connection);
        }
    }
}
