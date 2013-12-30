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
            this.normalize = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // normalize
            // 
            this.normalize.AutoSize = true;
            this.normalize.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.normalize.Location = new System.Drawing.Point(3, 3);
            this.normalize.Name = "normalize";
            this.normalize.Size = new System.Drawing.Size(75, 17);
            this.normalize.TabIndex = 0;
            this.normalize.Text = "Normalize:";
            this.normalize.UseVisualStyleBackColor = true;
            // 
            // KernelDensityDcmOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.normalize);
            this.Name = "KernelDensityDcmOptions";
            this.Size = new System.Drawing.Size(207, 151);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox normalize;
    }
}
