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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace PTL.ATT.GUI
{
    public partial class FeatureRemappingForm : Form
    {
        public FeatureRemappingForm()
        {
            InitializeComponent();
        }

        public FeatureRemappingForm(IEnumerable<Feature> currentFeatures, IEnumerable<Feature> availableFeatures)
            : this()
        {
            foreach (Feature f in currentFeatures)
                current.Items.Add(f);

            foreach (Feature f in availableFeatures)
                available.Items.Add(f);
        }

        private void reset_Click(object sender, EventArgs e)
        {
            foreach (Feature f in current.Items)
                f.PredictionResourceId = f.TrainingResourceId;

            RefreshItems();
        }

        private void ok_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void available_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (available.SelectedItem != null)
                foreach (Feature f in current.SelectedItems)
                    f.PredictionResourceId = (available.SelectedItem as Feature).TrainingResourceId;

            RefreshItems();
        }

        private void RefreshItems()
        {
            typeof(ListBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, current, new object[] { });
        }
    }
}
