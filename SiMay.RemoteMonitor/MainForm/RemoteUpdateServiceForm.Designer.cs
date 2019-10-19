namespace SiMay.RemoteMonitor.MainForm
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
            this.groupURL = new System.Windows.Forms.GroupBox();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.lblURL = new System.Windows.Forms.Label();
            this.groupLocalFile = new System.Windows.Forms.GroupBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.m_caption = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupURL.SuspendLayout();
            this.groupLocalFile.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioURL
            // 
            this.radioURL.AutoSize = true;
            this.radioURL.Location = new System.Drawing.Point(21, 97);
            this.radioURL.Name = "radioURL";
            this.radioURL.Size = new System.Drawing.Size(65, 16);
            this.radioURL.TabIndex = 8;
            this.radioURL.Text = "URL更新";
            this.radioURL.UseVisualStyleBackColor = true;
            // 
            // radioLocalFile
            // 
            this.radioLocalFile.AutoSize = true;
            this.radioLocalFile.Checked = true;
            this.radioLocalFile.Location = new System.Drawing.Point(21, 22);
            this.radioLocalFile.Name = "radioLocalFile";
            this.radioLocalFile.Size = new System.Drawing.Size(71, 16);
            this.radioLocalFile.TabIndex = 6;
            this.radioLocalFile.TabStop = true;
            this.radioLocalFile.Text = "上传更新";
            this.radioLocalFile.UseVisualStyleBackColor = true;
            // 
            // groupURL
            // 
            this.groupURL.Controls.Add(this.txtURL);
            this.groupURL.Controls.Add(this.lblURL);
            this.groupURL.Location = new System.Drawing.Point(21, 119);
            this.groupURL.Name = "groupURL";
            this.groupURL.Size = new System.Drawing.Size(479, 46);
            this.groupURL.TabIndex = 9;
            this.groupURL.TabStop = false;
            // 
            // txtURL
            // 
            this.txtURL.Location = new System.Drawing.Point(61, 18);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(396, 21);
            this.txtURL.TabIndex = 1;
            // 
            // lblURL
            // 
            this.lblURL.AutoSize = true;
            this.lblURL.Location = new System.Drawing.Point(26, 21);
            this.lblURL.Name = "lblURL";
            this.lblURL.Size = new System.Drawing.Size(29, 12);
            this.lblURL.TabIndex = 0;
            this.lblURL.Text = "URL:";
            // 
            // groupLocalFile
            // 
            this.groupLocalFile.Controls.Add(this.btnBrowse);
            this.groupLocalFile.Controls.Add(this.txtPath);
            this.groupLocalFile.Controls.Add(this.label1);
            this.groupLocalFile.Location = new System.Drawing.Point(23, 44);
            this.groupLocalFile.Name = "groupLocalFile";
            this.groupLocalFile.Size = new System.Drawing.Size(479, 43);
            this.groupLocalFile.TabIndex = 7;
            this.groupLocalFile.TabStop = false;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(380, 15);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "浏览";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(59, 15);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(315, 21);
            this.txtPath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "文件:";
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(364, 178);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(138, 23);
            this.btnUpdate.TabIndex = 11;
            this.btnUpdate.Text = "Update Client";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.BtnUpdate_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.m_caption);
            this.panel1.Location = new System.Drawing.Point(21, 201);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(329, 29);
            this.panel1.TabIndex = 12;
            // 
            // m_caption
            // 
            this.m_caption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_caption.Location = new System.Drawing.Point(0, 0);
            this.m_caption.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.m_caption.Name = "m_caption";
            this.m_caption.Size = new System.Drawing.Size(329, 29);
            this.m_caption.TabIndex = 0;
            this.m_caption.Text = "这是一个危险的操作，请确保在您的新客户端使用相同的设置或者URL路径正确后再进行!";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(364, 207);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(138, 23);
            this.button1.TabIndex = 13;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // RemoteUpdateServiceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(513, 247);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.radioURL);
            this.Controls.Add(this.radioLocalFile);
            this.Controls.Add(this.groupURL);
            this.Controls.Add(this.groupLocalFile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RemoteUpdateServiceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.groupURL.ResumeLayout(false);
            this.groupURL.PerformLayout();
            this.groupLocalFile.ResumeLayout(false);
            this.groupLocalFile.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioURL;
        private System.Windows.Forms.RadioButton radioLocalFile;
        private System.Windows.Forms.GroupBox groupURL;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.Label lblURL;
        private System.Windows.Forms.GroupBox groupLocalFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label m_caption;
        private System.Windows.Forms.Button button1;
    }
}