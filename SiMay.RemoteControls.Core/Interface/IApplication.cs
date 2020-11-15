using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.RemoteControls.Core
{
    /// <summary>
    /// 标识类型为应用
    /// </summary>
    public interface IApplication
    {
        /// <summary>
        /// 开始工作
        /// </summary>
        void Start();

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="arg"></param>
        void SetParameter(object arg);

        /// <summary>
        /// 当会话断开时
        /// </summary>
        void SessionClose(ApplicationBaseAdapterHandler handler);

        /// <summary>
        /// 当会话恢复时
        /// </summary>
        void ContinueTask(ApplicationBaseAdapterHandler handler);
    }
}
