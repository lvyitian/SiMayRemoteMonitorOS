using SiMay.Core;
using System;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Extensions
{
    public static class ListViewExtensions
    {
        private const uint SET_COLUMN_WIDTH = 4126;
        private static readonly IntPtr AUTOSIZE_USEHEADER = new IntPtr(-2);

        /// <summary>
        /// Automatically determines the correct column size on the the given listview.
        /// </summary>
        /// <param name="targetListView">The listview whose columns are to be autosized.</param>
        public static void AutosizeColumns(this ListView targetListView)
        {
            if (PlatformHelper.RunningOnMono) return;
            for (int lngColumn = 0; lngColumn <= (targetListView.Columns.Count - 1); lngColumn++)
            {
                Win32Api.SendMessage(targetListView.Handle, SET_COLUMN_WIDTH, new IntPtr(lngColumn), AUTOSIZE_USEHEADER);
            }
        }
    }
}