using SiMay.Core;
using SiMay.Core.Packets.RegEdit;
using System;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Application
{
    public partial class RegValueEditMultiStringForm : Form
    {
        private readonly RegValueData _value;

        public RegValueEditMultiStringForm(RegValueData value)
        {
            _value = value;

            InitializeComponent();

            this.valueNameTxtBox.Text = value.Name;
            this.valueDataTxtBox.Text = string.Join("\r\n", ByteConverterHelper.ToStringArray(value.Data));
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _value.Data = ByteConverterHelper.GetBytes(valueDataTxtBox.Text.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries));
            this.Tag = _value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
