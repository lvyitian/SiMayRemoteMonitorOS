using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControlsCore
{
    /// <summary>
    /// 文件流接口
    /// </summary>
    public interface IFileStream
    {
        string FullFileName { get; }
        long Length { get; }
        long Position { get; set; }
        void Write(byte[] data, int offset, int length);
        int Read(byte[] data, int offset, int length);
        void Close();
    }
}