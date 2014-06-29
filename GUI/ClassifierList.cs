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

        public void Populate(FeatureBasedDCM m)
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
            if (e.Button == System.Windows.Forms.MouseButtons.Right && SelectedItem != null)
            {
                Classifier classifier = SelectedItem as Classifier;
                if (classifier == null)
                    throw new NullReferenceException("Failed to cast classifier item from list");

                string updateWindowTitle = "Updating " + classifier.GetType().Name + "...";

                if (classifier is LibLinear)
                {
                    LibLinear liblinear = classifier as LibLinear;
                    DynamicForm f = new DynamicForm("Set LibLinear parameters");
                    f.AddCheckBox("Run feature selection:", ContentAlignment.MiddleRight, liblinear.RunFeatureSelection, "run_feature_selection");
                    f.AddDropDown("Positive weighting:", Enum.GetValues(typeof(LibLinear.PositiveClassWeighting)), liblinear.Weighting, "positive_weighting", true);
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        liblinear.RunFeatureSelection = f.GetValue<bool>("run_feature_selection");
                        liblinear.Weighting = f.GetValue<LibLinear.PositiveClassWeighting>("positive_weighting");
                    }
                }
                else if (classifier is SvmRank)
                {
                    SvmRank svmRank = classifier as SvmRank;
                    DynamicForm f = new DynamicForm("Set SvmRank parameters");
                    f.AddNumericUpdown("c:", (decimal)svmRank.C, 3, decimal.MinValue, decimal.MaxValue, (decimal)0.01, "c");
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        try { svmRank.C = Convert.ToSingle(f.GetValue<decimal>("c")); }
                        catch (Exception ex) { MessageBox.Show("Invalid value for C:  " + ex.Message); }
                    }
                }
                else if (classifier is RandomForest)
                {
                    RandomForest randomForest = classifier as RandomForest;
                    DynamicForm f = new DynamicForm("Set RandomForest parameters");
                    f.AddNumericUpdown("Number of trees:", randomForest.NumTrees, 0, 1, decimal.MaxValue, 1, "ntree");
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        randomForest.NumTrees = Convert.ToInt32(f.GetValue<decimal>("ntree"));
                    }
                }
                else if (classifier is AdaBoost)
                {
                    AdaBoost adaBoost = classifier as AdaBoost;
                    DynamicForm f = new DynamicForm("Set AdaBoost parameters");
                    f.AddNumericUpdown("Number of iterations:", adaBoost.Iterations, 0, 1, decimal.MaxValue, 1, "iterations");
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        adaBoost.Iterations = Convert.ToInt32(f.GetValue<decimal>("iterations"));
                    }
                }
            }
        }
    }
}
