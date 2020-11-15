using SiMay.Basic;
using SiMay.Core;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControls.Core
{
    public class SyncOperationHelper
    {
        /// <summary>
        /// 同步调用简单程序
        /// </summary>
        /// <param name="session"></param>
        /// <param name="msg"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static async Task<CallSyncResultPacket> CallSimpleService(SessionProviderContext session, MessageHead msg, string str)
            => await internalSendTo(session, MessageHead.S_GLOBAL_SIMPLEAPP_SYNC_CALL, msg, str.UnicodeStringToBytes());

        public static async Task<CallSyncResultPacket> CallSimpleService(SessionProviderContext session, MessageHead msg, object entity)
            => await internalSendTo(session, MessageHead.S_GLOBAL_SIMPLEAPP_SYNC_CALL, msg, SiMay.Serialize.Standard.PacketSerializeHelper.SerializePacket(entity));

        public static async Task<CallSyncResultPacket> CallSimpleService(SessionProviderContext session, MessageHead msg, byte[] data = null)
            => await internalSendTo(session, MessageHead.S_GLOBAL_SIMPLEAPP_SYNC_CALL, msg, data);



        /// <summary>
        /// 同步调用
        /// </summary>
        /// <param name="session"></param>
        /// <param name="msg"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static async Task<CallSyncResultPacket> SendTo(SessionProviderContext session, MessageHead msg, string str)
            => await internalSendTo(session, MessageHead.S_GLOBAL_SYNC_CALL, msg, str.UnicodeStringToBytes());

        public static async Task<CallSyncResultPacket> SendTo(SessionProviderContext session, MessageHead msg, object entity)
            => await internalSendTo(session, MessageHead.S_GLOBAL_SYNC_CALL, msg, SiMay.Serialize.Standard.PacketSerializeHelper.SerializePacket(entity));

        public static async Task<CallSyncResultPacket> SendTo(SessionProviderContext session, MessageHead msg, byte[] data = null)
            => await internalSendTo(session, MessageHead.S_GLOBAL_SYNC_CALL, msg, data);

        private static async Task<CallSyncResultPacket> internalSendTo(SessionProviderContext session, MessageHead globalHead, MessageHead msg, byte[] data = null)
        {
            var asyncSequence = session.AppTokens[SysConstants.INDEX_SYNC_SEQUENCE].ConvertTo<ConcurrentDictionary<int, ApplicationSyncAwaiter>>();

            var id = Guid.NewGuid().GetHashCode();

            var waitTranDatas = MessageHelper.CopyMessageHeadTo(msg, data ?? Array.Empty<byte>());
            byte[] bytes = MessageHelper.CopyMessageHeadTo(globalHead,
                new CallSyncPacket
                {
                    Id = id,
                    Datas = waitTranDatas
                });
            session.SendAsync(bytes);

            return await SyncAwait(asyncSequence, id);
        }

        private static ApplicationSyncAwaiter SyncAwait(IDictionary<int, ApplicationSyncAwaiter> asyncOperationSequence, int id)
            => new ApplicationSyncAwaiter(asyncOperationSequence, id);
    }
}
