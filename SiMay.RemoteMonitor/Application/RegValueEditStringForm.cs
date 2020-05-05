using SiMay.Platform.Windows;
using System;
using System.Windows.Forms;

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
            this.valueDataTxtBox.Text = ByteConverterHelper.ToString(value.Data);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            _value.Data = ByteConverterHelper.GetBytes(valueDataTxtBox.Text);
            this.Tag = _value;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
