using SiMay.Basic;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SiMay.ServiceCore
{
    /// <summary>
    /// 应用协议处理服务，提供消息封装丶消息解析处理及消息处理函数绑定
    /// </summary>
    public abstract class ApplicationProtocolService : ApplicationServiceBase
    {
        /// <summary>
        /// 数据处理绑定
        /// </summary>
        public PacketModelBinder<TcpSocketSaeaSession, MessageHead> HandlerBinder { get; set; }


        public ApplicationProtocolService()
        {
            HandlerBinder = new PacketModelBinder<TcpSocketSaeaSession, MessageHead>();
        }

        protected virtual void SendTo(TcpSocketSaeaSession session, MessageHead msg, object entity)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, entity);
            SendToBefore(session, bytes);
        }
        protected virtual void SendTo(TcpSocketSaeaSession session, MessageHead msg, byte[] data = null)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data);
            SendToBefore(session, bytes);
        }
        protected virtual void SendTo(TcpSocketSaeaSession session, MessageHead msg, string lpString)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, lpString);
            SendToBefore(session, bytes);
        }
        protected virtual void SendToBefore(TcpSocketSaeaSession session, byte[] data)
        {
            //为解决延迟发送导致的accessId不安全问题
            //var accessIdObj = Thread.GetData(Thread.GetNamedDataSlot("AccessId"));
            //var accessId = accessIdObj.IsNull() ? GetAccessId(session) : accessIdObj.ConvertTo<long>();

            var accessId = GetAccessId(session);

            SendTo(session, SysMessageConstructionHelper.WrapAccessId(data, accessId));
        }

        protected virtual void SendTo(TcpSocketSaeaSession session, byte[] data)
        {
            session.SendAsync(data);
        }
        /// <summary>
        /// 封装主控端标识
        /// </summary>
        /// <param name="data"></param>
        /// <param name="accessId"></param>
        /// <returns></returns>


        protected virtual T GetMessageEntity<T>(TcpSocketSaeaSession session)
            where T : new()
            => SysMessageConstructionHelper.GetMessageEntity<T>(session.CompletedBuffer);


        protected virtual byte[] GetMessage(TcpSocketSaeaSession session)
            => SysMessageConstructionHelper.GetMessage(session.CompletedBuffer);


        protected virtual MessageHead GetMessageHead(TcpSocketSaeaSession session)
            => SysMessageConstructionHelper.GetMessageHead(session.CompletedBuffer);


        /// <summary>
        /// 获取当前数据来源AccessId
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        protected long GetAccessId(TcpSocketSaeaSession session)
            => SysMessageConstructionHelper.GetAccessId(session.CompletedBuffer);


    }
}
