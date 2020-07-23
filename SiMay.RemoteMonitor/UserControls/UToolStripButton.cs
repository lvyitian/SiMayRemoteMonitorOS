using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiMay.RemoteMonitor.UserControls
{
    public class UToolStripButton : ToolStripButton
    {
        public UToolStripButton(string text, Image image, Type type)
        {
            this.ApplicationType = type;
            this.Text = text;
            this.Image = image;
            this.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
        }

        public Type ApplicationType { get; set; }
    }
}
