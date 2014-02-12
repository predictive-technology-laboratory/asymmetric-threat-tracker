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
                using (StreamWriter file = new StreamWriter(path))
                {
                    file.Write(modelDetails.Text);
                    file.Close();
                }
            }
        }

        private void close_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
