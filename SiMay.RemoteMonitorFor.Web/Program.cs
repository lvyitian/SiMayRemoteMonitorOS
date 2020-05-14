using SiMay.RemoteMonitorForWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteMonitorFor.Web
{
    class Program
    {
        static void Main(string[] args)
        {
            WebRemoteMonitorService service = new WebRemoteMonitorService();
            service.LanuchService();

            var key = string.Empty;
            while (!key.Equals("exit"))
                key = Console.ReadLine();
        }
    }
}
