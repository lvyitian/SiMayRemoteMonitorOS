using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.FileManager
{
    /// <summary>
    /// 删除文件
    /// </summary>
    public class FileDeletePack : BasePacket
    {
        public string[] FileNames { get; set; }
    }
    public class FileDeleteFinishPack : BasePacket
    {
        /// <summary>
        /// 删除成功的文件
        /// </summary>
        public string[] DeleteFileNames { get; set; }
    }
}
