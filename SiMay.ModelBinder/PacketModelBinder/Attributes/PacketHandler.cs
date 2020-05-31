using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.ModelBinder
{
    public class PacketHandler : Attribute
    {
        public object MessageHead { get; set; }
        public PacketHandler(object head)
            => MessageHead = head;
    }
}
