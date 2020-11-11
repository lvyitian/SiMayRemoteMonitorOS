using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Core.Standard.Packets
{
    public class CallSyncPacket : EntitySerializerBase
    {
        /// <summary>
        /// 同步Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 目标调用消息头
        /// </summary>
        public MessageHead TargetMessageHead { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Datas { get; set; }
    }

    public class CallSyncResultPacket : EntitySerializerBase
    {
        /// <summary>
        /// 同步Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 调用是否成功
        /// </summary>
        public bool IsOK { get; set; }

        /// <summary>
        /// 返回数据
        /// </summary>
        public byte[] Datas { get; set; }
    }
}
