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
                DynamicForm f = new DynamicForm("Configure feature...");
                f.AddTextBox("Feature name:", null, 50, "name");
                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string name = f.GetValue<string>("name").Trim();
                    if (name == "")
                        MessageBox.Show("Invalid name:  must not be blank.");
                    else
                    {
                        NpgsqlCommand cmd = DB.Connection.NewCommand(null);
                        int shapefileId = -1;
                        try
                        {
                            shapefileId = Shapefile.Create(cmd.Connection, name, (elements.Items[0] as Geometry).SRID, Shapefile.ShapefileType.DistanceFeature);
                            ShapefileGeometry.Create(cmd.Connection, shapefileId, elements.Items.Cast<Geometry>().ToList());
                            MessageBox.Show("Shapefile \"" + name + "\" created.");
                            elements.Items.Clear();
                            points.Items.Clear();
                        }
                        catch (Exception ex)
                        {
                            Console.Out.WriteLine("Failed to create shapefile:  " + ex.Message);

                            try { new Shapefile(shapefileId).Delete(); }
                            catch (Exception ex2) { Console.Out.WriteLine("Failed to delete failed shapefile:  " + ex2.Message); }
                        }
                        finally
                        {
                            DB.Connection.Return(cmd.Connection);
                        }
                    }
                }
            }
        }
    }
}
