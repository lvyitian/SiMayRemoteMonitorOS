using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Core.Packets.RemoteUpdate
{
    public class DataPacket
    {
        public int TotalSize { get; set; }

        public byte[] Data { get; set; }
    }
}
