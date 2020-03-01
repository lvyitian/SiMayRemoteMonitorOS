using SiMay.Net.SessionProvider.Core;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SiMay.Net.SessionProvider
{
    public class TcpSocketSessionContext : SessionProviderContext
    {
        public TcpSocketSessionContext(TcpSocketSaeaSession session)
        {
            CurrentSession = session;
            session.AppTokens = new object[] { this };
        }
        /// <summary>
        /// 发送长度
        /// </summary>
        public override int SendTransferredBytes => CurrentSession.SendTransferredBytes;

        /// <summary>
        /// 接受长度
        /// </summary>
        public override int ReceiveTransferredBytes => CurrentSession.ReceiveBytesTransferred;

        /// <summary>
        /// 完成缓冲区
        /// </summary>
        public override byte[] CompletedBuffer => CurrentSession.CompletedBuffer;

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public override void SendAsync(byte[] data, int offset, int length) => CurrentSession.SendAsync(data, offset, length);

        /// <summary>
        /// 关闭会话
        /// </summary>
        public override void SessionClose() => CurrentSession.Close(true);

        /// <summary>
        /// Socket设置
        /// </summary>
        /// <param name="optionLevel"></param>
        /// <param name="optionName"></param>
        /// <param name="optionValue"></param>
        public override void SetSocketOptions(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
        {
            if (optionValue is byte[] bytes)
                CurrentSession.Socket.SetSocketOption(optionLevel, optionName, bytes);
            else if (optionValue is int num)
                CurrentSession.Socket.SetSocketOption(optionLevel, optionName, num);
            else if (optionValue is bool b)
                CurrentSession.Socket.SetSocketOption(optionLevel, optionName, b);
            else
                CurrentSession.Socket.SetSocketOption(optionLevel, optionName, optionValue);
        }
    }
}
