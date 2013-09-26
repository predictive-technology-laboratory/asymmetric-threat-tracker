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
    partial class SpatialDistanceDcmForm
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
            this.cancel = new System.Windows.Forms.Button();
            this.ok = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.modelName = new System.Windows.Forms.TextBox();
            this.pointSpacing = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.featureDistanceThreshold = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.trainingSampleSize = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.predictionSampleSize = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.classifyNonZeroVectorsUniformly = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.smoothers = new PTL.ATT.GUI.SmootherList();
            this.classifiers = new PTL.ATT.GUI.ClassifierList();
            this.label11 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pointSpacing)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.featureDistanceThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingSampleSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.predictionSampleSize)).BeginInit();
            this.SuspendLayout();
            // 
            // cancel
            // 
            this.cancel.Location = new System.Drawing.Point(190, 370);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 7;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // ok
            // 
            this.ok.Location = new System.Drawing.Point(109, 370);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 23);
            this.ok.TabIndex = 6;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(146, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Name:";
            // 
            // modelName
            // 
            this.modelName.Location = new System.Drawing.Point(190, 11);
            this.modelName.Name = "modelName";
            this.modelName.Size = new System.Drawing.Size(177, 20);
            this.modelName.TabIndex = 0;
            // 
            // pointSpacing
            // 
            this.pointSpacing.Location = new System.Drawing.Point(190, 37);
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
            this.pointSpacing.TabIndex = 1;
            this.pointSpacing.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(110, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Point spacing:";
            // 
            // featureDistanceThreshold
            // 
            this.featureDistanceThreshold.Location = new System.Drawing.Point(190, 63);
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
            this.featureDistanceThreshold.TabIndex = 2;
            this.featureDistanceThreshold.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(49, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(135, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Feature distance threshold:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(273, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "meters";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(273, 39);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "meters";
            // 
            // trainingSampleSize
            // 
            this.trainingSampleSize.Location = new System.Drawing.Point(190, 89);
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
            this.trainingSampleSize.TabIndex = 3;
            this.trainingSampleSize.Value = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(79, 91);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(105, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Training sample size:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(273, 91);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(35, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "points";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(273, 117);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(35, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "points";
            // 
            // predictionSampleSize
            // 
            this.predictionSampleSize.Location = new System.Drawing.Point(190, 115);
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
            this.predictionSampleSize.TabIndex = 4;
            this.predictionSampleSize.Value = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(70, 117);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(114, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Prediction sample size:";
            // 
            // classifyNonZeroVectorsUniformly
            // 
            this.classifyNonZeroVectorsUniformly.AutoSize = true;
            this.classifyNonZeroVectorsUniformly.Location = new System.Drawing.Point(14, 142);
            this.classifyNonZeroVectorsUniformly.Name = "classifyNonZeroVectorsUniformly";
            this.classifyNonZeroVectorsUniformly.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.classifyNonZeroVectorsUniformly.Size = new System.Drawing.Size(190, 17);
            this.classifyNonZeroVectorsUniformly.TabIndex = 5;
            this.classifyNonZeroVectorsUniformly.Text = "Classify non-zero vectors uniformly:";
            this.classifyNonZeroVectorsUniformly.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(124, 266);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(60, 13);
            this.label10.TabIndex = 21;
            this.label10.Text = "Smoothers:";
            // 
            // smoothers
            // 
            this.smoothers.FormattingEnabled = true;
            this.smoothers.Location = new System.Drawing.Point(190, 266);
            this.smoothers.Name = "smoothers";
            this.smoothers.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.smoothers.Size = new System.Drawing.Size(177, 95);
            this.smoothers.TabIndex = 22;
            // 
            // classifiers
            // 
            this.classifiers.FormattingEnabled = true;
            this.classifiers.Location = new System.Drawing.Point(190, 165);
            this.classifiers.Name = "classifiers";
            this.classifiers.Size = new System.Drawing.Size(177, 95);
            this.classifiers.TabIndex = 23;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(133, 165);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(51, 13);
            this.label11.TabIndex = 24;
            this.label11.Text = "Classifier:";
            // 
            // SpatialDistanceDcmForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 405);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.classifiers);
            this.Controls.Add(this.smoothers);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.classifyNonZeroVectorsUniformly);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.predictionSampleSize);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.trainingSampleSize);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.featureDistanceThreshold);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pointSpacing);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.modelName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SpatialDistanceDcmForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select spatial distance DCM options...";
            ((System.ComponentModel.ISupportInitialize)(this.pointSpacing)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.featureDistanceThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingSampleSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.predictionSampleSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox modelName;
        private System.Windows.Forms.NumericUpDown pointSpacing;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown featureDistanceThreshold;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown trainingSampleSize;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown predictionSampleSize;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckBox classifyNonZeroVectorsUniformly;
        private System.Windows.Forms.Label label10;
        private SmootherList smoothers;
        private ClassifierList classifiers;
        private System.Windows.Forms.Label label11;
    }
}