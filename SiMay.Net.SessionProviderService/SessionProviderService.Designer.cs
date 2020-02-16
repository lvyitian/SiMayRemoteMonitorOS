namespace SiMay.Net.SessionProviderService
{
    partial class SessionProviderService
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.channelListView = new System.Windows.Forms.ListView();
            this.channel_id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.create_time = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.channel_type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.channel_rate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.channelListContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lableStatrTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbUpload = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel6 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbReceive = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel8 = new System.Windows.Forms.ToolStripStatusLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.lableIPAddress = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelPort = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lableConnectionCount = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.logList = new System.Windows.Forms.ListView();
            this.log_create_time = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.log = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.logContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.清空日志ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.timerFlowCalac = new System.Windows.Forms.Timer(this.components);
            this.channelListContext.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.logContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // channelListView
            // 
            this.channelListView.CheckBoxes = true;
            this.channelListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.channel_id,
            this.create_time,
            this.channel_type,
            this.channel_rate});
            this.channelListView.ContextMenuStrip = this.channelListContext;
            this.channelListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.channelListView.FullRowSelect = true;
            this.channelListView.HideSelection = false;
            this.channelListView.Location = new System.Drawing.Point(0, 0);
            this.channelListView.Margin = new System.Windows.Forms.Padding(4);
            this.channelListView.Name = "channelListView";
            this.channelListView.Size = new System.Drawing.Size(963, 236);
            this.channelListView.TabIndex = 0;
            this.channelListView.UseCompatibleStateImageBehavior = false;
            this.channelListView.View = System.Windows.Forms.View.Details;
            // 
            // channel_id
            // 
            this.channel_id.Text = "Id";
            this.channel_id.Width = 150;
            // 
            // create_time
            // 
            this.create_time.Text = "连接时间";
            this.create_time.Width = 150;
            // 
            // channel_type
            // 
            this.channel_type.Text = "连接类型";
            this.channel_type.Width = 150;
            // 
            // channel_rate
            // 
            this.channel_rate.Text = "实时速率(上行/下行)";
            this.channel_rate.Width = 175;
            // 
            // channelListContext
            // 
            this.channelListContext.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.channelListContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3});
            this.channelListContext.Name = "logsContext";
            this.channelListContext.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.channelListContext.Size = new System.Drawing.Size(139, 28);
            this.channelListContext.Opening += new System.ComponentModel.CancelEventHandler(this.channelListContext_Opening);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(138, 24);
            this.toolStripMenuItem3.Text = "关闭连接";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.lableStatrTime,
            this.toolStripStatusLabel3,
            this.toolStripStatusLabel2,
            this.lbUpload,
            this.toolStripStatusLabel4,
            this.toolStripStatusLabel6,
            this.lbReceive,
            this.toolStripStatusLabel8});
            this.statusStrip1.Location = new System.Drawing.Point(0, 589);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.statusStrip1.Size = new System.Drawing.Size(963, 26);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(73, 20);
            this.toolStripStatusLabel1.Text = "启动时间:";
            // 
            // lableStatrTime
            // 
            this.lableStatrTime.Name = "lableStatrTime";
            this.lableStatrTime.Size = new System.Drawing.Size(167, 20);
            this.lableStatrTime.Text = "toolStripStatusLabel1";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(451, 20);
            this.toolStripStatusLabel3.Spring = true;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(43, 20);
            this.toolStripStatusLabel2.Text = "上行:";
            // 
            // lbUpload
            // 
            this.lbUpload.Name = "lbUpload";
            this.lbUpload.Size = new System.Drawing.Size(40, 20);
            this.lbUpload.Text = "0.00";
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(43, 20);
            this.toolStripStatusLabel4.Text = "KB/S";
            // 
            // toolStripStatusLabel6
            // 
            this.toolStripStatusLabel6.Name = "toolStripStatusLabel6";
            this.toolStripStatusLabel6.Size = new System.Drawing.Size(43, 20);
            this.toolStripStatusLabel6.Text = "下行:";
            // 
            // lbReceive
            // 
            this.lbReceive.Name = "lbReceive";
            this.lbReceive.Size = new System.Drawing.Size(40, 20);
            this.lbReceive.Text = "0.00";
            // 
            // toolStripStatusLabel8
            // 
            this.toolStripStatusLabel8.Name = "toolStripStatusLabel8";
            this.toolStripStatusLabel8.Size = new System.Drawing.Size(43, 20);
            this.toolStripStatusLabel8.Text = "KB/S";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "IP:";
            // 
            // lableIPAddress
            // 
            this.lableIPAddress.AutoSize = true;
            this.lableIPAddress.Location = new System.Drawing.Point(55, 9);
            this.lableIPAddress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lableIPAddress.Name = "lableIPAddress";
            this.lableIPAddress.Size = new System.Drawing.Size(63, 15);
            this.lableIPAddress.TabIndex = 3;
            this.lableIPAddress.Text = "0.0.0.0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(197, 9);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Port:";
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(252, 9);
            this.labelPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(39, 15);
            this.labelPort.TabIndex = 5;
            this.labelPort.Text = "5200";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(737, 9);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 15);
            this.label5.TabIndex = 6;
            this.label5.Text = "会话连接数量:";
            // 
            // lableConnectionCount
            // 
            this.lableConnectionCount.AutoSize = true;
            this.lableConnectionCount.Location = new System.Drawing.Point(856, 9);
            this.lableConnectionCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lableConnectionCount.Name = "lableConnectionCount";
            this.lableConnectionCount.Size = new System.Drawing.Size(15, 15);
            this.lableConnectionCount.TabIndex = 7;
            this.lableConnectionCount.Text = "0";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 30);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.channelListView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.logList);
            this.splitContainer1.Size = new System.Drawing.Size(963, 554);
            this.splitContainer1.SplitterDistance = 236;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 8;
            // 
            // logList
            // 
            this.logList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.log_create_time,
            this.log});
            this.logList.ContextMenuStrip = this.logContext;
            this.logList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logList.FullRowSelect = true;
            this.logList.HideSelection = false;
            this.logList.Location = new System.Drawing.Point(0, 0);
            this.logList.Margin = new System.Windows.Forms.Padding(4);
            this.logList.Name = "logList";
            this.logList.Size = new System.Drawing.Size(963, 317);
            this.logList.TabIndex = 1;
            this.logList.UseCompatibleStateImageBehavior = false;
            this.logList.View = System.Windows.Forms.View.Details;
            // 
            // log_create_time
            // 
            this.log_create_time.Text = "发生时间";
            this.log_create_time.Width = 114;
            // 
            // log
            // 
            this.log.Text = "事件信息";
            this.log.Width = 400;
            // 
            // logContext
            // 
            this.logContext.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.logContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.清空日志ToolStripMenuItem1});
            this.logContext.Name = "logsContext";
            this.logContext.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.logContext.Size = new System.Drawing.Size(139, 76);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(138, 24);
            this.toolStripMenuItem1.Text = "复制日志";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(138, 24);
            this.toolStripMenuItem2.Text = "删除日志";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // 清空日志ToolStripMenuItem1
            // 
            this.清空日志ToolStripMenuItem1.Name = "清空日志ToolStripMenuItem1";
            this.清空日志ToolStripMenuItem1.Size = new System.Drawing.Size(138, 24);
            this.清空日志ToolStripMenuItem1.Text = "清空日志";
            this.清空日志ToolStripMenuItem1.Click += new System.EventHandler(this.清空日志ToolStripMenuItem1_Click);
            // 
            // timerFlowCalac
            // 
            this.timerFlowCalac.Enabled = true;
            this.timerFlowCalac.Interval = 1000;
            this.timerFlowCalac.Tick += new System.EventHandler(this.timerFlowCalac_Tick);
            // 
            // SessionProviderService
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(963, 615);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.lableConnectionCount);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.labelPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lableIPAddress);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusStrip1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SessionProviderService";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SiMay中间会话服务";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SessionProviderService_FormClosing);
            this.Load += new System.EventHandler(this.SessionProviderService_Load);
            this.channelListContext.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.logContext.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView channelListView;
        private System.Windows.Forms.ColumnHeader create_time;
        private System.Windows.Forms.ColumnHeader channel_type;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lableIPAddress;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripStatusLabel lableStatrTime;
        private System.Windows.Forms.Label lableConnectionCount;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ColumnHeader channel_rate;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView logList;
        private System.Windows.Forms.ColumnHeader log_create_time;
        private System.Windows.Forms.ColumnHeader log;
        private System.Windows.Forms.ContextMenuStrip logContext;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem 清空日志ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel lbUpload;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel6;
        private System.Windows.Forms.ToolStripStatusLabel lbReceive;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel8;
        private System.Windows.Forms.ContextMenuStrip channelListContext;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ColumnHeader channel_id;
        private System.Windows.Forms.Timer timerFlowCalac;
    }
}

