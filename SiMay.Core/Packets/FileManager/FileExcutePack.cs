using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.FileManager
{
    public class FileExcutePack : BasePacket
    {
        public string FilePath { get; set; }
    }
}
