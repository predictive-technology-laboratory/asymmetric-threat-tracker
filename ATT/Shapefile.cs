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
        public enum ShapefileType
        {
            Area,
            Feature
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
            Shapefile shapefile = new Shapefile(Convert.ToInt32(new NpgsqlCommand("INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES ('" + name + "'," + srid + ",'" + type + "') RETURNING " + Columns.Id, connection).ExecuteScalar()));
            ShapefileGeometry.CreateTable(shapefile);
            return shapefile.Id;
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
            if (!string.IsNullOrWhiteSpace(_name))
                return _name;
            else
                return _id.ToString();
        }

        public void Delete()
        {
            try
            {
                DB.Connection.ExecuteNonQuery("DELETE FROM " + Table + " WHERE " + Columns.Id + "=" + _id);
                DB.Connection.ExecuteNonQuery("DROP TABLE " + ShapefileGeometry.GetTableName(this));
            }
            catch (Exception ex) { Console.Out.WriteLine("Error deleting shapefile:  " + ex.Message); }
        }
    }
}
