using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.Core.PacketModelBinding;
using SiMay.Net.SessionProvider.SessionBased;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class RemoteScreenAdapterHandler : AdapterHandlerBase
    {
        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        internal override void MessageReceived(SessionHandler session)
        {
            if (this.IsClose)
                return;

            _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
        }

        public override void CloseHandler()
        {
            this._handlerBinder.Dispose();
            base.CloseHandler();
        }
    }
}
