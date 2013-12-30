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
                    features.SetSelected(features.Items.IndexOf(feature), true);
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
                MessageBox.Show("Must select features before remapping.");
            else
            {
                DynamicForm df = new DynamicForm("Remapping features...");
                df.AddDropDown("Prediction area:", Area.GetAll().ToArray(), null, "prediction_area");
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

        private void clearFeatureRemappingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _featureRemapKeyTargetPredictionResource.Clear();
            RefreshFeatures();
        }
        #endregion
    }
}
