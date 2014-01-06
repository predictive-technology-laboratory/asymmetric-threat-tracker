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

namespace PTL.ATT
{
    public class Shapefile
    {
        /// <summary>
        /// Gets shapefile information
        /// </summary>
        /// <param name="shapefilePath">Path to shapefile being imported</param>
        /// <param name="optionValuesToGet">Options for which a value is needed</param>
        /// <param name="optionValue">Dictionary in which to place retrieved option-value pairs</param>
        public delegate void GetShapefileInfoDelegate(string shapefilePath, List<string> optionValuesToGet, Dictionary<string, string> optionValue);

        public enum ShapefileType
        {
            Area,
            DistanceFeature,
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

        public static int Create(NpgsqlConnection connection, string name, int srid, ShapefileType type)
        {
            return Convert.ToInt32(new NpgsqlCommand("INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES ('" + name + "'," + srid + ",'" + type + "') RETURNING " + Columns.Id, connection).ExecuteScalar());
        }

        public static void ImportShapefile(string shapefilePath, ShapefileType type, GetShapefileInfoDelegate getShapefileInfo)
        {
            ImportShapefiles(new string[] { shapefilePath }, type, getShapefileInfo);
        }

        public static void ImportShapefiles(string[] shapefilePaths, ShapefileType type, GetShapefileInfoDelegate getShapefileInfo)
        {
            Console.Out.WriteLine("Importing " + shapefilePaths.Length + " shapefile(s)");

            NpgsqlCommand cmd = DB.Connection.NewCommand(null);
            int shapefileId = -1;
            try
            {
                Regex reprojectionRE = new Regex("(?<from>[0-9]+):(?<to>[0-9]+)");

                foreach (string shapefilePath in shapefilePaths)
                {
                    Dictionary<string, string> optionValue = new Dictionary<string, string>();

                    string importInfoPath = Path.Combine(Path.GetDirectoryName(shapefilePath), Path.GetFileNameWithoutExtension(shapefilePath) + ".att");
                    if (File.Exists(importInfoPath))
                        foreach (string line in File.ReadLines(importInfoPath))
                        {
                            string[] parts = line.Split('=');
                            optionValue.Add(parts[0].Trim(), parts[1].Trim());
                        }

                    List<string> neededValues = new List<string>();
                    if (!optionValue.ContainsKey("reprojection")) neededValues.Add("reprojection");
                    if (!optionValue.ContainsKey("name")) neededValues.Add("name");
                    if (neededValues.Count > 0)
                        getShapefileInfo(shapefilePath, neededValues, optionValue);

                    string reprojection = optionValue["reprojection"];
                    Match reprojectionMatch = reprojectionRE.Match(reprojection);
                    if (!reprojectionMatch.Success)
                    {
                        Console.Out.WriteLine("Invalid shapefile reprojection \"" + reprojection + "\". Must be in 1234:1234 format. Skipping.");
                        continue;
                    }

                    int fromSRID = int.Parse(reprojectionMatch.Groups["from"].Value);
                    int toSRID = int.Parse(reprojectionMatch.Groups["to"].Value);
                    if (fromSRID == toSRID)
                        reprojection = fromSRID.ToString();

                    string name = optionValue["name"];
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        Console.Out.WriteLine("Empty name given for shapefile \"" + shapefilePath + "\". Skipping.");
                        continue;
                    }

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

                    Console.Out.WriteLine("Importing shapefile into database");
                    shapefileId = Create(cmd.Connection, name, toSRID, type);
                    ShapefileGeometry.Create(cmd.Connection, shapefileId, toSRID, "temp", "geom");

                    cmd.CommandText = "DROP TABLE temp";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    cmd.CommandText = "DROP TABLE temp;";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex2) { Console.Out.WriteLine("Falied to drop table \"temp\":  " + ex2.Message); }

                try
                {
                    cmd.CommandText = "DELETE FROM " + Shapefile.Table + " WHERE " + Shapefile.Columns.Id + "=" + shapefileId;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex2) { Console.Out.WriteLine("Failed to delete shapefile:  " + ex2.Message); }

                throw new Exception("Failed to import shape file(s):  " + ex.Message);
            }
            finally
            {
                DB.Connection.Return(cmd.Connection);
            }

            Console.Out.WriteLine("Shapefile import finished.");
        }

        public static IEnumerable<Shapefile> GetAll()
        {
            List<Shapefile> shapefiles = new List<Shapefile>();
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Table);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                shapefiles.Add(new Shapefile(reader));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return shapefiles;
        }

        public static IEnumerable<Shapefile> GetForSRID(int srid)
        {
            if (srid < 0)
                throw new ArgumentException("Invalid SRID:  " + srid + ". Must be >= 0.");

            List<Shapefile> shapefiles = new List<Shapefile>();
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Table + " WHERE " + Columns.SRID + "=" + srid);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                shapefiles.Add(new Shapefile(reader));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return shapefiles;
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

        public Shapefile(int id)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Table + " WHERE " + Columns.Id + "=" + id);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            Construct(reader);
            reader.Close();
            DB.Connection.Return(cmd.Connection);
        }

        private Shapefile(NpgsqlDataReader reader)
        {
            Construct(reader);
        }

        private void Construct(NpgsqlDataReader reader)
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
