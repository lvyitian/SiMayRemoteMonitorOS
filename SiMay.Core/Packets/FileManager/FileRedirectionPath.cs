using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Environment;

namespace SiMay.Core.Packets.FileManager
{
    public class FileRedirectionPath : EntitySerializerBase
    {
        public SpecialFolder SpecialFolder { get; set; }
    }
}
