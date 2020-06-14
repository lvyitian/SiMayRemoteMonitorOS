using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteControlsCore
{
    public class SuspendTaskContext
    {
        public DateTime DisconnectTime { get; set; }
        public ApplicationAdapterHandler AdapterHandler { get; set; }
    }
}
