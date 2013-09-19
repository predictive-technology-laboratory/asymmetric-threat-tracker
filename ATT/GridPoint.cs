using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LAIR.MachineLearning;
using PTL.ATT.PostGIS.Geometry;

namespace PTL.ATT
{
    public class GridPoint : ClassifiableEntity
    {
        public const string Table = "grid_point";

        public class Columns
        {
            public const string Id = "id";
            public const string GridId = "grid_id";
            public const string Location = "location";
            public const string All = Id + "," + GridId + "," + Location;
        }

        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        private Grid _grid;

        public Grid Grid
        {
            get { return _grid; }
            set { _grid = value; }
        }
        private Point _location;

        public Point Location
        {
            get { return _location; }
            set { _location = value; }
        }

        public GridPoint(int id, Grid grid, Point location)
        {
            _id = id;
            _grid = grid;
            _location = location;
        }
    }
}
