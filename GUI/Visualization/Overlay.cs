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
using System.Drawing;

namespace PTL.ATT.GUI.Visualization
{
    public class Overlay : IComparable<Overlay>
    {
        private string _name;
        private List<List<PointF>> _points;
        private Color _color;
        private bool _displayed;
        private int _viewPriority;

        public int ViewPriority
        {
            get { return _viewPriority; }
            set { _viewPriority = value; }
        }

        public string Name
        {
            get { return _name; }
        }

        public List<List<PointF>> Points
        {
            get { return _points; }
        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public bool Displayed
        {
            get { return _displayed; }
            set { _displayed = value; }
        }

        public Overlay(string name, List<List<PointF>> points, Color color, bool displayed, int viewPriority)
        {
            _name = name;
            _points = points;
            _color = color;
            _displayed = displayed;
            _viewPriority = viewPriority;
        }

        public int CompareTo(Overlay other)
        {
            return _viewPriority.CompareTo(other.ViewPriority);
        }
    }
}
