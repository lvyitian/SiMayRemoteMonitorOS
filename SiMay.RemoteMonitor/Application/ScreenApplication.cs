using SiMay.Core;
using SiMay.RemoteMonitor.Attributes;
using System;
using System.Drawing;
using System.IO;
using SiMay.Basic;
using System.Windows.Forms;
using SiMay.RemoteControlsCore;
using SiMay.RemoteControlsCore.Enum;
using SiMay.RemoteMonitor.MainApplication;
using SiMay.RemoteControlsCore.HandlerAdapters;
using static SiMay.RemoteMonitor.Win32Api;
using static SiMay.Serialize.Standard.PacketSerializeHelper;
using System.Diagnostics;
using Accord.Video.FFMPEG;
using System.Threading;
using System.Threading.Tasks;
using SiMay.Platform.Windows;

namespace SiMay.RemoteMonitor.Application
{
    [OnTools]
    [ApplicationName("远程桌面")]
    [AppResourceName("ScreenManager")]
    [Application(typeof(RemoteScreenAdapterHandler), AppFlageConstant.REMOTE_DESKTOP, 0)]
    public partial class ScreenApplication : Form, IApplication
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
        private const Int32 IDM_CTRL_ALT_DEL = 1014;
        private const Int32 IDM_DELETE_WALLPAPER = 1015;
        private const Int32 IDM_CHANGE_MONITOR = 1016;
        private const Int32 IDM_RECORD = 1017;

        [ApplicationAdapterHandler]
        public RemoteScreenAdapterHandler RemoteScreenAdapterHandler { get; set; }

        private bool _islockMkey = false;
        private int _recvImgCount = 0;
        private bool _isControl = false;
        private long _traffic = 0;
        private ScreenDisplayMode _screenDisplayMode = ScreenDisplayMode.Fullscreen;
        private string _title = "//远程桌面【右键更多选项】 #Name# 帧率 {0}/秒 总流量 {1} KB";

        //原始图像高宽
        private int _srcImageHeight = 1000;
        private int _srcImageWidth = 1500;
        private bool _continueTask = true;

        private int _currenMonitorIndex = 0;
        private MonitorItem[] _monitorItems;

        private Graphics _currentFrameGraphics;
        private Graphics _videoFrameGraphics;
        private Bitmap _currentFrame;
        private Bitmap _videoFrame;
        private System.Windows.Forms.Timer _timer;
        private bool _stop = true;
        private int _menuContextIndex;
        public ScreenApplication()
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
            _timer.Stop();
            this.Text = this._title.FormatTo(0, (_traffic / (float)1024).ToString("0.00")) + " [" + this.RemoteScreenAdapterHandler.StateContext.ToString() + "]";
        }

        public void ContinueTask(ApplicationAdapterHandler handler)
        {
            _continueTask = true;
            _timer.Start();
            this.RemoteScreenAdapterHandler.GetInitializeBitInfo();
        }

        private void ScreenSpyForm_Load(object sender, EventArgs e)
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
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_DELETE_WALLPAPER, "清除壁纸");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_CHANGE_MONITOR, "监视器设置");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_CTRL_ALT_DEL, "Ctrl + Alt + Del");

            _menuContextIndex = index++;
            InsertMenu(sysMenuHandle, _menuContextIndex, MF_BYPOSITION, IDM_RECORD, "开始录制");

            CheckMenuItem(sysMenuHandle, IDM_FULL_SCREEN, MF_CHECKED);
            CheckMenuItem(sysMenuHandle, IDM_FULL_DIFFER, MF_CHECKED);
            CheckMenuItem(sysMenuHandle, IDM_16X, MF_CHECKED);

            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 1000;
            _timer.Tick += Timer_Tick;
            _timer.Start();

            this.Text = string.Format(this._title = this._title.Replace("#Name#", this.RemoteScreenAdapterHandler.OriginName), "0", "0.0");
            this.RemoteScreenAdapterHandler.OnClipoardReceivedEventHandler += OnClipoardReceivedEventHandler;
            this.RemoteScreenAdapterHandler.OnServcieInitEventHandler += OnServcieInitEventHandler;
            this.RemoteScreenAdapterHandler.OnScreenFragmentEventHandler += OnScreenFragmentEventHandler;
            this.RemoteScreenAdapterHandler.GetInitializeBitInfo();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND)
            {
                IntPtr sysMenuHandle = GetSystemMenu(m.HWnd, false);
                switch (m.WParam.ToInt32())
                {
                    case IDM_SCREENMON:

                        if (_screenDisplayMode == ScreenDisplayMode.Original)
                            return;

                        CheckMenuItem(sysMenuHandle, IDM_SCREENMON, MF_CHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_FULL_SCREEN, MF_UNCHECKED);

                        EnableMenuItem(sysMenuHandle, IDM_KEYMOUSE_CTRL, MF_ENABLED);

                        this.imgDesktop.Dock = DockStyle.None;
                        this.imgDesktop.SizeMode = PictureBoxSizeMode.AutoSize;

                        _screenDisplayMode = ScreenDisplayMode.Original;
                        break;
                    case IDM_FULL_SCREEN:
                        if (_screenDisplayMode == ScreenDisplayMode.Fullscreen)
                            return;

                        CheckMenuItem(sysMenuHandle, IDM_FULL_SCREEN, MF_CHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_SCREENMON, MF_UNCHECKED);

                        //CheckMenuItem(sysMenuHandle, IDM_KEYMOUSE_CTRL, MF_UNCHECKED);
                        //EnableMenuItem(sysMenuHandle, IDM_KEYMOUSE_CTRL, MF_DISABLED);

                        this.imgDesktop.Dock = DockStyle.Fill;
                        this.imgDesktop.SizeMode = PictureBoxSizeMode.StretchImage;
                        _screenDisplayMode = ScreenDisplayMode.Fullscreen;
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
                            this.RemoteScreenAdapterHandler.RemoteMouseBlock(true);
                            _islockMkey = true;
                            CheckMenuItem(sysMenuHandle, IDM_LOCK_MOUSEKEY, MF_CHECKED);
                        }
                        else
                        {
                            this.RemoteScreenAdapterHandler.RemoteMouseBlock(false);
                            _islockMkey = false;
                            CheckMenuItem(sysMenuHandle, IDM_LOCK_MOUSEKEY, MF_UNCHECKED);
                        }
                        break;
                    case IDM_BLACKSCREEN:
                        this.RemoteScreenAdapterHandler.RemoteScreenBlack();
                        break;
                    case IDM_SAVESCREEN:
                        string fileName = DateTime.Now.ToFileTime().ToString() + " 远程桌面快照.bmp";
                        Image img = imgDesktop.Image;
                        img.Save(fileName);

                        MessageBoxHelper.ShowBoxExclamation("快照已保存到:" + Path.Combine(Environment.CurrentDirectory, fileName));
                        break;
                    case IDM_FULL_DIFFER:

                        CheckMenuItem(sysMenuHandle, IDM_FULL_DIFFER, MF_CHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_DIFFER, MF_UNCHECKED);
                        this.RemoteScreenAdapterHandler.RemoteChangeScanMode(ScreenScanMode.Noninterlaced);
                        break;
                    case IDM_DIFFER:

                        CheckMenuItem(sysMenuHandle, IDM_FULL_DIFFER, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_DIFFER, MF_CHECKED);
                        this.RemoteScreenAdapterHandler.RemoteChangeScanMode(ScreenScanMode.Difference);
                        break;
                    case IDM_1X:

                        CheckMenuItem(sysMenuHandle, IDM_1X, MF_CHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_4X, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_16X, MF_UNCHECKED);
                        this.RemoteScreenAdapterHandler.RemoteResetBrandColor(BrandColorMode.X1);
                        break;
                    case IDM_4X:

                        CheckMenuItem(sysMenuHandle, IDM_1X, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_4X, MF_CHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_16X, MF_UNCHECKED);
                        this.RemoteScreenAdapterHandler.RemoteResetBrandColor(BrandColorMode.X4);
                        break;
                    case IDM_16X:

                        CheckMenuItem(sysMenuHandle, IDM_1X, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_4X, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_16X, MF_CHECKED);
                        this.RemoteScreenAdapterHandler.RemoteResetBrandColor(BrandColorMode.X16);
                        break;
                    case IDM_Qty:
                        var dlg = new ScreenQtyForm();
                        if (dlg.ShowDialog() == DialogResult.OK)
                            this.RemoteScreenAdapterHandler.RemoteSetScreenQuantity(dlg.QualityValue);
                        break;
                    case IDM_SET_CLIPBOARD:
                        using (var setClipoardDlg = new EnterForm())
                        {
                            setClipoardDlg.Caption = "设置的剪切板内容:";
                            if (setClipoardDlg.ShowDialog() == DialogResult.OK)
                            {
                                var text = setClipoardDlg.Value;
                                this.RemoteScreenAdapterHandler.SetRemoteClipoardText(text);
                            }
                        }
                        break;
                    case IDM_GET_CLIPBOARD:
                        this.RemoteScreenAdapterHandler.GetRemoteClipoardText();
                        break;
                    case IDM_CTRL_ALT_DEL:
                        this.RemoteScreenAdapterHandler.SendCtrlAltDel();
                        break;
                    case IDM_CHANGE_MONITOR:
                        var dialog = new ScreenMonitorChangeForm();
                        dialog.SetMonitors(_monitorItems, _currenMonitorIndex);
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            if (_currenMonitorIndex == dialog.CurrentMonitorIndex)
                                break;

                            _currenMonitorIndex = dialog.CurrentMonitorIndex;
                            this.RemoteScreenAdapterHandler.MonitorChange(dialog.CurrentMonitorIndex);
                        }
                        break;
                    case IDM_DELETE_WALLPAPER:
                        this.RemoteScreenAdapterHandler.RemoteDeleteWallPaper();
                        break;
                    case IDM_RECORD:
                        if (_stop)
                        {
                            if (_srcImageHeight == 0 || _srcImageWidth == 0)
                                return;
                            if (!_videoFrameGraphics.IsNull())
                                _videoFrameGraphics.Dispose();
                            if (!_videoFrame.IsNull())
                                _videoFrame.Dispose();
                            lock (this)
                            {
                                _videoFrame = new Bitmap(this._srcImageWidth, this._srcImageHeight);
                                _videoFrameGraphics = Graphics.FromImage(_videoFrame);
                                _videoFrameGraphics.DrawImage(_currentFrame, 0, 0);
                            }
                            _stop = false;
                            ModifyMenu(sysMenuHandle, _menuContextIndex, MF_BYPOSITION, IDM_RECORD, "停止录制");
                            Task.Run(CreateDesktopRecordThread);
                        }
                        else
                        {
                            _stop = true;
                            ModifyMenu(sysMenuHandle, _menuContextIndex, MF_BYPOSITION, IDM_RECORD, "开始录制");
                        }
                        break;
                }
            }
            base.WndProc(ref m);
        }

        private void OnServcieInitEventHandler(RemoteScreenAdapterHandler adapterHandler, int height, int width, int currentMonitorIndex, MonitorItem[] monitorItems)
        {
            if (!_videoFrameGraphics.IsNull())
                _videoFrameGraphics.Dispose();

            this._currenMonitorIndex = currentMonitorIndex;
            this._monitorItems = monitorItems;
            this._srcImageWidth = width;
            this._srcImageHeight = height;

            _currentFrame = new Bitmap(width, height);
            _currentFrameGraphics = Graphics.FromImage(_currentFrame);

            Graphics g = Graphics.FromImage(_currentFrame);
            g.Clear(Color.Black);
            g.DrawString("桌面加载中...", new Font("微软雅黑", 15, FontStyle.Regular), new SolidBrush(Color.Red), new Point((height / 2) - 40, width / 2));
            g.Dispose();
            this.StartGetScreen();
        }

        private void OnScreenFragmentEventHandler(RemoteScreenAdapterHandler adapterHandler, Fragment[] fragments, ScreenReceivedType type)
        {
            switch (type)
            {
                case ScreenReceivedType.Noninterlaced:
                    this.FrameDataHandler(fragments);
                    _recvImgCount++;
                    this.GetNextScreen();
                    break;
                case ScreenReceivedType.Difference:
                    this.FrameDataHandler(fragments);
                    break;
                case ScreenReceivedType.DifferenceEnd:
                    _recvImgCount++;
                    this.GetNextScreen();
                    break;
                default:
                    break;
            }
            this.imgDesktop.Image = this._currentFrame;
        }

        private void FrameDataHandler(Fragment[] fragments)
        {
            if (this.RemoteScreenAdapterHandler.WhetherClose)
                return;
            lock (this)
            {
                foreach (var fragment in fragments)
                {
                    using (MemoryStream ms = new MemoryStream(fragment.FragmentData))
                    {
                        var rect = new Rectangle(fragment.X, fragment.Y, fragment.Width, fragment.Height);
                        var childFrame = Image.FromStream(ms);

                        this._currentFrameGraphics.DrawImage(childFrame, rect);

                        if (!_stop)
                            this._videoFrameGraphics.DrawImage(childFrame, rect);

                        childFrame.Dispose();
                        _traffic += ms.Length;
                    }
                }
            }
        }

        private void StartGetScreen()
        {
            //if (_screenDisplayMode == ScreenDisplayMode.Fullscreen)
            this.RemoteScreenAdapterHandler.StartGetScreen(this._srcImageHeight, this._srcImageWidth, Math.Abs(this.imgDesktop.Left), Math.Abs(this.imgDesktop.Top), ScreenDisplayMode.Original);
            //else
            //    this.RemoteScreenAdapterHandler.StartGetScreen(this.ClientSize.Height, this.ClientSize.Width, Math.Abs(this.imgDesktop.Left), Math.Abs(this.imgDesktop.Top), this._screenDisplayMode);
        }

        private void GetNextScreen()
        {
            //if (_screenDisplayMode == ScreenDisplayMode.Fullscreen)
            this.RemoteScreenAdapterHandler.GetNextScreen(this._srcImageHeight, this._srcImageWidth, Math.Abs(this.imgDesktop.Left), Math.Abs(this.imgDesktop.Top), ScreenDisplayMode.Original);
            //else
            //    this.RemoteScreenAdapterHandler.GetNextScreen(this.ClientSize.Height, this.ClientSize.Width, Math.Abs(this.imgDesktop.Left), Math.Abs(this.imgDesktop.Top), this._screenDisplayMode);
        }

        private void OnClipoardReceivedEventHandler(RemoteScreenAdapterHandler adapterHandler, string text)
        {
            Clipboard.SetText(text);
            MessageBoxHelper.ShowBoxExclamation("已获取剪切板内容!");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_continueTask)
            {
                this.Text = string.Format(_title, _recvImgCount.ToString(), (_traffic / (float)1024).ToString("0.00"));
                _recvImgCount = 0;
            }
        }

        private async void CreateDesktopRecordThread()
        {
            var targetDirectory = Path.Combine(Environment.CurrentDirectory, RemoteScreenAdapterHandler.OriginName);
            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            var fileName = Path.Combine(targetDirectory, $"远程桌面_{RemoteScreenAdapterHandler.OriginName}_{DateTime.Now.ToString("yyyy-MM-dd hhmmss")}.avi");

            var videoWriter = new VideoFileWriter();
            videoWriter.Open(fileName, _srcImageWidth, _srcImageHeight, 10, VideoCodec.H264);

            while (!_stop)
            {
                if (_videoFrame.IsNull())
                    continue;

                lock (this)
                    videoWriter.WriteVideoFrame(_videoFrame);
                GC.Collect();
                await Task.Delay(100); //帧率控制,每秒10帧，防止写入过快
            }
            videoWriter.Close();
        }

        private void ScreenSpyForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_isControl)
                return;

            this.RemoteScreenAdapterHandler.RemoteMouseKeyEvent(MOUSEKEY_ENUM.KeyDown, e.KeyValue, 0);
        }

        private void ScreenSpyForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (!_isControl)
                return;

            this.RemoteScreenAdapterHandler.RemoteMouseKeyEvent(MOUSEKEY_ENUM.KeyUp, e.KeyValue, 0);
        }

        private void desktopImg_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_isControl)
                return;

            int x = e.X;
            int y = e.Y;

            if (_screenDisplayMode == 0)
            {
                x = (int)(e.X / ((float)this.imgDesktop.Width / (float)this._srcImageWidth));
                y = (int)(e.Y / ((float)this.imgDesktop.Height / (float)this._srcImageHeight));
            }

            switch (e.Button)
            {
                case MouseButtons.Left:
                    this.RemoteScreenAdapterHandler.RemoteMouseKeyEvent(MOUSEKEY_ENUM.LeftDown, x, y);
                    break;
                case MouseButtons.Middle:
                    this.RemoteScreenAdapterHandler.RemoteMouseKeyEvent(MOUSEKEY_ENUM.MiddleDown, x, y);
                    break;
                case MouseButtons.None:
                    break;
                case MouseButtons.Right:
                    this.RemoteScreenAdapterHandler.RemoteMouseKeyEvent(MOUSEKEY_ENUM.RightDown, x, y);
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
            if (!_isControl)
                return;

            int x = e.X;
            int y = e.Y;

            if (_screenDisplayMode == 0)
            {
                x = (int)(e.X / ((float)this.imgDesktop.Width / (float)this._srcImageWidth));
                y = (int)(e.Y / ((float)this.imgDesktop.Height / (float)this._srcImageHeight));
            }

            switch (e.Button)
            {
                case MouseButtons.Left:
                    this.RemoteScreenAdapterHandler.RemoteMouseKeyEvent(MOUSEKEY_ENUM.LeftUp, x, y);
                    break;
                case MouseButtons.Middle:
                    this.RemoteScreenAdapterHandler.RemoteMouseKeyEvent(MOUSEKEY_ENUM.MiddleUp, x, y);
                    break;
                case MouseButtons.None:
                    break;
                case MouseButtons.Right:
                    this.RemoteScreenAdapterHandler.RemoteMouseKeyEvent(MOUSEKEY_ENUM.RightUp, x, y);
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
            _stop = true;
            _timer.Stop();
            _timer.Dispose();
            this.RemoteScreenAdapterHandler.OnClipoardReceivedEventHandler -= OnClipoardReceivedEventHandler;
            this.RemoteScreenAdapterHandler.OnServcieInitEventHandler -= OnServcieInitEventHandler;
            this.RemoteScreenAdapterHandler.OnScreenFragmentEventHandler -= OnScreenFragmentEventHandler;
            this.RemoteScreenAdapterHandler.CloseSession();
        }

        private void m_desktop_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isControl)
                return;

            int x = e.X;
            int y = e.Y;

            if (_screenDisplayMode == 0)
            {
                x = (int)(e.X / ((float)this.imgDesktop.Width / (float)this._srcImageWidth));
                y = (int)(e.Y / ((float)this.imgDesktop.Height / (float)this._srcImageHeight));
            }
            this.RemoteScreenAdapterHandler.RemoteMouseKeyEvent(MOUSEKEY_ENUM.Move, x, y);

        }
        private void ScreenManager_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!_isControl)
                return;

            this.RemoteScreenAdapterHandler.RemoteMouseKeyEvent(MOUSEKEY_ENUM.Wheel, e.Delta, 0);
        }
    }
}