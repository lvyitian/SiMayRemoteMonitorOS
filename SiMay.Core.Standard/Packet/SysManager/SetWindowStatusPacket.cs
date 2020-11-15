using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class SetWindowStatusPacket : EntitySerializerBase
    {
        /// <summary>
        /// 窗体状态
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 窗口句柄
        /// </summary>
        public int[] Handlers { get; set; }
    }
}
