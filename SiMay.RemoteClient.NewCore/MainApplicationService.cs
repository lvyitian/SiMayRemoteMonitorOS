using SiMay.Basic;
using SiMay.Core;
using SiMay.ModelBinder;
using SiMay.Net.SessionProvider;
using SiMay.Net.SessionProvider.Providers;
using SiMay.Platform.Windows.Helper;
using SiMay.RemoteService.Loader;
using SiMay.Sockets.Tcp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace SiMay.Service.Core
{
    public class MainApplicationService : MainApplicationServiceBase, IAppMainService
    {
        public MainApplicationService()
        {
            //注册简单服务
            SimpleServiceCollection
                .SimpleServiceRegister<ConfiguartionSimpleService>()
                .SimpleServiceRegister<DesktopViewSimpleService>()
                .SimpleServiceRegister<ExecuteFileUpdateSimpleService>()
                .SimpleServiceRegister<MessageBoxSimpleService>()
                .SimpleServiceRegister<ShellSimpleService>()
                .SimpleServiceRegister<WebSimpleService>()
                .SimpleServiceRegister<WsStatusSimpleService>();
        }

        [PacketHandler(MessageHead.S_MAIN_ACTIVATE_APPLICATION_SERVICE)]
        public void ActivateApplicationService(SessionProviderContext session)
        {
            var activateServiceRequest = session.GetMessageEntity<ActivateServicePack>();
            string applicationKey = activateServiceRequest.CommandText.Split('.').Last<string>();

            //获取当前消息发送源主控端标识
            long accessId = session.GetAccessId();
            var context = SysUtil.RemoteServiceTypes.FirstOrDefault(x => x.RemoteServiceKey.Equals(applicationKey));
            if (!context.IsNull())
            {
                var serviceName = context.RemoteServiceType.GetCustomAttribute<ServiceNameAttribute>(true);
                SystemMessageNotify.ShowTip($"正在进行远程操作:{(serviceName.IsNull() ? context.RemoteServiceKey : serviceName.Name) }");
                var applicationService = Activator.CreateInstance(context.RemoteServiceType, null) as ApplicationRemoteService;
                applicationService.ApplicationKey = context.RemoteServiceKey;
                applicationService.ActivatedCommandText = activateServiceRequest.CommandText;
                applicationService.AccessId = accessId;
                this.PostToAwaitSequence(applicationService);
            }
        }

        /// <summary>
        /// 发送上线包
        /// </summary>
        [PacketHandler(MessageHead.S_GLOBAL_OK)]
        public void SendLoginPack(SessionProviderContext session)
        {
            var loginResult = LoginPacketBuilder();
            session.SendTo(MessageHead.C_MAIN_LOGIN, loginResult);
        }

        private LoginPacket LoginPacketBuilder(bool assemblyLoadCompleted = false)
        {
            string remarkInfomation = AppConfiguartion.RemarkInfomation ?? AppConfiguartion.DefaultRemarkInfo;
            string groupName = AppConfiguartion.GroupName ?? AppConfiguartion.DefaultGroupName;
            bool openScreenView = AppConfiguartion.IsOpenScreenView;//默认为打开屏幕墙

            var loginPack = new LoginPacket();
            loginPack.IPV4 = GetSystemInforHelper.GetLocalIPv4();
            loginPack.MachineName = Environment.MachineName ?? string.Empty;
            loginPack.Remark = remarkInfomation;
            loginPack.ProcessorCount = Environment.ProcessorCount;
            loginPack.ProcessorInfo = GetSystemInforHelper.GetMyCpuInfo;
            loginPack.MemorySize = GetSystemInforHelper.GetMyMemorySize;
            loginPack.StartRunTime = AppConfiguartion.RunTime;
            loginPack.ServiceVison = AppConfiguartion.Version;
            loginPack.UserName = Environment.UserName.ToString();
            loginPack.OSVersion = GetSystemInforHelper.GetOSFullName;
            loginPack.GroupName = groupName;
            loginPack.OpenScreenView = openScreenView;
            loginPack.ExistCameraDevice = GetSystemInforHelper.ExistCameraDevice();
            loginPack.ExitsRecordDevice = GetSystemInforHelper.ExistRecordDevice();
            loginPack.ExitsPlayerDevice = GetSystemInforHelper.ExistPlayDevice();
            loginPack.IdentifyId = AppConfiguartion.IdentifyId;
            loginPack.InitialAssemblyLoad = assemblyLoadCompleted;

            return loginPack;
        }
    }
}