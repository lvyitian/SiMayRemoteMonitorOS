using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class SysWindowMaxPack : EntitySerializerBase
    {
        public int State { get; set; }
        public int[] Handlers { get; set; }
    }
}
