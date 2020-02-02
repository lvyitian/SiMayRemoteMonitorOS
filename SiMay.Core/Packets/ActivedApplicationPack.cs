using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class ActivateServicePack : EntitySerializerBase
    {
        public string ApplicationKey { get; set; }
    }
    public class ActivateApplicationPack : EntitySerializerBase
    {
        public string IdentifyId { get; set; }
        public string ServiceKey { get; set; }
        public string OriginName { get; set; }
    }
}
