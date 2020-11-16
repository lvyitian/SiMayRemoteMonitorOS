using SiMay.Basic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SiMay.RemoteControls.Core;
using System.Threading.Tasks;

namespace SiMay.RemoteMonitor.UserControls
{
    public partial class UDesktopView : UserControl
    {
        public delegate void OnDoubleClickEventHnadler(SessionSyncContext sessionSync);
        public event OnDoubleClickEventHnadler OnDoubleClickEvent;

        private bool _isRun = true;

        public UDesktopView()
        {
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

        public DesktopViewSimpleApplication DesktopViewSimpleApplication { get; set; }

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


        public void StartPlay()
        {
            this._isRun = true;
            internalPlay();
        }

        private async void internalPlay()
        {
            var image = await DesktopViewSimpleApplication.GetDesktopViewFrame(SessionSyncContext.Session, this.Height, this.Width).ConfigureAwait(true);
            if (!image.IsNull())
            {
                img.Image?.Dispose();
                img.Image = image;
            }
            await Task.Delay(1000);
            if (this._isRun)
                internalPlay();
        }

        public void StopPlay()
            => this._isRun = false;
    }
}
