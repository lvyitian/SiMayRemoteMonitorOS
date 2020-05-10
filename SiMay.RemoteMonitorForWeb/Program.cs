using SiMay.Basic;
using SiMay.Net.SessionProvider;
using SiMay.RemoteControlsCore;
//using System.Windows.Form;
using System;

namespace SiMay.RemoteMonitorForWeb
{
    class Program
    {


        [STAThread]
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
