using System;

namespace KeyBot.Log
{
    internal interface ILogger
    {
        void Log(LogLevel level, string message);
        void Log(LogLevel level, Exception e);
    }
}
