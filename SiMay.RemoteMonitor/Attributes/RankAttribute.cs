using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteMonitor
{
    public class RankAttribute : Attribute
    {
        public int Rank { get; set; }

        public RankAttribute(int rank) => Rank = rank;
    }
}
