using AForge.Video.DirectShow;
using Microsoft.Win32;
using SiMay.Core;
using SiMay.Platform.Windows;
using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace SiMay.ServiceCore.Helper
{

    public class SystemInfoHelper
    {

        public static string GetGpuName()
        {
            try
            {
                string gpuName = string.Empty;
                string query = "SELECT * FROM Win32_DisplayConfiguration";

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject mObject in searcher.Get())
                    {
                        gpuName += mObject["Description"].ToString() + "; ";
                    }
                }

                return (!string.IsNullOrEmpty(gpuName)) ? gpuName : "N/A";
            }
            catch
            {
                return "Unknown";
            }
        }

        public static string GetAntivirus()
        {
            try
            {
                string antivirusName = string.Empty;
                // starting with Windows Vista we must use the root\SecurityCenter2 namespace
                string scope = (PlatformHelper.VistaOrHigher) ? "root\\SecurityCenter2" : "root\\SecurityCenter";
                string query = "SELECT * FROM AntivirusProduct";

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
                {
                    foreach (ManagementObject mObject in searcher.Get())
                    {
                        antivirusName += mObject["displayName"].ToString() + "; ";
                    }
                }

                return (!string.IsNullOrEmpty(antivirusName)) ? antivirusName : "N/A";
            }
            catch
            {
                return "Unknown";
            }
        }
        public static bool ExistRecordDevice()
        {
            return SiMay.Platform.Windows.Win32.waveInGetNumDevs() > 0 ? true : false;
        }

        public static bool ExistPlayDevice()
        {
            return SiMay.Platform.Windows.Win32.waveOutGetNumDevs() > 0 ? true : false;
        }

        //public static string GetLocalIPV4()
        //{
        //    try
        //    {
        //        string HostName = Dns.GetHostName();
        //        IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
        //        for (int i = 0; i < IpEntry.AddressList.Length; i++)
        //        {
        //            if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
        //                return IpEntry.AddressList[i].ToString();
        //        }
        //        return string.Empty;
        //    }
        //    catch
        //    {
        //        return string.Empty;
        //    }
        //}

        public static bool ExistCameraDevice()
        {
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count <= 0)
                return false;

            return true;
        }

        //获取CPU信息
        public static string GetMyCpuInfo
        {
            get
            {
                try
                {
                    RegistryKey reg = Registry.LocalMachine;
                    reg = reg.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0");
                    return reg.GetValue("ProcessorNameString").ToString();
                }
                catch (Exception ex)
                {
                    LogHelper.WriteErrorByCurrentMethod(ex);
                    return string.Empty;
                }
            }
        }

        //获取驱动器信息
        public static string GetMyDriveInfo
        {
            get
            {
                string[] MyDrive = Environment.GetLogicalDrives();
                long s0 = 0, s1 = 0;
                foreach (string MyDriveLetter in MyDrive)
                {
                    try
                    {
                        DriveInfo MyDriveInfo = new DriveInfo(MyDriveLetter);
                        if (MyDriveInfo.DriveType == DriveType.CDRom || MyDriveInfo.DriveType == DriveType.Removable)
                            continue;
                        s0 += MyDriveInfo.TotalSize;
                        s1 += MyDriveInfo.TotalFreeSpace;
                    }
                    catch { }
                }
                return "总空间:" + (s0 / 1073741824).ToString() + "GB 可用空间:" + (s1 / 1073741824).ToString() + "GB";
            }
        }

        //获取内存信息
        public static long GetMyMemorySize
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

        //获取操作系统版本
        public static string GetOSFullName
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
                catch (Exception ex)
                {
                    LogHelper.WriteErrorByCurrentMethod(ex);
                    return fullName;
                }
            }
        }

        //private const string Windows2000 = "5.0";
        //private const string WindowsXP = "5.1";
        //private const string Windows2003 = "5.2";
        //private const string Windows2008 = "6.0";
        //private const string Windows7 = "6.1";
        //private const string Windows8OrWindows81 = "6.2";
        //private const string Windows10 = "10.0";

        ////获取操作系统版本
        //public static string GsystemEdition
        //{
        //    get
        //    {
        //        string strClient = "";
        //        switch (System.Environment.OSVersion.Version.Major.ToString() + "." + System.Environment.OSVersion.Version.Minor.ToString())
        //        {
        //            case Windows2000:
        //                strClient = "Windows 2000";
        //                break;

        //            case WindowsXP:
        //                strClient = "Windows XP";
        //                break;

        //            case Windows2003:
        //                strClient = "Windows 2003";
        //                break;

        //            case Windows2008:
        //                strClient = "Windows 2008";
        //                break;

        //            case Windows7:
        //                strClient = "Windows 7";
        //                break;

        //            case Windows8OrWindows81:
        //                strClient = "Windows8OrWindows8.1";
        //                break;

        //            case Windows10:
        //                strClient = "Windows 10";
        //                break;

        //            default:
        //                strClient = "Other System";
        //                break;
        //        }
        //        return strClient;
        //    }
        //}

        public static string GetLocalIPV4()
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

        //public static string GetLocalIPV4()
        //{
        //    foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        //    {
        //        GatewayIPAddressInformation gatewayAddress = ni.GetIPProperties().GatewayAddresses.FirstOrDefault();
        //        if (gatewayAddress != null) //exclude virtual physical nic with no default gateway
        //        {
        //            if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
        //                ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
        //                ni.OperationalStatus == OperationalStatus.Up)
        //            {
        //                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
        //                {
        //                    if (ip.Address.AddressFamily != AddressFamily.InterNetwork ||
        //                        ip.AddressPreferredLifetime == UInt32.MaxValue) // exclude virtual network addresses
        //                        continue;

        //                    return ip.Address.ToString();
        //                }
        //            }
        //        }
        //    }

        //    return "-";
        //}

        //获取硬盘序列号
        public static string BIOSSerialNumber
        {
            get
            {
                try
                {
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_BIOS");
                    string sBIOSSerialNumber = "";
                    foreach (ManagementObject mo in searcher.Get())
                    {
                        sBIOSSerialNumber = mo["SerialNumber"].ToString().Trim();
                    }
                    return sBIOSSerialNumber;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteErrorByCurrentMethod(ex);
                    return string.Empty;
                }
            }
        }

        //获取网卡MAC
        public static string GetMacAddress
        {
            get
            {
                try
                {
                    string mac = "";
                    ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");
                    ManagementObjectCollection queryCollection = query.Get();
                    foreach (ManagementObject mo in queryCollection)
                    {
                        if (mo["IPEnabled"].ToString() == "True")
                            mac = mo["MacAddress"].ToString();
                    }
                    return mac;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteErrorByCurrentMethod(ex);
                    return string.Empty;
                }
            }
        }
    }
}