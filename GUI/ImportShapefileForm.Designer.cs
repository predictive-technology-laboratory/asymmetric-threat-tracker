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
    partial class ImportShapefileForm
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
            this.shapefileDir = new System.Windows.Forms.Button();
            this.shapefilePath = new System.Windows.Forms.TextBox();
            this.shapefileFile = new System.Windows.Forms.Button();
            this.shapefileType = new System.Windows.Forms.ComboBox();
            this.importShp = new System.Windows.Forms.Button();
            this.close = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Shapefile path/directory:";
            // 
            // shapefileDir
            // 
            this.shapefileDir.Location = new System.Drawing.Point(517, 21);
            this.shapefileDir.Name = "shapefileDir";
            this.shapefileDir.Size = new System.Drawing.Size(75, 23);
            this.shapefileDir.TabIndex = 2;
            this.shapefileDir.Text = "Directory...";
            this.shapefileDir.UseVisualStyleBackColor = true;
            this.shapefileDir.Click += new System.EventHandler(this.shapefileDir_Click);
            // 
            // shapefilePath
            // 
            this.shapefilePath.Location = new System.Drawing.Point(141, 23);
            this.shapefilePath.Name = "shapefilePath";
            this.shapefilePath.Size = new System.Drawing.Size(289, 20);
            this.shapefilePath.TabIndex = 0;
            // 
            // shapefileFile
            // 
            this.shapefileFile.Location = new System.Drawing.Point(436, 21);
            this.shapefileFile.Name = "shapefileFile";
            this.shapefileFile.Size = new System.Drawing.Size(75, 23);
            this.shapefileFile.TabIndex = 1;
            this.shapefileFile.Text = "File...";
            this.shapefileFile.UseVisualStyleBackColor = true;
            this.shapefileFile.Click += new System.EventHandler(this.shapefileFile_Click);
            // 
            // shapefileType
            // 
            this.shapefileType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.shapefileType.FormattingEnabled = true;
            this.shapefileType.Location = new System.Drawing.Point(141, 49);
            this.shapefileType.Name = "shapefileType";
            this.shapefileType.Size = new System.Drawing.Size(289, 21);
            this.shapefileType.TabIndex = 5;
            // 
            // importShp
            // 
            this.importShp.Location = new System.Drawing.Point(224, 87);
            this.importShp.Name = "importShp";
            this.importShp.Size = new System.Drawing.Size(75, 23);
            this.importShp.TabIndex = 6;
            this.importShp.Text = "Import";
            this.importShp.UseVisualStyleBackColor = true;
            this.importShp.Click += new System.EventHandler(this.importShp_Click);
            // 
            // close
            // 
            this.close.Location = new System.Drawing.Point(305, 87);
            this.close.Name = "close";
            this.close.Size = new System.Drawing.Size(75, 23);
            this.close.TabIndex = 7;
            this.close.Text = "Close";
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.close_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(58, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "Shapefile type:";
            // 
            // ImportShapeFileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(605, 121);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.close);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.shapefileDir);
            this.Controls.Add(this.shapefilePath);
            this.Controls.Add(this.shapefileFile);
            this.Controls.Add(this.shapefileType);
            this.Controls.Add(this.importShp);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ImportShapeFileForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import shape file(s)...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button shapefileDir;
        private System.Windows.Forms.TextBox shapefilePath;
        private System.Windows.Forms.Button shapefileFile;
        private System.Windows.Forms.ComboBox shapefileType;
        private System.Windows.Forms.Button importShp;
        private System.Windows.Forms.Button close;
        private System.Windows.Forms.Label label2;

    }
}