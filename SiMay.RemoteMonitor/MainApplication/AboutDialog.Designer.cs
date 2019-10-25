namespace SiMay.RemoteMonitor.MainApplication
{
    partial class AboutForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.lblSubTitle = new System.Windows.Forms.Label();
            this.lnkGiteePage = new System.Windows.Forms.LinkLabel();
            this.lnkCredits = new System.Windows.Forms.LinkLabel();
            this.lblLicense = new System.Windows.Forms.Label();
            this.rtxtContent = new System.Windows.Forms.RichTextBox();
            this.btnOkay = new System.Windows.Forms.Button();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblSubTitle
            // 
            this.lblSubTitle.AutoSize = true;
            this.lblSubTitle.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubTitle.Location = new System.Drawing.Point(50, 54);
            this.lblSubTitle.Name = "lblSubTitle";
            this.lblSubTitle.Size = new System.Drawing.Size(176, 13);
            this.lblSubTitle.TabIndex = 10;
            this.lblSubTitle.Text = "开源的思美远程监控管理系统";
            // 
            // lnkGiteePage
            // 
            this.lnkGiteePage.AutoSize = true;
            this.lnkGiteePage.Location = new System.Drawing.Point(479, 78);
            this.lnkGiteePage.Name = "lnkGiteePage";
            this.lnkGiteePage.Size = new System.Drawing.Size(59, 12);
            this.lnkGiteePage.TabIndex = 12;
            this.lnkGiteePage.TabStop = true;
            this.lnkGiteePage.Text = "Gitee.com";
            this.lnkGiteePage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkGithubPage_LinkClicked);
            // 
            // lnkCredits
            // 
            this.lnkCredits.AutoSize = true;
            this.lnkCredits.Location = new System.Drawing.Point(455, 100);
            this.lnkCredits.Name = "lnkCredits";
            this.lnkCredits.Size = new System.Drawing.Size(83, 12);
            this.lnkCredits.TabIndex = 13;
            this.lnkCredits.TabStop = true;
            this.lnkCredits.Text = "AGPL licenses";
            this.lnkCredits.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LnkCredits_LinkClicked);
            // 
            // lblLicense
            // 
            this.lblLicense.AutoSize = true;
            this.lblLicense.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLicense.Location = new System.Drawing.Point(50, 111);
            this.lblLicense.Name = "lblLicense";
            this.lblLicense.Size = new System.Drawing.Size(46, 15);
            this.lblLicense.TabIndex = 14;
            this.lblLicense.Text = "License";
            // 
            // rtxtContent
            // 
            this.rtxtContent.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtxtContent.Location = new System.Drawing.Point(53, 129);
            this.rtxtContent.Name = "rtxtContent";
            this.rtxtContent.ReadOnly = true;
            this.rtxtContent.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtxtContent.Size = new System.Drawing.Size(498, 252);
            this.rtxtContent.TabIndex = 15;
            this.rtxtContent.Text = resources.GetString("rtxtContent.Text");
            // 
            // btnOkay
            // 
            this.btnOkay.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnOkay.Location = new System.Drawing.Point(476, 387);
            this.btnOkay.Name = "btnOkay";
            this.btnOkay.Size = new System.Drawing.Size(75, 23);
            this.btnOkay.TabIndex = 16;
            this.btnOkay.Text = "&Okay";
            this.btnOkay.UseVisualStyleBackColor = true;
            // 
            // lblVersion
            // 
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.Location = new System.Drawing.Point(476, 58);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(75, 13);
            this.lblVersion.TabIndex = 11;
            this.lblVersion.Text = "%VERSION%";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(48, 24);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(241, 30);
            this.lblTitle.TabIndex = 8;
            this.lblTitle.Text = "SiMayRemoteMonitorOS";
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 425);
            this.Controls.Add(this.lblSubTitle);
            this.Controls.Add(this.lnkGiteePage);
            this.Controls.Add(this.lnkCredits);
            this.Controls.Add(this.lblLicense);
            this.Controls.Add(this.rtxtContent);
            this.Controls.Add(this.btnOkay);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.Padding = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "关于思美远程监控管理系统";
            this.Load += new System.EventHandler(this.AboutForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSubTitle;
        private System.Windows.Forms.LinkLabel lnkGiteePage;
        private System.Windows.Forms.LinkLabel lnkCredits;
        private System.Windows.Forms.Label lblLicense;
        private System.Windows.Forms.RichTextBox rtxtContent;
        private System.Windows.Forms.Button btnOkay;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblTitle;
    }
}
