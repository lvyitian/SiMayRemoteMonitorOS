using SiMay.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteMonitor
{
    class KeygenHelper
    {
        private static string GetDiskVolumeSerialNumber()
        {
            ManagementObject disk = new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
            disk.Get();
            return disk.GetPropertyValue("VolumeSerialNumber").ToString();
        }

        private static string GetCpuSerialNumber()
        {
            string strCpu = null;
            ManagementClass myCpu = new ManagementClass("win32_Processor");
            ManagementObjectCollection myCpuCollection = myCpu.GetInstances();
            foreach (ManagementObject myObject in myCpuCollection)
            {
                strCpu = myObject.Properties["Processorid"].Value.ToString();
            }
            return strCpu;
        }

        public static string GetRegSerializeKey() 
            => GetDiskVolumeSerialNumber() + GetCpuSerialNumber() + Environment.MachineName;
    }
}
