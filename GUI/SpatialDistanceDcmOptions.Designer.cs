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
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label4 = new System.Windows.Forms.Label();
            this.featureDistanceThreshold = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.trainingSampleSize = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.predictionSampleSize = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.classifiers = new PTL.ATT.GUI.ClassifierList();
            this.featuresMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.featureDistanceThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingSampleSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.predictionSampleSize)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 234);
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
            this.features.Location = new System.Drawing.Point(64, 234);
            this.features.Name = "features";
            this.features.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.features.Size = new System.Drawing.Size(228, 147);
            this.features.Sorted = true;
            this.features.TabIndex = 3;
            // 
            // featuresMenu
            // 
            this.featuresMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllFeaturesToolStripMenuItem,
            this.remapSelectedFeaturesDuringPredictionToolStripMenuItem});
            this.featuresMenu.Name = "featuresMenu";
            this.featuresMenu.Size = new System.Drawing.Size(307, 70);
            // 
            // selectAllFeaturesToolStripMenuItem
            // 
            this.selectAllFeaturesToolStripMenuItem.Name = "selectAllFeaturesToolStripMenuItem";
            this.selectAllFeaturesToolStripMenuItem.Size = new System.Drawing.Size(306, 22);
            this.selectAllFeaturesToolStripMenuItem.Text = "Select all";
            this.selectAllFeaturesToolStripMenuItem.Click += new System.EventHandler(this.selectAllFeaturesToolStripMenuItem_Click);
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
            this.label4.Location = new System.Drawing.Point(264, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 30;
            this.label4.Text = "meters";
            // 
            // featureDistanceThreshold
            // 
            this.featureDistanceThreshold.Location = new System.Drawing.Point(181, 55);
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
            this.label3.Location = new System.Drawing.Point(40, 57);
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
            this.label11.Location = new System.Drawing.Point(7, 81);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(51, 13);
            this.label11.TabIndex = 27;
            this.label11.Text = "Classifier:";
            this.toolTip.SetToolTip(this.label11, "Classifier to use");
            // 
            // trainingSampleSize
            // 
            this.trainingSampleSize.Location = new System.Drawing.Point(181, 3);
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
            this.trainingSampleSize.TabIndex = 46;
            this.trainingSampleSize.Value = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(264, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 49;
            this.label1.Text = "points";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(70, 5);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(105, 13);
            this.label12.TabIndex = 48;
            this.label12.Text = "Training sample size:";
            this.toolTip.SetToolTip(this.label12, "Number of points (both positive and negative) to use when training the model. Pos" +
        "itive points will be randomly removed to meet this requirement.");
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(61, 31);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(114, 13);
            this.label10.TabIndex = 50;
            this.label10.Text = "Prediction sample size:";
            this.toolTip.SetToolTip(this.label10, "Number of points to use when making predictions using this model. Points will be " +
        "randomly removed to meet this requirement.");
            // 
            // predictionSampleSize
            // 
            this.predictionSampleSize.Location = new System.Drawing.Point(181, 29);
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
            this.predictionSampleSize.TabIndex = 47;
            this.predictionSampleSize.Value = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(264, 31);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(35, 13);
            this.label8.TabIndex = 51;
            this.label8.Text = "points";
            // 
            // classifiers
            // 
            this.classifiers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.classifiers.FormattingEnabled = true;
            this.classifiers.Location = new System.Drawing.Point(64, 81);
            this.classifiers.Name = "classifiers";
            this.classifiers.Size = new System.Drawing.Size(228, 147);
            this.classifiers.Sorted = true;
            this.classifiers.TabIndex = 2;
            // 
            // SpatialDistanceDcmOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.trainingSampleSize);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.predictionSampleSize);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.features);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.classifiers);
            this.Controls.Add(this.featureDistanceThreshold);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label11);
            this.Name = "SpatialDistanceDcmOptions";
            this.Size = new System.Drawing.Size(302, 392);
            this.featuresMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.featureDistanceThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingSampleSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.predictionSampleSize)).EndInit();
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
        public System.Windows.Forms.NumericUpDown trainingSampleSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label10;
        public System.Windows.Forms.NumericUpDown predictionSampleSize;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
