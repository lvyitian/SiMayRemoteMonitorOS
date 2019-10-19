using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.RegEdit
{
    public class GetRenameRegistryValueResponsePack
    {
        public string KeyPath { get; set; }

        public string OldValueName { get; set; }

        public string NewValueName { get; set; }

        public bool IsError { get; set; }

        public string ErrorMsg { get; set; }
    }
}
