using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.PacketModelBinding;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControlsCore
{
    public abstract class ApplicationProtocolAdapterHandler : IDisposable
    {
        /// <summary>
        /// 数据处理函数绑定
        /// </summary>
        public PacketModelBinder<SessionProviderContext, MessageHead> HandlerBinder { get; set; }

        public ApplicationProtocolAdapterHandler()
        {
            HandlerBinder = new PacketModelBinder<SessionProviderContext, MessageHead>();
        }

        /// <summary>
        /// 发送实体对象
        /// </summary>
        /// <returns></returns>
        protected virtual void SendTo(SessionProviderContext session, MessageHead msg, object entity)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, entity);
            SendToBefore(session, bytes);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        protected virtual void SendTo(SessionProviderContext session, MessageHead msg, byte[] data = null)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, data);
            SendToBefore(session, bytes);
        }

        /// <summary>
        /// 发送字符串
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="lpString"></param>
        protected virtual void SendTo(SessionProviderContext session, MessageHead msg, string lpString)
        {
            byte[] bytes = MessageHelper.CopyMessageHeadTo(msg, lpString);
            SendToBefore(session, bytes);
        }
        protected virtual void SendToBefore(SessionProviderContext session, byte[] data)
        {
            var accessId = AppConfiguration.UseAccessId;
            SendTo(session, WrapAccessId(GZipHelper.Compress(data, 0, data.Length), accessId));
        }

        protected virtual void SendTo(SessionProviderContext session, byte[] data)
        {
            session.SendAsync(data);
        }

        /// <summary>
        /// 包装主控端标识
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

        protected virtual T GetMessageEntity<T>(SessionProviderContext session)
            where T : new()
        {
            return TakeHeadAndMessage(session).GetMessageEntity<T>();
        }

        protected virtual byte[] GetMessage(SessionProviderContext session)
        {
            return TakeHeadAndMessage(session).GetMessagePayload();
        }

        protected virtual MessageHead GetMessageHead(SessionProviderContext session)
        {
            return TakeHeadAndMessage(session).GetMessageHead<MessageHead>();
        }

        private byte[] TakeHeadAndMessage(SessionProviderContext session)
        {
            var length = session.CompletedBuffer.Length - sizeof(long);
            var bytes = new byte[length];
            Array.Copy(session.CompletedBuffer, sizeof(long), bytes, 0, length);
            return GZipHelper.Decompress(bytes);
        }

        public virtual void Dispose()
        {
            this.HandlerBinder.Dispose();
        }
    }
}
