using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.RemoteControlsCore
{
    public interface IDesktopView
    {
        /// <summary>
        /// 视图高
        /// </summary>
        int Height { get; set; }
        /// <summary>
        /// 视图宽
        /// </summary>
        int Width { get; set; }
        /// <summary>
        /// 视图标题
        /// </summary>
        string Caption { get; set; }

        /// <summary>
        /// 会话同步上下文
        /// </summary>
        SessionSyncContext SessionSyncContext { get; set; }


        MainApplicationAdapterHandler Owner { get; set; }

        /// <summary>
        /// 展示视图
        /// </summary>
        /// <param name="image"></param>
        void PlayerDekstopView(Image image);

        /// <summary>
        /// 关闭视图
        /// </summary>
        void CloseDesktopView();
    }
}
