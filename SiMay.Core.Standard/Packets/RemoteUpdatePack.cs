using SiMay.Core;
using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core
{
    public class RemoteUpdatePack : EntitySerializerBase
    {
        /// <summary>
        /// Url更新还是文件更新
        /// </summary>
        public RemoteUpdateType UrlOrFileUpdate { get; set; }

        public string DownloadUrl { get; set; }

        public byte[] FileDate { get; set; }
    }
}
