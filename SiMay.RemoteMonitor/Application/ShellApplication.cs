using SiMay.Core;
using SiMay.RemoteControls.Core;
using SiMay.RemoteMonitor.Attributes;
using System;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Application
{
    [OnTools]
    [Rank(60)]
    [ApplicationName("远程终端")]
    [AppResourceName("ShellManager")]
    public partial class ShellApplication : Form, IApplication
    {
        /// <summary>
        /// 多会话测试
        /// </summary>
        [ApplicationAdapterHandler]
        public TcpConnectionAdapterHandler TcpConnectionAdapterHandler { get; set; }

        [ApplicationAdapterHandler]
        public ShellAdapterHandler ShellAdapterHandler { get; set; }

        private string _title = "//远程终端 #Name#";

        private string _lastLine = string.Empty;

        public ShellApplication()
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
            this.Text = _title + " [" + handler.State.ToString() + "]";
        }

        public void ContinueTask(ApplicationBaseAdapterHandler handler)
        {
            this.Text = _title;
        }


        private void ShellForm_Load(object sender, EventArgs e)
        {
            this.Text = this._title = _title.Replace("#Name#", this.ShellAdapterHandler.OriginName);
            this.ShellAdapterHandler.OnOutputCommandEventHandler += OnOutputCommandEventHandler;
            this.ShellAdapterHandler.InputCommand(string.Empty);

            //TcpConnectionAdapterHandler.OnTcpListHandlerEvent += TcpConnectionAdapterHandler_OnTcpListHandlerEvent;

            //await TcpConnectionAdapterHandler.GetTcpList();
        }

        //private void TcpConnectionAdapterHandler_OnTcpListHandlerEvent(TcpConnectionAdapterHandler arg1, System.Collections.Generic.IEnumerable<TcpConnectionItem> arg2)
        //{
        //    this.txtCommandLine.AppendText(DateTime.Now.ToString());
        //}

        private void OnOutputCommandEventHandler(ShellAdapterHandler adapterHandler, string outputLine)
        {
            lock (this.txtCommandLine)
            {
                if (this.txtCommandLine.TextLength <= 0)
                {
                    outputLine = outputLine.Replace("\r\n", null);
                }
                if (this._lastLine != outputLine.Substring(2))
                {
                    this.txtCommandLine.AppendText(outputLine);
                }
            }
        }

        private void ShellForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.ShellAdapterHandler.OnOutputCommandEventHandler -= OnOutputCommandEventHandler;
            this.ShellAdapterHandler.CloseSession();

            this.TcpConnectionAdapterHandler.CloseSession();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                this._lastLine = this.txtCommandLine.Text.Substring(this.txtCommandLine.GetFirstCharIndexOfCurrentLine());
                var str = this._lastLine.Substring(this._lastLine.IndexOf('>') + 1);
                this.ShellAdapterHandler.InputCommand(str);

                e.Handled = true;
            }
            else if (e.KeyChar == (char)Keys.Back)
            {
                var str = this.txtCommandLine.Text.Substring(this.txtCommandLine.GetFirstCharIndexOfCurrentLine());
                if (str.Length > 1)
                {
                    if (str.Substring(str.Length - 1) == ">")
                    {
                        e.Handled = true;
                    }
                }
            }
        }
    }
}