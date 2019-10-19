using System;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Controls
{

    public partial class RegistryEditForm : Form
    {

        public RegistryEditForm()
        {
            InitializeComponent();
        }

        public string KeyName
        {
            set
            {
                this.textBox1.Text = value;
            }
            get
            {
                return textBox1.Text;
            }
        }

        public string Value
        {
            get
            {
                return textBox2.Text;
            }
            set
            {
                textBox2.Text = value;
            }
        }

        public bool KeyNameEnabled
        {
            set
            {
                textBox1.Enabled = value;
            }
            get
            {
                return textBox1.Enabled;
            }
        }

        public bool ValueTextEnabled
        {
            set
            {
                textBox2.Enabled = value;
            }
            get
            {
                return textBox2.Enabled;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RegistryEdit_Load(object sender, EventArgs e)
        {
        }
    }
}