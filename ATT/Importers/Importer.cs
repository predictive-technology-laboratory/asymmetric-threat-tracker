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
using Npgsql;
using LAIR.ResourceAPIs.PostgreSQL;
using LAIR.XML;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using LAIR.IO;

namespace PTL.ATT.Importers
{
    [Serializable]
    public abstract class Importer
    {
        /// <summary>
        /// Delegate for update requests.
        /// </summary>
        /// <param name="requestName">Name of request (informational).</param>
        /// <param name="currentValue">Current value.</param>
        /// <param name="possibleValues">Possible values.</param>
        /// <param name="requestId">ID of request</param>
        public delegate void UpdateRequestDelegate(string requestName, object currentValue, IEnumerable<object> possibleValues, string requestId);

        public const string Table = "importer";

        public class Columns
        {
            [Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string Importer = "importer";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return Reflector.GetSelectColumns(Table, typeof(Columns)); } }
        }

        [ConnectionPool.CreateTable]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Id + " SERIAL PRIMARY KEY," +
                   Columns.Importer + " BYTEA);";
        }

        public static IEnumerable<Importer> GetAll()
        {
            NpgsqlCommand cmd = null;
            NpgsqlDataReader reader = null;
            try
            {
                cmd = new NpgsqlCommand("SELECT " + Columns.Select + " FROM " + Table, DB.Connection.OpenConnection);
                reader = cmd.ExecuteReader();
                BinaryFormatter bf = new BinaryFormatter();
                List<Importer> importers = new List<Importer>();
                while (reader.Read())
                {
                    Importer importer = bf.Deserialize(new MemoryStream(reader[Table + "_" + Columns.Importer] as byte[])) as Importer;
                    importer.Id = Convert.ToInt32(reader[Table + "_" + Columns.Id]);
                    importers.Add(importer);
                }

                reader.Close();

                return importers;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                if (cmd != null)
                    DB.Connection.Return(cmd.Connection);
            }
        }

        private string _name;
        private string _path;
        private string _sourceURI;        
        private string _insertTable;
        private string _insertColumns;
        private int _id;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Path
        {
            get { return _path; }
        }

        public string SourceURI
        {
            get { return _sourceURI; }
        }

        public string InsertTable
        {
            get { return _insertTable; }
            set { _insertTable = value; }
        }

        public string InsertColumns
        {
            get { return _insertColumns; }
            set { _insertColumns = value; }
        }

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public Importer(string name, string path, string sourceURI)
        {
            _name = name;
            _path = path;
            _sourceURI = sourceURI;
        }

        public virtual void Import()
        {
            if (!System.IO.File.Exists(_path) && _sourceURI != null)
                Network.Download(_sourceURI, _path);
        }

        public void Save()
        {
            Delete();

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            _id = Convert.ToInt32(DB.Connection.ExecuteScalar("INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES (@" + Columns.Importer + ") RETURNING " + Columns.Id, new Parameter(Columns.Importer, NpgsqlTypes.NpgsqlDbType.Bytea, ms.ToArray())));
        }  

        public virtual void GetUpdateRequests(UpdateRequestDelegate updateRequest)
        {
            if (updateRequest == null)
                throw new ArgumentNullException("Must pass a non-null update request delegate");

            updateRequest("Name", _name, null, GetUpdateRequestId("name"));
            updateRequest("Path", _path, null, GetUpdateRequestId("path"));
            updateRequest("Source URI", _sourceURI, null, GetUpdateRequestId("uri"));
        }

        public virtual void Update(Dictionary<string, object> updateKeyValue)
        {
            _name = Convert.ToString(updateKeyValue[GetUpdateRequestId("name")]);
            _path = Convert.ToString(updateKeyValue[GetUpdateRequestId("path")]);
            _sourceURI = Convert.ToString(updateKeyValue[GetUpdateRequestId("uri")]);
        }

        internal string GetUpdateRequestId(string id)
        {
            return GetType() + "+" + id;
        }

        public void Delete()
        {
            DB.Connection.ExecuteNonQuery("DELETE FROM " + Table + " WHERE " + Columns.Id + "=" + _id);
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(_name))
                return _name;
            else if (!string.IsNullOrWhiteSpace(_path))
                return System.IO.Path.GetFileNameWithoutExtension(_path);
            else
                return GetType().FullName;
        }
    }
}
