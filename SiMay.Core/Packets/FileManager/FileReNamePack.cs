using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.FileManager
{
    public class FileReNamePack : BasePacket
    {
        public string SourceFileName { get; set; }
        public string TargetName { get; set; }
    }

    public class FileReNameFinishPack : BasePacket
    {
        public bool IsSuccess { get; set; }
        public string SourceFileName { get; set; }
        public string TargetName { get; set; }
    }
}
