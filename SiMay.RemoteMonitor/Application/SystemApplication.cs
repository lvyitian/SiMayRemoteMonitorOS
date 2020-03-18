using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.Packets;
using SiMay.RemoteControlsCore;
using SiMay.RemoteControlsCore.HandlerAdapters;
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
    [ApplicationName("系统管理")]
    [AppResourceName("SystemManager")]
    [Application(typeof(SystemAdapterHandler), AppJobConstant.REMOTE_SYSMANAGER, 70)]
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

        public void SessionClose(ApplicationAdapterHandler handler)
        {
            this.Text = _title + " [" + this.SystemAdapterHandler.StateContext.ToString() + "]";
        }

        public void ContinueTask(ApplicationAdapterHandler handler)
        {
            this.Text = _title;
        }

        private void SystemManager_Load(object sender, EventArgs e)
        {
            this.Text = _title;
            this.processList.Columns.Add("映像名称", 150);
            this.processList.Columns.Add("窗口标题", 150);
            this.processList.Columns.Add("窗口句柄", 100);
            this.processList.Columns.Add("内存", 100);
            this.processList.Columns.Add("线程数量", 100);
            this.processList.Columns.Add("会话标识", 100);
            this.processList.Columns.Add("用户名称", 100);
            this.processList.Columns.Add("文件位置", 300);

            this.UninstallList.Columns.Add("名称", 150);
            this.UninstallList.Columns.Add("发布者", 150);
            this.UninstallList.Columns.Add("安装时间", 100);
            this.UninstallList.Columns.Add("大小", 100);
            this.UninstallList.Columns.Add("版本", 100);

            this.SystemAdapterHandler.OnProcessListHandlerEvent += OnProcessListHandlerEvent;
            this.SystemAdapterHandler.OnSystemInfoHandlerEvent += OnSystemInfoHandlerEvent;
            this.SystemAdapterHandler.OnOccupyHandlerEvent += OnOccupyHandlerEvent;
            this.SystemAdapterHandler.OnSessionsEventHandler += OnSessionsEventHandler;
            this.SystemAdapterHandler.OnUninstallListEventHandler += OnUninstallListEventHandler;
            this._title = _title.Replace("#Name#", SystemAdapterHandler.OriginName);
            this.Text = this._title;
            this.SystemAdapterHandler.GetSystemInfoItems();
            this.SystemAdapterHandler.EnumSession();
            this.GetSystemInfos();

            this.SystemAdapterHandler.OnServicesListEventHandler += OnServicesListEventHandler;
            this.SystemAdapterHandler.Service_GetList();
            this.SystemAdapterHandler.Uninstall_GetList();
        }


        private void OnUninstallListEventHandler(SystemAdapterHandler adapterHandler, IEnumerable<UninstallInfo> uninstalllInfo)
        {
            this.UninstallList.Items.Clear();
            var uninstalllList = new List<UninstallInfo>(uninstalllInfo);
            foreach (var item in uninstalllList)
            {
                var uninstalllItem = new UninstallViewItem(item.DisplayName, item.Publisher, item.InstallDate, item.Size, item.DisplayVersion);
                this.UninstallList.Items.Add(uninstalllItem);
            }
        }

        private void OnSessionsEventHandler(SystemAdapterHandler adapterHandler, IEnumerable<Core.Packets.SysManager.SessionItem> sessions)
        {
            this.sessionsListView.Items.Clear();
            this.sessionsListView.Items.AddRange(sessions
                .Select(c => new SessionViewItem(c.UserName, c.SessionId, c.SessionState, c.WindowStationName, c.HasUserProcess))
                .ToArray());
        }

        private void OnOccupyHandlerEvent(SystemAdapterHandler adapterHandler, string cpuOccupy, string memroyOccupy)
        {
            this.cpuUse.Text = $"CPU使用率:{cpuOccupy}";
            this.moryUse.Text = $"内存:{memroyOccupy}";
        }

        private void OnSystemInfoHandlerEvent(SystemAdapterHandler adapterHandler, IEnumerable<SystemInfoItem> infoItems)
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

        private void OnProcessListHandlerEvent(SystemAdapterHandler adapterHandler, IEnumerable<ProcessItem> processItems)
        {
            var processLst = new List<ProcessItem>(processItems);
            var listViews = processList.Items.Cast<ProcListViewItem>().ToArray();

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
                    processList.Items.Add(processItem);
                }
            }

            for (int i = 0; i < waitRemoveItems.Count; i++)
            {
                var item = waitRemoveItems.Dequeue();
                item.Remove();
            }

            m_proNum.Text = processList.Items.Count.ToString();
        }

        private void SystemManagerFom_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.refreshTimer.Stop();
            this.refreshTimer.Dispose();
            this.SystemAdapterHandler.OnProcessListHandlerEvent -= OnProcessListHandlerEvent;
            this.SystemAdapterHandler.OnSystemInfoHandlerEvent -= OnSystemInfoHandlerEvent;
            this.SystemAdapterHandler.OnOccupyHandlerEvent -= OnOccupyHandlerEvent;
            this.SystemAdapterHandler.OnSessionsEventHandler -= OnSessionsEventHandler;
            this.SystemAdapterHandler.OnServicesListEventHandler -= OnServicesListEventHandler;
            this.SystemAdapterHandler.OnUninstallListEventHandler -= OnUninstallListEventHandler;
            this.SystemAdapterHandler.CloseSession();
        }

        private void GetSystemInfos()
        {
            this.SystemAdapterHandler.GetOccupy();
            this.SystemAdapterHandler.GetProcessList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection SelectItem = processList.SelectedItems;
            for (int i = 0; i < SelectItem.Count; i++)
                processList.Items[SelectItem[i].Index].Checked = true;

            var ids = new List<int>();
            foreach (ProcListViewItem item in processList.Items)
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

            this.SystemAdapterHandler.KillProcess(ids);
            this.SystemAdapterHandler.GetProcessList();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            var ids = this.GetSelectProcessIds();
            this.SystemAdapterHandler.SetProcessWindowMaxi(ids);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            var ids = this.GetSelectProcessIds();
            this.SystemAdapterHandler.SetProcessWindowMize(ids);
        }

        private IEnumerable<int> GetSelectProcessIds()
        {
            ListView.SelectedListViewItemCollection SelectItem = processList.SelectedItems;
            for (int i = 0; i < SelectItem.Count; i++)
                processList.Items[SelectItem[i].Index].Checked = true;

            var handlers = new List<int>();
            foreach (ProcListViewItem item in processList.Items)
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

        private void button1_Click(object sender, EventArgs e)
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
                this.SystemAdapterHandler.CreateProcessAsUser(sessionId);
            }

            this.SystemAdapterHandler.EnumSession();
        }

        private void OnServicesListEventHandler(SystemAdapterHandler adapterHandler, IEnumerable<ServiceItem> serviceItems)
        {
            this.serviceList.Items.Clear();
            var serviceList = new List<ServiceItem>(serviceItems);
            foreach (var item in serviceList)
            {
                var serviceitem = new ServiceViewItem(item.ServiceName, item.DisplayName, item.Description, item.Status, item.StartType, item.UserName);
                this.serviceList.Items.Add(serviceitem);
            }
        }

        private void tmunStart_Click(object sender, EventArgs e)
        {
            if (serviceList.SelectedItems.Count > 0)
            {
                ListView.SelectedListViewItemCollection selectItem = this.serviceList.SelectedItems;
                this.SystemAdapterHandler.Service_Strat(new ServiceItem()
                {
                    ServiceName = selectItem[0].Text
                });
            }
        }

        private void tmunStop_Click(object sender, EventArgs e)
        {
            if (serviceList.SelectedItems.Count > 0)
            {
                ListView.SelectedListViewItemCollection selectItem = this.serviceList.SelectedItems;
                this.SystemAdapterHandler.Service_Stop(new ServiceItem()
                {
                    ServiceName = selectItem[0].Text
                });
            }
        }

        private void tmunReStart_Click(object sender, EventArgs e)
        {
            if (serviceList.SelectedItems.Count > 0)
            {
                ListView.SelectedListViewItemCollection selectItem = this.serviceList.SelectedItems;
                this.SystemAdapterHandler.Service_ReStrat(new ServiceItem()
                {
                    ServiceName = selectItem[0].Text
                });
            }
        }

        private void tmunAutomatic_Click(object sender, EventArgs e)
        {
            if (serviceList.SelectedItems.Count > 0)
            {
                ListView.SelectedListViewItemCollection selectItem = this.serviceList.SelectedItems;
                this.SystemAdapterHandler.Service_StartType_Set(new ServiceItem()
                {
                    ServiceName = selectItem[0].Text,
                    StartType = "2"
                });
            }
        }

        private void tmunManual_Click(object sender, EventArgs e)
        {
            if (serviceList.SelectedItems.Count > 0)
            {
                ListView.SelectedListViewItemCollection selectItem = this.serviceList.SelectedItems;
                this.SystemAdapterHandler.Service_StartType_Set(new ServiceItem()
                {
                    ServiceName = selectItem[0].Text,
                    StartType = "3"
                });
            }
        }

        private void tmunDisable_Click(object sender, EventArgs e)
        {
            if (serviceList.SelectedItems.Count > 0)
            {
                ListView.SelectedListViewItemCollection selectItem = this.serviceList.SelectedItems;
                this.SystemAdapterHandler.Service_StartType_Set(new ServiceItem()
                {
                    ServiceName = selectItem[0].Text,
                    StartType = "4"
                });
            }
        }

        private void tmunUninstall_Click(object sender, EventArgs e)
        {
            if (UninstallList.SelectedItems.Count > 0)
            {
                ListView.SelectedListViewItemCollection selectItem = this.UninstallList.SelectedItems;
                this.SystemAdapterHandler.Uninstall_Un(new UninstallInfo()
                {
                    DisplayName = selectItem[0].Text
                });
            }
        }
    }
}