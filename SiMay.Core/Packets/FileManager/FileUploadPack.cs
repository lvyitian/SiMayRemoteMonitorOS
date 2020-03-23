using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class FileUploadPack : EntitySerializerBase
    {
        public string FileName { get; set; }
    }

    public class FileUploadFileStatus : EntitySerializerBase
    {
        /// <summary>
        /// 文件打开状态 0:文件不存在，1文件存在，2文件访问失败
        /// </summary>
        public int Status { get; set; }
        public long Position { get; set; }
    }

    public class FileFristUploadDataPack : EntitySerializerBase
    {
        public int FileMode { get; set; }
        public long Position { get; set; }
        public long FileSize { get; set; }
        public byte[] Data { get; set; }
    }

    public class FileUploadDataPack : EntitySerializerBase
    {
        public long FileSize { get; set; }
        public byte[] Data { get; set; }
    }
}
