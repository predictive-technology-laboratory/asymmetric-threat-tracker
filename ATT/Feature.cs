#region copyright
//    Copyright 2013 Matthew S. Gerber (gerber.matthew@gmail.com)
//
//    This file is part of the Asymmetric Threat Tracker (ATT).
//
//    The ATT is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    The ATT is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with the ATT.  If not, see <http://www.gnu.org/licenses/>.
#endregion
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using LAIR.ResourceAPIs.PostgreSQL;
using PTL.ATT.Models;
using LAIR.Collections.Generic;

namespace PTL.ATT
{
    public class Feature : IComparable<Feature>
    {
        public const string Table = "feature";

        public class Columns
        {
            [Reflector.Insert, Reflector.Select(true)]
            public const string Description = "description";
            [Reflector.Insert, Reflector.Select(true)]
            public const string EnumType = "enum_type";
            [Reflector.Insert, Reflector.Select(true)]
            public const string EnumValue = "enum_value";
            [Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string PredictionId = "prediction_id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string ResourceId = "resource_id";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return Reflector.GetSelectColumns(Table, typeof(Columns)); } }
        }

        [ConnectionPool.CreateTable(typeof(Prediction))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                    Columns.Description + " VARCHAR," +
                    Columns.EnumType + " VARCHAR," +
                    Columns.EnumValue + " VARCHAR," +
                    Columns.Id + " SERIAL PRIMARY KEY," +
                    Columns.PredictionId + " INT REFERENCES " + Prediction.Table + " ON DELETE CASCADE," +
                    Columns.ResourceId + " VARCHAR);" +
                    (connection.TableExists(Table) ? "" :
                    "CREATE INDEX ON " + Table + " (" + Columns.EnumType + ");" +
                    "CREATE INDEX ON " + Table + " (" + Columns.EnumValue + ");" +
                    "CREATE INDEX ON " + Table + " (" + Columns.PredictionId + ");" +
                    "CREATE INDEX ON " + Table + " (" + Columns.ResourceId + ");");
        }

        public static int Create(NpgsqlConnection connection, string description, Type enumType, Enum enumValue, int predictionId, string resourceId, bool vacuum)
        {
            NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES ('" + description + "','" + enumType + "','" + enumValue + "'," + predictionId + "," + (resourceId == null ? "NULL" : "'" + resourceId + "'") + ") RETURNING " + Columns.Id, connection);
            int id = Convert.ToInt32(cmd.ExecuteScalar());

            if (vacuum)
                VacuumTable();

            return id;
        }

        internal static void VacuumTable()
        {
            DB.Connection.ExecuteNonQuery("VACUUM ANALYZE " + Table);
        }

        private int _id;
        private Type _enumType;
        private Enum _enumValue;
        private string _description;
        private int _predictionId;
        private string _resourceId;

        public int Id
        {
            get {  return _id; }
        }

        public Type EnumType
        {
            get { return _enumType; }
        }

        public Enum EnumValue
        {
            get { return _enumValue; }
        }

        public string Description
        {
            get { return _description; }
        }

        public int PredictionId
        {
            get { return _predictionId; }
        }

        public Prediction Prediction
        {
            get { return new Prediction(_predictionId); }
        }

        public string ResourceId
        {
            get { return _resourceId; }
        }

        internal Feature(NpgsqlDataReader reader)
        {
            Construct(reader);
        }

        public Feature(Type enumType, Enum enumValue, string resourceId, string description)
        {
            Construct(-1, -1, enumType, enumValue, resourceId, description);
        }

        private void Construct(NpgsqlDataReader reader)
        {
            Type enumType = Reflection.GetType(Convert.ToString(reader[Table + "_" + Columns.EnumType]));

            Construct(Convert.ToInt32(reader[Table + "_" + Columns.Id]), 
                      Convert.ToInt32(reader[Table + "_" + Columns.PredictionId]), 
                      enumType,
                      (Enum)Enum.Parse(enumType, Convert.ToString(reader[Table + "_" + Columns.EnumValue])),
                      Convert.ToString(reader[Table + "_" + Columns.ResourceId]),
                      Convert.ToString(reader[Table + "_" + Columns.Description]));
        }

        private void Construct(int id, int predictionId, Type enumType, Enum enumValue, string resourceId, string description)
        {
            _id = id;
            _predictionId = predictionId;
            _enumType = enumType;
            _enumValue = enumValue;
            _description = description;
            _resourceId = resourceId == null ? "" : resourceId;
        }

        public override string ToString()
        {
            string enumStr = _enumType.ToString();
            enumStr = enumStr.Substring(enumStr.LastIndexOf("+") + 1);
            return _description + " (" + enumStr + ")";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Feature))
                return false;

            Feature f = obj as Feature;

            return _enumType == f.EnumType && _enumValue.ToString() == f.EnumValue.ToString() && _resourceId == f.ResourceId;
        }

        public override int GetHashCode()
        {
            return (_enumType + "-" + _enumValue + "-" + _resourceId).GetHashCode();
        }

        public int CompareTo(Feature other)
        {
            int cmp = _enumType.ToString().CompareTo(other.EnumType.ToString());

            if (cmp == 0)
                cmp = _enumValue.ToString().CompareTo(other.EnumValue.ToString());

            if (cmp == 0 && _resourceId != null && other.ResourceId != null)
            {
                int r1, r2;
                if (int.TryParse(_resourceId, out r1) && int.TryParse(other.ResourceId, out r2))
                    cmp = r1.CompareTo(r2);
                else
                    cmp = _resourceId.CompareTo(other.ResourceId);
            }

            return cmp;
        }
    }
}
