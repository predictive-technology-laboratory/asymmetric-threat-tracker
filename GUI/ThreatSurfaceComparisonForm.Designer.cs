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
    partial class ThreatSurfaceComparisonForm
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
            this._FolderBrowserDialogSaveImages = new System.Windows.Forms.FolderBrowserDialog();
            this.multiDynamicThreatMap = new PTL.ATT.GUI.Visualization.MultiDynamicThreatMap();
            this.SuspendLayout();
            // 
            // multiDynamicThreatMap
            // 
            this.multiDynamicThreatMap.BackColor = System.Drawing.Color.White;
            this.multiDynamicThreatMap.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.multiDynamicThreatMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.multiDynamicThreatMap.Location = new System.Drawing.Point(0, 0);
            this.multiDynamicThreatMap.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.multiDynamicThreatMap.Name = "multiDynamicThreatMap";
            this.multiDynamicThreatMap.Size = new System.Drawing.Size(717, 226);
            this.multiDynamicThreatMap.TabIndex = 0;
            // 
            // ThreatSurfaceComparisonForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(717, 226);
            this.Controls.Add(this.multiDynamicThreatMap);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ThreatSurfaceComparisonForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Compare Threat Surfaces";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog _FolderBrowserDialogSaveImages;
        private Visualization.MultiDynamicThreatMap multiDynamicThreatMap;
    }
}