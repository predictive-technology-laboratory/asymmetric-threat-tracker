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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PTL.ATT.Evaluation;

namespace PTL.ATT.GUI
{
    public partial class PredictionComparisonForm : Form
    {
        public List<Plot> SelectedPlots
        {
            get
            {
                List<Plot> plots = new List<Plot>();
                foreach (FlowLayoutPanel predictionPanel in predictionImageRows.Controls)
                    foreach (CheckedImageBox checkedImageBox in predictionPanel.Controls)
                        if (checkedImageBox.Checked)
                            plots.Add(checkedImageBox.Plot);

                return plots;
            }
        }

        public PredictionComparisonForm(IEnumerable<IEnumerable<Plot>> plotRows, Size size)
        {
            InitializeComponent();
            Size = size;

            foreach (IEnumerable<Plot> plotRow in plotRows)
                if(plotRow.Count() > 0)
                {
                    FlowLayoutPanel panel = new FlowLayoutPanel();
                    panel.FlowDirection = FlowDirection.LeftToRight;
                    panel.AutoScroll = true;
                    panel.WrapContents = false;
                    foreach (Plot plot in plotRow)
                        panel.Controls.Add(new CheckedImageBox(plot, false));

                    panel.Width = predictionImageRows.Width - 50;
                    panel.Height = panel.Controls.Cast<CheckedImageBox>().First().Height + 30;
                    panel.Scroll += new ScrollEventHandler(panel_Scroll);

                    predictionImageRows.Controls.Add(panel);
                }
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

        private void predictionPlots_SizeChanged(object sender, EventArgs e)
        {
            foreach (FlowLayoutPanel panel in predictionImageRows.Controls)
            {
                panel.Width = predictionImageRows.Width - 50;
                panel.Height = panel.Controls.Cast<CheckedImageBox>().First().Height + 30;
            }
        }

        private void panel_Scroll(object sender, ScrollEventArgs args)
        {
            if (ModifierKeys == Keys.Control)
            {
                FlowLayoutPanel scrolledPanel = sender as FlowLayoutPanel;
                float percentScrolled = scrolledPanel.HorizontalScroll.Value / (float)(scrolledPanel.HorizontalScroll.Maximum - scrolledPanel.HorizontalScroll.Minimum);
                foreach (FlowLayoutPanel panel in predictionImageRows.Controls)
                    if (panel != sender)
                        panel.HorizontalScroll.Value = (int)((panel.HorizontalScroll.Maximum - panel.HorizontalScroll.Minimum) * percentScrolled);
            }
        }
    }
}
