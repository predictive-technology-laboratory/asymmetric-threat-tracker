using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PTL.ATT.Evaluation;

namespace PTL.ATT.GUI
{
    public partial class CheckedImageBox : UserControl
    {
        private Plot _plot;

        public bool Checked
        {
            get { return checkBox.Checked; }
        }

        public Plot Plot
        {
            get { return _plot; }
        }

        public CheckedImageBox(Plot plot, bool check)
        {
            InitializeComponent();

            _plot = plot;

            image.Image = _plot.Image;
            checkBox.Checked = check;
            Size = new System.Drawing.Size(_plot.Image.Width, _plot.Image.Height + checkBox.Height + 10);
        }

        private void image_Click(object sender, EventArgs e)
        {
            checkBox.Checked = !checkBox.Checked;
        }

        private void CheckedImageBox_Click(object sender, EventArgs e)
        {

            checkBox.Checked = !checkBox.Checked;
        }
    }
}
