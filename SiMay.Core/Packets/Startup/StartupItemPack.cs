using SiMay.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.Startup
{
    public class StartupItemsPack : BasePacket
    {
        public StartupItemPack[] StartupItems { get; set; }
    }
    public class StartupItemPack : BasePacket
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public StartupType Type { get; set; }
    }
}
