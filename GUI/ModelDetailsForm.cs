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
