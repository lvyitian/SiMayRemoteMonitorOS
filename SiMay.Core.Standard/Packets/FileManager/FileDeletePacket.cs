using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    /// <summary>
    /// 删除文件
    /// </summary>
    public class FileDeletePacket : EntitySerializerBase
    {
        public string[] FileNames { get; set; }
    }
    public class FileDeleteFinishPack : EntitySerializerBase
    {
        /// <summary>
        /// 删除成功的文件
        /// </summary>
        public string[] DeleteFileNames { get; set; }
    }
}
