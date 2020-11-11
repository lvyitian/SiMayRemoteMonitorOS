using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.Standard.Packets;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
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
        /// 当前连接的主控端标识
        /// </summary>
        public long AccessId { get; set; }

        /// <summary>
        /// 服务唯一标识
        /// </summary>
        public string ApplicationKey { get; set; }

        /// <summary>
        /// 创建命令
        /// </summary>
        public string ActivatedCommandText { get; set; }

        /// <summary>
        /// 当前会话是否已关闭
        /// </summary>
        public bool WhetherClosed { get; set; } = false;

        [PacketHandler(MessageHead.S_GLOBAL_OK)]
        public void InitializeCompleted(SessionProviderContext session)
        {
            session.SendTo(MessageHead.C_MAIN_ACTIVE_APP,
                new ActivateApplicationPack()
                {
                    IdentifyId = AppConfiguartion.IdentifyId,
                    ApplicationKey = this.ApplicationKey,
                    ActivatedCommandText = this.ActivatedCommandText,
                    OriginName = Environment.MachineName + "@" + (AppConfiguartion.RemarkInfomation ?? AppConfiguartion.DefaultRemarkInfo)
                });
            this.SessionInited(session);
        }

        [PacketHandler(MessageHead.S_GLOBAL_ONCLOSE)]
        public void SessionClosed(SessionProviderContext session)
        {
            if (this.WhetherClosed)
                return;
            this.WhetherClosed = true;
            this.CloseSession();
            this.SessionClosed();
            this.HandlerBinder.Dispose();
        }

        public abstract void SessionInited(SessionProviderContext session);

        public abstract void SessionClosed();

        [PacketHandler(MessageHead.S_GLOBAL_SYNC_CALL)]
        public void CallFunctionSync(SessionProviderContext session)
        {
            var callTarget = session.GetMessageEntity<CallSyncPacket>();

            try
            {
                var returnEntity = this.HandlerBinder.CallFunctionPacketHandler(session, callTarget.TargetMessageHead, this);
                if (!returnEntity.IsNull())
                {
                    var syncResultPacket = new CallSyncResultPacket
                    {
                        Id = callTarget.Id,
                        Datas = SiMay.Serialize.Standard.PacketSerializeHelper.SerializePacket(returnEntity),
                        IsOK = true
                    };
                    session.SendTo(MessageHead.C_GLOBAL_SYNC_RESULT, syncResultPacket);
                }
            }
            catch (Exception)
            {
                session.SendTo(MessageHead.C_GLOBAL_SYNC_RESULT,
                    new CallSyncResultPacket
                    {
                        Id = callTarget.Id,
                        Datas = Array.Empty<byte>(),
                        IsOK = false
                    });
            }

        }
    }
}
