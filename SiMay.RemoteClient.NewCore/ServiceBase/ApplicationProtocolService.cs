using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.PacketModelBinding;
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
            var accessIdObj = Thread.GetData(Thread.GetNamedDataSlot("AccessId"));
            var accessId = accessIdObj.IsNull() ? GetAccessId(session) : accessIdObj.ConvertTo<long>();

            SendTo(session, WrapAccessId(GZipHelper.Compress(data, 0, data.Length), accessId));
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
        private byte[] WrapAccessId(byte[] data, long accessId)
        {
            var bytes = new byte[data.Length + sizeof(long)];
            BitConverter.GetBytes(accessId).CopyTo(bytes, 0);
            data.CopyTo(bytes, sizeof(long));
            return bytes;
        }

        protected virtual T GetMessageEntity<T>(TcpSocketSaeaSession session)
            where T : new()
        {
            return TakeHeadAndMessage(session).GetMessageEntity<T>();
        }

        protected virtual byte[] GetMessage(TcpSocketSaeaSession session)
        {
            return TakeHeadAndMessage(session).GetMessagePayload();
        }

        protected virtual MessageHead GetMessageHead(TcpSocketSaeaSession session)
        {
            return TakeHeadAndMessage(session).GetMessageHead<MessageHead>();
        }

        private byte[] TakeHeadAndMessage(TcpSocketSaeaSession session)
        {
            var bytes = session.CompletedBuffer.Copy(sizeof(long), session.CompletedBuffer.Length - sizeof(long));
            return GZipHelper.Decompress(bytes);
        }

        /// <summary>
        /// 获取当前数据来源AccessId
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        protected long GetAccessId(TcpSocketSaeaSession session)
        {
            if (session.CompletedBuffer.IsNull())
                return 0;
            return BitConverter.ToInt64(session.CompletedBuffer, 0);
        }

    }
}
