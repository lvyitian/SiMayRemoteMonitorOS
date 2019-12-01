using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;

namespace SiMay.ServiceCore.ApplicationService
{

    /// <summary>
    /// 应用服务基类
    /// </summary>
    public class ServiceManagerBase
    {
        /// <summary>
        /// 服务Id
        /// </summary>
        public string AppServiceKey { get; set; }

        /// <summary>
        /// 当前会话是否已关闭
        /// </summary>
        public bool Closed { get; set; } = false;

        /// <summary>
        /// 数据处理绑定
        /// </summary>
        public PacketModelBinder<TcpSocketSaeaSession, MessageHead> HandlerBinder { get; set; }


        public ServiceManagerBase()
        {
            HandlerBinder = new PacketModelBinder<TcpSocketSaeaSession, MessageHead>();
        }

        /// <summary>
        /// 当前会话
        /// </summary>
        protected TcpSocketSaeaSession Session { get; set; }

        /// <summary>
        /// 设置Session
        /// </summary>
        /// <param name="session"></param>
        public void SetSession(TcpSocketSaeaSession session)
            => Session = session;
        /// <summary>
        /// 同步发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected int SendToServer(byte[] data)
            => Session.Send(data);

        protected int SendToServer(MessageHead msg, byte[] data = null)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data);
            return Session.Send(bytes);
        }

        protected int SendToServer(MessageHead msg, string lpString)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, lpString);
            return Session.Send(bytes);
        }

        protected int SendToServer(MessageHead msg, byte[] data, int size)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data, size);
            return Session.Send(bytes);
        }
        protected void SendAsyncToServer(byte[] data)
            => Session.SendAsync(data);

        protected void SendAsyncToServer(MessageHead msg, object entity)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, entity);
            Console.WriteLine("send lenght:" + bytes.Length);
            Session.SendAsync(bytes);
        }
        protected void SendAsyncToServer(MessageHead msg, byte[] data = null)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data);
            Session.SendAsync(bytes);
        }
        protected void SendAsyncToServer(MessageHead msg, string lpString)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, lpString);
            Session.SendAsync(bytes);
        }
        protected void SendAsyncToServer(MessageHead msg, byte[] data, int size)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data, size);
            Session.SendAsync(bytes);
        }

        /// <summary>
        /// 关闭当前会话
        /// </summary>
        protected void CloseSession()
            => Session.Close(true);

        [PacketHandler(MessageHead.S_GLOBAL_OK)]
        public  void InitializeCompleted(TcpSocketSaeaSession session)
        {
            this.SessionInitialized(session);
            SendAsyncToServer(MessageHead.C_MAIN_ACTIVE_APP,
                new ActiveAppPack()
                {
                    IdentifyId = AppConfiguartion.IdentifyId,
                    ServiceKey = this.AppServiceKey,
                    OriginName = Environment.MachineName + "@" + (AppConfiguartion.RemarkInfomation ?? AppConfiguartion.DefaultRemarkInfo)
                });
        }

        [PacketHandler(MessageHead.S_GLOBAL_ONCLOSE)]
        public void SessionClosed(TcpSocketSaeaSession session)
        {
            if (this.Closed)
                return;
            this.Closed = true;
            this.CloseSession();
            this.SessionClosed();
            this.HandlerBinder.Dispose();
        }

        public virtual void SessionInitialized(TcpSocketSaeaSession session)
        {

        }

        public virtual void SessionClosed()
        {

        }
    }
}