using SiMay.Core;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.ModelBinder;

namespace SiMay.RemoteControlsCore
{
    [ApplicationKeyAttribute(ApplicationKeyConstant.REMOTE_UPDATE)]
    public class RemoteUpdateAdapterHandler : ApplicationAdapterHandler
    {
        /// <summary>
        /// 远程更新就绪
        /// </summary>
        public event Action OnRemoteUpdateReadyEventHandler;

        /// <summary>
        /// 获取下一个数据
        /// </summary>
        public event Action OnNextDataEventHandler;

        [PacketHandler(MessageHead.C_REMOTE_UPDATE_READY)]
        public void ReadyHandler(SessionProviderContext session)
            => OnRemoteUpdateReadyEventHandler?.Invoke();


        [PacketHandler(MessageHead.C_REMOTE_UPDATE_NEXT_DATA)]
        public void NextDataHandler(SessionProviderContext session)
            => OnNextDataEventHandler?.Invoke();

        public void SendFileData(byte[] data)
            => CurrentSession.SendTo( MessageHead.S_REMOTE_UPDATE_DATA, data);
    }
}
