using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SiMay.RemoteControlsCore;

namespace SiMay.RemoteMonitor.UserControls
{
    public partial class UDesktopView : UserControl, IDesktopView
    {
        public delegate void OnDoubleClickEventHnadler(SessionSyncContext sessionSync);
        public event OnDoubleClickEventHnadler OnDoubleClickEvent;

        //public MainApplicationAdapterHandler Owner { get; set; }


        public UDesktopView(SessionSyncContext syncContext)
        {
            SessionSyncContext = syncContext;
            InitializeComponent();
        }

        public string Caption
        {
            get { return checkBox.Text; }
            set { checkBox.Text = value; }
        }

        public bool Checked
        {
            get { return checkBox.Checked; }
            set { checkBox.Checked = value; }
        }

        public SessionSyncContext SessionSyncContext { get; set; }

        private void img_DoubleClick(object sender, EventArgs e)
        {
            this.OnDoubleClickEvent?.Invoke(SessionSyncContext);
        }

        private void UDesktopView_Load(object sender, EventArgs e)
        {
            Bitmap bmap = new Bitmap(img.Width, img.Height);
            Graphics g = Graphics.FromImage(bmap);
            g.Clear(Color.Black);
            g.DrawString("桌面加载中...", new Font("微软雅黑", 7, FontStyle.Regular), new SolidBrush(Color.Red), new Point((img.Width / 2) - 35, img.Height / 2));
            g.Dispose();

            img.Image = bmap;
        }

        private void img_Click(object sender, EventArgs e)
        {
            if (checkBox.Checked)
                checkBox.Checked = false;
            else
                checkBox.Checked = true;
        }

        public void PlayerDekstopView(Image image)
        {
            if (img == null)
                return;

            img.Image?.Dispose();
            img.Image = image;

            //Owner.GetDesktopViewFrame(SessionSyncContext);
        }

        public void CloseDesktopView()
        {
            this.Dispose();
        }
    }
}
