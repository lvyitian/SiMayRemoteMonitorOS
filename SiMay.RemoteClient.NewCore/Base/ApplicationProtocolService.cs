using SiMay.Basic;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SiMay.Service.Core
{
    /// <summary>
    /// 应用协议处理服务
    /// </summary>
    public abstract class ApplicationProtocolService : ApplicationServiceBase
    {
        /// <summary>
        /// 数据处理绑定
        /// </summary>
        public PacketModelBinder<SessionProviderContext, MessageHead> HandlerBinder { get; set; }


        public ApplicationProtocolService()
        {
            HandlerBinder = new PacketModelBinder<SessionProviderContext, MessageHead>();
        }
    }
}
