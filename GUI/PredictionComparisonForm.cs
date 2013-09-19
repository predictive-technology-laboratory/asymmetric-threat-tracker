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
