using Microsoft.Win32;
using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class RegValueData : EntitySerializerBase
    {
        public string Name { get; set; }

        public RegistryValueKind Kind { get; set; }

        public byte[] Data { get; set; }
    }
}
