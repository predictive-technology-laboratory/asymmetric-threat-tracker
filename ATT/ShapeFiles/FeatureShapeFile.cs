using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Npgsql;
using LAIR.ResourceAPIs.PostgreSQL;

namespace PTL.ATT.ShapeFiles
{
    public class FeatureShapeFile : ShapeFile
    {
        public enum ShapefileType
        {
            Distance,
            Raster
        }

        public new const string Table = "feature_shape_file";

        public new class Columns
        {
            [Reflector.Insert, Reflector.Select(true)]
            public const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            public const string ShapefileType = "shapefile_type";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return ShapeFile.Columns.Select + "," + Reflector.GetSelectColumns(Table, typeof(Columns)); } }
            public static string JoinShapeFile { get { return ShapeFile.Table + " JOIN " + Table + " ON " + ShapeFile.Table + "." + ShapeFile.Columns.Id + "=" + Table + "." + Columns.Id; } }
        }

        [ConnectionPool.CreateTable(typeof(ShapeFile))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Id + " INT PRIMARY KEY REFERENCES " + ShapeFile.Table + " ON DELETE CASCADE," +
                   Columns.ShapefileType + " VARCHAR);";
        }

        internal static int Create(NpgsqlConnection connection, string name, string shapefileType)
        {
            return Convert.ToInt32(new NpgsqlCommand("INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES (" + ShapeFile.Create(connection, name, typeof(FeatureShapeFile)) + ",'" + shapefileType + "') RETURNING " + Columns.Id, connection).ExecuteScalar());
        }

        public static IEnumerable<FeatureShapeFile> GetAvailable()
        {
            List<FeatureShapeFile> featureShapeFiles = new List<FeatureShapeFile>();

            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Columns.JoinShapeFile);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                featureShapeFiles.Add(new FeatureShapeFile(reader));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return featureShapeFiles;
        }

        private ShapefileType _type;

        public ShapefileType Type
        {
            get { return _type; }
        }

        internal FeatureShapeFile(int id)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " " + 
                                                         "FROM " + Columns.JoinShapeFile + " " + 
                                                         "WHERE " + Table + "." + Columns.Id + "=" + id); 
            NpgsqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            Construct(reader);
            reader.Close();
            DB.Connection.Return(cmd.Connection);
        }

        private FeatureShapeFile(NpgsqlDataReader reader)
        {
            Construct(reader);
        }

        protected override void Construct(NpgsqlDataReader reader)
        {
            base.Construct(reader);

            _type = (ShapefileType)Enum.Parse(typeof(ShapefileType), Convert.ToString(reader[Table + "_" + Columns.ShapefileType]));
        }

        public override string Details()
        {
            return base.Details() + Environment.NewLine +
                   "Feature type:  " + _type;
        }

        public override string ToString()
        {
            return base.ToString() + " (" + _type + ")";
        }
    }
}
