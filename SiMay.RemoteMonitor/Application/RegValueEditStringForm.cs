using System;
using System.Windows.Forms;
using SiMay.Core;
using SiMay.Core.Common;
using SiMay.Core.Packets.RegEdit;

namespace SiMay.RemoteMonitor.Application
{
    public partial class RegValueEditStringForm : Form
    {
        private readonly RegValueData _value;

        public RegValueEditStringForm(RegValueData value)
        {
            _value = value;

            InitializeComponent();

            this.valueNameTxtBox.Text = RegValueHelper.GetName(value.Name);
            this.valueDataTxtBox.Text = ByteConverter.ToString(value.Data);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _value.Data = ByteConverter.GetBytes(valueDataTxtBox.Text);
            this.Tag = _value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
