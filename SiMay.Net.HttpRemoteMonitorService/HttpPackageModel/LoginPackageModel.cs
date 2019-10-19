using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.HttpRemoteMonitorService.HttpModel
{
    public class LoginPackageModel
    {
        public AJaxMsgCommand Msg { get; set; }
        public string Id { get; set; }
        public string OS { get; set; }
        public string MachineName { get; set; }
        public string Des { get; set; }
        public string DesktopViewOpen { get; set; }
    }
}
