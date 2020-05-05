using Microsoft.Win32;
using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.RegEdit
{
    public class DoCreateRegistryValuePack : EntitySerializerBase
    {
        public string KeyPath { get; set; }

        public RegistryValueKind Kind { get; set; }
    }
}
