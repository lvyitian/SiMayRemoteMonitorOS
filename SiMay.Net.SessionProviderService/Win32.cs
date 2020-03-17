using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProviderService
{
    public class Win32
    {
        public const int WM_SYSCOMMAND = 0x112;

        public const Int32 MF_SEPARATOR = 0x800;
        public const Int32 MF_BYPOSITION = 0x400;
        public const Int32 MF_STRING = 0x0;
        public const Int32 MF_REMOVE = 0x1000;

        public const uint MF_ENABLED = 0;
        public const uint MF_DISABLED = (uint)0x00000002L;
        public const uint MF_UNCHECKED = 0;
        public const uint MF_CHECKED = (uint)0x00000008L;

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);//获取窗体系统菜单

        [DllImport("user32.dll")]
        public static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);//插入菜单

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool RemoveMenu(IntPtr hMenu, uint nPosition, uint nFlags);//删除菜单

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern bool SetMenuItemBitmaps(IntPtr hMenu, Int32 nPosition, Int32 nFlags, Bitmap pBmpUnchecked, Bitmap pBmpchecked);//设置菜单图片

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool InsertMenuItem(IntPtr hMenu, Int32 uItem, Int32 lpMenuItemInfo, bool fByPos);//插入菜单

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern bool AppendMenu(IntPtr hMenu, Int32 nFlags, Int32 IDNewItem, string lpNewItem);//追加菜单

        [DllImport("user32.dll")]
        public static extern ulong CheckMenuItem(IntPtr hmenu, uint uIDCheckItem, uint uCheck);//设置菜单勾选

        [DllImport("user32.dll")]
        public static extern bool EnableMenuItem(IntPtr hmenu, uint uIDEnableItem, uint wEnable);//设置菜单可用


        [DllImport("User32")]
        public static extern bool FlashWindow(IntPtr hWnd, bool bInvert);//任务栏图标闪烁
    }
}
