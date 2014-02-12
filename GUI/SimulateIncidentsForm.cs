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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
