using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteControlsCore
{
    public class SessionSyncContext
    {
        public SessionSyncContext(SessionProviderContext session, IDictionary<string, object> dictions)
        {
            Session = session;
            KeyDictions = dictions;
        }
        public object this[string key]
        {
            get
            {
                return KeyDictions[key];
            }
            set
            {
                KeyDictions[key] = value;
            }
        }
        public string UniqueId { get; set; } = Guid.NewGuid().ToString();
        public SessionProviderContext Session { get; set; }
        public IDictionary<string, object> KeyDictions { get; set; }
    }
}
