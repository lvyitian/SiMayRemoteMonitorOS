using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class AcknowledPacket : EntitySerializerBase
    {
        /// <summary>
        /// 连接类型
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// 访问Id，一般是主控端Id或者服务工作连接用于对接应用服务连接的主控端Id
        /// </summary>
        public long AccessId { get; set; }

        /// <summary>
        /// 连接KEY
        /// </summary>
        public long AccessKey { get; set; }

        /// <summary>
        /// 是否载入核心库程序
        /// </summary>
        public bool AssemblyLoad { get; set; }
    }
}
