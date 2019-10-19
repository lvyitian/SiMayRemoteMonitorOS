using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.Controls
{
    public partial class ScreenQtyForm : Form
    {
        public ScreenQtyForm()
        {
            InitializeComponent();
        }

        public long QualityValue { get; set; }

        private void Button1_Click(object sender, EventArgs e)
        {
            QualityValue = (long)qty.Value;
            this.DialogResult = DialogResult.OK;
        }
    }
}
