using SiMay.Core;
using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class StartupItemsPack : EntitySerializerBase
    {
        public StartupItemPack[] StartupItems { get; set; }
    }
    public class StartupItemPack : EntitySerializerBase
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public StartupType Type { get; set; }
    }
}
