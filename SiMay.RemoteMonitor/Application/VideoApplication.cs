using Accord.Video.FFMPEG;
using SiMay.Basic;
using SiMay.Core;
using SiMay.RemoteControlsCore;
using SiMay.RemoteControlsCore.HandlerAdapters;
using SiMay.RemoteMonitor.Attributes;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using static SiMay.RemoteMonitor.Win32Api;

namespace SiMay.RemoteMonitor.Application
{
    [OnTools]
    [ApplicationName("视频监控")]
    [AppResourceName("ViedoManager")]
    [Application(typeof(VideoAppAdapterHandler), AppJobConstant.REMOTE_VIDEO, 30)]
    public partial class VideoApplication : Form, IApplication
    {
        private string _title = "//视频监控 #Name#";

        [ApplicationAdapterHandler]
        public VideoAppAdapterHandler VideoAppAdapterHandler { get; set; }
        public VideoApplication()
        {
            InitializeComponent();
        }

        public void Start()
            => this.Show();


        public void SetParameter(object arg)
        {
            throw new NotImplementedException();
        }


        public void SessionClose(ApplicationAdapterHandler handler)
            => this.Text = _title + " [" + VideoAppAdapterHandler.StateContext.ToString() + "]";


        public void ContinueTask(ApplicationAdapterHandler handler)
        {
            this.Text = _title;
            VideoAppAdapterHandler.StartGetFrame();
        }

        const Int32 IDM_HEIGHT = 1000;
        const Int32 IDM_DEFAULT = 1001;
        const Int32 IDM_LOW = 1002;
        const Int32 IDM_SAVE = 1003;
        const Int32 IDM_RECORD = 1004;

        private int _videoFrameWidth = 0;
        private int _videoFrameHeight = 0;
        private Bitmap _videoFrame;
        private bool _stop = true;
        private int _menuContextIndex = 0;
        private void VedioManager_Load(object sender, EventArgs e)
        {
            int index = 7;
            IntPtr sysMenuHandle = GetSystemMenu(this.Handle, false);
            InsertMenu(sysMenuHandle, index++, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_HEIGHT, "高画质");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_DEFAULT, "中画质");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_LOW, "低画质");
            InsertMenu(sysMenuHandle, index++, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_SAVE, "保存快照");

            _menuContextIndex = index++;
            InsertMenu(sysMenuHandle, _menuContextIndex, MF_BYPOSITION, IDM_RECORD, "开始录制");
            CheckMenuItem(sysMenuHandle, IDM_DEFAULT, MF_CHECKED);
            this.Text = _title = _title.Replace("#Name#", VideoAppAdapterHandler.OriginName);

            this.ShowTip("视频帧加载中...");
            VideoAppAdapterHandler.OnImageFrameHandlerEvent += OnImageFrameHandlerEvent;
            VideoAppAdapterHandler.OnCameraNotStartupHandlerEvent += OnCameraNotStartupHandlerEvent;
            VideoAppAdapterHandler.StartGetFrame();
        }

        private void OnCameraNotStartupHandlerEvent(VideoAppAdapterHandler adapterHandler, int errorCode)
        {
            this.ShowTip("视频打开失败,未检测到视频设备!");
        }

        private void OnImageFrameHandlerEvent(VideoAppAdapterHandler adapterHandler, Image image)
        {
            lock (this)
            {
                _videoFrameHeight = image.Height;
                _videoFrameWidth = image.Width;

                if (!_videoFrame.IsNull())
                    _videoFrame.Dispose();

                _videoFrame = new Bitmap(image.Width, image.Height);
                Graphics g = Graphics.FromImage(_videoFrame);
                g.DrawImage(image, 0, 0);
                g.Dispose();
            }
            this.pictureBox.Image = image;
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

                        this.VideoAppAdapterHandler.RemoteSetFrameQuantity(3);
                        break;
                    case IDM_DEFAULT:

                        CheckMenuItem(sysMenuHandle, IDM_HEIGHT, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_DEFAULT, MF_CHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_LOW, MF_UNCHECKED);

                        this.VideoAppAdapterHandler.RemoteSetFrameQuantity(2);
                        break;
                    case IDM_LOW:

                        CheckMenuItem(sysMenuHandle, IDM_HEIGHT, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_DEFAULT, MF_UNCHECKED);
                        CheckMenuItem(sysMenuHandle, IDM_LOW, MF_CHECKED);

                        this.VideoAppAdapterHandler.RemoteSetFrameQuantity(1);
                        break;
                    case IDM_SAVE:

                        string fileName = Path.Combine(Environment.CurrentDirectory, DateTime.Now.ToFileTime().ToString() + " 视频监控快照.bmp");
                        Bitmap img = new Bitmap(pictureBox.Image.Width, pictureBox.Image.Height);
                        Graphics g = Graphics.FromImage(img);
                        g.DrawImage(pictureBox.Image, 0, 0);
                        img.Save(fileName);
                        g.Dispose();
                        img.Dispose();
                        MessageBoxHelper.ShowBoxExclamation("快照已保存到:" + fileName);

                        break;
                    case IDM_RECORD:
                        if (_stop)
                        {
                            if (_videoFrameWidth == 0 || _videoFrameHeight == 0)
                                return;
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

        private async void CreateDesktopRecordThread()
        {
            var targetDirectory = Path.Combine(Environment.CurrentDirectory, VideoAppAdapterHandler.OriginName);
            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            var fileName = Path.Combine(targetDirectory, $"摄像头查看_{VideoAppAdapterHandler.OriginName}_{DateTime.Now.ToString("yyyy-MM-dd hhmmss")}.avi");

            var videoWriter = new VideoFileWriter();
            videoWriter.Open(fileName, _videoFrameWidth, _videoFrameHeight, 10, VideoCodec.H264);

            do
            {
                if (!_videoFrame.IsNull())
                {
                    lock (this)
                        videoWriter.WriteVideoFrame(_videoFrame);
                    GC.Collect();
                }
                await Task.Delay(100); //帧率控制,每秒10帧，防止写入过快
            } while (!_stop);
            videoWriter.Close();
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
            _stop = true;
            VideoAppAdapterHandler.OnImageFrameHandlerEvent -= OnImageFrameHandlerEvent;
            VideoAppAdapterHandler.OnCameraNotStartupHandlerEvent -= OnCameraNotStartupHandlerEvent;
            VideoAppAdapterHandler.CloseSession();
        }
    }
}