using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProviderService
{
    public class AccessKeyExamine
    {
        public static long ServiceAccessKey = 0;

        static AccessKeyExamine()
        {
            ServiceAccessKey = long.Parse(ApplicationConfiguration.ServiceAccessKey);
        }

        public static bool CheckOut(long key)
        {
            if (key == ServiceAccessKey)
                return true;
            else
                return false;
        }
    }
}
