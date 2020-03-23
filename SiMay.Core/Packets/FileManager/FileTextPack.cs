using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class FileOpenTextPack : EntitySerializerBase
    {
        public string FileName { get; set; }
    }
    public class FileTextPack : EntitySerializerBase
    {
        /// <summary>
        /// 是否可以访问
        /// </summary>
        public bool IsSuccess { get; set; }
        public string Text { get; set; }
    }
}
