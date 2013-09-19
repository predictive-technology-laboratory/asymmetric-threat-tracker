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

namespace PTL.ATT.GUI
{
    public partial class KernelDensityDcmForm : Form
    {
        public string ModelName
        {
            get { return modelName.Text; }
        }

        public int PointSpacing
        {
            get { return (int)pointSpacing.Value; }
        }

        public int TrainingSampleSize
        {
            get { return (int)trainingSampleSize.Value; }
        }

        public int PredictionSampleSize
        {
            get { return (int)predictionSampleSize.Value; }
        }

        public bool Normalize
        {
            get { return normalize.Checked; }
        }

        public IEnumerable<Smoother> Smoothers
        {
            get { return smoothers.SelectedItems.Cast<Smoother>().ToArray(); }
        }

        public KernelDensityDcmForm()
        {
            InitializeComponent();

            smoothers.Populate(null);
        }

        public KernelDensityDcmForm(KernelDensityDCM current)
            : this()
        {
            modelName.Text = current.Name;
            pointSpacing.Value = current.PointSpacing;
            trainingSampleSize.Value = current.TrainingSampleSize;
            predictionSampleSize.Value = current.PredictionSampleSize;
            normalize.Checked = current.Normalize;
            smoothers.Populate(current);
        }

        private void ok_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
    }
}
