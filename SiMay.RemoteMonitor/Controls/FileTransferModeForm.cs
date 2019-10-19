using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Controls
{
    public partial class FileTransferModeForm : Form
    {
        public FileTransferModeForm()
        {
            InitializeComponent();
        }

        public enum TransferMode
        {
            /// <summary>
            /// 覆盖
            /// </summary>
            Replace,
            /// <summary>
            /// 全部覆盖
            /// </summary>
            ReplaceAll,
            /// <summary>
            /// 续传
            /// </summary>
            Continuingly,
            /// <summary>
            /// 全部续传
            /// </summary>
            ContinuinglyAll,
            /// <summary>
            /// 跳过
            /// </summary>
            JumpOver,
            /// <summary>
            /// 取消传输
            /// </summary>
            Cancel
        }

        public string TipMessage
        {
            get { return tip.Text; }
            set { tip.Text = value; }
        }

        public TransferMode TransferModeResult
        {
            get; private set;
        } = TransferMode.Cancel;

        private void FileTransferModeDialog_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TransferModeResult = TransferMode.Replace;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TransferModeResult = TransferMode.ReplaceAll;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            TransferModeResult = TransferMode.Continuingly;
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TransferModeResult = TransferMode.ContinuinglyAll;
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            TransferModeResult = TransferMode.JumpOver;
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            TransferModeResult = TransferMode.Cancel;
            this.Close();
        }
    }
}