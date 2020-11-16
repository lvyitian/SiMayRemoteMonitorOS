using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Service.Core
{
    public class RemoteSimpleServiceBase : IRemoteSimpleService
    {
        /// <summary>
        /// 数据处理绑定
        /// </summary>
        public PacketModelBinder<SessionProviderContext, MessageHead> HandlerBinder { get; set; }


        public RemoteSimpleServiceBase()
        {
            HandlerBinder = new PacketModelBinder<SessionProviderContext, MessageHead>();
        }
    }
}
