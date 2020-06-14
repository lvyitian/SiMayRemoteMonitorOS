using System;

namespace SiMay.Logger
{
    public abstract class BaseLogger
    {
        public abstract void Log(LogLevel level, string log);
    }
}
