namespace SiMay.RemoteMonitor.Controls
{
    partial class FileManager
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
            this.cmdContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.打开ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.详细信息ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.列表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.平铺ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.刷新目录ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.downloadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.downloadAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.文件夹ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.复制文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.删除文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.重命名ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.新建文件夹ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.刷新ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.取消选择ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.parentBut = new System.Windows.Forms.Button();
            this.txtRemotedirectory = new System.Windows.Forms.TextBox();
            this.refreshBut = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.transferCaption = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.time = new System.Windows.Forms.ToolStripStatusLabel();
            this.transferDatalenght = new System.Windows.Forms.ToolStripStatusLabel();
            this.transferProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSavePath = new System.Windows.Forms.LinkLabel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.treeContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.打开目录ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileList = new SiMay.RemoteMonitor.UserControls.UListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmdContext.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.treeContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdContext
            // 
            this.cmdContext.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.cmdContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.打开ToolStripMenuItem,
            this.toolStripMenuItem5,
            this.toolStripMenuItem4,
            this.刷新目录ToolStripMenuItem,
            this.toolStripSeparator2,
            this.downloadMenuItem,
            this.uploadMenuItem,
            this.toolStripSeparator3,
            this.复制文件ToolStripMenuItem,
            this.toolStripMenuItem1,
            this.删除文件ToolStripMenuItem,
            this.重命名ToolStripMenuItem,
            this.新建文件夹ToolStripMenuItem,
            this.toolStripSeparator1,
            this.刷新ToolStripMenuItem,
            this.取消选择ToolStripMenuItem});
            this.cmdContext.Name = "cmdContext";
            this.cmdContext.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.cmdContext.Size = new System.Drawing.Size(137, 308);
            // 
            // 打开ToolStripMenuItem
            // 
            this.打开ToolStripMenuItem.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.打开ToolStripMenuItem.Name = "打开ToolStripMenuItem";
            this.打开ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.打开ToolStripMenuItem.Text = "打开";
            this.打开ToolStripMenuItem.Click += new System.EventHandler(this.打开ToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(136, 22);
            this.toolStripMenuItem5.Text = "远程运行";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.ToolStripMenuItem5_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.详细信息ToolStripMenuItem,
            this.列表ToolStripMenuItem,
            this.平铺ToolStripMenuItem});
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(136, 22);
            this.toolStripMenuItem4.Text = "查看";
            // 
            // 详细信息ToolStripMenuItem
            // 
            this.详细信息ToolStripMenuItem.Checked = true;
            this.详细信息ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.详细信息ToolStripMenuItem.Name = "详细信息ToolStripMenuItem";
            this.详细信息ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.详细信息ToolStripMenuItem.Text = "详细信息";
            this.详细信息ToolStripMenuItem.Click += new System.EventHandler(this.详细信息ToolStripMenuItem_Click);
            // 
            // 列表ToolStripMenuItem
            // 
            this.列表ToolStripMenuItem.Name = "列表ToolStripMenuItem";
            this.列表ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.列表ToolStripMenuItem.Text = "列表";
            this.列表ToolStripMenuItem.Click += new System.EventHandler(this.列表ToolStripMenuItem_Click);
            // 
            // 平铺ToolStripMenuItem
            // 
            this.平铺ToolStripMenuItem.Name = "平铺ToolStripMenuItem";
            this.平铺ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.平铺ToolStripMenuItem.Text = "平铺";
            this.平铺ToolStripMenuItem.Click += new System.EventHandler(this.平铺ToolStripMenuItem_Click);
            // 
            // 刷新目录ToolStripMenuItem
            // 
            this.刷新目录ToolStripMenuItem.Name = "刷新目录ToolStripMenuItem";
            this.刷新目录ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.刷新目录ToolStripMenuItem.Text = "刷新目录";
            this.刷新目录ToolStripMenuItem.Click += new System.EventHandler(this.刷新目录ToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(133, 6);
            // 
            // downloadMenuItem
            // 
            this.downloadMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.downloadAsToolStripMenuItem});
            this.downloadMenuItem.Name = "downloadMenuItem";
            this.downloadMenuItem.Size = new System.Drawing.Size(136, 22);
            this.downloadMenuItem.Text = "下载";
            this.downloadMenuItem.Click += new System.EventHandler(this.downloadMenuItem_Click);
            // 
            // downloadAsToolStripMenuItem
            // 
            this.downloadAsToolStripMenuItem.Name = "downloadAsToolStripMenuItem";
            this.downloadAsToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.downloadAsToolStripMenuItem.Text = "下载另存为";
            this.downloadAsToolStripMenuItem.Click += new System.EventHandler(this.下载到ToolStripMenuItem_Click);
            // 
            // uploadMenuItem
            // 
            this.uploadMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.文件ToolStripMenuItem,
            this.文件夹ToolStripMenuItem});
            this.uploadMenuItem.Name = "uploadMenuItem";
            this.uploadMenuItem.Size = new System.Drawing.Size(136, 22);
            this.uploadMenuItem.Text = "上传";
            // 
            // 文件ToolStripMenuItem
            // 
            this.文件ToolStripMenuItem.Name = "文件ToolStripMenuItem";
            this.文件ToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.文件ToolStripMenuItem.Text = "文件";
            this.文件ToolStripMenuItem.Click += new System.EventHandler(this.文件ToolStripMenuItem_Click);
            // 
            // 文件夹ToolStripMenuItem
            // 
            this.文件夹ToolStripMenuItem.Name = "文件夹ToolStripMenuItem";
            this.文件夹ToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.文件夹ToolStripMenuItem.Text = "文件夹";
            this.文件夹ToolStripMenuItem.Click += new System.EventHandler(this.文件夹ToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(133, 6);
            // 
            // 复制文件ToolStripMenuItem
            // 
            this.复制文件ToolStripMenuItem.Name = "复制文件ToolStripMenuItem";
            this.复制文件ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.复制文件ToolStripMenuItem.Text = "复制";
            this.复制文件ToolStripMenuItem.Click += new System.EventHandler(this.复制文件ToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(136, 22);
            this.toolStripMenuItem1.Text = "粘贴";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // 删除文件ToolStripMenuItem
            // 
            this.删除文件ToolStripMenuItem.Name = "删除文件ToolStripMenuItem";
            this.删除文件ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.删除文件ToolStripMenuItem.Text = "删除";
            this.删除文件ToolStripMenuItem.Click += new System.EventHandler(this.删除文件ToolStripMenuItem_Click);
            // 
            // 重命名ToolStripMenuItem
            // 
            this.重命名ToolStripMenuItem.Name = "重命名ToolStripMenuItem";
            this.重命名ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.重命名ToolStripMenuItem.Text = "重命名";
            this.重命名ToolStripMenuItem.Click += new System.EventHandler(this.重命名ToolStripMenuItem_Click);
            // 
            // 新建文件夹ToolStripMenuItem
            // 
            this.新建文件夹ToolStripMenuItem.Name = "新建文件夹ToolStripMenuItem";
            this.新建文件夹ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.新建文件夹ToolStripMenuItem.Text = "新建文件夹";
            this.新建文件夹ToolStripMenuItem.Click += new System.EventHandler(this.新建文件夹ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(133, 6);
            // 
            // 刷新ToolStripMenuItem
            // 
            this.刷新ToolStripMenuItem.Name = "刷新ToolStripMenuItem";
            this.刷新ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.刷新ToolStripMenuItem.Text = "全部选择";
            this.刷新ToolStripMenuItem.Click += new System.EventHandler(this.选择全部ToolStripMenuItem_Click);
            // 
            // 取消选择ToolStripMenuItem
            // 
            this.取消选择ToolStripMenuItem.Name = "取消选择ToolStripMenuItem";
            this.取消选择ToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.取消选择ToolStripMenuItem.Text = "取消选择";
            this.取消选择ToolStripMenuItem.Click += new System.EventHandler(this.取消选择ToolStripMenuItem_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 73F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.Controls.Add(this.parentBut, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtRemotedirectory, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.refreshBut, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(861, 30);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // parentBut
            // 
            this.parentBut.Location = new System.Drawing.Point(764, 3);
            this.parentBut.Name = "parentBut";
            this.parentBut.Size = new System.Drawing.Size(85, 24);
            this.parentBut.TabIndex = 0;
            this.parentBut.Text = "上级目录";
            this.parentBut.UseVisualStyleBackColor = true;
            this.parentBut.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtRemotedirectory
            // 
            this.txtRemotedirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRemotedirectory.Location = new System.Drawing.Point(76, 4);
            this.txtRemotedirectory.Name = "txtRemotedirectory";
            this.txtRemotedirectory.Size = new System.Drawing.Size(582, 21);
            this.txtRemotedirectory.TabIndex = 2;
            this.txtRemotedirectory.MouseClick += new System.Windows.Forms.MouseEventHandler(this.TxtRemotedirectory_MouseClick);
            this.txtRemotedirectory.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtRemotedirectory_KeyPress);
            // 
            // refreshBut
            // 
            this.refreshBut.Location = new System.Drawing.Point(664, 3);
            this.refreshBut.Name = "refreshBut";
            this.refreshBut.Size = new System.Drawing.Size(94, 24);
            this.refreshBut.TabIndex = 1;
            this.refreshBut.Text = "刷新目录";
            this.refreshBut.UseVisualStyleBackColor = true;
            this.refreshBut.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "远程目录:";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.transferCaption,
            this.toolStripStatusLabel3,
            this.time,
            this.transferDatalenght,
            this.transferProgress,
            this.toolStripDropDownButton1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 493);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.statusStrip1.Size = new System.Drawing.Size(861, 23);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // transferCaption
            // 
            this.transferCaption.Name = "transferCaption";
            this.transferCaption.Size = new System.Drawing.Size(0, 18);
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(621, 18);
            this.toolStripStatusLabel3.Spring = true;
            // 
            // time
            // 
            this.time.Name = "time";
            this.time.Size = new System.Drawing.Size(0, 18);
            // 
            // transferDatalenght
            // 
            this.transferDatalenght.Name = "transferDatalenght";
            this.transferDatalenght.Size = new System.Drawing.Size(67, 18);
            this.transferDatalenght.Text = "已接收0KB";
            // 
            // transferProgress
            // 
            this.transferProgress.Name = "transferProgress";
            this.transferProgress.Size = new System.Drawing.Size(120, 17);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.ShowDropDownArrow = false;
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(36, 21);
            this.toolStripDropDownButton1.Text = "停止";
            this.toolStripDropDownButton1.Click += new System.EventHandler(this.toolStripDropDownButton1_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 476);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "下载目录:";
            // 
            // txtSavePath
            // 
            this.txtSavePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtSavePath.AutoSize = true;
            this.txtSavePath.Location = new System.Drawing.Point(74, 476);
            this.txtSavePath.Name = "txtSavePath";
            this.txtSavePath.Size = new System.Drawing.Size(23, 12);
            this.txtSavePath.TabIndex = 8;
            this.txtSavePath.TabStop = true;
            this.txtSavePath.Text = "C:\\";
            this.txtSavePath.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.savePath_LinkClicked);
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(796, 476);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(53, 12);
            this.linkLabel1.TabIndex = 9;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "设置目录";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // treeContext
            // 
            this.treeContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.打开目录ToolStripMenuItem});
            this.treeContext.Name = "treeContext";
            this.treeContext.Size = new System.Drawing.Size(125, 26);
            // 
            // 打开目录ToolStripMenuItem
            // 
            this.打开目录ToolStripMenuItem.Name = "打开目录ToolStripMenuItem";
            this.打开目录ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.打开目录ToolStripMenuItem.Text = "打开目录";
            this.打开目录ToolStripMenuItem.Click += new System.EventHandler(this.打开目录ToolStripMenuItem_Click);
            // 
            // fileList
            // 
            this.fileList.AllowDrop = true;
            this.fileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileList.CheckBoxes = true;
            this.fileList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.fileList.ContextMenuStrip = this.cmdContext;
            this.fileList.FullRowSelect = true;
            this.fileList.HideSelection = false;
            this.fileList.Location = new System.Drawing.Point(12, 30);
            this.fileList.Name = "fileList";
            this.fileList.Size = new System.Drawing.Size(837, 442);
            this.fileList.TabIndex = 4;
            this.fileList.UseCompatibleStateImageBehavior = false;
            this.fileList.UseWindowsThemStyle = true;
            this.fileList.View = System.Windows.Forms.View.Details;
            this.fileList.DragDrop += new System.Windows.Forms.DragEventHandler(this.fileList_DragDrop);
            this.fileList.DragEnter += new System.Windows.Forms.DragEventHandler(this.fileList_DragEnter);
            this.fileList.DoubleClick += new System.EventHandler(this.m_files_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "名称";
            this.columnHeader1.Width = 250;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "大小";
            this.columnHeader2.Width = 120;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "类型";
            this.columnHeader3.Width = 100;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "修改日期";
            this.columnHeader4.Width = 130;
            // 
            // FileManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(861, 516);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.txtSavePath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.fileList);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "FileManager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "远程文件管理";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FileManager_FormClosing);
            this.Load += new System.EventHandler(this.FileManager_Load);
            this.cmdContext.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.treeContext.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button parentBut;
        private System.Windows.Forms.Button refreshBut;
        private System.Windows.Forms.Label label1;
        private UserControls.UListView fileList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel transferCaption;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripProgressBar transferProgress;
        private System.Windows.Forms.TextBox txtRemotedirectory;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ContextMenuStrip cmdContext;
        private System.Windows.Forms.ToolStripMenuItem 打开ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 复制文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 删除文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 重命名ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 新建文件夹ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem 刷新ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem 取消选择ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem downloadMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uploadMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel txtSavePath;
        private System.Windows.Forms.ToolStripStatusLabel transferDatalenght;
        private System.Windows.Forms.ToolStripMenuItem 刷新目录ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem 详细信息ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 列表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 平铺ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 文件夹ToolStripMenuItem;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripStatusLabel time;
        private System.Windows.Forms.ToolStripMenuItem downloadAsToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip treeContext;
        private System.Windows.Forms.ToolStripMenuItem 打开目录ToolStripMenuItem;
    }
}