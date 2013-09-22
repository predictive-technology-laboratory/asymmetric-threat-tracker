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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PTL.ATT.Models;
using PTL.ATT.Smoothers;
using PTL.ATT.Classifiers;

namespace PTL.ATT.GUI
{
    public partial class ClassifierList : ListBox
    {
        public ClassifierList()
        {
            InitializeComponent();
        }

        public void Populate(SpatialDistanceDCM m)
        {
            Items.Clear();

            if (m != null)
            {
                Items.Add(m.Classifier);
                SetSelected(Items.IndexOf(m.Classifier), true);
            }

            foreach (Classifier available in Classifier.Available)
                if (Items.Cast<Classifier>().Count(present => present.GetType().Equals(available.GetType())) == 0)
                    Items.Add(available);

            if (SelectedItem == null && Items.Count > 0)
                SelectedIndex = 0;
        }

        private void ClassifierList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                int clickedIndex = IndexFromPoint(e.Location);
                if (clickedIndex >= 0)
                {
                    Classifier classifier = Items[clickedIndex] as Classifier;
                    string updateWindowTitle = "Updating " + classifier.GetType().Name + "...";

                    if (classifier is LibLinear)
                    {
                        LibLinear liblinear = classifier as LibLinear;
                        ParameterizeForm form = new ParameterizeForm("Set LibLinear parameters");
                        form.AddCheckBox("Run feature selection:", System.Windows.Forms.RightToLeft.Yes, liblinear.RunFeatureSelection, "run_feature_selection");
                        form.AddDropDown("Positive weighting:", Enum.GetValues(typeof(LibLinear.PositiveClassWeighting)), liblinear.Weighting, "positive_weighting");
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            liblinear.RunFeatureSelection = Convert.ToBoolean(form.GetValue("run_feature_selection"));
                            liblinear.Weighting = (LibLinear.PositiveClassWeighting)form.GetValue("positive_weighting");
                        }
                    }
                    else if (classifier is SvmRank)
                    {
                        SvmRank svmRank = classifier as SvmRank;
                        ParameterizeForm form = new ParameterizeForm("Set SvmRank parameters");
                        form.AddTextBox("c:  ", svmRank.C.ToString(), "c");
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            svmRank.C = Convert.ToSingle(form.GetValue("c"));
                        }
                    }
                }
            }
        }
    }
}
