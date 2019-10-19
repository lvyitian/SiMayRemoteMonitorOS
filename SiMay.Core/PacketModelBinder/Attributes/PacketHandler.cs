using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.PacketModelBinder.Attributes
{
    public class PacketHandler : Attribute
    {
        public MessageHead MessageHead { get; set; }
        public PacketHandler(MessageHead head)
            => MessageHead = head;
    }
}
