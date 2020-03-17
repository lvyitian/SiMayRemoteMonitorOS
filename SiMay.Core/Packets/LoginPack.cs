using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class LoginPack : EntitySerializerBase
    {
        public string IPV4 { get; set; }
        public string MachineName { get; set; }
        public string Remark { get; set; }
        public string GroupName { get; set; }
        public int ProcessorCount { get; set; }
        public string ProcessorInfo { get; set; }
        public long MemorySize { get; set; }
        public string StartRunTime { get; set; }
        public string ServiceVison { get; set; }
        public string UserName { get; set; }
        public string OSVersion { get; set; }
        public bool OpenScreenWall { get; set; }
        public bool ExistCameraDevice { get; set; }
        public bool ExitsRecordDevice { get; set; }
        public bool ExitsPlayerDevice { get; set; }
        public string IdentifyId { get; set; }
        public bool OpenScreenRecord { get; set; }
        public int RecordHeight { get; set; }
        public int RecordWidth { get; set; }
        public int RecordSpanTime { get; set; }

        public bool HasLoadServiceCOM { get; set; }
    }
}
