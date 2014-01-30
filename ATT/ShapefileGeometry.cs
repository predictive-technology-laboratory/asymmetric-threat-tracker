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
            public const string Time = "time";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
        }

        public static string GetTableName(Shapefile shapefile)
        {
            return "shapefile_geometry_" + shapefile.Id;
        }

        internal static void Create(NpgsqlConnection connection, Shapefile shapefile, string geometryTable, string geometryColumn)
        {
            string tableName = GetTableName(shapefile);

            NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO " + tableName + " (" + Columns.Insert + ") " +
                                                  "SELECT " + geometryColumn + ",'-infinity' " +
                                                  "FROM " + geometryTable, connection);

            cmd.ExecuteNonQuery();
        }

        public static void Create(NpgsqlConnection connection, Shapefile shapefile, List<Tuple<Geometry, DateTime>> geometryTimes)
        {
            if (geometryTimes.Count == 0)
                return;

            string tableName = GetTableName(shapefile);

            int numPerBatch = 1000;
            int num = 0;
            StringBuilder cmdTxt = new StringBuilder();
            List<Parameter> cmdParams = new List<Parameter>(numPerBatch);
            foreach (Tuple<Geometry, DateTime> geometryTime in geometryTimes)
            {
                string timeParamName = "time_" + num;
                cmdTxt.Append((cmdTxt.Length == 0 ? "INSERT INTO " + tableName + " (" + Columns.Insert + ") VALUES " : ",") + "(" + GetValue(geometryTime.Item1, shapefile.SRID, timeParamName) + ")");
                cmdParams.Add(new Parameter(timeParamName, NpgsqlTypes.NpgsqlDbType.Timestamp, geometryTime.Item2));

                if (++num == numPerBatch)
                {
                    NpgsqlCommand cmd = new NpgsqlCommand(cmdTxt.ToString(), connection);
                    ConnectionPool.AddParameters(cmd, cmdParams);
                    cmd.ExecuteNonQuery();
                    cmdTxt.Clear();
                    cmdParams.Clear();
                    num = 0;
                }
            }

            if (num > 0)
            {
                NpgsqlCommand cmd = new NpgsqlCommand(cmdTxt.ToString(), connection);
                ConnectionPool.AddParameters(cmd, cmdParams);
                cmd.ExecuteNonQuery();
                cmdTxt.Clear();
                cmdParams.Clear();
                num = 0;
            }
        }

        internal static void CreateTable(Shapefile shapefile)
        {
            string tableName = GetTableName(shapefile);
            DB.Connection.ExecuteNonQuery(
                "CREATE TABLE " + tableName + " (" +
                Columns.Geometry + " GEOMETRY(GEOMETRY," + shapefile.SRID + ")," +
                Columns.Id + " SERIAL PRIMARY KEY," +
                Columns.Time + " TIMESTAMP);" +
                "CREATE INDEX ON " + tableName + " USING GIST (" + Columns.Geometry + ");" +
                "CREATE INDEX ON " + tableName + " (" + Columns.Time + ");");
        }

        public static string GetValue(Geometry geometry, int targetSRID, string timeParamName)
        {
            return (geometry.SRID == targetSRID ? geometry.StGeometryFromText : "st_transform(" + geometry.StGeometryFromText + "," + targetSRID + ")") + ",@" + timeParamName;
        }
    }
}
