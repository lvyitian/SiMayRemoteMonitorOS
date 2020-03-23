using Microsoft.Win32;
using SiMay.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SiMay.ServiceCore
{
    public class SystemSessionHelper
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
                    Process.Start("cmd.exe", "/c shutdown -s -t 0");
                    break;

                case REBOOT:
                    Process.Start("cmd.exe", "/c shutdown -r -t 0");
                    break;
                case REG_ACTION:
                    SetAutoRun(true);
                    break;

                case REG_CANCEL_Action:
                    SetAutoRun(false);
                    break;

                case ATTRIB_HIDE:
                    SetExecutingFileHide(true);
                    break;
                case ATTRIB_SHOW:
                    SetExecutingFileHide(false);
                    break;
                case UNSTALL:
                    UserTrunkContext.UserTrunkContextInstance?.InitiativeExit();
                    Thread.Sleep(100);//等待服务响应
                    UnInstall();
                    break;
                case INSTALL_SERVICE:
                    InstallAutoStartService();
                    break;
                case UNINSTALL_SERVICE:
                    UnInstallAutoStartService();
                    break;
            }
        }

        //设置自启动
        public static void SetAutoRun(bool isRun)
        {
            try
            {
                RegistryKey keys;

                //win8~10启动键位于currenUser内
                if ((Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 2)
                    || (Environment.OSVersion.Version.Major == 10 && Environment.OSVersion.Version.Minor == 0))
                {
                    keys = Registry.CurrentUser;
                }
                else
                    keys = Registry.LocalMachine;

                if (isRun)
                {
                    RegistryKey key = keys.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVerSion\\Run", true);
                    if (key != null) key.SetValue("SiMayServiceEx", Application.ExecutablePath);
                }
                else
                {
                    RegistryKey key = keys.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVerSion\\Run", true);
                    if (key != null) key.DeleteValue("SiMayServiceEx");
                }
            }
            catch
            {

            }
        }

        public static void InstallAutoStartService()
        {
            SystemMessageNotify.ShowTip("SiMay远程控制被控服务正在安装服务!");
            var svcFullName = Assembly.GetExecutingAssembly().Location;
            var parameter = " \"-serviceStart\"";//服务启动标志
            svcFullName += parameter;
            if (ServiceInstallerHelper.InstallService(svcFullName, AppConfiguartion.ServiceName, AppConfiguartion.ServiceDisplayName))
            {
                SystemMessageNotify.ShowTip("SiMay远程控制被控服务安装完成!");
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

        public static void UnInstall()
        {
            Environment.Exit(0);
        }

        public static void SetExecutingFileHide(bool isHide)
        {
            try
            {
                if (isHide)
                    File.SetAttributes(Application.ExecutablePath,
                    FileAttributes.Hidden | FileAttributes.System);
                else
                    File.SetAttributes(Application.ExecutablePath, ~FileAttributes.Hidden);
            }
            catch { }
        }
    }
}
