using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.RegEdit
{
    public class GetCreateRegistryKeyResponsePack
    {
        public string ParentPath { get; set; }

        public RegSeekerMatch Match { get; set; }

        public bool IsError { get; set; }

        public string ErrorMsg { get; set; }
    }
}
