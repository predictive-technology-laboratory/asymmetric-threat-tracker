using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using PTL.ATT.PostGIS.Geometry;

namespace PTL.ATT
{
    public class Grid
    {
        public const string Table = "grid";

        public class Columns
        {

        }

        





        public static IEnumerable<Grid> GetAvailable()
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.All + " FROM " + Table);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                yield return new Grid(reader);

            cmd.Connection.Close();
        }

        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private int _aoId;

        public int AoId
        {
            get { return _aoId; }
            set { _aoId = value; }
        }
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private double _cellSize;

        public double CellSize
        {
            get { return _cellSize; }
            set { _cellSize = value; }
        }

        public IEnumerable<GridPoint> Points
        {
            get
            {
                NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + GridPoint.Columns.Id + " as id," +
                                                             "st_x(" + GridPoint.Columns.Location + ") as x," +
                                                             "st_y(" + GridPoint.Columns.Location + ") as y," +
                                                             "st_srid(" + GridPoint.Columns.Location + ") as srid FROM " + GridPoint.Table + " WHERE " + GridPoint.Columns.GridId + "=" + _id);

                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    yield return new GridPoint(Convert.ToInt32(reader["id"]), this, new Point(Convert.ToDouble(reader["x"]), Convert.ToDouble(reader["y"]), Convert.ToInt32(reader["srid"])));

                cmd.Connection.Close();
            }
        }

        public Grid(int id)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.All + " FROM " + Grid.Table + " WHERE " + Columns.Id + "=" + id);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            Construct(reader);
            cmd.Connection.Close();
        }

        public Grid(NpgsqlDataReader reader)
        {
            Construct(reader);
        }

        private void Construct(NpgsqlDataReader reader)
        {
            _id = Convert.ToInt32(reader[Columns.Id]);
            _aoId = Convert.ToInt32(reader[Columns.AoId]);
            _name = Convert.ToString(reader[Columns.Name]);
            _cellSize = Convert.ToDouble(reader[Columns.CellSize]);
        }

        public override string ToString()
        {
            return _id + ":  " + _name;
        }

        public string Details()
        {
            return "ID:  " + _id + Environment.NewLine +
                   "Name:  " + _name + Environment.NewLine +
                   "AO:  " + _aoId + Environment.NewLine +
                   "Cell size:  " + _cellSize;
        }

        public void Delete()
        {
            DB.Connection.ExecuteNonQuery("DELETE FROM " + Table + " WHERE " + Columns.Id + "=" + _id);
        }
    }
}
