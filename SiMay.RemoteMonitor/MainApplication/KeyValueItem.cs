using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteMonitor
{
    public class KeyValueItem
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return Key;
        }
    }
}
