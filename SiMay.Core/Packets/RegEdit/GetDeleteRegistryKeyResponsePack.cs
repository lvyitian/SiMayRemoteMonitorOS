using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.RegEdit
{
    public class GetDeleteRegistryKeyResponsePack
    {
        public string ParentPath { get; set; }

        public string KeyName { get; set; }

        public bool IsError { get; set; }

        public string ErrorMsg { get; set; }
    }
}
