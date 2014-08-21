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
    partial class DiscreteChoiceModelOptions
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
            this.label13 = new System.Windows.Forms.Label();
            this.modelName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.trainingAreas = new System.Windows.Forms.ComboBox();
            this.trainingStart = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.trainingEnd = new System.Windows.Forms.DateTimePicker();
            this.incidentTypes = new System.Windows.Forms.ListBox();
            this.incidentTypesMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectAllIncidentTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label6 = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.smoothers = new PTL.ATT.GUI.SmootherList();
            this.incidentTypesMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(67, 237);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(60, 13);
            this.label13.TabIndex = 46;
            this.label13.Text = "Smoothers:";
            this.toolTip.SetToolTip(this.label13, "Smoothers to apply after predictions from this model are run.");
            // 
            // modelName
            // 
            this.modelName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modelName.Location = new System.Drawing.Point(133, 3);
            this.modelName.Name = "modelName";
            this.modelName.Size = new System.Drawing.Size(231, 20);
            this.modelName.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(89, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 39;
            this.label2.Text = "Name:";
            this.toolTip.SetToolTip(this.label2, "A descriptive name for this model.");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(55, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 119;
            this.label3.Text = "Training area:";
            this.toolTip.SetToolTip(this.label3, "Area upon which this model is trained.");
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(242, 60);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(13, 13);
            this.label7.TabIndex = 122;
            this.label7.Text = "--";
            // 
            // trainingAreas
            // 
            this.trainingAreas.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trainingAreas.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.trainingAreas.FormattingEnabled = true;
            this.trainingAreas.Location = new System.Drawing.Point(133, 29);
            this.trainingAreas.Name = "trainingAreas";
            this.trainingAreas.Size = new System.Drawing.Size(231, 21);
            this.trainingAreas.Sorted = true;
            this.trainingAreas.TabIndex = 1;
            this.trainingAreas.SelectedIndexChanged += new System.EventHandler(this.trainingAreas_SelectedIndexChanged);
            // 
            // trainingStart
            // 
            this.trainingStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.trainingStart.Location = new System.Drawing.Point(133, 56);
            this.trainingStart.Name = "trainingStart";
            this.trainingStart.Size = new System.Drawing.Size(107, 20);
            this.trainingStart.TabIndex = 3;
            this.trainingStart.Value = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.trainingStart.CloseUp += new System.EventHandler(this.trainingStart_CloseUp);
            this.trainingStart.ValueChanged += new System.EventHandler(this.trainingStart_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(51, 84);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 121;
            this.label4.Text = "Incident types:";
            this.toolTip.SetToolTip(this.label4, "Incident types to model (right-click for options).");
            // 
            // trainingEnd
            // 
            this.trainingEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.trainingEnd.Location = new System.Drawing.Point(257, 56);
            this.trainingEnd.Name = "trainingEnd";
            this.trainingEnd.Size = new System.Drawing.Size(107, 20);
            this.trainingEnd.TabIndex = 4;
            this.trainingEnd.Value = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.trainingEnd.CloseUp += new System.EventHandler(this.trainingEnd_CloseUp);
            this.trainingEnd.ValueChanged += new System.EventHandler(this.trainingEnd_ValueChanged);
            // 
            // incidentTypes
            // 
            this.incidentTypes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.incidentTypes.ContextMenuStrip = this.incidentTypesMenu;
            this.incidentTypes.FormattingEnabled = true;
            this.incidentTypes.HorizontalScrollbar = true;
            this.incidentTypes.Location = new System.Drawing.Point(133, 84);
            this.incidentTypes.Name = "incidentTypes";
            this.incidentTypes.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.incidentTypes.Size = new System.Drawing.Size(231, 147);
            this.incidentTypes.Sorted = true;
            this.incidentTypes.TabIndex = 5;
            this.incidentTypes.SelectedIndexChanged += new System.EventHandler(this.incidentTypes_SelectedIndexChanged);
            // 
            // incidentTypesMenu
            // 
            this.incidentTypesMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllIncidentTypesToolStripMenuItem});
            this.incidentTypesMenu.Name = "incidentTypesMenu";
            this.incidentTypesMenu.Size = new System.Drawing.Size(121, 26);
            // 
            // selectAllIncidentTypesToolStripMenuItem
            // 
            this.selectAllIncidentTypesToolStripMenuItem.Name = "selectAllIncidentTypesToolStripMenuItem";
            this.selectAllIncidentTypesToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.selectAllIncidentTypesToolStripMenuItem.Text = "Select all";
            this.selectAllIncidentTypesToolStripMenuItem.Click += new System.EventHandler(this.selectAllIncidentTypesToolStripMenuItem_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 60);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(118, 13);
            this.label6.TabIndex = 120;
            this.label6.Text = "Training start/end date:";
            this.toolTip.SetToolTip(this.label6, "Date range upon which this model should be trained.");
            // 
            // toolTip
            // 
            this.toolTip.AutomaticDelay = 30000;
            this.toolTip.AutoPopDelay = 30000;
            this.toolTip.InitialDelay = 2000;
            this.toolTip.ReshowDelay = 100;
            this.toolTip.ShowAlways = true;
            // 
            // smoothers
            // 
            this.smoothers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.smoothers.FormattingEnabled = true;
            this.smoothers.Location = new System.Drawing.Point(133, 237);
            this.smoothers.Name = "smoothers";
            this.smoothers.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.smoothers.Size = new System.Drawing.Size(231, 147);
            this.smoothers.Sorted = true;
            this.smoothers.TabIndex = 8;
            // 
            // DiscreteChoiceModelOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.trainingAreas);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.trainingStart);
            this.Controls.Add(this.modelName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.trainingEnd);
            this.Controls.Add(this.incidentTypes);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.smoothers);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label13);
            this.MinimumSize = new System.Drawing.Size(367, 388);
            this.Name = "DiscreteChoiceModelOptions";
            this.Size = new System.Drawing.Size(367, 388);
            this.incidentTypesMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.TextBox modelName;
        public System.Windows.Forms.DateTimePicker trainingStart;
        public System.Windows.Forms.DateTimePicker trainingEnd;
        private System.Windows.Forms.ListBox incidentTypes;
        private SmootherList smoothers;
        private System.Windows.Forms.ContextMenuStrip incidentTypesMenu;
        private System.Windows.Forms.ToolStripMenuItem selectAllIncidentTypesToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip;
        public System.Windows.Forms.ComboBox trainingAreas;
    }
}
