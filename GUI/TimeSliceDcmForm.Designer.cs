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
    partial class TimeSliceDcmForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.timeSliceHours = new System.Windows.Forms.NumericUpDown();
            this.cancel = new System.Windows.Forms.Button();
            this.ok = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.modelName = new System.Windows.Forms.TextBox();
            this.pointSpacing = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.featureDistanceThreshold = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.timeSlicesPerPeriod = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.predictionSampleSize = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.trainingSampleSize = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.classifyNonZeroVectorsUniformly = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.smoothers = new PTL.ATT.GUI.SmootherList();
            this.label14 = new System.Windows.Forms.Label();
            this.classifiers = new PTL.ATT.GUI.ClassifierList();
            ((System.ComponentModel.ISupportInitialize)(this.timeSliceHours)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pointSpacing)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.featureDistanceThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeSlicesPerPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.predictionSampleSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingSampleSize)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(125, 171);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Time slice:";
            // 
            // timeSliceHours
            // 
            this.timeSliceHours.Location = new System.Drawing.Point(188, 169);
            this.timeSliceHours.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.timeSliceHours.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.timeSliceHours.Name = "timeSliceHours";
            this.timeSliceHours.Size = new System.Drawing.Size(77, 20);
            this.timeSliceHours.TabIndex = 5;
            this.timeSliceHours.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            // 
            // cancel
            // 
            this.cancel.Location = new System.Drawing.Point(188, 423);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 9;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // ok
            // 
            this.ok.Location = new System.Drawing.Point(107, 423);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 23);
            this.ok.TabIndex = 8;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(144, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Name:";
            // 
            // modelName
            // 
            this.modelName.Location = new System.Drawing.Point(188, 16);
            this.modelName.Name = "modelName";
            this.modelName.Size = new System.Drawing.Size(182, 20);
            this.modelName.TabIndex = 0;
            // 
            // pointSpacing
            // 
            this.pointSpacing.Location = new System.Drawing.Point(188, 42);
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
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(108, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Point spacing:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(269, 70);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "meters";
            // 
            // featureDistanceThreshold
            // 
            this.featureDistanceThreshold.Location = new System.Drawing.Point(188, 68);
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
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(47, 70);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(135, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Feature distance threshold:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(269, 44);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "meters";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(269, 171);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(33, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "hours";
            // 
            // timeSlicesPerPeriod
            // 
            this.timeSlicesPerPeriod.Location = new System.Drawing.Point(188, 195);
            this.timeSlicesPerPeriod.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.timeSlicesPerPeriod.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.timeSlicesPerPeriod.Name = "timeSlicesPerPeriod";
            this.timeSlicesPerPeriod.Size = new System.Drawing.Size(77, 20);
            this.timeSlicesPerPeriod.TabIndex = 6;
            this.timeSlicesPerPeriod.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(70, 197);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(112, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Time slices per period:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(269, 122);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(35, 13);
            this.label8.TabIndex = 25;
            this.label8.Text = "points";
            // 
            // predictionSampleSize
            // 
            this.predictionSampleSize.Location = new System.Drawing.Point(188, 120);
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
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(68, 122);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(114, 13);
            this.label10.TabIndex = 24;
            this.label10.Text = "Prediction sample size:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(269, 96);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 13);
            this.label11.TabIndex = 22;
            this.label11.Text = "points";
            // 
            // trainingSampleSize
            // 
            this.trainingSampleSize.Location = new System.Drawing.Point(188, 94);
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
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(77, 96);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(105, 13);
            this.label12.TabIndex = 21;
            this.label12.Text = "Training sample size:";
            // 
            // classifyNonZeroVectorsUniformly
            // 
            this.classifyNonZeroVectorsUniformly.AutoSize = true;
            this.classifyNonZeroVectorsUniformly.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.classifyNonZeroVectorsUniformly.Location = new System.Drawing.Point(12, 146);
            this.classifyNonZeroVectorsUniformly.Name = "classifyNonZeroVectorsUniformly";
            this.classifyNonZeroVectorsUniformly.Size = new System.Drawing.Size(190, 17);
            this.classifyNonZeroVectorsUniformly.TabIndex = 7;
            this.classifyNonZeroVectorsUniformly.Text = "Classify non-zero vectors uniformly:";
            this.classifyNonZeroVectorsUniformly.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(122, 322);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(60, 13);
            this.label13.TabIndex = 28;
            this.label13.Text = "Smoothers:";
            // 
            // smoothers
            // 
            this.smoothers.FormattingEnabled = true;
            this.smoothers.Location = new System.Drawing.Point(188, 322);
            this.smoothers.Name = "smoothers";
            this.smoothers.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.smoothers.Size = new System.Drawing.Size(182, 95);
            this.smoothers.Sorted = true;
            this.smoothers.TabIndex = 29;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(131, 221);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(51, 13);
            this.label14.TabIndex = 31;
            this.label14.Text = "Classifier:";
            // 
            // classifiers
            // 
            this.classifiers.FormattingEnabled = true;
            this.classifiers.Location = new System.Drawing.Point(188, 221);
            this.classifiers.Name = "classifiers";
            this.classifiers.Size = new System.Drawing.Size(177, 95);
            this.classifiers.Sorted = true;
            this.classifiers.TabIndex = 30;
            // 
            // TimeSliceDcmForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 455);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.classifiers);
            this.Controls.Add(this.smoothers);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.classifyNonZeroVectorsUniformly);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.predictionSampleSize);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.trainingSampleSize);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.timeSlicesPerPeriod);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.featureDistanceThreshold);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.pointSpacing);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.modelName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.timeSliceHours);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "TimeSliceDcmForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select time slice DCM options...";
            ((System.ComponentModel.ISupportInitialize)(this.timeSliceHours)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pointSpacing)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.featureDistanceThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeSlicesPerPeriod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.predictionSampleSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingSampleSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown timeSliceHours;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox modelName;
        private System.Windows.Forms.NumericUpDown pointSpacing;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown featureDistanceThreshold;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown timeSlicesPerPeriod;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown predictionSampleSize;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown trainingSampleSize;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox classifyNonZeroVectorsUniformly;
        private System.Windows.Forms.Label label13;
        private SmootherList smoothers;
        private System.Windows.Forms.Label label14;
        private ClassifierList classifiers;
    }
}