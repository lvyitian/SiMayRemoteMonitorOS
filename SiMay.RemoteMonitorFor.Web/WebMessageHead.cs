using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.RemoteMonitorForWeb
{
    public enum WebMessageHead
    {
        B_LOGIN = 1000,
        B_DESKTOP_VIEW_GETFRAME,
        B_MESSAGE_BOX,
        B_RESET_DES,
        B_SYS_SESSION,
        B_OPEN_URL,
        B_EXEC_DOWNLOAD,

        S_LOGIN_SESSION = 2000,
        S_CLOSE_SESSION,
        S_LOGOUT,
        S_DESKTOP_VIEW_DATA,
        S_ID_OR_KEY_WRONG
    }
}
