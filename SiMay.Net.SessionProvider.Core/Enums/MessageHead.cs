using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Net.SessionProvider.Core
{
    /// <summary>
    /// APP_XX : 主控端消息
    /// MID_XX : 中间服务消息
    /// </summary>
    public enum MessageHead
    {
        /// <summary>
        /// //获取所有Session
        /// </summary>
        APP_PULL_SESSION,

        /// <summary>
        /// 关联Session信息
        /// </summary>
        MID_SESSION,

        /// <summary>
        /// Session离线
        /// </summary>
        MID_SESSION_CLOSED,

        /// <summary>
        /// 发起一个工作连接
        /// </summary>
        MID_APPWORK,

        /// <summary>
        /// 向主服务连接发送消息
        /// </summary>
        APP_MESSAGE_DATA,

        /// <summary>
        /// 转发的主服务连接数据
        /// </summary>
        MID_MESSAGE_DATA,

        /// <summary>
        /// AccessKey错误
        /// </summary>
        MID_ACCESS_KEY_WRONG,

        /// <summary>
        /// 登出
        /// </summary>
        MID_LOGOUT,
    }
}
