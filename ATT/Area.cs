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
using PTL.ATT.ShapeFiles;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using LAIR.ResourceAPIs.PostgreSQL;
using LAIR.Collections.Generic;
using PTL.ATT.Models;

namespace PTL.ATT
{
    public class Area
    {
        internal const string Table = "area";

        internal class Columns
        {
            [Reflector.Select(true)]
            internal const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            internal const string Name = "name";
            [Reflector.Insert, Reflector.Select(true)]
            internal const string SRID = "srid";

            internal static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            internal static string Select { get { return Reflector.GetSelectColumns(Table, typeof(Columns)); } }
        }

        [ConnectionPool.CreateTable]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Id + " SERIAL PRIMARY KEY," +
                   Columns.Name + " VARCHAR," +
                   Columns.SRID + " INTEGER);";
        }

        public static int Create(Shapefile shapefile, string name)
        {
            int areaId = -1;
            try
            {
                areaId = Convert.ToInt32(DB.Connection.ExecuteScalar("INSERT INTO " + Area.Table + " (" + Columns.Insert + ") VALUES ('" + name + "'," + shapefile.SRID + ") RETURNING " + Columns.Id));

                Console.Out.WriteLine("Creating area geometry");
                AreaGeometry.Create(shapefile, areaId);

                Console.Out.WriteLine("Creating area bounding boxes");
                AreaBoundingBoxes.Create(areaId, shapefile.SRID, Configuration.AreaBoundingBoxSize);

                return areaId;
            }
            catch (Exception ex)
            {
                try { DB.Connection.ExecuteNonQuery("DELETE FROM " + Table + " WHERE " + Columns.Id + "=" + areaId); }
                catch { }
                throw ex;
            }
        }

        public static IEnumerable<Area> GetAvailable()
        {
            List<Area> areas = new List<Area>();
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Table);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                areas.Add(new Area(reader));

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            return areas;
        }

        private int _id;
        private string _name;
        private int _srid;
        private PostGIS.Polygon _boundingBox;

        public int Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public int SRID
        {
            get { return _srid; }
        }

        public PostGIS.Polygon BoundingBox
        {
            get { return _boundingBox; }
        }

        public Area(int id)
        {
            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " + Columns.Select + " FROM " + Area.Table + " WHERE " + Columns.Id + "=" + id);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            reader.Read();
            Construct(reader);
            reader.Close();
            DB.Connection.Return(cmd.Connection);
        }

        private Area(NpgsqlDataReader reader)
        {
            Construct(reader);
        }

        private void Construct(NpgsqlDataReader reader)
        {
            _id = Convert.ToInt32(reader[Table + "_" + Columns.Id]);
            _name = Convert.ToString(reader[Table + "_" + Columns.Name]);
            _srid = Convert.ToInt32(reader[Table + "_" + Columns.SRID]);

            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " +
                                                         "st_xmin(" + AreaGeometry.Columns.Geometry + ") as left," +
                                                         "st_xmax(" + AreaGeometry.Columns.Geometry + ") as right," +
                                                         "st_ymin(" + AreaGeometry.Columns.Geometry + ") as bottom," +
                                                         "st_ymax(" + AreaGeometry.Columns.Geometry + ") as top " +
                                                         "FROM " + AreaGeometry.GetTableName(_srid));
            reader = cmd.ExecuteReader();

            double left = double.MaxValue;
            double right = double.MinValue;
            double bottom = double.MaxValue;
            double top = double.MinValue;
            while (reader.Read())
            {
                double l = Convert.ToDouble(reader["left"]);
                if (l < left)
                    left = l;

                double r = Convert.ToDouble(reader["right"]);
                if (r > right)
                    right = r;

                double b = Convert.ToDouble(reader["bottom"]);
                if (b < bottom)
                    bottom = b;

                double t = Convert.ToDouble(reader["top"]);
                if (t > top)
                    top = t;
            }

            reader.Close();
            DB.Connection.Return(cmd.Connection);

            _boundingBox = new PostGIS.Polygon(new PostGIS.LineString(new List<PostGIS.Point>(new PostGIS.Point[]{
                               new PostGIS.Point(left, top, _srid),
                               new PostGIS.Point(right, top, _srid),
                               new PostGIS.Point(right, bottom, _srid),
                               new PostGIS.Point(left, bottom, _srid),
                               new PostGIS.Point(left, top, _srid)}), _srid), _srid);
        }

        public void Delete()
        {
            foreach (DiscreteChoiceModel model in DiscreteChoiceModel.GetForArea(this))
                if (model.HasMadePredictions)
                    throw new Exception("Predictions have been made based on the given area. Cannot delete it.");

            DB.Connection.ExecuteNonQuery("DELETE FROM " + Table + " WHERE " + Columns.Id + "=" + _id);
        }

        public override string ToString()
        {
            return _name;
        }

        public string GetDetails(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; ++i)
                indent += "\t";

            return (indentLevel > 0 ? Environment.NewLine : "") + indent + "ID:  " + _id + Environment.NewLine +
                   indent + "Name:  " + _name + Environment.NewLine +
                   indent + "Bounding box:  " + _boundingBox.LowerLeft + "," + _boundingBox.UpperLeft + "," + _boundingBox.UpperRight + "," + _boundingBox.LowerRight + Environment.NewLine +
                   indent + "SRID:  " + _srid;
        }

        public IEnumerable<int> Contains(IEnumerable<PostGIS.Point> points)
        {
            if (points.Count() == 0)
                return new int[0];

            NpgsqlCommand cmd = DB.Connection.NewCommand("CREATE TABLE temp (" +
                                                         "point GEOMETRY(POINT," + _srid + ")," +
                                                         "num INT);" +
                                                         "CREATE INDEX ON temp USING GIST(point);");
            cmd.ExecuteNonQuery();

            int pointNum = 0;
            int pointsPerBatch = 5000;
            StringBuilder cmdText = new StringBuilder();
            foreach (PostGIS.Point point in points)
            {
                if (point.SRID != _srid)
                    throw new Exception("Area SRID (" + _srid + ") does not match point SRID (" + point.SRID);

                cmdText.Append((cmdText.Length == 0 ? "INSERT INTO temp VALUES" : ",") + " (" + point.StGeometryFromText + "," + pointNum++ + ")");
                if ((pointNum % pointsPerBatch) == 0)
                {
                    cmd.CommandText = cmdText.ToString();
                    cmd.ExecuteNonQuery();
                    cmdText.Clear();
                }
            }

            if (cmdText.Length > 0)
            {
                cmd.CommandText = cmdText.ToString();
                cmd.ExecuteNonQuery();
                cmdText.Clear();
            }

            string areaGeometryTable = AreaGeometry.GetTableName(_srid);
            string areaBoundingBoxesTable = AreaBoundingBoxes.GetTableName(_srid);

            cmd.CommandText = "SELECT num " +
                              "FROM temp " +
                              "WHERE EXISTS (SELECT 1 " +
                                                "FROM " + areaGeometryTable + "," + areaBoundingBoxesTable + " " +
                                                "WHERE " + areaGeometryTable + "." + AreaGeometry.Columns.AreaId + "=" + _id + " AND " +
                                                           areaBoundingBoxesTable + "." + AreaBoundingBoxes.Columns.AreaId + "=" + _id + " AND " +
                                                           "(" +
                                                             "(" +
                                                                areaBoundingBoxesTable + "." + AreaBoundingBoxes.Columns.Relationship + "='" + AreaBoundingBoxes.Relationship.Within + "' AND " +
                                                                "st_intersects(temp.point," + areaBoundingBoxesTable + "." + AreaBoundingBoxes.Columns.BoundingBox + ")" +
                                                             ") " +
                                                             "OR " +
                                                             "(" +
                                                                areaBoundingBoxesTable + "." + AreaBoundingBoxes.Columns.Relationship + "='" + AreaBoundingBoxes.Relationship.Overlaps + "' AND " +
                                                                "st_intersects(temp.point," + areaBoundingBoxesTable + "." + AreaBoundingBoxes.Columns.BoundingBox + ") AND " +
                                                                "st_intersects(temp.point," + areaGeometryTable + "." + AreaGeometry.Columns.Geometry + ")" +
                                                             ")" +
                                                           ")" +
                                            ")";

            List<int> containedPointIndices = new List<int>(pointNum);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                containedPointIndices.Add(Convert.ToInt32(reader["num"]));
            reader.Close();

            cmd.CommandText = "DROP TABLE temp";
            cmd.ExecuteNonQuery();
            DB.Connection.Return(cmd.Connection);

            return containedPointIndices;
        }
    }
}
