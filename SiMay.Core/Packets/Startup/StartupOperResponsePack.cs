using SiMay.Core.Packets.Startup.Enums;
using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.Startup
{
    public class StartupOperResponsePack : EntitySerializerBase
    {
        public OperFlag OperFlag { get; set; }
        public string Msg { get; set; }
        public bool Successed { get; set; }
    }
}
