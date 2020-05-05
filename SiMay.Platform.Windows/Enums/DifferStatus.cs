using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiMay.Platform.Windows
{
    public enum DifferStatus
    {
        /// <summary>
        /// 全屏扫描完成
        /// </summary>
        FULL_DIFFERENCES,

        /// <summary>
        /// 差异下一帧扫描完成
        /// </summary>
        NEXT_SCREEN,

        /// <summary>
        /// 完成扫描
        /// </summary>
        COMPLETED
    }
}
