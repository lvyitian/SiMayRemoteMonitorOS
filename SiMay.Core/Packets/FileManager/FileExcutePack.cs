using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.FileManager
{
    public class FileExcutePack : EntitySerializerBase
    {
        public string FilePath { get; set; }
    }
}
