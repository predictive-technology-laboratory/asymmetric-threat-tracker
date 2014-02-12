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
