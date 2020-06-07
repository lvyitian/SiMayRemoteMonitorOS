using Microsoft.Win32;
using SiMay.Core;
using SiMay.Platform.Windows;
using SiMay.Platform.Windows.Helper;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SiMay.ServiceCore.MainService
{
    public class SystemHelper
    {
        /// <summary>
        /// 关机
        /// </summary>
        public const int SHUTDOWN = 0;

        /// <summary>
        /// 重启
        /// </summary>
        public const int REBOOT = 1;

        /// <summary>
        /// 自启动
        /// </summary>
        public const int REG_ACTION = 2;

        /// <summary>
        /// 取消自启动
        /// </summary>
        public const int REG_CANCEL_Action = 3;

        /// <summary>
        /// 隐藏文件
        /// </summary>
        public const int ATTRIB_HIDE = 4;

        /// <summary>
        /// 显示文件
        /// </summary>
        public const int ATTRIB_SHOW = 5;

        /// <summary>
        /// 卸载程序
        /// </summary>
        public const int UNSTALL = 6;

        /// <summary>
        /// 安装启动服务
        /// </summary>
        public const int INSTALL_SERVICE = 7;


        /// <summary>
        /// 卸载服务
        /// </summary>
        public const int UNINSTALL_SERVICE = 8;

        public static void SetSessionStatus(int status)
        {
            switch (status)
            {
                case SHUTDOWN:
                    SystemSessionHelper.Shutdown();
                    break;

                case REBOOT:
                    SystemSessionHelper.Reboot();
                    break;
                case REG_ACTION:
                    SystemSessionHelper.SetAutoRun(true);
                    break;

                case REG_CANCEL_Action:
                    SystemSessionHelper.SetAutoRun(false);
                    break;

                case ATTRIB_HIDE:
                    SystemSessionHelper.FileHide(true);
                    break;
                case ATTRIB_SHOW:
                    SystemSessionHelper.FileHide(false);
                    break;
                case UNSTALL:
                    RemoteService.Loader.UserTrunkContext.UserTrunkContextInstance?.InitiativeExit();
                    Thread.Sleep(100);//等待服务响应
                    SystemSessionHelper.UnInstall();
                    break;
                case INSTALL_SERVICE:
                    InstallAutoStartService();
                    break;
                case UNINSTALL_SERVICE:
                    UnInstallAutoStartService();
                    break;
            }
        }


        public static void InstallAutoStartService()
        {
            Platform.Windows.Helper.SystemMessageNotify.ShowTip("SiMay远程控制被控服务正在安装服务!");
            var svcFullName = Assembly.GetExecutingAssembly().Location;
            var parameter = " \"-serviceStart\"";//服务启动标志
            svcFullName += parameter;
            if (ServiceInstallerHelper.InstallService(svcFullName, AppConfiguartion.ServiceName, AppConfiguartion.ServiceDisplayName))
            {
                Platform.Windows.Helper.SystemMessageNotify.ShowTip("SiMay远程控制被控服务安装完成!");
                //服务安装完成启动成功
                Environment.Exit(0);
            }
            else
            {
                SystemMessageNotify.ShowTip("SiMay远程控制被控服务安装失败!");
                LogHelper.DebugWriteLog("Service Install Not Completed!!");
            }
        }

        public static void UnInstallAutoStartService()
        {
            SystemMessageNotify.ShowTip("SiMay远程控制被控服务正在卸载服务!");
            if (ServiceInstallerHelper.UnInstallService(AppConfiguartion.ServiceName))
                Environment.Exit(0);
            else
            {
                SystemMessageNotify.ShowTip("SiMay远程控制被控服务卸载失败!");
                LogHelper.DebugWriteLog("Service UnInstall Not Completed!!");
            }
        }
    }
}
