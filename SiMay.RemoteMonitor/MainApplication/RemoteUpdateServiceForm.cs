using SiMay.Basic;
using SiMay.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.MainApplication
{
    public partial class RemoteUpdateServiceForm : Form
    {
        public RemoteUpdateServiceForm()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }

        public RemoteUpdateType UrlOrFileUpdate { get; set; }
        public string Value { get; set; }
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (radioLocalFile.Checked)
            {
                if (!File.Exists(txtPath.Text))
                {
                    MessageBoxHelper.ShowBoxError("请选择正确的文件路径!");
                    return;
                }
                if (new FileInfo(txtPath.Text).Length > 1024 * 1024)
                {
                    MessageBoxHelper.ShowBoxError("文件大于1M!");
                    return;
                }
                Value = txtPath.Text;
                UrlOrFileUpdate =  RemoteUpdateType.File;
            }
            else
            {

                Value = txtURL.Text;
                UrlOrFileUpdate = RemoteUpdateType.Url;
            }

            if (MessageBox.Show("该操作是危险操作，请确认文件或URL是否正确，否则可能导致上线失败!", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                this.DialogResult = DialogResult.OK;
            else
                return;

            this.Close();
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = false;
                ofd.Filter = "Executable (*.exe)|*.exe";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = Path.Combine(ofd.InitialDirectory, ofd.FileName);
                }
            }
        }
    }
}
