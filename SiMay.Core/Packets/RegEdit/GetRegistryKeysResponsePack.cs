using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class GetRegistryKeysResponsePack : EntitySerializerBase
    {
        public RegSeekerMatch[] Matches { get; set; }

        public string RootKey { get; set; }

        public bool IsError { get; set; }

        public string ErrorMsg { get; set; }
    }
}
