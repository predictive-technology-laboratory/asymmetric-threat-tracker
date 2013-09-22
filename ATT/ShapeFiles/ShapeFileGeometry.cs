#region copyright
// Copyright 2013 
// Predictive Technology Laboratory 
// predictivetech@virginia.edu
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

namespace PTL.ATT.ShapeFiles
{
    public class ShapeFileGeometry
    {
        public const string Table = "shape_file_geometry";

        public class Columns
        {
            [Reflector.Insert, Reflector.Select(true)]
            public const string Geometry = "geom";
            [Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string ShapeFileId = "shape_file_id";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string JoinShapeFile { get { return ShapeFile.Table + " JOIN " + Table + " ON " + ShapeFile.Table + "." + ShapeFile.Columns.Id + "=" + Table + "." + Columns.ShapeFileId; } }
        }

        [ConnectionPool.CreateTable(typeof(ShapeFile))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Geometry + " GEOMETRY(GEOMETRY," + Configuration.PostgisSRID + ")," +
                   Columns.Id + " SERIAL PRIMARY KEY," +
                   Columns.ShapeFileId + " INT REFERENCES " + ShapeFile.Table + " ON DELETE CASCADE);" +
                   (connection.TableExists(Table) ? "" :
                   "CREATE INDEX ON " + Table + " USING GIST (" + Columns.Geometry + ");" +
                   "CREATE INDEX ON " + Table + " (" + Columns.ShapeFileId + ");");
        }

        internal static List<int> Create(NpgsqlConnection connection, int shapeFileId, string geometryTable, string geometryColumn)
        {
            List<int> ids = new List<int>();
            NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO " + Table + " (" + Columns.Insert + ") " + 
                                                  "SELECT " + geometryColumn + "," + shapeFileId + " " + 
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
