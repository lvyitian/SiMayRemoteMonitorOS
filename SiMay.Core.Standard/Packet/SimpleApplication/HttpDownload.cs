using SiMay.ReflectCache;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Core
{
    public class JoinHttpDownloadPacket : EntitySerializerBase
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 下载url
        /// </summary>
        public string Url { get; set; }
    }

    public class HttpDownloadStatusList : EntitySerializerBase
    {
        public HttpDownloadTaskItemContext[] httpDownloadTaskItemContexts { get; set; }
    }

    public class HttpDownloadTaskItemContext : EntitySerializerBase
    {
        public int Id { get; set; } = Guid.NewGuid().GetHashCode();

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }


        public long TotalBytesToReceive { get; set; }

        public long BytesReceived { get; set; }

        /// <summary>
        /// 0=正常
        /// 1=结束下载
        /// </summary>
        public int Status { get; set; } = 0;
    }
}
