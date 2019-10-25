using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.MainApplication
{
    public partial class EnterForm : Form
    {
        public EnterForm()
        {
            InitializeComponent();
        }

        public string Caption
        {
            get { return m_caption.Text; }
            set { m_caption.Text = value; }
        }

        public string Value
        {
            get { return txtEdit.Text; }
            set { txtEdit.Text = value; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Value == "")
            {
                MessageBox.Show("输入的内容不能为空!", "提示", 0, MessageBoxIcon.Exclamation);
                return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Value = txtEdit.Text;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
