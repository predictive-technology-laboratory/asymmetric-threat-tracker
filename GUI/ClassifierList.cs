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
                    f.AddDropDown("Positive weighting:", Enum.GetValues(typeof(LibLinear.PositiveClassWeighting)), liblinear.Weighting, "positive_weighting");
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
