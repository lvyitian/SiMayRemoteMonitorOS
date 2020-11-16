using SiMay.Basic;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiMay.Service.Core
{
    public class MessageBoxSimpleService : RemoteSimpleServiceBase
    {
        [PacketHandler(MessageHead.S_SIMPLE_MESSAGE_BOX)]
        public void ShowMessageBox(SessionProviderContext session)
        {
            var msg = session.GetMessageEntity<MessagePacket>();
            ThreadHelper.CreateThread(() =>
            {
                string title = msg.MessageTitle;
                string content = msg.MessageBody;

                switch ((MessageIconKind)msg.MessageIcon)
                {
                    case MessageIconKind.Error:
                        MessageBox.Show(content, title, 0, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        break;

                    case MessageIconKind.Question:
                        MessageBox.Show(content, title, 0, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        break;

                    case MessageIconKind.InforMation:
                        MessageBox.Show(content, title, 0, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        break;

                    case MessageIconKind.Exclaim:
                        MessageBox.Show(content, title, 0, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        break;
                }
            }, true);
        }
    }
}
