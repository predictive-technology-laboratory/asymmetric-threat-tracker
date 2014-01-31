﻿#region copyright
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
using PTL.ATT.Smoothers;
using PTL.ATT.Models;
using PTL.ATT.Classifiers;

namespace PTL.ATT.GUI
{
    public partial class FeatureBasedDcmForm : Form
    {
        public FeatureBasedDcmForm()
        {
            InitializeComponent();

            discreteChoiceModelOptions.trainingAreas.SelectedValueChanged += new EventHandler((o, e) =>
                {
                    featureBasedDcmOptions.TrainingArea = discreteChoiceModelOptions.TrainingArea;
                    featureBasedDcmOptions.RefreshAll();
                });

            featureBasedDcmOptions.GetFeatures = new Func<Area, List<Feature>>(a => FeatureBasedDCM.GetAvailableFeatures(a).ToList());

            discreteChoiceModelOptions.RefreshAreas();
        }

        public FeatureBasedDcmForm(FeatureBasedDCM current)
            : this()
        {
            discreteChoiceModelOptions.DiscreteChoiceModel = featureBasedDcmOptions.FeatureBasedDCM = current;
        }

        private void ok_Click(object sender, EventArgs e)
        {
            string errors = discreteChoiceModelOptions.ValidateInput() + featureBasedDcmOptions.ValidateInput();
            if (errors != "")
            {
                MessageBox.Show(errors);
                return;
            }

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