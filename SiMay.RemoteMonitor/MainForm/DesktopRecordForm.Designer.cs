namespace SiMay.RemoteMonitor.MainForm
{
    partial class DesktopRecordForm
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
            this.spantimeBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.startbtn = new System.Windows.Forms.Button();
            this.stopbtn = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.screenHeightBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.screenWidthBox = new System.Windows.Forms.TextBox();
            this.tip_label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // spantimeBox
            // 
            this.spantimeBox.Location = new System.Drawing.Point(253, 41);
            this.spantimeBox.Name = "spantimeBox";
            this.spantimeBox.Size = new System.Drawing.Size(100, 21);
            this.spantimeBox.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(194, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "记录间隔:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(359, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "ms";
            // 
            // startbtn
            // 
            this.startbtn.Location = new System.Drawing.Point(287, 72);
            this.startbtn.Name = "startbtn";
            this.startbtn.Size = new System.Drawing.Size(89, 23);
            this.startbtn.TabIndex = 11;
            this.startbtn.Text = "开始记录";
            this.startbtn.UseVisualStyleBackColor = true;
            this.startbtn.Click += new System.EventHandler(this.startbtn_Click);
            // 
            // stopbtn
            // 
            this.stopbtn.Location = new System.Drawing.Point(196, 72);
            this.stopbtn.Name = "stopbtn";
            this.stopbtn.Size = new System.Drawing.Size(89, 23);
            this.stopbtn.TabIndex = 12;
            this.stopbtn.Text = "停止";
            this.stopbtn.UseVisualStyleBackColor = true;
            this.stopbtn.Click += new System.EventHandler(this.stopbtn_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 14;
            this.label3.Text = "屏幕高:";
            // 
            // screenHeightBox
            // 
            this.screenHeightBox.Location = new System.Drawing.Point(64, 41);
            this.screenHeightBox.Name = "screenHeightBox";
            this.screenHeightBox.Size = new System.Drawing.Size(100, 21);
            this.screenHeightBox.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 77);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 12);
            this.label4.TabIndex = 16;
            this.label4.Text = "屏幕宽:";
            // 
            // screenWidthBox
            // 
            this.screenWidthBox.Location = new System.Drawing.Point(64, 74);
            this.screenWidthBox.Name = "screenWidthBox";
            this.screenWidthBox.Size = new System.Drawing.Size(100, 21);
            this.screenWidthBox.TabIndex = 15;
            // 
            // tip_label
            // 
            this.tip_label.AutoSize = true;
            this.tip_label.ForeColor = System.Drawing.Color.Red;
            this.tip_label.Location = new System.Drawing.Point(17, 16);
            this.tip_label.Name = "tip_label";
            this.tip_label.Size = new System.Drawing.Size(53, 12);
            this.tip_label.TabIndex = 17;
            this.tip_label.Text = "00000000";
            // 
            // DesktopRecordDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 106);
            this.Controls.Add(this.tip_label);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.screenWidthBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.screenHeightBox);
            this.Controls.Add(this.stopbtn);
            this.Controls.Add(this.startbtn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.spantimeBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "DesktopRecordDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "桌面记录";
            this.Load += new System.EventHandler(this.DesktopRecordManager_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox spantimeBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button startbtn;
        private System.Windows.Forms.Button stopbtn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox screenHeightBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox screenWidthBox;
        private System.Windows.Forms.Label tip_label;
    }
}