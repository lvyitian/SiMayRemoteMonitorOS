using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class DoChangeRegistryValuePack : EntitySerializerBase
    {
        public string KeyPath { get; set; }

        public RegValueData Value { get; set; }
    }
}
