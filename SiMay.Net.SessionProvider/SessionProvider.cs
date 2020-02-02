using SiMay.Net.SessionProvider.Delegate;
using SiMay.Net.SessionProvider.Notify;
using SiMay.Net.SessionProvider.SessionBased;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Net.SessionProvider
{
    public abstract class SessionProvider
    {
        protected OnSessionNotify<SessionCompletedNotify, SessionProviderContext> _onSessionNotifyProc;

        protected SessionProvider(OnSessionNotify<SessionCompletedNotify, SessionProviderContext> onSessionNotifyProc)
        {
            _onSessionNotifyProc = onSessionNotifyProc;
        }

        public abstract void StartSerivce();
        public abstract void BroadcastAsync(byte[] data);
        public abstract void BroadcastAsync(byte[] data, int offset, int lenght);
        public abstract void DisconnectAll();
        public abstract void CloseService();
    }
}