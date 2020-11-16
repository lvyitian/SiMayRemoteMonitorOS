using SiMay.Basic;
using SiMay.RemoteControls.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteMonitor
{
    public class SystemAppConfig : DefaultConfigBase
    {
        private string _filePath = Path.Combine(Environment.CurrentDirectory, "SiMayConfig.ini");

        protected override string Read(string key)
            => IniConfigHelper.GetValue("SiMayConfig", key, string.Empty, _filePath);

        protected override void Save(string key, string value)
            => IniConfigHelper.SetValue("SiMayConfig", key, value, _filePath);

        //public bool DesktopViewRecordAny(string machineName)
        //{
        //    GetConfig();
        //}

    }
}
