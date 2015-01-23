using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyBot
{
    public static class ExceptionHelper
    {
        public static string GetExceptionText(Exception e, bool includeStackTrace = false, bool includeTypes = true)
        {
            StringBuilder sb = new StringBuilder();
            //first available stack trace
            string stackTrace = null;
            while (e != null) {
                sb.AppendLine((includeTypes ? e.GetType().FullName + ": " : "") + e.Message);
                if (includeStackTrace && string.IsNullOrEmpty(stackTrace)) {
                    stackTrace = e.StackTrace;
                }
                e = e.InnerException;
            }
            if (includeStackTrace && stackTrace != null) {
                sb.AppendLine(stackTrace);
            }
            return sb.ToString();
        }
    }
}
