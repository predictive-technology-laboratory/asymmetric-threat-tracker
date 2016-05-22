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
using PTL.ATT.Evaluation;
using LAIR.Extensions;
using System.Threading;
using PTL.ATT.Models;
using PTL.ATT.GUI.Visualization;
using Npgsql;
using LAIR.ResourceAPIs.PostGIS;
namespace PTL.ATT.GUI
{
    public partial class ThreatSurfaceComparisonForm : Form
    {
         
        public ThreatSurfaceComparisonForm(IEnumerable<Prediction> selectedPrediction,int PlotHeight, Size size)
        {
            InitializeComponent();
            Size = size;
            DisplayPredictions(selectedPrediction, PlotHeight);
        }
        private void DisplayPredictions(IEnumerable<Prediction> predictions, int PlotHeight)
        {


            Thread displayThread = new Thread(new ThreadStart(delegate()
            {
                List<Thread> threads = new List<Thread>();
                foreach (var p in predictions)
                {
                    p.AssessmentPlots.Clear();
                    p.MostRecentlyEvaluatedIncidentTime = DateTime.MinValue;
                    Thread evalThread = new Thread(new ThreadStart(delegate()
                    {
                        DiscreteChoiceModel.Evaluate(p, PlotHeight, PlotHeight);

                    }));
                    evalThread.Start();
                    threads.Add(evalThread);


                }
                double pointDistanceThreshold = 100;

                List<Overlay> overlays = new List<Overlay>();
                Thread areaT = new Thread(new ParameterizedThreadStart(o =>
                {
                    Area area = o as Area;
                    NpgsqlCommand command = DB.Connection.NewCommand(null);
                    lock (overlays) { overlays.Add(new Overlay(area.Name, Geometry.GetPoints(command, area.Shapefile.GeometryTable, ShapefileGeometry.Columns.Geometry, ShapefileGeometry.Columns.Id, pointDistanceThreshold), Color.Black, true, 0)); }
                    DB.Connection.Return(command.Connection);
                }));

                areaT.Start(predictions.First().PredictionArea);
                threads.Add(areaT);

                if (predictions.First().Model is IFeatureBasedDCM)
                {
                    ICollection<Feature> features = (predictions.First().Model as IFeatureBasedDCM).Features;
                    if (features.Count > 0)
                    {
                        Dictionary<string, int> featureIdViewPriority = new Dictionary<string, int>();
                        foreach (Feature f in features.OrderBy(f => f.Id))
                            featureIdViewPriority.Add(f.Id, featureIdViewPriority.Count + 1);

                        string minId = features.Min(f => f.Id);
                        foreach (Feature f in features)
                        {
                            Thread t = new Thread(new ParameterizedThreadStart(o =>
                            {
                                Feature feature = o as Feature;
                                if (feature.EnumType == typeof(FeatureBasedDCM.FeatureType) && (feature.EnumValue.Equals(FeatureBasedDCM.FeatureType.MinimumDistanceToGeometry) ||
                                                                                                feature.EnumValue.Equals(FeatureBasedDCM.FeatureType.GeometryDensity)))
                                {
                                    Shapefile shapefile = new Shapefile(int.Parse(feature.PredictionResourceId));
                                    NpgsqlCommand command = DB.Connection.NewCommand(null);
                                    List<List<PointF>> points = Geometry.GetPoints(command, shapefile.GeometryTable, ShapefileGeometry.Columns.Geometry, ShapefileGeometry.Columns.Id, pointDistanceThreshold);
                                    DB.Connection.Return(command.Connection);
                                    lock (overlays) { overlays.Add(new Overlay(shapefile.Name, points, ColorPalette.GetColor(), false, featureIdViewPriority[f.Id])); }
                                }
                            }));

                            t.Start(f);
                            threads.Add(t);
                        }
                    }
                }
                foreach (Thread t in threads)
                    t.Join();



                overlays.Sort();
                overlays.Reverse();

                multiDynamicThreatMap.Display(predictions, overlays);
            }));
            displayThread.Start();
        }
        
     
    }
}
