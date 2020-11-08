using System;
using System.Text;
using System.Linq;
using System.Net.Sockets;
using System.Collections.Generic;
using SiMay.Sockets.Tcp.Session;
using SiMay.Net.SessionProvider.Core;

namespace SiMay.Net.SessionProvider
{
    public abstract class SessionProviderContext : IDisposable
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// 当前会话
        /// </summary>
        public TcpSocketSaeaSession CurrentSession { get; protected set; }

        /// <summary>
        /// 异步上下文对象
        /// </summary>
        public object[] AppTokens { get; set; }

        /// <summary>
        /// 发送长度
        /// </summary>
        public virtual int SendTransferredBytes { get; protected set; }

        /// <summary>
        /// 接受长度
        /// </summary>
        public virtual int ReceiveTransferredBytes { get; protected set; }

        /// <summary>
        /// 完成缓冲区
        /// </summary>
        public virtual byte[] CompletedBuffer { get; protected set; }

        /// <summary>
        /// 获取当前消息AccessId
        /// </summary>
        /// <returns></returns>
        public long GetAccessId()
            => ProxyProtocolConstructionHelper.GetAccessId(CompletedBuffer);

        /// <summary>
        /// Socket选项设置
        /// </summary>
        /// <param name="optionLevel"></param>
        /// <param name="optionName"></param>
        /// <param name="optionValue"></param>
        public abstract void SetSocketOptions(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue);

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="data"></param>
        public virtual void SendAsync(byte[] data)
            => SendAsync(data, 0, data.Length);

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
        public abstract void SessionClose(bool notify = true);

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            CurrentSession = null;
        }
    }
}