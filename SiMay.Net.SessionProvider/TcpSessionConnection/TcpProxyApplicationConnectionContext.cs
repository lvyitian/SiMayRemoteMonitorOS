using System;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using SiMay.Basic;
using SiMay.Sockets.Tcp.Session;
using SiMay.Net.SessionProvider.Core;

namespace SiMay.Net.SessionProvider
{
    public class TcpProxyApplicationConnectionContext : SessionProviderContext
    {
        /// <summary>
        /// 消息接受完成
        /// </summary>
        public event Action<TcpProxyApplicationConnectionContext> DataReceivedEventHandler;

        /// <summary>
        /// 消息发送完成
        /// </summary>
        public event Action<TcpProxyApplicationConnectionContext> DataSendEventHandler;

        /// <summary>
        /// 标识
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// 缓冲区
        /// </summary>
        public virtual List<byte> ListByteBuffer
        {
            get;
            set;
        } = new List<byte>();

        /// <summary>
        /// 设置当前会话
        /// </summary>
        /// <param name="session"></param>
        public void SetSession(TcpSocketSaeaSession session, long id, byte[] ackData)
        {
            Id = id;
            CurrentSession = session;
            CompletedBuffer = ackData;
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        public void OnMessage(int receiveLength)
        {
            ReceiveTransferredBytes = receiveLength;

            int defineHeadSize = sizeof(int);
            do
            {
                if (ListByteBuffer.Count < defineHeadSize)
                    return;

                byte[] lenBytes = ListByteBuffer.GetRange(0, defineHeadSize).ToArray();
                int packageLen = BitConverter.ToInt32(lenBytes, 0);

                if (packageLen < 0)
                    throw new Exception("Illegal length!");

                if (packageLen + defineHeadSize > ListByteBuffer.Count)
                    return;
                this.CompletedBuffer = ListByteBuffer.GetRange(defineHeadSize, packageLen).ToArray();
                this.DataReceivedEventHandler?.Invoke(this);
                ListByteBuffer.RemoveRange(0, packageLen + defineHeadSize);

            } while (ListByteBuffer.Count > defineHeadSize);
        }

        public override void SendAsync(byte[] data, int offset, int length)
        {
            var packetData = MessageHelper.CopyMessageHeadTo(MessageHead.APP_MESSAGE_DATA,
                new MessageDataPacket()
                {
                    AccessId = ApplicationConfiguartion.Options.AccessId,
                    DispatcherId = this.Id,
                    Data = data.Copy(offset, length)
                });
            this.CurrentSession.SendAsync(packetData);

            this.SendTransferredBytes = packetData.Length;
            this.DataSendEventHandler?.Invoke(this);
        }
        /// <summary>
        /// 不支持关闭代理连接
        /// </summary>
        public override void SessionClose()
        {
            throw new NotImplementedException("not support");
        }

        public override void SetSocketOptions(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
        {
            throw new NotImplementedException("not support");
        }

        public override void Dispose()
        {
            ListByteBuffer.Clear();
            base.Dispose();
        }
    }
}
