namespace SiMay.RemoteMonitor.MainApplication
{
    partial class RemoteUpdateServiceForm
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
            this.radioURL = new System.Windows.Forms.RadioButton();
            this.radioLocalFile = new System.Windows.Forms.RadioButton();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.m_caption = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.updateList = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioURL
            // 
            this.radioURL.AutoSize = true;
            this.radioURL.Location = new System.Drawing.Point(20, 73);
            this.radioURL.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.radioURL.Name = "radioURL";
            this.radioURL.Size = new System.Drawing.Size(90, 19);
            this.radioURL.TabIndex = 8;
            this.radioURL.Text = "URL更新:";
            this.radioURL.UseVisualStyleBackColor = true;
            // 
            // radioLocalFile
            // 
            this.radioLocalFile.AutoSize = true;
            this.radioLocalFile.Checked = true;
            this.radioLocalFile.Location = new System.Drawing.Point(20, 27);
            this.radioLocalFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.radioLocalFile.Name = "radioLocalFile";
            this.radioLocalFile.Size = new System.Drawing.Size(88, 19);
            this.radioLocalFile.TabIndex = 6;
            this.radioLocalFile.TabStop = true;
            this.radioLocalFile.Text = "文件更新";
            this.radioLocalFile.UseVisualStyleBackColor = true;
            // 
            // txtURL
            // 
            this.txtURL.Location = new System.Drawing.Point(116, 70);
            this.txtURL.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(543, 25);
            this.txtURL.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(559, 23);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(100, 29);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "浏览";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(116, 25);
            this.txtPath.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(435, 25);
            this.txtPath.TabIndex = 1;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(479, 111);
            this.btnUpdate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(184, 29);
            this.btnUpdate.TabIndex = 11;
            this.btnUpdate.Text = "确定更新";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.BtnUpdate_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.m_caption);
            this.panel1.Location = new System.Drawing.Point(22, 140);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(439, 36);
            this.panel1.TabIndex = 12;
            // 
            // m_caption
            // 
            this.m_caption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_caption.Location = new System.Drawing.Point(0, 0);
            this.m_caption.Name = "m_caption";
            this.m_caption.Size = new System.Drawing.Size(439, 36);
            this.m_caption.TabIndex = 0;
            this.m_caption.Text = "这是一个危险的操作，请确保在您的新客户端使用相同的设置或者URL路径正确后再进行!";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(479, 148);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(184, 29);
            this.button1.TabIndex = 13;
            this.button1.Text = "取消";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // updateList
            // 
            this.updateList.Location = new System.Drawing.Point(27, 187);
            this.updateList.Name = "updateList";
            this.updateList.Size = new System.Drawing.Size(636, 292);
            this.updateList.TabIndex = 14;
            // 
            // RemoteUpdateServiceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(687, 495);
            this.Controls.Add(this.txtURL);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.updateList);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.radioURL);
            this.Controls.Add(this.radioLocalFile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RemoteUpdateServiceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.RemoteUpdateServiceForm_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioURL;
        private System.Windows.Forms.RadioButton radioLocalFile;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label m_caption;
        private System.Windows.Forms.Button button1;
        public System.Windows.Forms.Panel updateList;
    }
}