using SiMay.Basic;
using SiMay.Core;
using SiMay.RemoteControls.Core;
using SiMay.RemoteMonitor.Attributes;
using SiMay.RemoteMonitor.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Application
{
    [OnTools]
    [Rank(70)]
    [ApplicationName("系统管理")]
    [AppResourceName("SystemManager")]
    public partial class SystemApplication : Form, IApplication
    {
        private string _title = "//系统管理 #Name#";

        [ApplicationAdapterHandler]
        public SystemAdapterHandler SystemAdapterHandler { get; set; }

        public SystemApplication()
        {
            InitializeComponent();
        }

        public void Start()
        {
            this.Show();
        }

        public void SetParameter(object arg)
        {
            throw new NotImplementedException();
        }

        public void SessionClose(ApplicationBaseAdapterHandler handler)
        {
            this.Text = _title + " [" + this.SystemAdapterHandler.State.ToString() + "]";
        }

        public void ContinueTask(ApplicationBaseAdapterHandler handler)
        {
            this.Text = _title;
        }

        private async void SystemManager_Load(object sender, EventArgs e)
        {
            this.Text = _title;
            this.processListView.Columns.Add("映像名称", 150);
            this.processListView.Columns.Add("窗口标题", 150);
            this.processListView.Columns.Add("窗口句柄", 100);
            this.processListView.Columns.Add("内存", 100);
            this.processListView.Columns.Add("线程数量", 100);
            this.processListView.Columns.Add("会话标识", 100);
            this.processListView.Columns.Add("用户名称", 100);
            this.processListView.Columns.Add("文件位置", 300);

            this._title = _title.Replace("#Name#", SystemAdapterHandler.OriginName);
            this.Text = this._title;
            var systemInfos = await this.SystemAdapterHandler.GetSystemInfoItems();
            if (!systemInfos.IsNull())
                OnSystemInfoHandlerEvent(systemInfos);

            var sessions = await this.SystemAdapterHandler.EnumSession();
            if (!sessions.IsNull())
                OnSessionsEventHandler(sessions);

            this.GetSystemInfos();
        }

        private void OnSessionsEventHandler(IEnumerable<SessionItem> sessions)
        {
            this.sessionsListView.Items.Clear();
            this.sessionsListView.Items.AddRange(sessions
                .Select(c => new SessionViewItem(c.UserName, c.SessionId, c.SessionState, c.WindowStationName, c.HasUserProcess))
                .ToArray());
        }

        private void OnOccupyHandlerEvent(string cpuOccupy, string memroyOccupy)
        {
            this.cpuUse.Text = $"CPU使用率:{cpuOccupy}";
            this.moryUse.Text = $"内存:{memroyOccupy}";
        }

        private void OnSystemInfoHandlerEvent(IEnumerable<SystemInfoItem> infoItems)
        {
            systemInfoList.Items.Clear();
            foreach (var item in infoItems)
            {
                var lv = new ListViewItem();
                lv.Text = item.ItemName;
                lv.SubItems.Add(item.Value);
                systemInfoList.Items.Add(lv);
            }
        }

        private void OnProcessListHandlerEvent(IEnumerable<ProcessItem> processItems)
        {
            var processLst = new List<ProcessItem>(processItems);
            var listViews = processListView.Items.Cast<ProcListViewItem>().ToArray();

            //待移除进程项
            var waitRemoveItems = new Queue<ProcListViewItem>();
            for (int i = 0; i < listViews.Length; i++)
            {
                var item = listViews[i];
                var processIndex = processLst.FindIndex(c => c.ProcessId == item.ProcessId);

                if (processIndex > 0)
                {
                    var process = processLst[processIndex];
                    processLst.RemoveAt(processIndex);
                    item.Update(process.ProcessId, process.ProcessName, process.WindowName, process.WindowHandler, process.ProcessMemorySize, process.ProcessThreadCount, process.FilePath, process.SessionId, process.User);
                }
                else
                    waitRemoveItems.Enqueue(item);
            }

            foreach (var item in processLst)
            {

                if (waitRemoveItems.Count > 0)
                {
                    var processItem = waitRemoveItems.Dequeue();
                    processItem.Update(item.ProcessId, item.ProcessName, item.WindowName, item.WindowHandler, item.ProcessMemorySize, item.ProcessThreadCount, item.FilePath, item.SessionId, item.User);
                }
                else
                {
                    var processItem = new ProcListViewItem(item.ProcessId, item.ProcessName, item.WindowName, item.WindowHandler, item.ProcessMemorySize, item.ProcessThreadCount, item.FilePath, item.SessionId, item.User);
                    processListView.Items.Add(processItem);
                }
            }

            for (int i = 0; i < waitRemoveItems.Count; i++)
            {
                var item = waitRemoveItems.Dequeue();
                item.Remove();
            }

            m_proNum.Text = processListView.Items.Count.ToString();
        }

        private void SystemManagerFom_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.refreshTimer.Stop();
            this.refreshTimer.Dispose();
            this.SystemAdapterHandler.CloseSession();
        }

        private async void GetSystemInfos()
        {
            var occupyResult = await this.SystemAdapterHandler.GetOccupy();
            if (!occupyResult.memoryUsage.IsNull() && !occupyResult.cpuusage.IsNull())
                OnOccupyHandlerEvent(occupyResult.cpuusage, occupyResult.memoryUsage);

            var processList = await this.SystemAdapterHandler.GetProcessList();
            if (!processList.IsNull())
                OnProcessListHandlerEvent(processList);
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection SelectItem = processListView.SelectedItems;
            for (int i = 0; i < SelectItem.Count; i++)
                processListView.Items[SelectItem[i].Index].Checked = true;

            var ids = new List<int>();
            foreach (ProcListViewItem item in processListView.Items)
            {
                if (item.Checked == true)
                    ids.Add(item.ProcessId);

                item.Checked = false;
            }

            if (!ids.Any())
            {
                MessageBoxHelper.ShowBoxExclamation("请选择需要结束的进程!");
                return;
            }

            if (MessageBox.Show("确认要结束选中的进程吗?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
                return;

            await this.SystemAdapterHandler.KillProcess(ids);
            var processList = await this.SystemAdapterHandler.GetProcessList();
            if (!processList.IsNull())
                OnProcessListHandlerEvent(processList);
        }
        private async void button3_Click(object sender, EventArgs e)
        {
            var ids = this.GetSelectProcessIds();
            await this.SystemAdapterHandler.SetProcessWindowMaxi(ids);
        }
        private async void button4_Click(object sender, EventArgs e)
        {
            var ids = this.GetSelectProcessIds();
            await this.SystemAdapterHandler.SetProcessWindowMize(ids);
        }

        private IEnumerable<int> GetSelectProcessIds()
        {
            ListView.SelectedListViewItemCollection SelectItem = processListView.SelectedItems;
            for (int i = 0; i < SelectItem.Count; i++)
                processListView.Items[SelectItem[i].Index].Checked = true;

            var handlers = new List<int>();
            foreach (ProcListViewItem item in processListView.Items)
            {
                if (item.Checked == true)
                {
                    if (item.WindowHandler != 0)
                        handlers.Add(item.WindowHandler);
                }
                item.Checked = false;
            }

            return handlers;
        }

        private void 立即刷新ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.GetSystemInfos();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            this.GetSystemInfos();
        }

        private void 正常ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!refreshTimer.Enabled)
                refreshTimer.Start();

            this.refreshTimer.Interval = 1500;
        }

        private void 高ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!refreshTimer.Enabled)
                refreshTimer.Start();

            this.refreshTimer.Interval = 500;
        }

        private void 低ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!refreshTimer.Enabled)
                refreshTimer.Start();

            this.refreshTimer.Interval = 2500;
        }

        private void 暂停ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (refreshTimer.Enabled)
                refreshTimer.Stop();
        }

        private void 关闭窗口ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection SelectItem = this.sessionsListView.SelectedItems;
            for (int i = 0; i < SelectItem.Count; i++)
                this.sessionsListView.Items[SelectItem[i].Index].Checked = true;

            var ids = new List<int>();
            foreach (SessionViewItem item in this.sessionsListView.Items)
            {
                if (item.Checked)
                {
                    if (item.HasUserProcess)
                    {
                        MessageBoxHelper.ShowBoxExclamation("不能重复创建用户进程!");
                        return;
                    }
                    ids.Add(item.SessionId);
                }

                item.Checked = false;
            }

            if (MessageBox.Show("确认要在选中的会话中创建被控用户进程吗?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
                return;

            foreach (var sessionId in ids)
            {
                await this.SystemAdapterHandler.CreateProcessAsUser(sessionId);
            }

            var sessions = await this.SystemAdapterHandler.EnumSession();
            if (!sessions.IsNull())
                OnSessionsEventHandler(sessions);
        }
    }
}