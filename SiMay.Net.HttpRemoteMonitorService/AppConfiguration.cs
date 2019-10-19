using SiMay.Basic;
using SiMay.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.HttpRemoteMonitorService
{
    public class AppConfiguration
    {
        private static string IniFilePath = Environment.CurrentDirectory + @"\SiMayWebRemoteMonitorService.ini";
        public static string SessionServiceIPAddress
        {
            get { return IniConfigHelper.GetValue("ServiceConfig", "SessionServiceIPAddress", "127.0.0.1", IniFilePath); }
            set { IniConfigHelper.SetValue("ServiceConfig", "SessionServiceIPAddress", value, IniFilePath); }
        }

        public static string SessionServicePort
        {
            get { return IniConfigHelper.GetValue("ServiceConfig", "SessionServicePort", "522", IniFilePath); }
            set { IniConfigHelper.SetValue("ServiceConfig", "SessionServicePort", value, IniFilePath); }
        }

        public static string AccessKey
        {
            get { return IniConfigHelper.GetValue("ServiceConfig", "AccessKey", "522222", IniFilePath); }
            set { IniConfigHelper.SetValue("ServiceConfig", "AccessKey", value, IniFilePath); }
        }

        public static string ServiceIPAddress
        {
            get { return IniConfigHelper.GetValue("ServiceConfig", "ServiceIPAddress", "0.0.0.0", IniFilePath); }
            set { IniConfigHelper.SetValue("ServiceConfig", "ServiceIPAddress", value, IniFilePath); }
        }

        public static string ServicePort
        {
            get { return IniConfigHelper.GetValue("ServiceConfig", "ServicePort", "523", IniFilePath); }
            set { IniConfigHelper.SetValue("ServiceConfig", "ServicePort", value, IniFilePath); }
        }

        public static string LoginId
        {
            get { return IniConfigHelper.GetValue("ServiceConfig", "LoginId", "123456789", IniFilePath); }
            set { IniConfigHelper.SetValue("ServiceConfig", "LoginId", value, IniFilePath); }
        }

        public static string LoginPassWord
        {
            get { return IniConfigHelper.GetValue("ServiceConfig", "LoginPassWord", "123456789", IniFilePath); }
            set { IniConfigHelper.SetValue("ServiceConfig", "LoginPassWord", value, IniFilePath); }
        }
    }
}
