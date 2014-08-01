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

using LAIR.ResourceAPIs.PostGIS;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PostGIS = LAIR.ResourceAPIs.PostGIS;

namespace PTL.ATT.GUI.Visualization
{
    public partial class CustomSpatialFeatureForm : Form
    {
        public CustomSpatialFeatureForm()
        {
            InitializeComponent();
        }

        private void createPoints_Click(object sender, EventArgs e)
        {
            if (points.Items.Count == 0)
                MessageBox.Show("Must create at least one point.");
            else if (points.Items.Count == 1)
                elements.Items.Add(points.Items[0] as PostGIS.Point);
            else
                elements.Items.Add(new MultiPoint(points.Items.Cast<PostGIS.Point>().ToList(), (points.Items[0] as PostGIS.Point).SRID));
        }

        private void createLine_Click(object sender, EventArgs e)
        {
            if (points.Items.Count <= 1)
                MessageBox.Show("Must create at least two points for a line.");
            else
                elements.Items.Add(new LineString(points.Items.Cast<PostGIS.Point>().ToList(), (points.Items[0] as PostGIS.Point).SRID));
        }

        private void createConnectedLine_Click(object sender, EventArgs e)
        {
            if (points.Items.Count <= 1)
                MessageBox.Show("Must create at least two points for a connected line.");
            else
            {
                List<PostGIS.Point> connectedLinePoints = points.Items.Cast<PostGIS.Point>().ToList();
                connectedLinePoints.Add(connectedLinePoints[0]);
                elements.Items.Add(new LineString(connectedLinePoints, connectedLinePoints[0].SRID));
            }
        }

        private void createPolygon_Click(object sender, EventArgs e)
        {
            if (points.Items.Count <= 1)
                MessageBox.Show("Must create at least two points for a polygon.");
            else
            {
                List<PostGIS.Point> polygonPoints = points.Items.Cast<PostGIS.Point>().ToList();
                polygonPoints.Add(polygonPoints[0]);
                elements.Items.Add(new Polygon(polygonPoints, polygonPoints[0].SRID));
            }
        }

        private void clearPoints_Click(object sender, EventArgs e)
        {
            points.Items.Clear();
        }

        private void removeSelectedElements_Click(object sender, EventArgs e)
        {
            foreach (Geometry geometry in elements.SelectedItems.Cast<Geometry>().ToList())
                elements.Items.Remove(geometry);
        }

        private void createFeature_Click(object sender, EventArgs e)
        {
            if (elements.Items.Count == 0)
                MessageBox.Show("Must create elements.");
            else
            {
                DynamicForm f = new DynamicForm("Configure feature...", DynamicForm.CloseButtons.OkCancel);
                f.AddTextBox("Feature name:", null, 50, "name");
                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string name = f.GetValue<string>("name").Trim();
                    if (name == "")
                        MessageBox.Show("Invalid name:  must not be blank.");
                    else
                    {
                        Shapefile shapefile = null;
                        try
                        {
                            shapefile = Shapefile.Create(name, (elements.Items[0] as Geometry).SRID, Shapefile.ShapefileType.Feature);
                            ShapefileGeometry.Create(shapefile, elements.Items.Cast<Geometry>().Select(g => new Tuple<Geometry, DateTime>(g, DateTime.MinValue)).ToList());
                            MessageBox.Show("Shapefile \"" + name + "\" created.");
                            elements.Items.Clear();
                            points.Items.Clear();
                        }
                        catch (Exception ex)
                        {
                            Console.Out.WriteLine("Failed to create shapefile:  " + ex.Message);

                            try { shapefile.Delete(); }
                            catch (Exception ex2) { Console.Out.WriteLine("Failed to delete failed shapefile:  " + ex2.Message); }
                        }
                    }
                }
            }
        }
    }
}
