namespace SiMay.RemoteMonitor.MainApplication
{
    partial class BuilderServiceForm
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
            this.label7 = new System.Windows.Forms.Label();
            this.mutex = new System.Windows.Forms.CheckBox();
            this.ishide = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupNameBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtAccesskey = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.mls_port = new System.Windows.Forms.ComboBox();
            this.mls_address = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtInitName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.logList = new System.Windows.Forms.ListBox();
            this.button3 = new System.Windows.Forms.Button();
            this.sessionModeList = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.installMode = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.Color.Silver;
            this.label7.Location = new System.Drawing.Point(13, 457);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(275, 12);
            this.label7.TabIndex = 29;
            this.label7.Text = "-本程序严禁使用于非法目的，否则一切后果自负。";
            // 
            // mutex
            // 
            this.mutex.AutoSize = true;
            this.mutex.Location = new System.Drawing.Point(99, 21);
            this.mutex.Name = "mutex";
            this.mutex.Size = new System.Drawing.Size(72, 16);
            this.mutex.TabIndex = 11;
            this.mutex.Text = "进程互斥";
            this.mutex.UseVisualStyleBackColor = true;
            // 
            // ishide
            // 
            this.ishide.AutoSize = true;
            this.ishide.Location = new System.Drawing.Point(12, 21);
            this.ishide.Name = "ishide";
            this.ishide.Size = new System.Drawing.Size(72, 16);
            this.ishide.TabIndex = 10;
            this.ishide.Text = "隐藏文件";
            this.ishide.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.groupNameBox);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.txtAccesskey);
            this.groupBox3.Controls.Add(this.button4);
            this.groupBox3.Controls.Add(this.button2);
            this.groupBox3.Controls.Add(this.mls_port);
            this.groupBox3.Controls.Add(this.mls_address);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.txtInitName);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Location = new System.Drawing.Point(11, 15);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(402, 152);
            this.groupBox3.TabIndex = 27;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "配置信息";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 115);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 12);
            this.label5.TabIndex = 15;
            this.label5.Text = "分组:";
            // 
            // groupNameBox
            // 
            this.groupNameBox.Location = new System.Drawing.Point(51, 112);
            this.groupNameBox.MaxLength = 20;
            this.groupNameBox.Name = "groupNameBox";
            this.groupNameBox.Size = new System.Drawing.Size(175, 21);
            this.groupNameBox.TabIndex = 16;
            this.groupNameBox.Text = "默认分组";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(232, 54);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(167, 12);
            this.label4.TabIndex = 14;
            this.label4.Text = "创建服务端前请连接测试一下!";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 12);
            this.label3.TabIndex = 12;
            this.label3.Text = "密码:";
            // 
            // txtAccesskey
            // 
            this.txtAccesskey.Location = new System.Drawing.Point(51, 81);
            this.txtAccesskey.MaxLength = 20;
            this.txtAccesskey.Name = "txtAccesskey";
            this.txtAccesskey.Size = new System.Drawing.Size(175, 21);
            this.txtAccesskey.TabIndex = 13;
            this.txtAccesskey.Text = "5200";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(248, 79);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(71, 22);
            this.button4.TabIndex = 11;
            this.button4.Text = "保存记录";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(325, 79);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(71, 22);
            this.button2.TabIndex = 10;
            this.button2.Text = "删除记录";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // mls_port
            // 
            this.mls_port.FormattingEnabled = true;
            this.mls_port.Location = new System.Drawing.Point(262, 23);
            this.mls_port.Name = "mls_port";
            this.mls_port.Size = new System.Drawing.Size(57, 20);
            this.mls_port.TabIndex = 9;
            // 
            // mls_address
            // 
            this.mls_address.FormattingEnabled = true;
            this.mls_address.Location = new System.Drawing.Point(51, 23);
            this.mls_address.Name = "mls_address";
            this.mls_address.Size = new System.Drawing.Size(175, 20);
            this.mls_address.TabIndex = 8;
            this.mls_address.SelectedIndexChanged += new System.EventHandler(this.mls_address_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 12);
            this.label6.TabIndex = 6;
            this.label6.Text = "备注:";
            // 
            // txtInitName
            // 
            this.txtInitName.Location = new System.Drawing.Point(51, 51);
            this.txtInitName.MaxLength = 20;
            this.txtInitName.Name = "txtInitName";
            this.txtInitName.Size = new System.Drawing.Size(175, 21);
            this.txtInitName.TabIndex = 7;
            this.txtInitName.Text = "SiMay远程管理";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "地址:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(230, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "端口:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(325, 23);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(71, 22);
            this.button1.TabIndex = 4;
            this.button1.Text = "连接测试";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.logList);
            this.groupBox2.Location = new System.Drawing.Point(11, 173);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(402, 152);
            this.groupBox2.TabIndex = 26;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "配置日志";
            // 
            // logList
            // 
            this.logList.BackColor = System.Drawing.Color.Black;
            this.logList.ForeColor = System.Drawing.Color.Lime;
            this.logList.FormattingEnabled = true;
            this.logList.ItemHeight = 12;
            this.logList.Location = new System.Drawing.Point(12, 20);
            this.logList.Name = "logList";
            this.logList.Size = new System.Drawing.Size(382, 124);
            this.logList.TabIndex = 9;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(312, 433);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(101, 39);
            this.button3.TabIndex = 25;
            this.button3.Text = "创建服务端文件";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // sessionModeList
            // 
            this.sessionModeList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sessionModeList.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.sessionModeList.FormattingEnabled = true;
            this.sessionModeList.Items.AddRange(new object[] {
            "本地服务器",
            "中间会话服务"});
            this.sessionModeList.Location = new System.Drawing.Point(75, 433);
            this.sessionModeList.Name = "sessionModeList";
            this.sessionModeList.Size = new System.Drawing.Size(104, 20);
            this.sessionModeList.TabIndex = 31;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(13, 435);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(59, 12);
            this.label8.TabIndex = 30;
            this.label8.Text = "会话模式:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.mutex);
            this.groupBox1.Controls.Add(this.ishide);
            this.groupBox1.Location = new System.Drawing.Point(11, 331);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(402, 52);
            this.groupBox1.TabIndex = 32;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "功能选项";
            // 
            // installMode
            // 
            this.installMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.installMode.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.installMode.FormattingEnabled = true;
            this.installMode.Items.AddRange(new object[] {
            "仅执行",
            "注册启动",
            "服务安装"});
            this.installMode.Location = new System.Drawing.Point(75, 410);
            this.installMode.Name = "installMode";
            this.installMode.Size = new System.Drawing.Size(104, 20);
            this.installMode.TabIndex = 34;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(13, 412);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(59, 12);
            this.label9.TabIndex = 33;
            this.label9.Text = "安装方式:";
            // 
            // BuilderServiceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(425, 489);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.installMode);
            this.Controls.Add(this.sessionModeList);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.button3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "BuilderServiceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "创建服务端";
            this.Load += new System.EventHandler(this.BuildClientForm_Load);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox ishide;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtInitName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox logList;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ComboBox mls_port;
        private System.Windows.Forms.ComboBox mls_address;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtAccesskey;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox mutex;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox groupNameBox;
        private System.Windows.Forms.ComboBox sessionModeList;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox installMode;
        private System.Windows.Forms.Label label9;
    }
}