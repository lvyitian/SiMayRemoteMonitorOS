using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace SiMay.RemoteService.Loader
{
    public class CommonHelper
    {
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
            catch (Exception e)
            {

            }
        }

        public static string GetHostByName(string host)
        {
            string _return = null;
            try
            {
                IPHostEntry hostinfo = Dns.GetHostByName(host);
                IPAddress[] aryIP = hostinfo.AddressList;
                _return = aryIP[0].ToString();
            }
            catch { }

            return _return;
        }

        public static void WriteText(Exception ex, string path)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DateTime:" + DateTime.Now.ToString());
                sb.AppendLine("Exception Message:" + ex.Message);
                sb.AppendLine("StackTrace:" + ex.StackTrace);
                StreamWriter fs = new StreamWriter(path, true);
                fs.WriteLine(sb.ToString());
                fs.Close();
            }
            catch { }

        }
    }
}
