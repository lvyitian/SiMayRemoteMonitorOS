using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.SysManager
{
    public class SysKillPack : EntitySerializerBase
    {
        public int[] ProcessIds { get; set; }
    }
}
