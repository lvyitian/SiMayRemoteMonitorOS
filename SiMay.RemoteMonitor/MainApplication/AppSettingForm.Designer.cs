namespace SiMay.RemoteMonitor.MainApplication
{
    partial class AppSettingForm
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
            this.save = new System.Windows.Forms.Button();
            this.connectNum = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.port = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ip = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.conPwd = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.funComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.maximizeCheckBox = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label13 = new System.Windows.Forms.Label();
            this.txtservice_port = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtservice_address = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.accessKey = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.pwdTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.sessionModeList = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // save
            // 
            this.save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.save.Location = new System.Drawing.Point(338, 10);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(75, 26);
            this.save.TabIndex = 0;
            this.save.Text = "保存设置";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.save_Click);
            // 
            // connectNum
            // 
            this.connectNum.Location = new System.Drawing.Point(185, 135);
            this.connectNum.Name = "connectNum";
            this.connectNum.Size = new System.Drawing.Size(115, 21);
            this.connectNum.TabIndex = 13;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(107, 137);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 12);
            this.label3.TabIndex = 12;
            this.label3.Text = "最大连接数:";
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(185, 81);
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(115, 21);
            this.port.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(119, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "监听端口:";
            // 
            // ip
            // 
            this.ip.Location = new System.Drawing.Point(185, 54);
            this.ip.Name = "ip";
            this.ip.Size = new System.Drawing.Size(115, 21);
            this.ip.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(108, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "服务器地址:";
            // 
            // conPwd
            // 
            this.conPwd.Location = new System.Drawing.Point(185, 108);
            this.conPwd.MaxLength = 6;
            this.conPwd.Name = "conPwd";
            this.conPwd.Size = new System.Drawing.Size(115, 21);
            this.conPwd.TabIndex = 17;
            this.conPwd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.conPwd_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(119, 111);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 12);
            this.label5.TabIndex = 16;
            this.label5.Text = "连接密码:";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(419, 10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 26);
            this.button1.TabIndex = 18;
            this.button1.Text = "关闭";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // funComboBox
            // 
            this.funComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.funComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.funComboBox.FormattingEnabled = true;
            this.funComboBox.Location = new System.Drawing.Point(185, 161);
            this.funComboBox.Name = "funComboBox";
            this.funComboBox.Size = new System.Drawing.Size(115, 20);
            this.funComboBox.TabIndex = 26;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(95, 165);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 12);
            this.label4.TabIndex = 25;
            this.label4.Text = "双击屏幕打开:";
            // 
            // maximizeCheckBox
            // 
            this.maximizeCheckBox.AutoSize = true;
            this.maximizeCheckBox.Location = new System.Drawing.Point(185, 190);
            this.maximizeCheckBox.Name = "maximizeCheckBox";
            this.maximizeCheckBox.Size = new System.Drawing.Size(96, 16);
            this.maximizeCheckBox.TabIndex = 24;
            this.maximizeCheckBox.Text = "启动时最大化";
            this.maximizeCheckBox.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label13);
            this.panel2.Controls.Add(this.txtservice_port);
            this.panel2.Controls.Add(this.label12);
            this.panel2.Controls.Add(this.txtservice_address);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.accessKey);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Controls.Add(this.pwdTextBox);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.ip);
            this.panel2.Controls.Add(this.maximizeCheckBox);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.funComboBox);
            this.panel2.Controls.Add(this.port);
            this.panel2.Controls.Add(this.conPwd);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.connectNum);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Location = new System.Drawing.Point(32, 2);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(405, 438);
            this.panel2.TabIndex = 0;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(95, 190);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(83, 12);
            this.label13.TabIndex = 40;
            this.label13.Text = "窗口启动状态:";
            // 
            // txtservice_port
            // 
            this.txtservice_port.Location = new System.Drawing.Point(185, 282);
            this.txtservice_port.Name = "txtservice_port";
            this.txtservice_port.Size = new System.Drawing.Size(115, 21);
            this.txtservice_port.TabIndex = 39;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(120, 286);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(59, 12);
            this.label12.TabIndex = 38;
            this.label12.Text = "服务端口:";
            // 
            // txtservice_address
            // 
            this.txtservice_address.Location = new System.Drawing.Point(185, 255);
            this.txtservice_address.Name = "txtservice_address";
            this.txtservice_address.Size = new System.Drawing.Size(115, 21);
            this.txtservice_address.TabIndex = 37;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(107, 258);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(71, 12);
            this.label10.TabIndex = 36;
            this.label10.Text = "服务器地址:";
            // 
            // accessKey
            // 
            this.accessKey.Location = new System.Drawing.Point(185, 309);
            this.accessKey.Name = "accessKey";
            this.accessKey.Size = new System.Drawing.Size(115, 21);
            this.accessKey.TabIndex = 35;
            this.accessKey.UseSystemPasswordChar = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(113, 312);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(65, 12);
            this.label11.TabIndex = 34;
            this.label11.Text = "AccessKey:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.Location = new System.Drawing.Point(95, 227);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(70, 12);
            this.label9.TabIndex = 31;
            this.label9.Text = "会话服务器";
            // 
            // pwdTextBox
            // 
            this.pwdTextBox.Location = new System.Drawing.Point(185, 383);
            this.pwdTextBox.Name = "pwdTextBox";
            this.pwdTextBox.Size = new System.Drawing.Size(115, 21);
            this.pwdTextBox.TabIndex = 30;
            this.pwdTextBox.UseSystemPasswordChar = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(95, 386);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(83, 12);
            this.label8.TabIndex = 29;
            this.label8.Text = "修改解锁密码:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(95, 352);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(57, 12);
            this.label7.TabIndex = 28;
            this.label7.Text = "锁定密码";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(95, 18);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 12);
            this.label6.TabIndex = 27;
            this.label6.Text = "基本设置";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.sessionModeList);
            this.panel4.Controls.Add(this.label14);
            this.panel4.Controls.Add(this.button1);
            this.panel4.Controls.Add(this.save);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 300);
            this.panel4.Margin = new System.Windows.Forms.Padding(2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(503, 45);
            this.panel4.TabIndex = 28;
            // 
            // sessionModeList
            // 
            this.sessionModeList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sessionModeList.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.sessionModeList.FormattingEnabled = true;
            this.sessionModeList.Items.AddRange(new object[] {
            "本地服务器",
            "中间会话服务"});
            this.sessionModeList.Location = new System.Drawing.Point(63, 11);
            this.sessionModeList.Name = "sessionModeList";
            this.sessionModeList.Size = new System.Drawing.Size(121, 20);
            this.sessionModeList.TabIndex = 20;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(4, 15);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(59, 12);
            this.label14.TabIndex = 19;
            this.label14.Text = "会话模式:";
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(503, 300);
            this.panel1.TabIndex = 29;
            // 
            // AppSettingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(503, 345);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "AppSettingDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "系统设置";
            this.Load += new System.EventHandler(this.SetForm_Load);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button save;
        private System.Windows.Forms.TextBox connectNum;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox port;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ip;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox conPwd;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox funComboBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox maximizeCheckBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox pwdTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox accessKey;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox sessionModeList;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtservice_port;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtservice_address;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label13;
    }
}