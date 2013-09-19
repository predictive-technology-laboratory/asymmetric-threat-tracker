using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PTL.ATT.ShapeFiles;
using PTL.ATT.Smoothers;

namespace PTL.ATT.GUI
{
    public partial class SmootherChooser : Form
    {
        public IEnumerable<Smoother> Selected
        {
            get { return smoothers.SelectedItems.Cast<Smoother>(); }
        }

        public SmootherChooser()
        {
            InitializeComponent();

            smoothers.Populate(null);
        }

        private void ok_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
