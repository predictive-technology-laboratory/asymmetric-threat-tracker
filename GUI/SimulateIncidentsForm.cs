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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PTL.ATT.Incidents;

namespace PTL.ATT.GUI
{
    public partial class SimulateIncidentsForm : Form
    {
        private Area _area;

        public SimulateIncidentsForm(Area area)
        {
            InitializeComponent();

            simulateStart.Value = DateTime.Today.Add(new TimeSpan(-1, 0, 0, 0));
            simulateEnd.Value = simulateStart.Value.Add(new TimeSpan(23, 59, 59));

            _area = area;
        }

        private void simulateStart_ValueChanged(object sender, EventArgs e)
        {
            if (simulateStart.Value > simulateEnd.Value)
                simulateEnd.Value = simulateStart.Value.Add(new TimeSpan(23, 59, 59));
        }

        private void simulateEnd_ValueChanged(object sender, EventArgs e)
        {
            if (simulateStart.Value > simulateEnd.Value)
                simulateStart.Value = simulateEnd.Value.Add(new TimeSpan(-23, -59, -59));
        }

        private void simulateIncidents_Click(object sender, EventArgs e)
        {
            Incident.Simulate(_area, new string[] { "A", "B", "C", "D", "E" }, simulateStart.Value, simulateEnd.Value, (int)simulateN.Value);
            MessageBox.Show("Simulation successful.");
        }

        private void close_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
