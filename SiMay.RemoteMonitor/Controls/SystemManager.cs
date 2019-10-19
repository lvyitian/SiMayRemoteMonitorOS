using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.Core.Packets.SysManager;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.RemoteMonitor.Attributes;
using SiMay.RemoteMonitor.ControlSource;
using SiMay.RemoteMonitor.Extensions;
using SiMay.RemoteMonitor.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Controls
{
    [ControlApp(70, "系统管理", "SystemManagerJob", "SystemManager")]
    public partial class SystemManager : Form, IControlSource
    {
        private string _title = "//系统管理 #Name#";
        private MessageAdapter _adapter;
        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        public SystemManager(MessageAdapter adapter)
        {
            _adapter = adapter;

            adapter.OnSessionNotifyPro += Adapter_OnSessionNotifyPro;
            //adapter.ResetMsg = this.GetType().GetControlKey();
            _title = _title.Replace("#Name#", adapter.OriginName);
            InitializeComponent();
        }
        public void Action()
            => this.Show();
        private void Adapter_OnSessionNotifyPro(SessionHandler session, Notify.SessionNotifyType notify)
        {
            switch (notify)
            {
                case Notify.SessionNotifyType.Message:
                    if (_adapter.WindowClosed)
                        return;

                    _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
                    break;
                case Notify.SessionNotifyType.OnReceive:
                    break;
                case Notify.SessionNotifyType.ContinueTask:
                    this.Text = _title;
                    break;
                case Notify.SessionNotifyType.SessionClosed:
                    this.Text = _title + " [" + _adapter.TipText + "]";
                    break;
                case Notify.SessionNotifyType.WindowShow:
                    this.Show();
                    break;
                case Notify.SessionNotifyType.WindowClose:
                    _adapter.WindowClosed = true;
                    this.Close();
                    break;
                default:
                    break;
            }
        }

        private void SystemManager_Load(object sender, EventArgs e)
        {
            this.Text = _title;
            this.processList.Columns.Add("映像名称", 130);
            this.processList.Columns.Add("窗口标题", 150);
            this.processList.Columns.Add("窗口句柄", 70);
            this.processList.Columns.Add("内存", 70);
            this.processList.Columns.Add("线程数量", 70);
            this.processList.Columns.Add("文件位置", 300);
            this._adapter.SendAsyncMessage(MessageHead.S_SYSTEM_GET_SYSTEMINFO);
            this.GetSystemInfos();
        }

        [PacketHandler(MessageHead.C_SYSTEM_SYSTEMINFO)]
        public void HandlerProcessList(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<SystemInfoPack>();
            systemInfoList.Items.Clear();
            foreach (var item in pack.SystemInfos)
            {
                var lv = new ListViewItem();
                lv.Text = item.ItemName;
                lv.SubItems.Add(item.Value);
                systemInfoList.Items.Add(lv);
            }
        }

        [PacketHandler(MessageHead.C_SYSTEM_OCCUPY_INFO)]
        public void HandlerOccupy(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<SystemOccupyPack>();
            this.cpuUse.Text = $"CPU使用率:{pack.CpuUsage}";
            this.moryUse.Text = $"内存:{pack.MemoryUsage}";
        }

        [PacketHandler(MessageHead.C_SYSTEM_PROCESS_LIST)]
        public void ProcessItemHandler(SessionHandler session)
        {
            var processLst = session.CompletedBuffer.GetMessageEntity<ProcessListPack>().ProcessList.ToList();

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
                    item.Update(process.ProcessId, process.ProcessName, process.WindowName, process.WindowHandler, process.ProcessMemorySize, process.ProcessThreadCount, process.FilePath);
                }
                else
                    waitRemoveItems.Enqueue(item);
            }

            foreach (var item in processLst)
            {

                if (waitRemoveItems.Count > 0)
                {
                    var processItem = waitRemoveItems.Dequeue();
                    processItem.Update(item.ProcessId, item.ProcessName, item.WindowName, item.WindowHandler, item.ProcessMemorySize, item.ProcessThreadCount, item.FilePath);
                }
                else
                {
                    var processItem = new ProcListViewItem(item.ProcessId, item.ProcessName, item.WindowName, item.WindowHandler, item.ProcessMemorySize, item.ProcessThreadCount, item.FilePath);
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
            _handlerBinder.Dispose();
            _adapter.WindowClosed = true;
            _adapter.SendAsyncMessage(MessageHead.S_GLOBAL_ONCLOSE);
        }

        private void GetSystemInfos()
        {
            _adapter.SendAsyncMessage(MessageHead.S_SYSTEM_GET_OCCUPY);
            _adapter.SendAsyncMessage(MessageHead.S_SYSTEM_GET_PROCESS_LIST);
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

            _adapter.SendAsyncMessage(MessageHead.S_SYSTEM_KILL,
                new SysKillPack()
                {
                    ProcessIds = ids.ToArray()
                });

            this._adapter.SendAsyncMessage(MessageHead.S_SYSTEM_GET_PROCESS_LIST);

        }
        private void button3_Click(object sender, EventArgs e)
        {
            this.SetWindowState(1);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            this.SetWindowState(0);
        }

        private void SetWindowState(int state)
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

            _adapter.SendAsyncMessage(MessageHead.S_SYSTEM_MAXIMIZE,
                new SysWindowMaxPack()
                {
                    State = state,
                    Handlers = handlers.ToArray()
                });
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
    }
}