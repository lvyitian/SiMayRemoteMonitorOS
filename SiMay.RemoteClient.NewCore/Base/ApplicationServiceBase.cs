using SiMay.Core;
using SiMay.Net.SessionProvider;
using System;

namespace SiMay.ServiceCore
{
    /// <summary>
    /// 基础应用服务
    /// </summary>
    public abstract class ApplicationServiceBase
    {

        /// <summary>
        /// 当前会话
        /// </summary>
        protected SessionProviderContext CurrentSession { get; set; }

        /// <summary>
        /// 设置当前Session
        /// </summary>
        /// <param name="session"></param>
        public void SetSession(SessionProviderContext session)
            => CurrentSession = session;

        /// <summary>
        /// 关闭当前会话
        /// </summary>
        public void CloseSession()
            => CurrentSession.SessionClose();
    }
}