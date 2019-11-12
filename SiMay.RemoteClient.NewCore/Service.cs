using SiMay.Core;
using SiMay.ServiceCore.Win32;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.TcpConfiguration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace SiMay.ServiceCore
{
    public partial class Service : ServiceBase
    {
        struct UserProcessToken
        {
            public int SessionId { get; set; }

        }
        public Service()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            var desktopName = Win32Interop.GetCurrentDesktop();
            var openProcessString = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(Assembly.GetExecutingAssembly().Location));
            //用户进程启动
            var result = Win32Interop.OpenInteractiveProcess(openProcessString + " \"-user\"", desktopName, true, out _);

            var serverConfig = new TcpSocketSaeaServerConfiguration();
            serverConfig.AppKeepAlive = true;
            serverConfig.PendingConnectionBacklog = 0;
            var trunkService = TcpSocketsFactory.CreateServerAgent(TcpSocketSaeaSessionType.Packet, serverConfig,
                (notity, session) =>
            {
                switch (notity)
                {
                    case TcpSocketCompletionNotify.OnConnected:
                        break;
                    case TcpSocketCompletionNotify.OnSend:
                        break;
                    case TcpSocketCompletionNotify.OnDataReceiveing:
                        break;
                    case TcpSocketCompletionNotify.OnDataReceived:
                        break;
                    case TcpSocketCompletionNotify.OnClosed:
                        break;
                    default:
                        break;
                }
            });

            bool completed = false;
            for (int trycount = 0; trycount < 100; trycount++)
            {
                try
                {
                    var ipEndPoint = new IPEndPoint(IPAddress.Any, 10000 + trycount);
                    trunkService.Listen(ipEndPoint);
                    completed = true;
                    break;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteErrorByCurrentMethod("trunkService open listen exception:" + ex.Message);
                }
                completed = false;
                Thread.Sleep(1000);
            }

            LogHelper.WriteErrorByCurrentMethod("listen all tcp port not completed，please check!");

            if (!completed)
                Environment.Exit(0);//监听所有端口失败
        }

        protected override void OnStop()
        {
        }
    }
}
