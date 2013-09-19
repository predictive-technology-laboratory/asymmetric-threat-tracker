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
