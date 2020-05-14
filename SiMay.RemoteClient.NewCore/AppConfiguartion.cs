using SiMay.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SiMay.ServiceCore
{
    public class ServiceAppConfig : AbstractAppConfigBase
    {
        public override string GetConfig(string key)
        {
            if (!AppConfig.ContainsKey(key))
                AppConfig[key] = AppConfigRegValueHelper.GetValue(key);

            string val;
            if (AppConfig.TryGetValue(key, out val))
                return val;
            else
                return null;
        }

        public override bool SetConfig(string key, string value)
        {
            bool successed = AppConfigRegValueHelper.SetValue(key, value);
            if (successed)
                AppConfig[key] = value;

            return successed;
        }
    }
    /// <summary>
    /// 系统全局配置
    /// </summary>
    public class AppConfiguartion
    {

        public static AbstractAppConfigBase SysConfigs { get; } = new ServiceAppConfig();

        public static bool IsAutoRun { get; set; }
        public static bool IsHideExcutingFile { get; set; }
        public static string DefaultRemarkInfo { get; set; }
        public static string DefaultGroupName { get; set; }
        public static string Version { get; set; }
        public static long AccessKey { get; set; }
        public static string RunTime { get; set; }
        /// <summary>
        /// 连接的服务端是否中间服务器
        /// </summary>
        public static bool CenterServiceMode { get; set; } = false;
        public static string IdentifyId { get; set; }

        public static IPEndPoint ServerIPEndPoint { get; set; }
        public static string HostAddress { get; set; }
        public static int HostPort { get; set; }

        public static string ServiceName { get; set; }

        public static string ServiceDisplayName { get; set; }

        public static string GroupName
        {
            get
            {
                var groupName = SysConfigs.GetConfig("GroupName");
                return groupName == "" ? null : groupName;
            }
            set
            {
                SysConfigs.SetConfig("GroupName", value);
            }
        }

        public static string RemarkInfomation
        {
            get
            {
                var remark = SysConfigs.GetConfig("RemarkInfomation");
                return remark;
            }
            set
            {
                SysConfigs.SetConfig("RemarkInfomation", value);
            }
        }

        public static bool IsOpenScreenView
        {
            get
            {
                try
                {
                    return bool.Parse(SysConfigs.GetConfig("isOpenScreenView") ?? "false");
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                SysConfigs.SetConfig("isOpenScreenView", value.ToString());
            }
        }

        public static bool IsScreenRecord
        {
            get
            {
                try
                {
                    return bool.Parse(SysConfigs.GetConfig("IsScreenRecord") ?? "false");
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                SysConfigs.SetConfig("IsScreenRecord", value.ToString());
            }
        }

        public static int ScreenRecordHeight
        {
            get
            {
                try
                {
                    return int.Parse(SysConfigs.GetConfig("ScreenRecordHeight") ?? "0");
                }
                catch
                {
                    return 800;
                }
            }
            set
            {
                SysConfigs.SetConfig("ScreenRecordHeight", value.ToString());
            }
        }
        public static int ScreenRecordWidth
        {
            get
            {
                try
                {
                    return int.Parse(SysConfigs.GetConfig("ScreenRecordWidth") ?? "0");
                }
                catch
                {
                    return 1200;
                }
            }
            set
            {
                SysConfigs.SetConfig("ScreenRecordWidth", value.ToString());
            }
        }
        public static int ScreenRecordSpanTime
        {
            get
            {
                try
                {
                    return int.Parse(SysConfigs.GetConfig("ScreenRecordSpanTime") ?? "0");
                }
                catch
                {
                    return 3000;
                }
            }
            set
            {
                SysConfigs.SetConfig("ScreenRecordSpanTime", value.ToString());
            }
        }


        public static bool KeyboardOffline
        {
            get
            {
                try
                {
                    return bool.Parse(SysConfigs.GetConfig("Offlinekeyboard") ?? "false");
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                SysConfigs.SetConfig("Offlinekeyboard", value.ToString());
            }
        }

        public static bool HasSystemAuthority
        {
            get
            {

                try
                {
                    return bool.Parse(SysConfigs.GetConfig("HasSystemAuthority"));
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                SysConfigs.SetConfig("HasSystemAuthority", value.ToString());
            }
        }
    }
}
