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
    public partial class SpatialDistanceDcmOptions : UserControl
    {
        private SpatialDistanceDCM _spatialDistanceDCM;
        private Dictionary<string, string> _featureRemapKeyTargetPredictionResource;
        private bool _initializing;
        private Area _trainingArea;
        private Func<Area, List<Feature>> _getFeatures;

        public int FeatureDistanceThreshold
        {
            get { return (int)featureDistanceThreshold.Value; }
        }

        public bool ClassifyNonZeroVectorsUniformly
        {
            get { return classifyNonZeroVectorsUniformly.Checked; }
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

        public SpatialDistanceDCM SpatialDistanceDCM
        {
            get { return _spatialDistanceDCM; }
            set
            {
                _spatialDistanceDCM = value;

                if (_spatialDistanceDCM != null)
                    RefreshAll();
            }
        }

        public SpatialDistanceDcmOptions()
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

            featureDistanceThreshold.Value = 1000;
            classifyNonZeroVectorsUniformly.Checked = false;
            classifiers.Populate(_spatialDistanceDCM);

            RefreshFeatures();

            if (_spatialDistanceDCM != null)
            {
                featureDistanceThreshold.Value = _spatialDistanceDCM.FeatureDistanceThreshold;
                classifyNonZeroVectorsUniformly.Checked = _spatialDistanceDCM.ClassifyNonZeroVectorsUniformly;

                foreach (Feature feature in _spatialDistanceDCM.Features)
                {
                    int index = features.Items.IndexOf(feature);
                    if (index >= 0)
                        features.SetSelected(index, true);
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
                        FeatureRemappingForm f = new FeatureRemappingForm(Features, _getFeatures(df.GetValue<Area>("prediction_area")));
                        f.ShowDialog();

                        _featureRemapKeyTargetPredictionResource.Clear();
                        foreach (Feature feature in features.Items)
                            _featureRemapKeyTargetPredictionResource.Add(feature.RemapKey, feature.PredictionResourceId);

                        RefreshFeatures();
                    }
                }
            }
        }

        private void clearFeatureRemappingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _featureRemapKeyTargetPredictionResource.Clear();
            RefreshFeatures();
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
    }
}
