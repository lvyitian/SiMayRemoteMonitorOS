using System;
using System.Windows.Forms;
using SiMay.Basic;
using SiMay.Core;

namespace SiMay.RemoteMonitor.MainApplication
{
    public partial class NotifyMessageBoxForm : Form
    {
        public NotifyMessageBoxForm()
        {
            InitializeComponent();
        }

        public string MessageBody { get; set; }
        public string MessageTitle { get; set; }
        public MessageIconKind MsgBoxIcon { get; set; }

        private void GMessageBox_Load(object sender, EventArgs e)
        {
            errorPic.Image = System.Drawing.SystemIcons.Error.ToBitmap();
            exclaPic.Image = System.Drawing.SystemIcons.Exclamation.ToBitmap();
            questionPic.Image = System.Drawing.SystemIcons.Question.ToBitmap();
            infoPic.Image = System.Drawing.SystemIcons.Information.ToBitmap();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_errorRadio.Checked == true)
                MessageBox.Show(txtValue.Text, txtTitle.Text, 0, MessageBoxIcon.Error);
            else if (m_questionRadio.Checked == true)
                MessageBox.Show(txtValue.Text, txtTitle.Text, 0, MessageBoxIcon.Question);
            else if (m_infoRadio.Checked == true)
                MessageBox.Show(txtValue.Text, txtTitle.Text, 0, MessageBoxIcon.Information);
            else if (m_exclaRadio.Checked == true)
                MessageBox.Show(txtValue.Text, txtTitle.Text, 0, MessageBoxIcon.Exclamation);
        }



        private void button2_Click(object sender, EventArgs e)
        {
            if (txtValue.Text.Length > 2000 || txtTitle.Text.Length > 256)
            {
                MessageBoxHelper.ShowBoxError("内容太长!");
                return;
            }
            if (m_errorRadio.Checked == true)
                MsgBoxIcon = MessageIconKind.Error;
            else if (m_questionRadio.Checked == true)
                MsgBoxIcon = MessageIconKind.Question;
            else if (m_infoRadio.Checked == true)
                MsgBoxIcon = MessageIconKind.InforMation;
            else if (m_exclaRadio.Checked == true)
                MsgBoxIcon = MessageIconKind.Exclaim;

            this.MessageTitle = txtTitle.Text;
            this.MessageBody = txtValue.Text;

            DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
    }
}
