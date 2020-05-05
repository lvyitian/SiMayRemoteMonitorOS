using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.Reg
{
    public class RegValuesPack : EntitySerializerBase
    {
        public RegValueItem[] Values { get; set; }
    }
}
