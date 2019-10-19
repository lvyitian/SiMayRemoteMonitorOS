using SiMay.Core;
using SiMay.Net.SessionProvider.SessionBased;
using SiMay.RemoteMonitor.Delegate;
using SiMay.RemoteMonitor.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteMonitor
{
    public class MessageAdapter
    {
        public string ResetMsg { get; set; }
        public bool WindowClosed { get; set; }
        public string IdentifyId { get; set; }
        public SessionHandler Session { get; set; }
        public string TipText { get; set; }
        public string OriginName { get; set; }
        public MessageAdapter(SessionHandler session, string identifyId,string originName)
        {
            Session = session;
            IdentifyId = identifyId;
            OriginName = originName;
        }
        public event SessionNotifyEventHandler<SessionHandler, SessionNotifyType> OnSessionNotifyPro;

        public void SessionClosed()
            => OnSessionNotifyPro?.Invoke(Session, SessionNotifyType.SessionClosed);

        public void OnSessionNotify(SessionHandler session, SessionNotifyType type)
            => OnSessionNotifyPro?.Invoke(session, type);

        public void ContinueTask()
            => OnSessionNotifyPro?.Invoke(Session, SessionNotifyType.ContinueTask);

        public void WindowShow()
            => OnSessionNotifyPro?.Invoke(Session, SessionNotifyType.WindowShow);

        public void WindowClose()
            => OnSessionNotifyPro?.Invoke(Session, SessionNotifyType.WindowClose);

        public MessageHead GetMessageHead()
            => Session.CompletedBuffer.GetMessageHead();
        public byte[] GetPayLoad()
            => Session.CompletedBuffer;
        public void SendAsyncMessage(byte[] data)
            => Session.SendAsync(data);
        public void SendAsyncMessage(MessageHead msg, object entity)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, entity);
            Session.SendAsync(bytes);
        }
        public void SendAsyncMessage(MessageHead msg, byte[] data = null)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data);
            Session.SendAsync(bytes);
        }
        public void SendAsyncMessage(MessageHead msg, string lpString)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, lpString);
            Session.SendAsync(bytes);
        }
        public void SendAsyncMessage(MessageHead msg, byte[] data, int size)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data, size);
            Session.SendAsync(bytes);
        }
    }
}