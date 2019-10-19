//using SiMay.Basic;
//using SiMay.Core;
//using SiMay.Core.AppConfig;
//using System.Collections.Generic;
//using System.Windows.Forms;

//namespace SiMay.RemoteMonitor
//{

//    public class ManagerAppConfig : AbstractAppConfigBase
//    {
//        Dictionary<string, string> _defaultConfig = new Dictionary<string, string>()
//        {
//            { "IPAddress" ,"0.0.0.0" },
//            { "Port" , "5200" },
//            { "ConnectPassWord", "5200" },
//            { "MaxConnectCount", "100000" },
//            { "Maximize", "false" },
//            { "DesktopViewHeight", "220" },
//            { "DesktopViewWidth", "280" },
//            { "lHosts", "127.0.0.1:5200" },
//            { "DbClickViewExc", "" },
//            { "LockPassWord", "5200" },
//            { "WindowsIsLock", "false" },
//            { "DesktopRefreshTimeSpan", "1500" },
//            { "AudioSamplesPerSecond", "8000" },
//            { "AudioBitsPerSample", "16" },
//            { "AudioChannels", "1" },
//            { "SessionMode", "0" },
//            { "AccessKey", "522222" },
//            { "ServiceIPAddress", "127.0.0.1" },
//            { "ServicePort", "522" }
//        };

//        string _filePath = Application.StartupPath + @"\SiMayConfig.ini";

//        public override string GetConfig(string key)
//        {
//            if (!AppConfig.ContainsKey(key) && _defaultConfig.ContainsKey(key))
//                AppConfig[key] = IniConfigHelper.GetValue("SiMayConfig", key, _defaultConfig[key], _filePath);

//            string val;
//            if (AppConfig.TryGetValue(key, out val))
//                return val;
//            else
//                return null;
//        }

//        public override bool SetConfig(string key, string value)
//        {
//            AppConfig[key] = value;
//            IniConfigHelper.SetValue("SiMayConfig", key, value, _filePath);
//            return true;
//        }
//    }
//    public class AppConfiguration
//    {

//        public static AbstractAppConfigBase SysConfig { get; } = new ManagerAppConfig();

//        public static string IPAddress
//        {
//            get { return SysConfig.GetConfig("IPAddress"); }
//            set { SysConfig.SetConfig("IPAddress", value); }
//        }

//        public static string Port
//        {
//            get { return SysConfig.GetConfig("Port"); }
//            set { SysConfig.SetConfig("Port", value); }
//        }

//        public static string ConnectPassWord
//        {
//            get { return SysConfig.GetConfig("ConnectPassWord"); }
//            set { SysConfig.SetConfig("ConnectPassWord", value); }
//        }

//        public static string MaxConnectCount
//        {
//            get { return SysConfig.GetConfig("MaxConnectCount"); }
//            set { SysConfig.SetConfig("MaxConnectCount", value); }
//        }

//        public static string WindowMaximize
//        {
//            get { return SysConfig.GetConfig("Maximize"); }
//            set { SysConfig.SetConfig("Maximize", value); }
//        }

//        public static string DesktopViewHeight
//        {
//            get { return SysConfig.GetConfig("DesktopViewHeight"); }
//            set { SysConfig.SetConfig("DesktopViewHeight", value); }
//        }

//        public static string DesktopViewWidth
//        {
//            get { return SysConfig.GetConfig("DesktopViewWidth"); }
//            set { SysConfig.SetConfig("DesktopViewWidth", value); }
//        }

//        public static string LHostString
//        {
//            get { return SysConfig.GetConfig("lHosts"); }
//            set { SysConfig.SetConfig("lHosts", value); }
//        }

//        public static string DbClickViewExc
//        {
//            get { return SysConfig.GetConfig("DbClickViewExc"); }
//            set { SysConfig.SetConfig("DbClickViewExc", value); }
//        }

//        public static string LockPassWord
//        {
//            get { return SysConfig.GetConfig("LockPassWord"); }
//            set { SysConfig.SetConfig("LockPassWord", value); }
//        }

//        public static string WindowsIsLock
//        {
//            get { return SysConfig.GetConfig("WindowsIsLock"); }
//            set { SysConfig.SetConfig("WindowsIsLock", value); }
//        }

//        public static string DesktopRefreshTimeSpan
//        {
//            get { return SysConfig.GetConfig("DesktopRefreshTimeSpan"); }
//            set { SysConfig.SetConfig("DesktopRefreshTimeSpan", value); }
//        }

//        public static string AudioSamplesPerSecond
//        {
//            get { return SysConfig.GetConfig("AudioSamplesPerSecond"); }
//            set { SysConfig.SetConfig("AudioSamplesPerSecond", value); }
//        }

//        public static string AudioBitsPerSample
//        {
//            get { return SysConfig.GetConfig("AudioBitsPerSample"); }
//            set { SysConfig.SetConfig("AudioBitsPerSample", value); }
//        }

//        public static string AudioChannels
//        {
//            get { return SysConfig.GetConfig("AudioChannels"); }
//            set { SysConfig.SetConfig("AudioChannels", value); }
//        }

//        public static string SessionMode
//        {
//            get { return SysConfig.GetConfig("SessionMode"); }
//            set { SysConfig.SetConfig("SessionMode", value); }
//        }
//        public static string AccessKey
//        {
//            get { return SysConfig.GetConfig("AccessKey"); }
//            set { SysConfig.SetConfig("AccessKey", value); }
//        }

//        public static string ServiceIPAddress
//        {
//            get { return SysConfig.GetConfig("ServiceIPAddress"); }
//            set { SysConfig.SetConfig("ServiceIPAddress", value); }
//        }

//        public static string ServicePort
//        {
//            get { return SysConfig.GetConfig("ServicePort"); }
//            set { SysConfig.SetConfig("ServicePort", value); }
//        }
//    }
//}