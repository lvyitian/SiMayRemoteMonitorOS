using SiMay.Basic;
using SiMay.Net.SessionProvider.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProviderService
{
    public class ApplicationConfiguration
    {
        private static string FileName = Environment.CurrentDirectory + @"\SiMayConfig.ini";

        public static string LogFileName { get; set; } = Path.Combine(Environment.CurrentDirectory, "SiMay.log");
        /// <summary>
        /// 本机地址
        /// </summary>
        public static string LoalAddress
        {
            get { return IniConfigHelper.GetValue("ServiceConfig", "IPAddress", "0.0.0.0", FileName); }
            set { IniConfigHelper.SetValue("ServiceConfig", "IPAddress", value, FileName); }
        }

        /// <summary>
        /// 端口
        /// </summary>
        public static int Port
        {
            get { return int.Parse(IniConfigHelper.GetValue("ServiceConfig", "Port", "522", FileName)); }
            set { IniConfigHelper.SetValue("ServiceConfig", "Port", value.ToString(), FileName); }
        }

        /// <summary>
        /// 连接挂起队列
        /// </summary>
        public static int Backlog
        {
            get { return int.Parse(IniConfigHelper.GetValue("ServiceConfig", "Backlog", "0", FileName)); }
            set { IniConfigHelper.SetValue("ServiceConfig", "Backlog", value.ToString(), FileName); }
        }

        /// <summary>
        /// 是否允许匿名登陆
        /// </summary>
        public static bool AnonyMous
        {
            get { return bool.Parse(IniConfigHelper.GetValue("ServiceConfig", "AnonyMous", "true", FileName)); }
            set { IniConfigHelper.SetValue("ServiceConfig", "AnonyMous", value.ToString(), FileName); }
        }

        /// <summary>
        /// 允许登陆的AccessId
        /// </summary>
        public static string AccessIds
        {
            get { return IniConfigHelper.GetValue("ServiceConfig", "AccessIds", "123456789", FileName); }
            set { IniConfigHelper.SetValue("ServiceConfig", "AccessIds", value.ToString(), FileName); }
        }

        /// <summary>
        /// 主控端登陆密码
        /// </summary>
        public static long MainAppAccessKey
        {
            get { return long.Parse(IniConfigHelper.GetValue("ServiceConfig", "MainAppAccessKey", "5200", FileName)); }
            set { IniConfigHelper.SetValue("ServiceConfig", "MainAppAccessKey", value.ToString(), FileName); }
        }

        /// <summary>
        /// 连接密码
        /// </summary>
        public static long AccessKey
        {
            get { return long.Parse(IniConfigHelper.GetValue("ServiceConfig", "AccessKey", "5200", FileName)); }
            set { IniConfigHelper.SetValue("ServiceConfig", "AccessKey", value.ToString(), FileName); }
        }
    }
}
