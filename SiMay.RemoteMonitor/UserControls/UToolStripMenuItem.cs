using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.UserControls
{
    public class UToolStripMenuItem : ToolStripMenuItem
    {
        public UToolStripMenuItem(string name, Type type) 
            : base(name)
            => CtrlType = type;

        public Type CtrlType { get; set; }
    }
}
