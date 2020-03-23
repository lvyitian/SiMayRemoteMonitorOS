using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Core
{
    public enum FileLocation
    {
        ExecuteDirectory,
        TargetDirectory
    }
    public class FileTransmissionCreatePacket : EntitySerializerBase
    {
        /// <summary>
        /// 文件储存位置
        /// </summary>
        public FileLocation FileLocation { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize { get; set; }
    }

    public class FileTransmissionResultPacket : EntitySerializerBase
    {
        public bool TransmissionSuccess { get; set; }

        public string FileName { get; set; }
    }
}
