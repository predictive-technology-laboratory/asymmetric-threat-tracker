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
    partial class TimeSliceDcmOptions
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
            this.timeSlicesPerPeriod = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.timeSliceHours = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this._CheckedListBoxSlicesToRun = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.timeSlicesPerPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeSliceHours)).BeginInit();
            this.SuspendLayout();
            // 
            // timeSlicesPerPeriod
            // 
            this.timeSlicesPerPeriod.Location = new System.Drawing.Point(142, 36);
            this.timeSlicesPerPeriod.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.timeSlicesPerPeriod.Size = new System.Drawing.Size(90, 24);
            this.timeSlicesPerPeriod.TabIndex = 1;
            this.timeSlicesPerPeriod.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(5, 38);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(142, 17);
            this.label9.TabIndex = 23;
            this.label9.Text = "Time slices per period:";
            this.toolTip.SetToolTip(this.label9, "Number of time slices (of above duration) that compose a single period.");
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(239, 6);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(43, 17);
            this.label7.TabIndex = 22;
            this.label7.Text = "hours";
            // 
            // timeSliceHours
            // 
            this.timeSliceHours.Location = new System.Drawing.Point(142, 4);
            this.timeSliceHours.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
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
            this.timeSliceHours.Size = new System.Drawing.Size(90, 24);
            this.timeSliceHours.TabIndex = 0;
            this.timeSliceHours.Value = new decimal(new int[] {
            6,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(69, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 17);
            this.label1.TabIndex = 19;
            this.label1.Text = "Time slice:";
            this.toolTip.SetToolTip(this.label1, "Duration of a single time slice.");
            // 
            // _CheckedListBoxSlicesToRun
            // 
            this._CheckedListBoxSlicesToRun.FormattingEnabled = true;
            this._CheckedListBoxSlicesToRun.Location = new System.Drawing.Point(142, 68);
            this._CheckedListBoxSlicesToRun.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this._CheckedListBoxSlicesToRun.Name = "_CheckedListBoxSlicesToRun";
            this._CheckedListBoxSlicesToRun.ScrollAlwaysVisible = true;
            this._CheckedListBoxSlicesToRun.Size = new System.Drawing.Size(175, 232);
            this._CheckedListBoxSlicesToRun.TabIndex = 26;
            this._CheckedListBoxSlicesToRun.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this._CheckedListBoxSlicesToRun_ItemCheck);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 17);
            this.label2.TabIndex = 25;
            this.label2.Text = "Which slice(s) to run:";
            // 
            // TimeSliceDcmOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._CheckedListBoxSlicesToRun);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.timeSlicesPerPeriod);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.timeSliceHours);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "TimeSliceDcmOptions";
            this.Size = new System.Drawing.Size(348, 319);
            ((System.ComponentModel.ISupportInitialize)(this.timeSlicesPerPeriod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeSliceHours)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown timeSlicesPerPeriod;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown timeSliceHours;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.CheckedListBox _CheckedListBoxSlicesToRun;
        private System.Windows.Forms.Label label2;
    }
}
