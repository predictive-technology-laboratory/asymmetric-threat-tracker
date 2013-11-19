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
using LAIR.Collections.Generic;

namespace PTL.ATT.ShapeFiles
{
    public class ShapeFileGeometry
    {
        public class Columns
        {
            [Reflector.Insert, Reflector.Select(true)]
            public const string Geometry = "geom";
            [Reflector.Select(true)]
            public const string Id = "id";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
        }

        internal static string GetTableName(int shapefileId)
        {
            return "shapefile_geometry_" + shapefileId;
        }        

        internal static List<int> Create(NpgsqlConnection connection, int shapefileId, string geometryTable, string geometryColumn, int srid)
        {
            string tableName = GetTableName(shapefileId);

            DB.Connection.ExecuteNonQuery(
                "CREATE TABLE " + tableName + " (" +
                Columns.Geometry + " GEOMETRY(GEOMETRY," + srid + ")," +
                Columns.Id + " SERIAL PRIMARY KEY," +
                "CREATE INDEX ON " + tableName + " USING GIST (" + Columns.Geometry + ");");

            List<int> ids = new List<int>();
            NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO " + tableName + " (" + Columns.Insert + ") " +
                                                  "SELECT " + geometryColumn + " " +
                                                  "FROM " + geometryTable + " RETURNING " + Columns.Id, connection);

            cmd.CommandTimeout = Configuration.PostgresCommandTimeout;
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                ids.Add(Convert.ToInt32(reader[Columns.Id]));

            reader.Close();

            return ids;
        }
    }
}
