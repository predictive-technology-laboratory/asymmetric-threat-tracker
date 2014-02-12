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
