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
    partial class FeatureRemappingForm
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
            this.components = new System.ComponentModel.Container();
            this.training = new System.Windows.Forms.ListBox();
            this.prediction = new System.Windows.Forms.ListBox();
            this.ok = new System.Windows.Forms.Button();
            this.reset = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // training
            // 
            this.training.FormattingEnabled = true;
            this.training.HorizontalScrollbar = true;
            this.training.Location = new System.Drawing.Point(12, 33);
            this.training.Name = "training";
            this.training.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.training.Size = new System.Drawing.Size(266, 290);
            this.training.TabIndex = 0;
            // 
            // prediction
            // 
            this.prediction.FormattingEnabled = true;
            this.prediction.HorizontalScrollbar = true;
            this.prediction.Location = new System.Drawing.Point(284, 33);
            this.prediction.Name = "prediction";
            this.prediction.Size = new System.Drawing.Size(266, 290);
            this.prediction.TabIndex = 1;
            this.prediction.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.available_MouseDoubleClick);
            // 
            // ok
            // 
            this.ok.Location = new System.Drawing.Point(475, 329);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 23);
            this.ok.TabIndex = 2;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // reset
            // 
            this.reset.Location = new System.Drawing.Point(394, 329);
            this.reset.Name = "reset";
            this.reset.Size = new System.Drawing.Size(75, 23);
            this.reset.TabIndex = 3;
            this.reset.Text = "Reset";
            this.reset.UseVisualStyleBackColor = true;
            this.reset.Click += new System.EventHandler(this.reset_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Training feature:";
            this.toolTip.SetToolTip(this.label1, "Original feature to remap during prediction.");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(281, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Prediction feature:";
            this.toolTip.SetToolTip(this.label2, "Feature to use during prediction. Right-click to map the selected training featur" +
        "e to a prediction feature.");
            // 
            // FeatureRemappingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 362);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.reset);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.prediction);
            this.Controls.Add(this.training);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FeatureRemappingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Remap features during prediction...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox training;
        private System.Windows.Forms.ListBox prediction;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button reset;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolTip toolTip;
    }
}