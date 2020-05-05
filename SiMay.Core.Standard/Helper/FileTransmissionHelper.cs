using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Basic;

namespace SiMay.Core
{
    public class FileTransmissionHelper
    {
        private const int DefaultBufferSize = 1024 * 100;

        public int BufferSize
        {
            get
            {
                return _bufferSize;
            }
            set
            {
                if (_bufferSize == value)
                    return;
                _buffer = new byte[value];
                _bufferSize = value;
            }
        }

        private byte[] _buffer = new byte[DefaultBufferSize];
        private FileStream _fileStream;
        private int _bufferSize = DefaultBufferSize;
        public FileTransmissionHelper(string fileName, FileMode fileMode) => _fileStream = new FileStream(fileName, fileMode);

        public byte[] Read()
        {
            int lenght = _fileStream.Read(_buffer, 0, _buffer.Length);
            if (lenght < _bufferSize)
                return _buffer.Copy(0, lenght);
            else
                return _buffer;
        }

        public void Write(byte[] data) => _fileStream.Write(data, 0, data.Length);

        public void Close()
        {
            _fileStream.Flush();
            _fileStream.Close();
        }
    }
}
