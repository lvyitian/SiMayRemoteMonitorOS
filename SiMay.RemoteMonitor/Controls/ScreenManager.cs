using SiMay.Core;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.RemoteMonitor.Attributes;
using SiMay.RemoteMonitor.ControlSource;
using SiMay.RemoteMonitor.Extensions;
using SiMay.RemoteMonitor.Notify;
using static SiMay.Serialize.PacketSerializeHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static SiMay.RemoteMonitor.Win32Api;
using SiMay.Core.Packets;
using SiMay.Core.Enums;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Basic;
using SiMay.Core.Packets.Screen;
using SiMay.RemoteMonitor.MainForm;

namespace SiMay.RemoteMonitor.Controls
{
    [ControlApp(0, "远程桌面", "RemoteDesktopJob", "ScreenManager")]
    public partial class ScreenManager : Form, IControlSource
    {
        private const Int32 IDM_SCREENMON = 1000;
        private const Int32 IDM_FULL_SCREEN = 1001;
        private const Int32 IDM_KEYMOUSE_CTRL = 1002;
        private const Int32 IDM_LOCK_MOUSEKEY = 1003;
        private const Int32 IDM_BLACKSCREEN = 1004;
        private const Int32 IDM_SAVESCREEN = 1005;
        private const Int32 IDM_FULL_DIFFER = 1006;
        private const Int32 IDM_DIFFER = 1007;
        private const Int32 IDM_1X = 1008;
        private const Int32 IDM_4X = 1009;
        private const Int32 IDM_16X = 1010;
        private const Int32 IDM_Qty = 1011;
        private const Int32 IDM_SET_CLIPBOARD = 1012;
        private const Int32 IDM_GET_CLIPBOARD = 1013;

        private bool _islockMkey = false;
        private int _recvImgCount = 0;
        private bool _isControl = false;
        private long _traffic = 0;
        private int _ctrlMode = 0;//0=全屏控制1=原始比例
        private string _title = "//远程桌面【右键更多选项】 #Name# 帧率 {0}/秒 总流量 {1} KB";

        //当前图像高宽
        private int _currentImageHeight = 0;
        private int _currentImageWidth = 0;

        //原始图像高宽
        private int _srcImageHeight = 0;
        private int _srcImageWidth = 0;
        private bool _continueTask = true;

        private Bitmap _image;
        private Timer _timer;
        private MessageAdapter _adapter;
        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        public ScreenManager(MessageAdapter adapter)
        {
            adapter.Session.Socket.NoDelay = false;

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
                    if (_adapter.WindowClosed)
                        return;

                    this._handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
                    break;
                case SessionNotifyType.OnReceive:
                    break;
                case SessionNotifyType.ContinueTask:
                    _continueTask = true;
                    _timer.Start();
                    break;
                case SessionNotifyType.SessionClosed:
                    _timer.Stop();
                    this.Text = _title.FormatTo(0, (_traffic / (float)1024).ToString("0.00")) + " [" + _adapter.TipText + "]";
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

        private void ScreenSpyForm_Load(object sender, EventArgs e)
        {
            this.Text = string.Format(_title, "0", "0.0");

            this.Init();

            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_continueTask)
            {
                this.Text = string.Format(_title, _recvImgCount.ToString(), (_traffic / (float)1024).ToString("0.00"));
                _recvImgCount = 0;
            }
        }

        private void Init()
        {
            IntPtr sysMenuHandle = GetSystemMenu(this.Handle, false);

            var index = 7;
            InsertMenu(sysMenuHandle, index++, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_SCREENMON, "原始分辨率");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_FULL_SCREEN, "全屏幕监视");
            InsertMenu(sysMenuHandle, index++, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_KEYMOUSE_CTRL, "鼠标控制");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_LOCK_MOUSEKEY, "锁定鼠键");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_BLACKSCREEN, "屏幕黑屏");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_SAVESCREEN, "保存快照");
            InsertMenu(sysMenuHandle, index++, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_SET_CLIPBOARD, "设置剪切板");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_GET_CLIPBOARD, "获取剪切板");
            InsertMenu(sysMenuHandle, index++, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_FULL_DIFFER, "差异扫描");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_DIFFER, "逐行扫描");
            InsertMenu(sysMenuHandle, index++, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_1X, "1位黑白");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_4X, "4位彩色");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_16X, "16位高彩");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_Qty, "质量设置");

            CheckMenuItem(sysMenuHandle, IDM_FULL_SCREEN, MF_CHECKED);
            CheckMenuItem(sysMenuHandle, IDM_FULL_DIFFER, MF_CHECKED);
            CheckMenuItem(sysMenuHandle, IDM_16X, MF_CHECKED);

            //EnableMenuItem(sysMenuHandle, IDM_KEYMOUSE_CTRL, MF_DISABLED);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND)
            {
                IntPtr sysMenuHandle = GetSystemMenu(m.HWnd, false);
                switch (m.WParam.ToInt32())
                {
                    case IDM_SCREENMON:

                        if (_ctrlMode == 1) return;

                        CheckMenuItem(sysMenuHandle, IDM_SCREENMON, MF_CHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_FULL_SCREEN, MF_UNCHECKED);

                        EnableMenuItem(sysMenuHandle, IDM_KEYMOUSE_CTRL, MF_ENABLED);

                        this.imgDesktop.Dock = DockStyle.None;
                        this.imgDesktop.SizeMode = PictureBoxSizeMode.AutoSize;

                        this._currentImageHeight = this._srcImageHeight;
                        this._currentImageWidth = this._srcImageWidth;

                        this._image = new Bitmap(this._currentImageWidth, this._currentImageHeight);
                        _ctrlMode = 1;
                        break;
                    case IDM_FULL_SCREEN:
                        if (_ctrlMode == 0) return;

                        CheckMenuItem(sysMenuHandle, IDM_FULL_SCREEN, MF_CHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_SCREENMON, MF_UNCHECKED);

                        CheckMenuItem(sysMenuHandle, IDM_KEYMOUSE_CTRL, MF_UNCHECKED);
                        //EnableMenuItem(sysMenuHandle, IDM_KEYMOUSE_CTRL, MF_DISABLED);

                        this.imgDesktop.Dock = DockStyle.Fill;
                        this.imgDesktop.SizeMode = PictureBoxSizeMode.StretchImage;

                        this._currentImageHeight = this.imgDesktop.Height;
                        this._currentImageWidth = this.imgDesktop.Width;
                        this._image = new Bitmap(this._currentImageWidth, this._currentImageHeight);
                        _ctrlMode = 0;
                        break;
                    case IDM_KEYMOUSE_CTRL:
                        if (_isControl == true)
                        {
                            CheckMenuItem(sysMenuHandle, IDM_KEYMOUSE_CTRL, MF_UNCHECKED);
                            _isControl = false;
                        }
                        else
                        {
                            CheckMenuItem(sysMenuHandle, IDM_KEYMOUSE_CTRL, MF_CHECKED);
                            _isControl = true;
                        }
                        break;
                    case IDM_LOCK_MOUSEKEY:
                        if (_islockMkey == false)
                        {
                            _adapter.SendAsyncMessage(MessageHead.S_SCREEN_MOUSEBLOCK, new byte[] { 10 });
                            _islockMkey = true;
                            CheckMenuItem(sysMenuHandle, IDM_LOCK_MOUSEKEY, MF_CHECKED);
                        }
                        else
                        {
                            _adapter.SendAsyncMessage(MessageHead.S_SCREEN_MOUSEBLOCK, new byte[] { 11 });
                            _islockMkey = false;
                            CheckMenuItem(sysMenuHandle, IDM_LOCK_MOUSEKEY, MF_UNCHECKED);
                        }
                        break;
                    case IDM_BLACKSCREEN:
                        _adapter.SendAsyncMessage(MessageHead.S_SCREEN_BLACKSCREEN);

                        break;
                    case IDM_SAVESCREEN:
                        string fileName = DateTime.Now.ToFileTime().ToString() + " 远程桌面快照.bmp";
                        Image img = imgDesktop.Image;
                        img.Save(fileName);

                        MessageBoxHelper.ShowBoxExclamation("快照已保存到:" + Application.StartupPath + "\\" + fileName);
                        break;
                    case IDM_FULL_DIFFER:

                        CheckMenuItem(sysMenuHandle, IDM_FULL_DIFFER, MF_CHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_DIFFER, MF_UNCHECKED);

                        _adapter.SendAsyncMessage(MessageHead.S_SCREEN_CHANGESCANMODE, new byte[] { 1 });

                        break;
                    case IDM_DIFFER:

                        CheckMenuItem(sysMenuHandle, IDM_FULL_DIFFER, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_DIFFER, MF_CHECKED);

                        _adapter.SendAsyncMessage(MessageHead.S_SCREEN_CHANGESCANMODE, new byte[] { 0 });

                        break;
                    case IDM_1X:

                        CheckMenuItem(sysMenuHandle, IDM_1X, MF_CHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_4X, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_16X, MF_UNCHECKED);

                        _adapter.SendAsyncMessage(MessageHead.S_SCREEN_RESET, new byte[] { 1 });

                        break;
                    case IDM_4X:

                        CheckMenuItem(sysMenuHandle, IDM_1X, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_4X, MF_CHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_16X, MF_UNCHECKED);

                        _adapter.SendAsyncMessage(MessageHead.S_SCREEN_RESET, new byte[] { 4 });

                        break;
                    case IDM_16X:

                        CheckMenuItem(sysMenuHandle, IDM_1X, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_4X, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_16X, MF_CHECKED);

                        _adapter.SendAsyncMessage(MessageHead.S_SCREEN_RESET, new byte[] { 16 });
                        break;
                    case IDM_Qty:
                        var dlg = new ScreenQtyForm();
                        if (dlg.ShowDialog() == DialogResult.OK)
                            _adapter.SendAsyncMessage(MessageHead.S_SCREEN_SETQTY, new ScreenSetQtyPack()
                            {
                                Quality = dlg.QualityValue
                            });
                        break;
                    case IDM_SET_CLIPBOARD:
                        using (var setClipoardDlg = new EnterForm())
                        {
                            setClipoardDlg.Caption = "设置的剪切板内容:";
                            if (setClipoardDlg.ShowDialog() == DialogResult.OK)
                            {
                                var text = setClipoardDlg.Value;
                                _adapter.SendAsyncMessage(MessageHead.S_SCREEN_SET_CLIPBOARD_TEXT,
                                    new ScreenSetClipoardPack()
                                    {
                                        Text = text
                                    });
                            }
                        }
                        break;
                    case IDM_GET_CLIPBOARD:
                        _adapter.SendAsyncMessage(MessageHead.S_SCREEN_GET_CLIPOARD_TEXT);
                        break;
                }
            }

            //适应屏幕
            if (_ctrlMode == 0)
            {
                if (_currentImageWidth != this.imgDesktop.Width || _currentImageHeight != this.imgDesktop.Height)
                {
                    _currentImageWidth = this.imgDesktop.Width;
                    _currentImageHeight = this.imgDesktop.Height;

                    //最小化窗体时，控件大小==0
                    if (_currentImageWidth == 0 && _currentImageHeight == 0)
                        return;

                    _image = new Bitmap(_currentImageWidth, _currentImageHeight);
                }
            }

            base.WndProc(ref m);
        }

        [PacketHandler(MessageHead.C_SCREEN_BITINFO)]
        public void SetBitmapHandler(SessionHandler session)
        {
            var bitinfo = session.CompletedBuffer.GetMessageEntity<ScreenInitBitPack>();
            //原始图像尺寸
            this._srcImageWidth = bitinfo.Width;
            this._srcImageHeight = bitinfo.Height;

            if (_ctrlMode == 0)
            {
                _currentImageHeight = this.imgDesktop.Height;
                _currentImageWidth = this.imgDesktop.Width;
            }
            else
            {
                _currentImageWidth = this._srcImageWidth;
                _currentImageHeight = this._srcImageHeight;
            }

            _image = new Bitmap(_currentImageWidth, _currentImageHeight);

            Graphics g = Graphics.FromImage(_image);
            g.Clear(Color.Black);
            g.DrawString("桌面加载中...", new Font("微软雅黑", 15, FontStyle.Regular), new SolidBrush(Color.Red), new Point((bitinfo.Width / 2) - 40, bitinfo.Height / 2));
            g.Dispose();

            var rect = SerializePacket(new ScreenHotRectanglePack()
            {
                X = Math.Abs(this.imgDesktop.Left),
                Y = Math.Abs(this.imgDesktop.Top),
                Height = this.ClientSize.Height,
                Width = this.ClientSize.Width,
                CtrlMode = _ctrlMode
            });
            //第一帧不计入连续帧
            for (int i = 0; i < 3; i++)
            {
                _adapter.SendAsyncMessage(MessageHead.S_SCREEN_NEXT_SCREENBITMP, rect);
            }
        }


        //已接收帧数
        int _frameCount = 0;

        private void GetNextScreen()
        {
            if (_adapter.WindowClosed) return;

            _frameCount++;
            //Console.WriteLine(this.imgDesktop.Height + " | " + Width + "|" + this.Height + " | " + this.Width);

            if (_frameCount == 1)//使帧数更连续
            {
                var rect = SerializePacket(new ScreenHotRectanglePack()
                {
                    X = Math.Abs(this.imgDesktop.Left),
                    Y = Math.Abs(this.imgDesktop.Top),
                    Height = this.ClientSize.Height,
                    Width = this.ClientSize.Width,
                    CtrlMode = _ctrlMode
                });
                for (int i = 0; i < 3; i++)
                    _adapter.SendAsyncMessage(MessageHead.S_SCREEN_NEXT_SCREENBITMP, rect);
            }
            else if (_frameCount == 3)
                _frameCount = 0;
        }

        [PacketHandler(MessageHead.C_SCREEN_DIFFBITMAP)]
        public void FullFragmentHandler(SessionHandler session)
        {
            int dataSize = session.CompletedBuffer.Length;
            var fragments = session.CompletedBuffer.GetMessageEntity<ScreenFragmentPack>();
            foreach (var fragment in fragments.Fragments)
            {
                using (MemoryStream ms = new MemoryStream(fragment.FragmentData))
                    this.DisplayScreen(Image.FromStream(ms), new Rectangle(fragment.X, fragment.Y, fragment.Width, fragment.Height));
            }
            _traffic += dataSize;

            _recvImgCount++;

            this.GetNextScreen();
        }

        [PacketHandler(MessageHead.C_SCREEN_BITMP)]
        public void SigleFragmentHandler(SessionHandler session)
        {
            int dataSize = session.CompletedBuffer.Length;
            var fragments = session.CompletedBuffer.GetMessageEntity<ScreenFragmentPack>();

            foreach (var fragment in fragments.Fragments)
            {
                using (MemoryStream ms = new MemoryStream(fragment.FragmentData))
                    this.DisplayScreen(Image.FromStream(ms), new Rectangle(fragment.X, fragment.Y, fragment.Width, fragment.Height));
            }

            _traffic += dataSize;
        }
        [PacketHandler(MessageHead.C_SCREEN_SCANCOMPLETE)]
        public void ScanFinishHandler(SessionHandler session)
        {
            _recvImgCount++;
            this.GetNextScreen();
        }

        [PacketHandler(MessageHead.C_SCREEN_CLIPOARD_TEXT)]
        public void GetClipoardValueHandler(SessionHandler session)
        {
            var response = session.CompletedBuffer.GetMessageEntity<ScreenClipoardValuePack>();
            Clipboard.SetText(response.Value);
            MessageBoxHelper.ShowBoxExclamation("已获取至剪切板!");
        }

        private void DisplayScreen(Image bit, Rectangle rect)
        {
            if (_adapter.WindowClosed) return;

            Graphics g = Graphics.FromImage(_image);
            g.DrawImage(bit, rect);
            g.Dispose();
            bit.Dispose();
            imgDesktop.Image = _image;
        }

        private void ScreenSpyForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_isControl) return;

            _adapter.SendAsyncMessage(MessageHead.S_SCREEN_MOUSEKEYEVENT,
                new ScreenMKeyPack()
                {
                    Key = MOUSEKEY_ENUM.KeyDown,
                    Point1 = e.KeyValue,
                    Point2 = 0
                });
        }

        private void ScreenSpyForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (!_isControl) return;

            _adapter.SendAsyncMessage(MessageHead.S_SCREEN_MOUSEKEYEVENT,
                new ScreenMKeyPack()
                {
                    Key = MOUSEKEY_ENUM.KeyUp,
                    Point1 = e.KeyValue,
                    Point2 = 0
                });
        }

        private void desktopImg_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_isControl) return;


            int x = e.X;
            int y = e.Y;

            if (_ctrlMode == 0)
            {
                x = (int)(e.X / ((float)this.imgDesktop.Width / (float)this._srcImageWidth));
                y = (int)(e.Y / ((float)this.imgDesktop.Height / (float)this._srcImageHeight));
            }

            switch (e.Button)
            {
                case MouseButtons.Left:
                    _adapter.SendAsyncMessage(MessageHead.S_SCREEN_MOUSEKEYEVENT,
                        new ScreenMKeyPack()
                        {
                            Key = MOUSEKEY_ENUM.LeftDown,
                            Point1 = x,
                            Point2 = y
                        });
                    break;

                case MouseButtons.Middle:

                    _adapter.SendAsyncMessage(MessageHead.S_SCREEN_MOUSEKEYEVENT,
                        new ScreenMKeyPack()
                        {
                            Key = MOUSEKEY_ENUM.MiddleDown,
                            Point1 = x,
                            Point2 = y
                        });
                    break;

                case MouseButtons.None:
                    break;

                case MouseButtons.Right:
                    _adapter.SendAsyncMessage(MessageHead.S_SCREEN_MOUSEKEYEVENT,
                        new ScreenMKeyPack()
                        {
                            Key = MOUSEKEY_ENUM.RightDown,
                            Point1 = x,
                            Point2 = y
                        });
                    break;

                case MouseButtons.XButton1:
                    break;

                case MouseButtons.XButton2:
                    break;

                default:
                    break;
            }
        }

        private void desktopImg_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_isControl) return;

            int x = e.X;
            int y = e.Y;

            if (_ctrlMode == 0)
            {
                x = (int)(e.X / ((float)this.imgDesktop.Width / (float)this._srcImageWidth));
                y = (int)(e.Y / ((float)this.imgDesktop.Height / (float)this._srcImageHeight));
            }

            switch (e.Button)
            {
                case MouseButtons.Left:
                    _adapter.SendAsyncMessage(MessageHead.S_SCREEN_MOUSEKEYEVENT,
                        new ScreenMKeyPack()
                        {
                            Key = MOUSEKEY_ENUM.LeftUp,
                            Point1 = x,
                            Point2 = y
                        });
                    break;

                case MouseButtons.Middle:
                    _adapter.SendAsyncMessage(MessageHead.S_SCREEN_MOUSEKEYEVENT,
                        new ScreenMKeyPack()
                        {
                            Key = MOUSEKEY_ENUM.MiddleUp,
                            Point1 = x,
                            Point2 = y
                        });
                    break;

                case MouseButtons.None:
                    break;

                case MouseButtons.Right:
                    _adapter.SendAsyncMessage(MessageHead.S_SCREEN_MOUSEKEYEVENT,
                        new ScreenMKeyPack()
                        {
                            Key = MOUSEKEY_ENUM.RightUp,
                            Point1 = x,
                            Point2 = y
                        });
                    break;

                case MouseButtons.XButton1:
                    break;

                case MouseButtons.XButton2:
                    break;

                default:
                    break;
            }
        }

        private void ScreenSpyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timer.Stop();
            _timer.Dispose();
            _adapter.WindowClosed = true;
            _handlerBinder.Dispose();
            _adapter.SendAsyncMessage(MessageHead.S_GLOBAL_ONCLOSE);
        }

        private void m_desktop_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isControl) return;

            int x = e.X;
            int y = e.Y;

            if (_ctrlMode == 0)
            {
                x = (int)(e.X / ((float)this.imgDesktop.Width / (float)this._srcImageWidth));
                y = (int)(e.Y / ((float)this.imgDesktop.Height / (float)this._srcImageHeight));
            }
            _adapter.SendAsyncMessage(MessageHead.S_SCREEN_MOUSEKEYEVENT,
                new ScreenMKeyPack()
                {
                    Key = MOUSEKEY_ENUM.Move,
                    Point1 = x,
                    Point2 = y
                });

        }
        private void ScreenManager_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!_isControl) return;

            _adapter.SendAsyncMessage(MessageHead.S_SCREEN_MOUSEKEYEVENT,
                new ScreenMKeyPack()
                {
                    Key = MOUSEKEY_ENUM.Wheel,
                    Point1 = e.Delta,
                    Point2 = 0
                });
        }
    }
}