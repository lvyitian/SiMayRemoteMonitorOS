using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Net.SessionProvider
{
    public class LogOutEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public LogOutEventArgs(string message) => Message = message;
    }
}
