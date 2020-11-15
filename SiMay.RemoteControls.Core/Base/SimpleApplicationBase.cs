using System;
using SiMay.Basic;
using SiMay.Core;
using SiMay.Net.SessionProvider;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControls.Core
{
    public abstract class SimpleApplicationBase
    {
        /// <summary>
        /// 同步调用简单程序
        /// </summary>
        /// <param name="session"></param>
        /// <param name="msg"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        protected async Task<CallSyncResultPacket> CallSimpleService(SessionProviderContext session, MessageHead msg, string str)
            => await SyncOperationHelper.CallSimpleService(session, msg, str.UnicodeStringToBytes());

        protected async Task<CallSyncResultPacket> CallSimpleService(SessionProviderContext session, MessageHead msg, object entity)
            => await SyncOperationHelper.CallSimpleService(session, msg, SiMay.Serialize.Standard.PacketSerializeHelper.SerializePacket(entity));

        protected async Task<CallSyncResultPacket> CallSimpleService(SessionProviderContext session, MessageHead msg, byte[] data = null)
            => await SyncOperationHelper.CallSimpleService(session, msg, data);


    }
}
