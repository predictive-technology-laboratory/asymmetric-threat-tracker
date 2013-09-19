using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PTL.ATT.Smoothers;
using PTL.ATT.Models;
using PTL.ATT.Classifiers;

namespace PTL.ATT.GUI
{
    public partial class SpatialDistanceDcmForm : Form
    {
        public string ModelName
        {
            get { return modelName.Text; }
        }

        public int PointSpacing
        {
            get { return (int)pointSpacing.Value; }
        }

        public int FeatureDistanceThreshold
        {
            get { return (int)featureDistanceThreshold.Value; }
        }

        public bool ClassifyNonZeroVectorsUniformly
        {
            get { return classifyNonZeroVectorsUniformly.Checked; }
        }

        public int TrainingSampleSize
        {
            get { return (int)trainingSampleSize.Value; }
        }

        public int PredictionSampleSize
        {
            get { return (int)predictionSampleSize.Value; }
        }

        public Classifier Classifier
        {
            get { return classifiers.SelectedItem as Classifier; }
        }

        public IEnumerable<Smoother> Smoothers
        {
            get { return smoothers.SelectedItems.Cast<Smoother>(); }
        }

        public SpatialDistanceDcmForm()
        {
            InitializeComponent();

            classifiers.Populate(null);
            smoothers.Populate(null);
        }

        public SpatialDistanceDcmForm(SpatialDistanceDCM current)
            : this()
        {
            modelName.Text = current.Name;
            pointSpacing.Value = current.PointSpacing;
            featureDistanceThreshold.Value = current.FeatureDistanceThreshold;
            classifyNonZeroVectorsUniformly.Checked = current.ClassifyNonZeroVectorsUniformly;
            trainingSampleSize.Value = current.TrainingSampleSize;
            predictionSampleSize.Value = current.PredictionSampleSize;

            classifiers.Populate(current);
            smoothers.Populate(current);
        }

        private void ok_Click(object sender, EventArgs e)
        {
            if (classifiers.SelectedItem == null)
                MessageBox.Show("You must select a classifier.");
            else
            {
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
    }
}
