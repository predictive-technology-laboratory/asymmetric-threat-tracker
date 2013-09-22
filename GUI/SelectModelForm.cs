#region copyright
// Copyright 2013 
// Predictive Technology Laboratory 
// predictivetech@virginia.edu
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
