using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.ServiceCore
{
    /// <summary>
    /// 远程应用服务
    /// </summary>
    public abstract class ApplicationRemoteService : ApplicationProtocolService
    {
        /// <summary>
        /// 主服务
        /// </summary>
        public MainService Self { get; set; }

        /// <summary>
        /// 当前连接的主控端标识
        /// </summary>
        public long AccessId { get; set; }

        /// <summary>
        /// 服务唯一标识
        /// </summary>
        public string AppServiceKey { get; set; }

        /// <summary>
        /// 当前会话是否已关闭
        /// </summary>
        public bool WhetherClosed { get; set; } = false;

        [PacketHandler(MessageHead.S_GLOBAL_CALL_CONTROLLER)]
        public void InvokeCustomeControllerHandler(TcpSocketSaeaSession session)
        {
            var callParameterPacket = GetMessageEntity<InvokerResponseControllerPacket>(CurrentSession);
            var result = ControllerModelHelper.Invoker(callParameterPacket.ControllerRoute, callParameterPacket.DataPacketTypeFullName, callParameterPacket.PacketData);
            if (!result.IsNull())
                SendTo(CurrentSession, MessageHead.C_GLOBAL_CONTROLLER_RESULT, result);
        }

        [PacketHandler(MessageHead.S_GLOBAL_OK)]
        public void InitializeCompleted(TcpSocketSaeaSession session)
        {
            SendTo(CurrentSession, MessageHead.C_MAIN_ACTIVE_APP,
                new ActivateApplicationPack()
                {
                    IdentifyId = AppConfiguartion.IdentifyId,
                    ServiceKey = this.AppServiceKey,
                    OriginName = Environment.MachineName + "@" + (AppConfiguartion.RemarkInfomation ?? AppConfiguartion.DefaultRemarkInfo)
                });
            this.SessionInited(session);
        }

        [PacketHandler(MessageHead.S_GLOBAL_ONCLOSE)]
        public void SessionClosed(TcpSocketSaeaSession session)
        {
            if (this.WhetherClosed)
                return;
            this.WhetherClosed = true;
            this.CloseSession();
            this.SessionClosed();
            this.HandlerBinder.Dispose();
        }

        public abstract void SessionInited(TcpSocketSaeaSession session);

        public abstract void SessionClosed();
    }
}
