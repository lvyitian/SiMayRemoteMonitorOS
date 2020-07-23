using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class ActivateServicePack : EntitySerializerBase
    {
        public string CommandText { get; set; }
    }
    public class ActivateApplicationPack : EntitySerializerBase
    {

        /// <summary>
        /// 被控端id
        /// </summary>
        public string IdentifyId { get; set; }

        /// <summary>
        /// 应用Key
        /// </summary>
        public string ApplicationKey { get; set; }

        /// <summary>
        /// 主服务备注
        /// </summary>
        public string OriginName { get; set; }

        /// <summary>
        /// 创建时命令
        /// </summary>
        public string ActivatedCommandText { get; set; }
    }
}
