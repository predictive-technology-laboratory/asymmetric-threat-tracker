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
                if (scrolledPanel == null)
                    throw new ArgumentException("Expected FlowLayoutPanel");

                float percentScrolled = scrolledPanel.HorizontalScroll.Value / (float)(scrolledPanel.HorizontalScroll.Maximum - scrolledPanel.HorizontalScroll.Minimum);
                foreach (FlowLayoutPanel panel in predictionImageRows.Controls)
                    if (panel != sender)
                        panel.HorizontalScroll.Value = (int)((panel.HorizontalScroll.Maximum - panel.HorizontalScroll.Minimum) * percentScrolled);
            }
        }
    }
}
