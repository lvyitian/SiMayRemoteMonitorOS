using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Net.SessionProvider.Delegate
{
    public delegate void OnSessionNotify<TEvent, Session>(TEvent e, Session session);
}
