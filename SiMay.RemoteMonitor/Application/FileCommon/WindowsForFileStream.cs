using SiMay.RemoteControlsCore.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteMonitor.Application.FileCommon
{
    public class WindowsForFileStream : IFileStream
    {
        private FileStream _fileStream;

        /// <summary>
        /// Windows文件流
        /// </summary>
        public WindowsForFileStream(FileStream fileStream)
            => this._fileStream = fileStream;
        public string FullFileName => this._fileStream.Name;

        public long Length => this._fileStream.Length;

        public long Position { get => this._fileStream.Position; set => this._fileStream.Position = value; }

        public void Close()
        {
            this._fileStream.Flush();
            this._fileStream.Close();
        }

        public int Read(byte[] data, int offset, int length)
        {
            return this._fileStream.Read(data, offset, length);
        }

        public void Write(byte[] data, int offset, int length)
        {
            this._fileStream.Write(data, offset, length);
        }
    }
}
