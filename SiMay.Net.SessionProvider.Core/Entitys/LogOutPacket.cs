using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Net.SessionProvider.Core
{
    public class LogOutPacket : EntitySerializerBase
    {
        /// <summary>
        /// 登出原因
        /// </summary>
        public string Message { get; set; }
    }
}
