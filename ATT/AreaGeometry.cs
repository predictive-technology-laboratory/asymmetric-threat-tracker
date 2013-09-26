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
using LAIR.ResourceAPIs.PostGIS;
using Npgsql;
using PTL.ATT.ShapeFiles;

namespace PTL.ATT
{
    public class AreaGeometry
    {
        public const string Table = "area_geometry";

        public class Columns
        {
            [Reflector.Insert, Reflector.Select(true)]
            public const string AreaId = "area_id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Geometry = "geom";
            [Reflector.Select(true)]
            public const string Id = "id";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return Reflector.GetSelectColumns(Table, typeof(Columns)); } }
        }

        [ConnectionPool.CreateTable(typeof(Area))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                Columns.AreaId + " INT REFERENCES " + Area.Table + " ON DELETE CASCADE," +
                Columns.Geometry + " GEOMETRY(GEOMETRY," + Configuration.PostgisSRID + ")," +
                Columns.Id + " SERIAL PRIMARY KEY);" +
                (connection.TableExists(Table) ? "" :
                "CREATE INDEX ON " + Table + " (" + Columns.AreaId + ");" +
                "CREATE INDEX ON " + Table + " USING GIST (" + Columns.Geometry + ");");
        }

        internal static int Create(NpgsqlConnection connection, Geometry geometry, int areaId)
        {
            return Convert.ToInt32(new NpgsqlCommand("INSERT INTO " + AreaGeometry.Table + " (" + AreaGeometry.Columns.Insert + ") VALUES (" + areaId + "," + geometry.StGeometryFromText + ") RETURNING " + Columns.Id, connection).ExecuteScalar());
        }

        internal static void Create(NpgsqlConnection connection, ShapeFile shapeFile, int areaId)
        {
            new NpgsqlCommand("INSERT INTO " + AreaGeometry.Table + " (" + Columns.Insert + ") " +
                              "SELECT " + areaId + "," + ShapeFileGeometry.Columns.Geometry + " " +
                              "FROM " + ShapeFileGeometry.Columns.JoinShapeFile + " " +
                              "WHERE " + ShapeFile.Table + "." + ShapeFile.Columns.Id + "=" + shapeFile.Id + " ", connection).ExecuteNonQuery();
        }
    }
}
