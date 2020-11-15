using SiMay.Core;
using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class RemoteUpdatePacket : EntitySerializerBase
    {
        /// <summary>
        /// Url更新或者文件更新
        /// </summary>
        public RemoteUpdateKind UrlOrFileUpdate { get; set; }

        public string DownloadUrl { get; set; }

        public byte[] FileData { get; set; }
    }
}
