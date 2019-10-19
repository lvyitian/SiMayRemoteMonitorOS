using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteMonitor.Notify
{
    public enum SessionNotifyType
    {
        Message,
        OnReceive,
        ContinueTask,
        SessionClosed,
        WindowShow,
        WindowClose
    }
}
