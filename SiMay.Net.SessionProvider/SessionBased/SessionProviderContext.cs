using SiMay.Sockets.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SiMay.Net.SessionProvider.SessionBased
{
    public abstract class SessionProviderContext
    {
        /// <summary>
        /// 当前Socket
        /// </summary>
        public Socket Socket { get; set; }

        /// <summary>
        /// 异步上下文对象
        /// </summary>
        public object[] AppTokens { get; set; }

 
        public abstract int SendTransferredBytes { get;}

        public abstract int ReceiveTransferredBytes { get;}

        /// <summary>
        /// 完成缓冲区
        /// </summary>
        public abstract byte[] CompletedBuffer { get; }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="data"></param>
        public abstract void SendAsync(byte[] data);

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public abstract void SendAsync(byte[] data, int offset, int length);

        /// <summary>
        /// 关闭会话
        /// </summary>
        public abstract void SessionClose();
    }
}