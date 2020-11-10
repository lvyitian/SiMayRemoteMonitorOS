using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControlsCore
{
    public enum TransportMode
    {
        /// <summary>
        /// 覆盖
        /// </summary>
        Replace,
        /// <summary>
        /// 全部覆盖
        /// </summary>
        ReplaceAll,
        /// <summary>
        /// 续传
        /// </summary>
        Continuingly,
        /// <summary>
        /// 全部续传
        /// </summary>
        ContinuinglyAll,
        /// <summary>
        /// 跳过
        /// </summary>
        JumpOver,
        /// <summary>
        /// 取消传输
        /// </summary>
        Cancel
    }
}
