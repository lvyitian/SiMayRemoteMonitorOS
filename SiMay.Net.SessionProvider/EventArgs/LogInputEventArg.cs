using System;
using System.Collections.Generic;
using System.Text;

namespace SiMay.Net.SessionProvider
{
    public class LogInputEventArg : EventArgs
    {
        public string Message { get; private set; }
        public LogInputEventArg(string message) => Message = message;
    }
}
