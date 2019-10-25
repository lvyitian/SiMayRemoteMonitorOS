using SiMay.Core;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;

namespace SiMay.ServiceCore.ApplicationService
{

    /// <summary>
    /// 工作模块基类
    /// 
    /// 作用：所有工作模块基于此类
    /// </summary>
    public class ServiceManager
    {
        protected TcpSocketSaeaSession _session;
        public void SetSession(TcpSocketSaeaSession session)
            => _session = session;

        protected int SendToServer(byte[] data)
            => _session.Send(data);

        protected int SendToServer(MessageHead msg, byte[] data = null)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data);
            return _session.Send(bytes);
        }

        protected int SendToServer(MessageHead msg, string lpString)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, lpString);
            return _session.Send(bytes);
        }

        protected int SendToServer(MessageHead msg, byte[] data, int size)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data, size);
            return _session.Send(bytes);
        }
        protected void SendAsyncToServer(byte[] data)
            => _session.SendAsync(data);

        protected void SendAsyncToServer(MessageHead msg, object entity)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, entity);
            Console.WriteLine("send lenght:" + bytes.Length);
            _session.SendAsync(bytes);
        }
        protected void SendAsyncToServer(MessageHead msg, byte[] data = null)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data);
            _session.SendAsync(bytes);
        }
        protected void SendAsyncToServer(MessageHead msg, string lpString)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, lpString);
            _session.SendAsync(bytes);
        }
        protected void SendAsyncToServer(MessageHead msg, byte[] data, int size)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data, size);
            _session.SendAsync(bytes);
        }
        public virtual void OnNotifyProc(TcpSocketCompletionNotify notify, TcpSocketSaeaSession session)
        {
        }
        protected void CloseSession()
            => _session.Close(true);
    }
}