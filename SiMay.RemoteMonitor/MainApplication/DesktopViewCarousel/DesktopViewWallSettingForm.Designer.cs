namespace SiMay.RemoteMonitor.MainApplication
{
    partial class DesktopViewWallSettingForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.deskrefreshTimeInterval = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.enabled = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.carouselInterval = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.deskrefreshTimeInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.carouselInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(112, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 12;
            this.label2.Text = "刷新间隔:";
            // 
            // deskrefreshTimeInterval
            // 
            this.deskrefreshTimeInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.deskrefreshTimeInterval.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.deskrefreshTimeInterval.Location = new System.Drawing.Point(172, 38);
            this.deskrefreshTimeInterval.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.deskrefreshTimeInterval.Minimum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.deskrefreshTimeInterval.Name = "deskrefreshTimeInterval";
            this.deskrefreshTimeInterval.ReadOnly = true;
            this.deskrefreshTimeInterval.Size = new System.Drawing.Size(92, 21);
            this.deskrefreshTimeInterval.TabIndex = 11;
            this.deskrefreshTimeInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.deskrefreshTimeInterval.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(112, 93);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 13;
            this.label1.Text = "启用轮播:";
            // 
            // enabled
            // 
            this.enabled.AutoSize = true;
            this.enabled.Location = new System.Drawing.Point(173, 92);
            this.enabled.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.enabled.Name = "enabled";
            this.enabled.Size = new System.Drawing.Size(48, 16);
            this.enabled.TabIndex = 14;
            this.enabled.Text = "启用";
            this.enabled.UseVisualStyleBackColor = true;
            this.enabled.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(296, 116);
            this.button1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(82, 33);
            this.button1.TabIndex = 15;
            this.button1.Text = "保存";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(112, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 17;
            this.label3.Text = "轮播间隔:";
            // 
            // carouselInterval
            // 
            this.carouselInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.carouselInterval.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.carouselInterval.Location = new System.Drawing.Point(172, 65);
            this.carouselInterval.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.carouselInterval.Minimum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.carouselInterval.Name = "carouselInterval";
            this.carouselInterval.ReadOnly = true;
            this.carouselInterval.Size = new System.Drawing.Size(92, 21);
            this.carouselInterval.TabIndex = 16;
            this.carouselInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.carouselInterval.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // DesktopViewWallSettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 158);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.carouselInterval);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.enabled);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.deskrefreshTimeInterval);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.Name = "DesktopViewWallSettingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "视图墙设置";
            this.Load += new System.EventHandler(this.DesktopViewWallSettingForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.deskrefreshTimeInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.carouselInterval)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown deskrefreshTimeInterval;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox enabled;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown carouselInterval;
    }
}