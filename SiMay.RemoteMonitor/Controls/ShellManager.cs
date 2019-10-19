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
using System.Net;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Controls
{
    [ControlApp(60, "远程终端", "RemoteShellJob", "ShellManager")]
    public partial class ShellManager : Form, IControlSource
    {
        private string _title = "//远程终端 #Name#";
        private MessageAdapter _adapter;
        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        public ShellManager(MessageAdapter adapter)
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
                    if (_adapter.WindowClosed)
                        return;

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

        string lastline = null;

        private void ShellForm_Load(object sender, EventArgs e)
        {
            this.Text = _title;
            _adapter.SendAsyncMessage(MessageHead.S_SHELL_INPUT, "");
        }

        private void ShellForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _handlerBinder.Dispose();
            _adapter.WindowClosed = true;
            _adapter.SendAsyncMessage(MessageHead.S_GLOBAL_ONCLOSE);
        }

        [PacketHandler(MessageHead.C_SHELL_RESULT)]
        public void SetText(SessionHandler session)
        {
            string text = session.CompletedBuffer.GetMessagePayload().ToUnicodeString();
            Console.WriteLine(text);
            lock (this.txtCommandLine)
            {
                if (this.txtCommandLine.TextLength <= 0)
                {
                    text = text.Replace("\r\n", null);
                }
                if (this.lastline != text.Substring(2))
                {
                    this.txtCommandLine.AppendText(text);
                }
            }
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                this.lastline = this.txtCommandLine.Text.Substring(this.txtCommandLine.GetFirstCharIndexOfCurrentLine());
                var str = this.lastline.Substring(this.lastline.IndexOf('>') + 1);

                _adapter.SendAsyncMessage(MessageHead.S_SHELL_INPUT, str);

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