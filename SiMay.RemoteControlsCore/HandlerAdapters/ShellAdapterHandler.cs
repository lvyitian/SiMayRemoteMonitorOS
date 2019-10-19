using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.Core.Extensions;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Net.SessionProvider.SessionBased;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class ShellAdapterHandler : AdapterHandlerBase
    {
        /// <summary>
        /// 输出命令处理事件
        /// </summary>
        public event Action<ShellAdapterHandler, string> OnOutputCommandEventHandler;

        private PacketModelBinder<SessionHandler> _handlerBinder = new PacketModelBinder<SessionHandler>();
        internal override void MessageReceived(SessionHandler session)
        {
            if (this.IsClose)
                return;

            _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead(), this);
        }

        [PacketHandler(MessageHead.C_SHELL_RESULT)]
        private void OutputCommandHandler(SessionHandler session)
        {
            string text = session.CompletedBuffer.GetMessagePayload().ToUnicodeString();
            this.OnOutputCommandEventHandler?.Invoke(this, text);
        }

        public void InputCommand(string command)
        {
            SendAsyncMessage(MessageHead.S_SHELL_INPUT, command);
        }

        public override void CloseHandler()
        {
            this._handlerBinder.Dispose();
            base.CloseHandler();
        }
    }
}
