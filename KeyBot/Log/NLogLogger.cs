using NLog;
using System;

namespace KeyBot.Log
{
    internal class NLogLogger: ILogger
    {
        protected Logger Logger;
        public NLogLogger(Logger logger)
        {
            Logger = logger;
        }

        public void Log(LogLevel level, string message)
        {
            Logger.Log(GetNLogLogLevel(level), message);
        }

        public void Log(LogLevel level, Exception ex)
        {
            Log(level, ExceptionHelper.GetExceptionText(ex, true));
        }

        protected NLog.LogLevel GetNLogLogLevel(LogLevel l)
        {
            switch (l) {
                case LogLevel.Info:
                    return NLog.LogLevel.Info;
                case LogLevel.Warning:
                    return NLog.LogLevel.Warn;
                case LogLevel.Error:
                    return NLog.LogLevel.Error;
                default:
                    return NLog.LogLevel.Debug;
            }
        }
    }
}
