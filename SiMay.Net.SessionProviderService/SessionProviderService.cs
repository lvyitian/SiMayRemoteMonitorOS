using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using SiMay.Basic;
using SiMay.Net.SessionProvider.Core;
using SiMay.Net.SessionProviderService.Properties;
using SiMay.Net.SessionProviderServiceCore;
using static System.Windows.Forms.ListViewItem;
using static SiMay.Net.SessionProviderService.Win32;

namespace SiMay.Net.SessionProviderService
{
    public partial class SessionProviderService : Form
    {
        private const Int32 IDM_OPTIONS = 1000;
        private const Int32 IDM_RUNLOG = 1001;

        public SessionProviderService()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND)
            {
                IntPtr sysMenuHandle = GetSystemMenu(m.HWnd, false);
                switch (m.WParam.ToInt64())
                {
                    case IDM_OPTIONS:
                        SystemOptionsDialog dlg = new SystemOptionsDialog();
                        dlg.ShowDialog();
                        break;
                }
            }
            base.WndProc(ref m);
        }

        private float _uploadTransferBytes;
        private float _receiveTransferBytes;
        private ImageList _log_imgList;
        private int _channelCount = 0;

        private MainSessionProviderService _sessionProviderService;
        private void SessionProviderService_Load(object sender, EventArgs e)
        {
            this.Text = "SiMay中间会话服务-IOASJHD " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            IntPtr sysMenuHandle = GetSystemMenu(this.Handle, false);

            InsertMenu(sysMenuHandle, 7, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, 8, MF_BYPOSITION, IDM_OPTIONS, "系统设置");

            this._log_imgList = new ImageList();
            this._log_imgList.Images.Add("ok", Resources.ok);
            this._log_imgList.Images.Add("err", Resources.erro);
            this.logList.SmallImageList = _log_imgList;

            string localAddress = ApplicationConfiguration.LoalAddress;
            int port = ApplicationConfiguration.Port;

            this.lableIPAddress.Text = localAddress;
            this.labelPort.Text = port.ToString();
            this.lableStatrTime.Text = DateTime.Now.ToString();

            this.LaunchService();
        }

        private void LaunchService()
        {
            _sessionProviderService = new MainSessionProviderService();
            _sessionProviderService.SynchronizationContext = SynchronizationContext.Current;
            _sessionProviderService.OnConnectedEventHandler += OnConnectedEventHandler;
            _sessionProviderService.OnClosedEventHandler += OnClosedEventHandler;
            _sessionProviderService.LogOutputEventHandler += LogOutputEventHandler;
            var startResult = _sessionProviderService.StartService(new StartServiceOptions()
            {
                LocalAddress = ApplicationConfiguration.LoalAddress,
                ServicePort = ApplicationConfiguration.Port,
                MaxPacketSize = 1024 * 1024 * 2,
                AccessKey = ApplicationConfiguration.AccessKey,
                MainAppAccessKey = ApplicationConfiguration.MainAppAccessKey,
                MainApplicationAllowAccessId = !ApplicationConfiguration.AccessIds.IsNullOrEmpty() ? ApplicationConfiguration.AccessIds.Split(',').Select(c => long.Parse(c)).ToArray() : new long[0],
                MainApplicationAnonyMous = ApplicationConfiguration.AnonyMous
            });
        }

        private void LogOutputEventHandler(LogOutLevelType levelType, string log) => this.Log(levelType, log);

        private void OnClosedEventHandler(TcpSessionChannelDispatcher dispatcher)
        {
            ChannelViewItem viewItem = this.channelListView.Items.FristOrDefault<ChannelViewItem>(c => c.ChannelDispatcher.Equals(dispatcher));
            this.channelListView.Items.Remove(viewItem);

            dispatcher.LogOutputEventHandler -= LogOutputEventHandler;

            this._channelCount--;
            this.lableConnectionCount.Text = _channelCount.ToString();
        }

        private void OnConnectedEventHandler(TcpSessionChannelDispatcher dispatcher)
        {
            var viewItem = new ChannelViewItem(dispatcher);
            this.channelListView.Items.Add(viewItem);
            dispatcher.LogOutputEventHandler += LogOutputEventHandler;
            this._channelCount++;
            this.lableConnectionCount.Text = _channelCount.ToString();
        }

        private void LogOutputEventHandler(DispatcherBase dispatcher, LogOutLevelType levelType, string log) => this.Log(levelType, log);

        private void Log(LogOutLevelType levelType, string log)
        {
            if (this.logList.Items.Count > 500)
                CleanViewLog();

            var viewItem = new ListViewItem();
            viewItem.Text = DateTime.Now.ToString();
            viewItem.SubItems.Add(log);
            switch (levelType)
            {
                case LogOutLevelType.Debug:
                    viewItem.ImageKey = "ok";
                    break;
                case LogOutLevelType.Warning:
                    viewItem.ImageKey = "err";
                    break;
                case LogOutLevelType.Error:
                    viewItem.ImageKey = "err";
                    break;
            }
            this.logList.Items.Add(viewItem);
            LogHelper.WriteLog($"Level:{levelType.ToString()} {log}", ApplicationConfiguration.LogFileName);
        }
        private void timerFlowCalac_Tick(object sender, EventArgs e)
        {
            foreach (ChannelViewItem item in channelListView.Items)
            {
                item.SetVelocityText(item.SendStreamLength, item.ReceiveStreamLength);
                this._uploadTransferBytes += item.SendStreamLength;
                this._receiveTransferBytes += item.ReceiveStreamLength;

                item.SendStreamLength = 0;
                item.ReceiveStreamLength = 0;
            }

            this.lbUpload.Text = (_uploadTransferBytes / 1024).ToString("0.00");
            this.lbReceive.Text = (_receiveTransferBytes / 1024).ToString("0.00");
            this._uploadTransferBytes = 0;
            this._receiveTransferBytes = 0;
        }

        private void CleanViewLog()
        {
            int i = 0;
            foreach (ListViewItem item in logList.Items)
            {
                i++;
                if (i > 1)
                    item.Remove();
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (this.logList.SelectedItems.Count != 0)
                Clipboard.SetText(string.Join(",", logList.Items[logList.SelectedItems.FristOrDefault<ListViewItem>().Index].SubItems.Select<ListViewSubItem, string>(c => c.Text)));
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (this.logList.SelectedItems.Count != 0)
            {
                int Index = logList.SelectedItems[0].Index;
                if (Index >= 1)
                    logList.Items[Index].Remove();
            }
        }

        private void 清空日志ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.CleanViewLog();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            foreach (ChannelViewItem item in channelListView.SelectedItems)
                item.ChannelDispatcher.CloseSession();
        }

        private void channelListContext_Opening(object sender, CancelEventArgs e)
        {
            if (this.channelListView.SelectedItems.Count == 0)
                this.channelListContext.Enabled = false;
            else
                this.channelListContext.Enabled = true;
        }

        private void SessionProviderService_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("服务正在运行中，确定要关闭吗?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
                e.Cancel = true;
        }
    }
}
