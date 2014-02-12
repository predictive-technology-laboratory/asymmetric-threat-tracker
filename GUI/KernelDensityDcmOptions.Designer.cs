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
    partial class KernelDensityDcmOptions
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
            this.normalize = new System.Windows.Forms.CheckBox();
            this.trainingSampleSize = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.trainingSampleSize)).BeginInit();
            this.SuspendLayout();
            // 
            // normalize
            // 
            this.normalize.AutoSize = true;
            this.normalize.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.normalize.Checked = true;
            this.normalize.CheckState = System.Windows.Forms.CheckState.Checked;
            this.normalize.Location = new System.Drawing.Point(52, 29);
            this.normalize.Name = "normalize";
            this.normalize.Size = new System.Drawing.Size(75, 17);
            this.normalize.TabIndex = 0;
            this.normalize.Text = "Normalize:";
            this.toolTip.SetToolTip(this.normalize, "Whether or not to normalize the resulting density estimate to be in [0,1]");
            this.normalize.UseVisualStyleBackColor = true;
            // 
            // trainingSampleSize
            // 
            this.trainingSampleSize.Location = new System.Drawing.Point(115, 3);
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
            500,
            0,
            0,
            0});
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(198, 5);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 13);
            this.label11.TabIndex = 49;
            this.label11.Text = "points";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(4, 5);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(105, 13);
            this.label12.TabIndex = 48;
            this.label12.Text = "Training sample size:";
            this.toolTip.SetToolTip(this.label12, "Number of points to use in constructing the KDE. Random points will be removed to" +
        " meet this requirement.");
            // 
            // KernelDensityDcmOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.trainingSampleSize);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.normalize);
            this.Name = "KernelDensityDcmOptions";
            this.Size = new System.Drawing.Size(250, 52);
            ((System.ComponentModel.ISupportInitialize)(this.trainingSampleSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox normalize;
        public System.Windows.Forms.NumericUpDown trainingSampleSize;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
