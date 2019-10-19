using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.FileManager
{
    public class FileGetTreeDirectoryPack
    {
        public string TargetRoot { get; set; }
    }

    public class FileTreeDirFilePack
    {
        public FileItem[] FileList { get; set; }
        public string Message { get; set; }
        public bool IsSccessed { get; set; }
    }
}
