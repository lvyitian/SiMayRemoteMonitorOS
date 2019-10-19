using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SiMay.Basic
{
    public class IPHelper
    {
        public static string GetHostByName(string host)
        {
            string _return = null;
            try
            {
                IPHostEntry hostinfo = Dns.GetHostByName(host);
                IPAddress[] aryIP = hostinfo.AddressList;
                _return = aryIP[0].ToString();
            }
            catch { }

            return _return;
        }
    }
}
