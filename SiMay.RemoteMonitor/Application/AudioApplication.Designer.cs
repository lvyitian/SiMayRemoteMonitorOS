namespace SiMay.RemoteMonitor.Application
{
    partial class AudioApplication
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.labRuntime = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.sendataLen = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.recvdataLen = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tip = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox2);
            this.groupBox1.Controls.Add(this.labRuntime);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.progressBar1);
            this.groupBox1.Controls.Add(this.sendataLen);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.recvdataLen);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.tip);
            this.groupBox1.Location = new System.Drawing.Point(9, 1);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(692, 178);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(209, 134);
            this.checkBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(119, 19);
            this.checkBox2.TabIndex = 11;
            this.checkBox2.Text = "录制远程声音";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // labRuntime
            // 
            this.labRuntime.AutoSize = true;
            this.labRuntime.Location = new System.Drawing.Point(405, 135);
            this.labRuntime.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labRuntime.Name = "labRuntime";
            this.labRuntime.Size = new System.Drawing.Size(148, 15);
            this.labRuntime.TabIndex = 10;
            this.labRuntime.Text = "已运行:00.00.00.00";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(564, 129);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 29);
            this.button1.TabIndex = 9;
            this.button1.Text = "设置";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(370, 69);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(294, 11);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 7;
            // 
            // sendataLen
            // 
            this.sendataLen.AutoSize = true;
            this.sendataLen.Location = new System.Drawing.Point(93, 94);
            this.sendataLen.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.sendataLen.Name = "sendataLen";
            this.sendataLen.Size = new System.Drawing.Size(39, 15);
            this.sendataLen.TabIndex = 6;
            this.sendataLen.Text = "0 KB";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 94);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "已发送:";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(25, 134);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(164, 19);
            this.checkBox1.TabIndex = 3;
            this.checkBox1.Text = "发送本地语音到远程";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // recvdataLen
            // 
            this.recvdataLen.AutoSize = true;
            this.recvdataLen.Location = new System.Drawing.Point(93, 69);
            this.recvdataLen.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.recvdataLen.Name = "recvdataLen";
            this.recvdataLen.Size = new System.Drawing.Size(39, 15);
            this.recvdataLen.TabIndex = 2;
            this.recvdataLen.Text = "0 KB";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 69);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "已接收:";
            // 
            // tip
            // 
            this.tip.AutoSize = true;
            this.tip.ForeColor = System.Drawing.Color.Red;
            this.tip.Location = new System.Drawing.Point(23, 40);
            this.tip.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.tip.Name = "tip";
            this.tip.Size = new System.Drawing.Size(175, 15);
            this.tip.TabIndex = 0;
            this.tip.Text = "正在监听远程声音......";
            // 
            // AudioApplication
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(711, 189);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "AudioApplication";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "语音监听";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AudioManager_FormClosing);
            this.Load += new System.EventHandler(this.AudioManager_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label recvdataLen;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label tip;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label sendataLen;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label labRuntime;
        private System.Windows.Forms.CheckBox checkBox2;
    }
}