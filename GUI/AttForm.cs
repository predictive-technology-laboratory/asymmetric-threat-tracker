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
using System.Globalization;
using NpgsqlTypes;
using System.IO;
using System.Threading;
using PTL.ATT.Models;
using System.Reflection;
using PTL.ATT.Classifiers;
using Npgsql;
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
using System.Runtime.Serialization.Formatters.Binary;

namespace PTL.ATT.GUI
{
    public partial class AttForm : Form
    {
        #region classes/types/delegates/enums
        public enum ImportFileType
        {
            Plain,
            Zip
        }

        public enum PathRelativizationId
        {
            EventDirectory,
            IncidentDirectory,
            ShapefileDirectory
        }

        [Serializable]
        public class ShapefileInfoRetriever : IShapefileInfoRetriever
        {
            private int _sourceSRID;
            private int _targetSRID;
            private string _name;

            public ShapefileInfoRetriever(string name, int sourceSRID, int targetSRID)
            {
                _name = name;
                _sourceSRID = sourceSRID;
                _targetSRID = targetSRID;
            }

            public void GetShapefileInfo(string shapefilePath, List<string> optionValuesToGet, Dictionary<string, string> optionValue)
            {
                DynamicForm df = new DynamicForm("Supply shapefile import options...", MessageBoxButtons.OK);
                foreach (string optionValueToGet in optionValuesToGet)
                {
                    string value = null;

                    if (optionValueToGet == "reprojection" && _sourceSRID > 0 && _targetSRID > 0)
                        value = _sourceSRID + ":" + _targetSRID;

                    if (optionValueToGet == "name" && !string.IsNullOrWhiteSpace(_name))
                        value = _name;

                    df.AddTextBox(optionValueToGet + ":", value, 50, optionValueToGet);

                    if (value != null)
                        optionValue.Add(optionValueToGet, value);
                }

                if (optionValuesToGet.Any(v => !optionValue.ContainsKey(v)))
                {
                    df.ShowDialog();

                    foreach (string optionValueToGet in optionValuesToGet)
                        if (!optionValue.ContainsKey(optionValueToGet))
                            optionValue.Add(optionValueToGet, df.GetValue<string>(optionValueToGet));
                }
            }
        }

        /// <summary>
        /// Completes a dynamic importer creation form.
        /// </summary>
        /// <param name="f">Base form.</param>
        /// <returns>Completed form.</returns>
        public delegate DynamicForm CompleteImporterFormDelegate(DynamicForm f);

        /// <summary>
        /// Called when an import finishes
        /// </summary>
        /// <returns></returns>
        public delegate void ImportCompletionDelegate();

        /// <summary>
        /// Creates an importer using an importer form.
        /// </summary>
        /// <param name="name">Name of importer.</param>
        /// <param name="path">Path of file to import.</param>
        /// <param name="sourceURI">URI of file to import.</param>
        /// <returns>Importer that is ready to run.</returns>
        public delegate Importer CreateImporterDelegate(string name, string path, string sourceURI, DynamicForm importerForm);

        /// <summary>
        /// Creates an XML row inserter.
        /// </summary>
        /// <param name="databaseColumnInputColumn">Mapping from database columns to input columns.</param>
        /// <returns>XML row inserter.</returns>
        public delegate XmlImporter.XmlRowInserter CreateXmlRowInserterDelegate(Dictionary<string, string> databaseColumnInputColumn);

        public enum ManageImporterAction
        {
            Load,
            Edit,
            Run,
            Store,
            Delete
        }
        #endregion

        #region members and properties
        public const int PlotHeight = 400;
        private bool _setPredictionsToolTip;
        private List<string> _groups;
        private LogWriter _logWriter;

        public List<string> Groups
        {
            get { return _groups; }
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

        public DiscreteChoiceModel SelectedModel
        {
            get { return models.SelectedItem as DiscreteChoiceModel; }
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

            _groups = new List<string>();
        }

        private void AttForm_Load(object sender, EventArgs e)
        {
            Splash splash = new Splash(4);
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
                                (ex.InnerException == null ? "" : "Inner exception message:  " + ex.InnerException.Message + Environment.NewLine) +
                                "Stack trace:  " + ex.StackTrace + Environment.NewLine + Environment.NewLine +
                                "Check the XML configuration files located in the Config sub-directory of the ATT executable directory.");

                Application.Exit();
                return;
            }

            PredictionStartDateTime = DateTime.Today;
            PredictionEndDateTime = DateTime.Today + new TimeSpan(23, 59, 59);

            splash.UpdateProgress("Importing data from database...");

            try { RefreshAll(); }
            catch (Exception ex)
            {
                done = true;

                MessageBox.Show("Failed to load the ATT database:  " + ex.Message + Environment.NewLine +
                                (ex.InnerException == null ? "" : "Inner exception message:  " + ex.InnerException.Message + Environment.NewLine) +
                                "Stack trace:  " + ex.StackTrace + Environment.NewLine + Environment.NewLine +
                                "If you think this error is caused by an outdated database schema, one solution is to delete all existing tables and then restart ATT. This will, however, destroy all of your data.");
                Application.Exit();
                return;
            }

            models.Anchor = predictionAreas.Anchor = predictions.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

            WindowState = FormWindowState.Maximized;

            verticalSplitContainer.SplitterDistance = models.Right + 20;

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
        public void importShapefilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import(Configuration.PostGisShapefileDirectory,

                   new CompleteImporterFormDelegate(f =>
                       {
                           f.AddNumericUpdown("Source SRID:", 0, 0, 0, decimal.MaxValue, 1, "source_srid");
                           f.AddNumericUpdown("Target SRID:", 0, 0, 0, decimal.MaxValue, 1, "target_srid");
                           f.AddDropDown("Shapefile type:", Enum.GetValues(typeof(Shapefile.ShapefileType)).Cast<Shapefile.ShapefileType>().ToArray(), Shapefile.ShapefileType.Area, "type", new Action<object, EventArgs>((o, args) =>
                               {
                                   ComboBox cb = o as ComboBox;
                                   NumericUpDown boxSizeUpDown = f.GetControl<NumericUpDown>("containment_box_size");
                                   if (boxSizeUpDown != null)
                                       boxSizeUpDown.Parent.Visible = (Shapefile.ShapefileType)cb.SelectedItem == Shapefile.ShapefileType.Area;
                               }));
                           f.AddNumericUpdown("Area containment box size (meters):", 1000, 0, 1, decimal.MaxValue, 1, "containment_box_size");
                           return f;
                       }),

                   new CreateImporterDelegate((name, path, sourceURI, importerForm) =>
                       {
                           int sourceSRID = Convert.ToInt32(importerForm.GetValue<decimal>("source_srid"));
                           int targetSRID = Convert.ToInt32(importerForm.GetValue<decimal>("target_srid"));
                           Shapefile.ShapefileType shapefileType = importerForm.GetValue<Shapefile.ShapefileType>("type");

                           ShapefileInfoRetriever shapefileInfoRetriever = new ShapefileInfoRetriever(name, sourceSRID, targetSRID);
                           string relativePath = RelativizePath(path, Configuration.PostGisShapefileDirectory, PathRelativizationId.ShapefileDirectory);

                           if (shapefileType == Shapefile.ShapefileType.Area)
                           {
                               int containmentBoxSize = Convert.ToInt32(importerForm.GetValue<decimal>("containment_box_size"));
                               return new AreaShapefileImporter(name, path, relativePath, sourceURI, sourceSRID, targetSRID, shapefileInfoRetriever, containmentBoxSize);
                           }
                           else if (shapefileType == Shapefile.ShapefileType.Feature)
                               return new FeatureShapefileImporter(name, path, relativePath, sourceURI, sourceSRID, targetSRID, shapefileInfoRetriever);
                           else
                               throw new NotImplementedException("Unrecognized shapefile type:  " + shapefileType);
                       }),

                   Configuration.PostGisShapefileDirectory,
                   "Shapefiles (*.shp;*.zip)|*.shp;*.zip", "*.shp",
                   new ImportCompletionDelegate(() => { RefreshPredictionAreas(); }));
        }

        private void importPointfilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import(Configuration.EventsImportDirectory,

                   new CompleteImporterFormDelegate(f =>
                       {
                           Area[] areas = Area.GetAll().ToArray();
                           if (areas.Length == 0)
                           {
                               MessageBox.Show("No areas available. Import one first.");
                               return null;
                           }

                           f.AddDropDown("Import into area:", areas, null, "area");
                           f.AddNumericUpdown("Source SRID:", 4326, 0, 0, decimal.MaxValue, 1, "source_srid");

                           return f;
                       }),

                   new CreateImporterDelegate((name, path, sourceURI, importerForm) =>
                       {
                           Area importArea = importerForm.GetValue<Area>("area");
                           int sourceSRID = Convert.ToInt32(importerForm.GetValue<decimal>("source_srid"));

                           Type[] rowInserterTypes = Assembly.GetAssembly(typeof(XmlImporter.XmlRowInserter)).GetTypes().Where(type => !type.IsAbstract && (type == typeof(XmlImporter.PointfileXmlRowInserter) || type.IsSubclassOf(typeof(XmlImporter.PointfileXmlRowInserter)))).ToArray();
                           string[] databaseColumns = new string[] { XmlImporter.PointfileXmlRowInserter.Columns.X, XmlImporter.PointfileXmlRowInserter.Columns.Y, XmlImporter.PointfileXmlRowInserter.Columns.Time };

                           return CreateXmlImporter(name, path, Configuration.EventsImportDirectory, PathRelativizationId.EventDirectory, sourceURI, rowInserterTypes, databaseColumns, databaseColumnInputColumn =>
                                                    {
                                                        return new XmlImporter.PointfileXmlRowInserter(databaseColumnInputColumn, sourceSRID, importArea);
                                                    });
                       }),

                   Configuration.EventsImportDirectory,
                   null, null, null);
        }

        private void deleteGeographicDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Shapefile[] shapefiles = Shapefile.GetAll().ToArray();
            if (shapefiles.Length == 0)
                MessageBox.Show("No geographic data available for deletion.");
            else
            {
                DynamicForm f = new DynamicForm("Select geographic data to delete...");
                f.AddListBox("Geographic data:", shapefiles, null, SelectionMode.MultiExtended, "shapefiles");
                if (f.ShowDialog() == DialogResult.OK)
                {
                    Shapefile[] selectedShapefiles = f.GetValue<System.Windows.Forms.ListBox.SelectedObjectCollection>("shapefiles").Cast<Shapefile>().ToArray();
                    if (selectedShapefiles.Length > 0 && MessageBox.Show("Are you sure you want to delete " + selectedShapefiles.Length + " shapefile(s)?", "Confirm delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        int shapefilesDeleted = 0;
                        foreach (Shapefile shapefile in selectedShapefiles)
                        {
                            List<Area> areasForShapefile = Area.GetForShapefile(shapefile);
                            List<DiscreteChoiceModel> modelsForShapefile = areasForShapefile.SelectMany(a => DiscreteChoiceModel.GetForArea(a, false)).Union(DiscreteChoiceModel.GetAll(false).Where(m => m is IFeatureBasedDCM && (m as IFeatureBasedDCM).Features.Any(feat => feat.TrainingResourceId == shapefile.Id.ToString() || feat.PredictionResourceId == shapefile.Id.ToString()))).ToList();
                            List<Prediction> predictionsForShapefile = areasForShapefile.SelectMany(a => Prediction.GetForArea(a)).ToList();
                            if (modelsForShapefile.Count > 0 || predictionsForShapefile.Count > 0)
                                if (MessageBox.Show("The shapefile \"" + shapefile + "\" is associated with " + modelsForShapefile.Count + " model(s) and " + predictionsForShapefile.Count + " prediction(s), which must be deleted before the shapefile can be deleted. Delete them now?", "Delete models and predictions?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                {
                                    foreach (Prediction prediction in predictionsForShapefile)
                                        prediction.Delete();

                                    foreach (DiscreteChoiceModel model in modelsForShapefile)
                                        model.Delete();
                                }
                                else
                                    continue;

                            try
                            {
                                shapefile.Delete();
                                ++shapefilesDeleted;
                            }
                            catch (Exception ex) { Console.Out.WriteLine("Error deleting shapefile \"" + shapefile.Name + "\":  " + ex.Message); }
                        }

                        Console.Out.WriteLine("Deleted " + shapefilesDeleted + " shapefile(s)");

                        if (shapefilesDeleted > 0)
                            RefreshAll();
                    }
                }
            }
        }

        public void importIncidentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import(Configuration.IncidentsImportDirectory,

                   new CompleteImporterFormDelegate(f =>
                       {
                           Area[] areas = Area.GetAll().ToArray();
                           if (areas.Length == 0)
                           {
                               MessageBox.Show("No areas available. Import one first.");
                               return null;
                           }

                           f.AddDropDown("Import into area:", areas, null, "area");
                           f.AddNumericUpdown("Source SRID:", 4326, 0, 0, decimal.MaxValue, 1, "source_srid");
                           f.AddNumericUpdown("Incident hour offset:", 0, 0, decimal.MinValue, decimal.MaxValue, 1, "offset");

                           return f;
                       }),

                   new CreateImporterDelegate((name, path, sourceURI, importerForm) =>
                       {
                           Area importArea = importerForm.GetValue<Area>("area");
                           int sourceSRID = Convert.ToInt32(importerForm.GetValue<decimal>("source_srid"));
                           int hourOffset = Convert.ToInt32(importerForm.GetValue<decimal>("offset"));

                           Type[] rowInserterTypes = Assembly.GetAssembly(typeof(XmlImporter.XmlRowInserter)).GetTypes().Where(type => !type.IsAbstract && (type == typeof(XmlImporter.IncidentXmlRowInserter) || type.IsSubclassOf(typeof(XmlImporter.IncidentXmlRowInserter)))).ToArray();
                           string[] databaseColumns = new string[] { Incident.Columns.NativeId, Incident.Columns.Time, Incident.Columns.Type, Incident.Columns.X(importArea), Incident.Columns.Y(importArea) };

                           return CreateXmlImporter(name, path, Configuration.IncidentsImportDirectory, PathRelativizationId.IncidentDirectory, sourceURI, rowInserterTypes, databaseColumns, databaseColumnInputColumn =>
                                                    {
                                                        return new XmlImporter.IncidentXmlRowInserter(databaseColumnInputColumn, importArea, hourOffset, sourceSRID);
                                                    });
                       }),

                   Configuration.IncidentsImportDirectory,
                   null, null, null);
        }

        public void clearImportedIncidentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Area clearArea = PromptForArea("Select area to clear incidents from...");
            if (clearArea != null && MessageBox.Show("Are you sure you want to clear all incidents from \"" + clearArea.Name + "\"? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                Incident.Clear(clearArea);
        }

        public void simulateIncidentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Area simulateArea = PromptForArea("Select area in which to simulate incidents...");
            if (simulateArea != null)
            {
                SimulateIncidentsForm f = new SimulateIncidentsForm(simulateArea);
                f.ShowDialog();
            }
        }

        public void clearSimulatedIncidentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Area clearSimulatedArea = PromptForArea("Select area to clear simulated incidents from...");
            if (clearSimulatedArea != null && MessageBox.Show("Are you sure you want to clear all simulated incidents from \"" + clearSimulatedArea.Name + "\"? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                Incident.ClearSimulated(clearSimulatedArea);
        }

        private void Import(string downloadDirectory,
                            CompleteImporterFormDelegate completeImporterForm,
                            CreateImporterDelegate createImporter,
                            string initialBrowsingDirectory,
                            string fileBrowserFilter,
                            string importFileSearchPattern,
                            ImportCompletionDelegate completionCallback)
        {
            Thread t = new Thread(new ThreadStart(delegate()
            {
                try
                {
                    DynamicForm importerForm = new DynamicForm("Enter import information...", MessageBoxButtons.OKCancel);

                    importerForm.AddTextBox("Import name (descriptive):", null, 70, "name");
                    importerForm.AddTextBox("Path:", null, 200, "path", addFileBrowsingButtons: true, initialBrowsingDirectory: initialBrowsingDirectory, fileFilter: fileBrowserFilter, textChanged: (o, e) =>
                        {
                            TextBox pathTextBox = o as TextBox;
                            ComboBox fileTypeCombo = importerForm.GetControl<ComboBox>("file_type");
                            string path = pathTextBox.Text.Trim().ToLower();
                            string extension = Path.GetExtension(path);
                            bool pathIsDirectory = Directory.Exists(path);
                            if (!pathIsDirectory)
                                if (extension == ".zip")
                                    fileTypeCombo.SelectedItem = ImportFileType.Zip;
                                else
                                    fileTypeCombo.SelectedItem = ImportFileType.Plain;
                        });

                    importerForm.AddTextBox("Download XML URI:", null, 200, "uri");
                    importerForm.AddDropDown("File type:", Enum.GetValues(typeof(ImportFileType)), ImportFileType.Plain, "file_type");
                    importerForm.AddCheckBox("Delete imported file after import:", ContentAlignment.MiddleRight, false, "delete");
                    importerForm.AddCheckBox("Save importer(s):", ContentAlignment.MiddleRight, false, "save_importer");

                    if (completeImporterForm != null)
                        importerForm = completeImporterForm(importerForm);

                    if (importerForm == null)
                        return;

                    if (importerForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string p = importerForm.GetValue<string>("path").Trim();
                        ImportFileType fileType = (ImportFileType)importerForm.GetValue<object>("file_type");
                        bool pathIsDirectory = Directory.Exists(p);

                        if (pathIsDirectory)
                            downloadDirectory = p;

                        #region download file if a URI is given -- some callbacks need the file in order to set up the importer
                        string sourceURI = importerForm.GetValue<string>("uri");
                        if (!string.IsNullOrWhiteSpace(sourceURI))
                        {
                            if (string.IsNullOrWhiteSpace(p) || pathIsDirectory)
                            {
                                p = Path.Combine(downloadDirectory, ReplaceInvalidFilenameCharacters("uri_download_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToLongTimeString() + (fileType == ImportFileType.Zip ? ".zip" : "")));
                                pathIsDirectory = false;
                            }

                            try { LAIR.IO.Network.Download(sourceURI, p); }
                            catch (Exception ex)
                            {
                                try { File.Delete(p); }
                                catch (Exception ex2) { Console.Out.WriteLine("Failed to delete partially downloaded file \"" + p + "\":  " + ex2.Message); }

                                Console.Out.WriteLine("Error downloading file from URI:  " + ex.Message);

                                return;
                            }
                        }
                        #endregion

                        #region decompress zip files if the user is providing them
                        if (fileType == ImportFileType.Zip)
                        {
                            string[] pathsToUnzip = new string[] { p };
                            if (pathIsDirectory)
                                pathsToUnzip = Directory.GetFiles(p, "*.zip", SearchOption.AllDirectories);

                            foreach (string pathToUnzip in pathsToUnzip)
                            {
                                string destinationDirectory = Importer.GetImportUnzipDirectory(pathToUnzip);
                                Console.Out.WriteLine("Unzipping \"" + pathToUnzip + "\" to \"" + destinationDirectory + "\"...");
                                try { ZipFile.ExtractToDirectory(pathToUnzip, destinationDirectory); }
                                catch (Exception) { }
                                if (!pathIsDirectory)
                                    p = destinationDirectory;
                            }

                            pathIsDirectory = true;
                        }
                        #endregion

                        string[] paths = new string[] { p };
                        if (pathIsDirectory)
                            paths = Directory.GetFiles(p, importFileSearchPattern == null ? "*" : importFileSearchPattern, SearchOption.AllDirectories);

                        foreach (string path in paths)
                            if (File.Exists(path))
                            {
                                string importName = importerForm.GetValue<string>("name");
                                if (paths.Length > 1)
                                    importName = "";

                                bool deleteImportedFileAfterImport = importerForm.GetValue<bool>("delete");
                                bool saveImporter = Convert.ToBoolean(importerForm.GetValue<bool>("save_importer"));

                                try
                                {
                                    Importer importer = createImporter(importName, path, sourceURI, importerForm);
                                    importer.Import();

                                    if (deleteImportedFileAfterImport)
                                        File.Delete(path);

                                    if (saveImporter)
                                        importer.Save(false);
                                }
                                catch (Exception ex)
                                {
                                    Console.Out.WriteLine("Error while importing from \"" + path + "\":  " + ex.Message);
                                }
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.Message);
                }

                if (completionCallback != null)
                    completionCallback();
            }));

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private XmlImporter CreateXmlImporter(string name, string path, string relativePathBase, PathRelativizationId relativizationId, string sourceURI, Type[] rowInserterTypes, string[] databaseColumns, CreateXmlRowInserterDelegate createXmlRowInserter)
        {
            DynamicForm rowInserterForm = new DynamicForm("Define row inserter...", MessageBoxButtons.OKCancel);

            rowInserterForm.AddDropDown("Row inserter:", rowInserterTypes, null, "row_inserter");

            string[] inputColumns = XmlImporter.GetColumnNames(path, "row", "row");
            Array.Sort(inputColumns);
            foreach (string databaseColumn in databaseColumns)
                rowInserterForm.AddDropDown(databaseColumn + ":", inputColumns, null, databaseColumn);

            XmlImporter importer = null;
            if (rowInserterForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Type rowInserterType = rowInserterForm.GetValue<Type>("row_inserter");

                Dictionary<string, string> databaseColumnInputColumn = new Dictionary<string, string>();
                foreach (string databaseColumn in databaseColumns)
                    databaseColumnInputColumn.Add(databaseColumn, rowInserterForm.GetValue<string>(databaseColumn));

                importer = new XmlImporter(name, path, RelativizePath(path, relativePathBase, relativizationId), sourceURI, createXmlRowInserter(databaseColumnInputColumn), "row", "row");
            }

            return importer;
        }

        private void manageStoredImportersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Importer[] storedImporters = Importer.GetAll().ToArray();

            Thread t = new Thread(new ThreadStart(() =>
                {
                    DialogResult manageDialogResult = System.Windows.Forms.DialogResult.OK;
                    bool refreshStoredImporters = false;
                    while (manageDialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        if (refreshStoredImporters)
                        {
                            storedImporters = Importer.GetAll().ToArray();
                            refreshStoredImporters = false;
                        }

                        DynamicForm f = new DynamicForm("Stored importers...", MessageBoxButtons.OKCancel);
                        f.AddListBox("Importers:", storedImporters, null, SelectionMode.MultiExtended, "importers");
                        f.AddDropDown("Action:", Enum.GetValues(typeof(ManageImporterAction)), null, "action");
                        if ((manageDialogResult = f.ShowDialog()) == System.Windows.Forms.DialogResult.OK)
                        {
                            ManageImporterAction action = f.GetValue<ManageImporterAction>("action");

                            if (action == ManageImporterAction.Load)
                            {
                                DynamicForm df = new DynamicForm("Select importer source...");
                                df.AddTextBox("Path:", null, 75, "path", addFileBrowsingButtons: true, fileFilter: "ATT importers|*.attimp", initialBrowsingDirectory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                                if (df.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                {
                                    string path = df.GetValue<string>("path");
                                    string[] importerPaths = null;
                                    if (Directory.Exists(path))
                                        importerPaths = Directory.GetFiles(path, "*.attimp", SearchOption.TopDirectoryOnly);
                                    else if (File.Exists(path))
                                        importerPaths = new string[] { path };

                                    if (importerPaths != null)
                                    {
                                        BinaryFormatter bf = new BinaryFormatter();
                                        foreach (string importerPath in importerPaths)
                                            using (FileStream fs = new FileStream(importerPath, FileMode.Open, FileAccess.Read))
                                            {
                                                try
                                                {
                                                    Importer importer = bf.Deserialize(fs) as Importer;

                                                    int relativizationIdEnd = importer.RelativePath.IndexOf('}');
                                                    string relativizationId = importer.RelativePath.Substring(0, relativizationIdEnd + 1).Trim('{', '}');
                                                    PathRelativizationId pathRelativizationId = (PathRelativizationId)Enum.Parse(typeof(PathRelativizationId), relativizationId);
                                                    string absolutePath = importer.RelativePath.Substring(relativizationIdEnd).Trim('}', Path.DirectorySeparatorChar);
                                                    if (pathRelativizationId == PathRelativizationId.EventDirectory)
                                                        absolutePath = Path.Combine(Configuration.EventsImportDirectory, absolutePath);
                                                    else if (pathRelativizationId == PathRelativizationId.IncidentDirectory)
                                                        absolutePath = Path.Combine(Configuration.IncidentsImportDirectory, absolutePath);
                                                    else if (pathRelativizationId == PathRelativizationId.ShapefileDirectory)
                                                        absolutePath = Path.Combine(Configuration.PostGisShapefileDirectory, absolutePath);
                                                    else
                                                        throw new NotImplementedException("Unrecognized path relativization id:  " + pathRelativizationId);

                                                    importer.Path = absolutePath;
                                                    importer.Save(false);
                                                    fs.Close();
                                                    refreshStoredImporters = true;
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.Out.WriteLine("Importer import failed:  " + ex.Message);
                                                }
                                            }
                                    }
                                }
                            }
                            else
                            {
                                string exportDirectory = null;
                                foreach (Importer importer in f.GetValue<System.Windows.Forms.ListBox.SelectedObjectCollection>("importers"))
                                    if (action == ManageImporterAction.Delete)
                                    {
                                        importer.Delete();
                                        refreshStoredImporters = true;
                                    }
                                    else if (action == ManageImporterAction.Edit)
                                    {
                                        Dictionary<string, object> updateKeyValue = new Dictionary<string, object>();
                                        DynamicForm updateForm = new DynamicForm("Update importer \"" + importer + "\"...");
                                        importer.GetUpdateRequests(new Importer.UpdateRequestDelegate((itemName, currentValue, possibleValues, id) =>
                                            {
                                                itemName += ":";

                                                if (possibleValues != null)
                                                    updateForm.AddDropDown(itemName, possibleValues.ToArray(), currentValue, id);
                                                else if (currentValue is string)
                                                    updateForm.AddTextBox(itemName, currentValue as string, -1, id);
                                                else if (currentValue is int)
                                                    updateForm.AddNumericUpdown(itemName, (int)currentValue, 0, int.MinValue, int.MaxValue, 1, id);
                                                else if (currentValue != null)
                                                    throw new NotImplementedException("Cannot dynamically generate form for update request");

                                                updateKeyValue.Add(id, currentValue);
                                            }));

                                        if (updateForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                        {
                                            foreach (string updateKey in updateKeyValue.Keys.ToArray())
                                                updateKeyValue[updateKey] = updateForm.GetValue<object>(updateKey);

                                            importer.Update(updateKeyValue);
                                            importer.Save(true);
                                            refreshStoredImporters = true;
                                        }
                                    }
                                    else if (action == ManageImporterAction.Store)
                                    {
                                        if (exportDirectory == null)
                                            exportDirectory = LAIR.IO.Directory.PromptForDirectory("Select export directory...", Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

                                        if (Directory.Exists(exportDirectory))
                                        {
                                            try
                                            {
                                                BinaryFormatter bf = new BinaryFormatter();
                                                using (FileStream fs = new FileStream(Path.Combine(exportDirectory, ReplaceInvalidFilenameCharacters(importer.ToString() + ".attimp")), FileMode.Create, FileAccess.ReadWrite))
                                                {
                                                    bf.Serialize(fs, importer);
                                                    fs.Close();
                                                    Console.Out.WriteLine("Exported \"" + importer + "\".");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.Out.WriteLine("Importer export failed:  " + ex.Message);
                                            }
                                        }
                                    }
                                    else if (action == ManageImporterAction.Run)
                                    {
                                        Console.Out.WriteLine("Running importer \"" + importer + "\"...");
                                        try { importer.Import(); }
                                        catch (Exception ex)
                                        {
                                            Console.Out.WriteLine("Import failed:  " + ex.Message);
                                        }
                                    }
                                    else
                                        MessageBox.Show("Unrecognized action:  " + action);
                            }
                        }
                    }

                    // might have imported/created an area
                    RefreshPredictionAreas();
                }));

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }
        #endregion

        #region model
        public void addModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Type[] modelTypes = Assembly.GetAssembly(typeof(DiscreteChoiceModel)).GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(DiscreteChoiceModel))).ToArray();
            if (modelTypes.Length == 0)
                MessageBox.Show("No model type are available.");
            else
            {
                DynamicForm modelForm = new DynamicForm("Select model type...", MessageBoxButtons.OKCancel);
                modelForm.AddDropDown("Model type:", modelTypes, null, "model_type");
                if (modelForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Type modelType = modelForm.GetValue<Type>("model_type");
                    int newModelId = -1;

                    if (modelType == typeof(KernelDensityDCM))
                    {
                        KernelDensityDcmForm f = new KernelDensityDcmForm();
                        if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            newModelId = KernelDensityDCM.Create(f.discreteChoiceModelOptions.ModelName,
                                                                 f.discreteChoiceModelOptions.PointSpacing,
                                                                 f.discreteChoiceModelOptions.TrainingArea,
                                                                 f.discreteChoiceModelOptions.TrainingStart,
                                                                 f.discreteChoiceModelOptions.TrainingEnd,
                                                                 f.kernelDensityDcmOptions.TrainingSampleSize,
                                                                 f.discreteChoiceModelOptions.IncidentTypes,
                                                                 f.kernelDensityDcmOptions.Normalize,
                                                                 f.discreteChoiceModelOptions.Smoothers);
                    }
                    else if (modelType == typeof(FeatureBasedDCM))
                    {
                        FeatureBasedDcmForm f = new FeatureBasedDcmForm();
                        if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            newModelId = FeatureBasedDCM.Create(null,
                                                                f.discreteChoiceModelOptions.ModelName,
                                                                f.discreteChoiceModelOptions.PointSpacing,
                                                                f.featureBasedDcmOptions.FeatureDistanceThreshold,
                                                                null,
                                                                f.discreteChoiceModelOptions.TrainingArea,
                                                                f.discreteChoiceModelOptions.TrainingStart,
                                                                f.discreteChoiceModelOptions.TrainingEnd,
                                                                f.featureBasedDcmOptions.TrainingSampleSize,
                                                                f.featureBasedDcmOptions.PredictionSampleSize,
                                                                f.discreteChoiceModelOptions.IncidentTypes,
                                                                f.featureBasedDcmOptions.Classifier,
                                                                f.discreteChoiceModelOptions.Smoothers,
                                                                f.featureBasedDcmOptions.Features);
                    }
                    else if (modelType == typeof(TimeSliceDCM))
                    {
                        TimeSliceDcmForm f = new TimeSliceDcmForm();
                        if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            newModelId = TimeSliceDCM.Create(null,
                                                             f.discreteChoiceModelOptions.ModelName,
                                                             f.discreteChoiceModelOptions.PointSpacing,
                                                             f.featureBasedDcmOptions.FeatureDistanceThreshold,
                                                             null,
                                                             f.discreteChoiceModelOptions.TrainingArea,
                                                             f.discreteChoiceModelOptions.TrainingStart,
                                                             f.discreteChoiceModelOptions.TrainingEnd,
                                                             f.featureBasedDcmOptions.TrainingSampleSize,
                                                             f.featureBasedDcmOptions.PredictionSampleSize,
                                                             f.discreteChoiceModelOptions.IncidentTypes,
                                                             f.featureBasedDcmOptions.Classifier,
                                                             f.discreteChoiceModelOptions.Smoothers,
                                                             f.featureBasedDcmOptions.Features,
                                                             f.timeSliceDcmOptions.TimeSliceHours,
                                                             f.timeSliceDcmOptions.TimeSlicesPerPeriod);
                    }

                    if (newModelId >= 0)
                        RefreshModels(newModelId);
                }
            }
        }

        public void editModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedModel == null)
                MessageBox.Show("Select model to edit.");
            else
            {
                DiscreteChoiceModel m = SelectedModel;

                if (m is TimeSliceDCM)
                {
                    TimeSliceDCM ts = m as TimeSliceDCM;
                    TimeSliceDcmForm f = new TimeSliceDcmForm(ts);
                    if (f.ShowDialog() == DialogResult.OK)
                        ts.Update(f.discreteChoiceModelOptions.ModelName,
                                  f.discreteChoiceModelOptions.PointSpacing,
                                  f.featureBasedDcmOptions.FeatureDistanceThreshold,
                                  f.discreteChoiceModelOptions.TrainingArea,
                                  f.discreteChoiceModelOptions.TrainingStart,
                                  f.discreteChoiceModelOptions.TrainingEnd,
                                  f.featureBasedDcmOptions.TrainingSampleSize,
                                  f.featureBasedDcmOptions.PredictionSampleSize,
                                  f.discreteChoiceModelOptions.IncidentTypes,
                                  f.featureBasedDcmOptions.Classifier,
                                  f.discreteChoiceModelOptions.Smoothers,
                                  f.featureBasedDcmOptions.Features,
                                  f.timeSliceDcmOptions.TimeSliceHours,
                                  f.timeSliceDcmOptions.TimeSlicesPerPeriod);
                }
                else if (m is FeatureBasedDCM)
                {
                    FeatureBasedDCM fb = m as FeatureBasedDCM;
                    FeatureBasedDcmForm f = new FeatureBasedDcmForm(fb);
                    if (f.ShowDialog() == DialogResult.OK)
                        fb.Update(f.discreteChoiceModelOptions.ModelName,
                                  f.discreteChoiceModelOptions.PointSpacing,
                                  f.featureBasedDcmOptions.FeatureDistanceThreshold,
                                  f.discreteChoiceModelOptions.TrainingArea,
                                  f.discreteChoiceModelOptions.TrainingStart,
                                  f.discreteChoiceModelOptions.TrainingEnd,
                                  f.featureBasedDcmOptions.TrainingSampleSize,
                                  f.featureBasedDcmOptions.PredictionSampleSize,
                                  f.discreteChoiceModelOptions.IncidentTypes,
                                  f.featureBasedDcmOptions.Classifier,
                                  f.discreteChoiceModelOptions.Smoothers,
                                  f.featureBasedDcmOptions.Features);
                }
                else if (m is KernelDensityDCM)
                {
                    KernelDensityDCM kde = m as KernelDensityDCM;
                    KernelDensityDcmForm f = new KernelDensityDcmForm(kde);
                    if (f.ShowDialog() == DialogResult.OK)
                        kde.Update(f.discreteChoiceModelOptions.ModelName,
                                   f.discreteChoiceModelOptions.PointSpacing,
                                   f.discreteChoiceModelOptions.TrainingArea,
                                   f.discreteChoiceModelOptions.TrainingStart,
                                   f.discreteChoiceModelOptions.TrainingEnd,
                                   f.kernelDensityDcmOptions.TrainingSampleSize,
                                   f.discreteChoiceModelOptions.IncidentTypes,
                                   f.kernelDensityDcmOptions.Normalize,
                                   f.discreteChoiceModelOptions.Smoothers);
                }

                RefreshModels(m.Id);
            }
        }

        public void deleteModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DiscreteChoiceModel m = SelectedModel;
            if (m == null)
                MessageBox.Show("Select a model to delete.");
            else if (MessageBox.Show("Are you sure you want to delete model \"" + m.Name + "\"?", "Confirm delete...", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                m.Delete();
                RefreshAll();
            }
        }

        private void models_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = null;
            if (SelectedModel != null)
            {
                text = SelectedModel.GetDetails(0);
                perIncident.Enabled = SelectedModel.IncidentTypes.Count > 1;
            }

            toolTip.SetToolTip(models, text);
        }
        #endregion

        #region prediction area
        private void predictionAreas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedPredictionArea != null)
                toolTip.SetToolTip(predictionAreas, SelectedPredictionArea.GetDetails(0));
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
                MessageBox.Show("Select a model to run.");
            else
            {
                string defaultPredictionName = m.Name + " (" + m.GetType().Name + ")" + (!perIncident.Checked ? " " + m.IncidentTypes.Concatenate("+") : "");
                string predictionName = GetValue.Show("Enter name for prediction" + (perIncident.Checked ? " (per-incident names will be added)" : "") + "...", defaultPredictionName);
                if (predictionName == null)
                    return;

                Run(true, predictionName, null);
            }
        }

        public void Run(bool newRun, string predictionName, Action<int> runFinishedCallback)
        {
            DiscreteChoiceModel m = SelectedModel;

            predictionName = predictionName.Trim();

            if (m == null)
                MessageBox.Show("Select a model to run.");
            else if (SelectedPredictionArea == null)
                MessageBox.Show("Select a prediction area.");
            else if (predictionName == "")
                MessageBox.Show("Provide a non-empty prediction name.");
            else
            {
                Area predictionArea = SelectedPredictionArea;
                IEnumerable<string> incidentTypes = m.IncidentTypes.ToArray();

                Thread t = new Thread(new ThreadStart(delegate()
                    {
                        int mostRecentPredictionId = -1;

                        DateTime trainingStart = m.TrainingStart;
                        DateTime trainingEnd = m.TrainingEnd;

                        for (int i = (int)startPrediction.Value - 1; i < numPredictions.Value; ++i)
                        {
                            Console.Out.WriteLine("Running prediction \"" + predictionName + "\" (" + (i + 1) + " of " + numPredictions.Value + ")");

                            TimeSpan offset = new TimeSpan(0, i * (int)predictionSpacingHours.Value, 0, 0);

                            if (perIncident.Checked)
                            {
                                foreach (string incidentType in incidentTypes)
                                {
                                    Console.Out.WriteLine("Running per-incident prediction \"" + incidentType + "\"");

                                    try
                                    {
                                        m.Update(m.Name, m.PointSpacing, m.TrainingArea, trainingStart + (slideTrainingStart.Checked ? offset : new TimeSpan(0L)), trainingEnd + (slideTrainingEnd.Checked ? offset : new TimeSpan(0L)), new string[] { incidentType }, m.Smoothers);
                                        mostRecentPredictionId = m.Run(predictionArea, PredictionStartDateTime + offset, PredictionEndDateTime + offset, predictionName + " " + incidentType + (numPredictions.Value > 1 ? " " + (i + 1) : ""), newRun);
                                        newRun = false;
                                    }
                                    catch (Exception ex)
                                    {
                                        string msg = "An error occurred while running prediction:  " + ex.Message + Environment.NewLine +
                                                      ex.StackTrace;
                                        Console.Out.WriteLine(msg);
                                        Notify("Error", msg);
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    m.Update(m.Name, m.PointSpacing, m.TrainingArea, trainingStart + (slideTrainingStart.Checked ? offset : new TimeSpan(0L)), trainingEnd + (slideTrainingEnd.Checked ? offset : new TimeSpan(0L)), m.IncidentTypes, m.Smoothers);
                                    mostRecentPredictionId = m.Run(predictionArea, PredictionStartDateTime + offset, PredictionEndDateTime + offset, predictionName + (numPredictions.Value > 1 ? " " + (i + 1) : ""), newRun);
                                    newRun = false;
                                }
                                catch (Exception ex)
                                {
                                    string msg = "An error occurred while running prediction:  " + ex.Message + Environment.NewLine +
                                                  ex.StackTrace;
                                    Console.Out.WriteLine(msg);
                                    Notify("Error", msg);
                                }
                            }

                            Console.Out.WriteLine("Completed prediction \"" + predictionName + "\" (" + (i + 1) + " of " + numPredictions.Value + ")");
                        }

                        m.Update(m.Name, m.PointSpacing, m.TrainingArea, trainingStart, trainingEnd, incidentTypes, m.Smoothers);
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
            else if (_groups[groupNum] == groupByIncidentTypesToolStripMenuItem.Text)
                grouper = p => "Incidents:  " + p.Model.IncidentTypes.OrderBy(i => i).Concatenate(", ");
            else if (_groups[groupNum] == groupByRunToolStripMenuItem.Text)
                grouper = p => "Run:  " + p.RunId;
            else if (_groups[groupNum] == groupByPredictionIntervalToolStripMenuItem.Text)
                grouper = p => "Time:  " + p.PredictionStartTime.ToShortDateString() + " " + p.PredictionStartTime.ToShortTimeString() + " -- " + p.PredictionEndTime.ToShortDateString() + " " + p.PredictionEndTime.ToShortTimeString();
            else
            {
                MessageBox.Show("Grouping \"" + _groups[groupNum] + "\" not implemented. Please send this message to the developers.");
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
            SetPredictionsTooltip(SelectedPredictions.Count == 1 ? TraversePredictionTree().Where(n => n.Tag is Prediction && SelectedPredictions.Contains(n.Tag as Prediction)).First() : null);
        }

        private void SetPredictionsTooltip(TreeNode selectedNode)
        {
            Prediction prediction = null;

            if (selectedNode != null && selectedNode.Checked && selectedNode.Tag is Prediction)
                prediction = selectedNode.Tag as Prediction;

            SetPredictionsTooltip(prediction);
        }

        private void SetPredictionsTooltip(Prediction prediction)
        {
            if (predictions.InvokeRequired)
            {
                predictions.Invoke(new Action<Prediction>(SetPredictionsTooltip), prediction);
                return;
            }

            if (_setPredictionsToolTip)
            {
                toolTip.SetToolTip(predictions, null);

                if (prediction != null)
                    toolTip.SetToolTip(predictions, prediction.GetDetails(0));
            }
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
                                    SetPredictionsTooltip(p);
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

                            if (p.Model is IFeatureBasedDCM)
                            {
                                ICollection<Feature> features = (p.Model as IFeatureBasedDCM).Features;
                                if (features.Count > 0)
                                {
                                    Dictionary<int, int> featureIdViewPriority = new Dictionary<int, int>();
                                    foreach (Feature f in features.OrderBy(f => f.Id))
                                        featureIdViewPriority.Add(f.Id, featureIdViewPriority.Count + 1);

                                    int minId = features.Min(f => f.Id);
                                    foreach (Feature f in features)
                                    {
                                        Thread t = new Thread(new ParameterizedThreadStart(delegate(object o)
                                            {
                                                Feature feature = o as Feature;
                                                if (feature.EnumType == typeof(FeatureBasedDCM.FeatureType) && (feature.EnumValue.Equals(FeatureBasedDCM.FeatureType.MinimumDistanceToGeometry) ||
                                                                                                                feature.EnumValue.Equals(FeatureBasedDCM.FeatureType.GeometryDensity)))
                                                {
                                                    Shapefile shapefile = new Shapefile(int.Parse(feature.PredictionResourceId));
                                                    NpgsqlConnection connection = DB.Connection.OpenConnection;
                                                    List<List<PointF>> points = Geometry.GetPoints(connection, ShapefileGeometry.GetTableName(shapefile), ShapefileGeometry.Columns.Geometry, ShapefileGeometry.Columns.Id, null, pointDistanceThreshold);
                                                    DB.Connection.Return(connection);
                                                    lock (overlays) { overlays.Add(new Overlay(feature.Description, points, ColorPalette.GetColor(), false, featureIdViewPriority[f.Id])); }
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

                            threatMap.Display(p, overlays);

                            Invoke(new Action(RefreshAssessmentPlots));
                        }));

                    displayThread.Start();
                }
            }
            else
                MessageBox.Show("Select a single prediction to display.");
        }

        public void editPredictionNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedPredictions.Count == 0)
                MessageBox.Show("Select one or more predictions to rename.");
            else
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
        }

        private void editPredictionRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedPredictions.Count == 0)
                MessageBox.Show("Select one or more predictions to edit run number for.");
            else
            {
                string newRunIdStr = GetValue.Show("New run number for " + SelectedPredictions.Count + " prediction(s).");
                if (newRunIdStr == null)
                    return;

                int newRunId;
                if (int.TryParse(newRunIdStr, out newRunId))
                    foreach (Prediction prediction in SelectedPredictions)
                        prediction.RunId = newRunId;

                RefreshPredictions(SelectedPredictions.Select(p => p.Id));
            }
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
                                    try
                                    {
                                        int copyId = selectedPrediction.Copy("Copy " + copyNum + " of " + selectedPrediction.Name, predictionNum == 1 && copyNum == 1, false);
                                        Point.VacuumTable(copyId);
                                        PointPrediction.VacuumTable(copyId);
                                    }
                                    catch (Exception ex) { Console.Out.WriteLine("Error while copying prediction:  " + ex.Message); }
                                }
                            }

                            try { Prediction.VacuumTable(); }
                            catch (Exception ex) { Console.Out.WriteLine("Failed to vacuum " + Prediction.Table + ":  " + ex.Message); }

                            string msg = "Done copying predictions";
                            Console.Out.WriteLine(msg);
                            Notify(msg, "");

                            RefreshPredictions(selectedPredictions.Select(p => p.Id).ToArray());
                        }));

                    t.Start();
                }
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

                                    if (File.Exists(selectedPrediction.PointPredictionLogPath))
                                    {
                                        DiscreteChoiceModel model = selectedPrediction.Model;
                                        Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> oldLog = model.ReadPointPredictionLog(selectedPrediction.PointPredictionLogPath);
                                        Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> newLog = new Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>>();
                                        foreach (PointPrediction pointPrediction in selectedPrediction.PointPredictions)
                                        {
                                            List<Tuple<string, double>> smoothedIncidentScore = new List<Tuple<string, double>>();
                                            foreach (string incident in pointPrediction.IncidentScore.Keys)
                                                smoothedIncidentScore.Add(new Tuple<string, double>(incident, Math.Round(pointPrediction.IncidentScore[incident], 3)));

                                            string logPointId = model.GetPointIdForLog(pointPrediction.PointId, pointPrediction.Time);
                                            newLog.Add(logPointId, new Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>(smoothedIncidentScore, oldLog[logPointId].Item2));
                                        }

                                        model.WritePointPredictionLog(newLog, selectedPrediction.PointPredictionLogPath);
                                    }
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
            if (TraversePredictionTree().Count(n => n.Checked) <= 1)
                MessageBox.Show("Select two or more predictions / groups to make a comparison.");
            else
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
                            if (series != DiscreteChoiceModel.OptimalSeriesName)
                            {
                                string baseSeriesTitle = plotTitle;
                                if (series == DiscreteChoiceModel.OptimalSeriesName)
                                    baseSeriesTitle = DiscreteChoiceModel.OptimalSeriesName + " " + baseSeriesTitle;

                                string seriesTitle = baseSeriesTitle;
                                int dupNameNum = 2;
                                while (seriesPoints.Keys.Count(k => k == seriesTitle) > 0)
                                    seriesTitle = baseSeriesTitle + " " + dupNameNum++;

                                seriesPoints.Add(seriesTitle, selectedPlot.SeriesPoints[series]);
                            }
                    }

                    SurveillancePlot comparisonPlot = new SurveillancePlot(comparisonTitle.ToString(), seriesPoints, 500, 500, Plot.Format.JPEG, 2);
                    List<TitledImage> comparisonPlotImages = new List<TitledImage>(new TitledImage[] { new TitledImage(comparisonPlot.Image, null) });
                    new ImageViewer(comparisonPlotImages, 0).ShowDialog();
                }
            }
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
                MessageBox.Show("Select at least two predictions to run an aggregate evaluation.");
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
            if (SelectedPredictions.Count == 0)
                MessageBox.Show("Select one or more predictions to view model directories.");
            else
                foreach (Prediction selectedPrediction in SelectedPredictions)
                {
                    try { Process.Start(selectedPrediction.Model.ModelDirectory); }
                    catch (Exception ex) { MessageBox.Show("Failed to open prediction model directory:  " + ex.Message); }
                }
        }

        public void deletePredictionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Prediction> selectedPredictions = SelectedPredictions;
            if (selectedPredictions.Count == 0)
                MessageBox.Show("Select one or more predictions to delete.");
            else if (MessageBox.Show("Are you sure you want to delete " + selectedPredictions.Count + " prediction(s)?", "Confirm delete", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
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
                        catch (Exception ex) { Console.Out.WriteLine("Failed to vacuum " + Prediction.Table + ":  " + ex.Message); }

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

            models.Items.Clear();
            predictions.Nodes.Clear();
            threatMap.Clear();
            assessments.ClearPlots();

            try
            {
                RefreshModels(-1);
                RefreshPredictionAreas();
                RefreshPredictions(-1);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("Failed to refresh information from database:  " + ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        public void RefreshModels(int modelIdToSelect)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(RefreshModels), modelIdToSelect);
                return;
            }

            toolTip.SetToolTip(models, null);

            models.Items.Clear();

            foreach (DiscreteChoiceModel m in DiscreteChoiceModel.GetAll(true))
                models.Items.Add(m);

            if (models.Items.Count > 0)
            {
                if (modelIdToSelect < 0)
                    modelIdToSelect = (models.Items[0] as DiscreteChoiceModel).Id;

                models.SelectedIndex = models.Items.IndexOf(models.Items.Cast<DiscreteChoiceModel>().Where(m => m.Id == modelIdToSelect).First());
            }
        }

        public void RefreshPredictionAreas()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshPredictionAreas));
                return;
            }

            toolTip.SetToolTip(predictionAreas, null);

            predictionAreas.Items.Clear();
            foreach (Area area in Area.GetAll())
                predictionAreas.Items.Add(area);

            if (predictionAreas.Items.Count > 0)
                predictionAreas.SelectedIndex = 0;
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

            predictions.BeginUpdate();

            predictions.Nodes.Clear();
            toolTip.SetToolTip(predictions, null);

            List<Prediction> allPredictions = Prediction.GetAll();
            allPredictions.Sort(new Comparison<Prediction>((p1, p2) => p1.Id.CompareTo(p2.Id)));
            foreach (PredictionGroup group in Group(allPredictions, _groups, 0))
                AddToTree(predictions.Nodes, group);

            predictions.EndUpdate();

            if (predictionIdsToSelect != null)
            {
                _setPredictionsToolTip = predictionIdsToSelect.Count() == 1;
                TraversePredictionTree().Where(n => n.Tag is Prediction && predictionIdsToSelect.Contains((n.Tag as Prediction).Id)).Select(n => n.Checked = true).ToArray();
            }

            _setPredictionsToolTip = true;
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

        private void encryptTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string textToEncrypt = null;

            while (true)
            {
                DynamicForm f = new DynamicForm("Encrypt text...", MessageBoxButtons.OKCancel);
                f.AddTextBox("Text:", null, 20, "text", '*', true);
                f.AddTextBox("Confirm text:", null, 20, "confirmed", '*', true);

                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textToEncrypt = f.GetValue<string>("text").Trim();
                    string confirmedText = f.GetValue<string>("confirmed").Trim();
                    if (textToEncrypt.Length == 0)
                        MessageBox.Show("Empty text is not allowed.");
                    else if (textToEncrypt != confirmedText)
                        MessageBox.Show("Entries do not match.");
                    else
                        break;
                }
                else
                    return;
            }

            try
            {
                DynamicForm showEncrypted = new DynamicForm("Encrypted text", MessageBoxButtons.OK);
                showEncrypted.AddTextBox("Result:", textToEncrypt.Encrypt(Configuration.EncryptionKey, Configuration.EncryptionInitialization).Select(b => b.ToString()).Concatenate("-"), -1, "encrypted");
                showEncrypted.ShowDialog();
            }
            catch (Exception ex) { MessageBox.Show("Error getting encrypted text:  " + ex.Message); }
        }
        #endregion

        #region about
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(ATT.Configuration.LicenseText, "About the Asymmetric Threat Tracker");
        }
        #endregion

        #region miscellaneous
        private string RelativizePath(string path, string relativePathBase, PathRelativizationId relativizationId)
        {
            if (!path.StartsWith(relativePathBase))
                return path;

            return "{" + relativizationId + "}" + Path.DirectorySeparatorChar + path.Substring(relativePathBase.Length).Trim(Path.DirectorySeparatorChar);
        }

        private string ReplaceInvalidFilenameCharacters(string fileName)
        {
            return new string(fileName.Select(c => c == ' ' || Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());
        }

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

        private Area PromptForArea(string prompt)
        {
            Area[] areas = Area.GetAll().ToArray();
            if (areas.Length == 0)
            {
                MessageBox.Show("No areas available.");
                return null;
            }

            DynamicForm f = new DynamicForm(prompt, MessageBoxButtons.OKCancel);
            f.AddDropDown("Areas:", areas, null, "area");
            Area importArea = null;
            if (f.ShowDialog() == DialogResult.OK)
                importArea = f.GetValue<Area>("area");

            return importArea;
        }
        #endregion
    }
}
