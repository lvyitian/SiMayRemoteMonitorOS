namespace SiMay.RemoteMonitor.UnconventionalApplication
{
    partial class RemoteUpdateApplication
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.originBox = new System.Windows.Forms.CheckBox();
            this.fileProgressBar = new System.Windows.Forms.ProgressBar();
            this.progressText = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // originBox
            // 
            this.originBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.originBox.AutoSize = true;
            this.originBox.Location = new System.Drawing.Point(23, 11);
            this.originBox.Name = "originBox";
            this.originBox.Size = new System.Drawing.Size(109, 19);
            this.originBox.TabIndex = 0;
            this.originBox.Text = "originName";
            this.originBox.UseVisualStyleBackColor = true;
            // 
            // fileProgressBar
            // 
            this.fileProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileProgressBar.Location = new System.Drawing.Point(130, 11);
            this.fileProgressBar.Name = "fileProgressBar";
            this.fileProgressBar.Size = new System.Drawing.Size(327, 18);
            this.fileProgressBar.TabIndex = 1;
            // 
            // progressText
            // 
            this.progressText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressText.AutoSize = true;
            this.progressText.Location = new System.Drawing.Point(463, 12);
            this.progressText.Name = "progressText";
            this.progressText.Size = new System.Drawing.Size(103, 15);
            this.progressText.TabIndex = 2;
            this.progressText.Text = "progressText";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(572, 8);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "关闭";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // RemoteUpdateApplication
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.progressText);
            this.Controls.Add(this.fileProgressBar);
            this.Controls.Add(this.originBox);
            this.Name = "RemoteUpdateApplication";
            this.Size = new System.Drawing.Size(645, 40);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox originBox;
        private System.Windows.Forms.ProgressBar fileProgressBar;
        private System.Windows.Forms.Label progressText;
        private System.Windows.Forms.Button button1;
    }
}
