using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SiMay.Basic
{

    public class IniConfigHelper
    {

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);

        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key,
            string def, StringBuilder retVal, int size, string filePath);

        public static string GetValue(string node, string key, string keyValue, string filePath)
        {
            StringBuilder temp = new StringBuilder(1024);

            GetPrivateProfileString(node, key, keyValue, temp, 1024, filePath);

            return (Convert.ToString(temp));
        }

        public static void SetValue(string node, string key, string keyValue, string filePath)
        {
            long OpStation = WritePrivateProfileString(node, key, keyValue, filePath);
        }
    }
}