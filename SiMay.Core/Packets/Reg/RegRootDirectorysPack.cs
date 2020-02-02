using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.Reg
{
    public class RegRootDirectorysPack : EntitySerializerBase
    {
        public string[] RootDirectorys { get; set; }
    }
}
