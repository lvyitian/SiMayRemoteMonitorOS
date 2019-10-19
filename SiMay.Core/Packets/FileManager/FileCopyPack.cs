using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.FileManager
{
    /// <summary>
    /// 复制文件
    /// </summary>
    public class FileCopyPack : BasePacket
    {
        public string[] FileNames { get; set; }
        public string TargetDirectoryPath { get; set; }
    }
    /// <summary>
    /// 复制结束
    /// </summary>
    public class FileCopyFinishPack : BasePacket
    {
        /// <summary>
        /// 复制异常的文件
        /// </summary>
        public string[] ExceptionFileNames { get; set; }
    }
}
