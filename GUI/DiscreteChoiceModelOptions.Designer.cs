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
            this.label8 = new System.Windows.Forms.Label();
            this.predictionSampleSize = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.trainingSampleSize = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.pointSpacing = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
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
            ((System.ComponentModel.ISupportInitialize)(this.predictionSampleSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingSampleSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pointSpacing)).BeginInit();
            this.incidentTypesMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(61, 315);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(60, 13);
            this.label13.TabIndex = 46;
            this.label13.Text = "Smoothers:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(210, 291);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(35, 13);
            this.label8.TabIndex = 45;
            this.label8.Text = "points";
            // 
            // predictionSampleSize
            // 
            this.predictionSampleSize.Location = new System.Drawing.Point(127, 289);
            this.predictionSampleSize.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.predictionSampleSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.predictionSampleSize.Name = "predictionSampleSize";
            this.predictionSampleSize.Size = new System.Drawing.Size(77, 20);
            this.predictionSampleSize.TabIndex = 7;
            this.predictionSampleSize.Value = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 291);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(114, 13);
            this.label10.TabIndex = 44;
            this.label10.Text = "Prediction sample size:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(210, 265);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 13);
            this.label11.TabIndex = 43;
            this.label11.Text = "points";
            // 
            // trainingSampleSize
            // 
            this.trainingSampleSize.Location = new System.Drawing.Point(127, 263);
            this.trainingSampleSize.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.trainingSampleSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.trainingSampleSize.Name = "trainingSampleSize";
            this.trainingSampleSize.Size = new System.Drawing.Size(77, 20);
            this.trainingSampleSize.TabIndex = 6;
            this.trainingSampleSize.Value = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(16, 265);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(105, 13);
            this.label12.TabIndex = 42;
            this.label12.Text = "Training sample size:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(210, 58);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 41;
            this.label5.Text = "meters";
            // 
            // pointSpacing
            // 
            this.pointSpacing.Location = new System.Drawing.Point(127, 56);
            this.pointSpacing.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.pointSpacing.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.pointSpacing.Name = "pointSpacing";
            this.pointSpacing.Size = new System.Drawing.Size(77, 20);
            this.pointSpacing.TabIndex = 2;
            this.pointSpacing.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 40;
            this.label1.Text = "Point spacing:";
            // 
            // modelName
            // 
            this.modelName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modelName.Location = new System.Drawing.Point(127, 3);
            this.modelName.Name = "modelName";
            this.modelName.Size = new System.Drawing.Size(231, 20);
            this.modelName.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(83, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 39;
            this.label2.Text = "Name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(49, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 13);
            this.label3.TabIndex = 119;
            this.label3.Text = "Training area:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(236, 86);
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
            this.trainingAreas.Location = new System.Drawing.Point(127, 29);
            this.trainingAreas.Name = "trainingAreas";
            this.trainingAreas.Size = new System.Drawing.Size(231, 21);
            this.trainingAreas.Sorted = true;
            this.trainingAreas.TabIndex = 1;
            this.trainingAreas.SelectedIndexChanged += new System.EventHandler(this.trainingAreas_SelectedIndexChanged);
            // 
            // trainingStart
            // 
            this.trainingStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.trainingStart.Location = new System.Drawing.Point(127, 82);
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
            this.label4.Location = new System.Drawing.Point(45, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 121;
            this.label4.Text = "Incident types:";
            // 
            // trainingEnd
            // 
            this.trainingEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.trainingEnd.Location = new System.Drawing.Point(251, 82);
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
            this.incidentTypes.Location = new System.Drawing.Point(127, 110);
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
            this.label6.Location = new System.Drawing.Point(3, 86);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(118, 13);
            this.label6.TabIndex = 120;
            this.label6.Text = "Training start/end date:";
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
            this.smoothers.Location = new System.Drawing.Point(127, 315);
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
            this.Controls.Add(this.trainingSampleSize);
            this.Controls.Add(this.trainingEnd);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.incidentTypes);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.smoothers);
            this.Controls.Add(this.predictionSampleSize);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pointSpacing);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(367, 470);
            this.Name = "DiscreteChoiceModelOptions";
            this.Size = new System.Drawing.Size(367, 470);
            ((System.ComponentModel.ISupportInitialize)(this.predictionSampleSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingSampleSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pointSpacing)).EndInit();
            this.incidentTypesMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.NumericUpDown predictionSampleSize;
        public System.Windows.Forms.NumericUpDown trainingSampleSize;
        public System.Windows.Forms.NumericUpDown pointSpacing;
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
