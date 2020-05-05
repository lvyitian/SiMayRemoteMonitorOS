using SiMay.Basic;
using SiMay.Net.SessionProvider;
using SiMay.RemoteControlsCore;
//using System.Windows.Form;
using System;

namespace SiMay.RemoteMonitorForWeb
{
    class Program
    {
        private static MainApplicationAdapterHandler _appMainAdapterHandler = new MainApplicationAdapterHandler(null);

        [STAThread]
        static void Main(string[] args)
        {
            _appMainAdapterHandler.ViewRefreshInterval = 1500;
            _appMainAdapterHandler.OnProxyNotifyHandlerEvent += OnProxyNotify;
            _appMainAdapterHandler.OnReceiveHandlerEvent += OnReceiveHandlerEvent;
            _appMainAdapterHandler.OnTransmitHandlerEvent += OnTransmitHandlerEvent;
            _appMainAdapterHandler.OnLogOutHandlerEvent += OnLogOutHandlerEvent;
            _appMainAdapterHandler.OnCreateDesktopViewHandlerEvent += OnCreateDesktopViewHandlerEvent;
            _appMainAdapterHandler.OnLoginHandlerEvent += OnLoginHandlerEvent;
            _appMainAdapterHandler.OnLoginUpdateHandlerEvent += OnLoginUpdateHandlerEvent;
            _appMainAdapterHandler.OnApplicationCreatedEventHandler += OnApplicationCreatedEventHandler;
            _appMainAdapterHandler.OnLogHandlerEvent += OnLogHandlerEvent;
            _appMainAdapterHandler.StartApp();

            while (true)
                Console.ReadLine();
        }

        private static void OnLogOutHandlerEvent(SessionSyncContext obj)
        {
            throw new NotImplementedException();
        }

        private static void OnLoginUpdateHandlerEvent(SessionSyncContext obj)
        {
            throw new NotImplementedException();
        }

        private static void OnLogHandlerEvent(string arg1, LogSeverityLevel arg2)
        {
            throw new NotImplementedException();
        }

        private static void OnApplicationCreatedEventHandler(IApplication obj)
        {
            throw new NotImplementedException();
        }

        private static void OnLoginHandlerEvent(SessionSyncContext syncContext)
        {
            var whetherOpenDesktopView = syncContext.KeyDictions[SysConstants.HasLaunchDesktopRecord].ConvertTo<bool>();
            if (!whetherOpenDesktopView)
                _appMainAdapterHandler.RemoteOpenDesktopView(syncContext);
        }

        private static IDesktopView OnCreateDesktopViewHandlerEvent(SessionSyncContext syncContext)
        {
            throw new NotImplementedException();
        }

        private static void OnTransmitHandlerEvent(SessionProviderContext obj)
        {
            throw new NotImplementedException();
        }

        private static void OnReceiveHandlerEvent(SessionProviderContext obj)
        {
            throw new NotImplementedException();
        }

        private static void OnProxyNotify(ProxyProviderNotify arg1, EventArgs arg2)
        {
            throw new NotImplementedException();
        }
    }
}
