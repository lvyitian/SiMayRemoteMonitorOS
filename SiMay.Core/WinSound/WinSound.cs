using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindSound
{
    public class WinSound
    {
        public static int GetWaveInDeviceIdByName(string deviceName)
        {
            uint num = Win32.waveInGetNumDevs();

            Win32.WAVEINCAPS caps = new Win32.WAVEINCAPS();
            for (int i = 0; i < num; i++)
            {
                Win32.HRESULT hr = (Win32.HRESULT)Win32.waveInGetDevCaps(i, ref caps, Marshal.SizeOf(typeof(Win32.WAVEINCAPS)));
                if (hr == Win32.HRESULT.S_OK)
                {
                    if (caps.szPname == deviceName)
                    {
                        return i;
                    }
                }
            }
            return Win32.WAVE_MAPPER;
        }

        public static List<string> GetWaveInDeviceNames()
        {
            uint num = Win32.waveInGetNumDevs();

            List<string> names = new List<string>();
            Win32.WAVEINCAPS caps = new Win32.WAVEINCAPS();
            for (int i = 0; i < num; i++)
            {
                Win32.HRESULT hr = (Win32.HRESULT)Win32.waveInGetDevCaps(i, ref caps, Marshal.SizeOf(typeof(Win32.WAVEINCAPS)));
                if (hr == Win32.HRESULT.S_OK)
                {
                    names.Add(caps.szPname);
                }
            }
            return names;
        }

        /// <summary>
        /// GetWaveOutDeviceIdByName
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetWaveOutDeviceIdByName(string name)
        {
            uint num = Win32.waveOutGetNumDevs();
            Win32.WAVEOUTCAPS caps = new Win32.WAVEOUTCAPS();
            for (int i = 0; i < num; i++)
            {
                Win32.HRESULT hr = (Win32.HRESULT)Win32.waveOutGetDevCaps(i, ref caps, Marshal.SizeOf(typeof(Win32.WAVEOUTCAPS)));
                if (hr == Win32.HRESULT.S_OK)
                {
                    if (caps.szPname == name)
                    {
                        return i;
                    }
                }
            }
            return Win32.WAVE_MAPPER;
        }

        /// <summary>
        /// GetWaveOutDeviceIdByName
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<string> GetWaveOutDeviceNames()
        {
            uint num = Win32.waveOutGetNumDevs();

            List<string> names = new List<string>();
            Win32.WAVEOUTCAPS caps = new Win32.WAVEOUTCAPS();
            for (int i = 0; i < num; i++)
            {
                Win32.HRESULT hr = (Win32.HRESULT)Win32.waveOutGetDevCaps(i, ref caps, Marshal.SizeOf(typeof(Win32.WAVEOUTCAPS)));
                if (hr == Win32.HRESULT.S_OK)
                {
                    names.Add(caps.szPname);
                }
            }
            return names;
        }
    }
}
