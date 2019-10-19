using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProviderService.Packet
{
    public class AckPacket
    {
        /// <summary>
        /// 命令头
        /// </summary>
        public short MsgCommand { get; set; }
        /// <summary>
        /// 用于连接后，业务层验证此键是否允许访问
        /// </summary>
        public long AccessKey { get; set; }
        /// <summary>
        /// 代理协议，判断连接类型
        /// </summary>
        public byte ServiceType { get; set; }
    }
}
