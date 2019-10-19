using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.HttpRemoteMonitorService
{
    public enum AJaxMsgCommand
    {
        B_LOGIN = 1000,
        B_DESKTOPVIEW_PULL,
        B_RESET_REMARK,
        B_OPEN_DESKTOPVIEW,
        B_CLOSE_DESKTOPVIEW,
        B_MESSAGEBOX,
        B_SESSION_MANAGER,
        B_OPEN_URL,
        B_DOWNLOAD_EX,
        B_UNSTALLER,

        S_SESSION_LOGIN = 2000,
        S_SESSION_CLOSE,
        S_MANAGER_LOGOUT,
        S_DESKTOPVIEW_IMG,
        S_MANAGERCHANNEL_LOGOUT,
        S_IDORKEYWRONG
    }
}
