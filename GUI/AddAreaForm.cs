using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PTL.ATT.ShapeFiles;

namespace PTL.ATT.GUI
{
    public partial class AddAreaForm : Form
    {
        public AreaShapeFile AreaShapefile
        {
            get { return areas.SelectedItem as AreaShapeFile; }
        }

        public AddAreaForm()
        {
            InitializeComponent();
        }

        private void AddAreaForm_Load(object sender, EventArgs e)
        {
            foreach (AreaShapeFile asf in AreaShapeFile.GetAvailable())
                areas.Items.Add(asf);

            if (areas.Items.Count > 0)
                areas.SelectedIndex = 0;
            else
            {
                MessageBox.Show("No shape files available from which to create area. Import shape files first.");
                cancel_Click(sender, e);
            }
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
