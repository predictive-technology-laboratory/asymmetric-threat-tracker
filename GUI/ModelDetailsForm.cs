using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace PTL.ATT.GUI
{
    public partial class ModelDetailsForm : Form
    {
        public ModelDetailsForm(string title, string text)
        {
            InitializeComponent();

            Text = title;
            modelDetails.Text = text;
        }

        private void ModelDetailsForm_Load(object sender, EventArgs e)
        {
            modelDetails.SelectAll();
            modelDetails.DeselectAll();
        }

        private void save_Click(object sender, EventArgs e)
        {
            string path = LAIR.IO.File.PromptForSavePath("Select save path...");
            if (path != null)
            {
                StreamWriter file = new StreamWriter(path);
                file.Write(modelDetails.Text);
                file.Close();
            }
        }

        private void close_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
