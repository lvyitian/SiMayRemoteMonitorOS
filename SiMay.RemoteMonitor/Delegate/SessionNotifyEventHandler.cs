using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteMonitor.Delegate
{
    public delegate void SessionNotifyEventHandler<Session, Notify>(Session session, Notify notify);
}
