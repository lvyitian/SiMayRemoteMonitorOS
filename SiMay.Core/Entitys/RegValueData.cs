using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Entitys
{
    public class RegValueData
    {
        public string Name { get; set; }

        public RegistryValueKind Kind { get; set; }

        public byte[] Data { get; set; }
    }
}
