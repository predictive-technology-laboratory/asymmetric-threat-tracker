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
            string[] shapeFilePaths;
            if (File.Exists(shapefilePath.Text))
                shapeFilePaths = new string[] { shapefilePath.Text };
            else if (Directory.Exists(shapefilePath.Text))
                shapeFilePaths = Directory.GetFiles(shapefilePath.Text, "*.shp").ToArray();
            else
            {
                MessageBox.Show("Path \"" + shapefilePath.Text + "\" does not exist.");
                return;
            }

            if (shapeFilePaths.Length == 0)
                MessageBox.Show("No shapefile(s) found at \"" + shapefilePath.Text + "\".");
            else
            {
                try
                {
                    Shapefile.ShapefileType selectedShapefileType = (Shapefile.ShapefileType)shapefileType.SelectedItem;

                    Thread t = new Thread(new ThreadStart(delegate()
                        {
                            try
                            {
                                Shapefile.ImportShapefiles(shapeFilePaths.Select(path => new Tuple<string, string>(path, Path.GetFileNameWithoutExtension(path))).ToArray(), selectedShapefileType);
                                Console.Out.WriteLine("Shapefile import succeeded");
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
