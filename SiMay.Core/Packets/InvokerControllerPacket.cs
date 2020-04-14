using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Core
{
    public class InvokerControllerPacket
    {
        /// <summary>
        /// 控制程序路由
        /// </summary>
        public string ControllerRoute { get; set; }

        /// <summary>
        /// 实体类型限定全名
        /// </summary>
        public string DataPacketTypeFullName { get; set; }

        /// <summary>
        /// 实体数据
        /// </summary>
        public byte[] PacketData { get; set; }
    }

    public class InvokerResponsePacket
    {
        /// <summary>
        /// 控制程序路由
        /// </summary>
        public string ControllerRoute { get; set; }


        /// <summary>
        /// 实体数据
        /// </summary>
        public byte[] PacketData { get; set; }
    }
}
