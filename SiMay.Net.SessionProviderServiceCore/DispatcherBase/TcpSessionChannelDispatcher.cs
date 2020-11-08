using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Net.SessionProviderServiceCore
{
    public abstract class TcpSessionChannelDispatcher : DispatcherBase
    {
        /// <summary>
        /// 接收长度
        /// </summary>
        public event Action<TcpSessionChannelDispatcher, long> ReceiveStreamLengthEventHandler;

        /// <summary>
        /// 发送长度
        /// </summary>
        public event Action<TcpSessionChannelDispatcher, long> SendStreamLengthEventHandler;

        public override void OnProcess()
        {
            this.ReceiveStreamLengthEventHandler?.Invoke(this, ListByteBuffer.Count);
        }

        public virtual void SendTo(byte[] data)
        {
            this.CurrentSession.SendAsync(data);
            this.SendStreamLengthEventHandler?.Invoke(this, data.Length);
        }
    }
}
