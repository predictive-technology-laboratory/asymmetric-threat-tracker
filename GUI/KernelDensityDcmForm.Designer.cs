namespace PTL.ATT.GUI
{
    partial class KernelDensityDcmForm
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
            this.cancel = new System.Windows.Forms.Button();
            this.ok = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.modelName = new System.Windows.Forms.TextBox();
            this.pointSpacing = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.predictionSampleSize = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.trainingSampleSize = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.normalize = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.smoothers = new PTL.ATT.GUI.SmootherList();
            ((System.ComponentModel.ISupportInitialize)(this.pointSpacing)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.predictionSampleSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingSampleSize)).BeginInit();
            this.SuspendLayout();
            // 
            // cancel
            // 
            this.cancel.Location = new System.Drawing.Point(165, 250);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 6;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // ok
            // 
            this.ok.Location = new System.Drawing.Point(84, 250);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 23);
            this.ok.TabIndex = 5;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(79, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Name:";
            // 
            // modelName
            // 
            this.modelName.Location = new System.Drawing.Point(123, 21);
            this.modelName.Name = "modelName";
            this.modelName.Size = new System.Drawing.Size(189, 20);
            this.modelName.TabIndex = 0;
            // 
            // pointSpacing
            // 
            this.pointSpacing.Location = new System.Drawing.Point(123, 47);
            this.pointSpacing.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.pointSpacing.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.pointSpacing.Name = "pointSpacing";
            this.pointSpacing.Size = new System.Drawing.Size(77, 20);
            this.pointSpacing.TabIndex = 1;
            this.pointSpacing.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Point spacing:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(206, 49);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(38, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "meters";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(206, 101);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(35, 13);
            this.label8.TabIndex = 31;
            this.label8.Text = "points";
            // 
            // predictionSampleSize
            // 
            this.predictionSampleSize.Location = new System.Drawing.Point(123, 99);
            this.predictionSampleSize.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.predictionSampleSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.predictionSampleSize.Name = "predictionSampleSize";
            this.predictionSampleSize.Size = new System.Drawing.Size(77, 20);
            this.predictionSampleSize.TabIndex = 3;
            this.predictionSampleSize.Value = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 101);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(114, 13);
            this.label10.TabIndex = 30;
            this.label10.Text = "Prediction sample size:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(206, 75);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 13);
            this.label11.TabIndex = 28;
            this.label11.Text = "points";
            // 
            // trainingSampleSize
            // 
            this.trainingSampleSize.Location = new System.Drawing.Point(123, 73);
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
            this.trainingSampleSize.TabIndex = 2;
            this.trainingSampleSize.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(12, 75);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(105, 13);
            this.label12.TabIndex = 27;
            this.label12.Text = "Training sample size:";
            // 
            // normalize
            // 
            this.normalize.AutoSize = true;
            this.normalize.Location = new System.Drawing.Point(62, 125);
            this.normalize.Name = "normalize";
            this.normalize.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.normalize.Size = new System.Drawing.Size(75, 17);
            this.normalize.TabIndex = 4;
            this.normalize.Text = "Normalize:";
            this.normalize.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(57, 148);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(60, 13);
            this.label13.TabIndex = 33;
            this.label13.Text = "Smoothers:";
            // 
            // smoothers
            // 
            this.smoothers.FormattingEnabled = true;
            this.smoothers.Location = new System.Drawing.Point(123, 148);
            this.smoothers.Name = "smoothers";
            this.smoothers.Size = new System.Drawing.Size(189, 95);
            this.smoothers.TabIndex = 34;
            // 
            // KernelDensityDcmForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 283);
            this.Controls.Add(this.smoothers);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.normalize);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.predictionSampleSize);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.trainingSampleSize);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.pointSpacing);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.modelName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "KernelDensityDcmForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select kernel density DCM options...";
            ((System.ComponentModel.ISupportInitialize)(this.pointSpacing)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.predictionSampleSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingSampleSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox modelName;
        private System.Windows.Forms.NumericUpDown pointSpacing;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown predictionSampleSize;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown trainingSampleSize;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox normalize;
        private System.Windows.Forms.Label label13;
        private SmootherList smoothers;
    }
}