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
            this.label5 = new System.Windows.Forms.Label();
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateEncryptedPasswordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetEncryptionKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendNotificationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendTestNotificationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.incidentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importIncidentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.simulateIncidentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearSimulatedIncidentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shapefilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.status = new System.Windows.Forms.StatusStrip();
            this.trainingAreas = new System.Windows.Forms.ComboBox();
            this.trainingAreaMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addTrainingAreaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteTrainingAreaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.features = new System.Windows.Forms.ListBox();
            this.featuresMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectAllFeaturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearFeatureRemappingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trainingEnd = new System.Windows.Forms.DateTimePicker();
            this.trainingStart = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.models = new System.Windows.Forms.ComboBox();
            this.modelMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label9 = new System.Windows.Forms.Label();
            this.run = new System.Windows.Forms.Button();
            this.incidentTypes = new System.Windows.Forms.ListBox();
            this.incidentTypesMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectAllIncidentTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label11 = new System.Windows.Forms.Label();
            this.predictionAreas = new System.Windows.Forms.ComboBox();
            this.predictionAreaMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addPredictionAreaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deletePredictionAreaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.groupByFeaturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupByIncidentTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupByModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.selectFeaturesForPredictionAndRerunToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.label7 = new System.Windows.Forms.Label();
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
            this.trainingAreaMenu.SuspendLayout();
            this.featuresMenu.SuspendLayout();
            this.modelMenu.SuspendLayout();
            this.incidentTypesMenu.SuspendLayout();
            this.predictionAreaMenu.SuspendLayout();
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
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(54, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Training area:";
            // 
            // mainMenu
            // 
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.incidentsToolStripMenuItem,
            this.shapefilesToolStripMenuItem,
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
            this.editConfigurationToolStripMenuItem,
            this.generateEncryptedPasswordToolStripMenuItem,
            this.resetEncryptionKeyToolStripMenuItem,
            this.sendNotificationsToolStripMenuItem,
            this.sendTestNotificationToolStripMenuItem,
            this.viewLogToolStripMenuItem,
            this.deleteLogToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // editConfigurationToolStripMenuItem
            // 
            this.editConfigurationToolStripMenuItem.Name = "editConfigurationToolStripMenuItem";
            this.editConfigurationToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.editConfigurationToolStripMenuItem.Text = "Edit configuration...";
            this.editConfigurationToolStripMenuItem.Click += new System.EventHandler(this.editConfigurationToolStripMenuItem_Click);
            // 
            // generateEncryptedPasswordToolStripMenuItem
            // 
            this.generateEncryptedPasswordToolStripMenuItem.Name = "generateEncryptedPasswordToolStripMenuItem";
            this.generateEncryptedPasswordToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.generateEncryptedPasswordToolStripMenuItem.Text = "Generate encrypted password...";
            this.generateEncryptedPasswordToolStripMenuItem.Click += new System.EventHandler(this.generateEncryptedPasswordToolStripMenuItem_Click);
            // 
            // resetEncryptionKeyToolStripMenuItem
            // 
            this.resetEncryptionKeyToolStripMenuItem.Name = "resetEncryptionKeyToolStripMenuItem";
            this.resetEncryptionKeyToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.resetEncryptionKeyToolStripMenuItem.Text = "Reset encryption key";
            this.resetEncryptionKeyToolStripMenuItem.Click += new System.EventHandler(this.resetEncryptionKeyToolStripMenuItem_Click);
            // 
            // sendNotificationsToolStripMenuItem
            // 
            this.sendNotificationsToolStripMenuItem.CheckOnClick = true;
            this.sendNotificationsToolStripMenuItem.Name = "sendNotificationsToolStripMenuItem";
            this.sendNotificationsToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.sendNotificationsToolStripMenuItem.Text = "Send notifications";
            this.sendNotificationsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.sendNotificationsToolStripMenuItem_CheckedChanged);
            // 
            // sendTestNotificationToolStripMenuItem
            // 
            this.sendTestNotificationToolStripMenuItem.Name = "sendTestNotificationToolStripMenuItem";
            this.sendTestNotificationToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.sendTestNotificationToolStripMenuItem.Text = "Send test notification";
            this.sendTestNotificationToolStripMenuItem.Click += new System.EventHandler(this.sendTestNotificationToolStripMenuItem_Click);
            // 
            // viewLogToolStripMenuItem
            // 
            this.viewLogToolStripMenuItem.Name = "viewLogToolStripMenuItem";
            this.viewLogToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.viewLogToolStripMenuItem.Text = "View log";
            this.viewLogToolStripMenuItem.Click += new System.EventHandler(this.viewLogToolStripMenuItem_Click);
            // 
            // deleteLogToolStripMenuItem
            // 
            this.deleteLogToolStripMenuItem.Name = "deleteLogToolStripMenuItem";
            this.deleteLogToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.deleteLogToolStripMenuItem.Text = "Delete log";
            this.deleteLogToolStripMenuItem.Click += new System.EventHandler(this.deleteLogToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // incidentsToolStripMenuItem
            // 
            this.incidentsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importIncidentsToolStripMenuItem,
            this.clearToolStripMenuItem,
            this.toolStripSeparator1,
            this.simulateIncidentsToolStripMenuItem,
            this.clearSimulatedIncidentsToolStripMenuItem});
            this.incidentsToolStripMenuItem.Name = "incidentsToolStripMenuItem";
            this.incidentsToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.incidentsToolStripMenuItem.Text = "Incidents";
            // 
            // importIncidentsToolStripMenuItem
            // 
            this.importIncidentsToolStripMenuItem.Name = "importIncidentsToolStripMenuItem";
            this.importIncidentsToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.importIncidentsToolStripMenuItem.Text = "Import...";
            this.importIncidentsToolStripMenuItem.Click += new System.EventHandler(this.importIncidentsToolStripMenuItem_Click);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.clearToolStripMenuItem.Text = "Clear incidents";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(200, 6);
            // 
            // simulateIncidentsToolStripMenuItem
            // 
            this.simulateIncidentsToolStripMenuItem.Name = "simulateIncidentsToolStripMenuItem";
            this.simulateIncidentsToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.simulateIncidentsToolStripMenuItem.Text = "Simulate...";
            this.simulateIncidentsToolStripMenuItem.Click += new System.EventHandler(this.simulateIncidentsToolStripMenuItem_Click);
            // 
            // clearSimulatedIncidentsToolStripMenuItem
            // 
            this.clearSimulatedIncidentsToolStripMenuItem.Name = "clearSimulatedIncidentsToolStripMenuItem";
            this.clearSimulatedIncidentsToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.clearSimulatedIncidentsToolStripMenuItem.Text = "Clear simulated incidents";
            this.clearSimulatedIncidentsToolStripMenuItem.Click += new System.EventHandler(this.clearSimulatedIncidentsToolStripMenuItem_Click);
            // 
            // shapefilesToolStripMenuItem
            // 
            this.shapefilesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem});
            this.shapefilesToolStripMenuItem.Name = "shapefilesToolStripMenuItem";
            this.shapefilesToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.shapefilesToolStripMenuItem.Text = "Shapefiles";
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.importToolStripMenuItem.Text = "Import...";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importShapeFilesToolStripMenuItem_Click);
            // 
            // pluginsToolStripMenuItem
            // 
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.pluginsToolStripMenuItem.Text = "Plugins";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
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
            // trainingAreas
            // 
            this.trainingAreas.ContextMenuStrip = this.trainingAreaMenu;
            this.trainingAreas.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.trainingAreas.FormattingEnabled = true;
            this.trainingAreas.Location = new System.Drawing.Point(132, 14);
            this.trainingAreas.Name = "trainingAreas";
            this.trainingAreas.Size = new System.Drawing.Size(231, 21);
            this.trainingAreas.Sorted = true;
            this.trainingAreas.TabIndex = 0;
            this.trainingAreas.SelectedIndexChanged += new System.EventHandler(this.trainingAreas_SelectedIndexChanged);
            // 
            // trainingAreaMenu
            // 
            this.trainingAreaMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTrainingAreaToolStripMenuItem,
            this.deleteTrainingAreaToolStripMenuItem});
            this.trainingAreaMenu.Name = "trainingAreaMenu";
            this.trainingAreaMenu.Size = new System.Drawing.Size(117, 48);
            // 
            // addTrainingAreaToolStripMenuItem
            // 
            this.addTrainingAreaToolStripMenuItem.Name = "addTrainingAreaToolStripMenuItem";
            this.addTrainingAreaToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.addTrainingAreaToolStripMenuItem.Text = "Add";
            this.addTrainingAreaToolStripMenuItem.Click += new System.EventHandler(this.addTrainingAreaToolStripMenuItem_Click);
            // 
            // deleteTrainingAreaToolStripMenuItem
            // 
            this.deleteTrainingAreaToolStripMenuItem.Name = "deleteTrainingAreaToolStripMenuItem";
            this.deleteTrainingAreaToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.deleteTrainingAreaToolStripMenuItem.Text = "Delete";
            this.deleteTrainingAreaToolStripMenuItem.Click += new System.EventHandler(this.deleteTrainingAreaToolStripMenuItem_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(75, 250);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 40;
            this.label2.Text = "Features:";
            // 
            // features
            // 
            this.features.ContextMenuStrip = this.featuresMenu;
            this.features.FormattingEnabled = true;
            this.features.HorizontalScrollbar = true;
            this.features.Location = new System.Drawing.Point(132, 250);
            this.features.Name = "features";
            this.features.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.features.Size = new System.Drawing.Size(231, 147);
            this.features.TabIndex = 5;
            // 
            // featuresMenu
            // 
            this.featuresMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllFeaturesToolStripMenuItem,
            this.toolStripSeparator5,
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem,
            this.clearFeatureRemappingToolStripMenuItem});
            this.featuresMenu.Name = "featuresMenu";
            this.featuresMenu.Size = new System.Drawing.Size(301, 98);
            // 
            // selectAllFeaturesToolStripMenuItem
            // 
            this.selectAllFeaturesToolStripMenuItem.Name = "selectAllFeaturesToolStripMenuItem";
            this.selectAllFeaturesToolStripMenuItem.Size = new System.Drawing.Size(300, 22);
            this.selectAllFeaturesToolStripMenuItem.Text = "Select all";
            this.selectAllFeaturesToolStripMenuItem.Click += new System.EventHandler(this.selectAllFeaturesToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(297, 6);
            // 
            // remapSelectedFeaturesDuringPredictionToolStripMenuItem
            // 
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem.Name = "remapSelectedFeaturesDuringPredictionToolStripMenuItem";
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem.Size = new System.Drawing.Size(300, 22);
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem.Text = "Remap selected features during prediction...";
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem.Click += new System.EventHandler(this.remapSelectedFeaturesDuringPredictionToolStripMenuItem_Click);
            // 
            // clearFeatureRemappingToolStripMenuItem
            // 
            this.clearFeatureRemappingToolStripMenuItem.Name = "clearFeatureRemappingToolStripMenuItem";
            this.clearFeatureRemappingToolStripMenuItem.Size = new System.Drawing.Size(300, 22);
            this.clearFeatureRemappingToolStripMenuItem.Text = "Clear feature remapping";
            this.clearFeatureRemappingToolStripMenuItem.Click += new System.EventHandler(this.clearFeatureRemappingToolStripMenuItem_Click);
            // 
            // trainingEnd
            // 
            this.trainingEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.trainingEnd.Location = new System.Drawing.Point(256, 42);
            this.trainingEnd.Name = "trainingEnd";
            this.trainingEnd.Size = new System.Drawing.Size(107, 20);
            this.trainingEnd.TabIndex = 2;
            this.trainingEnd.Value = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.trainingEnd.CloseUp += new System.EventHandler(this.trainingEnd_CloseUp);
            this.trainingEnd.ValueChanged += new System.EventHandler(this.trainingEnd_ValueChanged);
            // 
            // trainingStart
            // 
            this.trainingStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.trainingStart.Location = new System.Drawing.Point(132, 42);
            this.trainingStart.Name = "trainingStart";
            this.trainingStart.Size = new System.Drawing.Size(107, 20);
            this.trainingStart.TabIndex = 1;
            this.trainingStart.Value = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.trainingStart.CloseUp += new System.EventHandler(this.trainingStart_CloseUp);
            this.trainingStart.ValueChanged += new System.EventHandler(this.trainingStart_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(118, 13);
            this.label4.TabIndex = 62;
            this.label4.Text = "Training start/end date:";
            // 
            // models
            // 
            this.models.ContextMenuStrip = this.modelMenu;
            this.models.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.models.FormattingEnabled = true;
            this.models.Location = new System.Drawing.Point(132, 223);
            this.models.Name = "models";
            this.models.Size = new System.Drawing.Size(231, 21);
            this.models.Sorted = true;
            this.models.TabIndex = 4;
            this.models.SelectedIndexChanged += new System.EventHandler(this.models_SelectedIndexChanged);
            // 
            // modelMenu
            // 
            this.modelMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addModelToolStripMenuItem,
            this.updateModelToolStripMenuItem,
            this.deleteModelToolStripMenuItem});
            this.modelMenu.Name = "modelMenu";
            this.modelMenu.Size = new System.Drawing.Size(121, 70);
            // 
            // addModelToolStripMenuItem
            // 
            this.addModelToolStripMenuItem.Name = "addModelToolStripMenuItem";
            this.addModelToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.addModelToolStripMenuItem.Text = "Add";
            this.addModelToolStripMenuItem.Click += new System.EventHandler(this.addModelToolStripMenuItem_Click);
            // 
            // updateModelToolStripMenuItem
            // 
            this.updateModelToolStripMenuItem.Name = "updateModelToolStripMenuItem";
            this.updateModelToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.updateModelToolStripMenuItem.Text = "Update";
            this.updateModelToolStripMenuItem.Click += new System.EventHandler(this.updateModelToolStripMenuItem_Click);
            // 
            // deleteModelToolStripMenuItem
            // 
            this.deleteModelToolStripMenuItem.Name = "deleteModelToolStripMenuItem";
            this.deleteModelToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.deleteModelToolStripMenuItem.Text = "Delete";
            this.deleteModelToolStripMenuItem.Click += new System.EventHandler(this.deleteModelToolStripMenuItem_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(87, 226);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(39, 13);
            this.label9.TabIndex = 72;
            this.label9.Text = "Model:";
            // 
            // run
            // 
            this.run.Location = new System.Drawing.Point(312, 530);
            this.run.Name = "run";
            this.run.Size = new System.Drawing.Size(36, 23);
            this.run.TabIndex = 17;
            this.run.Text = "Run";
            this.run.UseVisualStyleBackColor = true;
            this.run.Click += new System.EventHandler(this.run_Click);
            // 
            // incidentTypes
            // 
            this.incidentTypes.ContextMenuStrip = this.incidentTypesMenu;
            this.incidentTypes.FormattingEnabled = true;
            this.incidentTypes.HorizontalScrollbar = true;
            this.incidentTypes.Location = new System.Drawing.Point(132, 70);
            this.incidentTypes.Name = "incidentTypes";
            this.incidentTypes.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.incidentTypes.Size = new System.Drawing.Size(231, 147);
            this.incidentTypes.Sorted = true;
            this.incidentTypes.TabIndex = 3;
            this.incidentTypes.SelectedIndexChanged += new System.EventHandler(this.incidentTypes_SelectedIndexChanged);
            // 
            // incidentTypesMenu
            // 
            this.incidentTypesMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllIncidentTypesToolStripMenuItem});
            this.incidentTypesMenu.Name = "incidentTypesMenu";
            this.incidentTypesMenu.Size = new System.Drawing.Size(128, 26);
            // 
            // selectAllIncidentTypesToolStripMenuItem
            // 
            this.selectAllIncidentTypesToolStripMenuItem.Name = "selectAllIncidentTypesToolStripMenuItem";
            this.selectAllIncidentTypesToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.selectAllIncidentTypesToolStripMenuItem.Text = "Select all";
            this.selectAllIncidentTypesToolStripMenuItem.Click += new System.EventHandler(this.selectAllIncidentTypesToolStripMenuItem_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(50, 70);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(76, 13);
            this.label11.TabIndex = 88;
            this.label11.Text = "Incident types:";
            // 
            // predictionAreas
            // 
            this.predictionAreas.ContextMenuStrip = this.predictionAreaMenu;
            this.predictionAreas.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.predictionAreas.FormattingEnabled = true;
            this.predictionAreas.Location = new System.Drawing.Point(132, 403);
            this.predictionAreas.Name = "predictionAreas";
            this.predictionAreas.Size = new System.Drawing.Size(231, 21);
            this.predictionAreas.Sorted = true;
            this.predictionAreas.TabIndex = 6;
            this.predictionAreas.SelectedIndexChanged += new System.EventHandler(this.predictionAreas_SelectedIndexChanged);
            // 
            // predictionAreaMenu
            // 
            this.predictionAreaMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addPredictionAreaToolStripMenuItem,
            this.deletePredictionAreaToolStripMenuItem});
            this.predictionAreaMenu.Name = "predictionAreaMenu";
            this.predictionAreaMenu.Size = new System.Drawing.Size(117, 48);
            // 
            // addPredictionAreaToolStripMenuItem
            // 
            this.addPredictionAreaToolStripMenuItem.Name = "addPredictionAreaToolStripMenuItem";
            this.addPredictionAreaToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.addPredictionAreaToolStripMenuItem.Text = "Add";
            this.addPredictionAreaToolStripMenuItem.Click += new System.EventHandler(this.addTrainingAreaToolStripMenuItem_Click);
            // 
            // deletePredictionAreaToolStripMenuItem
            // 
            this.deletePredictionAreaToolStripMenuItem.Name = "deletePredictionAreaToolStripMenuItem";
            this.deletePredictionAreaToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.deletePredictionAreaToolStripMenuItem.Text = "Delete";
            this.deletePredictionAreaToolStripMenuItem.Click += new System.EventHandler(this.deletePredictionAreaToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(45, 406);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 95;
            this.label1.Text = "Prediction area:";
            // 
            // predictionEndDate
            // 
            this.predictionEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.predictionEndDate.Location = new System.Drawing.Point(132, 456);
            this.predictionEndDate.Name = "predictionEndDate";
            this.predictionEndDate.Size = new System.Drawing.Size(107, 20);
            this.predictionEndDate.TabIndex = 9;
            this.predictionEndDate.Value = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.predictionEndDate.ValueChanged += new System.EventHandler(this.predictionEndDateTime_Changed);
            // 
            // predictionStartDate
            // 
            this.predictionStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.predictionStartDate.Location = new System.Drawing.Point(132, 430);
            this.predictionStartDate.Name = "predictionStartDate";
            this.predictionStartDate.Size = new System.Drawing.Size(107, 20);
            this.predictionStartDate.TabIndex = 7;
            this.predictionStartDate.Value = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.predictionStartDate.ValueChanged += new System.EventHandler(this.predictionStartDateTime_Changed);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(46, 434);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 98;
            this.label3.Text = "Prediction start:";
            // 
            // slideTrainingEnd
            // 
            this.slideTrainingEnd.AutoSize = true;
            this.slideTrainingEnd.Checked = true;
            this.slideTrainingEnd.CheckState = System.Windows.Forms.CheckState.Checked;
            this.slideTrainingEnd.Enabled = false;
            this.slideTrainingEnd.Location = new System.Drawing.Point(225, 534);
            this.slideTrainingEnd.Name = "slideTrainingEnd";
            this.slideTrainingEnd.Size = new System.Drawing.Size(85, 17);
            this.slideTrainingEnd.TabIndex = 16;
            this.slideTrainingEnd.Text = "Training end";
            this.slideTrainingEnd.UseVisualStyleBackColor = true;
            this.slideTrainingEnd.CheckedChanged += new System.EventHandler(this.slideTrainingEnd_CheckedChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(93, 535);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(33, 13);
            this.label15.TabIndex = 129;
            this.label15.Text = "Slide:";
            // 
            // predictions
            // 
            this.predictions.CheckBoxes = true;
            this.predictions.ContextMenuStrip = this.predictionsMenu;
            this.predictions.HideSelection = false;
            this.predictions.Location = new System.Drawing.Point(133, 559);
            this.predictions.Margin = new System.Windows.Forms.Padding(0);
            this.predictions.Name = "predictions";
            this.predictions.Size = new System.Drawing.Size(230, 216);
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
            this.selectFeaturesForPredictionAndRerunToolStripMenuItem,
            this.smoothPredictionToolStripMenuItem,
            this.comparePredictionsToolStripMenuItem,
            this.aggregateAndEvaluateToolStripMenuItem,
            this.toolStripSeparator4,
            this.showModelDetailsToolStripMenuItem,
            this.openModelDirectoryToolStripMenuItem});
            this.predictionsMenu.Name = "predictionsMenu";
            this.predictionsMenu.Size = new System.Drawing.Size(209, 308);
            // 
            // displayPredictionToolStripMenuItem
            // 
            this.displayPredictionToolStripMenuItem.Name = "displayPredictionToolStripMenuItem";
            this.displayPredictionToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.displayPredictionToolStripMenuItem.Text = "Display";
            this.displayPredictionToolStripMenuItem.Click += new System.EventHandler(this.displayPredictionToolStripMenuItem_Click);
            // 
            // groupByToolStripMenuItem
            // 
            this.groupByToolStripMenuItem.DropDown = this.predictionGroups;
            this.groupByToolStripMenuItem.Name = "groupByToolStripMenuItem";
            this.groupByToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.groupByToolStripMenuItem.Text = "Group by...";
            // 
            // predictionGroups
            // 
            this.predictionGroups.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.groupByFeaturesToolStripMenuItem,
            this.groupByIncidentTypesToolStripMenuItem,
            this.groupByModelToolStripMenuItem,
            this.groupByPredictionIntervalToolStripMenuItem,
            this.groupByRunToolStripMenuItem});
            this.predictionGroups.Name = "sorts";
            this.predictionGroups.OwnerItem = this.groupByToolStripMenuItem;
            this.predictionGroups.Size = new System.Drawing.Size(195, 114);
            this.predictionGroups.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(this.submenu_Closing);
            // 
            // groupByFeaturesToolStripMenuItem
            // 
            this.groupByFeaturesToolStripMenuItem.CheckOnClick = true;
            this.groupByFeaturesToolStripMenuItem.Name = "groupByFeaturesToolStripMenuItem";
            this.groupByFeaturesToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.groupByFeaturesToolStripMenuItem.Text = "Features";
            this.groupByFeaturesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.groupByFeaturesToolStripMenuItem_CheckedChanged);
            // 
            // groupByIncidentTypesToolStripMenuItem
            // 
            this.groupByIncidentTypesToolStripMenuItem.CheckOnClick = true;
            this.groupByIncidentTypesToolStripMenuItem.Name = "groupByIncidentTypesToolStripMenuItem";
            this.groupByIncidentTypesToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.groupByIncidentTypesToolStripMenuItem.Text = "Incident types";
            this.groupByIncidentTypesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.groupByIncidentTypesToolStripMenuItem_CheckedChanged);
            // 
            // groupByModelToolStripMenuItem
            // 
            this.groupByModelToolStripMenuItem.CheckOnClick = true;
            this.groupByModelToolStripMenuItem.Name = "groupByModelToolStripMenuItem";
            this.groupByModelToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.groupByModelToolStripMenuItem.Text = "Model";
            this.groupByModelToolStripMenuItem.CheckedChanged += new System.EventHandler(this.groupByModelToolStripMenuItem_CheckedChanged);
            // 
            // groupByPredictionIntervalToolStripMenuItem
            // 
            this.groupByPredictionIntervalToolStripMenuItem.CheckOnClick = true;
            this.groupByPredictionIntervalToolStripMenuItem.Name = "groupByPredictionIntervalToolStripMenuItem";
            this.groupByPredictionIntervalToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.groupByPredictionIntervalToolStripMenuItem.Text = "Prediction time interval";
            this.groupByPredictionIntervalToolStripMenuItem.CheckedChanged += new System.EventHandler(this.groupByPredictionIntervalToolStripMenuItem_CheckedChanged);
            // 
            // groupByRunToolStripMenuItem
            // 
            this.groupByRunToolStripMenuItem.CheckOnClick = true;
            this.groupByRunToolStripMenuItem.Name = "groupByRunToolStripMenuItem";
            this.groupByRunToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.groupByRunToolStripMenuItem.Text = "Run";
            this.groupByRunToolStripMenuItem.CheckedChanged += new System.EventHandler(this.groupByRunToolStripMenuItem_CheckedChanged);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.selectAllToolStripMenuItem.Text = "Select all";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // deselectAllToolStripMenuItem
            // 
            this.deselectAllToolStripMenuItem.Name = "deselectAllToolStripMenuItem";
            this.deselectAllToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.deselectAllToolStripMenuItem.Text = "Deselect all";
            this.deselectAllToolStripMenuItem.Click += new System.EventHandler(this.deselectAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(205, 6);
            // 
            // setToolStripMenuItem
            // 
            this.setToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editPredictionNameToolStripMenuItem,
            this.editPredictionRunToolStripMenuItem});
            this.setToolStripMenuItem.Name = "setToolStripMenuItem";
            this.setToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.setToolStripMenuItem.Text = "Edit";
            // 
            // editPredictionNameToolStripMenuItem
            // 
            this.editPredictionNameToolStripMenuItem.Name = "editPredictionNameToolStripMenuItem";
            this.editPredictionNameToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.editPredictionNameToolStripMenuItem.Text = "Name";
            this.editPredictionNameToolStripMenuItem.Click += new System.EventHandler(this.editPredictionNameToolStripMenuItem_Click);
            // 
            // editPredictionRunToolStripMenuItem
            // 
            this.editPredictionRunToolStripMenuItem.Name = "editPredictionRunToolStripMenuItem";
            this.editPredictionRunToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.editPredictionRunToolStripMenuItem.Text = "Run";
            this.editPredictionRunToolStripMenuItem.Click += new System.EventHandler(this.editPredictionRunToolStripMenuItem_Click);
            // 
            // copyPredictionToolStripMenuItem
            // 
            this.copyPredictionToolStripMenuItem.Name = "copyPredictionToolStripMenuItem";
            this.copyPredictionToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.copyPredictionToolStripMenuItem.Text = "Copy";
            this.copyPredictionToolStripMenuItem.Click += new System.EventHandler(this.copyPredictionToolStripMenuItem_Click);
            // 
            // deletePredictionsToolStripMenuItem
            // 
            this.deletePredictionsToolStripMenuItem.Name = "deletePredictionsToolStripMenuItem";
            this.deletePredictionsToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.deletePredictionsToolStripMenuItem.Text = "Delete";
            this.deletePredictionsToolStripMenuItem.Click += new System.EventHandler(this.deletePredictionsToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(205, 6);
            // 
            // selectFeaturesForPredictionAndRerunToolStripMenuItem
            // 
            this.selectFeaturesForPredictionAndRerunToolStripMenuItem.Name = "selectFeaturesForPredictionAndRerunToolStripMenuItem";
            this.selectFeaturesForPredictionAndRerunToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.selectFeaturesForPredictionAndRerunToolStripMenuItem.Text = "Select features and rerun";
            this.selectFeaturesForPredictionAndRerunToolStripMenuItem.Click += new System.EventHandler(this.selectFeaturesForPredictionAndRerunToolStripMenuItem_Click);
            // 
            // smoothPredictionToolStripMenuItem
            // 
            this.smoothPredictionToolStripMenuItem.Name = "smoothPredictionToolStripMenuItem";
            this.smoothPredictionToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.smoothPredictionToolStripMenuItem.Text = "Smooth";
            this.smoothPredictionToolStripMenuItem.Click += new System.EventHandler(this.smoothPredictionToolStripMenuItem_Click);
            // 
            // comparePredictionsToolStripMenuItem
            // 
            this.comparePredictionsToolStripMenuItem.Name = "comparePredictionsToolStripMenuItem";
            this.comparePredictionsToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.comparePredictionsToolStripMenuItem.Text = "Compare";
            this.comparePredictionsToolStripMenuItem.Click += new System.EventHandler(this.comparePredictionsToolStripMenuItem_Click);
            // 
            // aggregateAndEvaluateToolStripMenuItem
            // 
            this.aggregateAndEvaluateToolStripMenuItem.Name = "aggregateAndEvaluateToolStripMenuItem";
            this.aggregateAndEvaluateToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.aggregateAndEvaluateToolStripMenuItem.Text = "Aggregate and evaluate";
            this.aggregateAndEvaluateToolStripMenuItem.Click += new System.EventHandler(this.aggregateAndEvaluateToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(205, 6);
            // 
            // showModelDetailsToolStripMenuItem
            // 
            this.showModelDetailsToolStripMenuItem.Name = "showModelDetailsToolStripMenuItem";
            this.showModelDetailsToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.showModelDetailsToolStripMenuItem.Text = "Show model details";
            this.showModelDetailsToolStripMenuItem.Click += new System.EventHandler(this.showModelDetailsToolStripMenuItem_Click);
            // 
            // openModelDirectoryToolStripMenuItem
            // 
            this.openModelDirectoryToolStripMenuItem.Name = "openModelDirectoryToolStripMenuItem";
            this.openModelDirectoryToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.openModelDirectoryToolStripMenuItem.Text = "Open model directory";
            this.openModelDirectoryToolStripMenuItem.Click += new System.EventHandler(this.openModelDirectoryToolStripMenuItem_Click);
            // 
            // slideTrainingStart
            // 
            this.slideTrainingStart.AutoSize = true;
            this.slideTrainingStart.Checked = true;
            this.slideTrainingStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.slideTrainingStart.Enabled = false;
            this.slideTrainingStart.Location = new System.Drawing.Point(132, 534);
            this.slideTrainingStart.Name = "slideTrainingStart";
            this.slideTrainingStart.Size = new System.Drawing.Size(87, 17);
            this.slideTrainingStart.TabIndex = 15;
            this.slideTrainingStart.Text = "Training start";
            this.slideTrainingStart.UseVisualStyleBackColor = true;
            this.slideTrainingStart.CheckedChanged += new System.EventHandler(this.slideTrainingStart_CheckedChanged);
            // 
            // startPrediction
            // 
            this.startPrediction.Enabled = false;
            this.startPrediction.Location = new System.Drawing.Point(132, 508);
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
            this.label14.Location = new System.Drawing.Point(96, 486);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(30, 13);
            this.label14.TabIndex = 127;
            this.label14.Text = "Run:";
            // 
            // perIncident
            // 
            this.perIncident.AutoSize = true;
            this.perIncident.Enabled = false;
            this.perIncident.Location = new System.Drawing.Point(197, 509);
            this.perIncident.Name = "perIncident";
            this.perIncident.Size = new System.Drawing.Size(151, 17);
            this.perIncident.TabIndex = 14;
            this.perIncident.Text = "Create per-incident models";
            this.perIncident.UseVisualStyleBackColor = true;
            this.perIncident.EnabledChanged += new System.EventHandler(this.perIncident_EnabledChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(48, 460);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(78, 13);
            this.label8.TabIndex = 126;
            this.label8.Text = "Prediction end:";
            // 
            // predictionEndTime
            // 
            this.predictionEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.predictionEndTime.Location = new System.Drawing.Point(256, 456);
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
            this.predictionStartTime.Location = new System.Drawing.Point(256, 430);
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
            this.label13.Location = new System.Drawing.Point(181, 486);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(64, 13);
            this.label13.TabIndex = 122;
            this.label13.Text = "prediction(s)";
            // 
            // numPredictions
            // 
            this.numPredictions.Location = new System.Drawing.Point(132, 482);
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
            this.label12.Location = new System.Drawing.Point(302, 486);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(57, 13);
            this.label12.TabIndex = 118;
            this.label12.Text = "hr(s). apart";
            // 
            // predictionSpacingHours
            // 
            this.predictionSpacingHours.Enabled = false;
            this.predictionSpacingHours.Location = new System.Drawing.Point(256, 482);
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
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(241, 46);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(13, 13);
            this.label7.TabIndex = 114;
            this.label7.Text = "--";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(64, 559);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 13);
            this.label6.TabIndex = 106;
            this.label6.Text = "Predictions:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(9, 510);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(117, 13);
            this.label10.TabIndex = 127;
            this.label10.Text = "Starting with prediction:";
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
            this.verticalSplitContainer.Panel1.Controls.Add(this.label5);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label7);
            this.verticalSplitContainer.Panel1.Controls.Add(this.trainingAreas);
            this.verticalSplitContainer.Panel1.Controls.Add(this.trainingStart);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label12);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label9);
            this.verticalSplitContainer.Panel1.Controls.Add(this.slideTrainingEnd);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label11);
            this.verticalSplitContainer.Panel1.Controls.Add(this.models);
            this.verticalSplitContainer.Panel1.Controls.Add(this.predictions);
            this.verticalSplitContainer.Panel1.Controls.Add(this.trainingEnd);
            this.verticalSplitContainer.Panel1.Controls.Add(this.perIncident);
            this.verticalSplitContainer.Panel1.Controls.Add(this.startPrediction);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label13);
            this.verticalSplitContainer.Panel1.Controls.Add(this.incidentTypes);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label2);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label4);
            this.verticalSplitContainer.Panel1.Controls.Add(this.numPredictions);
            this.verticalSplitContainer.Panel1.Controls.Add(this.predictionEndTime);
            this.verticalSplitContainer.Panel1.Controls.Add(this.label8);
            this.verticalSplitContainer.Panel1.Controls.Add(this.features);
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
            this.verticalSplitContainer.Size = new System.Drawing.Size(1126, 690);
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
            this.threatSplitContainer.Size = new System.Drawing.Size(722, 690);
            this.threatSplitContainer.SplitterDistance = 435;
            this.threatSplitContainer.TabIndex = 0;
            // 
            // threatMap
            // 
            this.threatMap.BackColor = System.Drawing.Color.White;
            this.threatMap.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.threatMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.threatMap.Location = new System.Drawing.Point(0, 0);
            this.threatMap.Name = "threatMap";
            this.threatMap.Size = new System.Drawing.Size(718, 431);
            this.threatMap.TabIndex = 0;
            // 
            // assessments
            // 
            this.assessments.Dock = System.Windows.Forms.DockStyle.Fill;
            this.assessments.Location = new System.Drawing.Point(0, 0);
            this.assessments.Name = "assessments";
            this.assessments.Size = new System.Drawing.Size(718, 247);
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
            this.horizontalSplitContainer.SplitterDistance = 690;
            this.horizontalSplitContainer.TabIndex = 0;
            // 
            // log
            // 
            this.log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.log.Location = new System.Drawing.Point(0, 0);
            this.log.Name = "log";
            this.log.ReadOnly = true;
            this.log.Size = new System.Drawing.Size(1122, 91);
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
            this.trainingAreaMenu.ResumeLayout(false);
            this.featuresMenu.ResumeLayout(false);
            this.modelMenu.ResumeLayout(false);
            this.incidentTypesMenu.ResumeLayout(false);
            this.predictionAreaMenu.ResumeLayout(false);
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

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.StatusStrip status;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolStripMenuItem incidentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importIncidentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem simulateIncidentsToolStripMenuItem;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ToolStripMenuItem clearSimulatedIncidentsToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shapefilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ContextMenuStrip trainingAreaMenu;
        private System.Windows.Forms.ContextMenuStrip incidentTypesMenu;
        private System.Windows.Forms.ToolStripMenuItem addTrainingAreaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteTrainingAreaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAllIncidentTypesToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip modelMenu;
        private System.Windows.Forms.ToolStripMenuItem addModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteModelToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip featuresMenu;
        private System.Windows.Forms.ToolStripMenuItem selectAllFeaturesToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip predictionAreaMenu;
        private System.Windows.Forms.ToolStripMenuItem addPredictionAreaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deletePredictionAreaToolStripMenuItem;
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
        private System.Windows.Forms.ToolStripMenuItem editConfigurationToolStripMenuItem;
        private System.Windows.Forms.SplitContainer verticalSplitContainer;
        private System.Windows.Forms.SplitContainer horizontalSplitContainer;
        private System.Windows.Forms.SplitContainer threatSplitContainer;
        private Visualization.Assessments assessments;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pluginsToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem groupByFeaturesToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem groupByModelToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem groupByIncidentTypesToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem groupByRunToolStripMenuItem;
        public System.Windows.Forms.ComboBox trainingAreas;
        public System.Windows.Forms.ListBox features;
        public System.Windows.Forms.ComboBox models;
        public System.Windows.Forms.Button run;
        public System.Windows.Forms.ListBox incidentTypes;
        public System.Windows.Forms.ComboBox predictionAreas;
        public System.Windows.Forms.NumericUpDown numPredictions;
        public System.Windows.Forms.CheckBox slideTrainingEnd;
        public System.Windows.Forms.CheckBox slideTrainingStart;
        public System.Windows.Forms.NumericUpDown predictionSpacingHours;
        public System.Windows.Forms.CheckBox perIncident;
        public System.Windows.Forms.NumericUpDown startPrediction;
        public System.Windows.Forms.TreeView predictions;
        private System.Windows.Forms.DateTimePicker trainingEnd;
        private System.Windows.Forms.DateTimePicker trainingStart;
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
        private System.Windows.Forms.ToolStripMenuItem selectFeaturesForPredictionAndRerunToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateEncryptedPasswordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendNotificationsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetEncryptionKeyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendTestNotificationToolStripMenuItem;
        public Visualization.ThreatMap threatMap;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem remapSelectedFeaturesDuringPredictionToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem clearFeatureRemappingToolStripMenuItem;
    }
}

