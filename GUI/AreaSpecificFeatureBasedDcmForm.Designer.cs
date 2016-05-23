namespace PTL.ATT.GUI
{
    partial class AreaSpecificFeatureBasedDcmForm
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
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.featureBasedDcmOptions = new PTL.ATT.GUI.FeatureBasedDcmOptions();
            this.discreteChoiceModelOptions = new PTL.ATT.GUI.DiscreteChoiceModelOptions();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.areaSpecificFeatureBasedDcmOptions = new PTL.ATT.GUI.AreaSpecificFeatureBasedDcmOptions();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.oKToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cancelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.featureBasedDcmOptions);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage2.Size = new System.Drawing.Size(705, 530);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Feature-based DCM Options";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // featureBasedDcmOptions
            // 
            this.featureBasedDcmOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.featureBasedDcmOptions.FeatureBasedDCM = null;
            this.featureBasedDcmOptions.Location = new System.Drawing.Point(4, 4);
            this.featureBasedDcmOptions.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.featureBasedDcmOptions.MinimumSize = new System.Drawing.Size(408, 473);
            this.featureBasedDcmOptions.Name = "featureBasedDcmOptions";
            this.featureBasedDcmOptions.Size = new System.Drawing.Size(697, 522);
            this.featureBasedDcmOptions.TabIndex = 0;
            // 
            // discreteChoiceModelOptions
            // 
            this.discreteChoiceModelOptions.DiscreteChoiceModel = null;
            this.discreteChoiceModelOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.discreteChoiceModelOptions.Location = new System.Drawing.Point(4, 4);
            this.discreteChoiceModelOptions.Margin = new System.Windows.Forms.Padding(5);
            this.discreteChoiceModelOptions.MinimumSize = new System.Drawing.Size(489, 478);
            this.discreteChoiceModelOptions.Name = "discreteChoiceModelOptions";
            this.discreteChoiceModelOptions.Size = new System.Drawing.Size(697, 522);
            this.discreteChoiceModelOptions.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 28);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(713, 559);
            this.tabControl1.TabIndex = 15;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.discreteChoiceModelOptions);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(705, 530);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Discrete Choice DCM Options";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.areaSpecificFeatureBasedDcmOptions);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage3.Size = new System.Drawing.Size(705, 530);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Area-Specific DCM Options";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // areaSpecificFeatureBasedDcmOptions
            // 
            this.areaSpecificFeatureBasedDcmOptions.AreaSpecificFeatureBasedDCM = null;
            this.areaSpecificFeatureBasedDcmOptions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.areaSpecificFeatureBasedDcmOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.areaSpecificFeatureBasedDcmOptions.Location = new System.Drawing.Point(4, 4);
            this.areaSpecificFeatureBasedDcmOptions.Name = "areaSpecificFeatureBasedDcmOptions";
            this.areaSpecificFeatureBasedDcmOptions.Size = new System.Drawing.Size(697, 522);
            this.areaSpecificFeatureBasedDcmOptions.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.oKToolStripMenuItem,
            this.cancelToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(713, 28);
            this.menuStrip1.TabIndex = 16;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // oKToolStripMenuItem
            // 
            this.oKToolStripMenuItem.Name = "oKToolStripMenuItem";
            this.oKToolStripMenuItem.Size = new System.Drawing.Size(41, 24);
            this.oKToolStripMenuItem.Text = "OK";
            this.oKToolStripMenuItem.Click += new System.EventHandler(this.oKToolStripMenuItem_Click);
            // 
            // cancelToolStripMenuItem
            // 
            this.cancelToolStripMenuItem.Name = "cancelToolStripMenuItem";
            this.cancelToolStripMenuItem.Size = new System.Drawing.Size(65, 24);
            this.cancelToolStripMenuItem.Text = "Cancel";
            this.cancelToolStripMenuItem.Click += new System.EventHandler(this.cancelToolStripMenuItem_Click);
            // 
            // AreaSpecificFeatureBasedDcmForm
            // 
           
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(713, 587);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "AreaSpecificFeatureBasedDcmForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Area-Specific Feature-based DCM...";
            this.tabPage2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage2;
        public DiscreteChoiceModelOptions discreteChoiceModelOptions;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem oKToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cancelToolStripMenuItem;
        private AreaSpecificFeatureBasedDcmOptions areaSpecificFeatureBasedDcmOptions;
        private FeatureBasedDcmOptions featureBasedDcmOptions;
    }
}