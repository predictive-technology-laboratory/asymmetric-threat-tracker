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

        public static void Create(Shapefile shapefile, List<Tuple<Geometry, DateTime>> geometryTimes)
        {
            if (geometryTimes.Count == 0)
                return;

            DB.Connection.ExecuteNonQuery(
                "CREATE TABLE " + shapefile.GeometryTable + " (" +
                Columns.Geometry + " GEOMETRY(GEOMETRY," + shapefile.SRID + ")," +
                Columns.Id + " SERIAL PRIMARY KEY," +
                Columns.Time + " TIMESTAMP);" +
                "CREATE INDEX ON " + shapefile.GeometryTable + " USING GIST (" + Columns.Geometry + ");" +
                "CREATE INDEX ON " + shapefile.GeometryTable + " (" + Columns.Time + ");");

            int numPerBatch = 1000;
            int num = 0;
            StringBuilder cmdTxt = new StringBuilder();
            List<Parameter> cmdParams = new List<Parameter>(numPerBatch);
            NpgsqlCommand cmd = DB.Connection.NewCommand(null);
            foreach (Tuple<Geometry, DateTime> geometryTime in geometryTimes)
            {
                string timeParamName = "time_" + num;
                cmdTxt.Append((cmdTxt.Length == 0 ? "INSERT INTO " + shapefile.GeometryTable + " (" + Columns.Insert + ") VALUES " : ",") + "(" + GetValue(geometryTime.Item1, shapefile.SRID, timeParamName) + ")");
                cmdParams.Add(new Parameter(timeParamName, NpgsqlTypes.NpgsqlDbType.Timestamp, geometryTime.Item2));

                if (++num == numPerBatch)
                {
                    cmd.CommandText = cmdTxt.ToString();
                    ConnectionPool.AddParameters(cmd, cmdParams);
                    cmd.ExecuteNonQuery();
                    cmdTxt.Clear();
                    cmdParams.Clear();
                    cmd.CommandText = null;
                    num = 0;
                }
            }

            if (num > 0)
            {
                cmd.CommandText = cmdTxt.ToString();
                ConnectionPool.AddParameters(cmd, cmdParams);
                cmd.ExecuteNonQuery();
                cmdTxt.Clear();
                cmdParams.Clear();
                cmd.CommandText = null;
                num = 0;
            }
        }

        public static string GetValue(Geometry geometry, int targetSRID, string timeParamName)
        {
            return (geometry.SRID == targetSRID ? geometry.StGeometryFromText : "st_transform(" + geometry.StGeometryFromText + "," + targetSRID + ")") + ",@" + timeParamName;
        }
    }
}
