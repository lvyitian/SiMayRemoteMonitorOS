using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteService.Entitys
{
    public class StartParameter
    {
        public string UniqueId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string RemarkInformation { get; set; }
        public string GroupName { get; set; }
        public bool IsAutoStart { get; set; }
        public bool IsHide { get; set; }
        public int SessionMode { get; set; }
        public long AccessKey { get; set; }
        public bool IsMutex { get; set; }
        public string ServiceVersion { get; set; }
        public string RunTimeText { get; set; }
    }
}
