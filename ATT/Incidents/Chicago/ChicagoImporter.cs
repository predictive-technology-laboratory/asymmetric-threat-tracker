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

namespace PTL.ATT.Incidents.Chicago
{
    public class ChicagoImporter : Importer
    {
        public ChicagoImporter()
            : base()
        {
        }

        public override void Import(string path, Area area)
        {
            Console.Out.WriteLine("Importing incidents from \"" + path + "\"");

            Set<int> existingNativeIDs = new Set<int>(false);
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + ChicagoIncident.Columns.NativeId + " FROM " + ChicagoIncident.Table);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                existingNativeIDs.Add(Convert.ToInt32(reader[0]));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            StringBuilder incidentInsert = new StringBuilder();
            string incidentInsertBase = "INSERT INTO " + Incident.GetTableName(area.SRID) + " (" + Incident.Columns.Insert + ") VALUES ";
            List<Parameter> incidentParameters = new List<Parameter>();

            StringBuilder chicagoIncidentInsert = new StringBuilder();
            string chicagoIncidentInsertBase = "INSERT INTO " + ChicagoIncident.Table + " (" + ChicagoIncident.Columns.Insert + ") VALUES ";
            List<Parameter> chicagoIncidentParameters = new List<Parameter>();

            XmlParser p = new XmlParser(new FileStream(path, FileMode.Open));
            p.SkipToElement("row");
            p.MoveToElementNode(false);
            int totalRows = 0;
            int totalImported = 0;
            int alreadyPresent = 0;
            int batchCount = 0;
            string rowXML;
            try
            {
                while ((rowXML = p.OuterXML("row")) != null)
                {
                    ++totalRows;

                    XmlParser rowP = new XmlParser(rowXML);
                    int nativeId = int.Parse(rowP.ElementText("id")); rowP.Reset();

                    // avoid previously imported records and duplicate records in current import
                    if (existingNativeIDs.Add(nativeId))
                    {
                        string caseNumber = rowP.ElementText("case_number"); rowP.Reset();
                        DateTime date = DateTime.Parse(rowP.ElementText("date")) + new TimeSpan(Configuration.IncidentHourOffset, 0, 0); rowP.Reset();
                        string block = rowP.ElementText("block"); rowP.Reset();
                        string iucr = rowP.ElementText("iucr"); rowP.Reset();
                        string primaryType = rowP.ElementText("primary_type"); rowP.Reset();
                        string description = rowP.ElementText("description"); rowP.Reset();
                        string locationDescription = rowP.ElementText("location_description"); rowP.Reset();
                        bool arrest = bool.Parse(rowP.ElementText("arrest")); rowP.Reset();
                        bool domestic = bool.Parse(rowP.ElementText("domestic")); rowP.Reset();
                        string beat = rowP.ElementText("beat"); rowP.Reset();
                        string ward = rowP.ElementText("ward"); rowP.Reset();
                        string fbiCode = rowP.ElementText("fbi_code"); rowP.Reset();

                        // only use incidents that have coordinates
                        double x;
                        if (!double.TryParse(rowP.ElementText("longitude"), out x))
                            continue;

                        rowP.Reset();

                        double y;
                        if (!double.TryParse(rowP.ElementText("latitude"), out y))
                            continue;

                        rowP.Reset();

                        PostGIS.Point location = new PostGIS.Point(x, y, Configuration.IncidentNativeLocationSRID);

                        incidentInsert.Append((batchCount == 0 ? incidentInsertBase : ",") + "(" + Incident.GetValue(area.Id, "st_transform(" + location.StGeometryFromText + "," + area.SRID + ")", false, "@date_" + nativeId, primaryType) + ")");
                        incidentParameters.Add(new Parameter("date_" + nativeId, NpgsqlDbType.Timestamp, date));

                        chicagoIncidentInsert.Append((batchCount == 0 ? chicagoIncidentInsertBase : ",") + "(" + ChicagoIncident.GetValue(arrest, beat, block, caseNumber, description, domestic, fbiCode, "@id_" + nativeId, iucr, locationDescription, nativeId, ward) + ")");
                        chicagoIncidentParameters.Add(new Parameter("id_" + nativeId, NpgsqlDbType.Integer, null));

                        if (++batchCount >= 5000)
                        {
                            Insert(incidentInsert, incidentParameters, chicagoIncidentInsert, chicagoIncidentParameters);

                            incidentInsert.Clear();
                            incidentParameters.Clear();

                            chicagoIncidentInsert.Clear();
                            chicagoIncidentParameters.Clear();

                            totalImported += batchCount;
                            batchCount = 0;

                            Console.Out.WriteLine("Imported " + totalImported + " incidents of " + totalRows + " total in the file (" + alreadyPresent + " incidents were already in the database)");
                        }
                    }
                    else
                        ++alreadyPresent;
                }
                p.Close();

                if (batchCount > 0)
                {
                    Insert(incidentInsert, incidentParameters, chicagoIncidentInsert, chicagoIncidentParameters);
                    totalImported += batchCount;
                }

                Incident.VacuumTable(area.SRID);
                ChicagoIncident.VacuumTable();

                Console.Out.WriteLine("Import from \"" + path + "\" was successful.  Imported " + totalImported + " incidents of " + totalRows + " total in the file (" + alreadyPresent + " incidents were already in the database)");
            }
            catch (Exception ex)
            {
                throw new Exception("An import error occurred. You can safely restart the import from the same file. Message:  " + ex.Message);
            }
        }

        private void Insert(StringBuilder incidentInsert, List<Parameter> incidentParameters, StringBuilder chicagoIncidentInsert, List<Parameter> chicagoInsertParameters)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand(null);

            // begin transaction
            try
            {
                cmd.CommandText = "BEGIN";
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                DB.Connection.Return(cmd.Connection);
                throw new Exception("Failed to begin insert transaction:  " + ex.Message);
            }

            // insert data
            try
            {
                incidentInsert.Append(" RETURNING " + Incident.Columns.Id);
                cmd.CommandText = incidentInsert.ToString();
                ConnectionPool.AddParameters(cmd, incidentParameters);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                foreach (Parameter p in chicagoInsertParameters)
                {
                    if (!reader.Read())
                        throw new Exception("Failed to get ID for Chicago incident");

                    p.Value = Convert.ToInt32(reader[0]);
                }

                reader.Close();

                cmd.CommandText = chicagoIncidentInsert.ToString();
                cmd.Parameters.Clear();
                ConnectionPool.AddParameters(cmd, chicagoInsertParameters);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Error encountered while importing:  " + ex.Message);

                try
                {
                    // try to roll back transaction
                    Console.Out.WriteLine("Trying to roll back current insert transaction");

                    cmd.CommandText = "ROLLBACK";
                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex2)
                {
                    Console.Out.WriteLine("Failed to roll back current transation:  " + ex2.Message);
                }

                DB.Connection.Return(cmd.Connection);
                throw ex;
            }

            // commit transaction
            try
            {
                cmd.CommandText = "COMMIT";
                cmd.Parameters.Clear();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Failed to commit insert transaction:  " + ex.Message);
            }

            DB.Connection.Return(cmd.Connection);
        }
    }
}
