using SiMay.Core.Common;
using SiMay.RemoteMonitor.Entitys.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.UserControls
{
    public class UListView : ListView
    {
        public bool UseWindowsThemStyle { get; set; } = true;

        private const uint WM_CHANGEUISTATE = 0x127;

        private const short UIS_SET = 1;
        private const short UISF_HIDEFOCUS = 0x1;
        private readonly IntPtr _removeDots = new IntPtr((int)UIS_SET << 16 | (int)(short)UISF_HIDEFOCUS);

        private ListViewColumnSorter LvwColumnSorter { get; set; }
        public UListView()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.LvwColumnSorter = new ListViewColumnSorter();
            this.ListViewItemSorter = LvwColumnSorter;
            this.View = View.Details;
            this.FullRowSelect = true;
        }

        protected override void OnColumnClick(ColumnClickEventArgs e)
        {
            base.OnColumnClick(e);

            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == this.LvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                this.LvwColumnSorter.Order = (this.LvwColumnSorter.Order == SortOrder.Ascending)
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                this.LvwColumnSorter.SortColumn = e.Column;
                this.LvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            //if (!this.VirtualMode)
            this.Sort();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (UseWindowsThemStyle)
            {
                if (PlatformHelper.RunningOnMono) return;

                if (PlatformHelper.VistaOrHigher)
                {
                    // set window theme to explorer
                    Win32Api.SetWindowTheme(this.Handle, "explorer", null);
                }

                if (PlatformHelper.XpOrHigher)
                {
                    // removes the ugly dotted line around focused item
                    Win32Api.SendMessage(this.Handle, WM_CHANGEUISTATE, _removeDots, IntPtr.Zero);
                }
            }
        }
    }
}
