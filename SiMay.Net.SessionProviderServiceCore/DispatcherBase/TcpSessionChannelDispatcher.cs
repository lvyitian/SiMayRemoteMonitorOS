using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Net.SessionProviderServiceCore
{
    public abstract class TcpSessionChannelDispatcher : DispatcherBase
    {
        public virtual void SendTo(byte[] data)
        {
            CurrentSession.SendAsync(data);
        }
    }
}
