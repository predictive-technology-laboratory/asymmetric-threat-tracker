#region copyright
// Copyright 2013 Matthew S. Gerber (gerber.matthew@gmail.com)
// 
// This file is part of the Asymmetric Threat Tracker (ATT).
// 
// The ATT is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// The ATT is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with the ATT.  If not, see <http://www.gnu.org/licenses/>.
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
using PTL.ATT.ShapeFiles;
using System.Threading;
using Npgsql;

namespace PTL.ATT.GUI
{
    public partial class ImportShapeFileForm : Form
    {
        AttForm _attForm;

        public ImportShapeFileForm(AttForm attForm)
        {
            InitializeComponent();

            foreach (FeatureShapeFile.ShapefileType type in Enum.GetValues(typeof(FeatureShapeFile.ShapefileType)))
                featureType.Items.Add(type);

            featureType.SelectedIndex = 0;

            _attForm = attForm;
        }

        private void close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void shapefileFile_Click(object sender, EventArgs e)
        {
            string file = LAIR.IO.File.PromptForOpenPath("Select shape file...", Configuration.PostGisShapefileDirectory, "Shapefiles (*.shp)|*.shp");
            if (file != null)
                shapeFilePath.Text = file;
        }

        private void shapefileDir_Click(object sender, EventArgs e)
        {
            string dir = LAIR.IO.Directory.PromptForDirectory("Select shape file directory...", Configuration.PostGisShapefileDirectory);
            if (dir != null)
                shapeFilePath.Text = dir;
        }

        private void importShp_Click(object sender, EventArgs e)
        {
            string[] shapeFilePaths;
            if (File.Exists(shapeFilePath.Text))
                shapeFilePaths = new string[] { shapeFilePath.Text };
            else if (Directory.Exists(shapeFilePath.Text))
                shapeFilePaths = Directory.GetFiles(shapeFilePath.Text, "*.shp").ToArray();
            else
            {
                MessageBox.Show("Path \"" + shapeFilePath.Text + "\" does not exist.");
                return;
            }

            if (shapeFilePaths.Length == 0)
                MessageBox.Show("No shapefile(s) found at \"" + shapeFilePath.Text + "\".");
            else
            {
                try
                {
                    string selectedShapefileType = featureType.SelectedItem.ToString();

                    Thread t = new Thread(new ThreadStart(delegate()
                        {
                            try
                            {
                                if (areaShp.Checked)
                                    ShapeFile.ImportShapeFiles(shapeFilePaths, AreaShapeFile.Table, AreaShapeFile.Columns.Insert, typeof(AreaShapeFile), new object[] { });
                                else
                                    ShapeFile.ImportShapeFiles(shapeFilePaths, FeatureShapeFile.Table, FeatureShapeFile.Columns.Insert, typeof(FeatureShapeFile), new object[] { selectedShapefileType });

                                Console.Out.WriteLine("Shapefile import succeeded.");
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
