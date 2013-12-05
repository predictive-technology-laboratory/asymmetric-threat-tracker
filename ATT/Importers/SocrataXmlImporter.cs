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
using LAIR.ResourceAPIs.PostgreSQL;
using LAIR.XML;
using Npgsql;
using System.Threading;
using LAIR.Collections.Generic;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using NpgsqlTypes;

namespace PTL.ATT.Importers
{
    public class SocrataXmlImporter : Importer
    {
        public static string[] GetColumnNames(string path)
        {
            using (FileStream file = new FileStream(path, FileMode.Open))
            {
                XmlParser p = new XmlParser(file);
                p.SkipToElement("row");
                p.MoveToElementNode(false);
                string rowXML = p.OuterXML("row");
                XmlParser rowP = new XmlParser(rowXML);
                rowP.MoveToElementNode(true);
                List<string> columnNames = new List<string>();
                while (rowP.MoveToElementNode(false) != null)
                    columnNames.Add(rowP.CurrentName);

                return columnNames.ToArray();
            }
        }

        public SocrataXmlImporter()
            : base()
        {
        }

        public override void Import(string path, string table, string columns, Func<XmlParser, Tuple<string, List<Parameter>>> rowToInsertValueAndParams)
        {
            base.Import(path, table, columns, rowToInsertValueAndParams);

            using (FileStream file = new FileStream(path, FileMode.Open))
            {
                XmlParser p = new XmlParser(file);
                p.SkipToElement("row");
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
                    while ((rowXML = p.OuterXML("row")) != null)
                    {
                        ++totalRows;

                        Tuple<string, List<Parameter>> valueParameters = rowToInsertValueAndParams(new XmlParser(rowXML));

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
