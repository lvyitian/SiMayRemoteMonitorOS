using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.FileManager
{
    public class FileReNamePack : EntitySerializerBase
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
