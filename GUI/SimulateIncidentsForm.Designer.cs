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
    partial class SimulateIncidentsForm
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
            this.simulateN = new System.Windows.Forms.NumericUpDown();
            this.simulateIncidents = new System.Windows.Forms.Button();
            this.simulateEnd = new System.Windows.Forms.DateTimePicker();
            this.simulateStart = new System.Windows.Forms.DateTimePicker();
            this.close = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.simulateN)).BeginInit();
            this.SuspendLayout();
            // 
            // simulateN
            // 
            this.simulateN.Location = new System.Drawing.Point(142, 76);
            this.simulateN.Maximum = new decimal(new int[] {
            9999999,
            0,
            0,
            0});
            this.simulateN.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.simulateN.Name = "simulateN";
            this.simulateN.Size = new System.Drawing.Size(50, 20);
            this.simulateN.TabIndex = 2;
            this.simulateN.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.simulateN.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            // 
            // simulateIncidents
            // 
            this.simulateIncidents.Location = new System.Drawing.Point(142, 102);
            this.simulateIncidents.Name = "simulateIncidents";
            this.simulateIncidents.Size = new System.Drawing.Size(111, 23);
            this.simulateIncidents.TabIndex = 3;
            this.simulateIncidents.Text = "Simulate incidents";
            this.simulateIncidents.UseVisualStyleBackColor = true;
            this.simulateIncidents.Click += new System.EventHandler(this.simulateIncidents_Click);
            // 
            // simulateEnd
            // 
            this.simulateEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.simulateEnd.Location = new System.Drawing.Point(142, 50);
            this.simulateEnd.Name = "simulateEnd";
            this.simulateEnd.Size = new System.Drawing.Size(84, 20);
            this.simulateEnd.TabIndex = 1;
            this.simulateEnd.Value = new System.DateTime(2012, 8, 30, 0, 0, 0, 0);
            this.simulateEnd.ValueChanged += new System.EventHandler(this.simulateEnd_ValueChanged);
            // 
            // simulateStart
            // 
            this.simulateStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.simulateStart.Location = new System.Drawing.Point(142, 24);
            this.simulateStart.Name = "simulateStart";
            this.simulateStart.Size = new System.Drawing.Size(84, 20);
            this.simulateStart.TabIndex = 0;
            this.simulateStart.ValueChanged += new System.EventHandler(this.simulateStart_ValueChanged);
            // 
            // close
            // 
            this.close.Location = new System.Drawing.Point(142, 147);
            this.close.Name = "close";
            this.close.Size = new System.Drawing.Size(75, 23);
            this.close.TabIndex = 4;
            this.close.Text = "Close";
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.close_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(80, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 78;
            this.label1.Text = "Start date:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(32, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 13);
            this.label2.TabIndex = 79;
            this.label2.Text = "Number of incidents:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(83, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 80;
            this.label3.Text = "End date:";
            // 
            // SimulateIncidentsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(325, 191);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.close);
            this.Controls.Add(this.simulateN);
            this.Controls.Add(this.simulateIncidents);
            this.Controls.Add(this.simulateEnd);
            this.Controls.Add(this.simulateStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SimulateIncidentsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SimulateIncidentsForm";
            ((System.ComponentModel.ISupportInitialize)(this.simulateN)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown simulateN;
        private System.Windows.Forms.Button simulateIncidents;
        private System.Windows.Forms.DateTimePicker simulateEnd;
        private System.Windows.Forms.DateTimePicker simulateStart;
        private System.Windows.Forms.Button close;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;

    }
}