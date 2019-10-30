using SiMay.RemoteControlsCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Application
{
    public partial class FileTransferModeForm : Form
    {
        public FileTransferModeForm()
        {
            InitializeComponent();
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