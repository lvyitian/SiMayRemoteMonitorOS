using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.FileManager
{
    /// <summary>
    /// 文件下载
    /// </summary>
    public class FileDownloadPack : EntitySerializerBase
    {
        public string FileName { get; set; }
        /// <summary>
        /// 本地文件断点
        /// </summary>
        public long Position { get; set; }
    }

    /// <summary>
    /// 首数据包(减少交互快速传输文件)
    /// </summary>
    public class FileFristDownloadDataPack : EntitySerializerBase
    {
        //public string fileName { get; set; }
        /// <summary>
        /// 远程文件状态，1文件存在，0文件访问异常,-1文件不存在
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 文件长度
        /// </summary>
        public long FileSize { get; set; }
        /// <summary>
        /// 首数据块
        /// </summary>
        public byte[] Data { get; set; }
    }

    public class FileDownloadDataPack : EntitySerializerBase
    {
        public byte[] Data { get; set; }
    }
}
