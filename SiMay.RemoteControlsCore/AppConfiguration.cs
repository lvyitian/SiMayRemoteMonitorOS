using SiMay.Basic;
using SiMay.Core;
using SiMay.Core.AppConfig;
using System.Collections.Generic;
using System;
using System.IO;

namespace SiMay.RemoteControlsCore
{

    public class ManagerAppConfig : AbstractAppConfigBase
    {
        Dictionary<string, string> _defaultConfig = new Dictionary<string, string>()
        {
            { "IPAddress" ,"0.0.0.0" },
            { "Port" , "5200" },
            { "ConnectPassWord", "5200" },
            { "MaxConnectCount", "100000" },
            { "Maximize", "false" },
            { "lHosts", "127.0.0.1:5200" },
            { "DbClickViewExc", "" },
            { "LockPassWord", "5200" },
            { "WindowsIsLock", "false" },
            { "DesktopRefreshInterval", "1500" },
            { "AudioSamplesPerSecond", "8000" },
            { "AudioBitsPerSample", "16" },
            { "AudioChannels", "1" },
            { "SessionMode", "0" },
            { "ServiceIPAddress", "127.0.0.1" },
            { "ServicePort", "522" },
            { "EnabledCarousel", "true" },
            { "CarouselInterval", "5000" },
            { "ViewColumn", "4" },
            { "ViewRow", "3" },
            { "EnabledAnonyMous", "true" },
            { "AccessId" , "123456789" },
            { "MainAppAccessKey", "5200" },
            { "AccessKey", "5200" }
        };

        string _filePath = Path.Combine(Environment.CurrentDirectory, "SiMayConfig.ini");

        public override string GetConfig(string key)
        {
            if (!AppConfig.ContainsKey(key) && _defaultConfig.ContainsKey(key))
                AppConfig[key] = IniConfigHelper.GetValue("SiMayConfig", key, _defaultConfig[key], _filePath);

            string val;
            if (AppConfig.TryGetValue(key, out val))
                return val;
            else
                return null;
        }

        public override bool SetConfig(string key, string value)
        {
            AppConfig[key] = value;
            IniConfigHelper.SetValue("SiMayConfig", key, value, _filePath);
            return true;
        }
    }
    public class AppConfiguration
    {

        public static AbstractAppConfigBase SysConfig { get; } = new ManagerAppConfig();

        public static string IPAddress
        {
            get { return SysConfig.GetConfig("IPAddress"); }
            set { SysConfig.SetConfig("IPAddress", value); }
        }

        public static int Port
        {
            get { return int.Parse(SysConfig.GetConfig("Port")); }
            set { SysConfig.SetConfig("Port", value.ToString()); }
        }

        public static string ConnectPassWord
        {
            get { return SysConfig.GetConfig("ConnectPassWord"); }
            set { SysConfig.SetConfig("ConnectPassWord", value); }
        }

        public static int MaxConnectCount
        {
            get { return int.Parse(SysConfig.GetConfig("MaxConnectCount")); }
            set { SysConfig.SetConfig("MaxConnectCount", value.ToString()); }
        }

        public static bool WindowMaximize
        {
            get { return bool.Parse(SysConfig.GetConfig("Maximize")); }
            set { SysConfig.SetConfig("Maximize", value.ToString()); }
        }
        public static string LHostString
        {
            get { return SysConfig.GetConfig("lHosts"); }
            set { SysConfig.SetConfig("lHosts", value); }
        }

        public static string DbClickViewExc
        {
            get { return SysConfig.GetConfig("DbClickViewExc"); }
            set { SysConfig.SetConfig("DbClickViewExc", value); }
        }

        public static string LockPassWord
        {
            get { return SysConfig.GetConfig("LockPassWord"); }
            set { SysConfig.SetConfig("LockPassWord", value); }
        }

        public static bool WindowsIsLock
        {
            get { return bool.Parse(SysConfig.GetConfig("WindowsIsLock")); }
            set { SysConfig.SetConfig("WindowsIsLock", value.ToString()); }
        }

        public static int DesktopRefreshInterval
        {
            get { return int.Parse(SysConfig.GetConfig("DesktopRefreshInterval")); }
            set { SysConfig.SetConfig("DesktopRefreshInterval", value.ToString()); }
        }

        public static int AudioSamplesPerSecond
        {
            get { return int.Parse(SysConfig.GetConfig("AudioSamplesPerSecond")); }
            set { SysConfig.SetConfig("AudioSamplesPerSecond", value.ToString()); }
        }

        public static int AudioBitsPerSample
        {
            get { return int.Parse(SysConfig.GetConfig("AudioBitsPerSample")); }
            set { SysConfig.SetConfig("AudioBitsPerSample", value.ToString()); }
        }

        public static int AudioChannels
        {
            get { return int.Parse(SysConfig.GetConfig("AudioChannels")); }
            set { SysConfig.SetConfig("AudioChannels", value.ToString()); }
        }

        public static string SessionMode
        {
            get { return SysConfig.GetConfig("SessionMode"); }
            set { SysConfig.SetConfig("SessionMode", value); }
        }
        public static string AccessKey
        {
            get { return SysConfig.GetConfig("AccessKey"); }
            set { SysConfig.SetConfig("AccessKey", value); }
        }

        public static string ServiceIPAddress
        {
            get { return SysConfig.GetConfig("ServiceIPAddress"); }
            set { SysConfig.SetConfig("ServiceIPAddress", value); }
        }

        public static int ServicePort
        {
            get { return int.Parse(SysConfig.GetConfig("ServicePort")); }
            set { SysConfig.SetConfig("ServicePort", value.ToString()); }
        }

        public static bool CarouselEnabled
        {
            get { return bool.Parse(SysConfig.GetConfig("EnabledCarousel")); }
            set { SysConfig.SetConfig("EnabledCarousel", value.ToString()); }
        }

        public static int CarouselInterval
        {
            get { return int.Parse(SysConfig.GetConfig("CarouselInterval")); }
            set { SysConfig.SetConfig("CarouselInterval", value.ToString()); }
        }

        public static int ViewColumn
        {
            get { return int.Parse(SysConfig.GetConfig("ViewColumn")); }
            set { SysConfig.SetConfig("ViewColumn", value.ToString()); }
        }

        public static int ViewRow
        {
            get { return int.Parse(SysConfig.GetConfig("ViewRow")); }
            set { SysConfig.SetConfig("ViewRow", value.ToString()); }
        }

        /// <summary>
        /// 临时Id
        /// </summary>
        public static long UseAccessId { get; set; }

        public static long AccessId
        {
            get { return long.Parse(SysConfig.GetConfig("AccessId")); }
            set { SysConfig.SetConfig("AccessId", value.ToString()); }
        }

        public static long MainAppAccessKey
        {
            get { return long.Parse(SysConfig.GetConfig("MainAppAccessKey")); }
            set { SysConfig.SetConfig("MainAppAccessKey", value.ToString()); }
        }

        public static bool EnabledAnonyMous
        {
            get { return bool.Parse(SysConfig.GetConfig("EnabledAnonyMous")); }
            set { SysConfig.SetConfig("EnabledAnonyMous", value.ToString()); }
        }
    }
}