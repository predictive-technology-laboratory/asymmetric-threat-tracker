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
    partial class SelectModelForm
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
            this.ok = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.spatialDistanceDCM = new System.Windows.Forms.RadioButton();
            this.timeSliceDCM = new System.Windows.Forms.RadioButton();
            this.kdeDCM = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // ok
            // 
            this.ok.Location = new System.Drawing.Point(17, 97);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 23);
            this.ok.TabIndex = 3;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // cancel
            // 
            this.cancel.Location = new System.Drawing.Point(98, 97);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 4;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // spatialDistanceDCM
            // 
            this.spatialDistanceDCM.AutoSize = true;
            this.spatialDistanceDCM.Location = new System.Drawing.Point(39, 35);
            this.spatialDistanceDCM.Name = "spatialDistanceDCM";
            this.spatialDistanceDCM.Size = new System.Drawing.Size(127, 17);
            this.spatialDistanceDCM.TabIndex = 1;
            this.spatialDistanceDCM.Text = "Spatial distance DCM";
            this.spatialDistanceDCM.UseVisualStyleBackColor = true;
            // 
            // timeSliceDCM
            // 
            this.timeSliceDCM.AutoSize = true;
            this.timeSliceDCM.Location = new System.Drawing.Point(39, 58);
            this.timeSliceDCM.Name = "timeSliceDCM";
            this.timeSliceDCM.Size = new System.Drawing.Size(99, 17);
            this.timeSliceDCM.TabIndex = 2;
            this.timeSliceDCM.Text = "Time slice DCM";
            this.timeSliceDCM.UseVisualStyleBackColor = true;
            // 
            // kdeDCM
            // 
            this.kdeDCM.AutoSize = true;
            this.kdeDCM.Checked = true;
            this.kdeDCM.Location = new System.Drawing.Point(39, 12);
            this.kdeDCM.Name = "kdeDCM";
            this.kdeDCM.Size = new System.Drawing.Size(118, 17);
            this.kdeDCM.TabIndex = 0;
            this.kdeDCM.TabStop = true;
            this.kdeDCM.Text = "Kernel density DCM";
            this.kdeDCM.UseVisualStyleBackColor = true;
            // 
            // SelectModelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(194, 135);
            this.Controls.Add(this.spatialDistanceDCM);
            this.Controls.Add(this.timeSliceDCM);
            this.Controls.Add(this.kdeDCM);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SelectModelForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select model type...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.RadioButton timeSliceDCM;
        private System.Windows.Forms.RadioButton spatialDistanceDCM;
        private System.Windows.Forms.RadioButton kdeDCM;
    }
}