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
using LAIR.Extensions;
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
        private IEnumerable<IEnumerable<Plot>> _plotRows;
        public PredictionComparisonForm(IEnumerable<IEnumerable<Plot>> plotRows, Size size)
        {
            InitializeComponent();
            Size = size;
            this._plotRows =  plotRows;
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
        private void CombineAndSavePlots(SurveillancePlot[] Plots, string Path)
        {
            StringBuilder comparisonTitle = new StringBuilder();
            Dictionary<string, List<PointF>> seriesPoints = new Dictionary<string, List<PointF>>();

            Array.ForEach(Plots, selectedPlot =>
            {

                string plotTitle = selectedPlot.Title.Replace(Environment.NewLine, " ").RemoveRepeatedWhitespace();
                comparisonTitle.Append((comparisonTitle.Length == 0 ? "Comparison of " : ", ") + plotTitle);
                foreach (string series in selectedPlot.SeriesPoints.Keys)
                    if (series != PTL.ATT.Models.DiscreteChoiceModel.OptimalSeriesName)
                    {
                        string baseSeriesTitle = plotTitle;
                        if (series == PTL.ATT.Models.DiscreteChoiceModel.OptimalSeriesName)
                            baseSeriesTitle = PTL.ATT.Models.DiscreteChoiceModel.OptimalSeriesName + " " + baseSeriesTitle;

                        string seriesTitle = baseSeriesTitle;
                        int dupNameNum = 2;
                        while (seriesPoints.Keys.Count(k => k == seriesTitle) > 0)
                            seriesTitle = baseSeriesTitle + " " + dupNameNum++;

                        seriesPoints.Add(seriesTitle, selectedPlot.SeriesPoints[series]);

                    }
            });
            SurveillancePlot comparisonPlot = new SurveillancePlot(comparisonTitle.ToString(), -1, seriesPoints, 500, 500, Plot.Format.JPEG, 2);
            comparisonPlot.Image.Save(String.Format("{0}\\{1}.jpeg",Path ,comparisonTitle.Replace(':','_').Replace('/','_') ));
        }
        private void _ButtonSaveOneToOneImages_Click(object sender, EventArgs e)
        {
            if (_FolderBrowserDialogSaveImages.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    int MinNumberOfPlotsPerRow = (from plotrow in _plotRows select plotrow.Count()).Min();
                    for (int i = 0; i < MinNumberOfPlotsPerRow; i++)
                    {
                        SurveillancePlot[] plots = new SurveillancePlot[_plotRows.Count()];
                        for (int j = 0; j < _plotRows.Count(); j++)
                            plots[j] = (SurveillancePlot)_plotRows.ElementAt(j).ElementAt(i);
                        CombineAndSavePlots(plots, _FolderBrowserDialogSaveImages.SelectedPath);
                    }
                    System.Windows.Forms.MessageBox.Show("Images are saved!", "Save 1-to-1", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
