namespace SiMay.RemoteMonitor.Controls
{
    partial class TcpConnectionManager
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
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.刷新列表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.关闭连接ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lstConnections = new SiMay.RemoteMonitor.UserControls.UListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.刷新列表ToolStripMenuItem,
            this.关闭连接ToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(125, 48);
            // 
            // 刷新列表ToolStripMenuItem
            // 
            this.刷新列表ToolStripMenuItem.Name = "刷新列表ToolStripMenuItem";
            this.刷新列表ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.刷新列表ToolStripMenuItem.Text = "刷新列表";
            this.刷新列表ToolStripMenuItem.Click += new System.EventHandler(this.刷新列表ToolStripMenuItem_Click);
            // 
            // 关闭连接ToolStripMenuItem
            // 
            this.关闭连接ToolStripMenuItem.Name = "关闭连接ToolStripMenuItem";
            this.关闭连接ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.关闭连接ToolStripMenuItem.Text = "关闭连接";
            this.关闭连接ToolStripMenuItem.Click += new System.EventHandler(this.关闭连接ToolStripMenuItem_Click);
            // 
            // lstConnections
            // 
            this.lstConnections.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.lstConnections.ContextMenuStrip = this.contextMenuStrip;
            this.lstConnections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstConnections.FullRowSelect = true;
            this.lstConnections.Location = new System.Drawing.Point(0, 0);
            this.lstConnections.Name = "lstConnections";
            this.lstConnections.Size = new System.Drawing.Size(800, 450);
            this.lstConnections.TabIndex = 0;
            this.lstConnections.UseCompatibleStateImageBehavior = false;
            this.lstConnections.UseWindowsThemStyle = true;
            this.lstConnections.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "进程";
            this.columnHeader1.Width = 179;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "本地地址";
            this.columnHeader2.Width = 95;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "本地端口";
            this.columnHeader3.Width = 75;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "远程地址";
            this.columnHeader4.Width = 95;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "远程端口";
            this.columnHeader5.Width = 75;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "状态";
            this.columnHeader6.Width = 85;
            // 
            // TcpConnectionManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lstConnections);
            this.Name = "TcpConnectionManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TCP连接管理";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestApp_FormClosing);
            this.Load += new System.EventHandler(this.TcpConnectionManager_Load);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private UserControls.UListView lstConnections;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem 刷新列表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 关闭连接ToolStripMenuItem;
    }
}