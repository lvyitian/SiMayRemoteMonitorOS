using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProvider.Core
{
    public enum MsgCommand
    {
        Msg_Pull_Session,//获取所有主连接
        Msg_Set_Session,//创建Session
        Msg_Set_Session_Id,//主连接关联Session
        Msg_Close_Session,//Session离线
        Msg_Connect_Work,//发起一个工作连接
        Msg_LogOut,//退出登陆
        Msg_MessageData,//数据
        Msg_AccessKeyWrong//AccessKey错误
    }
}
