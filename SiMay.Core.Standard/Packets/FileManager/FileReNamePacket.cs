using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class FileReNamePacket : EntitySerializerBase
    {
        public string SourceFileName { get; set; }
        public string TargetName { get; set; }
    }

    public class FileReNameFinishPack : EntitySerializerBase
    {
        public bool IsSuccess { get; set; }
        public string SourceFileName { get; set; }
        public string TargetName { get; set; }
    }
}
