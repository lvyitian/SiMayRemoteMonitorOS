using SiMay.Basic;
using SiMay.Core;
using SiMay.Net.SessionProvider;
using SiMay.RemoteControls.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControls.Core
{
    /// <summary>
    /// 应用处理器基类
    /// </summary>
    public abstract class ApplicationBaseAdapterHandler : ApplicationProtocolAdapterHandler
    {
        /// <summary>
        /// 当前会话对象
        /// </summary>
        protected SessionProviderContext CurrentSession { get; set; }

        /// <summary>
        /// 简单程序集合
        /// </summary>
        public IDictionary<string, SimpleApplicationBase> SimpleApplicationCollection
            => SimpleApplicationHelper.SimpleApplicationCollection;

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
            App.SessionClose(this);
        }

        public virtual void CloseSession()
        {
            this._attachedConnection = false;
            this._manualClose = true;
            this.HandlerBinder.Dispose();
            SendToAsync(MessageHead.S_GLOBAL_ONCLOSE);
        }

        /// <summary>
        /// 应用服务异步调用
        /// </summary>
        /// <param name="msg">调用远程目标消息头</param>
        /// <param name="data">发送到远程的消息</param>
        public void SendToAsync(MessageHead msg, string str)
            => SendToAsync(msg, str.UnicodeStringToBytes());

        public void SendToAsync(MessageHead msg, object entity)
            => SendToAsync(msg, SiMay.Serialize.Standard.PacketSerializeHelper.SerializePacket(entity));

        public void SendToAsync(MessageHead msg, byte[] datas = null)
            => CurrentSession.SendTo(msg, datas);

        /// <summary>
        /// 应用服务同步调用
        /// </summary>
        /// <param name="msg">远程调用的目标消息头</param>
        /// <param name="data">发送到远程的消息</param>
        /// <returns>远程执行返回的消息结果</returns>
        public async Task<CallSyncResultPacket> SendTo(MessageHead msg, string str)
            => await SendTo(msg, str.UnicodeStringToBytes());

        public async Task<CallSyncResultPacket> SendTo(MessageHead msg, object entity)
            => await SendTo(msg, SiMay.Serialize.Standard.PacketSerializeHelper.SerializePacket(entity));

        public async Task<CallSyncResultPacket> SendTo(MessageHead msg, byte[] data = null)
            => await SyncOperationHelper.SendTo(CurrentSession, msg, data);
    }
}
