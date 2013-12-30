namespace PTL.ATT.GUI
{
    partial class SpatialDistanceDcmOptions
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
            this.label2 = new System.Windows.Forms.Label();
            this.features = new System.Windows.Forms.ListBox();
            this.featuresMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectAllFeaturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearFeatureRemappingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label4 = new System.Windows.Forms.Label();
            this.featureDistanceThreshold = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.classifyNonZeroVectorsUniformly = new System.Windows.Forms.CheckBox();
            this.classifiers = new PTL.ATT.GUI.ClassifierList();
            this.featuresMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.featureDistanceThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 210);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 42;
            this.label2.Text = "Features:";
            // 
            // features
            // 
            this.features.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.features.ContextMenuStrip = this.featuresMenu;
            this.features.FormattingEnabled = true;
            this.features.HorizontalScrollbar = true;
            this.features.Location = new System.Drawing.Point(71, 210);
            this.features.Name = "features";
            this.features.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.features.Size = new System.Drawing.Size(228, 173);
            this.features.Sorted = true;
            this.features.TabIndex = 3;
            // 
            // featuresMenu
            // 
            this.featuresMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllFeaturesToolStripMenuItem,
            this.toolStripSeparator5,
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem,
            this.clearFeatureRemappingToolStripMenuItem});
            this.featuresMenu.Name = "featuresMenu";
            this.featuresMenu.Size = new System.Drawing.Size(307, 76);
            // 
            // selectAllFeaturesToolStripMenuItem
            // 
            this.selectAllFeaturesToolStripMenuItem.Name = "selectAllFeaturesToolStripMenuItem";
            this.selectAllFeaturesToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            this.selectAllFeaturesToolStripMenuItem.Text = "Select all";
            this.selectAllFeaturesToolStripMenuItem.Click += new System.EventHandler(this.selectAllFeaturesToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(303, 6);
            // 
            // remapSelectedFeaturesDuringPredictionToolStripMenuItem
            // 
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem.Name = "remapSelectedFeaturesDuringPredictionToolStripMenuItem";
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem.Text = "Remap selected features during prediction...";
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem.Click += new System.EventHandler(this.remapSelectedFeaturesDuringPredictionToolStripMenuItem_Click);
            // 
            // clearFeatureRemappingToolStripMenuItem
            // 
            this.clearFeatureRemappingToolStripMenuItem.Name = "clearFeatureRemappingToolStripMenuItem";
            this.clearFeatureRemappingToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            this.clearFeatureRemappingToolStripMenuItem.Text = "Clear feature remapping";
            this.clearFeatureRemappingToolStripMenuItem.Click += new System.EventHandler(this.clearFeatureRemappingToolStripMenuItem_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(264, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 30;
            this.label4.Text = "meters";
            // 
            // featureDistanceThreshold
            // 
            this.featureDistanceThreshold.Location = new System.Drawing.Point(181, 3);
            this.featureDistanceThreshold.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.featureDistanceThreshold.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.featureDistanceThreshold.Name = "featureDistanceThreshold";
            this.featureDistanceThreshold.Size = new System.Drawing.Size(77, 20);
            this.featureDistanceThreshold.TabIndex = 0;
            this.featureDistanceThreshold.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(40, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(135, 13);
            this.label3.TabIndex = 29;
            this.label3.Text = "Feature distance threshold:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(14, 57);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(51, 13);
            this.label11.TabIndex = 27;
            this.label11.Text = "Classifier:";
            // 
            // classifyNonZeroVectorsUniformly
            // 
            this.classifyNonZeroVectorsUniformly.AutoSize = true;
            this.classifyNonZeroVectorsUniformly.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.classifyNonZeroVectorsUniformly.Location = new System.Drawing.Point(3, 29);
            this.classifyNonZeroVectorsUniformly.Name = "classifyNonZeroVectorsUniformly";
            this.classifyNonZeroVectorsUniformly.Size = new System.Drawing.Size(190, 17);
            this.classifyNonZeroVectorsUniformly.TabIndex = 1;
            this.classifyNonZeroVectorsUniformly.Text = "Classify non-zero vectors uniformly:";
            this.classifyNonZeroVectorsUniformly.UseVisualStyleBackColor = true;
            // 
            // classifiers
            // 
            this.classifiers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.classifiers.FormattingEnabled = true;
            this.classifiers.Location = new System.Drawing.Point(71, 57);
            this.classifiers.Name = "classifiers";
            this.classifiers.Size = new System.Drawing.Size(228, 147);
            this.classifiers.Sorted = true;
            this.classifiers.TabIndex = 2;
            // 
            // SpatialDistanceDcmOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.features);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.classifiers);
            this.Controls.Add(this.featureDistanceThreshold);
            this.Controls.Add(this.classifyNonZeroVectorsUniformly);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label11);
            this.Name = "SpatialDistanceDcmOptions";
            this.Size = new System.Drawing.Size(302, 386);
            this.featuresMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.featureDistanceThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label11;
        private ClassifierList classifiers;
        private System.Windows.Forms.CheckBox classifyNonZeroVectorsUniformly;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown featureDistanceThreshold;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.ListBox features;
        private System.Windows.Forms.ContextMenuStrip featuresMenu;
        private System.Windows.Forms.ToolStripMenuItem selectAllFeaturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem remapSelectedFeaturesDuringPredictionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearFeatureRemappingToolStripMenuItem;
    }
}
