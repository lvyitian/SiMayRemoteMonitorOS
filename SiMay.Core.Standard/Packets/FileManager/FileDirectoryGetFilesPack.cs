using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    /// <summary>
    /// 获取文件夹文件
    /// </summary>
    public class FileDirectoryGetFilesPack : EntitySerializerBase
    {
        public string DirectoryPath { get; set; }
    }

    public class FileDirectoryFilesPack : EntitySerializerBase
    {
        /// <summary>
        /// 文件信息
        /// </summary>
        public DirectoryFileItem[] Files { get; set; }
    }

    public class DirectoryFileItem : EntitySerializerBase
    {
        public DirectoryFileType Type { get; set; }
        public string FileName { get; set; }
    }

    public enum DirectoryFileType
    {
        File,
        Directory
    }
}
