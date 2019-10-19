using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.HttpRemoteMonitorService.HttpModel
{
    public class DesktopViewModel
    {
        public AJaxMsgCommand Msg { get; set; }
        public string Id { get; set; }
        public string ImageBase64 { get; set; }
    }
}
