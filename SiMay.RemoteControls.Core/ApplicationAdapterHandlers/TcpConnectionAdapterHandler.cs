using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.Net.SessionProvider;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class TcpConnectionAdapterHandler : ApplicationAdapterHandler
    {
        /// <summary>
        /// Tcp连接信息
        /// </summary>
        public event Action<TcpConnectionAdapterHandler, IEnumerable<TcpConnectionItem>> OnTcpListHandlerEvent;

        [PacketHandler(MessageHead.C_TCP_LIST)]
        private void TcpListHandler(SessionProviderContext session)
        {
            var pack = GetMessageEntity<TcpConnectionPack>(session);
            this.OnTcpListHandlerEvent?.Invoke(this, pack.TcpConnections);
        }

        /// <summary>
        /// 获取Tcp连接信息
        /// </summary>
        public void GetTcpList()
        {
            SendTo(CurrentSession, MessageHead.S_TCP_GET_LIST);
        }

        /// <summary>
        /// 关闭Tcp连接
        /// </summary>
        /// <param name="killTcps"></param>
        public void CloseTcpList(IEnumerable<KillTcpConnectionItem> killTcps)
        {
            SendTo(CurrentSession, MessageHead.S_TCP_CLOSE_CHANNEL,
                new KillTcpConnectionPack()
                {
                    Kills = killTcps.ToArray()
                });
        }
    }
}
