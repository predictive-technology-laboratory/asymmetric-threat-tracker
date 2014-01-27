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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PTL.ATT.Classifiers;
using PTL.ATT.Models;

namespace PTL.ATT.GUI
{
    public partial class FeatureBasedDcmOptions : UserControl
    {
        private FeatureBasedDCM _featureBasedDCM;
        private Dictionary<string, string> _featureRemapKeyTargetPredictionResource;
        private bool _initializing;
        private Area _trainingArea;
        private Func<Area, List<Feature>> _getFeatures;
        private Feature _parameterizeFeature;

        public int TrainingSampleSize
        {
            get { return (int)trainingSampleSize.Value; }
        }

        public int PredictionSampleSize
        {
            get { return (int)predictionSampleSize.Value; }
        }

        public int FeatureDistanceThreshold
        {
            get { return (int)featureDistanceThreshold.Value; }
        }

        public Classifier Classifier
        {
            get { return classifiers.SelectedItem as Classifier; }
        }

        public List<Feature> Features
        {
            get { return features.SelectedItems.Cast<Feature>().ToList(); }
        }

        public Func<Area, List<Feature>> GetFeatures
        {
            set { _getFeatures = value; }
        }

        public Area TrainingArea
        {
            set { _trainingArea = value; }
        }

        public FeatureBasedDCM FeatureBasedDCM
        {
            get { return _featureBasedDCM; }
            set
            {
                _featureBasedDCM = value;

                if (_featureBasedDCM != null)
                {
                    _featureRemapKeyTargetPredictionResource.Clear();
                    foreach (Feature feature in _featureBasedDCM.Features)
                        if (feature.PredictionResourceId != feature.TrainingResourceId)
                            _featureRemapKeyTargetPredictionResource.Add(feature.RemapKey, feature.PredictionResourceId);

                    RefreshAll();
                }
            }
        }

        public FeatureBasedDcmOptions()
        {
            _initializing = true;
            InitializeComponent();
            _initializing = false;

            _featureRemapKeyTargetPredictionResource = new Dictionary<string, string>();

            RefreshAll();
        }

        public void RefreshAll()
        {
            if (_initializing)
                return;

            trainingSampleSize.Value = 30000;
            predictionSampleSize.Value = 30000;
            featureDistanceThreshold.Value = 1000;
            classifiers.Populate(_featureBasedDCM);

            RefreshFeatures();

            if (_featureBasedDCM != null)
            {
                trainingSampleSize.Value = _featureBasedDCM.TrainingSampleSize;
                predictionSampleSize.Value = _featureBasedDCM.PredictionSampleSize;
                featureDistanceThreshold.Value = _featureBasedDCM.FeatureDistanceThreshold;

                foreach (Feature feature in _featureBasedDCM.Features)
                {
                    int index = features.Items.IndexOf(feature);
                    Feature featureInList = features.Items[index] as Feature;
                    features.SetSelected(index, true);
                    featureInList.ParameterValue = feature.ParameterValue;
                }
            }
        }

        private void RefreshFeatures()
        {
            features.Items.Clear();

            if (_trainingArea != null)
            {
                List<Feature> availableFeatures = _getFeatures(_trainingArea);

                foreach (Feature f in availableFeatures)
                    if (_featureRemapKeyTargetPredictionResource.ContainsKey(f.RemapKey))
                        f.PredictionResourceId = _featureRemapKeyTargetPredictionResource[f.RemapKey];

                features.Items.AddRange(availableFeatures.ToArray());
            }
        }

        #region features
        public void selectAllFeaturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < features.Items.Count; ++i)
                features.SetSelected(i, true);
        }

        private void remapSelectedFeaturesDuringPredictionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Features.Count == 0)
                MessageBox.Show("Must select one or more features before remapping.");
            else
            {
                Area[] predictionAreas = Area.GetAll().ToArray();
                if (predictionAreas.Length == 0)
                    MessageBox.Show("No prediction areas available for remapping.");
                else
                {
                    DynamicForm df = new DynamicForm("Remapping features...");
                    df.AddDropDown("Prediction area:", predictionAreas, null, "prediction_area");
                    if (df.ShowDialog() == DialogResult.OK)
                    {
                        List<Feature> selectedFeatures = Features;
                        FeatureRemappingForm f = new FeatureRemappingForm(selectedFeatures, _getFeatures(df.GetValue<Area>("prediction_area")));
                        f.ShowDialog();

                        _featureRemapKeyTargetPredictionResource.Clear();
                        foreach (Feature feature in selectedFeatures)
                            if (feature.PredictionResourceId != feature.TrainingResourceId)
                                _featureRemapKeyTargetPredictionResource.Add(feature.RemapKey, feature.PredictionResourceId);

                        RefreshFeatures();

                        foreach (Feature feature in selectedFeatures)
                            features.SetSelected(features.Items.IndexOf(feature), true);
                    }
                }
            }
        }
        #endregion

        public string ValidateInput()
        {
            StringBuilder errors = new StringBuilder();

            if (Classifier == null)
                errors.AppendLine("A classifier must be selected.");

            if (Features.Count == 0)
                errors.AppendLine("One or more features must be selected.");

            return errors.ToString();
        }

        private void features_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                int index = features.IndexFromPoint(e.Location);
                if (index != -1)
                {
                    _parameterizeFeature = features.Items[index] as Feature;
                    parameterizeFeatureToolStripMenuItem.Text = "Parameterize \"" + _parameterizeFeature.Description + "\"...";
                    parameterizeFeatureToolStripMenuItem.Enabled = _parameterizeFeature.ParameterValue.Count > 0;
                }
            }
        }

        private void parameterizeFeatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_parameterizeFeature != null && _parameterizeFeature.ParameterValue.Count > 0)
            {
                DynamicForm f = new DynamicForm("Parameterize \"" + _parameterizeFeature.Description + "\"...", MessageBoxButtons.OKCancel);
                foreach (string parameter in _parameterizeFeature.ParameterValue.Keys.OrderBy(k => k))
                    f.AddTextBox(parameter + ":", _parameterizeFeature.ParameterValue[parameter], 20, parameter);

                if (f.ShowDialog() == DialogResult.OK)
                    foreach (string parameter in _parameterizeFeature.ParameterValue.Keys.OrderBy(k => k))
                        _parameterizeFeature.ParameterValue[parameter] = f.GetValue<string>(parameter);
            }
        }
    }
}
