using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SiMay.Basic
{
    public class MessageBoxHelper
    {
        public static void ShowBoxExclamation(string tip, string title = "提示")
            => ShowBox(tip, title, MessageBoxIcon.Exclamation);

        public static void ShowBoxError(string tip, string title = "提示")
            => ShowBox(tip, title, MessageBoxIcon.Error);

        public static void ShowBox(string tip, string title, MessageBoxIcon boxIcon)
            => MessageBox.Show(tip, title, 0, boxIcon);
    }
}
