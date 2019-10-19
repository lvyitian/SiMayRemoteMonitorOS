using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace SiMay.RemoteService
{
    public class SystemInforUtil
    {
        public static string LocalIPV4
        {
            get
            {
                try
                {
                    string HostName = Dns.GetHostName();
                    IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                    for (int i = 0; i < IpEntry.AddressList.Length; i++)
                    {
                        if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                            return IpEntry.AddressList[i].ToString();
                    }
                    return string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public static string LocalCpuInfo
        {
            get
            {
                try
                {
                    RegistryKey reg = Registry.LocalMachine;
                    reg = reg.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0");
                    return reg.GetValue("ProcessorNameString").ToString();
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public static string OSFullName
        {
            get
            {
                string fullName = "Unknown OS";

                try
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
                    {
                        foreach (ManagementObject os in searcher.Get())
                        {
                            fullName = os["Caption"].ToString();
                            break;
                        }
                    }

                    fullName = Regex.Replace(fullName, "^.*(?=Windows)", "").TrimEnd().TrimStart(); // Remove everything before first match "Windows" and trim end & start
                    var is64Bit = Environment.Is64BitOperatingSystem;
                    return $"{fullName} {(is64Bit ? 64 : 32)} Bit";
                }
                catch
                {
                    return fullName;
                }
            }
        }

        public static long LocalMemorySize
        {
            get
            {
                try
                {
                    Microsoft.VisualBasic.Devices.Computer My = new Microsoft.VisualBasic.Devices.Computer();
                    return (long)My.Info.TotalPhysicalMemory;
                }
                catch
                {
                    return 0;
                }
            }
        }
    }
}
