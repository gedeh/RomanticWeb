using System;
using System.Diagnostics;

namespace RomanticWeb.Diagnostics
{
    /// <summary>Exposes a simple logging facility.</summary>
    public class SimpleLogger : ILogger
    {
        /// <inheritdoc />
        public void Log(LogLevel level, string messageFormat, params object[] arguments)
        {
            switch (level)
            {
                case LogLevel.Fatal:
                case LogLevel.Error:
                    Trace.TraceError(messageFormat, arguments);
                    break;
                case LogLevel.Warning:
                    Trace.TraceWarning(messageFormat, arguments);
                    break;
                case LogLevel.Info:
                    Trace.TraceInformation(messageFormat, arguments);
                    break;
                case LogLevel.Debug:
                case LogLevel.Trace:
                    Trace.WriteLine(String.Format(messageFormat, arguments), level.ToString());
                    break;
            }
        }

        /// <inheritdoc />
        public void Log(LogLevel level, Exception exception, string messageFormat, params object[] arguments)
        {
            Log(level, messageFormat, arguments);
        }
    }
}
