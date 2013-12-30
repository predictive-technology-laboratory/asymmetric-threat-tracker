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
using PTL.ATT.Models;

namespace PTL.ATT.GUI
{
    public partial class FeatureRemappingForm : Form
    {
        public FeatureRemappingForm()
        {
            InitializeComponent();
        }

        public FeatureRemappingForm(IEnumerable<Feature> trainingFeatures, IEnumerable<Feature> predictionFeatures)
            : this()
        {
            foreach (Feature trainingFeature in trainingFeatures)
                training.Items.Add(trainingFeature);

            foreach (Feature predictionFeature in predictionFeatures)
                prediction.Items.Add(predictionFeature);
        }

        private void reset_Click(object sender, EventArgs e)
        {
            foreach (Feature trainingFeature in training.Items)
                trainingFeature.PredictionResourceId = trainingFeature.TrainingResourceId;

            RefreshItems();
        }

        private void ok_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void available_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (prediction.SelectedItem != null)
            {
                Feature target = prediction.SelectedItem as Feature;
                if (target == null)
                    throw new NullReferenceException("Expected SelectedItem to be a Feature");

                foreach (Feature trainingFeature in training.SelectedItems)
                    if (trainingFeature.EnumType == target.EnumType && trainingFeature.EnumValue.ToString() == target.EnumValue.ToString())
                        trainingFeature.PredictionResourceId = target.TrainingResourceId;
                    else
                        MessageBox.Show("Cannot map incompatible features:  " + trainingFeature + " --> " + target);

                RefreshItems();
            }
        }

        private void RefreshItems()
        {
            typeof(ListBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, training, new object[] { });
        }
    }
}
