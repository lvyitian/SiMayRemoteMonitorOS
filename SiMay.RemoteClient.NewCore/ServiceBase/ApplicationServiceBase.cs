using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;

namespace SiMay.ServiceCore
{
    /// <summary>
    /// 基础应用服务
    /// </summary>
    public abstract class ApplicationServiceBase
    {

        /// <summary>
        /// 当前会话
        /// </summary>
        protected TcpSocketSaeaSession CurrentSession { get; set; }

        /// <summary>
        /// 设置当前Session
        /// </summary>
        /// <param name="session"></param>
        public void SetSession(TcpSocketSaeaSession session)
            => CurrentSession = session;

        /// <summary>
        /// 关闭当前会话
        /// </summary>
        public void CloseSession()
            => CurrentSession.Close(true);
    }
}