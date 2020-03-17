using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteService.Loader
{
    public class RegValueHelper
    {

        public static bool SetConfig(string node, string value)
        {
            bool result = true;
            try
            {
                RegistryKey key = Registry.CurrentUser;
                RegistryKey software = key.CreateSubKey("SYSTEM\\SoftWare\\SiMayService");
                software.SetValue(node, value);
            }
            catch
            {
                result = false;
            }

            return result;
        }

        public static string GetConfig(string node)
        {
            string value = null;

            try
            {
                RegistryKey key = Registry.CurrentUser;
                RegistryKey software = key.OpenSubKey("SYSTEM\\SoftWare\\SiMayService");
                value = software.GetValue(node).ToString();
            }
            catch (Exception e)
            {

            }

            return value;
        }
    }
}
