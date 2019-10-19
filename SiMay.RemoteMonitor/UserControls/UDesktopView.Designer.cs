namespace SiMay.RemoteMonitor.UserControls
{
    partial class UDesktopView
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.img = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.checkBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.img)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // img
            // 
            this.img.BackColor = System.Drawing.Color.White;
            this.img.Cursor = System.Windows.Forms.Cursors.Hand;
            this.img.Dock = System.Windows.Forms.DockStyle.Fill;
            this.img.Location = new System.Drawing.Point(0, 0);
            this.img.Name = "img";
            this.img.Size = new System.Drawing.Size(284, 145);
            this.img.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.img.TabIndex = 1;
            this.img.TabStop = false;
            this.img.Click += new System.EventHandler(this.img_Click);
            this.img.DoubleClick += new System.EventHandler(this.img_DoubleClick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.checkBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 145);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(284, 21);
            this.panel1.TabIndex = 2;
            // 
            // checkBox
            // 
            this.checkBox.AutoSize = true;
            this.checkBox.ForeColor = System.Drawing.Color.Blue;
            this.checkBox.Location = new System.Drawing.Point(3, 3);
            this.checkBox.Name = "checkBox";
            this.checkBox.Size = new System.Drawing.Size(78, 16);
            this.checkBox.TabIndex = 0;
            this.checkBox.Text = "checkBox1";
            this.checkBox.UseVisualStyleBackColor = true;
            // 
            // UserTVDesktop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.img);
            this.Controls.Add(this.panel1);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Name = "UserTVDesktop";
            this.Size = new System.Drawing.Size(284, 166);
            this.Load += new System.EventHandler(this.UDesktopView_Load);
            ((System.ComponentModel.ISupportInitialize)(this.img)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox img;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox checkBox;
    }
}
