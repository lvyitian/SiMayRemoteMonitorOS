using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Sockets.Delegate
{
    public delegate void NotifyEventHandler<Tevent, TSession>(Tevent e, TSession session);
}
