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
using System.IO;
using System.Diagnostics;
using LAIR.ResourceAPIs.PostgreSQL;

using Npgsql;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PTL.ATT.ShapeFiles
{
    public class ShapeFile
    {
        public enum ShapefileType
        {
            Area,
            DistanceFeature,
            RasterFeature
        }

        internal const string Table = "shapefile";

        internal class Columns
        {
            [Reflector.Select(true)]
            internal const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            internal const string Name = "name";
            [Reflector.Insert, Reflector.Select(true)]
            internal const string SRID = "srid";
            [Reflector.Insert, Reflector.Select(true)]
            internal const string Type = "type";

            internal static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            internal static string Select { get { return Reflector.GetSelectColumns(Table, typeof(Columns)); } }
        }

        [ConnectionPool.CreateTable]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Id + " SERIAL PRIMARY KEY," +
                   Columns.Name + " VARCHAR," +
                   Columns.SRID + " INTEGER," +
                   Columns.Type + " VARCHAR);" +
                   (connection.TableExists(Table) ? "" :
                   "CREATE INDEX ON " + Table + " (" + Columns.Type + ");");
        }

        private static int Create(NpgsqlConnection connection, string name, int srid, ShapefileType type)
        {
            return Convert.ToInt32(new NpgsqlCommand("INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES ('" + name + "'," + srid + ",'" + type + "') RETURNING " + Columns.Id, connection).ExecuteScalar());
        }

        public static void ImportShapeFiles(string[] shapefilePaths, ShapefileType type)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand(null);

            try
            {
                cmd.CommandText = "BEGIN";
                cmd.ExecuteNonQuery();

                Regex reprojectionRE = new Regex("(?<from>[0-9]+):(?<to>[0-9]+)");

                foreach (string shapefilePath in shapefilePaths)
                {
                    string shapefileName = Path.GetFileNameWithoutExtension(shapefilePath);

                    string reprojectionPath = Path.Combine(Path.GetDirectoryName(shapefilePath), shapefileName + ".srid");
                    if (!File.Exists(reprojectionPath))
                        throw new Exception("Could not find SRID file at \"" + reprojectionPath + "\"");

                    string reprojection = File.ReadAllText(reprojectionPath);
                    Match reprojectionMatch = reprojectionRE.Match(reprojection);
                    if (!reprojectionMatch.Success)
                        throw new Exception("Invalid shapefile reprojection \"" + reprojection + "\". Must be in 1234:1234 format.");

                    int fromSRID = int.Parse(reprojectionMatch.Groups["from"].Value);
                    int toSRID = int.Parse(reprojectionMatch.Groups["to"].Value);
                    if (fromSRID == toSRID)
                        reprojection = fromSRID.ToString();

                    string sql;
                    string error;
                    using (Process process = new Process())
                    {
                        process.StartInfo.FileName = Configuration.Shp2PgsqlPath;
                        process.StartInfo.Arguments = "-I -g geom -s " + reprojection + " \"" + shapefilePath + "\" temp";
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.Start();

                        Console.Out.WriteLine("Converting shapefile \"" + shapefilePath + "\".");

                        sql = process.StandardOutput.ReadToEnd().Replace("BEGIN;", "").Replace("COMMIT;", "");
                        error = process.StandardError.ReadToEnd().Trim().Replace(Environment.NewLine, "; ").Replace("\n", "; ");

                        process.WaitForExit();
                    }

                    Console.Out.WriteLine(error);

                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    Console.Out.WriteLine("Importing shapefile into database.");
                    int shapefileId = Create(cmd.Connection, shapefileName, toSRID, type);
                    ShapeFileGeometry.Create(cmd.Connection, shapefileId, toSRID, "temp", "geom");

                    cmd.CommandText = "DROP TABLE temp";
                    cmd.ExecuteNonQuery();
                }

                cmd.CommandText = "COMMIT";
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to import shape file(s):  " + ex.Message);
            }
            finally
            {
                DB.Connection.Return(cmd.Connection);
            }
        }

        public static IEnumerable<ShapeFile> GetAvailable(int srid)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Table + " WHERE " + Columns.SRID + "=" + srid);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                yield return new ShapeFile(reader);

            reader.Close();
            DB.Connection.Return(cmd.Connection);
        }

        private int _id;
        private string _name;
        private int _srid;
        private ShapefileType _type;

        #region properties
        public int Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public int SRID
        {
            get { return _srid; }
        }

        public ShapefileType Type
        {
            get { return _type; }
        }
        #endregion

        private ShapeFile(NpgsqlDataReader reader)
        {
            _id = Convert.ToInt32(reader[Table + "_" + Columns.Id]);
            _name = Convert.ToString(reader[Table + "_" + Columns.Name]);
            _srid = Convert.ToInt32(reader[Table + "_" + Columns.SRID]);
            _type = (ShapefileType)Enum.Parse(typeof(ShapefileType), Convert.ToString(reader[Table + "_" + Columns.Type]));
        }

        public virtual string Details()
        {
            return "ID:  " + _id + Environment.NewLine +
                   "Name:  " + _name + Environment.NewLine +
                   "SRID:  " + _srid + Environment.NewLine +
                   "Type:  " + _type;
        }

        public override string ToString()
        {
            return _name;
        }

        public void Delete()
        {
            DB.Connection.ExecuteNonQuery("DELETE FROM " + Table + " WHERE " + Columns.Id + "=" + _id);
        }
    }
}
