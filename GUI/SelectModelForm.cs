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
    public partial class SelectModelForm : Form
    {
        public Type Type
        {
            get
            {
                if (spatialDistanceDCM.Checked)
                    return typeof(SpatialDistanceDCM);
                else if (timeSliceDCM.Checked)
                    return typeof(TimeSliceDCM);
                else if (kdeDCM.Checked)
                    return typeof(KernelDensityDCM);
                else
                    throw new Exception("Unknown model selection");
            }
        }

        public SelectModelForm()
        {
            InitializeComponent();
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
