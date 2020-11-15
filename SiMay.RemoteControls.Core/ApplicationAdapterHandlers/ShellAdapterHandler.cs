using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;

namespace SiMay.RemoteControls.Core
{
    [ApplicationServiceKey(ApplicationKeyConstant.REMOTE_SHELL)]
    public class ShellAdapterHandler : ApplicationBaseAdapterHandler
    {
        /// <summary>
        /// 输出命令处理事件
        /// </summary>
        public event Action<ShellAdapterHandler, string> OnOutputCommandEventHandler;

        [PacketHandler(MessageHead.C_SHELL_RESULT)]
        private void OutputCommandHandler(SessionProviderContext session)
        {
            string text = session.GetMessage().ToUnicodeString();
            this.OnOutputCommandEventHandler?.Invoke(this, text);
        }

        public void InputCommand(string command)
        {
            SendToAsync( MessageHead.S_SHELL_INPUT, command);
        }
    }
}
