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
using LAIR.ResourceAPIs.PostGIS;
using Npgsql;

namespace PTL.ATT
{
    public class AreaGeometry
    {
        /// <summary>
        /// Table definition.
        /// </summary>
        public class Columns
        {
            [Reflector.Insert]
            public const string AreaId = "area_id";
            [Reflector.Insert]
            public const string Geometry = "geom";
            public const string Id = "id";

            internal static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
        }

        public static string GetTableName(int srid)
        {
            return "area_geometry_" + srid;
        }

        private static string CreateTable(int srid)
        {
            string tableName = GetTableName(srid);

            if (!DB.Connection.TableExists(tableName))
                DB.Connection.ExecuteNonQuery(
                    "CREATE TABLE " + tableName + " (" +
                    Columns.AreaId + " INTEGER REFERENCES " + Area.Table + " ON DELETE CASCADE," +
                    Columns.Geometry + " GEOMETRY(GEOMETRY," + srid + ")," +
                    Columns.Id + " SERIAL PRIMARY KEY);" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.AreaId + ");" +
                    "CREATE INDEX ON " + tableName + " USING GIST (" + Columns.Geometry + ");");

            return tableName;
        }

        internal static int Create(Geometry geometry, int areaId)
        {
            return Convert.ToInt32(DB.Connection.ExecuteScalar("INSERT INTO " + CreateTable(geometry.SRID) + " (" + AreaGeometry.Columns.Insert + ") VALUES (" + areaId + "," + geometry.StGeometryFromText + ") RETURNING " + Columns.Id));
        }

        internal static void Create(Shapefile shapefile, int areaId)
        {
            DB.Connection.ExecuteNonQuery(
                "INSERT INTO " + CreateTable(shapefile.SRID) + " (" + Columns.Insert + ") " +
                "SELECT " + areaId + "," + ShapefileGeometry.Columns.Geometry + " " +
                "FROM " + ShapefileGeometry.GetTableName(shapefile));
        }
    }
}
