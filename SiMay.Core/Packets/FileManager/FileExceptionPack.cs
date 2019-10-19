using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.FileManager
{
    public class FileExceptionPack : BasePacket
    {
        public DateTime OccurredTime { get; set; }
        public string TipMessage { get; set; }
        public string StackTrace { get; set; }
        public string ExceptionMessage { get; set; }
    }
}
