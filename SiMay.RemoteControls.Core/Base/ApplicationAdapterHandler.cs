using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.Standard.Packets;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControlsCore
{
    public abstract class ApplicationAdapterHandler : ApplicationProtocolAdapterHandler
    {
        /// <summary>
        /// 当前会话对象
        /// </summary>
        protected SessionProviderContext CurrentSession { get; set; }

        /// <summary>
        /// 设置当前会话
        /// </summary>
        /// <param name="session"></param>
        public void SetSession(SessionProviderContext session)
            => CurrentSession = session;

        /// <summary>
        /// 用户状态上下文
        /// </summary>
        public object State { get; set; } = string.Empty;

        /// <summary>
        /// 来源备注名
        /// </summary>
        public string OriginName { get; set; }

        /// <summary>
        /// 被控服务身份Id+AppKey
        /// </summary>
        public string IdentifyId { get; set; }

        /// <summary>
        /// 当前会话是否由用户关闭
        /// </summary>
        private bool _manualClose;

        public bool IsManualClose()
            => _manualClose;

        /// <summary>
        /// 绝对连接状态
        /// </summary>
        private bool _attachedConnection { get; set; } = true;

        public bool GetAttachedConnectionState()
            => _attachedConnection;

        /// <summary>
        /// 展示应用对象
        /// </summary>
        public IApplication App { get; set; }

        /// <summary>
        /// 当会话重连后触发
        /// </summary>
        /// <param name="session"></param>
        public virtual void ContinueTask(SessionProviderContext session)
        {
            //再发出重连命令后，如果使用者主动关闭消息处理器将不再建立连接
            if (this.IsManualClose())
            {
                //通知远程释放资源
                session.SendTo(MessageHead.S_GLOBAL_ONCLOSE);
            }
            else
            {
                _attachedConnection = true;
                App.ContinueTask(this);
            }
        }
        /// <summary>
        /// 当会话中断后触发
        /// </summary>
        /// <param name="session"></param>
        public virtual void SessionClosed(SessionProviderContext session)
        {
            _attachedConnection = false;

            //释放所有异步任务
            foreach (var operation in _asyncOperationSequence.Values.ToArray())
                operation.Complete(null);

            App.SessionClose(this);
        }

        public virtual void CloseSession()
        {
            this._attachedConnection = false;
            this._manualClose = true;
            this.HandlerBinder.Dispose();
            CurrentSession.SendTo(MessageHead.S_GLOBAL_ONCLOSE);
        }
        /// <summary>
        /// 异步任务回调队列
        /// </summary>
        private IDictionary<int, ApplicationSyncAwaiter> _asyncOperationSequence = new Dictionary<int, ApplicationSyncAwaiter>();

        /// <summary>
        /// 同步完成
        /// </summary>
        public void CallSyncCompleted()
        {
            var syncResult = CurrentSession.GetMessageEntity<CallSyncResultPacket>();

            if (_asyncOperationSequence.ContainsKey(syncResult.Id))
                _asyncOperationSequence[syncResult.Id].Complete(syncResult);
        }

        public async Task<CallSyncResultPacket> SendTo(MessageHead msg, object entity)
            => await SendTo(msg, SiMay.Serialize.Standard.PacketSerializeHelper.SerializePacket(entity));

        public async Task<CallSyncResultPacket> SendTo(MessageHead msg, byte[] data = null)
        {
            var id = Guid.NewGuid().GetHashCode();
            byte[] bytes = MessageHelper.CopyMessageHeadTo(MessageHead.S_GLOBAL_SYNC_CALL,
                new CallSyncPacket
                {
                    Id = id,
                    TargetMessageHead = msg,
                    Datas = data ?? Array.Empty<byte>()
                });
            CurrentSession.SendAsync(bytes);

            return await SyncOperationHelper.SyncAwait(_asyncOperationSequence, id);//异步等待
        }

    }
}
