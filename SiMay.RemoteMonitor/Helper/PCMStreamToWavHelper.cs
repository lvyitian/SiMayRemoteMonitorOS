using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteMonitor
{
    public class PCMStreamToWavHelper
    {
        private BinaryWriter _binaryWriter;
        private FileStream _fileStream;
        private int _samplesPerSecond;
        private short _bitsPerSample;
        private short _channels;
        private int _dataBufferSize;
        private bool _disposed;
        /// <summary>
        /// 累计写入长度
        /// </summary>
        private int _writeLength = 0;

        public string FileName { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">输出文件名</param>
        /// <param name="samplesPerSecond">采样频率</param>
        /// <param name="bitsPerSample">每个采样需要的bit数</param>
        /// <param name="channels">声道数量</param>
        public PCMStreamToWavHelper(string fileName, int samplesPerSecond, short bitsPerSample, short channels, int dataBufferSize)
        {
            _samplesPerSecond = samplesPerSecond;
            _bitsPerSample = bitsPerSample;
            _channels = channels;
            _dataBufferSize = dataBufferSize;
            FileName = fileName;
            this.CreateSoundFile(fileName);
        }

        public void WritePCMDataChunk(byte[] data)
        {
            if (_disposed)
                return;

            _writeLength += data.Length;
            _binaryWriter.Write(data, 0, data.Length);
        }

        private void CreateSoundFile(string path)
        {
            _fileStream = new FileStream(path, FileMode.Create);

            _binaryWriter = new BinaryWriter(_fileStream);

            //Set up file with RIFF chunk info. 每个WAVE文件的头四个字节便是“RIFF”。  
            char[] ChunkRiff = { 'R', 'I', 'F', 'F' };
            char[] ChunkType = { 'W', 'A', 'V', 'E' };
            char[] ChunkFmt = { 'f', 'm', 't', ' ' };
            char[] ChunkData = { 'd', 'a', 't', 'a' };

            short shPad = 1;                // File padding  

            int nFormatChunkLength = 0x10; // Format chunk length.  

            int nLength = 0;                // File length, minus first 8 bytes of RIFF description. This will be filled in later.  

            // 一个样本点的字节数目  
            short shBytesPerSample = 2;

            // RIFF 块  
            _binaryWriter.Write(ChunkRiff);
            _binaryWriter.Write(nLength);
            _binaryWriter.Write(ChunkType);

            // WAVE块  
            _binaryWriter.Write(ChunkFmt);
            _binaryWriter.Write(nFormatChunkLength);
            _binaryWriter.Write(shPad);


            _binaryWriter.Write(_channels); // Mono,声道数目，1-- 单声道；2-- 双声道  
            _binaryWriter.Write(_samplesPerSecond);// 16KHz 采样频率                     
            _binaryWriter.Write(_dataBufferSize); //每秒所需字节数  
            _binaryWriter.Write(shBytesPerSample);//数据块对齐单位(每个采样需要的字节数)  
            _binaryWriter.Write(_bitsPerSample);  // 16Bit,每个采样需要的bit数    

            // 数据块  
            _binaryWriter.Write(ChunkData);
            _binaryWriter.Write((int)0);   // The sample length will be written in later.  
        }

        public void Close()
        {
            _disposed = true;
            _binaryWriter.Seek(4, SeekOrigin.Begin);
            _binaryWriter.Write(_writeLength + 36);   // 写文件长度  
            _binaryWriter.Seek(40, SeekOrigin.Begin);
            _binaryWriter.Write(_writeLength);
            _fileStream.Close();
        }
    }
}
