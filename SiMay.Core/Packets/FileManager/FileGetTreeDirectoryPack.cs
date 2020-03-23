using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class FileGetTreeDirectoryPack : EntitySerializerBase
    {
        public string TargetRoot { get; set; }
    }

    public class FileTreeDirFilePack : EntitySerializerBase
    {
        public FileItem[] FileList { get; set; }
        public string Message { get; set; }
        public bool IsSccessed { get; set; }
    }
}
