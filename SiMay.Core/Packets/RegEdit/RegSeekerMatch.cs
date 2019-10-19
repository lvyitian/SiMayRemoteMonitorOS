using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets.RegEdit
{
    public class RegSeekerMatch
    {
        public string Key { get; set; }

        public RegValueData[] Data { get; set; }

        public bool HasSubKeys { get; set; }

        public override string ToString()
        {
            return $"({Key}:{Data})";
        }
    }
}
