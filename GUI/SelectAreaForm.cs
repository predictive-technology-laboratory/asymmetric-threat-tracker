using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PTL.ATT.GUI
{
    public partial class SelectAreaForm : Form
    {
        public Area SelectedArea
        {
            get { return areas.SelectedItem as Area; }
        }

        public int AreaCount
        {
            get { return areas.Items.Count; }
        }

        public SelectAreaForm(string prompt)
        {
            InitializeComponent();

            Text = prompt;

            foreach (Area area in Area.GetAvailable())
                areas.Items.Add(area);                       

            DialogResult = System.Windows.Forms.DialogResult.Cancel;

            Size = PreferredSize;
        }

        private void ok_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
