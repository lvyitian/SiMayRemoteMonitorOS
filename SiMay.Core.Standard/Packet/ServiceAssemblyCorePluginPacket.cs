using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class ServiceAssemblyCorePluginPacket : EntitySerializerBase
    {
        public AssemblyFileItem[] Files { get; set; }
    }

    public class AssemblyFileItem : EntitySerializerBase
    {
        /// <summary>
        /// 程序集名
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// 程序集字节集
        /// </summary>
        public byte[] Data { get; set; }
    }
}
