using SiMay.Core;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public object StateContext { get; set; }

        /// <summary>
        /// 应用唯一标识
        /// </summary>
        public string ApplicationKey { get; set; }

        /// <summary>
        /// 来源备注名
        /// </summary>
        public string OriginName { get; set; }

        /// <summary>
        /// 被控服务身份Id+AppKey
        /// </summary>
        public string IdentifyId { get; set; }

        /// <summary>
        /// 当前会话是否关闭
        /// </summary>
        public bool WhetherClose { get; private set; }

        /// <summary>
        /// 展示应用对象
        /// </summary>
        public IApplication App { get; set; }

        /// <summary>
        /// 当会话重连后触发
        /// </summary>
        /// <param name="session"></param>
        public virtual void ContinueTask(SessionProviderContext session)
            => App.ContinueTask(this);

        /// <summary>
        /// 当会话中断后触发
        /// </summary>
        /// <param name="session"></param>
        public virtual void SessionClosed(SessionProviderContext session)
            => App.SessionClose(this);

        public virtual void CloseSession()
        {
            this.WhetherClose = true;
            this.HandlerBinder.Dispose();
            SendTo(CurrentSession, MessageHead.S_GLOBAL_ONCLOSE);
        }
    }
}
