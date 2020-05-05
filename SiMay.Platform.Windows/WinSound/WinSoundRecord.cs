using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Platform.Windows
{
    unsafe public class WinSoundRecord
    {
        private IntPtr hWaveIn = IntPtr.Zero;
        private String WaveInDeviceName = "";
        private int SamplesPerSecond = 8000;
        private int BitsPerSample = 16;
        private int Channels = 1;
        private int BufferCount = 8;
        private int BufferSize = 1024;
        private Win32.WAVEHDR*[] WaveInHeaders;
        private Win32.WAVEHDR* CurrentRecordedHeader;
        private Win32.DelegateWaveInProc delegateWaveInProc;
        private System.Threading.Thread ThreadRecording;
        private System.Threading.AutoResetEvent AutoResetEventDataRecorded = new System.Threading.AutoResetEvent(false);

        public delegate void DelegateDataRecorded(Byte[] bytes);

        public event DelegateDataRecorded DataRecorded;

        public WinSoundRecord()
        {
            delegateWaveInProc = new Win32.DelegateWaveInProc(waveInProc);
        }

        /// <summary>
        /// StartWaveIn
        /// </summary>
        /// <returns></returns>
        private bool OpenWaveIn()
        {
            Win32.WAVEFORMATEX waveFormatEx = new Win32.WAVEFORMATEX();
            waveFormatEx.wFormatTag = (ushort)Win32.WaveFormatFlags.WAVE_FORMAT_PCM;
            waveFormatEx.nChannels = (ushort)Channels;
            waveFormatEx.nSamplesPerSec = (ushort)SamplesPerSecond;
            waveFormatEx.wBitsPerSample = (ushort)BitsPerSample;
            waveFormatEx.nBlockAlign = (ushort)((waveFormatEx.wBitsPerSample * waveFormatEx.nChannels) >> 3);
            waveFormatEx.nAvgBytesPerSec = (uint)(waveFormatEx.nBlockAlign * waveFormatEx.nSamplesPerSec);

            int deviceId = WinSound.GetWaveInDeviceIdByName(WaveInDeviceName);
            Win32.MMRESULT hr = Win32.waveInOpen(ref hWaveIn, deviceId, ref waveFormatEx, delegateWaveInProc, 0, (int)Win32.WaveProcFlags.CALLBACK_FUNCTION);

            if (hWaveIn == IntPtr.Zero)
                return false;

            GCHandle.Alloc(hWaveIn, GCHandleType.Pinned);

            return true;
        }

        public bool Open(string deviceName, int samplesPerSecond, int bitsPerSample, int channels, int bufferSize, int bufferCount)
        {
            this.WaveInDeviceName = deviceName;
            this.SamplesPerSecond = samplesPerSecond;
            this.BitsPerSample = bitsPerSample;
            this.Channels = channels;
            this.BufferSize = bufferSize;
            this.BufferCount = bufferCount;

            if (OpenWaveIn())
            {
                if (CreateWaveInHeaders())
                {
                    Win32.MMRESULT hr = Win32.waveInStart(hWaveIn);
                    if (hr == Win32.MMRESULT.MMSYSERR_NOERROR)
                    {
                        StartThreadRecording();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// StartThreadRecording
        /// </summary>
        private void StartThreadRecording()
        {
            ThreadRecording = new System.Threading.Thread(new System.Threading.ThreadStart(OnThreadRecording));
            ThreadRecording.Name = "Recording";
            ThreadRecording.Priority = System.Threading.ThreadPriority.Highest;
            ThreadRecording.Start();
        }

        /// <summary>
        /// OnThreadRecording
        /// </summary>
        private void OnThreadRecording()
        {
            while (true)
            {

                AutoResetEventDataRecorded.WaitOne();


                if (CurrentRecordedHeader->dwBytesRecorded > 0)
                {

                    Byte[] bytes = new Byte[CurrentRecordedHeader->dwBytesRecorded];
                    Marshal.Copy(CurrentRecordedHeader->lpData, bytes, 0, (int)CurrentRecordedHeader->dwBytesRecorded);

                    DataRecorded?.Invoke(bytes);

                    for (int i = 0; i < WaveInHeaders.Length; i++)
                    {
                        if ((WaveInHeaders[i]->dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) == 0)
                        {
                            Win32.MMRESULT hr = Win32.waveInAddBuffer(hWaveIn, WaveInHeaders[i], sizeof(Win32.WAVEHDR));
                        }
                    }
                }

            }
        }

        /// <summary>
        /// CreateWaveInHeaders
        /// </summary>
        /// <param name="count"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        private bool CreateWaveInHeaders()
        {
            WaveInHeaders = new Win32.WAVEHDR*[BufferCount];
            int createdHeaders = 0;

            for (int i = 0; i < BufferCount; i++)
            {
                WaveInHeaders[i] = (Win32.WAVEHDR*)Marshal.AllocHGlobal(sizeof(Win32.WAVEHDR));

                WaveInHeaders[i]->dwLoops = 0;
                WaveInHeaders[i]->dwUser = IntPtr.Zero;
                WaveInHeaders[i]->lpNext = IntPtr.Zero;
                WaveInHeaders[i]->reserved = IntPtr.Zero;
                WaveInHeaders[i]->lpData = Marshal.AllocHGlobal(BufferSize);
                WaveInHeaders[i]->dwBufferLength = (uint)BufferSize;
                WaveInHeaders[i]->dwBytesRecorded = 0;
                WaveInHeaders[i]->dwFlags = 0;

                Win32.MMRESULT hr = Win32.waveInPrepareHeader(hWaveIn, WaveInHeaders[i], sizeof(Win32.WAVEHDR));
                if (hr == Win32.MMRESULT.MMSYSERR_NOERROR)
                {
                    if (i == 0)
                    {
                        hr = Win32.waveInAddBuffer(hWaveIn, WaveInHeaders[i], sizeof(Win32.WAVEHDR));
                    }
                    createdHeaders++;
                }
            }

            return (createdHeaders == BufferCount);
        }

        public void Stop()
        {
            Win32.MMRESULT hr = Win32.waveInStop(hWaveIn);

            int resetCount = 0;
            while (IsAnyWaveInHeaderInState(Win32.WaveHdrFlags.WHDR_INQUEUE) & resetCount < 20)
            {
                hr = Win32.waveInReset(hWaveIn);
                System.Threading.Thread.Sleep(50);
                resetCount++;
            }

            FreeWaveInHeaders();
            hr = Win32.waveInClose(hWaveIn);

            AutoResetEventDataRecorded.Set();
        }

        /// <summary>
        /// IsAnyWaveInHeaderInState
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private bool IsAnyWaveInHeaderInState(Win32.WaveHdrFlags state)
        {
            for (int i = 0; i < WaveInHeaders.Length; i++)
            {
                if ((WaveInHeaders[i]->dwFlags & state) == state)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// FreeWaveInHeaders
        /// </summary>
        private void FreeWaveInHeaders()
        {
            if (WaveInHeaders != null)
            {
                for (int i = 0; i < WaveInHeaders.Length; i++)
                {
                    Win32.MMRESULT hr = Win32.waveInUnprepareHeader(hWaveIn, WaveInHeaders[i], sizeof(Win32.WAVEHDR));

                    int count = 0;
                    while (count <= 100 && (WaveInHeaders[i]->dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) == Win32.WaveHdrFlags.WHDR_INQUEUE)
                    {
                        System.Threading.Thread.Sleep(20);
                        count++;
                    }

                    if ((WaveInHeaders[i]->dwFlags & Win32.WaveHdrFlags.WHDR_INQUEUE) != Win32.WaveHdrFlags.WHDR_INQUEUE)
                    {
                        if (WaveInHeaders[i]->lpData != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(WaveInHeaders[i]->lpData);
                            WaveInHeaders[i]->lpData = IntPtr.Zero;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// waveInProc
        /// </summary>
        /// <param name="hWaveIn"></param>
        /// <param name="msg"></param>
        /// <param name="dwInstance"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        private void waveInProc(IntPtr hWaveIn, Win32.WIM_Messages msg, IntPtr dwInstance, Win32.WAVEHDR* pWaveHdr, IntPtr lParam)
        {
            switch (msg)
            {
                case Win32.WIM_Messages.OPEN:
                    break;
                case Win32.WIM_Messages.DATA:
                    CurrentRecordedHeader = pWaveHdr;
                    AutoResetEventDataRecorded.Set();
                    break;
                case Win32.WIM_Messages.CLOSE:
                    AutoResetEventDataRecorded.Set();
                    this.hWaveIn = IntPtr.Zero;
                    break;
            }
        }
    }
}
