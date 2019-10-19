using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace SiMay.ServiceCore
{
    public static class Win32Api
    {
        [DllImport("user32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(
            IntPtr hwnd,
            int wMsg,
            int wParam,
            int lParam);

        public const int WM_SYSCOMMAND = 0x112;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;

        public const uint SC_MONITORPOWER = 0xf170;

        public static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);

        public const int MOUSEEVENTF_MOVE = 0x0001;            //mouse move
        public const int MOUSEEVENTF_LEFTDOWN = 0x0002;        //left button down
        public const int MOUSEEVENTF_LEFTUP = 0x0004;          //left button up
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;       //right button down
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;         //right button up
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;      //middle button down
        public const int MOUSEEVENTF_MIDDLEUP = 0x0040;        //middle button up
        public const int MOUSEEVENTF_XDOWN = 0x0080;           //x button down
        public const int MOUSEEVENTF_XUP = 0x0100;             //x button down
        public const int MOUSEEVENTF_WHEEL = 0x0800;           //wheel button rolled
        public const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        [DllImport("User32.dll")]
        public static extern bool SetCursorPos(
            int x,
            int y);

        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(
            byte bVk,
            byte bScan,
            uint dwFlags,
            uint dwExtraInfo);

        public const int WM_KEYUP = 0x0002;

        [DllImport("user32")]
        public static extern int mouse_event(int dwFlags,
            int dx,
            int dy,
            int cButtons,
            int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern int SendMessage(
            IntPtr hWnd,
            uint Msg,
            uint wParam,
            int lParam);

        [DllImport("user32.dll")]
        public static extern void BlockInput(
            bool Block);

        public const int WH_KEYBOARD_LL = 13;
        //private const int WM_KEYUP = 0x101;
        public const int WM_KEYDOWN = 0x100;

        public struct KeyInfoStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        [DllImport("user32.dll")]
        public static extern int SetWindowsHookEx(
            int idHook,
            HookProc hProc,
            IntPtr hMod,
            int dwThreadId);

        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(
            int hHook,
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(int hHook);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern bool GetDiskFreeSpaceEx(
            string lpDirectoryName,
            out ulong lpFreeBytesAvailable,
            out ulong lpTotalNumberOfBytes,
            out ulong lpTotalNumberOfFreeBytes);


        public enum ShowWindowCommands : int
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,    //用最近的大小和位置显示，激活
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_MAX = 10
        }

        [DllImport("shell32.dll")]
        public static extern IntPtr ShellExecute(
            IntPtr hwnd,
            string lpszOp,
            string lpszFile,
            string lpszParams,
            string lpszDir,
            ShowWindowCommands FsShowCmd
            );

        //[DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool PostThreadMessage(int idThread, int Msg, IntPtr wParam, IntPtr lParam);



        [DllImport("iphlpapi.dll", SetLastError = true)]
        internal static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion,
            TcpTableClass tblClass, uint reserved = 0);

        [DllImport("iphlpapi.dll")]
        internal static extern int SetTcpEntry(IntPtr pTcprow);

        [StructLayout(LayoutKind.Sequential)]
        internal struct MibTcprowOwnerPid
        {
            public uint state;
            public uint localAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] localPort;
            public uint remoteAddr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] remotePort;
            public uint owningPid;

            public IPAddress LocalAddress
            {
                get { return new IPAddress(localAddr); }
            }

            public ushort LocalPort
            {
                get { return BitConverter.ToUInt16(new byte[2] { localPort[1], localPort[0] }, 0); }
            }

            public IPAddress RemoteAddress
            {
                get { return new IPAddress(remoteAddr); }
            }

            public ushort RemotePort
            {
                get { return BitConverter.ToUInt16(new byte[2] { remotePort[1], remotePort[0] }, 0); }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MibTcptableOwnerPid
        {
            public uint dwNumEntries;
            private readonly MibTcprowOwnerPid table;
        }

        internal enum TcpTableClass
        {
            TcpTableBasicListener,
            TcpTableBasicConnections,
            TcpTableBasicAll,
            TcpTableOwnerPidListener,
            TcpTableOwnerPidConnections,
            TcpTableOwnerPidAll,
            TcpTableOwnerModuleListener,
            TcpTableOwnerModuleConnections,
            TcpTableOwnerModuleAll
        }
    }
}
