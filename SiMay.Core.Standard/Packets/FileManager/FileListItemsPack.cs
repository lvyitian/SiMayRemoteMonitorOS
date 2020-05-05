using SiMay.Core;
using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class FileListPack : EntitySerializerBase
    {
        public string FilePath { get; set; }

    }
    public class FileListItemsPack : EntitySerializerBase
    {
        public FileItem[] FileList { get; set; }
        public string Path { get; set; }
        public string Message { get; set; }
        public bool IsSccessed { get; set; }
    }
    public class FileItem : EntitySerializerBase
    {
        public string FileName { get; set; }

        public long FileSize { get; set; }
        public long UsingSize { get; set; }
        public long FreeSize { get; set; }

        public FileType FileType { get; set; }

        public DateTime LastAccessTime { get; set; }
    }
}
