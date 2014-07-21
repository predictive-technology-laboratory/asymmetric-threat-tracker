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

namespace PTL.ATT.GUI
{
    partial class AttForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AttForm));
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notificationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendNotificationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendTestNotificationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.encryptTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetEncryptionKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.viewLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.geographicDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importShapefilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importPointfilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteGeographicDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.incidentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importIncidentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseIncidentTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearImportedIncidentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.simulateIncidentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearSimulatedIncidentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.manageStoredImportersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.status = new System.Windows.Forms.StatusStrip();
            this.models = new System.Windows.Forms.ComboBox();
            this.modelMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label9 = new System.Windows.Forms.Label();
            this.run = new System.Windows.Forms.Button();
            this.predictionAreas = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.predictionEndDate = new System.Windows.Forms.DateTimePicker();
            this.predictionStartDate = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.slideTrainingEnd = new System.Windows.Forms.CheckBox();
            this.label15 = new System.Windows.Forms.Label();
            this.predictions = new System.Windows.Forms.TreeView();
            this.predictionsMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.displayPredictionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupByToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.predictionGroups = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.groupByIncidentTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupByPredictionIntervalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupByRunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deselectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.setToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editPredictionNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editPredictionRunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyPredictionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deletePredictionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.smoothPredictionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.comparePredictionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aggregateAndEvaluateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.showModelDetailsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openModelDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.slideTrainingStart = new System.Windows.Forms.CheckBox();
            this.startPrediction = new System.Windows.Forms.NumericUpDown();
            this.label14 = new System.Windows.Forms.Label();
            this.perIncident = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.predictionEndTime = new System.Windows.Forms.DateTimePicker();
            this.predictionStartTime = new System.Windows.Forms.DateTimePicker();
            this.label13 = new System.Windows.Forms.Label();
            this.numPredictions = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.predictionSpacingHours = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.verticalSplitContainer = new System.Windows.Forms.SplitContainer();
            this.threatSplitContainer = new System.Windows.Forms.SplitContainer();
            this.threatMap = new PTL.ATT.GUI.Visualization.ThreatMap();
            this.assessments = new PTL.ATT.GUI.Visualization.Assessments();
            this.horizontalSplitContainer = new System.Windows.Forms.SplitContainer();
            this.log = new System.Windows.Forms.RichTextBox();
            this.mainMenu.SuspendLayout();
            this.modelMenu.SuspendLayout();
            this.predictionsMenu.SuspendLayout();
            this.predictionGroups.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.startPrediction)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPredictions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.predictionSpacingHours)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.verticalSplitContainer)).BeginInit();
            this.verticalSplitContainer.Panel1.SuspendLayout();
            this.verticalSplitContainer.Panel2.SuspendLayout();
            this.verticalSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.threatSplitContainer)).BeginInit();
            this.threatSplitContainer.Panel1.SuspendLayout();
            this.threatSplitContainer.Panel2.SuspendLayout();
            this.threatSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.horizontalSplitContainer)).BeginInit();
            this.horizontalSplitContainer.Panel1.SuspendLayout();
            this.horizontalSplitContainer.Panel2.SuspendLayout();
            this.horizontalSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.importToolStripMenuItem1,
            this.pluginsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(1126, 24);
            this.mainMenu.TabIndex = 0;
            this.mainMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.notificationsToolStripMenuItem,
            this.toolStripSeparator8,
            this.encryptTextToolStripMenuItem,
            this.resetEncryptionKeyToolStripMenuItem,
            this.toolStripSeparator7,
            this.viewLogToolStripMenuItem,
            this.deleteLogToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // notificationsToolStripMenuItem
            // 
            this.notificationsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sendNotificationsToolStripMenuItem,
            this.sendTestNotificationToolStripMenuItem});
            this.notificationsToolStripMenuItem.Name = "notificationsToolStripMenuItem";
            this.notificationsToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.notificationsToolStripMenuItem.Text = "Notifications";
            // 
            // sendNotificationsToolStripMenuItem
            // 
            this.sendNotificationsToolStripMenuItem.CheckOnClick = true;
            this.sendNotificationsToolStripMenuItem.Name = "sendNotificationsToolStripMenuItem";
            this.sendNotificationsToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.sendNotificationsToolStripMenuItem.Text = "Send notifications";
            this.sendNotificationsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.sendNotificationsToolStripMenuItem_CheckedChanged);
            // 
            // sendTestNotificationToolStripMenuItem
            // 
            this.sendTestNotificationToolStripMenuItem.Name = "sendTestNotificationToolStripMenuItem";
            this.sendTestNotificationToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.sendTestNotificationToolStripMenuItem.Text = "Send test notification";
            this.sendTestNotificationToolStripMenuItem.Click += new System.EventHandler(this.sendTestNotificationToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(180, 6);
            // 
            // encryptTextToolStripMenuItem
            // 
            this.encryptTextToolStripMenuItem.Name = "encryptTextToolStripMenuItem";
            this.encryptTextToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.encryptTextToolStripMenuItem.Text = "Encrypt text...";
            this.encryptTextToolStripMenuItem.Click += new System.EventHandler(this.encryptTextToolStripMenuItem_Click);
            // 
            // resetEncryptionKeyToolStripMenuItem
            // 
            this.resetEncryptionKeyToolStripMenuItem.Name = "resetEncryptionKeyToolStripMenuItem";
            this.resetEncryptionKeyToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.resetEncryptionKeyToolStripMenuItem.Text = "Reset encryption key";
            this.resetEncryptionKeyToolStripMenuItem.Click += new System.EventHandler(this.resetEncryptionKeyToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(180, 6);
            // 
            // viewLogToolStripMenuItem
            // 
            this.viewLogToolStripMenuItem.Name = "viewLogToolStripMenuItem";
            this.viewLogToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.viewLogToolStripMenuItem.Text = "View log";
            this.viewLogToolStripMenuItem.Click += new System.EventHandler(this.viewLogToolStripMenuItem_Click);
            // 
            // deleteLogToolStripMenuItem
            // 
            this.deleteLogToolStripMenuItem.Name = "deleteLogToolStripMenuItem";
            this.deleteLogToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.deleteLogToolStripMenuItem.Text = "Delete log";
            this.deleteLogToolStripMenuItem.Click += new System.EventHandler(this.deleteLogToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(180, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // importToolStripMenuItem1
            // 
            this.importToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.geographicDataToolStripMenuItem,
            this.incidentsToolStripMenuItem,
            this.toolStripSeparator10,
            this.manageStoredImportersToolStripMenuItem});
            this.importToolStripMenuItem1.Name = "importToolStripMenuItem1";
            this.importToolStripMenuItem1.Size = new System.Drawing.Size(43, 20);
            this.importToolStripMenuItem1.Text = "Data";
            // 
            // geographicDataToolStripMenuItem
            // 
            this.geographicDataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importShapefilesToolStripMenuItem,
            this.importPointfilesToolStripMenuItem,
            this.deleteGeographicDataToolStripMenuItem});
            this.geographicDataToolStripMenuItem.Name = "geographicDataToolStripMenuItem";
            this.geographicDataToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.geographicDataToolStripMenuItem.Text = "Geographic data";
            // 
            // importShapefilesToolStripMenuItem
            // 
            this.importShapefilesToolStripMenuItem.Name = "importShapefilesToolStripMenuItem";
            this.importShapefilesToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.importShapefilesToolStripMenuItem.Text = "Import shapefiles...";
            this.importShapefilesToolStripMenuItem.Click += new System.EventHandler(this.importShapefilesToolStripMenuItem_Click);
            // 
            // importPointfilesToolStripMenuItem
            // 
            this.importPointfilesToolStripMenuItem.Name = "importPointfilesToolStripMenuItem";
            this.importPointfilesToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.importPointfilesToolStripMenuItem.Text = "Import pointfiles...";
            this.importPointfilesToolStripMenuItem.Click += new System.EventHandler(this.importPointfilesToolStripMenuItem_Click);
            // 
            // deleteGeographicDataToolStripMenuItem
            // 
            this.deleteGeographicDataToolStripMenuItem.Name = "deleteGeographicDataToolStripMenuItem";
            this.deleteGeographicDataToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.deleteGeographicDataToolStripMenuItem.Text = "Delete...";
            this.deleteGeographicDataToolStripMenuItem.Click += new System.EventHandler(this.deleteGeographicDataToolStripMenuItem_Click);
            // 
            // incidentsToolStripMenuItem
            // 
            this.incidentsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importIncidentsToolStripMenuItem,
            this.collapseIncidentTypesToolStripMenuItem,
            this.clearImportedIncidentsToolStripMenuItem,
            this.toolStripSeparator6,
            this.simulateIncidentsToolStripMenuItem,
            this.clearSimulatedIncidentsToolStripMenuItem});
            this.incidentsToolStripMenuItem.Name = "incidentsToolStripMenuItem";
            this.incidentsToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.incidentsToolStripMenuItem.Text = "Incidents";
            // 
            // importIncidentsToolStripMenuItem
            // 
            this.importIncidentsToolStripMenuItem.Name = "importIncidentsToolStripMenuItem";
            this.importIncidentsToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.importIncidentsToolStripMenuItem.Text = "Import incidents...";
            this.importIncidentsToolStripMenuItem.Click += new System.EventHandler(this.importIncidentsToolStripMenuItem_Click);
            // 
            // collapseIncidentTypesToolStripMenuItem
            // 
            this.collapseIncidentTypesToolStripMenuItem.Name = "collapseIncidentTypesToolStripMenuItem";
            this.collapseIncidentTypesToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.collapseIncidentTypesToolStripMenuItem.Text = "Collapse incident types...";
            this.collapseIncidentTypesToolStripMenuItem.Click += new System.EventHandler(this.collapseIncidentTypesToolStripMenuItem_Click);
            // 
            // clearImportedIncidentsToolStripMenuItem
            // 
            this.clearImportedIncidentsToolStripMenuItem.Name = "clearImportedIncidentsToolStripMenuItem";
            this.clearImportedIncidentsToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.clearImportedIncidentsToolStripMenuItem.Text = "Clear imported incidents...";
            this.clearImportedIncidentsToolStripMenuItem.Click += new System.EventHandler(this.clearImportedIncidentsToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(213, 6);
            // 
            // simulateIncidentsToolStripMenuItem
            // 
            this.simulateIncidentsToolStripMenuItem.Name = "simulateIncidentsToolStripMenuItem";
            this.simulateIncidentsToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.simulateIncidentsToolStripMenuItem.Text = "Simulate incidents...";
            this.simulateIncidentsToolStripMenuItem.Click += new System.EventHandler(this.simulateIncidentsToolStripMenuItem_Click);
            // 
            // clearSimulatedIncidentsToolStripMenuItem
            // 
            this.clearSimulatedIncidentsToolStripMenuItem.Name = "clearSimulatedIncidentsToolStripMenuItem";
            this.clearSimulatedIncidentsToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.clearSimulatedIncidentsToolStripMenuItem.Text = "Clear simulated incidents...";
            this.clearSimulatedIncidentsToolStripMenuItem.Click += new System.EventHandler(this.clearSimulatedIncidentsToolStripMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(213, 6);
            // 
            // manageStoredImportersToolStripMenuItem
            // 
            this.manageStoredImportersToolStripMenuItem.Name = "manageStoredImportersToolStripMenuItem";
            this.manageStoredImportersToolStripMenuItem.Size = new System.Drawing.Size(216, 22);
            this.manageStoredImportersToolStripMenuItem.Text = "Manage stored importers...";
            this.manageStoredImportersToolStripMenuItem.Click += new System.EventHandler(this.manageStoredImportersToolStripMenuItem_Click);
            // 
            // pluginsToolStripMenuItem
            // 
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.pluginsToolStripMenuItem.Text = "Plugins";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.aboutToolStripMenuItem.Text = "About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // status
            // 
            this.status.Location = new System.Drawing.Point(0, 813);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(1126, 22);
            this.status.TabIndex = 4;
            // 
            // models
            // 
            this.models.ContextMenuStrip = this.modelMenu;
            this.models.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.models.FormattingEnabled = true;
            this.models.Location = new System.Drawing.Point(139, 22);
            this.models.Name = "models";
            this.models.Size = new System.Drawing.Size(227, 21);
            this.models.Sorted = true;
            this.models.TabIndex = 4;
            this.models.SelectedIndexChanged += new System.EventHandler(this.models_SelectedIndexChanged);
            // 
            // modelMenu
            // 
            this.modelMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addModelToolStripMenuItem,
            this.editModelToolStripMenuItem,
            this.deleteModelToolStripMenuItem});
            this.modelMenu.Name = "modelMenu";
            this.modelMenu.Size = new System.Drawing.Size(108, 70);
            // 
            // addModelToolStripMenuItem
            // 
            this.addModelToolStripMenuItem.Name = "addModelToolStripMenuItem";
            this.addModelToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.addModelToolStripMenuItem.Text = "Add";
            this.addModelToolStripMenuItem.Click += new System.EventHandler(this.addModelToolStripMenuItem_Click);
            // 
            // editModelToolStripMenuItem
            // 
            this.editModelToolStripMenuItem.Name = "editModelToolStripMenuItem";
            this.editModelToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.editModelToolStripMenuItem.Text = "Edit";
            this.editModelToolStripMenuItem.Click += new System.EventHandler(this.editModelToolStripMenuItem_Click);
            // 
            // deleteModelToolStripMenuItem
            // 
            this.deleteModelToolStripMenuItem.Name = "deleteModelToolStripMenuItem";
            this.deleteModelToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteModelToolStripMenuItem.Text = "Delete";
            this.deleteModelToolStripMenuItem.Click += new System.EventHandler(this.deleteModelToolStripMenuItem_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(94, 25);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(39, 13);
            this.label9.TabIndex = 72;
            this.label9.Text = "Model:";
            this.toolTip.SetToolTip(this.label9, "Model to use for making predictions");
            // 
            // run
            // 
            this.run.Location = new System.Drawing.Point(319, 176);
            this.run.Name = "run";
            this.run.Size = new System.Drawing.Size(36, 23);
            this.run.TabIndex = 17;
            this.run.Text = "Run";
            this.toolTip.SetToolTip(this.run, "Run the prediction as configured");
            this.run.UseVisualStyleBackColor = true;
            this.run.Click += new System.EventHandler(this.run_Click);
            // 
            // predictionAreas
            // 
            this.predictionAreas.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.predictionAreas.FormattingEnabled = true;
            this.predictionAreas.Location = new System.Drawing.Point(139, 49);
            this.predictionAreas.Name = "predictionAreas";
            this.predictionAreas.Size = new System.Drawing.Size(227, 21);
            this.predictionAreas.Sorted = true;
            this.predictionAreas.TabIndex = 6;
            this.predictionAreas.SelectedIndexChanged += new System.EventHandler(this.predictionAreas_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(52, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 95;
            this.label1.Text = "Prediction area:";
            this.toolTip.SetToolTip(this.label1, "Area for which to predict selected model\'s incidents");
            // 
            // predictionEndDate
            // 
            this.predictionEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.predictionEndDate.Location = new System.Drawing.Point(139, 102);
            this.predictionEndDate.Name = "predictionEndDate";
            this.predictionEndDate.Size = new System.Drawing.Size(107, 20);
            this.predictionEndDate.TabIndex = 9;
            this.predictionEndDate.Value = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.predictionEndDate.ValueChanged += new System.EventHandler(this.predictionEndDateTime_Changed);
            // 
            // predictionStartDate
            // 
            this.predictionStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.predictionStartDate.Location = new System.Drawing.Point(139, 76);
            this.predictionStartDate.Name = "predictionStartDate";
            this.predictionStartDate.Size = new System.Drawing.Size(107, 20);
            this.predictionStartDate.TabIndex = 7;
            this.predictionStartDate.Value = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.predictionStartDate.ValueChanged += new System.EventHandler(this.predictionStartDateTime_Changed);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(53, 80);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 98;
            this.label3.Text = "Prediction start:";
            this.toolTip.SetToolTip(this.label3, "Temporal start of prediction");
            // 
            // slideTrainingEnd
            // 
            this.slideTrainingEnd.AutoSize = true;
            this.slideTrainingEnd.Checked = true;
            this.slideTrainingEnd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.slideTrainingEnd.Enabled = false;
            this.slideTrainingEnd.Location = new System.Drawing.Point(232, 180);
            this.slideTrainingEnd.Name = "slideTrainingEnd";
            this.slideTrainingEnd.Size = new System.Drawing.Size(85, 17);
            this.slideTrainingEnd.TabIndex = 16;
            this.slideTrainingEnd.Text = "Training end";
            this.toolTip.SetToolTip(this.slideTrainingEnd, "Whether or not to slide the training window\'s end position");
            this.slideTrainingEnd.UseVisualStyleBackColor = true;
            this.slideTrainingEnd.CheckedChanged += new System.EventHandler(this.slideTrainingEnd_CheckedChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(100, 181);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(33, 13);
            this.label15.TabIndex = 129;
            this.label15.Text = "Slide:";
            this.toolTip.SetToolTip(this.label15, "For batched predictions, how to slide the training window");
            // 
            // predictions
            // 
            this.predictions.CheckBoxes = true;
            this.predictions.ContextMenuStrip = this.predictionsMenu;
            this.predictions.HideSelection = false;
            this.predictions.Location = new System.Drawing.Point(140, 205);
            this.predictions.Margin = new System.Windows.Forms.Padding(0);
            this.predictions.Name = "predictions";
            this.predictions.Size = new System.Drawing.Size(226, 216);
            this.predictions.TabIndex = 18;
            this.predictions.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.predictions_AfterCheck);
            // 
            // predictionsMenu
            // 
            this.predictionsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.displayPredictionToolStripMenuItem,
            this.groupByToolStripMenuItem,
            this.selectAllToolStripMenuItem,
            this.deselectAllToolStripMenuItem,
            this.toolStripSeparator2,
            this.setToolStripMenuItem,
            this.copyPredictionToolStripMenuItem,
            this.deletePredictionsToolStripMenuItem,
            this.toolStripSeparator3,
            this.smoothPredictionToolStripMenuItem,
            this.comparePredictionsToolStripMenuItem,
            this.aggregateAndEvaluateToolStripMenuItem,
            this.toolStripSeparator4,
            this.showModelDetailsToolStripMenuItem,
            this.openModelDirectoryToolStripMenuItem});
            this.predictionsMenu.Name = "predictionsMenu";
            this.predictionsMenu.Size = new System.Drawing.Size(200, 286);
            // 
            // displayPredictionToolStripMenuItem
            // 
            this.displayPredictionToolStripMenuItem.Name = "displayPredictionToolStripMenuItem";
            this.displayPredictionToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.displayPredictionToolStripMenuItem.Text = "Display";
            this.displayPredictionToolStripMenuItem.Click += new System.EventHandler(this.displayPredictionToolStripMenuItem_Click);
            // 
            // groupByToolStripMenuItem
            // 
            this.groupByToolStripMenuItem.DropDown = this.predictionGroups;
            this.groupByToolStripMenuItem.Name = "groupByToolStripMenuItem";
            this.groupByToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.groupByToolStripMenuItem.Text = "Group by...";
            // 
            // predictionGroups
            // 
            this.predictionGroups.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.groupByIncidentTypesToolStripMenuItem,
            this.groupByPredictionIntervalToolStripMenuItem,
            this.groupByRunToolStripMenuItem});
            this.predictionGroups.Name = "sorts";
            this.predictionGroups.Size = new System.Drawing.Size(198, 92);
            this.predictionGroups.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(this.submenu_Closing);
            // 
            // groupByIncidentTypesToolStripMenuItem
            // 
            this.groupByIncidentTypesToolStripMenuItem.CheckOnClick = true;
            this.groupByIncidentTypesToolStripMenuItem.Name = "groupByIncidentTypesToolStripMenuItem";
            this.groupByIncidentTypesToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.groupByIncidentTypesToolStripMenuItem.Text = "Incident types";
            this.groupByIncidentTypesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.groupByIncidentTypesToolStripMenuItem_CheckedChanged);
            // 
            // groupByPredictionIntervalToolStripMenuItem
            // 
            this.groupByPredictionIntervalToolStripMenuItem.CheckOnClick = true;
            this.groupByPredictionIntervalToolStripMenuItem.Name = "groupByPredictionIntervalToolStripMenuItem";
            this.groupByPredictionIntervalToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.groupByPredictionIntervalToolStripMenuItem.Text = "Prediction time interval";
            this.groupByPredictionIntervalToolStripMenuItem.CheckedChanged += new System.EventHandler(this.groupByPredictionIntervalToolStripMenuItem_CheckedChanged);
            // 
            // groupByRunToolStripMenuItem
            // 
            this.groupByRunToolStripMenuItem.CheckOnClick = true;
            this.groupByRunToolStripMenuItem.Name = "groupByRunToolStripMenuItem";
            this.groupByRunToolStripMenuItem.Size = new System.Drawing.Size(197, 22);
            this.groupByRunToolStripMenuItem.Text = "Run";
            this.groupByRunToolStripMenuItem.CheckedChanged += new System.EventHandler(this.groupByRunToolStripMenuItem_CheckedChanged);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.selectAllToolStripMenuItem.Text = "Select all";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // deselectAllToolStripMenuItem
            // 
            this.deselectAllToolStripMenuItem.Name = "deselectAllToolStripMenuItem";
            this.deselectAllToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.deselectAllToolStripMenuItem.Text = "Deselect all";
            this.deselectAllToolStripMenuItem.Click += new System.EventHandler(this.deselectAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(196, 6);
            // 
            // setToolStripMenuItem
            // 
            this.setToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editPredictionNameToolStripMenuItem,
            this.editPredictionRunToolStripMenuItem});
            this.setToolStripMenuItem.Name = "setToolStripMenuItem";
            this.setToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.setToolStripMenuItem.Text = "Edit";
            // 
            // editPredictionNameToolStripMenuItem
            // 
            this.editPredictionNameToolStripMenuItem.Name = "editPredictionNameToolStripMenuItem";
            this.editPredictionNameToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.editPredictionNameToolStripMenuItem.Text = "Name";
            this.editPredictionNameToolStripMenuItem.Click += new System.EventHandler(this.editPredictionNameToolStripMenuItem_Click);
            // 
            // editPredictionRunToolStripMenuItem
            // 
            this.editPredictionRunToolStripMenuItem.Name = "editPredictionRunToolStripMenuItem";
            this.editPredictionRunToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.editPredictionRunToolStripMenuItem.Text = "Run";
            this.editPredictionRunToolStripMenuItem.Click += new System.EventHandler(this.editPredictionRunToolStripMenuItem_Click);
            // 
            // copyPredictionToolStripMenuItem
            // 
            this.copyPredictionToolStripMenuItem.Name = "copyPredictionToolStripMenuItem";
            this.copyPredictionToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.copyPredictionToolStripMenuItem.Text = "Copy";
            this.copyPredictionToolStripMenuItem.Click += new System.EventHandler(this.copyPredictionToolStripMenuItem_Click);
            // 
            // deletePredictionsToolStripMenuItem
            // 
            this.deletePredictionsToolStripMenuItem.Name = "deletePredictionsToolStripMenuItem";
            this.deletePredictionsToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.deletePredictionsToolStripMenuItem.Text = "Delete";
            this.deletePredictionsToolStripMenuItem.Click += new System.EventHandler(this.deletePredictionsToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(196, 6);
            // 
            // smoothPredictionToolStripMenuItem
            // 
            this.smoothPredictionToolStripMenuItem.Name = "smoothPredictionToolStripMenuItem";
            this.smoothPredictionToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.smoothPredictionToolStripMenuItem.Text = "Smooth";
            this.smoothPredictionToolStripMenuItem.Click += new System.EventHandler(this.smoothPredictionToolStripMenuItem_Click);
            // 
            // comparePredictionsToolStripMenuItem
            // 
            this.comparePredictionsToolStripMenuItem.Name = "comparePredictionsToolStripMenuItem";
            this.comparePredictionsToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.comparePredictionsToolStripMenuItem.Text = "Compare";
            this.comparePredictionsToolStripMenuItem.Click += new System.EventHandler(this.comparePredictionsToolStripMenuItem_Click);
            // 
            // aggregateAndEvaluateToolStripMenuItem
            // 
            this.aggregateAndEvaluateToolStripMenuItem.Name = "aggregateAndEvaluateToolStripMenuItem";
            this.aggregateAndEvaluateToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.aggregateAndEvaluateToolStripMenuItem.Text = "Aggregate and evaluate";
            this.aggregateAndEvaluateToolStripMenuItem.Click += new System.EventHandler(this.aggregateAndEvaluateToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(196, 6);
            // 
            // showModelDetailsToolStripMenuItem
            // 
            this.showModelDetailsToolStripMenuItem.Name = "showModelDetailsToolStripMenuItem";
            this.showModelDetailsToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.showModelDetailsToolStripMenuItem.Text = "Show model details";
            this.showModelDetailsToolStripMenuItem.Click += new System.EventHandler(this.showModelDetailsToolStripMenuItem_Click);
            // 
            // openModelDirectoryToolStripMenuItem
            // 
            this.openModelDirectoryToolStripMenuItem.Name = "openModelDirectoryToolStripMenuItem";
            this.openModelDirectoryToolStripMenuItem.Size = new System.Drawing.Size(199, 22);
            this.openModelDirectoryToolStripMenuItem.Text = "Open model directory";
            this.openModelDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openModelDirectoryToolStripMenuItem_Click);
            // 
            // slideTrainingStart
            // 
            this.slideTrainingStart.AutoSize = true;
            this.slideTrainingStart.Checked = true;
            this.slideTrainingStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.slideTrainingStart.Enabled = false;
            this.slideTrainingStart.Location = new System.Drawing.Point(139, 180);
            this.slideTrainingStart.Name = "slideTrainingStart";
            this.slideTrainingStart.Size = new System.Drawing.Size(87, 17);
            this.slideTrainingStart.TabIndex = 15;
            this.slideTrainingStart.Text = "Training start";
            this.toolTip.SetToolTip(this.slideTrainingStart, "Whether or not to slide the training window\'s start position");
            this.slideTrainingStart.UseVisualStyleBackColor = true;
            this.slideTrainingStart.CheckedChanged += new System.EventHandler(this.slideTrainingStart_CheckedChanged);
            // 
            // startPrediction
            // 
            this.startPrediction.Enabled = false;
            this.startPrediction.Location = new System.Drawing.Point(139, 154);
            this.startPrediction.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.startPrediction.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.startPrediction.Name = "startPrediction";
            this.startPrediction.Size = new System.Drawing.Size(49, 20);
            this.startPrediction.TabIndex = 13;
            this.startPrediction.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(103, 132);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(30, 13);
            this.label14.TabIndex = 127;
            this.label14.Text = "Run:";
            this.toolTip.SetToolTip(this.label14, "Number of predictions to run in batch mode");
            // 
            // perIncident
            // 
            this.perIncident.AutoSize = true;
            this.perIncident.Enabled = false;
            this.perIncident.Location = new System.Drawing.Point(258, 155);
            this.perIncident.Name = "perIncident";
            this.perIncident.Size = new System.Drawing.Size(82, 17);
            this.perIncident.TabIndex = 14;
            this.perIncident.Text = "Per-incident";
            this.toolTip.SetToolTip(this.perIncident, "For models with multiple incident types, whether or not to break model into separ" +
        "ate per-incident models and run per-incident predictions.");
            this.perIncident.UseVisualStyleBackColor = true;
            this.perIncident.EnabledChanged += new System.EventHandler(this.perIncident_EnabledChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(55, 106);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(78, 13);
            this.label8.TabIndex = 126;
            this.label8.Text = "Prediction end:";
            this.toolTip.SetToolTip(this.label8, "Temporal end of prediction");
            // 
            // predictionEndTime
            // 
            this.predictionEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.predictionEndTime.Location = new System.Drawing.Point(258, 102);
            this.predictionEndTime.Name = "predictionEndTime";
            this.predictionEndTime.ShowUpDown = true;
            this.predictionEndTime.Size = new System.Drawing.Size(92, 20);
            this.predictionEndTime.TabIndex = 10;
            this.predictionEndTime.Value = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.predictionEndTime.CloseUp += new System.EventHandler(this.predictionEndDateTime_Changed);
            this.predictionEndTime.ValueChanged += new System.EventHandler(this.predictionEndDateTime_Changed);
            // 
            // predictionStartTime
            // 
            this.predictionStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.predictionStartTime.Location = new System.Drawing.Point(258, 76);
            this.predictionStartTime.Name = "predictionStartTime";
            this.predictionStartTime.ShowUpDown = true;
            this.predictionStartTime.Size = new System.Drawing.Size(92, 20);
            this.predictionStartTime.TabIndex = 8;
            this.predictionStartTime.Value = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.predictionStartTime.CloseUp += new System.EventHandler(this.predictionStartDateTime_Changed);
            this.predictionStartTime.ValueChanged += new System.EventHandler(this.predictionStartDateTime_Changed);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(188, 132);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(64, 13);
            this.label13.TabIndex = 122;
            this.label13.Text = "prediction(s)";
            // 
            // numPredictions
            // 
            this.numPredictions.Location = new System.Drawing.Point(139, 128);
            this.numPredictions.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.numPredictions.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPredictions.Name = "numPredictions";
            this.numPredictions.Size = new System.Drawing.Size(49, 20);
            this.numPredictions.TabIndex = 11;
            this.numPredictions.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPredictions.ValueChanged += new System.EventHandler(this.numPredictions_ValueChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(310, 132);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(57, 13);
            this.label12.TabIndex = 118;
            this.label12.Text = "hr(s). apart";
            this.toolTip.SetToolTip(this.label12, "Temporal spacing of batched predictions");
            // 
            // predictionSpacingHours
            // 
            this.predictionSpacingHours.Enabled = false;
            this.predictionSpacingHours.Location = new System.Drawing.Point(258, 128);
            this.predictionSpacingHours.Maximum = new decimal(new int[] {
            9999999,
            0,
            0,
            0});
            this.predictionSpacingHours.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.predictionSpacingHours.Name = "predictionSpacingHours";
            this.predictionSpacingHours.Size = new System.Drawing.Size(46, 20);
            this.predictionSpacingHours.TabIndex = 12;
            this.predictionSpacingHours.Value = new decimal(new int[] {
            24,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(71, 205);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 13);
            this.label6.TabIndex = 106;
            this.label6.Text = "Predictions:";
            this.toolTip.SetToolTip(this.label6, "Previous predictions");
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 156);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(117, 13);
            this.label10.TabIndex = 127;
            this.label10.Text = "Starting with prediction:";
            this.toolTip.SetToolTip(this.label10, "Position in batch sequence at which to start");
            // 
            // toolTip
            // 
            this.toolTip.AutomaticDelay = 30000;
            this.toolTip.AutoPopDelay = 30000;
            this.toolTip.InitialDelay = 2000;
            this.toolTip.ReshowDelay = 100;
            this.toolTip.ShowAlways = true;
            // 
            // verticalSplitContainer
            // 
            this.verticalSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.verticalSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.verticalSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.verticalSplitContainer.Name = "verticalSplitContainer";
            // 
            // verticalSplitContainer.Panel1
            // 
            this.verticalSplitContainer.Panel1.AutoScroll = true;
            this.verticalSplitContainer.Panel1.Controls.Add(this.run);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label12);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label9);
            this.verticalSplitContainer.Panel1.Controls.Add(this.slideTrainingEnd);
            this.verticalSplitContainer.Panel1.Controls.Add(this.models);
            this.verticalSplitContainer.Panel1.Controls.Add(this.predictions);
            this.verticalSplitContainer.Panel1.Controls.Add(this.perIncident);
            this.verticalSplitContainer.Panel1.Controls.Add(this.startPrediction);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label13);
            this.verticalSplitContainer.Panel1.Controls.Add(this.numPredictions);
            this.verticalSplitContainer.Panel1.Controls.Add(this.predictionEndTime);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label8);
            this.verticalSplitContainer.Panel1.Controls.Add(this.slideTrainingStart);
            this.verticalSplitContainer.Panel1.Controls.Add(this.predictionStartTime);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label6);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label14);
            this.verticalSplitContainer.Panel1.Controls.Add(this.predictionAreas);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label10);
            this.verticalSplitContainer.Panel1.Controls.Add(this.predictionStartDate);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label1);
            this.verticalSplitContainer.Panel1.Controls.Add(this.predictionSpacingHours);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label3);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label15);
            this.verticalSplitContainer.Panel1.Controls.Add(this.predictionEndDate);
            this.verticalSplitContainer.Panel1MinSize = 0;
            // 
            // verticalSplitContainer.Panel2
            // 
            this.verticalSplitContainer.Panel2.Controls.Add(this.threatSplitContainer);
            this.verticalSplitContainer.Panel2MinSize = 0;
            this.verticalSplitContainer.Size = new System.Drawing.Size(1126, 600);
            this.verticalSplitContainer.SplitterDistance = 400;
            this.verticalSplitContainer.TabIndex = 7;
            // 
            // threatSplitContainer
            // 
            this.threatSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.threatSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.threatSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.threatSplitContainer.Name = "threatSplitContainer";
            this.threatSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // threatSplitContainer.Panel1
            // 
            this.threatSplitContainer.Panel1.Controls.Add(this.threatMap);
            this.threatSplitContainer.Panel1MinSize = 0;
            // 
            // threatSplitContainer.Panel2
            // 
            this.threatSplitContainer.Panel2.Controls.Add(this.assessments);
            this.threatSplitContainer.Panel2MinSize = 0;
            this.threatSplitContainer.Size = new System.Drawing.Size(722, 600);
            this.threatSplitContainer.SplitterDistance = 378;
            this.threatSplitContainer.TabIndex = 0;
            // 
            // threatMap
            // 
            this.threatMap.BackColor = System.Drawing.Color.White;
            this.threatMap.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.threatMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.threatMap.Location = new System.Drawing.Point(0, 0);
            this.threatMap.Name = "threatMap";
            this.threatMap.Size = new System.Drawing.Size(718, 374);
            this.threatMap.TabIndex = 0;
            // 
            // assessments
            // 
            this.assessments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.assessments.Location = new System.Drawing.Point(0, 0);
            this.assessments.Name = "assessments";
            this.assessments.Size = new System.Drawing.Size(718, 214);
            this.assessments.TabIndex = 0;
            // 
            // horizontalSplitContainer
            // 
            this.horizontalSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.horizontalSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.horizontalSplitContainer.Location = new System.Drawing.Point(0, 24);
            this.horizontalSplitContainer.Name = "horizontalSplitContainer";
            this.horizontalSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // horizontalSplitContainer.Panel1
            // 
            this.horizontalSplitContainer.Panel1.Controls.Add(this.verticalSplitContainer);
            this.horizontalSplitContainer.Panel1MinSize = 0;
            // 
            // horizontalSplitContainer.Panel2
            // 
            this.horizontalSplitContainer.Panel2.Controls.Add(this.log);
            this.horizontalSplitContainer.Panel2MinSize = 0;
            this.horizontalSplitContainer.Size = new System.Drawing.Size(1126, 789);
            this.horizontalSplitContainer.SplitterDistance = 600;
            this.horizontalSplitContainer.TabIndex = 0;
            // 
            // log
            // 
            this.log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.log.Location = new System.Drawing.Point(0, 0);
            this.log.Name = "log";
            this.log.ReadOnly = true;
            this.log.Size = new System.Drawing.Size(1122, 181);
            this.log.TabIndex = 0;
            this.log.Text = "";
            // 
            // AttForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1126, 835);
            this.Controls.Add(this.horizontalSplitContainer);
            this.Controls.Add(this.status);
            this.Controls.Add(this.mainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenu;
            this.Name = "AttForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Asymmetric Threat Tracker";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AttForm_FormClosing);
            this.Load += new System.EventHandler(this.AttForm_Load);
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.modelMenu.ResumeLayout(false);
            this.predictionsMenu.ResumeLayout(false);
            this.predictionGroups.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.startPrediction)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPredictions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.predictionSpacingHours)).EndInit();
            this.verticalSplitContainer.Panel1.ResumeLayout(false);
            this.verticalSplitContainer.Panel1.PerformLayout();
            this.verticalSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.verticalSplitContainer)).EndInit();
            this.verticalSplitContainer.ResumeLayout(false);
            this.threatSplitContainer.Panel1.ResumeLayout(false);
            this.threatSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.threatSplitContainer)).EndInit();
            this.threatSplitContainer.ResumeLayout(false);
            this.horizontalSplitContainer.Panel1.ResumeLayout(false);
            this.horizontalSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.horizontalSplitContainer)).EndInit();
            this.horizontalSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.StatusStrip status;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ContextMenuStrip modelMenu;
        private System.Windows.Forms.ToolStripMenuItem addModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteModelToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip predictionsMenu;
        private System.Windows.Forms.ToolStripMenuItem displayPredictionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem comparePredictionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showModelDetailsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deletePredictionsToolStripMenuItem;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ToolStripMenuItem openModelDirectoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aggregateAndEvaluateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem groupByToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip predictionGroups;
        private System.Windows.Forms.ToolStripMenuItem deselectAllToolStripMenuItem;
        private System.Windows.Forms.SplitContainer verticalSplitContainer;
        private System.Windows.Forms.SplitContainer horizontalSplitContainer;
        private System.Windows.Forms.SplitContainer threatSplitContainer;
        private Visualization.Assessments assessments;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pluginsToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem groupByIncidentTypesToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem groupByRunToolStripMenuItem;
        public System.Windows.Forms.ComboBox models;
        public System.Windows.Forms.Button run;
        public System.Windows.Forms.ComboBox predictionAreas;
        public System.Windows.Forms.NumericUpDown numPredictions;
        public System.Windows.Forms.CheckBox slideTrainingEnd;
        public System.Windows.Forms.CheckBox slideTrainingStart;
        public System.Windows.Forms.NumericUpDown predictionSpacingHours;
        public System.Windows.Forms.CheckBox perIncident;
        public System.Windows.Forms.NumericUpDown startPrediction;
        public System.Windows.Forms.TreeView predictions;
        private System.Windows.Forms.DateTimePicker predictionEndDate;
        private System.Windows.Forms.DateTimePicker predictionStartDate;
        private System.Windows.Forms.DateTimePicker predictionStartTime;
        private System.Windows.Forms.DateTimePicker predictionEndTime;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem copyPredictionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem smoothPredictionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem groupByPredictionIntervalToolStripMenuItem;
        private System.Windows.Forms.RichTextBox log;
        private System.Windows.Forms.ToolStripMenuItem setToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editPredictionNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editPredictionRunToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteLogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem encryptTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetEncryptionKeyToolStripMenuItem;
        public Visualization.ThreatMap threatMap;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem incidentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearImportedIncidentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importIncidentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem geographicDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem notificationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendNotificationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendTestNotificationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem simulateIncidentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearSimulatedIncidentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importShapefilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteGeographicDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem importPointfilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem manageStoredImportersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseIncidentTypesToolStripMenuItem;
    }
}

