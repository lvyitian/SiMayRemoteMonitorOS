using System;
using System.Runtime.InteropServices;

namespace SiMay.Platform.Windows
{
    public static class WTSAPI32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct WTS_SESSION_INFO
        {
            public Int32 SessionID;
            [MarshalAs(UnmanagedType.LPStr)]
            public String pWinStationName;
            public WTS_CONNECTSTATE_CLASS State;
        }
        public enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }
        public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern IntPtr WTSOpenServer(string pServerName);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        public static extern int WTSEnumerateSessions(
            System.IntPtr hServer,
            int Reserved,
            int Version,
            ref System.IntPtr ppSessionInfo,
            ref int pCount);
        public enum WTSInfoClass
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType,
            WTSIdleTime,
            WTSLogonTime,
            WTSIncomingBytes,
            WTSOutgoingBytes,
            WTSIncomingFrames,
            WTSOutgoingFrames,
            WTSClientInfo,
            WTSSessionInfo
        }

        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WTSInfoClass wtsInfoClass, out IntPtr ppBuffer, out uint pBytesReturned);
        [DllImport("Wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pointer);
        public static string GetUserNameBySessionId(int dwSessionId)
        {
            IntPtr buffer;
            uint strLen;
            var username = "System";
            if (WTSQuerySessionInformation(IntPtr.Zero, dwSessionId, WTSInfoClass.WTSUserName, out buffer, out strLen) && strLen > 1)
            {
                username = Marshal.PtrToStringAnsi(buffer);
                WTSFreeMemory(buffer);
            }
            return username;
        }
    }
}
