using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.RemoteControlsCore
{
    public class SuspendTaskResumEventArgs : EventArgs
    {
        public SessionProviderContext Session { get; set; }

        /// <summary>
        /// 来源名称
        /// </summary>
        public string OriginName { get; set; }
    }
}
