namespace SiMay.RemoteMonitor.Controls
{
    partial class AudioConfigurationForm
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
            this.nSamplesPerSec = new System.Windows.Forms.ComboBox();
            this.wBitsPerSample = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.nChannels = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "采样频率:";
            // 
            // nSamplesPerSec
            // 
            this.nSamplesPerSec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nSamplesPerSec.FormattingEnabled = true;
            this.nSamplesPerSec.Items.AddRange(new object[] {
            "8000",
            "11025",
            "22050",
            "44100"});
            this.nSamplesPerSec.Location = new System.Drawing.Point(77, 35);
            this.nSamplesPerSec.Name = "nSamplesPerSec";
            this.nSamplesPerSec.Size = new System.Drawing.Size(89, 20);
            this.nSamplesPerSec.TabIndex = 1;
            // 
            // wBitsPerSample
            // 
            this.wBitsPerSample.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.wBitsPerSample.FormattingEnabled = true;
            this.wBitsPerSample.Items.AddRange(new object[] {
            "8",
            "16",
            "32"});
            this.wBitsPerSample.Location = new System.Drawing.Point(237, 35);
            this.wBitsPerSample.Name = "wBitsPerSample";
            this.wBitsPerSample.Size = new System.Drawing.Size(89, 20);
            this.wBitsPerSample.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(172, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "采样位数:";
            // 
            // nChannels
            // 
            this.nChannels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.nChannels.FormattingEnabled = true;
            this.nChannels.Items.AddRange(new object[] {
            "1",
            "2"});
            this.nChannels.Location = new System.Drawing.Point(397, 35);
            this.nChannels.Name = "nChannels";
            this.nChannels.Size = new System.Drawing.Size(89, 20);
            this.nChannels.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(332, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "声道数量:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(399, 76);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(89, 31);
            this.button1.TabIndex = 6;
            this.button1.Text = "保存配置";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // AudioConfigurationManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 120);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.nChannels);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.wBitsPerSample);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.nSamplesPerSec);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "AudioConfigurationManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "语音设置";
            this.Load += new System.EventHandler(this.AudioConfigurationManager_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox nSamplesPerSec;
        private System.Windows.Forms.ComboBox wBitsPerSample;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox nChannels;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
    }
}