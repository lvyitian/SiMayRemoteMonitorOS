using SiMay.Core.ScreenSpy.Entitys;
using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class ScreenFragmentPack : EntitySerializerBase
    {
        public Fragment[] Fragments { get; set; }
    }
}
