using SiMay.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class FileListPack : BasePacket
    {
        public string FilePath { get; set; }

    }
    public class FileListItemsPack : BasePacket
    {
        public FileItem[] FileList { get; set; }
        public string Path { get; set; }
        public string Message { get; set; }
        public bool IsSccessed { get; set; }
    }
    public class FileItem
    {
        public string FileName { get; set; }

        public long FileSize { get; set; }
        public long UsingSize { get; set; }
        public long FreeSize { get; set; }

        public FileType FileType { get; set; }

        public DateTime LastAccessTime { get; set; }
    }
}
