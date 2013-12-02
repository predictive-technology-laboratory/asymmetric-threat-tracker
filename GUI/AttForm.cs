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
using System.Globalization;
using NpgsqlTypes;
using System.IO;
using System.Threading;
using PTL.ATT.Models;
using System.Reflection;
using PTL.ATT.Classifiers;
using Npgsql;
using PTL.ATT.ShapeFiles;
using LAIR.Collections.Generic;
using PTL.ATT.GUI.Visualization;
using LAIR.ResourceAPIs.PostGIS;
using LAIR.Extensions;
using LAIR.Misc;
using PTL.ATT.Evaluation;
using System.Diagnostics;
using PTL.ATT.GUI.Plugins;
using System.Text.RegularExpressions;
using LAIR.XML;
using PTL.ATT.Smoothers;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Newtonsoft.Json.Linq;
using LAIR.ResourceAPIs.PostgreSQL;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using PTL.ATT.Importers;

namespace PTL.ATT.GUI
{
    public partial class AttForm : Form
    {
        public const int PlotHeight = 400;

        public enum IncidentImportSource
        {
            LocalFile,
            URI
        }

        #region members and properties
        private bool _setTrainingStartEndToolTip;
        private bool _setPredictionsToolTip;
        private List<string> _groups;
        private LogWriter _logWriter;
        private Dictionary<string, string> _featureRemapKeyTargetPredictionResource;

        public List<string> Groups
        {
            get { return _groups; }
        }

        public DateTime TrainingStartDate
        {
            get { return trainingStart.Value; }
            set
            {
                trainingStart.Value = value;
                RefreshIncidentTypes();
            }
        }

        public DateTime TrainingEndDate
        {
            get { return trainingEnd.Value; }
            set
            {
                trainingEnd.Value = value;
                RefreshIncidentTypes();
            }
        }

        public DateTime PredictionStartDateTime
        {
            get
            {
                return new DateTime(predictionStartDate.Value.Year,
                                    predictionStartDate.Value.Month,
                                    predictionStartDate.Value.Day,
                                    predictionStartTime.Value.Hour,
                                    predictionStartTime.Value.Minute,
                                    predictionStartTime.Value.Second);
            }
            set
            {
                predictionStartDate.Value = value;
                predictionStartTime.Value = value;
            }
        }

        public DateTime PredictionEndDateTime
        {
            get
            {
                return new DateTime(predictionEndDate.Value.Year,
                                    predictionEndDate.Value.Month,
                                    predictionEndDate.Value.Day,
                                    predictionEndTime.Value.Hour,
                                    predictionEndTime.Value.Minute,
                                    predictionEndTime.Value.Second);
            }
            set
            {
                predictionEndDate.Value = value;
                predictionEndTime.Value = value;
            }
        }

        public Area SelectedTrainingArea
        {
            get { return trainingAreas.SelectedItem as Area; }
        }

        public IEnumerable<string> SelectedIncidentTypes
        {
            get { return incidentTypes.SelectedItems.Cast<string>(); }
        }

        public DiscreteChoiceModel SelectedModel
        {
            get { return models.SelectedItem as DiscreteChoiceModel; }
        }

        public IEnumerable<Feature> SelectedFeatures
        {
            get { return features.SelectedItems.Cast<Feature>(); }
        }

        public Area SelectedPredictionArea
        {
            get { return predictionAreas.SelectedItem as Area; }
        }

        public List<Prediction> SelectedPredictions
        {
            get
            {
                Set<Prediction> selectedPredictions = new Set<Prediction>(false);
                foreach (TreeNode node in TraversePredictionTree())
                    if (node.Checked)
                        if (node.Tag is PredictionGroup)
                            selectedPredictions.AddRange(TraversePredictionTree(node.Nodes).Where(n => n.Tag is Prediction).Select(n => n.Tag as Prediction));
                        else
                            selectedPredictions.Add(node.Tag as Prediction);

                return selectedPredictions.ToList();
            }
        }

        public Prediction SelectedPrediction
        {
            get
            {
                List<Prediction> selectedPredictions = SelectedPredictions;
                if (selectedPredictions.Count == 1)
                    return selectedPredictions[0];
                else
                    throw new Exception("Other than 1 prediction is selected");
            }
        }
        #endregion

        #region construction / loading / closing
        public AttForm()
        {
            InitializeComponent();

            _setTrainingStartEndToolTip = true;
            _groups = new List<string>();
            _featureRemapKeyTargetPredictionResource = new Dictionary<string, string>();
        }

        private void AttForm_Load(object sender, EventArgs e)
        {
            Splash splash = new Splash(7);
            bool done = false;
            Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                {
                    Splash s = o as Splash;
                    s.Show();
                    while (!done) { Application.DoEvents(); }
                    s.Close();
                    s.Dispose();
                }));

            t.Start(splash);

            try
            {
                splash.UpdateProgress("Loading ATT configuration...");

                string attConfigPath = Path.Combine("Config", "att_config.xml");
                if (!File.Exists(attConfigPath))
                    throw new Exception("Failed to find att_config.xml file");

                ATT.Configuration.Initialize(attConfigPath, true);

                splash.UpdateProgress("Loading GUI configuration...");

                string guiConfigPath = Path.Combine("Config", "gui_config.xml");
                if (!File.Exists(guiConfigPath))
                    throw new Exception("Failed to find gui_config.xml file");

                GUI.Configuration.Initialize(guiConfigPath);

                splash.UpdateProgress("Loading plugins...");
                if (GUI.Configuration.PluginTypes.Count == 0)
                    pluginsToolStripMenuItem.Visible = false;
                else
                    foreach (Plugin plugin in GUI.Configuration.PluginTypes)
                    {
                        ToolStripMenuItem pluginMenuItem = new ToolStripMenuItem(plugin.MenuItemName);
                        pluginMenuItem.Tag = plugin;
                        pluginMenuItem.Click += new EventHandler(pluginsMenu_Click);
                        pluginsToolStripMenuItem.DropDownItems.Add(pluginMenuItem);
                    }

                _logWriter = new LogWriter(log, Configuration.LogPath, true, Console.Out);
                Console.SetOut(_logWriter);
                Console.SetError(_logWriter);
            }
            catch (Exception ex)
            {
                done = true;

                MessageBox.Show("Failed to initialize:  " + ex.Message + Environment.NewLine +
                                "Stack trace:  " + ex.StackTrace + Environment.NewLine + Environment.NewLine +
                                "Check the XML configuration files located in the Config sub-directory of the ATT executable directory.");

                Application.Exit();
                return;
            }

            trainingStart.Value = DateTime.Today.Add(new TimeSpan(-7, 0, 0, 0));
            trainingEnd.Value = trainingStart.Value.Add(new TimeSpan(6, 23, 59, 59));

            PredictionStartDateTime = DateTime.Today;
            PredictionEndDateTime = DateTime.Today + new TimeSpan(23, 59, 59);

            splash.UpdateProgress("Importing data from database...");

            try { RefreshAll(); }
            catch (Exception ex)
            {
                done = true;

                MessageBox.Show("Failed to load the ATT database:  " + ex.Message + Environment.NewLine +
                                "Stack trace:  " + ex.StackTrace + Environment.NewLine + Environment.NewLine +
                                "If you think this error is caused by an outdated database schema, one solution is to delete all existing tables and then restart ATT. This will, however, destroy all of your data.");
                Application.Exit();
                return;
            }

            SetTrainingStartEndToolTip();

            trainingAreas.Anchor = incidentTypes.Anchor = models.Anchor = features.Anchor = predictionAreas.Anchor = predictions.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

            WindowState = FormWindowState.Maximized;

            verticalSplitContainer.SplitterDistance = trainingAreas.Right + 20;

            if (Configuration.MonoAddIncidentRefresh)
            {
                ToolStripMenuItem refresh = new ToolStripMenuItem("Refresh");
                refresh.Click += new EventHandler((o, args) => RefreshIncidentTypes());
                incidentTypesMenu.Items.Add(refresh);
            }

            splash.UpdateProgress("ATT started");
            done = true;

            Console.Out.WriteLine("ATT started");
        }

        public void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AttForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.Out.WriteLine("ATT exited.");
        }
        #endregion

        #region data
        public void importShapefilesFromDiskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportShapeFileForm form = new ImportShapeFileForm(this);
            form.ShowDialog();
        }

        private void importShapefileFromSocrataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(delegate()
                {
                }));

            t.Start();
        }

        private void deleteShapefilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Shapefile[] shapefiles = Shapefile.GetAvailable().ToArray();
            if (shapefiles.Length == 0)
                MessageBox.Show("No shapefiles available for deletion.");
            else
            {
                DynamicForm f = new DynamicForm("Select shapefile(s) to delete...");
                f.AddListBox("Shapefile(s):", shapefiles, null, SelectionMode.MultiExtended, "shapefiles");
                if (f.ShowDialog() == DialogResult.OK)
                {
                    Shapefile[] selected = f.GetValue<System.Windows.Forms.ListBox.SelectedObjectCollection>("shapefiles").Cast<Shapefile>().ToArray();
                    if (selected.Length > 0 && MessageBox.Show("Are you sure you want to delete " + selected.Length + " shapefile(s)?", "Confirm delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        foreach (Shapefile shapefile in selected)
                            shapefile.Delete();

                        RefreshFeatures();

                        Console.Out.WriteLine("Deleted " + selected.Length + " shapefile(s)");
                    }
                }
            }
        }

        public void importIncidentsFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportIncidents(IncidentImportSource.LocalFile);
        }

        private void importIncidentsFromSocrataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportIncidents(IncidentImportSource.URI);
        }

        private void ImportIncidents(IncidentImportSource incidentImportSource)
        {
            Area[] areas = Area.GetAvailable().ToArray();
            Type[] importerTypes = Assembly.GetAssembly(typeof(Importer)).GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Importer))).ToArray();
            if (Area.GetAvailable().Count() == 0)
                MessageBox.Show("No areas available. Create one first.");
            else if (importerTypes.Length == 0)
                MessageBox.Show("No incident importers available.");
            else
            {
                Thread t = new Thread(new ThreadStart(delegate()
                    {
                        DynamicForm f = new DynamicForm("Enter import information...", MessageBoxButtons.OKCancel);

                        if (incidentImportSource == IncidentImportSource.LocalFile)
                        {
                            string path = LAIR.IO.File.PromptForOpenPath("Select incident file...", Configuration.IncidentsDataDirectory);
                            if (path == null)
                                return;
                            else
                                f.AddTextBox("Path:", path, "path");
                        }
                        else if (incidentImportSource == IncidentImportSource.URI)
                        {
                            f.AddTextBox("Download XML URI:", "                                                                      ", "uri", '\0', true);
                        }
                        else
                            throw new NotImplementedException("Unknown incident import source:  " + incidentImportSource);

                        f.AddCheckBox("Delete file after import:", System.Windows.Forms.RightToLeft.Yes, false, "delete");
                        f.AddDropDown("Importer:", importerTypes, importerTypes[0], "importer");
                        f.AddNumericUpdown("Source SRID:", 4326, 0, 0, decimal.MaxValue, 1, "source_srid");
                        f.AddDropDown("Destination area:", areas, areas[0], "area");
                        f.AddNumericUpdown("Incident hour offset:", 0, 0, decimal.MinValue, decimal.MaxValue, 1, "offset");

                        if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            #region get path of file to import
                            string path;
                            if (incidentImportSource == IncidentImportSource.LocalFile)
                                path = f.GetValue<string>("path");
                            else if (incidentImportSource == IncidentImportSource.URI)
                            {
                                path = Path.Combine(Configuration.IncidentsDataDirectory, new string(("socrata_import_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString() + ".xml").Select(c => c == ' ' || Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray()));

                                try { Download(f.GetValue<string>("uri"), path); }
                                catch (Exception ex)
                                {
                                    try { File.Delete(path); }
                                    catch (Exception ex2) { Console.Out.WriteLine("Failed to delete partially downloaded file \"" + path + "\":  " + ex2.Message); }

                                    Console.Out.WriteLine("Error downloading file from Socrata URI:  " + ex.Message);

                                    return;
                                }
                            }
                            else
                                throw new NotImplementedException("Unknown incident import source:  " + incidentImportSource);
                            #endregion

                            #region import file
                            if (path != null && File.Exists(path))
                            {
                                Type importerType = f.GetValue<Type>("importer");
                                int sourceSRID = Convert.ToInt32(f.GetValue<decimal>("source_srid"));
                                Area importArea = f.GetValue<Area>("area");
                                int hourOffset = Convert.ToInt32(f.GetValue<decimal>("offset"));
                                bool deleteFileAfterImport = f.GetValue<bool>("delete");

                                try
                                {
                                    Incident.CreateTable(importArea);
                                    Set<int> existingNativeIDs = Incident.GetNativeIds(importArea);
                                    existingNativeIDs.ThrowExceptionOnDuplicateAdd = false;

                                    if (importerType == typeof(SocrataXmlImporter))
                                    {
                                        f = new DynamicForm("Map ATT database columns to Socrata columns...", MessageBoxButtons.OK);
                                        string[] dbCols = new string[] { Incident.Columns.NativeId, Incident.Columns.Time, Incident.Columns.Type, Incident.Columns.X(importArea), Incident.Columns.Y(importArea) };
                                        string[] socrataCols = SocrataXmlImporter.GetColumnNames(path);
                                        Array.Sort(socrataCols);
                                        foreach (string dbCol in dbCols)
                                            f.AddDropDown(dbCol + ":", socrataCols, null, dbCol);

                                        f.ShowDialog();

                                        Dictionary<string, string> dbColSocrataCol = new Dictionary<string, string>();
                                        foreach (string dbCol in dbCols)
                                            dbColSocrataCol.Add(dbCol, f.GetValue<string>(dbCol));

                                        new SocrataXmlImporter().Import(path, Incident.GetTableName(importArea), Incident.Columns.Insert, new Func<XmlParser, Tuple<string, List<Parameter>>>(
                                            rowP =>
                                            {
                                                int nativeId = int.Parse(rowP.ElementText(dbColSocrataCol[Incident.Columns.NativeId])); rowP.Reset();

                                                if (existingNativeIDs.Add(nativeId))
                                                {
                                                    DateTime time = DateTime.Parse(rowP.ElementText(dbColSocrataCol[Incident.Columns.Time])) + new TimeSpan(hourOffset, 0, 0); rowP.Reset();
                                                    string type = rowP.ElementText(dbColSocrataCol[Incident.Columns.Type]); rowP.Reset();

                                                    double x;
                                                    if (!double.TryParse(rowP.ElementText(dbColSocrataCol[Incident.Columns.X(importArea)]), out x))
                                                        return null;

                                                    rowP.Reset();

                                                    double y;
                                                    if (!double.TryParse(rowP.ElementText(dbColSocrataCol[Incident.Columns.Y(importArea)]), out y))
                                                        return null;

                                                    rowP.Reset();

                                                    PostGIS.Point location = new PostGIS.Point(x, y, sourceSRID);

                                                    string value = Incident.GetValue(importArea, nativeId, location, false, "@time_" + nativeId, type);
                                                    List<Parameter> parameters = new List<Parameter>(new Parameter[] { new Parameter("time_" + nativeId, NpgsqlDbType.Timestamp, time) });
                                                    return new Tuple<string, List<Parameter>>(value, parameters);
                                                }
                                                else
                                                    return null;
                                            }));

                                        if (deleteFileAfterImport)
                                            File.Delete(path);
                                    }
                                    else
                                        throw new NotImplementedException("Unknown importer type:  " + importerType);
                                }
                                catch (Exception ex)
                                {
                                    Console.Out.WriteLine("Error while importing:  " + ex.Message);
                                }

                                RefreshIncidentTypes();
                            }
                            #endregion
                        }
                    }));

                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }
        }

        public void clearImportedIncidentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Area clearArea = PromptForArea("Select area to clear incidents from...");
            if (clearArea != null && MessageBox.Show("Are you sure you want to clear all incidents from \"" + clearArea.Name + "\"? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Incident.Clear(clearArea);
                RefreshIncidentTypes();
                RefreshFeatures();
            }
        }

        public void simulateIncidentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Area simulateArea = PromptForArea("Select area in which to simulate incidents...");
            if (simulateArea != null)
            {
                SimulateIncidentsForm f = new SimulateIncidentsForm(simulateArea);
                f.ShowDialog();
                RefreshIncidentTypes();
            }
        }

        public void clearSimulatedIncidentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Area clearSimulatedArea = PromptForArea("Select area to clear simulated incidents from...");
            if (clearSimulatedArea != null && MessageBox.Show("Are you sure you want to clear all simulated incidents from \"" + clearSimulatedArea.Name + "\"? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Incident.ClearSimulated(clearSimulatedArea);
                RefreshIncidentTypes();
                RefreshFeatures();
            }
        }
        #endregion

        #region training area
        private void trainingAreas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedTrainingArea != null)
                toolTip.SetToolTip(trainingAreas, SelectedTrainingArea.GetDetails(0));

            RefreshIncidentTypes();
            RefreshModels(-1, true);
        }

        public void addAreaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Shapefile[] areaShapefiles = Shapefile.GetAvailable().Where(sf => sf.Type == Shapefile.ShapefileType.Area).ToArray();
            if (areaShapefiles.Length == 0)
                MessageBox.Show("No shapefiles available from which to create area. Import shapefiles first.");
            else
            {
                DynamicForm f = new DynamicForm("Select shapefile to base area on...", MessageBoxButtons.OKCancel);
                f.AddDropDown("Shapefile:", areaShapefiles, null, "shapefile");
                if (f.ShowDialog() == DialogResult.OK)
                {
                    Shapefile selected = f.GetValue<Shapefile>("shapefile");
                    if (selected != null)
                    {
                        Thread t = new Thread(new ThreadStart(delegate()
                            {
                                Area.Create(selected, selected.Name);
                                Console.Out.WriteLine("Finished creating area");
                                RefreshAreas();
                            }));
                        t.Start();
                    }
                }
            }
        }

        public void deleteTrainingAreaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedTrainingArea != null)
                DeleteArea(SelectedTrainingArea);
        }

        private void DeleteArea(Area area)
        {
            bool hasMadePredictions = false;
            bool deletePredictions = false;
            foreach (Prediction prediction in Prediction.GetForArea(area))
            {
                hasMadePredictions = true;

                if (!deletePredictions && !(deletePredictions = MessageBox.Show("The area \"" + area.Name + "\" is associated with models and predictions, which must be deleted before the area can be deleted. Delete them now?", "Delete models and predictions?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes))
                    break;

                prediction.Delete();
            }

            if (!hasMadePredictions || deletePredictions)
            {
                area.Delete();
                RefreshAll();
            }
        }
        #endregion

        #region training dates
        private void trainingStart_ValueChanged(object sender, EventArgs e)
        {
            if (trainingEnd.Value < trainingStart.Value)
                trainingEnd.Value = trainingStart.Value + new TimeSpan(23, 59, 59);
        }

        private void trainingStart_CloseUp(object sender, EventArgs e)
        {
            RefreshIncidentTypes();
        }

        private void trainingEnd_ValueChanged(object sender, EventArgs e)
        {
            if (trainingEnd.Value < trainingStart.Value)
                trainingStart.Value = trainingEnd.Value - new TimeSpan(23, 59, 59);
        }

        private void trainingEnd_CloseUp(object sender, EventArgs e)
        {
            RefreshIncidentTypes();
        }

        private void SetTrainingStartEndToolTip()
        {
            if (SelectedTrainingArea != null)
            {
                string tip = Incident.Count(trainingStart.Value, trainingEnd.Value, SelectedTrainingArea, SelectedIncidentTypes.ToArray()) + " total incidents";
                toolTip.SetToolTip(trainingStart, tip);
                toolTip.SetToolTip(trainingEnd, tip);
            }
        }
        #endregion

        #region incidents
        public void selectAllIncidentTypesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _setTrainingStartEndToolTip = false;

            for (int i = 0; i < incidentTypes.Items.Count; ++i)
                incidentTypes.SetSelected(i, true);

            _setTrainingStartEndToolTip = true;

            SetTrainingStartEndToolTip();
        }

        private void incidentTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            perIncident.Enabled = incidentTypes.SelectedItems.Count > 1;

            DiscreteChoiceModel m = SelectedModel;

            if (m != null)
            {
                m.Update(m.Name, m.PointSpacing, m.TrainingArea, m.TrainingStart, m.TrainingEnd, m.TrainingSampleSize, m.PredictionSampleSize, SelectedIncidentTypes, m.Smoothers);
                toolTip.SetToolTip(models, m.GetDetails(0));
            }

            if (_setTrainingStartEndToolTip)
                SetTrainingStartEndToolTip();
        }
        #endregion

        #region model
        public void addModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedTrainingArea == null)
                MessageBox.Show("Must select a training area.");
            else if (incidentTypes.SelectedItems.Count == 0)
                MessageBox.Show("Must select incident types.");
            else
            {
                SelectModelForm selectModel = new SelectModelForm();
                if (selectModel.ShowDialog() == DialogResult.OK)
                {
                    int newModelId = -1;
                    if (selectModel.Type == typeof(SpatialDistanceDCM))
                    {
                        SpatialDistanceDcmForm f = new SpatialDistanceDcmForm();
                        if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            newModelId = SpatialDistanceDCM.Create(null, f.ModelName, f.PointSpacing, f.FeatureDistanceThreshold, f.ClassifyNonZeroVectorsUniformly, null, SelectedTrainingArea, trainingStart.Value, trainingEnd.Value, f.TrainingSampleSize, f.PredictionSampleSize, SelectedIncidentTypes, f.Classifier, f.Smoothers);
                    }
                    else if (selectModel.Type == typeof(TimeSliceDCM))
                    {
                        TimeSliceDcmForm f = new TimeSliceDcmForm();
                        if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            newModelId = TimeSliceDCM.Create(null, f.ModelName, f.PointSpacing, f.FeatureDistanceThreshold, f.ClassifyNonZeroVectorsUniformly, null, SelectedTrainingArea, trainingStart.Value, trainingEnd.Value, f.TrainingSampleSize, f.PredictionSampleSize, SelectedIncidentTypes, f.Classifier, f.Smoothers, f.TimeSliceHours, f.TimeSlicesPerPeriod);
                    }
                    else if (selectModel.Type == typeof(KernelDensityDCM))
                    {
                        KernelDensityDcmForm f = new KernelDensityDcmForm();
                        if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            newModelId = KernelDensityDCM.Create(f.ModelName, f.PointSpacing, SelectedTrainingArea, trainingStart.Value, trainingEnd.Value, f.TrainingSampleSize, f.PredictionSampleSize, SelectedIncidentTypes, f.Normalize, f.Smoothers);
                    }

                    if (newModelId >= 0)
                        RefreshModels(newModelId, true);
                }
            }
        }

        public void updateModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedModel == null)
                MessageBox.Show("Must select model.");
            else if (SelectedTrainingArea == null)
                MessageBox.Show("Must select a training area.");
            else if (SelectedIncidentTypes.Count() == 0)
                MessageBox.Show("Must select incident types.");
            else
            {
                DiscreteChoiceModel m = SelectedModel;

                if (m.HasMadePredictions)
                    if (MessageBox.Show("You cannot update a model that has been used to make predictions. Would you like to copy the current model instead?", "Copy model?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        m = DiscreteChoiceModel.Instantiate(m.Copy());
                    else
                        return;

                if (m is TimeSliceDCM)
                {
                    TimeSliceDCM ts = m as TimeSliceDCM;
                    TimeSliceDcmForm f = new TimeSliceDcmForm(ts);
                    if (f.ShowDialog() == DialogResult.OK)
                        ts.Update(f.ModelName, f.PointSpacing, f.FeatureDistanceThreshold, f.ClassifyNonZeroVectorsUniformly, SelectedTrainingArea, trainingStart.Value, trainingEnd.Value, f.TrainingSampleSize, f.PredictionSampleSize, SelectedIncidentTypes, f.Classifier, f.Smoothers, f.TimeSliceHours, f.TimeSlicesPerPeriod);
                }
                else if (m is SpatialDistanceDCM)
                {
                    SpatialDistanceDCM sd = m as SpatialDistanceDCM;
                    SpatialDistanceDcmForm f = new SpatialDistanceDcmForm(sd);
                    if (f.ShowDialog() == DialogResult.OK)
                        sd.Update(f.ModelName, f.PointSpacing, f.FeatureDistanceThreshold, f.ClassifyNonZeroVectorsUniformly, SelectedTrainingArea, trainingStart.Value, trainingEnd.Value, f.TrainingSampleSize, f.PredictionSampleSize, SelectedIncidentTypes, f.Classifier, f.Smoothers);
                }
                else if (m is KernelDensityDCM)
                {
                    KernelDensityDCM kde = m as KernelDensityDCM;
                    KernelDensityDcmForm f = new KernelDensityDcmForm(kde);
                    if (f.ShowDialog() == DialogResult.OK)
                        kde.Update(f.ModelName, f.PointSpacing, SelectedTrainingArea, trainingStart.Value, trainingEnd.Value, f.TrainingSampleSize, f.PredictionSampleSize, SelectedIncidentTypes, f.Normalize, f.Smoothers);
                }

                RefreshModels(m.Id, true);
            }
        }

        public void deleteModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DiscreteChoiceModel m = SelectedModel;
            if (m != null)
            {
                if (m.HasMadePredictions)
                    if (MessageBox.Show("Cannot delete model until all of its associated predictions have been deleted. Delete them now?", "Delete predictions?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        m.DeletePredictions();

                if (!m.HasMadePredictions)
                    m.Delete();

                RefreshAll();
            }
        }

        private void models_SelectedIndexChanged(object sender, EventArgs e)
        {
            DiscreteChoiceModel m = SelectedModel;

            if (m != null)
            {
                trainingAreas.SelectedItem = trainingAreas.Items.Cast<Area>().Where(a => a.Id == m.TrainingAreaId).First();

                trainingStart.Value = m.TrainingStart;
                trainingEnd.Value = m.TrainingEnd;

                RefreshIncidentTypes();
                RefreshFeatures();

                toolTip.SetToolTip(models, m.GetDetails(0));
            }
        }
        #endregion

        #region features
        public void selectAllFeaturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < features.Items.Count; ++i)
                features.SetSelected(i, true);
        }

        private void remapSelectedFeaturesDuringPredictionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedModel == null)
                MessageBox.Show("Must select model before remapping.");
            else if (SelectedFeatures.Count() == 0)
                MessageBox.Show("Must select features before remapping.");
            else if (SelectedPredictionArea == null)
                MessageBox.Show("Must select prediction area before remapping.");
            else
            {
                FeatureRemappingForm f = new FeatureRemappingForm(SelectedFeatures, SelectedModel.GetAvailableFeatures(SelectedPredictionArea));
                f.ShowDialog();

                _featureRemapKeyTargetPredictionResource.Clear();
                foreach (Feature feature in features.Items)
                    _featureRemapKeyTargetPredictionResource.Add(feature.RemapKey, feature.PredictionResourceId);

                RefreshFeatures();
            }
        }

        private void clearFeatureRemappingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _featureRemapKeyTargetPredictionResource.Clear();
            RefreshFeatures();
        }
        #endregion

        #region prediction area
        private void predictionAreas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedPredictionArea != null)
                toolTip.SetToolTip(predictionAreas, SelectedPredictionArea.GetDetails(0));
        }

        public void deletePredictionAreaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedPredictionArea != null)
                DeleteArea(SelectedPredictionArea);
        }
        #endregion

        #region prediction times
        private void predictionStartDateTime_Changed(object sender, EventArgs e)
        {
            if (PredictionEndDateTime < PredictionStartDateTime)
                PredictionEndDateTime = PredictionStartDateTime + new TimeSpan(23, 59, 59);
        }

        private void predictionEndDateTime_Changed(object sender, EventArgs e)
        {
            if (PredictionEndDateTime < PredictionStartDateTime)
                PredictionStartDateTime = PredictionEndDateTime - new TimeSpan(23, 59, 59);
        }
        #endregion

        #region prediction parameters
        private void numPredictions_ValueChanged(object sender, EventArgs e)
        {
            predictionSpacingHours.Enabled = startPrediction.Enabled = slideTrainingStart.Enabled = slideTrainingEnd.Enabled = numPredictions.Value > 1;
            startPrediction.Maximum = numPredictions.Value;
        }

        private void slideTrainingStart_CheckedChanged(object sender, EventArgs e)
        {
            if (slideTrainingStart.Checked)
                slideTrainingEnd.Checked = true;
        }

        private void perIncident_EnabledChanged(object sender, EventArgs e)
        {
            if (!perIncident.Enabled)
                perIncident.Checked = false;
        }

        private void slideTrainingEnd_CheckedChanged(object sender, EventArgs e)
        {
            if (slideTrainingStart.Checked && !slideTrainingEnd.Checked)
            {
                MessageBox.Show("You cannot slide the training start and not slide the training end.");
                slideTrainingEnd.Checked = true;
            }
        }

        private void run_Click(object sender, EventArgs e)
        {
            DiscreteChoiceModel m = SelectedModel;

            if (m == null)
                MessageBox.Show("Must select a model.");
            else
            {
                string defaultPredictionName = m.Name + " (" + m.GetType().Name + ")" + (!perIncident.Checked ? " " + m.IncidentTypes.Concatenate("+") : "");
                string predictionName = GetValue.Show("Enter name for prediction" + (perIncident.Checked ? " (per-incident names will be added)" : "") + "...", defaultPredictionName);
                if (predictionName == null)
                    return;

                Run(true, predictionName, -1, null);
            }
        }

        public void Run(bool newRun, string predictionName, int idOfSpatiotemporallyIdenticalPrediction, Action<int> runFinishedCallback)
        {
            DiscreteChoiceModel m = SelectedModel;

            predictionName = predictionName.Trim();

            if (m == null)
                MessageBox.Show("Must select a model.");
            else if (!(models.SelectedItem is KernelDensityDCM) && features.SelectedItems.Count == 0)
                MessageBox.Show("Must select one or more features.");
            else if (SelectedPredictionArea == null)
                MessageBox.Show("Must select a prediction area.");
            else if (predictionName == "")
                MessageBox.Show("Must provide a non-empty prediction name.");
            else
            {
                Area predictionArea = SelectedPredictionArea;
                IEnumerable<Feature> selectedFeatures = SelectedFeatures.ToArray();
                IEnumerable<string> incidentTypes = m.IncidentTypes.ToArray();

                Thread t = new Thread(new ThreadStart(delegate()
                    {
                        int mostRecentPredictionId = -1;

                        for (int i = (int)startPrediction.Value - 1; i < numPredictions.Value; ++i)
                        {
                            Console.Out.WriteLine("Running prediction \"" + predictionName + "\" (" + (i + 1) + " of " + numPredictions.Value + ")");

                            TimeSpan offset = new TimeSpan(0, i * (int)predictionSpacingHours.Value, 0, 0);

                            if (perIncident.Checked)
                            {
                                idOfSpatiotemporallyIdenticalPrediction = -1;
                                foreach (string incidentType in incidentTypes)
                                {
                                    Console.Out.WriteLine("Running per-incident prediction \"" + incidentType + "\"");

                                    try
                                    {
                                        m.Update(m.Name, m.PointSpacing, m.TrainingArea, trainingStart.Value + (slideTrainingStart.Checked ? offset : new TimeSpan(0L)), trainingEnd.Value + (slideTrainingEnd.Checked ? offset : new TimeSpan(0L)), m.TrainingSampleSize, m.PredictionSampleSize, new string[] { incidentType }, m.Smoothers);
                                        mostRecentPredictionId = m.Run(selectedFeatures, idOfSpatiotemporallyIdenticalPrediction, predictionArea, PredictionStartDateTime + offset, PredictionEndDateTime + offset, predictionName + " " + incidentType + (numPredictions.Value > 1 ? " " + (i + 1) : ""), newRun);
                                        newRun = false;
                                    }
                                    catch (Exception ex)
                                    {
                                        string msg = "ERROR:  An error occurred while running prediction:  " + ex.Message + Environment.NewLine +
                                                      ex.StackTrace;
                                        Console.Out.WriteLine(msg);
                                        Notify("Error", msg);
                                    }

                                    if (idOfSpatiotemporallyIdenticalPrediction == -1)
                                        idOfSpatiotemporallyIdenticalPrediction = mostRecentPredictionId;
                                }
                            }
                            else
                            {
                                try
                                {
                                    m.Update(m.Name, m.PointSpacing, m.TrainingArea, trainingStart.Value + (slideTrainingStart.Checked ? offset : new TimeSpan(0L)), trainingEnd.Value + (slideTrainingEnd.Checked ? offset : new TimeSpan(0L)), m.TrainingSampleSize, m.PredictionSampleSize, m.IncidentTypes, m.Smoothers);
                                    mostRecentPredictionId = m.Run(selectedFeatures, idOfSpatiotemporallyIdenticalPrediction, predictionArea, PredictionStartDateTime + offset, PredictionEndDateTime + offset, predictionName + (numPredictions.Value > 1 ? " " + (i + 1) : ""), newRun);
                                    newRun = false;
                                }
                                catch (Exception ex)
                                {
                                    string msg = "ERROR:  An error occurred while running prediction:  " + ex.Message + Environment.NewLine +
                                                  ex.StackTrace;
                                    Console.Out.WriteLine(msg);
                                    Notify("Error", msg);
                                }
                            }
                        }

                        m.Update(m.Name, m.PointSpacing, m.TrainingArea, trainingStart.Value, trainingEnd.Value, m.TrainingSampleSize, m.PredictionSampleSize, incidentTypes, m.Smoothers);
                        RefreshPredictions(mostRecentPredictionId);

                        if (runFinishedCallback != null)
                            Invoke(new Action(delegate() { runFinishedCallback(mostRecentPredictionId); }));

                        Notify("Done running predictions", "");
                    }));

                t.Start();
            }
        }
        #endregion

        #region predictions
        public void GroupPredictions()
        {
            predictions.BeginUpdate();

            List<Prediction> preds = TraversePredictionTree().Where(n => n.Tag is Prediction).Select(n => n.Tag as Prediction).ToList();
            preds.Sort(new Comparison<Prediction>((p1, p2) => p1.Id.CompareTo(p2.Id)));
            predictions.Nodes.Clear();
            foreach (PredictionGroup group in Group(preds, _groups, 0))
                AddToTree(predictions.Nodes, group);

            predictions.EndUpdate();

            SetPredictionsTooltip(null);
        }

        private IEnumerable<PredictionGroup> Group(IEnumerable<Prediction> predictions, List<string> _groups, int groupNum)
        {
            if (groupNum >= _groups.Count)
            {
                foreach (Prediction prediction in predictions)
                    yield return new PredictionGroup(prediction.Name, prediction);

                yield break;
            }

            Func<Prediction, string> grouper;
            if (_groups[groupNum] == groupByModelToolStripMenuItem.Text)
                grouper = p => "Model:  " + p.Model.Name + " " + p.Model.Id.ToString();
            else if (_groups[groupNum] == groupByFeaturesToolStripMenuItem.Text)
                grouper = p => "Features:  " + p.SelectedFeatures.OrderBy(f => f.Description).Select(f => f.Description).Concatenate(", ");
            else if (_groups[groupNum] == groupByIncidentTypesToolStripMenuItem.Text)
                grouper = p => "Incidents:  " + p.IncidentTypes.OrderBy(i => i).Concatenate(", ");
            else if (_groups[groupNum] == groupByRunToolStripMenuItem.Text)
                grouper = p => "Run:  " + p.RunId;
            else if (_groups[groupNum] == groupByPredictionIntervalToolStripMenuItem.Text)
                grouper = p => "Time:  " + p.PredictionStartTime.ToShortDateString() + " " + p.PredictionStartTime.ToShortTimeString() + " -- " + p.PredictionEndTime.ToShortDateString() + " " + p.PredictionEndTime.ToShortTimeString();
            else
            {
                MessageBox.Show("ERROR:  Grouping \"" + _groups[groupNum] + "\" not implemented. Please send this message to the developers.");
                yield break;
            }

            foreach (IGrouping<string, Prediction> group in predictions.GroupBy(grouper))
            {
                PredictionGroup predictionGroup = new PredictionGroup(group.Key);
                foreach (PredictionGroup subGroup in Group(group, _groups, groupNum + 1))
                    predictionGroup.SubGroups.Add(subGroup);

                yield return predictionGroup;
            }
        }

        private void AddToTree(TreeNodeCollection tree, PredictionGroup group)
        {
            TreeNode node = new TreeNode(group.Name + (group.Prediction == null ? " (" + group.Count + ")" : ""));

            if (group.Prediction == null)
                node.Tag = group;
            else
                node.Tag = group.Prediction;

            foreach (PredictionGroup subGroup in group.SubGroups)
                AddToTree(node.Nodes, subGroup);

            tree.Add(node);
        }

        public IEnumerable<TreeNode> TraversePredictionTree(TreeNodeCollection tree = null)
        {
            if (tree == null)
                tree = predictions.Nodes;

            foreach (TreeNode node in tree)
            {
                yield return node;

                foreach (TreeNode subNode in TraversePredictionTree(node.Nodes))
                    yield return subNode;
            }
        }

        private void groupByModelToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (groupByModelToolStripMenuItem.Checked)
            {
                if (!_groups.Contains(groupByModelToolStripMenuItem.Text))
                    _groups.Add(groupByModelToolStripMenuItem.Text);
            }
            else
                _groups.Remove(groupByModelToolStripMenuItem.Text);

            RefreshPredictions(SelectedPredictions.Select(s => s.Id).ToArray());
        }

        private void groupByFeaturesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (groupByFeaturesToolStripMenuItem.Checked)
            {
                if (!_groups.Contains(groupByFeaturesToolStripMenuItem.Text))
                    _groups.Add(groupByFeaturesToolStripMenuItem.Text);
            }
            else
                _groups.Remove(groupByFeaturesToolStripMenuItem.Text);

            RefreshPredictions(SelectedPredictions.Select(s => s.Id).ToArray());
        }

        private void groupByIncidentTypesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (groupByIncidentTypesToolStripMenuItem.Checked)
            {
                if (!_groups.Contains(groupByIncidentTypesToolStripMenuItem.Text))
                    _groups.Add(groupByIncidentTypesToolStripMenuItem.Text);
            }
            else
                _groups.Remove(groupByIncidentTypesToolStripMenuItem.Text);

            RefreshPredictions(SelectedPredictions.Select(s => s.Id).ToArray());
        }

        private void groupByPredictionIntervalToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (groupByPredictionIntervalToolStripMenuItem.Checked)
            {
                if (!_groups.Contains(groupByPredictionIntervalToolStripMenuItem.Text))
                    _groups.Add(groupByPredictionIntervalToolStripMenuItem.Text);
            }
            else
                _groups.Remove(groupByPredictionIntervalToolStripMenuItem.Text);

            RefreshPredictions(SelectedPredictions.Select(s => s.Id).ToArray());
        }

        private void groupByRunToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (groupByRunToolStripMenuItem.Checked)
            {
                if (!_groups.Contains(groupByRunToolStripMenuItem.Text))
                    _groups.Add(groupByRunToolStripMenuItem.Text);
            }
            else
                _groups.Remove(groupByRunToolStripMenuItem.Text);

            RefreshPredictions(SelectedPredictions.Select(s => s.Id).ToArray());
        }

        private void submenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                e.Cancel = true;
        }

        private void predictions_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_setPredictionsToolTip)
                SetPredictionsTooltip(SelectedPredictions.Count == 1 ? e.Node : null);
        }

        private void SetPredictionsTooltip(TreeNode selectedNode)
        {
            string text = null;
            if (selectedNode != null && selectedNode.Checked && selectedNode.Tag is Prediction)
                text = (selectedNode.Tag as Prediction).GetDetails(0);

            toolTip.SetToolTip(predictions, text);
        }

        public void displayPredictionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedPredictions.Count == 1)
            {
                if (SelectedPredictions[0] != threatMap.DisplayedPrediction)
                {
                    Prediction p = SelectedPredictions[0];

                    Thread displayThread = new Thread(new ThreadStart(delegate()
                        {
                            List<Thread> threads = new List<Thread>();

                            Thread evalThread = new Thread(new ThreadStart(delegate()
                                {
                                    DiscreteChoiceModel.Evaluate(p, PlotHeight, PlotHeight);
                                    Invoke(new Action(delegate() { toolTip.SetToolTip(predictions, p.GetDetails(0)); }));
                                }));
                            evalThread.Start();
                            threads.Add(evalThread);

                            double pointDistanceThreshold = 100;

                            List<Overlay> overlays = new List<Overlay>();
                            Thread areaT = new Thread(new ParameterizedThreadStart(delegate(object o)
                                {
                                    Area area = o as Area;
                                    Dictionary<string, string> constraints = new Dictionary<string, string>();
                                    constraints.Add(AreaGeometry.Columns.AreaId, "'" + area.Id + "'");
                                    NpgsqlConnection connection = DB.Connection.OpenConnection;
                                    lock (overlays) { overlays.Add(new Overlay(area.Name, Geometry.GetPoints(connection, AreaGeometry.GetTableName(p.PredictionArea.SRID), AreaGeometry.Columns.Geometry, AreaGeometry.Columns.Id, constraints, pointDistanceThreshold), Color.Black, true, 0)); }
                                    DB.Connection.Return(connection);
                                }));

                            areaT.Start(p.PredictionArea);
                            threads.Add(areaT);

                            foreach (Feature f in p.SelectedFeatures)
                            {
                                Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                                    {
                                        Feature feature = o as Feature;
                                        if (feature.EnumType == typeof(SpatialDistanceDCM.SpatialDistanceFeature) && feature.EnumValue.Equals(SpatialDistanceDCM.SpatialDistanceFeature.DistanceShapeFile))
                                        {
                                            Dictionary<string, string> constraints = new Dictionary<string, string>();
                                            constraints.Add(ShapefileGeometry.Columns.ShapefileId, feature.PredictionResourceId.ToString());
                                            NpgsqlConnection connection = DB.Connection.OpenConnection;
                                            List<List<PointF>> points = Geometry.GetPoints(connection, ShapefileGeometry.GetTableName(p.PredictionArea.SRID), ShapefileGeometry.Columns.Geometry, ShapefileGeometry.Columns.Id, constraints, pointDistanceThreshold);
                                            DB.Connection.Return(connection);
                                            lock (overlays) { overlays.Add(new Overlay(feature.Description, points, ColorPalette.GetColor(), false, 1 + overlays.Count)); }
                                        }
                                    }));

                                t.Start(f);
                                threads.Add(t);
                            }

                            foreach (Thread t in threads)
                                t.Join();

                            overlays.Sort();
                            overlays.Reverse();

                            threatMap.Display(p, overlays);

                            Invoke(new Action(RefreshAssessmentPlots));
                        }));

                    displayThread.Start();
                }
            }
            else
                MessageBox.Show("Must select a single prediction to display.");
        }

        public void editPredictionNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Prediction> selectedPredictions = SelectedPredictions;
            if (selectedPredictions.Count == 1)
            {
                string name = GetValue.Show("New prediction name.", SelectedPrediction.Name);
                if (name != null && name.Trim() != "")
                    selectedPredictions[0].Name = name.Trim();
            }
            else if (selectedPredictions.Count > 1)
            {
                string name = GetValue.Show("Common base name for " + selectedPredictions.Count + " predictions.");
                if (name != null && name.Trim() != "")
                    for (int i = 0; i < selectedPredictions.Count; ++i)
                        selectedPredictions[i].Name = name.Trim() + "-" + i;
            }
            else
                return;

            Set<int> selectedIds = new Set<int>(selectedPredictions.Select(p => p.Id).ToArray());
            if (threatMap.DisplayedPrediction != null && selectedIds.Contains(threatMap.DisplayedPrediction.Id))
            {
                threatMap.DisplayedPrediction.AssessmentPlots = selectedPredictions.Where(p => p.Id == threatMap.DisplayedPrediction.Id).First().AssessmentPlots;
                RefreshAssessmentPlots();
            }

            RefreshPredictions(selectedIds);
        }

        private void editPredictionRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Prediction> selectedPredictions = SelectedPredictions;

            string newRunIdStr = GetValue.Show("New run number for " + selectedPredictions.Count + " prediction(s).");
            if (newRunIdStr == null)
                return;

            int newRunId;
            if (int.TryParse(newRunIdStr, out newRunId))
                foreach (Prediction prediction in selectedPredictions)
                    prediction.RunId = newRunId;

            RefreshPredictions(selectedPredictions.Select(p => p.Id));
        }

        private void copyPredictionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Prediction> selectedPredictions = SelectedPredictions;
            if (selectedPredictions.Count == 0)
                MessageBox.Show("Select one or more predictions to copy.");
            else if (selectedPredictions.Count == 1 || MessageBox.Show("Are you sure you want to copy " + selectedPredictions.Count + " prediction(s)?", "Confirm copy", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                DynamicForm f = new DynamicForm("Set copy parameters");
                f.AddNumericUpdown("Number of copies of each prediction:  ", 1, 0, 1, decimal.MaxValue, 1, "copies");
                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    int numCopies;
                    try { numCopies = Convert.ToInt32(f.GetValue<decimal>("copies")); }
                    catch (Exception ex) { MessageBox.Show("Invalid number of copies:  " + ex.Message); return; }

                    Thread t = new Thread(new ThreadStart(delegate()
                        {
                            int predictionNum = 0;
                            foreach (Prediction selectedPrediction in selectedPredictions)
                            {
                                ++predictionNum;

                                for (int copyNum = 1; copyNum <= numCopies; ++copyNum)
                                {
                                    Console.Out.WriteLine("Creating copy " + copyNum + " (of " + numCopies + ") of prediction " + predictionNum + " (of " + selectedPredictions.Count + ")");
                                    int copyId = selectedPrediction.Copy("Copy " + copyNum + " of " + selectedPrediction.Name, predictionNum == 1 && copyNum == 1, false);

                                    try { Point.VacuumTable(copyId); }
                                    catch (Exception ex) { Console.Out.WriteLine("ERROR:  failed to vacuum point table:  " + ex.Message); }
                                    try { PointPrediction.VacuumTable(copyId); }
                                    catch (Exception ex) { Console.Out.WriteLine("ERROR:  failed to vacuum point table:  " + ex.Message); }
                                }
                            }

                            try { Prediction.VacuumTable(); }
                            catch (Exception ex) { Console.Out.WriteLine("ERROR:  failed to vacuum " + Prediction.Table + ":  " + ex.Message); }

                            string msg = "Done copying predictions";
                            Console.Out.WriteLine(msg);
                            Notify(msg, "");

                            RefreshPredictions(selectedPredictions.Select(p => p.Id).ToArray());
                        }));

                    t.Start();
                }
            }
        }

        private void selectFeaturesForPredictionAndRerunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Set<Prediction> predictions = new Set<Prediction>(SelectedPredictions.Where(p => p.Model is IFeatureBasedDCM).ToArray());
            if (predictions.Count > 0 && MessageBox.Show("Are you sure you want to select features for " + predictions.Count + " prediction(s) and rerun them? This will erase previous results for these predictions.", "Confirm feature selection", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                Thread t = new Thread(new ThreadStart(delegate()
                    {
                        foreach (Prediction prediction in predictions)
                            try { (prediction.Model as IFeatureBasedDCM).SelectFeatures(prediction, true); }
                            catch (Exception ex) { Console.Out.WriteLine("An error occurred while selecting features for prediction:  " + ex.Message); }

                        if (threatMap.DisplayedPrediction != null && predictions.Contains(threatMap.DisplayedPrediction))
                        {
                            RefreshPredictions(threatMap.DisplayedPrediction.Id);
                            displayPredictionToolStripMenuItem_Click(sender, e);
                        }
                    }));

                t.Start();
            }
        }

        private void smoothPredictionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Prediction> selectedPredictions = SelectedPredictions;

            if (selectedPredictions.Count == 0)
                MessageBox.Show("Select one or more predictions to smooth.");
            else if (selectedPredictions.Count == 1 || MessageBox.Show("Are you sure you want to smooth " + selectedPredictions.Count + " predictions?", "Confirm smoothing application", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                SmootherChooser smootherChooser = new SmootherChooser();
                if (smootherChooser.ShowDialog() == DialogResult.OK && smootherChooser.Selected.Count() > 0)
                {
                    Thread t = new Thread(new ThreadStart(delegate()
                        {
                            try
                            {
                                int predictionNum = 0;
                                foreach (Prediction selectedPrediction in selectedPredictions)
                                {
                                    Console.Out.WriteLine("Smoothing prediction " + ++predictionNum + " of " + selectedPredictions.Count);
                                    foreach (Smoother smoother in smootherChooser.Selected)
                                    {
                                        Console.Out.WriteLine("  --> Applying smoother:  " + smoother);
                                        smoother.Apply(selectedPrediction);
                                    }

                                    selectedPrediction.MostRecentlyEvaluatedIncidentTime = DateTime.MinValue;
                                    selectedPrediction.UpdateEvaluation();
                                    selectedPrediction.Name = "Smoothed " + selectedPrediction.Name;

                                    #region update prediction log with smoothed threat scores
                                    if (File.Exists(selectedPrediction.PointPredictionLogPath))
                                    {
                                        Dictionary<int, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> oldLog = selectedPrediction.ReadPointPredictionLog();
                                        Dictionary<int, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> newLog = new Dictionary<int, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>>();
                                        foreach (PointPrediction pointPrediction in selectedPrediction.PointPredictions)
                                        {
                                            List<Tuple<string, double>> smoothedIncidentScore = new List<Tuple<string, double>>();
                                            foreach (string incident in pointPrediction.IncidentScore.Keys)
                                                smoothedIncidentScore.Add(new Tuple<string, double>(incident, Math.Round(pointPrediction.IncidentScore[incident], 3)));

                                            newLog.Add(pointPrediction.PointId, new Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>(smoothedIncidentScore, oldLog[pointPrediction.PointId].Item2));
                                        }

                                        selectedPrediction.WritePointPredictionLog(newLog);
                                    }
                                    #endregion
                                }

                                RefreshPredictions(selectedPredictions.Select(p => p.Id).ToArray());
                                if (threatMap.DisplayedPrediction != null && selectedPredictions.Contains(threatMap.DisplayedPrediction))
                                {
                                    RefreshPredictions(threatMap.DisplayedPrediction.Id);
                                    displayPredictionToolStripMenuItem_Click(sender, e);
                                }

                                string msg = "Done smoothing predictions";
                                Console.Out.WriteLine(msg);
                                Notify(msg, "");
                            }
                            catch (Exception ex)
                            {
                                string msg = "Error smoothing predictions:  " + ex.Message;
                                Console.Out.WriteLine(msg);
                                Notify("Error", msg);
                            }
                        }));

                    t.Start();
                }
            }
        }

        public void comparePredictionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TraversePredictionTree().Count(n => n.Checked) > 1)
            {
                EvaluateSelectedPredictions();

                List<List<Plot>> plotRows = new List<List<Plot>>();
                foreach (TreeNode node in TraversePredictionTree())
                    if (node.Checked)
                        if (node.Tag is PredictionGroup)
                            plotRows.Add(new List<Plot>(new Plot[] { (node.Tag as PredictionGroup).AggregatePlot }));
                        else if (node.Tag is Prediction)
                            plotRows.Add((node.Tag as Prediction).AssessmentPlots);
                        else
                            throw new Exception("Unexpected node tag:  " + node.Tag);

                PredictionComparisonForm comparisonForm = new PredictionComparisonForm(plotRows, Size);
                if (comparisonForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    StringBuilder comparisonTitle = new StringBuilder();
                    Dictionary<string, List<PointF>> seriesPoints = new Dictionary<string, List<PointF>>();
                    foreach (SurveillancePlot selectedPlot in comparisonForm.SelectedPlots)
                    {
                        string plotTitle = selectedPlot.Title.Replace(Environment.NewLine, " ").RemoveRepeatedWhitespace();
                        comparisonTitle.Append((comparisonTitle.Length == 0 ? "Comparison of " : ", ") + plotTitle);
                        foreach (string series in selectedPlot.SeriesPoints.Keys)
                        {
                            string seriesTitle = plotTitle;
                            if (series == DiscreteChoiceModel.OptimalSeriesName)
                                seriesTitle = DiscreteChoiceModel.OptimalSeriesName + " " + seriesTitle;

                            seriesTitle = seriesPoints.ContainsKey(seriesTitle) ? seriesTitle + " " + seriesPoints.Keys.Count(k => k == seriesTitle) : seriesTitle;

                            seriesPoints.Add(seriesTitle, selectedPlot.SeriesPoints[series]);
                        }
                    }

                    SurveillancePlot comparisonPlot = new SurveillancePlot(comparisonTitle.ToString(), seriesPoints, 500, 500, Plot.Format.JPEG, 2);
                    List<TitledImage> comparisonPlotImages = new List<TitledImage>(new TitledImage[] { new TitledImage(comparisonPlot.Image, null) });
                    new ImageViewer(comparisonPlotImages, 0).ShowDialog();
                }
            }
            else
                MessageBox.Show("Select two or more predictions / groups to make a comparison.");
        }

        public void showModelDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedPredictions.Count == 0)
                MessageBox.Show("Select one or more predictions to show model details for.");
            else
            {
                try
                {
                    StringBuilder report = new StringBuilder();
                    foreach (Prediction p in SelectedPredictions)
                        report.AppendLine(p.Model.GetDetails(p).Trim() + Environment.NewLine);

                    ModelDetailsForm modelDetailsForm = new ModelDetailsForm("Model details", report.ToString());
                    modelDetailsForm.ShowDialog();
                }
                catch (Exception ex)
                {
                    string errorMsg = "Error getting model details:  " + ex.Message + Environment.NewLine +
                                       ex.StackTrace;
                    Console.Out.WriteLine(errorMsg);
                    Notify("Error", errorMsg);
                }
            }
        }

        public void aggregateAndEvaluateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedPredictions.Count < 2)
                MessageBox.Show("Select multiple predictions to run an aggregate evaluation.");
            else
            {
                List<TitledImage> images = new List<TitledImage>();

                string title = "Aggregated";
                if (TraversePredictionTree().Count(n => n.Checked) == 1)
                    title = TraversePredictionTree().Where(n => n.Checked).First().Text;

                try { images.Add(new TitledImage(DiscreteChoiceModel.EvaluateAggregate(SelectedPredictions, 500, 500, title, title).Image, title)); }
                catch (Exception ex) { MessageBox.Show("Error rendering aggregate plot:  " + ex.Message); }

                new ImageViewer(images, 0).ShowDialog();
            }
        }

        public void openModelDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Prediction selectedPrediction in SelectedPredictions)
            {
                try { Process.Start(selectedPrediction.ModelDirectory); }
                catch (Exception ex) { MessageBox.Show("Failed to open prediction model directory:  " + ex.Message); }
            }
        }

        public void deletePredictionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Prediction> selectedPredictions = SelectedPredictions;
            if (selectedPredictions.Count > 0)
            {
                if (MessageBox.Show("Are you sure you want to delete " + selectedPredictions.Count + " prediction(s)?", "Confirm delete", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    Thread t = new Thread(new ThreadStart(delegate()
                        {
                            int predictionNum = 0;
                            foreach (Prediction prediction in selectedPredictions)
                            {
                                Console.Out.WriteLine("Deleting prediction " + ++predictionNum + " of " + selectedPredictions.Count);
                                prediction.Delete();
                            }

                            try { Prediction.VacuumTable(); }
                            catch (Exception ex) { Console.Out.WriteLine("ERROR:  failed to vacuum " + Prediction.Table + ":  " + ex.Message); }

                            string msg = "Done deleting predictions";
                            Console.Out.WriteLine(msg);
                            Notify(msg, "");

                            RefreshPredictions(-1);
                            threatMap.Clear();
                            Invoke(new Action(() => assessments.ClearPlots()));
                        }));
                    t.Start();
                }
            }
            else
                MessageBox.Show("Must select one or more predictions to delete.");
        }

        public void deselectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (TreeNode n in TraversePredictionTree())
                n.Checked = false;
        }

        public void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (TreeNode n in TraversePredictionTree())
                n.Checked = true;
        }

        public void EvaluateSelectedPredictions()
        {
            Set<Thread> threads = new Set<Thread>(PTL.ATT.GUI.Configuration.ProcessorCount);
            for (int i = 0; i < PTL.ATT.GUI.Configuration.ProcessorCount; ++i)
            {
                Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                    {
                        int skip = (int)o;
                        foreach (TreeNode node in TraversePredictionTree().Where(n => n.Checked))
                            if (skip-- <= 0)
                            {
                                if (node.Tag is PredictionGroup)
                                {
                                    PredictionGroup group = node.Tag as PredictionGroup;
                                    if (group.AggregatePlot == null)
                                        group.AggregatePlot = DiscreteChoiceModel.EvaluateAggregate(TraversePredictionTree(node.Nodes).Where(n => n.Tag is Prediction).Select(n => n.Tag as Prediction), 500, 500, group.Name, group.Name);
                                }
                                else if (node.Tag is Prediction)
                                    DiscreteChoiceModel.Evaluate(node.Tag as Prediction, PlotHeight, PlotHeight);
                                else
                                    throw new Exception("Unexpected node tag:  " + node.Tag);

                                skip = PTL.ATT.GUI.Configuration.ProcessorCount - 1;
                            }
                    }));

                t.Start(i);
                threads.Add(t);
            }

            foreach (Thread t in threads)
                t.Join();
        }
        #endregion

        #region refreshing
        public void RefreshAll()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshAll));
                return;
            }

            threatMap.Clear();
            assessments.ClearPlots();

            try
            {
                RefreshAreas();
                RefreshPredictions(-1);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Failed to refresh information from database:  " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public void RefreshAreas()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshAreas));
                return;
            }

            toolTip.SetToolTip(trainingAreas, null);
            toolTip.SetToolTip(predictionAreas, null);

            trainingAreas.Items.Clear();
            foreach (Area area in Area.GetAvailable())
                trainingAreas.Items.Add(area);

            if (trainingAreas.Items.Count > 0)
                trainingAreas.SelectedIndex = 0;

            predictionAreas.Items.Clear();
            foreach (Area area in Area.GetAvailable())
                predictionAreas.Items.Add(area);

            if (predictionAreas.Items.Count > 0)
                predictionAreas.SelectedIndex = 0;
        }

        public void RefreshIncidentTypes()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshIncidentTypes));
                return;
            }

            _setTrainingStartEndToolTip = false;

            incidentTypes.Items.Clear();
            if (SelectedTrainingArea != null)
                foreach (string incidentType in Incident.GetUniqueTypes(trainingStart.Value, trainingEnd.Value, SelectedTrainingArea))
                    incidentTypes.Items.Add(incidentType);

            if (SelectedModel != null)
                foreach (string incidentType in SelectedModel.IncidentTypes)
                {
                    int index = incidentTypes.Items.IndexOf(incidentType);
                    if (index >= 0)
                        incidentTypes.SetSelected(index, true);
                }

            _setTrainingStartEndToolTip = true;

            SetTrainingStartEndToolTip();
        }

        public void RefreshModels(int modelIdToSelect, bool refreshFeatures)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, bool>(RefreshModels), modelIdToSelect, refreshFeatures);
                return;
            }

            toolTip.SetToolTip(models, null);

            models.Items.Clear();

            if (SelectedTrainingArea != null)
            {
                foreach (DiscreteChoiceModel m in DiscreteChoiceModel.GetForArea(SelectedTrainingArea))
                    models.Items.Add(m);

                if (models.Items.Count > 0)
                {
                    if (modelIdToSelect < 0)
                        modelIdToSelect = (models.Items[0] as DiscreteChoiceModel).Id;

                    models.SelectedIndex = models.Items.IndexOf(models.Items.Cast<DiscreteChoiceModel>().Where(m => m.Id == modelIdToSelect).First());
                }

                if (refreshFeatures)
                    RefreshFeatures();
            }
        }

        public void RefreshFeatures()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshFeatures));
                return;
            }

            features.Items.Clear();

            DiscreteChoiceModel model = SelectedModel;

            if (model != null)
            {
                List<Feature> sortedFeatures = new List<Feature>(model.GetAvailableFeatures(SelectedTrainingArea));
                sortedFeatures.Sort();

                foreach (Feature f in sortedFeatures)
                    if (_featureRemapKeyTargetPredictionResource.ContainsKey(f.RemapKey))
                        f.PredictionResourceId = _featureRemapKeyTargetPredictionResource[f.RemapKey];

                features.Items.AddRange(sortedFeatures.ToArray());
            }
        }

        public void RefreshPredictions(int predictionIdToSelect)
        {
            RefreshPredictions(predictionIdToSelect == -1 ? null : new int[] { predictionIdToSelect });
        }

        public void RefreshPredictions(IEnumerable<int> predictionIdsToSelect)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<IEnumerable<int>>(RefreshPredictions), predictionIdsToSelect);
                return;
            }

            predictions.Nodes.Clear();
            foreach (Prediction prediction in Prediction.GetAvailable())
            {
                TreeNode node = new TreeNode();
                node.Tag = prediction;
                predictions.Nodes.Add(node);
            }

            GroupPredictions();

            if (predictionIdsToSelect != null)
            {
                _setPredictionsToolTip = predictionIdsToSelect.Count() == 1;
                TraversePredictionTree().Where(n => n.Tag is Prediction && predictionIdsToSelect.Contains((n.Tag as Prediction).Id)).Select(n => n.Checked = true).ToArray();

                if (!_setPredictionsToolTip)
                    SetPredictionsTooltip(null);

                _setPredictionsToolTip = true;
            }
        }
        #endregion

        #region threat map / assessments
        private void visualizer_MouseEnter(object sender, EventArgs e)
        {
            Focus();
        }

        public void RefreshAssessmentPlots()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshAssessmentPlots));
                return;
            }

            assessments.ClearPlots();
            foreach (Plot plot in threatMap.DisplayedPrediction.AssessmentPlots)
            {
                PictureBox plotBox = new PictureBox();
                plotBox.Size = plot.Image.Size;
                plotBox.Image = plot.Image;
                plotBox.MouseDoubleClick += new MouseEventHandler(plot_MouseDoubleClick);
                assessments.AddPlot(plotBox);
            }
        }

        private void plot_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            List<TitledImage> images = new List<TitledImage>();
            foreach (Plot plot in threatMap.DisplayedPrediction.AssessmentPlots)
                try { images.Add(new TitledImage(plot.Image, null)); }
                catch (Exception ex) { MessageBox.Show("Error rendering plot:  " + ex.Message); }

            ImageViewer v = new ImageViewer(images, assessments.GetIndexOf(sender as Control));
            v.ShowDialog();
        }
        #endregion

        #region notifications
        private void sendNotificationsToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (sendNotificationsToolStripMenuItem.Checked)
                try { Configuration.SetEncryptionKey(false); }
                catch (Exception ex)
                {
                    MessageBox.Show("Error setting up encryption:  " + ex.Message);
                    sendNotificationsToolStripMenuItem.Checked = false;
                }
        }

        private void sendTestNotificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Notify("Test", "This is a test notification from the ATT", true);
        }

        private void resetEncryptionKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Warning:  resetting the encryption key will render useless any previously encrypted material (e.g., passwords that are stored in encrypted form). You must re-encrypt this material in order to use it. Proceed?", "Confirm reset", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                Configuration.SetEncryptionKey(true);
        }

        public void Notify(string subject, string body, bool ignoreNotifySwitch = false, bool bodyIsHTML = false)
        {
            if (!ignoreNotifySwitch && !sendNotificationsToolStripMenuItem.Checked)
                return;

            Thread t = new Thread(new ThreadStart(delegate()
                {
                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        try
                        {
                            string hostName = Dns.GetHostName();
                            body += Environment.NewLine +
                                    Environment.NewLine +
                                    "Sent from " + hostName + " at " + Dns.GetHostEntry(hostName).AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).Select(ip => ip.ToString()).Concatenate(" / ");

                            foreach (Tuple<string, string> to in Configuration.NotificationToEmailNames)
                                LAIR.IO.Email.SendViaSMTP(Configuration.NotificationHost, Configuration.NotificationPort, Configuration.NotificationEnableSSL, Configuration.NotificationUsername, Configuration.NotificationPassword, Configuration.NotificationFromEmail, Configuration.NotificationFromName, to.Item1, to.Item2, null, subject, body, bodyIsHTML);
                        }
                        catch (Exception ex) { Console.Out.WriteLine("Failed to send email notification:  " + ex.Message); }
                    }
                    else
                        Console.Out.WriteLine("Network is not available. Email notifications will not be sent.");
                }));

            t.Start();
        }

        private void encryptStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string password = null;

            while (true)
            {
                DynamicForm f = new DynamicForm("Enter password");
                f.AddTextBox("Password:", "            ", "password", '*', true);
                f.AddTextBox("Confirm password:", "            ", "confirmed", '*', true);

                if (f.ShowDialog() == DialogResult.Cancel)
                    break;

                string pass = f.GetValue<string>("password").Trim();
                string confirmed = f.GetValue<string>("confirmed").Trim();
                if (pass != confirmed)
                    MessageBox.Show("Entries do not match.");
                else
                {
                    password = pass;
                    break;
                }
            }

            if (password != null && password.Length > 0)
                try
                {
                    DynamicForm showEncrypted = new DynamicForm("Encrypted password", MessageBoxButtons.OK);
                    showEncrypted.AddTextBox("Result:", password.Encrypt(Configuration.EncryptionKey, Configuration.EncryptionInitialization).Select(b => b.ToString()).Concatenate("-"), "encrypted");
                    showEncrypted.ShowDialog();
                }
                catch (Exception ex) { MessageBox.Show("Error getting encrypted password:  " + ex.Message); }
        }
        #endregion

        #region about
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(ATT.Configuration.LicenseText, "About the Asymmetric Threat Tracker");
        }
        #endregion

        #region miscellaneous
        private void viewLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try { Process.Start(Configuration.LoggingEditor, Configuration.LogPath); }
            catch (Exception ex) { MessageBox.Show("Error while opening log:  " + ex.Message); }
        }

        private void deleteLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete the log?", "Confirm delete", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                try { _logWriter.Clear(); }
                catch (Exception ex) { MessageBox.Show("Error while deleting log:  " + ex.Message); }
        }

        private void pluginsMenu_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(delegate()
                {
                    Invoke(new Action(delegate()
                        {
                            Plugin plugin = (sender as ToolStripMenuItem).Tag as Plugin;
                            Console.Out.WriteLine("Running plugin action \"" + plugin.MenuItemName + "\"");
                            plugin.Run(this);
                        }));
                }));

            t.Start();
        }

        private Area PromptForArea(string prompt, int srid = -1)
        {
            Area[] areas = Area.GetAvailable(srid).ToArray();
            if (areas.Length == 0)
            {
                MessageBox.Show("No areas available" + (srid == -1 ? "" : " for SRID " + srid) + ". Create one first.");
                return null;
            }

            DynamicForm f = new DynamicForm(prompt, MessageBoxButtons.OKCancel);
            f.AddDropDown("Areas:", areas, null, "area");
            Area importArea = null;
            if (f.ShowDialog() == DialogResult.OK)
                importArea = f.GetValue<Area>("area");

            return importArea;
        }

        private void Download(string uri, string path)
        {
            Console.Out.Write("Downloading \"" + uri + "\" to \"" + path + "\"...");

            WebRequest request = WebRequest.Create(uri);
            request.Method = "GET";

            using (FileStream downloadFile = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (WebResponse response = request.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            {
                long totalBytesRead = 0;
                long bytesUntilUpdate = 5 * (long)Math.Pow(2, 20);  // update every 5MB
                byte[] buffer = new byte[1024 * 64];
                int bytesRead;
                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    downloadFile.Write(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;
                    bytesUntilUpdate -= bytesRead;
                    if (bytesUntilUpdate <= 0)
                    {
                        bytesUntilUpdate = (long)Math.Pow(2, 20);
                        Console.Out.Write(string.Format("{0:0.00}", totalBytesRead / (double)bytesUntilUpdate) + " MB...");
                    }
                }
                downloadFile.Close();
                response.Close();
                responseStream.Close();
                Console.Out.WriteLine("download finished.");
            }
        }
        #endregion
    }
}
