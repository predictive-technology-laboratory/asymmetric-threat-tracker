using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Npgsql;
using System.Diagnostics;
using LAIR.ResourceAPIs.PostgreSQL;

namespace PTL.ATT.ShapeFiles
{
    public class AreaShapeFile : ShapeFile
    {
        public new const string Table = "area_shape_file";

        public new class Columns
        {
            [Reflector.Insert, Reflector.Select(true)]
            public const string Id = "id";

            public static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            public static string Select { get { return ShapeFile.Columns.Select + "," + Reflector.GetSelectColumns(Table, typeof(Columns)); } }
            public static string JoinShapeFile { get { return ShapeFile.Table + " JOIN " + Table + " ON " + ShapeFile.Table + "." + ShapeFile.Columns.Id + "=" + Table + "." + Columns.Id; } }
        }

        [ConnectionPool.CreateTable(typeof(ShapeFile))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Id + " INT PRIMARY KEY REFERENCES " + ShapeFile.Table + " ON DELETE CASCADE);";
        }

        internal static int Create(NpgsqlConnection connection, string name)
        {
            return Convert.ToInt32(new NpgsqlCommand("INSERT INTO " + Table + " (" + Columns.Insert + ") VALUES (" + ShapeFile.Create(connection, name, typeof(AreaShapeFile)) + ") RETURNING " + Columns.Id, connection).ExecuteScalar());
        }

        public static IEnumerable<AreaShapeFile> GetAvailable()
        {
            List<AreaShapeFile> areaShapeFiles = new List<AreaShapeFile>();

            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Columns.JoinShapeFile);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                areaShapeFiles.Add(new AreaShapeFile(reader));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return areaShapeFiles;
        }

        public AreaShapeFile(int id)
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

        private AreaShapeFile(NpgsqlDataReader reader)
        {
            Construct(reader);
        }
    }
}
