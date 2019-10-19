using SiMay.Net.SessionProvider.Core;
using SiMay.Net.SessionProviderService.OnChannelListViewItem;
using SiMay.Net.SessionProviderService.Properties;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Server;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SiMay.Net.SessionProvider.Core.Win32Api;

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

        float _uploadTransferBytes;
        float _receiveTransferBytes;

        ImageList _log_imgList;

        List<TcpChannelContext> _channelContexts = new List<TcpChannelContext>();
        TcpSocketSaeaServer _server;

        TcpChannelContext _manager_channel;
        int _managerchannel_login = 0;

        int _connectionCount = 0;
        private void SessionProviderService_Load(object sender, EventArgs e)
        {

            this.Text = "SiMay中间会话服务-IOASJHD BEAT " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            IntPtr sysMenuHandle = GetSystemMenu(this.Handle, false);

            InsertMenu(sysMenuHandle, 7, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, 8, MF_BYPOSITION, IDM_OPTIONS, "系统设置");
            InsertMenu(sysMenuHandle, 9, MF_BYPOSITION, IDM_RUNLOG, "运行日志");

            _log_imgList = new ImageList();
            _log_imgList.Images.Add("ok", Resources.ok);
            _log_imgList.Images.Add("err", Resources.erro);

            logList.SmallImageList = _log_imgList;

            string address = ApplicationConfiguration.IPAddress;
            string port = ApplicationConfiguration.Port;

            this.lableIPAddress.Text = address;
            this.labelPort.Text = port;
            this.lableStatrTime.Text = DateTime.Now.ToString();
            var ipe = new IPEndPoint(IPAddress.Parse(address), int.Parse(port));


            var serverConfig = new TcpSocketSaeaServerConfiguration();
            serverConfig.ReuseAddress = false;
            serverConfig.KeepAlive = true;
            serverConfig.KeepAliveInterval = 5000;
            serverConfig.KeepAliveSpanTime = 1000;
            serverConfig.PendingConnectionBacklog = int.Parse(ApplicationConfiguration.Backlog);

            _server = TcpSocketsFactory.CreateServerAgent(TcpSocketSaeaSessionType.Full, serverConfig, (notify, session) =>
            {
                switch (notify)
                {
                    case TcpSocketCompletionNotify.OnConnected:
                        _connectionCount++;

                        this.Invoke(new Action(() =>
                        {
                            this.lableConnectionCount.Text = _connectionCount.ToString();
                        }));

                        //创建通道上下文，等待确认通道类型
                        TcpChannelContext context = new TcpChannelContext(session);
                        context.OnChannelTypeNotify += Context_TcpAwaitnotifyProc;

                        _channelContexts.Add(context);
                        break;
                    case TcpSocketCompletionNotify.OnSend:
                        this._uploadTransferBytes += session.SendTransferredBytes;
                        break;
                    case TcpSocketCompletionNotify.OnDataReceiveing:
                        this._receiveTransferBytes += session.ReceiveBytesTransferred;

                        ((TcpChannelContext)session.AppTokens[0]).OnMessage(session);
                        break;
                    case TcpSocketCompletionNotify.OnClosed:
                        _connectionCount--;

                        this.Invoke(new Action(() =>
                        {
                            this.lableConnectionCount.Text = _connectionCount.ToString();
                        }));

                        this.SessionClosed(session);
                        break;
                    default:
                        break;
                }

            });
            try
            {
                _server.Listen(ipe);
                LogShowQueueHelper.WriteLog("SiMay中间会话服务端口" + port + "启动成功!");
            }
            catch
            {
                LogShowQueueHelper.WriteLog("SiMay中间会话服务端口" + port + "启动失败!", "err");
            }

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 100;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            System.Timers.Timer flow_timer = new System.Timers.Timer();
            flow_timer.Interval = 1000;
            flow_timer.Elapsed += Flow_timer_Elapsed;
            flow_timer.Start();
        }

        private void Flow_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                this.labelUpload.Text = (this._uploadTransferBytes / 1024).ToString("0.00");
                this.labelReceive.Text = (this._receiveTransferBytes / 1024).ToString("0.00");

                this._receiveTransferBytes = 0;
                this._uploadTransferBytes = 0;
            }));
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            while (LogShowQueueHelper.LogQueue.Count != 0)
            {
                if (LogShowQueueHelper.LogQueue.Count > 0)
                {
                    Log log = LogShowQueueHelper.LogQueue.Dequeue();



                    this.Invoke(new Action(() =>
                       {
                           WriteRuninglog(log.log, log.Success == 0 ? "ok" : "err");
                       }));
                }
            }
        }
        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="key"></param>
        private void WriteRuninglog(string log, string key = "ok")
        {
            ListViewItem lv = new ListViewItem();
            lv.ImageKey = key;
            lv.Text = DateTime.Now.ToString();
            lv.SubItems.Add(log);

            LogHelper.WriteLog(log, "OnRun.log");

            if (logList.Items.Count >= 1)
                logList.Items.Insert(1, lv);
            else
                logList.Items.Insert(0, lv);
        }
        /// <summary>
        /// 清除日志
        /// </summary>
        private void Clearlogs()
        {
            int i = 0;
            foreach (ListViewItem item in logList.Items)
            {
                i++;
                if (i > 1)
                    item.Remove();
            }
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND)
            {
                IntPtr sysMenuHandle = GetSystemMenu(m.HWnd, false);
                switch (m.WParam.ToInt64())
                {
                    case IDM_OPTIONS:
                        AppOptionsDialog dlg = new AppOptionsDialog();
                        dlg.ShowDialog();
                        break;
                    case IDM_RUNLOG:
                        if (File.Exists("OnRun.log"))
                        {
                            Process.Start("OnRun.log");
                        }
                        break;
                }
            }
            base.WndProc(ref m);
        }
        private void SessionClosed(TcpSocketSaeaSession session)
        {
            var context = ((TcpChannelContext)session.AppTokens[0]);
            context.OnClosed();
            context.OnChannelTypeNotify -= Context_TcpAwaitnotifyProc;
            context.OnManagerChannelMessage -= Context_OnManagerChannelMessage;
            context.OnMainChannelMessage -= Context_OnMainChannelMessage;

            _channelContexts.Remove(context);

            LogShowQueueHelper.WriteLog("有连接断开,工作类型:" + context.ChannelType, "err");

            if (context.ChannelType == TcpChannelContextServiceType.ManagerChannel)
            {
                if (this._managerchannel_login > 0)
                    Interlocked.Decrement(ref _managerchannel_login);
                else
                    _manager_channel = null;

            }
            else if (context.ChannelType == TcpChannelContextServiceType.MainChannel)
            {
                byte[] data = BitConverter.GetBytes(context.RemoteId);

                if (this._managerchannel_login > 0)
                    SendMessage(_manager_channel, MessageHelper.CommandCopyTo(MsgCommand.Msg_Close_Session, data));
            }

            if (context.ChannelType != TcpChannelContextServiceType.None)
            {
                this.RemoveChannelListViewItem(context);
            }
        }

        private void RemoveChannelListViewItem(TcpChannelContext context)
        {
            this.BeginInvoke(new Action(() =>
            {
                for (int i = 0; i < this.channelListView.Items.Count; i++)
                {
                    ChannelListViewItem lv = this.channelListView.Items[i] as ChannelListViewItem;
                    if (lv.TcpChannelContext.Id == context.Id)
                    {
                        this.channelListView.Items.RemoveAt(i);
                        break;
                    }
                }
            }));
        }

        private void AddChannelListViewItem(TcpChannelContext context)
        {
            this.BeginInvoke(new Action(() =>
            {
                ChannelListViewItem item = new ChannelListViewItem(context);
                item.Text = DateTime.Now.ToString();
                item.SubItems.Add(context.ChannelType.ToString());
                item.SubItems.Add("0.00/0.00");
                this.channelListView.Items.Add(item);
            }));
        }

        private void Context_TcpAwaitnotifyProc(TcpChannelContext context, TcpChannelContextServiceType type)
        {
            this.AddChannelListViewItem(context);

            LogShowQueueHelper.WriteLog("连接被确认,工作类型:" + type.ToString());

            switch (type)
            {
                case TcpChannelContextServiceType.MainChannel:
                    this.MainChannelProcess(context);
                    break;
                case TcpChannelContextServiceType.WorkChannel:

                    //通知发起工作连接
                    if (this._managerchannel_login > 0)
                        SendMessage(_manager_channel, MessageHelper.CommandCopyTo(MsgCommand.Msg_Connect_Work));
                    else
                    {
                        context.Session.Close(true);
                    }

                    break;
                case TcpChannelContextServiceType.ManagerChannel:
                    this.ManagerChannelProcess(context);
                    break;
                case TcpChannelContextServiceType.ManagerWorkChannel:

                    this.JoinWorkChannelContext(context);
                    break;
                default:
                    LogShowQueueHelper.WriteLog("未确认的连接:" + type.ToString());
                    break;
            }
        }

        private void MainChannelProcess(TcpChannelContext context)
        {
            if (this._managerchannel_login > 0)
            {
                //发送新上线会话的ID
                byte[] data = BitConverter.GetBytes(context.Id);
                SendMessage(_manager_channel, MessageHelper.CommandCopyTo(MsgCommand.Msg_Set_Session, data));
            }

            context.OnMainChannelMessage += Context_OnMainChannelMessage;
        }

        private void ManagerChannelProcess(TcpChannelContext context)
        {
            //登出管理连接
            if (_managerchannel_login > 0)
            {
                //close所有mainContext
                //while (this._channelContexts.Count < 1)
                //{
                //    for (int i = 0; i < this._channelContexts.Count; i++)
                //    {

                //    }
                //    Thread.Sleep(10);
                //}


                byte[] body = MessageHelper.CommandCopyTo(MsgCommand.Msg_LogOut);
                SendMessage(_manager_channel, body);
                LogShowQueueHelper.WriteLog("ManagerChannel LogOut");
            }

            Interlocked.Increment(ref _managerchannel_login);

            if (_manager_channel != null)
            {
                _manager_channel.OnManagerChannelMessage -= Context_OnManagerChannelMessage;
                _manager_channel.OnChannelTypeNotify -= Context_TcpAwaitnotifyProc;
            }

            _manager_channel = context;
            context.OnManagerChannelMessage += Context_OnManagerChannelMessage;
        }

        private void Context_OnManagerChannelMessage(TcpChannelContext context, byte[] data)
        {

            MsgCommand msg = (MsgCommand)data[0];
            switch (msg)
            {
                case MsgCommand.Msg_Pull_Session:
                    this.ProcessPullMainChannel(context);
                    break;
                case MsgCommand.Msg_Set_Session_Id:
                    this.SetSessionId(context, data);
                    break;
                case MsgCommand.Msg_Close_Session:
                    this.CloseSession(data);
                    break;
                case MsgCommand.Msg_MessageData:
                    this.ProcessSendMessage(data);
                    break;
                default:
                    break;
            }
        }

        private void Context_OnMainChannelMessage(TcpChannelContext context, byte[] data)
        {
            if (this._managerchannel_login == 0) return;

            byte[] bytes = new byte[sizeof(Int64) + data.Length];
            BitConverter.GetBytes(context.RemoteId).CopyTo(bytes, 0);
            data.CopyTo(bytes, sizeof(Int64));

            //MainChannel的数据封装代理协议发送出去
            if (this._managerchannel_login > 0)
                SendMessage(_manager_channel, MessageHelper.CommandCopyTo(MsgCommand.Msg_MessageData, bytes));
        }

        private void ProcessSendMessage(byte[] data)
        {
            //byte[] body = new byte[data.Length - 1];
            //Array.Copy(data, 1, body, 0, body.Length);

            long id = BitConverter.ToInt64(data, 1);

            Console.WriteLine("ProcessSendMessage:" + id);
            GCHandle gc = GCHandle.FromIntPtr(new IntPtr(id));
            Console.WriteLine("ProcessSendMessage OK");
            TcpChannelContext context = gc.Target as TcpChannelContext;

            if (context == null)
                return;

            //将消息转发给MainChannel
            context.SendMessage(data, sizeof(Int64) + 1, data.Length - sizeof(Int64) - 1);
        }

        private void CloseSession(byte[] data)
        {
            long id = BitConverter.ToInt64(data, 1);
            Console.WriteLine("CloseSession:" + id);
            GCHandle gc = GCHandle.FromIntPtr(new IntPtr(id));
            Console.WriteLine("CloseSession OK");
            TcpChannelContext target_context = gc.Target as TcpChannelContext;

            if (target_context == null)
                return;

            target_context.Session.Close(true);

        }

        private void SetSessionId(TcpChannelContext context, byte[] data)
        {
            byte[] body = new byte[data.Length - 1];
            Array.Copy(data, 1, body, 0, body.Length);

            try
            {
                byte[] sessionItems = new byte[sizeof(Int64) * 2];
                for (int i = 0; i < body.Length / (sizeof(Int64) * 2); i++)
                {
                    Array.Copy(body, i * sessionItems.Length, sessionItems, 0, sessionItems.Length);
                    long id = BitConverter.ToInt64(sessionItems, 0);

                    LogShowQueueHelper.WriteLog("DEBUG SetSessionId:" + id);

                    GCHandle gc = GCHandle.FromIntPtr(new IntPtr(id));

                    LogShowQueueHelper.WriteLog("DEBUG SetSessionId OK");

                    TcpChannelContext target_context = gc.Target as TcpChannelContext;
                    id = BitConverter.ToInt64(sessionItems, sizeof(Int64));

                    LogShowQueueHelper.WriteLog("DEBUG SetSessionId RmoteId:" + id);

                    //关联控制端SessionId
                    target_context.RemoteId = id;

                    byte[] ack = target_context.AckPack;

                    byte[] ackPack = new byte[ack.Length + sizeof(Int64)];
                    BitConverter.GetBytes(id).CopyTo(ackPack, 0);
                    ack.CopyTo(ackPack, sizeof(Int64));

                    //发送应用层连接确认报(连接密码等信息)
                    if (this._managerchannel_login > 0)
                        SendMessage(_manager_channel, MessageHelper.CommandCopyTo(MsgCommand.Msg_MessageData, ackPack));
                }

                if (this._managerchannel_login > 0)
                    LogShowQueueHelper.WriteLog("Set SessionId SessionCount:" + body.Length / (sizeof(Int64) * 2));
            }
            catch (Exception e)
            {
                LogShowQueueHelper.WriteLog("Set SessionId Exception:" + e.Message, "err");
            }


        }
        private void ProcessPullMainChannel(TcpChannelContext context)
        {
            //查找所有MainChannel
            List<TcpChannelContext> contexts = _channelContexts.Where(x => x.ChannelType == TcpChannelContextServiceType.MainChannel).ToList();
            byte[] data = new byte[contexts.Count * sizeof(Int64)];
            for (int i = 0; i < contexts.Count; i++)
            {
                BitConverter.GetBytes(contexts[i].Id).CopyTo(data, i * sizeof(Int64));
                Console.WriteLine("ProcessPullMainChannel:" + contexts[i].Id);
            }

            if (_managerchannel_login > 0)
            {
                SendMessage(_manager_channel, MessageHelper.CommandCopyTo(MsgCommand.Msg_Set_Session, data));
                LogShowQueueHelper.WriteLog("Get Session SessionCount:" + contexts.Count);
            }
        }

        private void JoinWorkChannelContext(TcpChannelContext context)
        {
            //查找没有关联过的WorkChannel
            TcpChannelContext workContext = _channelContexts.Find(x => x.ChannelType == TcpChannelContextServiceType.WorkChannel && !x.IsJoin);
            if (workContext != null)
            {
                workContext.WorkChannelJoinContext(context.Session, context.GetBuffer);

                //关联完成，释放资源
                context.OnChannelTypeNotify -= Context_TcpAwaitnotifyProc;
                context.DisposeContext();

                this._channelContexts.Remove(context);
                this.RemoveChannelListViewItem(context);
            }
            else
            {
                //没找到就断开连接,回调会释放所有资源
                context.Session.Close(true);
            }
        }

        private void SendMessage(TcpChannelContext context, byte[] data)
        {
            byte[] body = new byte[sizeof(Int32) + data.Length];
            BitConverter.GetBytes(data.Length).CopyTo(body, 0);
            data.CopyTo(body, 4);

            if (context == null)
                return;

            context.SendMessage(body, 0, body.Length);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (logList.SelectedItems.Count != 0)
            {
                Clipboard.SetText(logList.Items[logList.SelectedItems[0].Index].SubItems[1].Text);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (logList.SelectedItems.Count != 0)
            {
                int Index = logList.SelectedItems[0].Index;
                if (Index >= 1)
                    logList.Items[Index].Remove();
            }
        }

        private void 清空日志ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Clearlogs();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            var channelsListItems = channelListView.SelectedItems;
            foreach (ChannelListViewItem item in channelsListItems)
                item.TcpChannelContext.Session.Close(true);
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
            {
                e.Cancel = true;
            }
        }
    }
}
