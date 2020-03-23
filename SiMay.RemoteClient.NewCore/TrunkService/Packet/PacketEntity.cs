using SiMay.Core;
using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.ServiceCore
{
    public class ActivePack : EntitySerializerBase
    {
        public int SessionId { get; set; }
    }

    public class SessionStatusPack : EntitySerializerBase
    {
        public SessionItem[] Sessions { get; set; }
    }

    public class CreateUserProcessPack : EntitySerializerBase
    {
        public int SessionId { get; set; }
    }
}
