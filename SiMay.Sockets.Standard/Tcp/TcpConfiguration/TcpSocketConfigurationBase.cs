using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Sockets.Tcp.TcpConfiguration
{
    public class TcpSocketConfigurationBase : ITcpSocketSaeaConfiguration
    {
        public TcpSocketConfigurationBase()
        {
            CompressTransferFromPacket = true;//是否压缩数据
            ReceiveBufferSize = 8192;
            SendBufferSize = 1024 * 1024 * 2;//默认2m，避免多次send，可根据自己的应用设置最优参数值
            ReceiveTimeout = TimeSpan.Zero;
            SendTimeout = TimeSpan.Zero;
            NoDelay = true;//是否开启nagle算法
            AppKeepAlive = true;//心跳包
            KeepAlive = false;
            KeepAliveInterval = 5000;
            KeepAliveSpanTime = 1000;
            ReuseAddress = true;//是否重用IP地址
        }

        internal bool _intervalWhetherService { get; set; }
        public bool CompressTransferFromPacket{ get; set; }
        public int ReceiveBufferSize { get; set; }
        public int SendBufferSize { get; set; }
        public TimeSpan ReceiveTimeout { get; set; }
        public TimeSpan SendTimeout { get; set; }
        public bool NoDelay { get; set; }
        public bool AppKeepAlive { get; set; }
        public bool KeepAlive { get; set; }
        public int KeepAliveInterval { get; set; }
        public int KeepAliveSpanTime { get; set; }
        public bool ReuseAddress { get; set; }
    }
}
