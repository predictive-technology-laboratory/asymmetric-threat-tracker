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

namespace PTL.ATT.GUI.Visualization
{
    partial class CustomSpatialFeatureForm
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
            this.createPoints = new System.Windows.Forms.Button();
            this.createLine = new System.Windows.Forms.Button();
            this.createPolygon = new System.Windows.Forms.Button();
            this.elements = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.removeSelectedElements = new System.Windows.Forms.Button();
            this.createFeature = new System.Windows.Forms.Button();
            this.points = new System.Windows.Forms.ListBox();
            this.clearPoints = new System.Windows.Forms.Button();
            this.createConnectedLine = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // createPoints
            // 
            this.createPoints.Location = new System.Drawing.Point(286, 30);
            this.createPoints.Name = "createPoints";
            this.createPoints.Size = new System.Drawing.Size(121, 23);
            this.createPoints.TabIndex = 0;
            this.createPoints.Text = "Create point(s)";
            this.toolTip.SetToolTip(this.createPoints, "Creates a feature element containing a single point. If multiple points are used," +
        " a multi-point is created.");
            this.createPoints.UseVisualStyleBackColor = true;
            this.createPoints.Click += new System.EventHandler(this.createPoints_Click);
            // 
            // createLine
            // 
            this.createLine.Location = new System.Drawing.Point(286, 59);
            this.createLine.Name = "createLine";
            this.createLine.Size = new System.Drawing.Size(121, 23);
            this.createLine.TabIndex = 1;
            this.createLine.Text = "Create line";
            this.toolTip.SetToolTip(this.createLine, "Creates a line from the points");
            this.createLine.UseVisualStyleBackColor = true;
            this.createLine.Click += new System.EventHandler(this.createLine_Click);
            // 
            // createPolygon
            // 
            this.createPolygon.Location = new System.Drawing.Point(286, 117);
            this.createPolygon.Name = "createPolygon";
            this.createPolygon.Size = new System.Drawing.Size(121, 23);
            this.createPolygon.TabIndex = 2;
            this.createPolygon.Text = "Create polygon";
            this.toolTip.SetToolTip(this.createPolygon, "Creates a polygon formed by the points and a segment joining the last point to th" +
        "e first point");
            this.createPolygon.UseVisualStyleBackColor = true;
            this.createPolygon.Click += new System.EventHandler(this.createPolygon_Click);
            // 
            // elements
            // 
            this.elements.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.elements.FormattingEnabled = true;
            this.elements.HorizontalScrollbar = true;
            this.elements.Location = new System.Drawing.Point(413, 30);
            this.elements.Name = "elements";
            this.elements.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.elements.Size = new System.Drawing.Size(268, 225);
            this.elements.TabIndex = 3;
            this.toolTip.SetToolTip(this.elements, "Elements used to create a single feature");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Points:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(410, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Feature elements:";
            // 
            // removeSelectedElements
            // 
            this.removeSelectedElements.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.removeSelectedElements.Location = new System.Drawing.Point(413, 261);
            this.removeSelectedElements.Name = "removeSelectedElements";
            this.removeSelectedElements.Size = new System.Drawing.Size(154, 23);
            this.removeSelectedElements.TabIndex = 9;
            this.removeSelectedElements.Text = "Remove selected element(s)";
            this.toolTip.SetToolTip(this.removeSelectedElements, "Removed selected elements from the list");
            this.removeSelectedElements.UseVisualStyleBackColor = true;
            this.removeSelectedElements.Click += new System.EventHandler(this.removeSelectedElements_Click);
            // 
            // createFeature
            // 
            this.createFeature.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.createFeature.Location = new System.Drawing.Point(573, 261);
            this.createFeature.Name = "createFeature";
            this.createFeature.Size = new System.Drawing.Size(108, 23);
            this.createFeature.TabIndex = 10;
            this.createFeature.Text = "Create feature";
            this.toolTip.SetToolTip(this.createFeature, "Creates a feature containing all elements listed");
            this.createFeature.UseVisualStyleBackColor = true;
            this.createFeature.Click += new System.EventHandler(this.createFeature_Click);
            // 
            // points
            // 
            this.points.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.points.FormattingEnabled = true;
            this.points.HorizontalScrollbar = true;
            this.points.Location = new System.Drawing.Point(12, 30);
            this.points.Name = "points";
            this.points.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.points.Size = new System.Drawing.Size(268, 225);
            this.points.TabIndex = 12;
            this.toolTip.SetToolTip(this.points, "Points used to create a feature element");
            // 
            // clearPoints
            // 
            this.clearPoints.Location = new System.Drawing.Point(286, 146);
            this.clearPoints.Name = "clearPoints";
            this.clearPoints.Size = new System.Drawing.Size(121, 23);
            this.clearPoints.TabIndex = 13;
            this.clearPoints.Text = "Clear points";
            this.toolTip.SetToolTip(this.clearPoints, "Clears all points");
            this.clearPoints.UseVisualStyleBackColor = true;
            this.clearPoints.Click += new System.EventHandler(this.clearPoints_Click);
            // 
            // createConnectedLine
            // 
            this.createConnectedLine.Location = new System.Drawing.Point(286, 88);
            this.createConnectedLine.Name = "createConnectedLine";
            this.createConnectedLine.Size = new System.Drawing.Size(121, 23);
            this.createConnectedLine.TabIndex = 14;
            this.createConnectedLine.Text = "Create connected line";
            this.toolTip.SetToolTip(this.createConnectedLine, "Creates a connected line formed by the points and a segment joining the last poin" +
        "t to the first point");
            this.createConnectedLine.UseVisualStyleBackColor = true;
            this.createConnectedLine.Click += new System.EventHandler(this.createConnectedLine_Click);
            // 
            // CustomSpatialFeatureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 296);
            this.Controls.Add(this.createConnectedLine);
            this.Controls.Add(this.clearPoints);
            this.Controls.Add(this.points);
            this.Controls.Add(this.createFeature);
            this.Controls.Add(this.removeSelectedElements);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.elements);
            this.Controls.Add(this.createPolygon);
            this.Controls.Add(this.createLine);
            this.Controls.Add(this.createPoints);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "CustomSpatialFeatureForm";
            this.Text = "Define spatial feature...";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button createPoints;
        private System.Windows.Forms.Button createLine;
        private System.Windows.Forms.Button createPolygon;
        private System.Windows.Forms.ListBox elements;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button removeSelectedElements;
        private System.Windows.Forms.Button createFeature;
        public System.Windows.Forms.ListBox points;
        private System.Windows.Forms.Button clearPoints;
        private System.Windows.Forms.Button createConnectedLine;
        private System.Windows.Forms.ToolTip toolTip;
    }
}