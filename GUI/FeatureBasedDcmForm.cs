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
using PTL.ATT.Smoothers;
using PTL.ATT.Models;
using PTL.ATT.Classifiers;

namespace PTL.ATT.GUI
{
    public partial class FeatureBasedDcmForm : Form
    {
        private FeatureBasedDCM _resultingModel;

        public FeatureBasedDCM ResultingModel
        {
            get { return _resultingModel; }
        }

        
        public FeatureBasedDcmForm()
        {
            InitializeComponent();

            discreteChoiceModelOptions.trainingAreas.SelectedValueChanged += (o, e) =>
                {
                    featureBasedDcmOptions.TrainingArea = discreteChoiceModelOptions.TrainingArea;
                };

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

            _resultingModel = featureBasedDcmOptions.FeatureBasedDCM;
            if (_resultingModel == null)
                _resultingModel = new FeatureBasedDCM();

            discreteChoiceModelOptions.CommitValues(_resultingModel);
            featureBasedDcmOptions.CommitValues(_resultingModel);

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
