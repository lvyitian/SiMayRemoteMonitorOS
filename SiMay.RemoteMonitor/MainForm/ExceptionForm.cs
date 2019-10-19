using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor
{
    public partial class ExceptionDialog : Form
    {
        public ExceptionDialog(string msg)
        {
            InitializeComponent();

            exceptionMsg_Text.Text = msg;

            this.Show();
        }
    }
}
