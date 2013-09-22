#region copyright
//    Copyright 2013 Matthew S. Gerber (gerber.matthew@gmail.com)
//
//    This file is part of the Asymmetric Threat Tracker (ATT).
//
//    The ATT is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    The ATT is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with the ATT.  If not, see <http://www.gnu.org/licenses/>.
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

namespace PTL.ATT.GUI
{
    public partial class SmootherList : ListBox
    {
        public SmootherList()
        {
            InitializeComponent();
        }

        public void Populate(DiscreteChoiceModel m)
        {
            Items.Clear();

            if(m != null)
                foreach (Smoother s in m.Smoothers)
                {
                    Items.Add(s);
                    SetSelected(Items.IndexOf(s), true);
                }

            foreach (Smoother available in Smoother.Available)
                if (Items.Cast<Smoother>().Count(present => present.GetType().Equals(available.GetType())) == 0)
                    Items.Add(available);
        }

        private void SmootherList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                int clickedIndex = IndexFromPoint(e.Location);
                if (clickedIndex >= 0)
                {
                    Smoother smoother = Items[clickedIndex] as Smoother;
                    string updateWindowTitle = "Updating " + smoother.GetType().Name + "...";

                    if (smoother is KdeSmoother)
                    {
                        KdeSmoother kdeSmoother = smoother as KdeSmoother;
                        ParameterizeForm form = new ParameterizeForm("Set KDE smoother parameters");
                        form.AddTextBox("Sample size:  ", kdeSmoother.SampleSize.ToString(), "sample_size");
                        form.AddCheckBox("Normalize:  ", System.Windows.Forms.RightToLeft.Yes, kdeSmoother.Normalize, "normalize");
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            kdeSmoother.SampleSize = Convert.ToInt32(form.GetValue("sample_size"));
                            kdeSmoother.Normalize = Convert.ToBoolean(form.GetValue("normalize"));
                        }
                    }
                    else if (smoother is WeightedAverageSmoother)
                    {
                        WeightedAverageSmoother avgSmoother = smoother as WeightedAverageSmoother;
                        ParameterizeForm form = new ParameterizeForm("Set KDE smoother parameters");
                        form.AddTextBox("Minimum:  ", avgSmoother.Minimum.ToString(), "minimum");
                        form.AddTextBox("Maximum:  ", avgSmoother.Maximum.ToString(), "maximum");
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            avgSmoother.Minimum = Convert.ToDouble(form.GetValue("minimum"));
                            avgSmoother.Maximum = Convert.ToDouble(form.GetValue("maximum"));
                        }
                    }
                    else if (smoother is MarsSmoother)
                    {
                        MarsSmoother marsSmoother = smoother as MarsSmoother;
                        ParameterizeForm form = new ParameterizeForm("Set MARS smoother parameters");
                        form.AddTextBox("Number of considered parent terms (-1 for all):  ", marsSmoother.ConsideredParentTerms.ToString(), "parent");
                        form.AddTextBox("Degree of interaction:  ", marsSmoother.InteractionDegree.ToString(), "interaction");
                        form.AddTextBox("Number of knots (-1 for auto):  ", marsSmoother.NumberOfKnots.ToString(), "knots");
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            marsSmoother.ConsideredParentTerms = Convert.ToInt32(form.GetValue("parent"));
                            marsSmoother.InteractionDegree = Convert.ToInt32(form.GetValue("interaction"));
                            marsSmoother.NumberOfKnots = Convert.ToInt32(form.GetValue("knots"));
                        }
                    }
                    else
                        throw new NotImplementedException("Unrecognized smoother type:  " + smoother.GetType().FullName);
                }
            }
        }
    }
}
