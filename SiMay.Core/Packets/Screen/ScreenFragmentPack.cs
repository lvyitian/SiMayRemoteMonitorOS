using SiMay.Core.ScreenSpy.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class ScreenFragmentPack : BasePacket
    {
        public Fragment[] Fragments { get; set; }
    }
}
