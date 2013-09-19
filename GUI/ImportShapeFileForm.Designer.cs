namespace PTL.ATT.GUI
{
    partial class ImportShapeFileForm
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
            this.shapeFilePath = new System.Windows.Forms.TextBox();
            this.shapefileFile = new System.Windows.Forms.Button();
            this.areaShp = new System.Windows.Forms.RadioButton();
            this.featureShp = new System.Windows.Forms.RadioButton();
            this.featureType = new System.Windows.Forms.ComboBox();
            this.importShp = new System.Windows.Forms.Button();
            this.close = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Shape file/directory:";
            // 
            // shapefileDir
            // 
            this.shapefileDir.Location = new System.Drawing.Point(494, 21);
            this.shapefileDir.Name = "shapefileDir";
            this.shapefileDir.Size = new System.Drawing.Size(75, 23);
            this.shapefileDir.TabIndex = 2;
            this.shapefileDir.Text = "Directory...";
            this.shapefileDir.UseVisualStyleBackColor = true;
            this.shapefileDir.Click += new System.EventHandler(this.shapefileDir_Click);
            // 
            // shapeFilePath
            // 
            this.shapeFilePath.Location = new System.Drawing.Point(118, 23);
            this.shapeFilePath.Name = "shapeFilePath";
            this.shapeFilePath.Size = new System.Drawing.Size(289, 20);
            this.shapeFilePath.TabIndex = 0;
            // 
            // shapefileFile
            // 
            this.shapefileFile.Location = new System.Drawing.Point(413, 21);
            this.shapefileFile.Name = "shapefileFile";
            this.shapefileFile.Size = new System.Drawing.Size(75, 23);
            this.shapefileFile.TabIndex = 1;
            this.shapefileFile.Text = "File...";
            this.shapefileFile.UseVisualStyleBackColor = true;
            this.shapefileFile.Click += new System.EventHandler(this.shapefileFile_Click);
            // 
            // areaShp
            // 
            this.areaShp.AutoSize = true;
            this.areaShp.Checked = true;
            this.areaShp.Location = new System.Drawing.Point(118, 49);
            this.areaShp.Name = "areaShp";
            this.areaShp.Size = new System.Drawing.Size(106, 17);
            this.areaShp.TabIndex = 3;
            this.areaShp.TabStop = true;
            this.areaShp.Text = "Area shape file(s)";
            this.areaShp.UseVisualStyleBackColor = true;
            // 
            // featureShp
            // 
            this.featureShp.AutoSize = true;
            this.featureShp.Location = new System.Drawing.Point(118, 72);
            this.featureShp.Name = "featureShp";
            this.featureShp.Size = new System.Drawing.Size(120, 17);
            this.featureShp.TabIndex = 4;
            this.featureShp.TabStop = true;
            this.featureShp.Text = "Feature shape file(s)";
            this.featureShp.UseVisualStyleBackColor = true;
            // 
            // featureType
            // 
            this.featureType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.featureType.FormattingEnabled = true;
            this.featureType.Location = new System.Drawing.Point(244, 71);
            this.featureType.Name = "featureType";
            this.featureType.Size = new System.Drawing.Size(121, 21);
            this.featureType.TabIndex = 5;
            // 
            // importShp
            // 
            this.importShp.Location = new System.Drawing.Point(213, 119);
            this.importShp.Name = "importShp";
            this.importShp.Size = new System.Drawing.Size(75, 23);
            this.importShp.TabIndex = 6;
            this.importShp.Text = "Import";
            this.importShp.UseVisualStyleBackColor = true;
            this.importShp.Click += new System.EventHandler(this.importShp_Click);
            // 
            // close
            // 
            this.close.Location = new System.Drawing.Point(294, 119);
            this.close.Name = "close";
            this.close.Size = new System.Drawing.Size(75, 23);
            this.close.TabIndex = 7;
            this.close.Text = "Close";
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.close_Click);
            // 
            // ImportShapeFileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(583, 155);
            this.Controls.Add(this.close);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.shapefileDir);
            this.Controls.Add(this.shapeFilePath);
            this.Controls.Add(this.shapefileFile);
            this.Controls.Add(this.areaShp);
            this.Controls.Add(this.featureShp);
            this.Controls.Add(this.featureType);
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
        private System.Windows.Forms.TextBox shapeFilePath;
        private System.Windows.Forms.Button shapefileFile;
        private System.Windows.Forms.RadioButton areaShp;
        private System.Windows.Forms.RadioButton featureShp;
        private System.Windows.Forms.ComboBox featureType;
        private System.Windows.Forms.Button importShp;
        private System.Windows.Forms.Button close;

    }
}