using System;

namespace RomanticWeb.Diagnostics
{
    /// <summary>Exposes a logging facility.</summary>
    public interface ILogger
    {
        /// <summary>Logs a message.</summary>
        /// <param name="level">Log level.</param>
        /// <param name="messageFormat">Message format string.</param>
        /// <param name="arguments">Format string arguments.</param>
        void Log(LogLevel level, string messageFormat, params object[] arguments);

        /// <summary>Logs a message.</summary>
        /// <param name="level">Log level.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="messageFormat">Message format string.</param>
        /// <param name="arguments">Format string arguments.</param>
        void Log(LogLevel level, Exception exception, string messageFormat, params object[] arguments);
    }
}
