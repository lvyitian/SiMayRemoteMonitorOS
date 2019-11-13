using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.ServiceCore
{
    public class ActivePack
    {
        public int SessionId { get; set; }
    }

    public class SessionStatusPack
    {
        public SessionItem[] Sessions { get; set; }
    }

    public class CreateUserProcessPack
    {
        public int SessionId { get; set; }
    }

    public class SessionItem
    {
        public int SessionId { get; set; }
        public bool HasUserProcess { get; set; }
    }
}
