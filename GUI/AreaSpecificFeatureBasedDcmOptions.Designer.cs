namespace PTL.ATT.GUI
{
    partial class AreaSpecificFeatureBasedDcmOptions
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
            this.label1 = new System.Windows.Forms.Label();
            this._CheckedListBoxZipCodes = new System.Windows.Forms.CheckedListBox();
            this._btnCheckall = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this._ButtonLoadZipcodes = new System.Windows.Forms.Button();
            this._ComboBoxZipcodeShapeFiles = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 41);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 17);
            this.label1.TabIndex = 20;
            this.label1.Text = "Zip Codes:";
            // 
            // _CheckedListBoxZipCodes
            // 
            this._CheckedListBoxZipCodes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._CheckedListBoxZipCodes.FormattingEnabled = true;
            this._CheckedListBoxZipCodes.Location = new System.Drawing.Point(68, 41);
            this._CheckedListBoxZipCodes.MultiColumn = true;
            this._CheckedListBoxZipCodes.Name = "_CheckedListBoxZipCodes";
            this._CheckedListBoxZipCodes.Size = new System.Drawing.Size(236, 137);
            this._CheckedListBoxZipCodes.TabIndex = 21;
            this._CheckedListBoxZipCodes.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this._CheckedListBoxZipCodes_ItemCheck);
            // 
            // _btnCheckall
            // 
            this._btnCheckall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._btnCheckall.Location = new System.Drawing.Point(177, 195);
            this._btnCheckall.Name = "_btnCheckall";
            this._btnCheckall.Size = new System.Drawing.Size(127, 28);
            this._btnCheckall.TabIndex = 22;
            this._btnCheckall.Text = "check/uncheck all";
            this._btnCheckall.UseVisualStyleBackColor = true;
            this._btnCheckall.Click += new System.EventHandler(this._btnCheckall_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-3, 14);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(127, 17);
            this.label2.TabIndex = 20;
            this.label2.Text = "Zip Code ShapeFile:";
            // 
            // _ButtonLoadZipcodes
            // 
            this._ButtonLoadZipcodes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._ButtonLoadZipcodes.Location = new System.Drawing.Point(252, 10);
            this._ButtonLoadZipcodes.Name = "_ButtonLoadZipcodes";
            this._ButtonLoadZipcodes.Size = new System.Drawing.Size(52, 28);
            this._ButtonLoadZipcodes.TabIndex = 22;
            this._ButtonLoadZipcodes.Text = "Load";
            this._ButtonLoadZipcodes.UseVisualStyleBackColor = true;
            this._ButtonLoadZipcodes.Click += new System.EventHandler(this._ButtonLoadZipcodes_Click);
            // 
            // _ComboBoxZipcodeShapeFiles
            // 
            this._ComboBoxZipcodeShapeFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._ComboBoxZipcodeShapeFiles.FormattingEnabled = true;
            this._ComboBoxZipcodeShapeFiles.Location = new System.Drawing.Point(121, 11);
            this._ComboBoxZipcodeShapeFiles.Name = "_ComboBoxZipcodeShapeFiles";
            this._ComboBoxZipcodeShapeFiles.Size = new System.Drawing.Size(121, 24);
            this._ComboBoxZipcodeShapeFiles.TabIndex = 24;
            this._ComboBoxZipcodeShapeFiles.SelectedIndexChanged += new System.EventHandler(this._ComboBoxZipcodeShapeFiles_SelectedIndexChanged);
            // 
            // AreaSpecificFeatureBasedDcmOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._ComboBoxZipcodeShapeFiles);
            this.Controls.Add(this._ButtonLoadZipcodes);
            this.Controls.Add(this._btnCheckall);
            this.Controls.Add(this._CheckedListBoxZipCodes);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "AreaSpecificFeatureBasedDcmOptions";
            this.Size = new System.Drawing.Size(307, 231);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox _CheckedListBoxZipCodes;
        private System.Windows.Forms.Button _btnCheckall;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button _ButtonLoadZipcodes;
        private System.Windows.Forms.ComboBox _ComboBoxZipcodeShapeFiles;
    }
}
