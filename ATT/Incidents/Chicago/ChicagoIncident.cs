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
        public new const string Table = "chicago_incident";

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
            [Reflector.Insert, Reflector.Select(true)]
            public const string NativeId = "native_id";
            [Reflector.Insert,Reflector.Select(true)]
            public const string Ward = "ward";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return Incident.Columns.Select + "," + Reflector.GetSelectColumns(Table, typeof(Columns)); } }
            public static string JoinIncident { get { return Incident.Table + " JOIN " + Table + " ON " + Incident.Table + "." + Incident.Columns.Id + "=" + Table + "." + Columns.Id; } }
        }

        [ConnectionPool.CreateTable(typeof(Incident))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Arrest + " BOOLEAN," +
                   Columns.Beat + " VARCHAR," +
                   Columns.Block + " VARCHAR," +
                   Columns.CaseNumber + " VARCHAR," +
                   Columns.Description + " VARCHAR," +
                   Columns.Domestic + " BOOLEAN," +
                   Columns.FbiCode + " VARCHAR," +
                   Columns.Id + " INT PRIMARY KEY REFERENCES " + Incident.Table + " ON DELETE CASCADE," +
                   Columns.IUCR + " VARCHAR," +
                   Columns.LocationDescription + " VARCHAR," +
                   Columns.NativeId + " INT UNIQUE," +
                   Columns.Ward + " VARCHAR);" +
                   (connection.TableExists(Table) ? "" :
                   "CREATE INDEX ON " + Table + " (" + Columns.CaseNumber + ");" +
                   "CREATE INDEX ON " + Table + " (" + Columns.NativeId + ");");
        }

        internal new static void VacuumTable()
        {
            DB.Connection.ExecuteNonQuery("VACUUM ANALYZE " + Table);
        }

        public static string GetValue(bool arrest, string beat, string block, string caseNumber, string description, bool domestic, string fbiCode, string id, string iucr, string locationDescription, int nativeId, string ward)
        {
            return arrest + ",'" + Util.Escape(beat) + "','" + Util.Escape(block) + "','" + Util.Escape(caseNumber) + "','" + Util.Escape(description) + "'," + domestic + ",'" + Util.Escape(fbiCode) + "'," + id + ",'" + Util.Escape(iucr) + "','" + Util.Escape(locationDescription) + "'," + nativeId + ",'" + Util.Escape(ward) + "'";
        }

        public new static IEnumerable<ChicagoIncident> Get(DateTime start, DateTime end, params string[] types)
        {
            string typesCondition = null;
            if (types != null)
                foreach (string type in types)
                    typesCondition = (typesCondition == null ? "" : " OR ") + Incident.Columns.Type + "=" + type;

            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " " +
                                                         "FROM " + Columns.JoinIncident + " " +
                                                         "WHERE " + (typesCondition == null ? "" : Incident.Columns.Type + "='" + typesCondition + "' AND ") +
                                                                    Incident.Columns.Time + " >= @start AND " +
                                                                    Incident.Columns.Time + " <= @end",
                                                         new Parameter("start", NpgsqlDbType.Timestamp, start),
                                                         new Parameter("end", NpgsqlDbType.Timestamp, end));

            List<ChicagoIncident> incidents = new List<ChicagoIncident>();
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                incidents.Add(new ChicagoIncident(reader));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return incidents;
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
        private int _nativeId;
        private string _ward;

        public ChicagoIncident(NpgsqlDataReader reader)
            : base(reader)
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
            _nativeId = Convert.ToInt32(reader[Table + "_" + Columns.NativeId]);
            _ward = Convert.ToString(reader[Table + "_" + Columns.Ward]);
        }

        public override string ToString()
        {
            return base.ToString() + " " + _description;
        }
    }
}
