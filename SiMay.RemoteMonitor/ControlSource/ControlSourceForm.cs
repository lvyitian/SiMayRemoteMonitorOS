using SiMay.Core;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.RemoteMonitor.Extensions;
using SiMay.RemoteMonitor.Notify;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.ControlSource
{
    public class ControlSourceForm : Form, IControlSource
    {
        public MessageAdapter Adapter { get; set; }
        public ControlSourceForm(MessageAdapter adapter)
        {
            Adapter = adapter;
            adapter.OnSessionNotifyPro += AdapterNotify;
            //adapter.ResetMsg = this.GetType().GetControlKey();

            this.FormClosing += ControlSourceForm_FormClosing;
        }

        protected virtual void AdapterNotify(SessionHandler session, SessionNotifyType notify)
        {
            switch (notify)
            {
                case SessionNotifyType.Message:
                    this.OnMessage(session, session.CompletedBuffer.GetMessageHead());
                    break;
                case SessionNotifyType.OnReceive:
                    this.OnReceive(session, session.ReceiveTransferredBytes);
                    break;
                case SessionNotifyType.ContinueTask:
                    this.ContinueTask(session);
                    break;
                case SessionNotifyType.WorkSessionClosed:
                    this.SessionClosed(session);
                    break;
                case SessionNotifyType.WindowShow:
                    this.Show();
                    break;
                case SessionNotifyType.WindowClose:
                    Adapter.WindowClosed = true;
                    this.Close();
                    break;
                default:
                    break;
            }
        }
        public virtual void Action()
        {
            this.Show();
        }

        public virtual void OnMessage(SessionHandler session, MessageHead head)
        {

        }

        /// <summary>
        /// 数据接收事件
        /// </summary>
        /// <param name="session"></param>
        /// <param name="lenght">如果通讯层启用了压缩模式传输，那么length为为解压前的数据大小</param>
        public virtual void OnReceive(SessionHandler session, int lenght)
        {

        }

        public virtual void ContinueTask(SessionHandler session)
        {

        }

        public virtual void SessionClosed(SessionHandler session)
        {
            this.Text = Adapter.TipText;
        }
        private void ControlSourceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //及时的通知远程关闭连接并释放资源
            Adapter.SendAsyncMessage(MessageHead.S_GLOBAL_ONCLOSE);

            //手动关闭设为true,否则系统将会重新连接
            Adapter.WindowClosed = true;
        }
    }
}
