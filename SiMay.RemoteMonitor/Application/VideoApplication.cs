using SiMay.Basic;
using SiMay.RemoteControlsCore;
using SiMay.RemoteControlsCore.HandlerAdapters;
using SiMay.RemoteMonitor.Attributes;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using static SiMay.RemoteMonitor.Win32Api;

namespace SiMay.RemoteMonitor.Application
{
    [OnTools]
    [ApplicationName("视频监控")]
    [AppResourceName("ViedoManager")]
    [Application(typeof(VideoAppAdapterHandler), "RemoteViedoJob", 30)]
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


        public void SessionClose(AdapterHandlerBase handler)
            => this.Text = _title + " [" + VideoAppAdapterHandler.StateContext.ToString() + "]";


        public void ContinueTask(AdapterHandlerBase handler)
        {
            this.Text = _title;
            VideoAppAdapterHandler.StartGetFrame();
        }

        const Int32 IDM_HEIGHT = 1000;
        const Int32 IDM_DEFAULT = 1001;
        const Int32 IDM_LOW = 1002;
        const Int32 IDM_SAVE = 1003;

        private void VedioManager_Load(object sender, EventArgs e)
        {
            int index = 7;
            IntPtr sysMenuHandle = GetSystemMenu(this.Handle, false);
            InsertMenu(sysMenuHandle, index, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_HEIGHT, "高画质");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_DEFAULT, "中画质");
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_LOW, "低画质");
            InsertMenu(sysMenuHandle, index++, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, index++, MF_BYPOSITION, IDM_SAVE, "保存快照");

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
            VideoAppAdapterHandler.OnImageFrameHandlerEvent -= OnImageFrameHandlerEvent;
            VideoAppAdapterHandler.OnCameraNotStartupHandlerEvent -= OnCameraNotStartupHandlerEvent;
            VideoAppAdapterHandler.CloseHandler();
        }
    }
}