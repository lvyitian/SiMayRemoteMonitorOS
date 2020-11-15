using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Basic;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;

namespace SiMay.RemoteControls.Core
{
    [ApplicationServiceKey(ApplicationKeyConstant.REMOTE_TCP)]
    public class TcpConnectionAdapterHandler : ApplicationBaseAdapterHandler
    {
        /// <summary>
        /// 获取Tcp连接信息
        /// </summary>
        public async Task<TcpConnectionItem[]> GetTcpList()
        {
            var responsed = await SendTo(MessageHead.S_TCP_GET_LIST);
            if (!responsed.IsNull() && responsed.IsOK)
            {
                var tcpConnectionPack = responsed.Datas.GetMessageEntity<TcpConnectionPacket>();
                return tcpConnectionPack.TcpConnections;
            }
            else
                return null;
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
        }
    }
}
