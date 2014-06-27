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

using LAIR.Collections.Generic;
using LAIR.ResourceAPIs.PostgreSQL;
using LAIR.XML;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostGIS = LAIR.ResourceAPIs.PostGIS;

namespace PTL.ATT.Importers
{
    [Serializable]
    public class XmlImporter : Importer
    {
        #region row inserters
        /// <summary>
        /// XML row inserter
        /// </summary>
        [Serializable]
        public abstract class XmlRowInserter
        {
            private XmlImporter _xmlImporter;

            /// <summary>
            /// A reference to the importer that is going to use this row inserter.
            /// </summary>
            public XmlImporter XmlImporter
            {
                get { return _xmlImporter; }
                set { _xmlImporter = value; }
            }

            /// <summary>
            /// Called by the importer just prior to row processing and insertion.
            /// </summary>
            public abstract void Initialize();

            /// <summary>
            /// Initializes table insertion (called from row inserters).
            /// </summary>
            /// <param name="insertTable">Table into which to insert data</param>
            /// <param name="insertColumns">Columns into which to insert data (comma-separated)</param>
            protected void Initialize(string insertTable, string insertColumns)
            {
                _xmlImporter.InsertTable = insertTable;
                _xmlImporter.InsertColumns = insertColumns;
            }

            /// <summary>
            /// Returns an insertion value with parameters given a row from an XML file.
            /// </summary>
            /// <param name="xmlRowParser">XML row</param>
            /// <returns>Insertion value with parameters</returns>
            public abstract Tuple<string, List<Parameter>> GetInsertValueAndParameters(XmlParser xmlRowParser);

            /// <summary>
            /// Cleans up any unnecessary memory
            /// </summary>
            internal abstract void Cleanup();

            public virtual void GetUpdateRequests(UpdateRequestDelegate updateRequest)
            {
            }

            public virtual void Update(Dictionary<string, object> updateKeyValue)
            {
            }
        }

        /// <summary>
        /// Inserter for incidents from XML files (in Socrata format)
        /// </summary>
        [Serializable]
        public class IncidentXmlRowInserter : XmlRowInserter
        {
            private Dictionary<string, string> _dbColInputCol;
            private Area _importArea;
            private int _hourOffset;
            private int _sourceSRID;
            private Set<int> _existingNativeIDs;

            public IncidentXmlRowInserter(Dictionary<string, string> dbColInputCol, Area importArea, int hourOffset, int sourceSRID)
            {
                _dbColInputCol = dbColInputCol;
                _importArea = importArea;
                _hourOffset = hourOffset;
                _sourceSRID = sourceSRID;
            }

            public override void Initialize()
            {
                if (_importArea == null || !Area.GetAll().Any(a => a.Id == _importArea.Id))
                    throw new Exception("No import area is defined for these incidents. If you're running a stored importer, try editing the importer and selecting an import area.");

                base.Initialize(Incident.CreateTable(_importArea), Incident.Columns.Insert);

                _existingNativeIDs = Incident.GetNativeIds(_importArea);
                _existingNativeIDs.ThrowExceptionOnDuplicateAdd = false;
            }

            public override void GetUpdateRequests(UpdateRequestDelegate updateRequest)
            {
                base.GetUpdateRequests(updateRequest);

                updateRequest("Area", _importArea, Area.GetAll(), XmlImporter.GetUpdateRequestId("area"));
                updateRequest("Hour offset", _hourOffset, null, XmlImporter.GetUpdateRequestId("offset"));
                updateRequest("Source SRID", _sourceSRID, null, XmlImporter.GetUpdateRequestId("source_srid"));
            }

            public override void Update(Dictionary<string, object> updateKeyValue)
            {
                base.Update(updateKeyValue);

                _importArea = (Area)updateKeyValue[XmlImporter.GetUpdateRequestId("area")];
                _hourOffset = Convert.ToInt32(updateKeyValue[XmlImporter.GetUpdateRequestId("offset")]);
                _sourceSRID = Convert.ToInt32(updateKeyValue[XmlImporter.GetUpdateRequestId("source_srid")]);
            }

            public override Tuple<string, List<Parameter>> GetInsertValueAndParameters(XmlParser rowXmlParser)
            {
                int nativeId = int.Parse(rowXmlParser.ElementText(_dbColInputCol[Incident.Columns.NativeId])); rowXmlParser.Reset();

                if (_existingNativeIDs.Add(nativeId))
                {
                    DateTime time = DateTime.Parse(rowXmlParser.ElementText(_dbColInputCol[Incident.Columns.Time])) + new TimeSpan(_hourOffset, 0, 0); rowXmlParser.Reset();
                    string type = rowXmlParser.ElementText(_dbColInputCol[Incident.Columns.Type]); rowXmlParser.Reset();

                    double x;
                    if (!double.TryParse(rowXmlParser.ElementText(_dbColInputCol[Incident.Columns.X(_importArea)]), out x))
                        return null;

                    rowXmlParser.Reset();

                    double y;
                    if (!double.TryParse(rowXmlParser.ElementText(_dbColInputCol[Incident.Columns.Y(_importArea)]), out y))
                        return null;

                    rowXmlParser.Reset();

                    PostGIS.Point location = new PostGIS.Point(x, y, _sourceSRID);

                    string value = Incident.GetValue(_importArea, nativeId, location, false, "@time_" + nativeId, type);
                    List<Parameter> parameters = new List<Parameter>(new Parameter[] { new Parameter("time_" + nativeId, NpgsqlDbType.Timestamp, time) });
                    return new Tuple<string, List<Parameter>>(value, parameters);
                }
                else
                    return null;
            }

            internal override void Cleanup()
            {
                _existingNativeIDs = null;
            }   
        }

        /// <summary>
        /// Inserter for point data from XML files (in Socrata format)
        /// </summary>
        [Serializable]
        public class PointfileXmlRowInserter : XmlRowInserter
        {
            public static class Columns
            {
                public const string X = "X";
                public const string Y = "Y";
                public const string Time = "Time";
            }

            private Dictionary<string, string> _dbColInputCol;
            private int _sourceSRID;
            private Area _importArea;
            private int _rowNum;

            public PointfileXmlRowInserter(Dictionary<string, string> dbColInputCol, int sourceSRID, Area importArea)
            {
                _dbColInputCol = dbColInputCol;
                _sourceSRID = sourceSRID;
                _importArea = importArea;
            }

            public override void Initialize()
            {
                if (!Area.GetAll().Any(a => a.Id == _importArea.Id))
                    throw new Exception("Area into which incidents are to be imported (ID=" + _importArea.Id + ", name=" + _importArea.Name + ") does not exist in the database. If you're running a stored importer, try editing the importer.");

                _rowNum = 0;

                Shapefile shapefile = Shapefile.Create(XmlImporter.Name, _importArea.Shapefile.SRID, Shapefile.ShapefileType.Feature);

                DB.Connection.ExecuteNonQuery(
                    "CREATE TABLE " + shapefile.GeometryTable + " (" +
                    ShapefileGeometry.Columns.Geometry + " GEOMETRY(GEOMETRY," + shapefile.SRID + ")," +
                    ShapefileGeometry.Columns.Id + " SERIAL PRIMARY KEY," +
                    ShapefileGeometry.Columns.Time + " TIMESTAMP);" +
                    "CREATE INDEX ON " + shapefile.GeometryTable + " USING GIST (" + ShapefileGeometry.Columns.Geometry + ");" +
                    "CREATE INDEX ON " + shapefile.GeometryTable + " (" + ShapefileGeometry.Columns.Time + ");");

                base.Initialize(shapefile.GeometryTable, ShapefileGeometry.Columns.Insert);
            }

            public override void GetUpdateRequests(UpdateRequestDelegate updateRequest)
            {
                base.GetUpdateRequests(updateRequest);

                updateRequest("Area", _importArea, Area.GetForSRID(_importArea.Shapefile.SRID), XmlImporter.GetUpdateRequestId("area"));
                updateRequest("Source SRID", _sourceSRID, null, XmlImporter.GetUpdateRequestId("source_srid"));
            }

            public override void Update(Dictionary<string, object> updateKeyValue)
            {
                base.Update(updateKeyValue);

                _importArea = (Area)updateKeyValue[XmlImporter.GetUpdateRequestId("area")];
                _sourceSRID = Convert.ToInt32(updateKeyValue[XmlImporter.GetUpdateRequestId("source_srid")]);
            }

            public override Tuple<string, List<Parameter>> GetInsertValueAndParameters(XmlParser xmlRowParser)
            {
                double x;
                if (!double.TryParse(xmlRowParser.ElementText(_dbColInputCol[Columns.X]), out x))
                    return null;

                xmlRowParser.Reset();

                double y;
                if (!double.TryParse(xmlRowParser.ElementText(_dbColInputCol[Columns.Y]), out y))
                    return null;

                xmlRowParser.Reset();

                DateTime time;
                if (!DateTime.TryParse(xmlRowParser.ElementText(_dbColInputCol[Columns.Time]), out time))
                    return null;

                string timeParamName = "time_" + _rowNum++;
                List<Parameter> parameters = new List<Parameter>(new Parameter[] { new Parameter(timeParamName, NpgsqlDbType.Timestamp, time) });
                return new Tuple<string, List<Parameter>>(ShapefileGeometry.GetValue(new PostGIS.Point(x, y, _sourceSRID), _importArea.Shapefile.SRID, timeParamName), parameters);
            }

            internal override void Cleanup()
            {
            }
        }
        #endregion

        #region static members
        public static string[] GetColumnNames(string path, string rootElementName, string rowElementName)
        {
            Console.Out.WriteLine("Scanning \"" + path + "\" for input field names...");

            using (FileStream file = new FileStream(path, FileMode.Open))
            {
                XmlParser p = new XmlParser(file);
                p.SkipToElement(rootElementName);
                Set<string> columnNames = new Set<string>(false);
                while (true)
                {
                    p.MoveToElementNode(false);
                    string rowXML = p.OuterXML(rowElementName);
                    if (rowXML == null)
                        break;

                    XmlParser rowP = new XmlParser(rowXML);
                    rowP.MoveToElementNode(true);

                    while (rowP.MoveToElementNode(false) != null)
                        columnNames.Add(rowP.CurrentName);
                }

                return columnNames.ToArray();
            }
        }
        #endregion

        private XmlRowInserter _xmlRowInserter;
        private string _rootElementName;
        private string _rowElementName;

        public XmlImporter(string name, string path, string relativePath, string sourceURI, XmlRowInserter xmlRowInserter, string rootElementName, string rowElementName)
            : base(name, path, relativePath, sourceURI)
        {
            _xmlRowInserter = xmlRowInserter;
            _rootElementName = rootElementName;
            _rowElementName = rowElementName;

            _xmlRowInserter.XmlImporter = this;
        }

        public override void Import()
        {
            base.Import();

            using (FileStream file = new FileStream(Path, FileMode.Open))
            {
                XmlParser p = new XmlParser(file);
                p.SkipToElement(_rootElementName);
                p.MoveToElementNode(false);
                int totalRows = 0;
                int totalImported = 0;
                int skippedRows = 0;
                int batchCount = 0;
                string rowXML;
                NpgsqlCommand insertCmd = DB.Connection.NewCommand(null);
                StringBuilder cmdTxt = new StringBuilder();
                try
                {
                    _xmlRowInserter.Initialize();

                    while ((rowXML = p.OuterXML(_rowElementName)) != null)
                    {
                        ++totalRows;

                        Tuple<string, List<Parameter>> valueParameters = _xmlRowInserter.GetInsertValueAndParameters(new XmlParser(rowXML));

                        if (valueParameters == null)
                            ++skippedRows;
                        else
                        {
                            cmdTxt.Append((batchCount == 0 ? "INSERT INTO " + InsertTable + " (" + InsertColumns + ") VALUES " : ",") + "(" + valueParameters.Item1 + ")");

                            if (valueParameters.Item2.Count > 0)
                                ConnectionPool.AddParameters(insertCmd, valueParameters.Item2);

                            if (++batchCount >= 5000)
                            {
                                insertCmd.CommandText = cmdTxt.ToString();
                                insertCmd.ExecuteNonQuery();
                                insertCmd.Parameters.Clear();
                                cmdTxt.Clear();
                                totalImported += batchCount;
                                batchCount = 0;

                                Console.Out.WriteLine("Imported " + totalImported + " rows of " + totalRows + " total in the file (" + skippedRows + " rows were skipped)");
                            }
                        }
                    }

                    if (batchCount > 0)
                    {
                        insertCmd.CommandText = cmdTxt.ToString();
                        insertCmd.ExecuteNonQuery();
                        insertCmd.Parameters.Clear();
                        cmdTxt.Clear();
                        totalImported += batchCount;
                        batchCount = 0;
                    }

                    Console.Out.WriteLine("Cleaning up database after import");
                    DB.Connection.ExecuteNonQuery("VACUUM ANALYZE " + InsertTable);
                    _xmlRowInserter.Cleanup();

                    Console.Out.WriteLine("Import from \"" + Path + "\" was successful.  Imported " + totalImported + " rows of " + totalRows + " total in the file (" + skippedRows + " rows were skipped)");
                }
                catch (Exception ex)
                {
                    throw new Exception("An import error occurred:  " + ex.Message);
                }
                finally
                {
                    DB.Connection.Return(insertCmd.Connection);
                }
            }
        }

        public override void GetUpdateRequests(UpdateRequestDelegate updateRequest)
        {
            base.GetUpdateRequests(updateRequest);

            _xmlRowInserter.GetUpdateRequests(updateRequest);
        }

        public override void Update(Dictionary<string, object> updateKeyValue)
        {
            base.Update(updateKeyValue);

            _xmlRowInserter.Update(updateKeyValue);
        }
    }
}
