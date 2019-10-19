using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.Reg
{
    public class RegRootDirectorysPack : BasePacket
    {
        public string[] RootDirectorys { get; set; }
    }
}
