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
using System.Threading;
using Npgsql;

namespace PTL.ATT.GUI
{
    public partial class ImportShapefileForm : Form
    {
        AttForm _attForm;

        public ImportShapefileForm(AttForm attForm)
        {
            InitializeComponent();

            foreach (Shapefile.ShapefileType type in Enum.GetValues(typeof(Shapefile.ShapefileType)))
                shapefileType.Items.Add(type);

            shapefileType.SelectedIndex = 0;

            _attForm = attForm;
        }

        private void close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void shapefileFile_Click(object sender, EventArgs e)
        {
            string file = LAIR.IO.File.PromptForOpenPath("Select shapefile...", Configuration.PostGisShapefileDirectory, "Shapefiles (*.shp)|*.shp");
            if (file != null)
                shapefilePath.Text = file;
        }

        private void shapefileDir_Click(object sender, EventArgs e)
        {
            string dir = LAIR.IO.Directory.PromptForDirectory("Select shapefile directory...", Configuration.PostGisShapefileDirectory);
            if (dir != null)
                shapefilePath.Text = dir;
        }

        private void importShp_Click(object sender, EventArgs e)
        {
            string[] shapefilePaths;
            if (File.Exists(shapefilePath.Text))
                shapefilePaths = new string[] { shapefilePath.Text };
            else if (Directory.Exists(shapefilePath.Text))
                shapefilePaths = Directory.GetFiles(shapefilePath.Text, "*.shp").ToArray();
            else
            {
                MessageBox.Show("Path \"" + shapefilePath.Text + "\" does not exist.");
                return;
            }

            if (shapefilePaths.Length == 0)
                MessageBox.Show("No shapefile(s) found at \"" + shapefilePath.Text + "\".");
            else
            {
                try
                {
                    Shapefile.ShapefileType selectedShapefileType = (Shapefile.ShapefileType)shapefileType.SelectedItem;
                    importShp.Enabled = false;
                    Thread t = new Thread(new ThreadStart(delegate()
                        {
                            try
                            {
                                Shapefile.ImportShapefiles(shapefilePaths, selectedShapefileType, new Shapefile.GetShapefileInfoDelegate(new Action<string, List<string>, Dictionary<string, string>>((path, options, optionValue) =>
                                    {
                                        if (options.Count > 0)
                                        {
                                            DynamicForm f = new DynamicForm("Enter shapefile information...");
                                            foreach (string option in options)
                                                f.AddTextBox(option + ":", null, 50, option);

                                            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                                foreach (string option in options)
                                                    optionValue.Add(option, f.GetValue<string>(option));
                                        }
                                    })));

                                if (!IsDisposed)
                                    Invoke(new Action(() =>
                                        {
                                            importShp.Enabled = true;
                                        }));
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("There was an error while importing one or more shapefiles:  " + ex.Message);
                            }
                        }));

                    t.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error importing shape files:  " + ex.Message);
                }
            }
        }
    }
}
