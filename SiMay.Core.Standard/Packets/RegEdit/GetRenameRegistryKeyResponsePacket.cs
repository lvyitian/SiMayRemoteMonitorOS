using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class GetRenameRegistryKeyResponsePacket : EntitySerializerBase
    {
        public string ParentPath { get; set; }

        public string OldKeyName { get; set; }

        public string NewKeyName { get; set; }

        public bool IsError { get; set; }

        public string ErrorMsg { get; set; }
    }
}
