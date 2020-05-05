using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Platform.Windows
{
    unsafe public class WinSoundPlayer
    {
        private string WaveOutDeviceName;
        private int SamplesPerSecond = 8000;
        private int BitsPerSample = 16;
        private int Channels = 1;
        private int BufferSize = 1024;
        private int BufferCount = 8;


        private IntPtr hWaveOut;
        private Win32.WAVEHDR*[] WaveOutHeaders;
        private System.Threading.AutoResetEvent AutoResetEventDataPlayed = new System.Threading.AutoResetEvent(false);
        private Win32.DelegateWaveOutProc delegateWaveOutProc;


        public WinSoundPlayer()
        {
            delegateWaveOutProc = new Win32.DelegateWaveOutProc(waveOutProc);
        }

        public bool Open(string deviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferSize, int bufferCount)
        {
            this.WaveOutDeviceName = deviceName;
            this.SamplesPerSecond = samplesPerSecond;
            this.BitsPerSample = bitsPerSample;
            this.Channels = channels;
            this.BufferSize = bufferSize;
            this.BufferCount = bufferCount;

            if (OpenWaveOut())
            {
                if (CreateWaveOutHeaders())
                    return true;
            }

            return false;
        }

        private bool OpenWaveOut()
        {
            Win32.WAVEFORMATEX waveFormatEx = new Win32.WAVEFORMATEX();
            waveFormatEx.wFormatTag = (ushort)Win32.WaveFormatFlags.WAVE_FORMAT_PCM;
            waveFormatEx.nChannels = (ushort)Channels;
            waveFormatEx.nSamplesPerSec = (ushort)SamplesPerSecond;
            waveFormatEx.wBitsPerSample = (ushort)BitsPerSample;
            waveFormatEx.nBlockAlign = (ushort)((waveFormatEx.wBitsPerSample * waveFormatEx.nChannels) >> 3);
            waveFormatEx.nAvgBytesPerSec = (uint)(waveFormatEx.nBlockAlign * waveFormatEx.nSamplesPerSec);

            int deviceId = WinSound.GetWaveOutDeviceIdByName(WaveOutDeviceName);

            Win32.MMRESULT hr = Win32.waveOutOpen(ref hWaveOut, deviceId, ref waveFormatEx, delegateWaveOutProc, 0, (int)Win32.WaveProcFlags.CALLBACK_FUNCTION);

            if (hr != Win32.MMRESULT.MMSYSERR_NOERROR)
                return false;

            GCHandle.Alloc(hWaveOut, GCHandleType.Pinned);

            return true;
        }


        /// <summary>
        /// CreateWaveOutHeaders
        /// </summary>
        /// <returns></returns>
        private bool CreateWaveOutHeaders()
        {
            WaveOutHeaders = new Win32.WAVEHDR*[BufferCount];
            int createdHeaders = 0;

            for (int i = 0; i < BufferCount; i++)
            {
                WaveOutHeaders[i] = (Win32.WAVEHDR*)Marshal.AllocHGlobal(sizeof(Win32.WAVEHDR));

                WaveOutHeaders[i]->dwLoops = 0;
                WaveOutHeaders[i]->dwUser = IntPtr.Zero;
                WaveOutHeaders[i]->lpNext = IntPtr.Zero;
                WaveOutHeaders[i]->reserved = IntPtr.Zero;
                WaveOutHeaders[i]->lpData = Marshal.AllocHGlobal(BufferSize);
                WaveOutHeaders[i]->dwBufferLength = (uint)BufferSize;
                WaveOutHeaders[i]->dwBytesRecorded = 0;
                WaveOutHeaders[i]->dwFlags = 0;

                Win32.MMRESULT hr = Win32.waveOutPrepareHeader(hWaveOut, WaveOutHeaders[i], sizeof(Win32.WAVEHDR));
                if (hr == Win32.MMRESULT.MMSYSERR_NOERROR)
                {
                    createdHeaders++;
                }
            }

            return (createdHeaders == BufferCount);
        }

        public bool PlayData(byte[] data)
        {
            int index = GetNextFreeWaveOutHeaderIndex();
            if (index != -1)
            {
                if (WaveOutHeaders[index]->dwBufferLength != data.Length)
                {
                    Marshal.FreeHGlobal(WaveOutHeaders[index]->lpData);
                    WaveOutHeaders[index]->lpData = Marshal.AllocHGlobal(data.Length);
                    WaveOutHeaders[index]->dwBufferLength = (uint)data.Length;
                }

                WaveOutHeaders[index]->dwBufferLength = (uint)data.Length;
                WaveOutHeaders[index]->dwUser = (IntPtr)index;
                Marshal.Copy(data, 0, WaveOutHeaders[index]->lpData, data.Length);

                Win32.MMRESULT hr = Win32.waveOutWrite(hWaveOut, WaveOutHeaders[index], sizeof(Win32.WAVEHDR));
                if (hr == Win32.MMRESULT.MMSYSERR_NOERROR)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// GetNextFreeWaveOutHeaderIndex
        /// </summary>
        /// <returns></returns>
        private int GetNextFreeWaveOutHeaderIndex()
        {
            for (int i = 0; i < WaveOutHeaders.Length; i++)
            {
                if (IsHeaderPrepared(*WaveOutHeaders[i]) && !IsHeaderInqueue(*WaveOutHeaders[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// IsHeaderPrepared
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        private bool IsHeaderPrepared(Win32.WAVEHDR header)
        {
            return (header.dwFlags & Win32.WaveHdrFlags.WHDR_PREPARED) > 0;
        }

        /// <summary>
        /// IsHeaderInqueue
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        private bool IsHeaderInqueue(Win32.WAVEHDR header)
        {
            return (header.dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) > 0;
        }

        /// <summary>
        /// waveOutProc
        /// </summary>
        /// <param name="hWaveOut"></param>
        /// <param name="msg"></param>
        /// <param name="dwInstance"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        private void waveOutProc(IntPtr hWaveOut, Win32.WOM_Messages msg, IntPtr dwInstance, Win32.WAVEHDR* pWaveHeader, IntPtr lParam)
        {
            switch (msg)
            {
                case Win32.WOM_Messages.OPEN:
                    break;
                case Win32.WOM_Messages.DONE:
                    break;
                case Win32.WOM_Messages.CLOSE:
                    this.hWaveOut = IntPtr.Zero;
                    break;
            }
        }

        public bool Close()
        {
            int count = 0;
            while (Win32.waveOutReset(hWaveOut) != Win32.MMRESULT.MMSYSERR_NOERROR && count <= 100)
            {
                System.Threading.Thread.Sleep(50);
                count++;
            }

            FreeWaveOutHeaders();

            count = 0;
            while (Win32.waveOutClose(hWaveOut) != Win32.MMRESULT.MMSYSERR_NOERROR && count <= 100)
            {
                System.Threading.Thread.Sleep(50);
                count++;
            }

            return true;
        }

        private void FreeWaveOutHeaders()
        {
            for (int i = 0; i < WaveOutHeaders.Length; i++)
            {
                Win32.MMRESULT hr = Win32.waveOutUnprepareHeader(hWaveOut, WaveOutHeaders[i], sizeof(Win32.WAVEHDR));

                int count = 0;
                while (count <= 100 && (WaveOutHeaders[i]->dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) == Win32.WaveHdrFlags.WHDR_INQUEUE)
                {
                    System.Threading.Thread.Sleep(20);
                    count++;
                }

                if ((WaveOutHeaders[i]->dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) != Win32.WaveHdrFlags.WHDR_INQUEUE)
                {
                    if (WaveOutHeaders[i]->lpData != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(WaveOutHeaders[i]->lpData);
                        WaveOutHeaders[i]->lpData = IntPtr.Zero;
                    }
                }
            }

        }
    }
}
