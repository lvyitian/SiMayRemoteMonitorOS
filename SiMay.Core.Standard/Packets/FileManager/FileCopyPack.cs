using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    /// <summary>
    /// 复制文件
    /// </summary>
    public class FileCopyPack : EntitySerializerBase
    {
        public string[] FileNames { get; set; }
        public string TargetDirectoryPath { get; set; }
    }
    /// <summary>
    /// 复制结束
    /// </summary>
    public class FileCopyFinishPack : EntitySerializerBase
    {
        /// <summary>
        /// 复制异常的文件
        /// </summary>
        public string[] ExceptionFileNames { get; set; }
    }
}
