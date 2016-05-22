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

namespace PTL.ATT.GUI.Visualization
{
    partial class MultiDynamicThreatMap
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.threatMapMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.examinePredictionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.defineSpatialFeatureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.setBackgroundColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setPointDrawingDiameterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportThreatSurfaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.topPanel = new System.Windows.Forms.Panel();
            this.timeSlice = new System.Windows.Forms.TrackBar();
            this.threatResolution = new System.Windows.Forms.NumericUpDown();
            this.sliceTime = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.resetZoom = new System.Windows.Forms.Button();
            this.resetPan = new System.Windows.Forms.Button();
            this.panDownBtn = new System.Windows.Forms.Button();
            this.panLeftBtn = new System.Windows.Forms.Button();
            this.zoomOutBtn = new System.Windows.Forms.Button();
            this.zoomInBtn = new System.Windows.Forms.Button();
            this.panRightBtn = new System.Windows.Forms.Button();
            this.panUpBtn = new System.Windows.Forms.Button();
            this.overlayCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.incidentTypeCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.incidentGainCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.label7 = new System.Windows.Forms.Label();
            this.totalNumberOfIncidentsLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.totalNumberOfLocationsLabel = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.numberOfIncidentsCaptured = new System.Windows.Forms.NumericUpDown();
            this.areaPercentNum = new System.Windows.Forms.NumericUpDown();
            this.numberOfLocationsCovered = new System.Windows.Forms.NumericUpDown();
            this.incidentPercentNum = new System.Windows.Forms.NumericUpDown();
            this.panel1 = new System.Windows.Forms.Panel();
            this.areaPercent = new System.Windows.Forms.TrackBar();
            this.modelsCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.baselineModel = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.threatMapMenu.SuspendLayout();
            this.topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timeSlice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.threatResolution)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numberOfIncidentsCaptured)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.areaPercentNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numberOfLocationsCovered)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.incidentPercentNum)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.areaPercent)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // threatMapMenu
            // 
            this.threatMapMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.threatMapMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.examinePredictionsToolStripMenuItem,
            this.toolStripSeparator1,
            this.defineSpatialFeatureToolStripMenuItem,
            this.toolStripSeparator2,
            this.setBackgroundColorToolStripMenuItem,
            this.setPointDrawingDiameterToolStripMenuItem,
            this.exportThreatSurfaceToolStripMenuItem});
            this.threatMapMenu.Name = "threatSurfaceMenu";
            this.threatMapMenu.Size = new System.Drawing.Size(366, 136);
            // 
            // examinePredictionsToolStripMenuItem
            // 
            this.examinePredictionsToolStripMenuItem.Name = "examinePredictionsToolStripMenuItem";
            this.examinePredictionsToolStripMenuItem.Size = new System.Drawing.Size(365, 24);
            this.examinePredictionsToolStripMenuItem.Text = "Examine prediction(s) in highlighted region";
            this.examinePredictionsToolStripMenuItem.Click += new System.EventHandler(this.examinePredictionsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(362, 6);
            // 
            // defineSpatialFeatureToolStripMenuItem
            // 
            this.defineSpatialFeatureToolStripMenuItem.Name = "defineSpatialFeatureToolStripMenuItem";
            this.defineSpatialFeatureToolStripMenuItem.Size = new System.Drawing.Size(365, 24);
            this.defineSpatialFeatureToolStripMenuItem.Text = "Define spatial feature...";
            this.defineSpatialFeatureToolStripMenuItem.Click += new System.EventHandler(this.defineSpatialFeatureToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(362, 6);
            // 
            // setBackgroundColorToolStripMenuItem
            // 
            this.setBackgroundColorToolStripMenuItem.Name = "setBackgroundColorToolStripMenuItem";
            this.setBackgroundColorToolStripMenuItem.Size = new System.Drawing.Size(365, 24);
            this.setBackgroundColorToolStripMenuItem.Text = "Set background color...";
            this.setBackgroundColorToolStripMenuItem.Click += new System.EventHandler(this.setBackgroundColorToolStripMenuItem_Click);
            // 
            // setPointDrawingDiameterToolStripMenuItem
            // 
            this.setPointDrawingDiameterToolStripMenuItem.Name = "setPointDrawingDiameterToolStripMenuItem";
            this.setPointDrawingDiameterToolStripMenuItem.Size = new System.Drawing.Size(365, 24);
            this.setPointDrawingDiameterToolStripMenuItem.Text = "Set point drawing diameter...";
            this.setPointDrawingDiameterToolStripMenuItem.Click += new System.EventHandler(this.setPointDrawingDiameterToolStripMenuItem_Click);
            // 
            // exportThreatSurfaceToolStripMenuItem
            // 
            this.exportThreatSurfaceToolStripMenuItem.Name = "exportThreatSurfaceToolStripMenuItem";
            this.exportThreatSurfaceToolStripMenuItem.Size = new System.Drawing.Size(365, 24);
            this.exportThreatSurfaceToolStripMenuItem.Text = "Export threat surface";
            this.exportThreatSurfaceToolStripMenuItem.Click += new System.EventHandler(this.exportThreatSurfaceToolStripMenuItem_Click);
            // 
            // topPanel
            // 
            this.topPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.topPanel.BackColor = System.Drawing.SystemColors.Control;
            this.topPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.topPanel.Controls.Add(this.timeSlice);
            this.topPanel.Controls.Add(this.threatResolution);
            this.topPanel.Controls.Add(this.sliceTime);
            this.topPanel.Controls.Add(this.label1);
            this.topPanel.Controls.Add(this.label4);
            this.topPanel.Controls.Add(this.label2);
            this.topPanel.Location = new System.Drawing.Point(135, -1);
            this.topPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(1048, 64);
            this.topPanel.TabIndex = 18;
            this.topPanel.Visible = false;
            this.topPanel.MouseLeave += new System.EventHandler(this.topPanel_MouseLeave);
            // 
            // timeSlice
            // 
            this.timeSlice.Location = new System.Drawing.Point(320, 6);
            this.timeSlice.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.timeSlice.Maximum = 0;
            this.timeSlice.Name = "timeSlice";
            this.timeSlice.Size = new System.Drawing.Size(167, 56);
            this.timeSlice.TabIndex = 17;
            this.timeSlice.Scroll += new System.EventHandler(this.timeSlice_ValueChanged);
            // 
            // threatResolution
            // 
            this.threatResolution.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.threatResolution.Location = new System.Drawing.Point(111, 18);
            this.threatResolution.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.threatResolution.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.threatResolution.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.threatResolution.Name = "threatResolution";
            this.threatResolution.Size = new System.Drawing.Size(59, 24);
            this.threatResolution.TabIndex = 8;
            this.threatResolution.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.threatResolution.ValueChanged += new System.EventHandler(this.threatResolution_ValueChanged);
            // 
            // sliceTime
            // 
            this.sliceTime.AutoSize = true;
            this.sliceTime.BackColor = System.Drawing.Color.Transparent;
            this.sliceTime.Location = new System.Drawing.Point(493, 21);
            this.sliceTime.Name = "sliceTime";
            this.sliceTime.Size = new System.Drawing.Size(62, 17);
            this.sliceTime.TabIndex = 16;
            this.sliceTime.Text = "slice time";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(34, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 17);
            this.label1.TabIndex = 9;
            this.label1.Text = "Resolution:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Location = new System.Drawing.Point(246, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 17);
            this.label4.TabIndex = 15;
            this.label4.Text = "Time slice:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(177, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 17);
            this.label2.TabIndex = 10;
            this.label2.Text = "meters";
            // 
            // resetZoom
            // 
            this.resetZoom.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resetZoom.Image = global::PTL.ATT.GUI.Properties.Resources.resetZoom;
            this.resetZoom.Location = new System.Drawing.Point(47, 107);
            this.resetZoom.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.resetZoom.Name = "resetZoom";
            this.resetZoom.Size = new System.Drawing.Size(36, 36);
            this.resetZoom.TabIndex = 12;
            this.resetZoom.UseVisualStyleBackColor = true;
            this.resetZoom.Click += new System.EventHandler(this.resetZoom_Click);
            // 
            // resetPan
            // 
            this.resetPan.Image = global::PTL.ATT.GUI.Properties.Resources.resetPan;
            this.resetPan.Location = new System.Drawing.Point(54, 41);
            this.resetPan.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.resetPan.Name = "resetPan";
            this.resetPan.Size = new System.Drawing.Size(21, 22);
            this.resetPan.TabIndex = 11;
            this.resetPan.UseVisualStyleBackColor = true;
            this.resetPan.Click += new System.EventHandler(this.resetPan_Click);
            // 
            // panDownBtn
            // 
            this.panDownBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panDownBtn.Image = global::PTL.ATT.GUI.Properties.Resources.down;
            this.panDownBtn.Location = new System.Drawing.Point(54, 70);
            this.panDownBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panDownBtn.Name = "panDownBtn";
            this.panDownBtn.Size = new System.Drawing.Size(21, 30);
            this.panDownBtn.TabIndex = 3;
            this.panDownBtn.UseVisualStyleBackColor = true;
            this.panDownBtn.Click += new System.EventHandler(this.panDownBtn_Click);
            // 
            // panLeftBtn
            // 
            this.panLeftBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panLeftBtn.Image = global::PTL.ATT.GUI.Properties.Resources.left;
            this.panLeftBtn.Location = new System.Drawing.Point(19, 41);
            this.panLeftBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panLeftBtn.Name = "panLeftBtn";
            this.panLeftBtn.Size = new System.Drawing.Size(28, 22);
            this.panLeftBtn.TabIndex = 1;
            this.panLeftBtn.UseVisualStyleBackColor = true;
            this.panLeftBtn.Click += new System.EventHandler(this.panLeftBtn_Click);
            // 
            // zoomOutBtn
            // 
            this.zoomOutBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.zoomOutBtn.Image = global::PTL.ATT.GUI.Properties.Resources.zoom_out;
            this.zoomOutBtn.Location = new System.Drawing.Point(90, 107);
            this.zoomOutBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.zoomOutBtn.Name = "zoomOutBtn";
            this.zoomOutBtn.Size = new System.Drawing.Size(36, 36);
            this.zoomOutBtn.TabIndex = 5;
            this.zoomOutBtn.UseVisualStyleBackColor = true;
            this.zoomOutBtn.Click += new System.EventHandler(this.zoomOutBtn_Click);
            // 
            // zoomInBtn
            // 
            this.zoomInBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.zoomInBtn.Image = global::PTL.ATT.GUI.Properties.Resources.zoom_in;
            this.zoomInBtn.Location = new System.Drawing.Point(3, 107);
            this.zoomInBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.zoomInBtn.Name = "zoomInBtn";
            this.zoomInBtn.Size = new System.Drawing.Size(36, 36);
            this.zoomInBtn.TabIndex = 4;
            this.zoomInBtn.UseVisualStyleBackColor = true;
            this.zoomInBtn.Click += new System.EventHandler(this.zoomInBtn_Click);
            // 
            // panRightBtn
            // 
            this.panRightBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panRightBtn.Image = global::PTL.ATT.GUI.Properties.Resources.right;
            this.panRightBtn.Location = new System.Drawing.Point(82, 41);
            this.panRightBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panRightBtn.Name = "panRightBtn";
            this.panRightBtn.Size = new System.Drawing.Size(28, 22);
            this.panRightBtn.TabIndex = 2;
            this.panRightBtn.UseVisualStyleBackColor = true;
            this.panRightBtn.Click += new System.EventHandler(this.panRightBtn_Click);
            // 
            // panUpBtn
            // 
            this.panUpBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panUpBtn.Image = global::PTL.ATT.GUI.Properties.Resources.up;
            this.panUpBtn.Location = new System.Drawing.Point(54, 4);
            this.panUpBtn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panUpBtn.Name = "panUpBtn";
            this.panUpBtn.Size = new System.Drawing.Size(21, 30);
            this.panUpBtn.TabIndex = 0;
            this.panUpBtn.UseVisualStyleBackColor = true;
            this.panUpBtn.Click += new System.EventHandler(this.panUpBtn_Click);
            // 
            // overlayCheckBoxes
            // 
            this.overlayCheckBoxes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.overlayCheckBoxes.AutoScroll = true;
            this.overlayCheckBoxes.BackColor = System.Drawing.Color.Transparent;
            this.overlayCheckBoxes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.overlayCheckBoxes.Location = new System.Drawing.Point(3, 408);
            this.overlayCheckBoxes.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.overlayCheckBoxes.Name = "overlayCheckBoxes";
            this.overlayCheckBoxes.Size = new System.Drawing.Size(237, 188);
            this.overlayCheckBoxes.TabIndex = 7;
            this.overlayCheckBoxes.WrapContents = false;
            this.overlayCheckBoxes.Scroll += new System.Windows.Forms.ScrollEventHandler(this.checkBoxes_Scroll);
            // 
            // incidentTypeCheckBoxes
            // 
            this.incidentTypeCheckBoxes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.incidentTypeCheckBoxes.AutoScroll = true;
            this.incidentTypeCheckBoxes.BackColor = System.Drawing.Color.Transparent;
            this.incidentTypeCheckBoxes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.incidentTypeCheckBoxes.Location = new System.Drawing.Point(3, 212);
            this.incidentTypeCheckBoxes.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.incidentTypeCheckBoxes.Name = "incidentTypeCheckBoxes";
            this.incidentTypeCheckBoxes.Size = new System.Drawing.Size(237, 188);
            this.incidentTypeCheckBoxes.TabIndex = 6;
            this.incidentTypeCheckBoxes.WrapContents = false;
            this.incidentTypeCheckBoxes.Scroll += new System.Windows.Forms.ScrollEventHandler(this.checkBoxes_Scroll);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 182);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 21;
            this.button1.Text = "save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // incidentGainCheckBoxes
            // 
            this.incidentGainCheckBoxes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.incidentGainCheckBoxes.AutoScroll = true;
            this.incidentGainCheckBoxes.BackColor = System.Drawing.Color.Transparent;
            this.incidentGainCheckBoxes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.incidentGainCheckBoxes.Location = new System.Drawing.Point(794, 336);
            this.incidentGainCheckBoxes.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.incidentGainCheckBoxes.Name = "incidentGainCheckBoxes";
            this.incidentGainCheckBoxes.Size = new System.Drawing.Size(237, 188);
            this.incidentGainCheckBoxes.TabIndex = 6;
            this.incidentGainCheckBoxes.WrapContents = false;
            this.incidentGainCheckBoxes.Scroll += new System.Windows.Forms.ScrollEventHandler(this.checkBoxes_Scroll);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Location = new System.Drawing.Point(68, 35);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 17);
            this.label7.TabIndex = 9;
            this.label7.Text = "out of";
            // 
            // totalNumberOfIncidentsLabel
            // 
            this.totalNumberOfIncidentsLabel.AutoSize = true;
            this.totalNumberOfIncidentsLabel.BackColor = System.Drawing.Color.Transparent;
            this.totalNumberOfIncidentsLabel.Location = new System.Drawing.Point(108, 35);
            this.totalNumberOfIncidentsLabel.Name = "totalNumberOfIncidentsLabel";
            this.totalNumberOfIncidentsLabel.Size = new System.Drawing.Size(0, 17);
            this.totalNumberOfIncidentsLabel.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Location = new System.Drawing.Point(68, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 17);
            this.label5.TabIndex = 9;
            this.label5.Text = "out of";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(155, 35);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "incidents (";
            // 
            // totalNumberOfLocationsLabel
            // 
            this.totalNumberOfLocationsLabel.AutoSize = true;
            this.totalNumberOfLocationsLabel.BackColor = System.Drawing.Color.Transparent;
            this.totalNumberOfLocationsLabel.Location = new System.Drawing.Point(108, 5);
            this.totalNumberOfLocationsLabel.Name = "totalNumberOfLocationsLabel";
            this.totalNumberOfLocationsLabel.Size = new System.Drawing.Size(0, 17);
            this.totalNumberOfLocationsLabel.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Location = new System.Drawing.Point(265, 36);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(27, 17);
            this.label6.TabIndex = 9;
            this.label6.Text = "%)";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.Color.Transparent;
            this.label9.Location = new System.Drawing.Point(155, 5);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(70, 17);
            this.label9.TabIndex = 9;
            this.label9.Text = "locations (";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.Color.Transparent;
            this.label10.Location = new System.Drawing.Point(265, 6);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(27, 17);
            this.label10.TabIndex = 9;
            this.label10.Text = "%)";
            // 
            // numberOfIncidentsCaptured
            // 
            this.numberOfIncidentsCaptured.Location = new System.Drawing.Point(10, 33);
            this.numberOfIncidentsCaptured.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.numberOfIncidentsCaptured.Name = "numberOfIncidentsCaptured";
            this.numberOfIncidentsCaptured.Size = new System.Drawing.Size(59, 24);
            this.numberOfIncidentsCaptured.TabIndex = 8;
            this.numberOfIncidentsCaptured.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.numberOfIncidentsCaptured_KeyPress);
            // 
            // areaPercentNum
            // 
            this.areaPercentNum.Location = new System.Drawing.Point(222, 4);
            this.areaPercentNum.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.areaPercentNum.Name = "areaPercentNum";
            this.areaPercentNum.Size = new System.Drawing.Size(43, 24);
            this.areaPercentNum.TabIndex = 8;
            this.areaPercentNum.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.areaPercentNum_KeyPress);
            // 
            // numberOfLocationsCovered
            // 
            this.numberOfLocationsCovered.Location = new System.Drawing.Point(10, 4);
            this.numberOfLocationsCovered.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.numberOfLocationsCovered.Name = "numberOfLocationsCovered";
            this.numberOfLocationsCovered.Size = new System.Drawing.Size(59, 24);
            this.numberOfLocationsCovered.TabIndex = 8;
            this.numberOfLocationsCovered.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.numberOfLocationsCovered_KeyPress);
            // 
            // incidentPercentNum
            // 
            this.incidentPercentNum.Location = new System.Drawing.Point(222, 33);
            this.incidentPercentNum.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.incidentPercentNum.Name = "incidentPercentNum";
            this.incidentPercentNum.Size = new System.Drawing.Size(43, 24);
            this.incidentPercentNum.TabIndex = 8;
            this.incidentPercentNum.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.incidentPercentNum_KeyPress);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.incidentPercentNum);
            this.panel1.Controls.Add(this.numberOfLocationsCovered);
            this.panel1.Controls.Add(this.areaPercentNum);
            this.panel1.Controls.Add(this.numberOfIncidentsCaptured);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.totalNumberOfLocationsLabel);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.totalNumberOfIncidentsLabel);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.areaPercent);
            this.panel1.Location = new System.Drawing.Point(247, 532);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(784, 64);
            this.panel1.TabIndex = 19;
            // 
            // areaPercent
            // 
            this.areaPercent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.areaPercent.Location = new System.Drawing.Point(282, 4);
            this.areaPercent.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.areaPercent.Maximum = 100;
            this.areaPercent.Name = "areaPercent";
            this.areaPercent.Size = new System.Drawing.Size(497, 56);
            this.areaPercent.TabIndex = 17;
            this.areaPercent.Scroll += new System.EventHandler(this.areaPercent_Scroll);
            // 
            // modelsCheckBoxes
            // 
            this.modelsCheckBoxes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.modelsCheckBoxes.AutoScroll = true;
            this.modelsCheckBoxes.BackColor = System.Drawing.Color.Transparent;
            this.modelsCheckBoxes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.modelsCheckBoxes.Location = new System.Drawing.Point(794, 140);
            this.modelsCheckBoxes.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.modelsCheckBoxes.Name = "modelsCheckBoxes";
            this.modelsCheckBoxes.Size = new System.Drawing.Size(237, 188);
            this.modelsCheckBoxes.TabIndex = 6;
            this.modelsCheckBoxes.WrapContents = false;
            this.modelsCheckBoxes.Scroll += new System.Windows.Forms.ScrollEventHandler(this.checkBoxes_Scroll);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.baselineModel);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Location = new System.Drawing.Point(797, 107);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(230, 30);
            this.panel2.TabIndex = 20;
            // 
            // baselineModel
            // 
            this.baselineModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.baselineModel.FormattingEnabled = true;
            this.baselineModel.Location = new System.Drawing.Point(62, 2);
            this.baselineModel.Name = "baselineModel";
            this.baselineModel.Size = new System.Drawing.Size(165, 24);
            this.baselineModel.TabIndex = 10;
            this.baselineModel.SelectedIndexChanged += new System.EventHandler(this.baselineModel_SelectedIndexChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.Color.Transparent;
            this.label11.Location = new System.Drawing.Point(2, 6);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(60, 17);
            this.label11.TabIndex = 9;
            this.label11.Text = "Baseline:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(82, 182);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 24);
            this.textBox1.TabIndex = 24;
            this.textBox1.Visible = false;
            // 
            // MultiDynamicThreatMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ContextMenuStrip = this.threatMapMenu;
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.resetZoom);
            this.Controls.Add(this.resetPan);
            this.Controls.Add(this.panDownBtn);
            this.Controls.Add(this.panLeftBtn);
            this.Controls.Add(this.zoomOutBtn);
            this.Controls.Add(this.zoomInBtn);
            this.Controls.Add(this.panRightBtn);
            this.Controls.Add(this.panUpBtn);
            this.Controls.Add(this.overlayCheckBoxes);
            this.Controls.Add(this.modelsCheckBoxes);
            this.Controls.Add(this.incidentGainCheckBoxes);
            this.Controls.Add(this.incidentTypeCheckBoxes);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MultiDynamicThreatMap";
            this.Size = new System.Drawing.Size(1182, 702);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ThreatMap_MouseDown);
            this.MouseEnter += new System.EventHandler(this.ThreatMap_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.ThreatMap_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ThreatMap_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ThreatMap_MouseUp);
            this.Resize += new System.EventHandler(this.ThreatMap_Resize);
            this.threatMapMenu.ResumeLayout(false);
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timeSlice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.threatResolution)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numberOfIncidentsCaptured)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.areaPercentNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numberOfLocationsCovered)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.incidentPercentNum)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.areaPercent)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button panDownBtn;
        private System.Windows.Forms.Button panLeftBtn;
        private System.Windows.Forms.Button zoomOutBtn;
        private System.Windows.Forms.Button zoomInBtn;
        private System.Windows.Forms.Button panRightBtn;
        private System.Windows.Forms.Button panUpBtn;
        private System.Windows.Forms.FlowLayoutPanel incidentTypeCheckBoxes;
        private System.Windows.Forms.NumericUpDown threatResolution;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FlowLayoutPanel overlayCheckBoxes;
        private System.Windows.Forms.Button resetPan;
        private System.Windows.Forms.Button resetZoom;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label sliceTime;
        private System.Windows.Forms.TrackBar timeSlice;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.ContextMenuStrip threatMapMenu;
        private System.Windows.Forms.ToolStripMenuItem exportThreatSurfaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem examinePredictionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setBackgroundColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setPointDrawingDiameterToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem defineSpatialFeatureToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.FlowLayoutPanel incidentGainCheckBoxes;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label totalNumberOfIncidentsLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label totalNumberOfLocationsLabel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown numberOfIncidentsCaptured;
        private System.Windows.Forms.NumericUpDown areaPercentNum;
        private System.Windows.Forms.NumericUpDown numberOfLocationsCovered;
        private System.Windows.Forms.NumericUpDown incidentPercentNum;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TrackBar areaPercent;
        private System.Windows.Forms.FlowLayoutPanel modelsCheckBoxes;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ComboBox baselineModel;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
    }
}
