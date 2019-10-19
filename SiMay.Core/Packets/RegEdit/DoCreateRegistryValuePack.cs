using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.RegEdit
{
    public class DoCreateRegistryValuePack
    {
        public string KeyPath { get; set; }

        public RegistryValueKind Kind { get; set; }
    }
}
