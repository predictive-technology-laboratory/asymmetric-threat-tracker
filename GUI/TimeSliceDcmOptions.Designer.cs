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
            ((System.ComponentModel.ISupportInitialize)(this.timeSlicesPerPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeSliceHours)).BeginInit();
            this.SuspendLayout();
            // 
            // timeSlicesPerPeriod
            // 
            this.timeSlicesPerPeriod.Location = new System.Drawing.Point(122, 29);
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
            this.label9.Location = new System.Drawing.Point(4, 31);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(112, 13);
            this.label9.TabIndex = 23;
            this.label9.Text = "Time slices per period:";
            this.toolTip.SetToolTip(this.label9, "Number of time slices (of above duration) that compose a single period");
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(205, 5);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(33, 13);
            this.label7.TabIndex = 22;
            this.label7.Text = "hours";
            // 
            // timeSliceHours
            // 
            this.timeSliceHours.Location = new System.Drawing.Point(122, 3);
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
            this.label1.Location = new System.Drawing.Point(59, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Time slice:";
            this.toolTip.SetToolTip(this.label1, "Duration of a single time slice");
            // 
            // TimeSliceDcmOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label7);
            this.Controls.Add(this.timeSlicesPerPeriod);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.timeSliceHours);
            this.Name = "TimeSliceDcmOptions";
            this.Size = new System.Drawing.Size(247, 55);
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
    }
}
