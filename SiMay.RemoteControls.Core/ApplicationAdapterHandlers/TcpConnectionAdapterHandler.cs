using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Basic;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    [ApplicationKey(ApplicationKeyConstant.REMOTE_TCP)]
    public class TcpConnectionAdapterHandler : ApplicationAdapterHandler
    {
        /// <summary>
        /// Tcp连接信息
        /// </summary>
        public event Action<TcpConnectionAdapterHandler, IEnumerable<TcpConnectionItem>> OnTcpListHandlerEvent;

        /// <summary>
        /// 获取Tcp连接信息
        /// </summary>
        public async void GetTcpList()
        {
            var responsed = await SendTo(MessageHead.S_TCP_GET_LIST);

            if (!responsed.IsNull())
            {
                var tcpConnectionPack = SiMay.Serialize.Standard.PacketSerializeHelper.DeserializePacket<TcpConnectionPacket>(responsed.Datas);
                this.OnTcpListHandlerEvent?.Invoke(this, tcpConnectionPack.TcpConnections);
            }
        }

        /// <summary>
        /// 关闭Tcp连接
        /// </summary>
        /// <param name="killTcps"></param>
        public async Task CloseTcpList(IEnumerable<KillTcpConnectionItem> killTcps)
        {

            await SendTo(MessageHead.S_TCP_CLOSE_CHANNEL,
                new KillTcpConnectionPack()
                {
                    Kills = killTcps.ToArray()
                });

            GetTcpList();
        }
    }
}
