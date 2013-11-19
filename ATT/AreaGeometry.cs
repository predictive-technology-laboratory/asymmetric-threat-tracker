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
using LAIR.ResourceAPIs.PostGIS;
using Npgsql;
using PTL.ATT.ShapeFiles;

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

        internal static void Create(ShapeFile shapefile, int areaId)
        {
            DB.Connection.ExecuteNonQuery(
                "INSERT INTO " + CreateTable(shapefile.SRID) + " (" + Columns.Insert + ") " +
                "SELECT " + areaId + "," + ShapeFileGeometry.Columns.Geometry + " " +
                "FROM " + ShapeFileGeometry.GetTableName(shapefile.SRID));
        }
    }
}
