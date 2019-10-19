using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.RegEdit
{
    public class DoChangeRegistryValuePack
    {
        public string KeyPath { get; set; }

        public RegValueData Value { get; set; }
    }
}
