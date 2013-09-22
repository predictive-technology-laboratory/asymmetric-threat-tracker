#region copyright
// Copyright 2013 
// Predictive Technology Laboratory 
// predictivetech@virginia.edu
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace PTL.ATT.GUI.Visualization
{
    public partial class Visualizer : UserControl
    {
        private Prediction _displayedPrediction;
        private IEnumerable<Overlay> _overlays;

        internal Prediction DisplayedPrediction
        {
            get { return _displayedPrediction; }
        }

        internal IEnumerable<Overlay> Overlays
        {
            get { return _overlays; }
        }

        public Visualizer()
        {
            InitializeComponent();
        }

        public virtual void Display(Prediction prediction, IEnumerable<Overlay> overlays)
        {
            Invoke(new Action(Clear));

            _displayedPrediction = prediction;
            _overlays = overlays;
        }

        public virtual void Clear()
        {
            _displayedPrediction = null;
            _overlays = null;
        }
    }
}
