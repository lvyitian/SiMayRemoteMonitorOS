using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.ServiceCore
{
    public enum TrunkMessageHead : Int16
    {
        S_EnumerateSessions = 1000,//枚举所有会话信息
        S_Active,//激活
        S_SendSas,//发送安全序列
        S_CreateUserProcess,//创建用户进程
        S_InitiativeExit,//主动退出

        C_SessionItems = 2000 //会话信息
    }
}
