using SiMay.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Core.Packets
{
    public class RemoteUpdatePack
    {
        /// <summary>
        /// Url更新还是文件更新
        /// </summary>
        public RemoteUpdateType UrlOrFileUpdate { get; set; }

        public string DownloadUrl { get; set; }

        public byte[] FileDate { get; set; }
    }
}
