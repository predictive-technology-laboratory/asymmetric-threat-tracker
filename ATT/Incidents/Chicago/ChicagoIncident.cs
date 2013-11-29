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
using Npgsql;
using System.Reflection;
using LAIR.ResourceAPIs.PostgreSQL;
using NpgsqlTypes;

namespace PTL.ATT.Incidents.Chicago
{
    public class ChicagoIncident : Incident
    {
        public const string Table = "chicago_incident";

        public new class Columns
        {
            [Reflector.Insert,Reflector.Select(true)]
            public const string Arrest = "arrest";
            [Reflector.Insert,Reflector.Select(true)]
            public const string Beat = "beat";
            [Reflector.Insert,Reflector.Select(true)]
            public const string Block = "block";
            [Reflector.Insert,Reflector.Select(true)]
            public const string CaseNumber = "case_number";
            [Reflector.Insert,Reflector.Select(true)]
            public const string Description = "description";
            [Reflector.Insert,Reflector.Select(true)]
            public const string Domestic = "domestic";
            [Reflector.Insert,Reflector.Select(true)]
            public const string FbiCode = "fbi_code";
            [Reflector.Insert]
            public const string Id = "id";
            [Reflector.Insert,Reflector.Select(true)]
            public const string IUCR = "iucr";
            [Reflector.Insert,Reflector.Select(true)]
            public const string LocationDescription = "location_description";
            [Reflector.Insert,Reflector.Select(true)]
            public const string Ward = "ward";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select(Area area) { return Incident.Columns.Select(area) + "," + Reflector.GetSelectColumns(Table, typeof(Columns)); }
            public static string JoinIncident(Area area) { return Incident.GetTableName(area) + " JOIN " + Table + " ON " + Incident.GetTableName(area) + "." + Incident.Columns.Id + "=" + Table + "." + Columns.Id; }
        }

        internal new static void CreateTable(Area area)
        {
            Incident.CreateTable(area);

            if (!DB.Connection.TableExists(Table))
                DB.Connection.ExecuteNonQuery(
                    "CREATE TABLE " + Table + " (" +
                    Columns.Arrest + " BOOLEAN," +
                    Columns.Beat + " VARCHAR," +
                    Columns.Block + " VARCHAR," +
                    Columns.CaseNumber + " VARCHAR," +
                    Columns.Description + " VARCHAR," +
                    Columns.Domestic + " BOOLEAN," +
                    Columns.FbiCode + " VARCHAR," +
                    Columns.Id + " INT PRIMARY KEY REFERENCES " + Incident.GetTableName(area) + " ON DELETE CASCADE," +
                    Columns.IUCR + " VARCHAR," +
                    Columns.LocationDescription + " VARCHAR," +
                    Columns.Ward + " VARCHAR);" +
                    "CREATE INDEX ON " + Table + " (" + Columns.CaseNumber + ");");
        }

        internal static void VacuumTable()
        {
            DB.Connection.ExecuteNonQuery("VACUUM ANALYZE " + Table);
        }

        internal static string GetValue(bool arrest, string beat, string block, string caseNumber, string description, bool domestic, string fbiCode, string id, string iucr, string locationDescription, string ward)
        {
            return arrest + ",'" + Util.Escape(beat) + "','" + Util.Escape(block) + "','" + Util.Escape(caseNumber) + "','" + Util.Escape(description) + "'," + domestic + ",'" + Util.Escape(fbiCode) + "'," + id + ",'" + Util.Escape(iucr) + "','" + Util.Escape(locationDescription) + "','" + Util.Escape(ward) + "'";
        }

        private bool _arrest;
        private string _beat;
        private string _block;
        private string _caseNumber;
        private string _description;
        private bool _domestic;
        private string _fbiCode;
        private string _iucr;
        private string _locationDescription;
        private string _ward;

        public ChicagoIncident(NpgsqlDataReader reader, Area area)
            : base(reader, area)
        {
            _arrest = Convert.ToBoolean(reader[Table + "_" + Columns.Arrest]);
            _beat = Convert.ToString(reader[Table + "_" + Columns.Beat]);
            _block = Convert.ToString(reader[Table + "_" + Columns.Block]);
            _caseNumber = Convert.ToString(reader[Table + "_" + Columns.CaseNumber]);
            _description = Convert.ToString(reader[Table + "_" + Columns.Description]);
            _domestic = Convert.ToBoolean(reader[Table + "_" + Columns.Domestic]);
            _fbiCode = Convert.ToString(reader[Table + "_" + Columns.FbiCode]);
            _iucr = Convert.ToString(reader[Table + "_" + Columns.IUCR]);
            _locationDescription = Convert.ToString(reader[Table + "_" + Columns.LocationDescription]);
            _ward = Convert.ToString(reader[Table + "_" + Columns.Ward]);
        }

        public override string ToString()
        {
            return base.ToString() + " " + _description;
        }
    }
}
