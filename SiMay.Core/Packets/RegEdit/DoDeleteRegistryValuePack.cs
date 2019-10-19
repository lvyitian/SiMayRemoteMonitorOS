using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.RegEdit
{
    public class DoDeleteRegistryValuePack
    {
        public string KeyPath { get; set; }

        public string ValueName { get; set; }
    }
}
