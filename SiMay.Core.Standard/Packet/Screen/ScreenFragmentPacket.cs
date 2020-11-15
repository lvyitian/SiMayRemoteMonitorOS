using SiMay.Platform;
using SiMay.Platform.Windows;
using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class ScreenFragmentPacket : EntitySerializerBase
    {
        public Fragment[] Fragments { get; set; }
    }
}
