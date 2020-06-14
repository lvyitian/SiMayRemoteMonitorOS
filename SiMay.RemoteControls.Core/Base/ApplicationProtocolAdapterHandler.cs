using SiMay.Basic;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControlsCore
{
    public abstract class ApplicationProtocolAdapterHandler : IDisposable
    {
        /// <summary>
        /// 数据处理函数绑定
        /// </summary>
        public PacketModelBinder<SessionProviderContext, MessageHead> HandlerBinder { get; set; }

        public ApplicationProtocolAdapterHandler()
        {
            HandlerBinder = new PacketModelBinder<SessionProviderContext, MessageHead>();
        }


        public virtual void Dispose()
        {
            this.HandlerBinder.Dispose();
        }
    }
}
