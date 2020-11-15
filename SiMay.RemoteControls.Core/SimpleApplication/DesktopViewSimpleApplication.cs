using SiMay.Basic;
using SiMay.Core;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControls.Core
{
    public class DesktopViewSimpleApplication : SimpleApplicationBase
    {
        public async Task<MessagePacket> SayHello(SessionProviderContext session)
        {
            return await CallSimpleService(session, MessageHead.S_SIMPLE_MESSAGE_BOX, "你好");
        }

        public Dictionary<string, IDesktopView> DesktopViewCollection { get; } = new Dictionary<string, IDesktopView>();

        public T CreateDesktopView<T>(SessionProviderContext session)
            where T : IDesktopView
        {
            if (session.AppTokens[SysConstants.INDEX_WORKER].IsNull() || !(session.AppTokens[SysConstants.INDEX_WORKER] is SessionSyncContext))
                throw new ArgumentException("只能由主服务会话创建视图!");

            var syncContext = session.AppTokens[SysConstants.INDEX_WORKER].ConvertTo<SessionSyncContext>();
            var desktopView = Activator.CreateInstance<T>();
            desktopView.SessionSyncContext = syncContext;
            DesktopViewCollection[syncContext.UniqueId] = desktopView;

            return desktopView;
        }
    }
}
