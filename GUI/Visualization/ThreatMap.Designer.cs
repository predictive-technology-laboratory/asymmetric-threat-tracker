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
 
namespace PTL.ATT.GUI.Visualization
{
    partial class ThreatMap
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
            this.incidentTypeCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.threatResolution = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.overlayCheckBoxes = new System.Windows.Forms.FlowLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.sliceTime = new System.Windows.Forms.Label();
            this.timeSlice = new System.Windows.Forms.TrackBar();
            this.topPanel = new System.Windows.Forms.Panel();
            this.threatMapMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.examinePredictionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportThreatSurfaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setBackgroundColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panDownBtn = new System.Windows.Forms.Button();
            this.resetZoom = new System.Windows.Forms.Button();
            this.resetPan = new System.Windows.Forms.Button();
            this.panLeftBtn = new System.Windows.Forms.Button();
            this.zoomOutBtn = new System.Windows.Forms.Button();
            this.zoomInBtn = new System.Windows.Forms.Button();
            this.panRightBtn = new System.Windows.Forms.Button();
            this.panUpBtn = new System.Windows.Forms.Button();
            this.setPointDrawingDiameterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.threatResolution)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeSlice)).BeginInit();
            this.topPanel.SuspendLayout();
            this.threatMapMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // incidentTypeCheckBoxes
            // 
            this.incidentTypeCheckBoxes.AutoScroll = true;
            this.incidentTypeCheckBoxes.BackColor = System.Drawing.Color.Transparent;
            this.incidentTypeCheckBoxes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.incidentTypeCheckBoxes.Location = new System.Drawing.Point(3, 122);
            this.incidentTypeCheckBoxes.Name = "incidentTypeCheckBoxes";
            this.incidentTypeCheckBoxes.Size = new System.Drawing.Size(203, 153);
            this.incidentTypeCheckBoxes.TabIndex = 6;
            this.incidentTypeCheckBoxes.WrapContents = false;
            this.incidentTypeCheckBoxes.Scroll += new System.Windows.Forms.ScrollEventHandler(this.checkBoxes_Scroll);
            // 
            // threatResolution
            // 
            this.threatResolution.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.threatResolution.Location = new System.Drawing.Point(95, 15);
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
            this.threatResolution.Size = new System.Drawing.Size(51, 20);
            this.threatResolution.TabIndex = 8;
            this.threatResolution.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.threatResolution.ValueChanged += new System.EventHandler(this.threatResolution_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(29, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Resolution:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(152, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "meters";
            // 
            // overlayCheckBoxes
            // 
            this.overlayCheckBoxes.BackColor = System.Drawing.Color.Transparent;
            this.overlayCheckBoxes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.overlayCheckBoxes.Location = new System.Drawing.Point(3, 281);
            this.overlayCheckBoxes.Name = "overlayCheckBoxes";
            this.overlayCheckBoxes.Size = new System.Drawing.Size(203, 153);
            this.overlayCheckBoxes.TabIndex = 7;
            this.overlayCheckBoxes.WrapContents = false;
            this.overlayCheckBoxes.Scroll += new System.Windows.Forms.ScrollEventHandler(this.checkBoxes_Scroll);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Location = new System.Drawing.Point(211, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Time slice:";
            // 
            // sliceTime
            // 
            this.sliceTime.AutoSize = true;
            this.sliceTime.BackColor = System.Drawing.Color.Transparent;
            this.sliceTime.Location = new System.Drawing.Point(423, 17);
            this.sliceTime.Name = "sliceTime";
            this.sliceTime.Size = new System.Drawing.Size(50, 13);
            this.sliceTime.TabIndex = 16;
            this.sliceTime.Text = "slice time";
            // 
            // timeSlice
            // 
            this.timeSlice.Location = new System.Drawing.Point(274, 5);
            this.timeSlice.Maximum = 0;
            this.timeSlice.Name = "timeSlice";
            this.timeSlice.Size = new System.Drawing.Size(143, 45);
            this.timeSlice.TabIndex = 17;
            this.timeSlice.Scroll += new System.EventHandler(this.timeSlice_ValueChanged);
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
            this.topPanel.Location = new System.Drawing.Point(116, -1);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(772, 52);
            this.topPanel.TabIndex = 18;
            this.topPanel.Visible = false;
            this.topPanel.MouseLeave += new System.EventHandler(this.topPanel_MouseLeave);
            // 
            // threatMapMenu
            // 
            this.threatMapMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.examinePredictionsToolStripMenuItem,
            this.exportThreatSurfaceToolStripMenuItem,
            this.setBackgroundColorToolStripMenuItem,
            this.setPointDrawingDiameterToolStripMenuItem});
            this.threatMapMenu.Name = "threatSurfaceMenu";
            this.threatMapMenu.Size = new System.Drawing.Size(303, 114);
            // 
            // examinePredictionsToolStripMenuItem
            // 
            this.examinePredictionsToolStripMenuItem.Name = "examinePredictionsToolStripMenuItem";
            this.examinePredictionsToolStripMenuItem.Size = new System.Drawing.Size(302, 22);
            this.examinePredictionsToolStripMenuItem.Text = "Examine prediction(s) in highlighted region";
            this.examinePredictionsToolStripMenuItem.Click += new System.EventHandler(this.examinePredictionsToolStripMenuItem_Click);
            // 
            // exportThreatSurfaceToolStripMenuItem
            // 
            this.exportThreatSurfaceToolStripMenuItem.Name = "exportThreatSurfaceToolStripMenuItem";
            this.exportThreatSurfaceToolStripMenuItem.Size = new System.Drawing.Size(302, 22);
            this.exportThreatSurfaceToolStripMenuItem.Text = "Export threat surface";
            this.exportThreatSurfaceToolStripMenuItem.Click += new System.EventHandler(this.exportThreatSurfaceToolStripMenuItem_Click);
            // 
            // setBackgroundColorToolStripMenuItem
            // 
            this.setBackgroundColorToolStripMenuItem.Name = "setBackgroundColorToolStripMenuItem";
            this.setBackgroundColorToolStripMenuItem.Size = new System.Drawing.Size(302, 22);
            this.setBackgroundColorToolStripMenuItem.Text = "Set background color...";
            this.setBackgroundColorToolStripMenuItem.Click += new System.EventHandler(this.setBackgroundColorToolStripMenuItem_Click);
            // 
            // panDownBtn
            // 
            this.panDownBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panDownBtn.Image = global::PTL.ATT.GUI.Properties.Resources.down;
            this.panDownBtn.Location = new System.Drawing.Point(46, 57);
            this.panDownBtn.Name = "panDownBtn";
            this.panDownBtn.Size = new System.Drawing.Size(18, 24);
            this.panDownBtn.TabIndex = 3;
            this.panDownBtn.UseVisualStyleBackColor = true;
            this.panDownBtn.Click += new System.EventHandler(this.panDownBtn_Click);
            // 
            // resetZoom
            // 
            this.resetZoom.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resetZoom.Image = global::PTL.ATT.GUI.Properties.Resources.resetZoom;
            this.resetZoom.Location = new System.Drawing.Point(40, 87);
            this.resetZoom.Name = "resetZoom";
            this.resetZoom.Size = new System.Drawing.Size(31, 29);
            this.resetZoom.TabIndex = 12;
            this.resetZoom.UseVisualStyleBackColor = true;
            this.resetZoom.Click += new System.EventHandler(this.resetZoom_Click);
            // 
            // resetPan
            // 
            this.resetPan.Image = global::PTL.ATT.GUI.Properties.Resources.resetPan;
            this.resetPan.Location = new System.Drawing.Point(46, 33);
            this.resetPan.Name = "resetPan";
            this.resetPan.Size = new System.Drawing.Size(18, 18);
            this.resetPan.TabIndex = 11;
            this.resetPan.UseVisualStyleBackColor = true;
            this.resetPan.Click += new System.EventHandler(this.resetPan_Click);
            // 
            // panLeftBtn
            // 
            this.panLeftBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panLeftBtn.Image = global::PTL.ATT.GUI.Properties.Resources.left;
            this.panLeftBtn.Location = new System.Drawing.Point(16, 33);
            this.panLeftBtn.Name = "panLeftBtn";
            this.panLeftBtn.Size = new System.Drawing.Size(24, 18);
            this.panLeftBtn.TabIndex = 1;
            this.panLeftBtn.UseVisualStyleBackColor = true;
            this.panLeftBtn.Click += new System.EventHandler(this.panLeftBtn_Click);
            // 
            // zoomOutBtn
            // 
            this.zoomOutBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.zoomOutBtn.Image = global::PTL.ATT.GUI.Properties.Resources.zoom_out;
            this.zoomOutBtn.Location = new System.Drawing.Point(77, 87);
            this.zoomOutBtn.Name = "zoomOutBtn";
            this.zoomOutBtn.Size = new System.Drawing.Size(31, 29);
            this.zoomOutBtn.TabIndex = 5;
            this.zoomOutBtn.UseVisualStyleBackColor = true;
            this.zoomOutBtn.Click += new System.EventHandler(this.zoomOutBtn_Click);
            // 
            // zoomInBtn
            // 
            this.zoomInBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.zoomInBtn.Image = global::PTL.ATT.GUI.Properties.Resources.zoom_in;
            this.zoomInBtn.Location = new System.Drawing.Point(3, 87);
            this.zoomInBtn.Name = "zoomInBtn";
            this.zoomInBtn.Size = new System.Drawing.Size(31, 29);
            this.zoomInBtn.TabIndex = 4;
            this.zoomInBtn.UseVisualStyleBackColor = true;
            this.zoomInBtn.Click += new System.EventHandler(this.zoomInBtn_Click);
            // 
            // panRightBtn
            // 
            this.panRightBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panRightBtn.Image = global::PTL.ATT.GUI.Properties.Resources.right;
            this.panRightBtn.Location = new System.Drawing.Point(70, 33);
            this.panRightBtn.Name = "panRightBtn";
            this.panRightBtn.Size = new System.Drawing.Size(24, 18);
            this.panRightBtn.TabIndex = 2;
            this.panRightBtn.UseVisualStyleBackColor = true;
            this.panRightBtn.Click += new System.EventHandler(this.panRightBtn_Click);
            // 
            // panUpBtn
            // 
            this.panUpBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panUpBtn.Image = global::PTL.ATT.GUI.Properties.Resources.up;
            this.panUpBtn.Location = new System.Drawing.Point(46, 3);
            this.panUpBtn.Name = "panUpBtn";
            this.panUpBtn.Size = new System.Drawing.Size(18, 24);
            this.panUpBtn.TabIndex = 0;
            this.panUpBtn.UseVisualStyleBackColor = true;
            this.panUpBtn.Click += new System.EventHandler(this.panUpBtn_Click);
            // 
            // setPointDrawingDiameterToolStripMenuItem
            // 
            this.setPointDrawingDiameterToolStripMenuItem.Name = "setPointDrawingDiameterToolStripMenuItem";
            this.setPointDrawingDiameterToolStripMenuItem.Size = new System.Drawing.Size(302, 22);
            this.setPointDrawingDiameterToolStripMenuItem.Text = "Set point drawing diameter...";
            this.setPointDrawingDiameterToolStripMenuItem.Click += new System.EventHandler(this.setPointDrawingDiameterToolStripMenuItem_Click);
            // 
            // ThreatMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ContextMenuStrip = this.threatMapMenu;
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.resetZoom);
            this.Controls.Add(this.resetPan);
            this.Controls.Add(this.overlayCheckBoxes);
            this.Controls.Add(this.incidentTypeCheckBoxes);
            this.Controls.Add(this.panDownBtn);
            this.Controls.Add(this.panLeftBtn);
            this.Controls.Add(this.zoomOutBtn);
            this.Controls.Add(this.zoomInBtn);
            this.Controls.Add(this.panRightBtn);
            this.Controls.Add(this.panUpBtn);
            this.DoubleBuffered = true;
            this.Name = "ThreatMap";
            this.Size = new System.Drawing.Size(886, 494);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ThreatMap_MouseDown);
            this.MouseLeave += new System.EventHandler(this.ThreatMap_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ThreatMap_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ThreatMap_MouseUp);
            this.Resize += new System.EventHandler(this.ThreatMap_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.threatResolution)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeSlice)).EndInit();
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.threatMapMenu.ResumeLayout(false);
            this.ResumeLayout(false);

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
    }
}
