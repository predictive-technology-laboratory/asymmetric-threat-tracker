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
                    if (smoother == null)
                        throw new NullReferenceException("Expected Smoother objects in Items");

                    string updateWindowTitle = "Updating " + smoother.GetType().Name + "...";

                    if (smoother is KdeSmoother)
                    {
                        KdeSmoother kdeSmoother = smoother as KdeSmoother;
                        DynamicForm f = new DynamicForm("Set KDE smoother parameters");
                        f.AddNumericUpdown("Sample size:", kdeSmoother.SampleSize, 0, 1, decimal.MaxValue, 1, "sample_size");
                        f.AddCheckBox("Normalize:", ContentAlignment.MiddleRight, kdeSmoother.Normalize, "normalize");
                        if (f.ShowDialog() == DialogResult.OK)
                        {
                            try { kdeSmoother.SampleSize = Convert.ToInt32(f.GetValue<decimal>("sample_size")); }
                            catch (Exception ex) { MessageBox.Show("Invalid value for sample size:  " + ex.Message); }

                            kdeSmoother.Normalize = f.GetValue<bool>("normalize");
                        }
                    }
                    else if (smoother is WeightedAverageSmoother)
                    {
                        WeightedAverageSmoother avgSmoother = smoother as WeightedAverageSmoother;
                        DynamicForm f = new DynamicForm("Set weighted average smoother parameters");
                        f.AddNumericUpdown("Minimum:", (decimal)avgSmoother.Minimum, 0, 0, decimal.MaxValue, 1, "minimum");
                        f.AddNumericUpdown("Maximum:", (decimal)avgSmoother.Maximum, 0, 0, decimal.MaxValue, 1, "maximum");
                        if (f.ShowDialog() == DialogResult.OK)
                        {
                            try { avgSmoother.Minimum = Convert.ToDouble(f.GetValue<decimal>("minimum")); }
                            catch (Exception ex) { MessageBox.Show("Invalid value for minimum:  " + ex.Message); }

                            try
                            {
                                double value = Convert.ToDouble(f.GetValue<decimal>("maximum"));
                                if (value < avgSmoother.Minimum)
                                {
                                    avgSmoother.Maximum = avgSmoother.Minimum + 500;
                                    throw new Exception("Maximum must be greater than or equal to minimum (" + avgSmoother.Minimum + "). Setting maximum to " + avgSmoother.Maximum + ".");
                                }

                                avgSmoother.Maximum = value;
                            }
                            catch (Exception ex) { MessageBox.Show("Invalid value for maximum:  " + ex.Message); }
                        }
                    }
                    else if (smoother is MarsSmoother)
                    {
                        MarsSmoother marsSmoother = smoother as MarsSmoother;
                        DynamicForm f = new DynamicForm("Set MARS smoother parameters");
                        f.AddNumericUpdown("Number of considered parent terms (-1 for all):", marsSmoother.ConsideredParentTerms, 0, -1, decimal.MaxValue, 1, "parent");
                        f.AddNumericUpdown("Degree of interaction:", marsSmoother.InteractionDegree, 0, 1, decimal.MaxValue, 1, "interaction");
                        f.AddNumericUpdown("Number of knots (-1 for auto):", marsSmoother.NumberOfKnots, 0, -1, decimal.MaxValue, 1, "knots");
                        if (f.ShowDialog() == DialogResult.OK)
                        {
                            try { marsSmoother.ConsideredParentTerms = Convert.ToInt32(f.GetValue<decimal>("parent")); }
                            catch (Exception ex) { MessageBox.Show("Invalid value for parent terms:  " + ex.Message); }

                            try { marsSmoother.InteractionDegree = Convert.ToInt32(f.GetValue<decimal>("interaction")); }
                            catch (Exception ex) { MessageBox.Show("Invalid value for interaction degree:  " + ex.Message); }

                            try { marsSmoother.NumberOfKnots = Convert.ToInt32(f.GetValue<decimal>("knots")); }
                            catch (Exception ex) { MessageBox.Show("Invalid value for number of knots:  " + ex.Message); }
                        }
                    }
                    else
                        throw new NotImplementedException("Unrecognized smoother type:  " + smoother.GetType().FullName);
                }
            }
        }
    }
}
