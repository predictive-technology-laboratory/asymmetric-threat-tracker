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
    partial class FeatureBasedDcmOptions
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
            this.parameterizeFeatureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parameterizeSelectedFeaturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label4 = new System.Windows.Forms.Label();
            this.featureDistanceThreshold = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.negativePointStandoff = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.trainingPointSpacing = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.classifiers = new PTL.ATT.GUI.ClassifierList();
            this.featuresMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.featureDistanceThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.negativePointStandoff)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingPointSpacing)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 234);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 42;
            this.label2.Text = "Features:";
            this.toolTip.SetToolTip(this.label2, "Features to use");
            // 
            // features
            // 
            this.features.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.features.ContextMenuStrip = this.featuresMenu;
            this.features.FormattingEnabled = true;
            this.features.HorizontalScrollbar = true;
            this.features.Location = new System.Drawing.Point(71, 234);
            this.features.Name = "features";
            this.features.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.features.Size = new System.Drawing.Size(232, 147);
            this.features.TabIndex = 3;
            this.features.MouseUp += new System.Windows.Forms.MouseEventHandler(this.features_MouseUp);
            // 
            // featuresMenu
            // 
            this.featuresMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllFeaturesToolStripMenuItem,
            this.parameterizeFeatureToolStripMenuItem,
            this.parameterizeSelectedFeaturesToolStripMenuItem,
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem});
            this.featuresMenu.Name = "featuresMenu";
            this.featuresMenu.Size = new System.Drawing.Size(307, 92);
            // 
            // selectAllFeaturesToolStripMenuItem
            // 
            this.selectAllFeaturesToolStripMenuItem.Name = "selectAllFeaturesToolStripMenuItem";
            this.selectAllFeaturesToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            this.selectAllFeaturesToolStripMenuItem.Text = "Select all";
            this.selectAllFeaturesToolStripMenuItem.Click += new System.EventHandler(this.selectAllFeaturesToolStripMenuItem_Click);
            // 
            // parameterizeFeatureToolStripMenuItem
            // 
            this.parameterizeFeatureToolStripMenuItem.Name = "parameterizeFeatureToolStripMenuItem";
            this.parameterizeFeatureToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            this.parameterizeFeatureToolStripMenuItem.Text = "Parameterize feature...";
            this.parameterizeFeatureToolStripMenuItem.Click += new System.EventHandler(this.parameterizeFeatureToolStripMenuItem_Click);
            // 
            // parameterizeSelectedFeaturesToolStripMenuItem
            // 
            this.parameterizeSelectedFeaturesToolStripMenuItem.Name = "parameterizeSelectedFeaturesToolStripMenuItem";
            this.parameterizeSelectedFeaturesToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            this.parameterizeSelectedFeaturesToolStripMenuItem.Text = "Parameterize selected features....";
            this.parameterizeSelectedFeaturesToolStripMenuItem.Click += new System.EventHandler(this.parameterizeSelectedFeaturesToolStripMenuItem_Click);
            // 
            // remapSelectedFeaturesDuringPredictionToolStripMenuItem
            // 
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem.Name = "remapSelectedFeaturesDuringPredictionToolStripMenuItem";
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem.Text = "Remap selected features during prediction...";
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem.Click += new System.EventHandler(this.remapSelectedFeaturesDuringPredictionToolStripMenuItem_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(268, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 30;
            this.label4.Text = "meters";
            // 
            // featureDistanceThreshold
            // 
            this.featureDistanceThreshold.Location = new System.Drawing.Point(185, 55);
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
            this.label3.Location = new System.Drawing.Point(44, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(135, 13);
            this.label3.TabIndex = 29;
            this.label3.Text = "Feature distance threshold:";
            this.toolTip.SetToolTip(this.label3, "Distance to search for existence of spatial features. Higher values require longe" +
        "r runtimes but have more spatial fidelity than lower values, which are quicker b" +
        "ut have lower spatial fidelity.");
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(14, 81);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(51, 13);
            this.label11.TabIndex = 27;
            this.label11.Text = "Classifier:";
            this.toolTip.SetToolTip(this.label11, "Classifier to use");
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(59, 31);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(120, 13);
            this.label6.TabIndex = 53;
            this.label6.Text = "Negative point standoff:";
            this.toolTip.SetToolTip(this.label6, "How far a negative point must be from a positive point in order to be included in" +
        " the training sample");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(268, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 54;
            this.label5.Text = "meters";
            // 
            // negativePointStandoff
            // 
            this.negativePointStandoff.Location = new System.Drawing.Point(185, 29);
            this.negativePointStandoff.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.negativePointStandoff.Name = "negativePointStandoff";
            this.negativePointStandoff.Size = new System.Drawing.Size(77, 20);
            this.negativePointStandoff.TabIndex = 52;
            this.negativePointStandoff.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(268, 5);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(38, 13);
            this.label7.TabIndex = 57;
            this.label7.Text = "meters";
            // 
            // trainingPointSpacing
            // 
            this.trainingPointSpacing.Location = new System.Drawing.Point(185, 3);
            this.trainingPointSpacing.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.trainingPointSpacing.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.trainingPointSpacing.Name = "trainingPointSpacing";
            this.trainingPointSpacing.Size = new System.Drawing.Size(77, 20);
            this.trainingPointSpacing.TabIndex = 55;
            this.trainingPointSpacing.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(23, 5);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(156, 13);
            this.label9.TabIndex = 56;
            this.label9.Text = "Negative training point spacing:";
            this.toolTip.SetToolTip(this.label9, "How far apart negative training points should be spaced");
            // 
            // classifiers
            // 
            this.classifiers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.classifiers.FormattingEnabled = true;
            this.classifiers.Location = new System.Drawing.Point(71, 81);
            this.classifiers.Name = "classifiers";
            this.classifiers.Size = new System.Drawing.Size(232, 147);
            this.classifiers.Sorted = true;
            this.classifiers.TabIndex = 2;
            // 
            // FeatureBasedDcmOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label7);
            this.Controls.Add(this.trainingPointSpacing);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.negativePointStandoff);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.features);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.classifiers);
            this.Controls.Add(this.featureDistanceThreshold);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label11);
            this.MinimumSize = new System.Drawing.Size(306, 384);
            this.Name = "FeatureBasedDcmOptions";
            this.Size = new System.Drawing.Size(306, 384);
            this.featuresMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.featureDistanceThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.negativePointStandoff)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingPointSpacing)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label11;
        private ClassifierList classifiers;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown featureDistanceThreshold;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.ListBox features;
        private System.Windows.Forms.ContextMenuStrip featuresMenu;
        private System.Windows.Forms.ToolStripMenuItem selectAllFeaturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem remapSelectedFeaturesDuringPredictionToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ToolStripMenuItem parameterizeFeatureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem parameterizeSelectedFeaturesToolStripMenuItem;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown negativePointStandoff;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.NumericUpDown trainingPointSpacing;
        private System.Windows.Forms.Label label9;
    }
}
