using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SiMay.Basic;

namespace SiMay.RemoteMonitor.MainApplication
{
    public partial class DesktopViewWallSettingForm : Form
    {
        DesktopViewSettingContext _settingContext;
        public DesktopViewWallSettingForm(DesktopViewSettingContext settingContext)
        {
            _settingContext = settingContext;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (deskrefreshTimeInterval.Value < 300)
            {
                MessageBoxHelper.ShowBoxError("设置未保存,刷新间隔不能小于300!", "error");
                return;
            }
            this._settingContext.ViewFreshInterval = (int)deskrefreshTimeInterval.Value;
            this._settingContext.CarouselEnabled = this.enabled.Checked;
            this._settingContext.ViewCarouselInterval = (int)carouselInterval.Value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.SwithTip();
        }

        private void SwithTip()
        {
            if (enabled.Checked)
                enabled.Text = "启用";
            else
                enabled.Text = "不启用";
        }

        private void DesktopViewWallSettingForm_Load(object sender, EventArgs e)
        {
            //this.DialogResult = DialogResult.Cancel;
            this.deskrefreshTimeInterval.Value = this._settingContext.ViewFreshInterval;
            this.carouselInterval.Value = this._settingContext.ViewCarouselInterval;
            this.enabled.Checked = this._settingContext.CarouselEnabled;
            this.SwithTip();
        }
    }
}
