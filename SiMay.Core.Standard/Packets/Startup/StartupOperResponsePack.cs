using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class StartupOperResponsePack : EntitySerializerBase
    {
        public OperFlag OperFlag { get; set; }
        public string Msg { get; set; }
        public bool Successed { get; set; }
    }
}
