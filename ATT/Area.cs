#region copyright
// Copyright 2013-2014 The Rector & Visitors of the University of Virginia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Npgsql;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using LAIR.ResourceAPIs.PostgreSQL;
using LAIR.Collections.Generic;
using PTL.ATT.Models;

namespace PTL.ATT
{
    [Serializable]
    public class Area
    {
        #region static members
        internal const string Table = "area";

        internal class Columns
        {
            [Reflector.Select(true)]
            internal const string Id = "id";
            [Reflector.Insert, Reflector.Select(true)]
            internal const string Name = "name";
            [Reflector.Insert, Reflector.Select(true)]
            internal const string ShapefileId = "shapefile_id";

            internal static string Insert { get { return Reflector.GetInsertColumns(typeof(Columns)); } }
            internal static string Select { get { return Reflector.GetSelectColumns(Table, typeof(Columns)); } }
        }

        [ConnectionPool.CreateTable(typeof(Shapefile))]
        private static string CreateTable(ConnectionPool connection)
        {
            return "CREATE TABLE IF NOT EXISTS " + Table + " (" +
                   Columns.Id + " SERIAL PRIMARY KEY," +
                   Columns.Name + " VARCHAR," +
                   Columns.ShapefileId + " INTEGER REFERENCES " + Shapefile.Table + " ON DELETE CASCADE);";
        }

        public static Area Create(Shapefile shapefile, string name, int pointContainmentBoundingBoxSize)
        {
            Area area = null;
            try
            {
                area = new Area(Convert.ToInt32(DB.Connection.ExecuteScalar("INSERT INTO " + Area.Table + " (" + Columns.Insert + ") VALUES ('" + name + "'," + shapefile.Id + ") RETURNING " + Columns.Id)));

                AreaBoundingBoxes.Create(area, pointContainmentBoundingBoxSize);

                return area;
            }
            catch (Exception ex)
            {
                try { area.Delete(); }
                catch (Exception ex2) { Console.Out.WriteLine("Failed to delete area:  " + ex2.Message); }

                throw ex;
            }
        }

        public static List<Area> GetAll()
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

        public static List<Area> GetForSRID(int srid)
        {
            if (srid < 0)
                throw new ArgumentException("Invalid SRID:  " + srid + ". Must be >= 0.", "srid");

            return GetAll().Where(a => a.Shapefile.SRID == srid).ToList();
        }

        public static List<Area> GetForShapefile(Shapefile shapefile)
        {
            return GetAll().Where(a => a.Shapefile.Id == shapefile.Id).ToList();
        }
        #endregion

        private int _id;
        private string _name;
        private PostGIS.Polygon _boundingBox;
        private Shapefile _shapefile;

        public int Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public PostGIS.Polygon BoundingBox
        {
            get { return _boundingBox; }
        }

        public Shapefile Shapefile
        {
            get { return _shapefile; }
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
            _shapefile = new Shapefile(Convert.ToInt32(reader[Table + "_" + Columns.ShapefileId]));

            NpgsqlCommand cmd = DB.Connection.NewCommand("SELECT " +
                                                         "st_xmin(" + ShapefileGeometry.Columns.Geometry + ") as left," +
                                                         "st_xmax(" + ShapefileGeometry.Columns.Geometry + ") as right," +
                                                         "st_ymin(" + ShapefileGeometry.Columns.Geometry + ") as bottom," +
                                                         "st_ymax(" + ShapefileGeometry.Columns.Geometry + ") as top " +
                                                         "FROM " + _shapefile.GeometryTable);
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
                               new PostGIS.Point(left, top, _shapefile.SRID),
                               new PostGIS.Point(right, top, _shapefile.SRID),
                               new PostGIS.Point(right, bottom, _shapefile.SRID),
                               new PostGIS.Point(left, bottom, _shapefile.SRID),
                               new PostGIS.Point(left, top, _shapefile.SRID)}), _shapefile.SRID), _shapefile.SRID);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Area))
                return false;

            Area other = obj as Area;

            return _id == other.Id;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
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
                   indent + "Shapefile:  " + _shapefile;
        }

        public IEnumerable<int> Intersects(IEnumerable<PostGIS.Point> points)
        {
            return Intersects(points, -1);
        }

        public IEnumerable<int> Intersects(IEnumerable<PostGIS.Point> points, float pointBoundingBoxSize)
        {
            if (points.Count() == 0)
                return new int[0];

            string entityType = pointBoundingBoxSize > 0 ? "POLYGON" : "POINT";

            NpgsqlCommand cmd = DB.Connection.NewCommand("CREATE TABLE temp (" +
                                                         "entity GEOMETRY(" + entityType + "," + _shapefile.SRID + ")," +
                                                         "num INT);" +
                                                         "CREATE INDEX ON temp USING GIST(entity);");
            cmd.ExecuteNonQuery();

            int pointNum = 0;
            int pointsPerBatch = 5000;
            StringBuilder cmdText = new StringBuilder();
            foreach (PostGIS.Point point in points)
            {
                if (point.SRID != _shapefile.SRID)
                    throw new Exception("Area SRID (" + _shapefile.SRID + ") does not match point SRID (" + point.SRID);

                string entity = pointBoundingBoxSize > 0 ? "st_expand(" + point.StGeometryFromText + "," + pointBoundingBoxSize + ")" : point.StGeometryFromText;

                cmdText.Append((cmdText.Length == 0 ? "INSERT INTO temp VALUES" : ",") + " (" + entity + "," + pointNum++ + ")");
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

            cmd.CommandText = "SELECT num " +
                              "FROM temp " +
                              "WHERE " + GetIntersectsCondition("temp.entity");

            List<int> intersectedPointIndices = new List<int>(pointNum);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                intersectedPointIndices.Add(Convert.ToInt32(reader["num"]));
            reader.Close();

            cmd.CommandText = "DROP TABLE temp";
            cmd.ExecuteNonQuery();
            DB.Connection.Return(cmd.Connection);

            return intersectedPointIndices;
        }

        internal void Delete()
        {
            try { DB.Connection.ExecuteNonQuery("DELETE FROM " + Table + " WHERE " + Columns.Id + "=" + _id); }
            catch (Exception ex) { Console.Out.WriteLine("Error deleting area:  " + ex.Message); }

            try { DB.Connection.ExecuteNonQuery("DROP TABLE " + AreaBoundingBoxes.GetTableName(this)); }
            catch (Exception ex) { Console.Out.WriteLine("Error deleting area bounding boxes:  " + ex.Message); }

            try { DB.Connection.ExecuteNonQuery("DROP TABLE " + Incident.GetTableName(this, true)); }
            catch (Exception ex) { Console.Out.WriteLine("Error deleting incident table:  " + ex.Message); }
        }

        internal string GetIntersectsCondition(string entityColumn)
        {
            string areaBoundingBoxesTable = AreaBoundingBoxes.GetTableName(this);
            string areaBoundingBoxesRelationshipColumn = areaBoundingBoxesTable + "." + AreaBoundingBoxes.Columns.Relationship;

            return "EXISTS (SELECT 1 " +
                           "FROM " + areaBoundingBoxesTable + "," + _shapefile.GeometryTable + " " +
                           "WHERE st_intersects(" + entityColumn + "," + areaBoundingBoxesTable + "." + AreaBoundingBoxes.Columns.BoundingBox + ") AND " +
                                  "(" +
                                    areaBoundingBoxesRelationshipColumn + "='" + AreaBoundingBoxes.Relationship.Within + "' OR " +
                                    "(" +
                                      areaBoundingBoxesRelationshipColumn + "='" + AreaBoundingBoxes.Relationship.Overlaps + "' AND " +
                                      "st_intersects(" + entityColumn + "," + _shapefile.GeometryTable + "." + ShapefileGeometry.Columns.Geometry + ")" + // this is the slow operation, so we're hiding it behind simpler checks as much as possible
                                    ")" +
                                  ")" +
                           ")";
        }
    }
}
