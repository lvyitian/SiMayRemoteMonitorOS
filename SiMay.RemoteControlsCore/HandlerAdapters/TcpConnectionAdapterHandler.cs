using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets.TcpConnection;
using SiMay.Net.SessionProvider.SessionBased;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class TcpConnectionAdapterHandler : AdapterHandlerBase
    {
        /// <summary>
        /// Tcp连接信息
        /// </summary>
        public event Action<TcpConnectionAdapterHandler, IEnumerable<TcpConnectionItem>> OnTcpListHandlerEvent;

        [PacketHandler(MessageHead.C_TCP_LIST)]
        private void TcpListHandler(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<TcpConnectionPack>();
            this.OnTcpListHandlerEvent?.Invoke(this, pack.TcpConnections);
        }

        /// <summary>
        /// 获取Tcp连接信息
        /// </summary>
        public void GetTcpList()
        {
            SendAsyncMessage(MessageHead.S_TCP_GET_LIST);
        }

        /// <summary>
        /// 关闭Tcp连接
        /// </summary>
        /// <param name="killTcps"></param>
        public void CloseTcpList(IEnumerable<KillTcpConnectionItem> killTcps)
        {
            SendAsyncMessage(MessageHead.S_TCP_CLOSE_CHANNEL, new KillTcpConnectionPack()
            {
                Kills = killTcps.ToArray()
            });
        }
    }
}
