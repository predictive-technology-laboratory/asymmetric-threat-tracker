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
using LAIR.ResourceAPIs.PostGIS;

namespace PTL.ATT
{
    public class ShapefileGeometry
    {
        public class Columns
        {
            [Reflector.Insert]
            public const string Geometry = "geom";
            public const string Id = "id";
            [Reflector.Insert]
            public const string ShapefileId = "shapefile_id";

            internal static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
        }

        public static string GetTableName(int srid)
        {
            return "shapefile_geometry_" + srid;
        }

        internal static List<int> Create(NpgsqlConnection connection, int shapefileId, int srid, string geometryTable, string geometryColumn)
        {
            string tableName = GetTableName(srid);

            if (!DB.Connection.TableExists(tableName))
                DB.Connection.ExecuteNonQuery(
                    "CREATE TABLE " + tableName + " (" +
                    Columns.Geometry + " GEOMETRY(GEOMETRY," + srid + ")," +
                    Columns.Id + " SERIAL PRIMARY KEY," +
                    Columns.ShapefileId + " INTEGER REFERENCES " + Shapefile.Table + " ON DELETE CASCADE);" +
                    "CREATE INDEX ON " + tableName + " USING GIST (" + Columns.Geometry + ");" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.ShapefileId + ");");

            List<int> ids = new List<int>();
            NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO " + tableName + " (" + Columns.Insert + ") " +
                                                  "SELECT " + geometryColumn + "," + shapefileId + " " +
                                                  "FROM " + geometryTable + " RETURNING " + Columns.Id, connection);

            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                ids.Add(Convert.ToInt32(reader[0]));

            reader.Close();

            return ids;
        }

        public static List<int> Create(NpgsqlConnection connection, int shapefileId, List<Geometry> geometries)
        {
            if (geometries.Count == 0)
                return new List<int>();

            int srid = geometries[0].SRID;
            string tableName = GetTableName(srid);

            if (!DB.Connection.TableExists(tableName))
                DB.Connection.ExecuteNonQuery(
                    "CREATE TABLE " + tableName + " (" +
                    Columns.Geometry + " GEOMETRY(GEOMETRY," + srid + ")," +
                    Columns.Id + " SERIAL PRIMARY KEY," +
                    Columns.ShapefileId + " INTEGER REFERENCES " + Shapefile.Table + " ON DELETE CASCADE);" +
                    "CREATE INDEX ON " + tableName + " USING GIST (" + Columns.Geometry + ");" +
                    "CREATE INDEX ON " + tableName + " (" + Columns.ShapefileId + ");");

            StringBuilder cmdTxt = new StringBuilder();
            List<int> ids = new List<int>();
            int numPerBatch = 1000;
            int num = 0;
            foreach (Geometry geometry in geometries)
            {
                cmdTxt.Append((cmdTxt.Length == 0 ? "INSERT INTO " + tableName + " (" + Columns.Insert + ") VALUES " : ",") + "(" + geometry.StGeometryFromText + "," + shapefileId + ")");

                if (++num == numPerBatch)
                {
                    NpgsqlCommand cmd = new NpgsqlCommand(cmdTxt.ToString() + " RETURNING " + Columns.Id, connection);
                    NpgsqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                        ids.Add(Convert.ToInt32(reader[Columns.Id]));

                    reader.Close();
                    cmdTxt.Clear();
                    num = 0;
                }
            }

            if (num > 0)
            {
                NpgsqlCommand cmd = new NpgsqlCommand(cmdTxt.ToString() + " RETURNING " + Columns.Id, connection);
                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    ids.Add(Convert.ToInt32(reader[Columns.Id]));

                reader.Close();
                cmdTxt.Clear();
                num = 0;
            }

            return ids;
        }
    }
}
