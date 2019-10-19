using SiMay.Basic;
using SiMay.Net.SessionProvider.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProviderService
{
    public class ApplicationConfiguration
    {
        private static string IniFilePath = Environment.CurrentDirectory + @"\SiMayConfig.ini";
        public static string IPAddress
        {
            get { return IniConfigHelper.GetValue("ServiceConfig", "IPAddress", "0.0.0.0", IniFilePath); }
            set { IniConfigHelper.SetValue("ServiceConfig", "IPAddress", value, IniFilePath); }
        }

        public static string Port
        {
            get { return IniConfigHelper.GetValue("ServiceConfig", "Port", "522", IniFilePath); }
            set { IniConfigHelper.SetValue("ServiceConfig", "Port", value, IniFilePath); }
        }

        public static string Backlog
        {
            get { return IniConfigHelper.GetValue("ServiceConfig", "Backlog", "0", IniFilePath); }
            set { IniConfigHelper.SetValue("ServiceConfig", "Backlog", value, IniFilePath); }
        }

        public static string ServiceAccessKey
        {
            get { return IniConfigHelper.GetValue("ServiceConfig", "ServiceAccessKey", "522222", IniFilePath); }
            set { IniConfigHelper.SetValue("ServiceConfig", "ServiceAccessKey", value, IniFilePath); }
        }
    }
}
