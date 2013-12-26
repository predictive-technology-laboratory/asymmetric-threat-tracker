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
    public class XmlImporter : Importer
    {
        #region row inserters
        public abstract class XmlRowInserter
        {
            public abstract Tuple<string, List<Parameter>> GetInsertValueAndParameters(XmlParser xmlRowParser);
        }

        public class IncidentXmlRowInserter : XmlRowInserter
        {
            private Dictionary<string, string> _dbColSocrataCol;
            private Area _importArea;
            private int _hourOffset;
            private int _sourceSRID;
            private Set<int> _existingNativeIDs;

            public IncidentXmlRowInserter(Dictionary<string, string> dbColSocrataCol, Area importArea, int hourOffset, int sourceSRID, Set<int> existingNativeIDs)
            {
                _dbColSocrataCol = dbColSocrataCol;
                _importArea = importArea;
                _hourOffset = hourOffset;
                _sourceSRID = sourceSRID;
                _existingNativeIDs = existingNativeIDs;
            }

            public override Tuple<string, List<Parameter>> GetInsertValueAndParameters(XmlParser rowXmlParser)
            {
                int nativeId = int.Parse(rowXmlParser.ElementText(_dbColSocrataCol[Incident.Columns.NativeId])); rowXmlParser.Reset();

                if (_existingNativeIDs.Add(nativeId))
                {
                    DateTime time = DateTime.Parse(rowXmlParser.ElementText(_dbColSocrataCol[Incident.Columns.Time])) + new TimeSpan(_hourOffset, 0, 0); rowXmlParser.Reset();
                    string type = rowXmlParser.ElementText(_dbColSocrataCol[Incident.Columns.Type]); rowXmlParser.Reset();

                    double x;
                    if (!double.TryParse(rowXmlParser.ElementText(_dbColSocrataCol[Incident.Columns.X(_importArea)]), out x))
                        return null;

                    rowXmlParser.Reset();

                    double y;
                    if (!double.TryParse(rowXmlParser.ElementText(_dbColSocrataCol[Incident.Columns.Y(_importArea)]), out y))
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
        }
        #endregion

        public static string[] GetColumnNames(string path, string rootElementName, string rowElementName)
        {
            using (FileStream file = new FileStream(path, FileMode.Open))
            {
                XmlParser p = new XmlParser(file);
                p.SkipToElement(rootElementName);
                p.MoveToElementNode(false);
                string rowXML = p.OuterXML(rowElementName);
                XmlParser rowP = new XmlParser(rowXML);
                rowP.MoveToElementNode(true);
                List<string> columnNames = new List<string>();
                while (rowP.MoveToElementNode(false) != null)
                    columnNames.Add(rowP.CurrentName);

                return columnNames.ToArray();
            }
        }

        private XmlRowInserter _xmlRowInserter;
        private string _rootElementName;
        private string _rowElementName;

        public XmlImporter(XmlRowInserter xmlRowInserter, string rootElementName, string rowElementName)
        {
            _xmlRowInserter = xmlRowInserter;
            _rootElementName = rootElementName;
            _rowElementName = rowElementName;
        }

        public override void Import(string path, string table, string columns)
        {
            using (FileStream file = new FileStream(path, FileMode.Open))
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
                    while ((rowXML = p.OuterXML(_rowElementName)) != null)
                    {
                        ++totalRows;

                        Tuple<string, List<Parameter>> valueParameters = _xmlRowInserter.GetInsertValueAndParameters(new XmlParser(rowXML));

                        if (valueParameters == null)
                            ++skippedRows;
                        else
                        {
                            cmdTxt.Append((batchCount == 0 ? "INSERT INTO " + table + " (" + columns + ") VALUES " : ",") + "(" + valueParameters.Item1 + ")");

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
                    DB.Connection.ExecuteNonQuery("VACUUM ANALYZE " + table);

                    Console.Out.WriteLine("Import from \"" + path + "\" was successful.  Imported " + totalImported + " rows of " + totalRows + " total in the file (" + skippedRows + " rows were skipped)");
                }
                catch (Exception ex)
                {
                    throw new Exception("An import error occurred. You can safely restart the import from the same file. Message:  " + ex.Message);
                }
                finally
                {
                    DB.Connection.Return(insertCmd.Connection);
                }
            }
        }
    }
}
