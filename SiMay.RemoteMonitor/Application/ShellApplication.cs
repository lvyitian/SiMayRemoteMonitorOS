using SiMay.RemoteControlsCore;
using SiMay.RemoteControlsCore.HandlerAdapters;
using SiMay.RemoteMonitor.Attributes;
using System;
using System.Net;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Application
{
    [OnTools]
    [ApplicationName("远程终端")]
    [AppResourceName("ShellManager")]
    [Application(typeof(ShellAdapterHandler), "RemoteShellJob", 60)]
    public partial class ShellApplication : Form, IApplication
    {

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

        public void SessionClose(AdapterHandlerBase handler)
        {
            this.Text = _title + " [" + this.ShellAdapterHandler.StateContext.ToString() + "]";
        }

        public void ContinueTask(AdapterHandlerBase handler)
        {
            this.Text = _title;
        }


        private void ShellForm_Load(object sender, EventArgs e)
        {
            this.Text = this._title = _title.Replace("#Name#", this.ShellAdapterHandler.OriginName);
            this.ShellAdapterHandler.OnOutputCommandEventHandler += OnOutputCommandEventHandler;
            this.ShellAdapterHandler.InputCommand("");
        }

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
            this.ShellAdapterHandler.CloseHandler();
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