using System;

namespace RomanticWeb.Diagnostics
{
    /// <summary>Provides useful <see cref="ILogger" /> extensions.</summary>
    public static class LoggerExtensions
    {
        /// <summary>Logs a fatal error.</summary>
        /// <param name="logger">Target logger.</param>
        /// <param name="message">Message format string.</param>
        /// <param name="arguments">Message format string arguments.</param>
        public static void Fatal(this ILogger logger, string message, params object[] arguments)
        {
            if (logger == null)
            {
                return;
            }

            logger.Log(LogLevel.Fatal, message, arguments);
        }

        /// <summary>Logs a fatal error with exception.</summary>
        /// <param name="logger">Target logger.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Message format string.</param>
        /// <param name="arguments">Message format string arguments.</param>
        public static void Fatal(this ILogger logger, Exception exception, string message, params object[] arguments)
        {
            if (logger == null)
            {
                return;
            }

            logger.Log(LogLevel.Fatal, exception, message, arguments);
        }

        /// <summary>Logs an error.</summary>
        /// <param name="logger">Target logger.</param>
        /// <param name="message">Message format string.</param>
        /// <param name="arguments">Message format string arguments.</param>
        public static void Error(this ILogger logger, string message, params object[] arguments)
        {
            if (logger == null)
            {
                return;
            }

            logger.Log(LogLevel.Error, message, arguments);
        }

        /// <summary>Logs an error with exception.</summary>
        /// <param name="logger">Target logger.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Message format string.</param>
        /// <param name="arguments">Message format string arguments.</param>
        public static void Error(this ILogger logger, Exception exception, string message, params object[] arguments)
        {
            if (logger == null)
            {
                return;
            }

            logger.Log(LogLevel.Error, exception, message, arguments);
        }

        /// <summary>Logs a warning.</summary>
        /// <param name="logger">Target logger.</param>
        /// <param name="message">Message format string.</param>
        /// <param name="arguments">Message format string arguments.</param>
        public static void Warning(this ILogger logger, string message, params object[] arguments)
        {
            if (logger == null)
            {
                return;
            }

            logger.Log(LogLevel.Warning, message, arguments);
        }

        /// <summary>Logs an information with exception.</summary>
        /// <param name="logger">Target logger.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Message format string.</param>
        /// <param name="arguments">Message format string arguments.</param>
        public static void Warning(this ILogger logger, Exception exception, string message, params object[] arguments)
        {
            if (logger == null)
            {
                return;
            }

            logger.Log(LogLevel.Warning, exception, message, arguments);
        }

        /// <summary>Logs a warning.</summary>
        /// <param name="logger">Target logger.</param>
        /// <param name="message">Message format string.</param>
        /// <param name="arguments">Message format string arguments.</param>
        public static void Info(this ILogger logger, string message, params object[] arguments)
        {
            if (logger == null)
            {
                return;
            }

            logger.Log(LogLevel.Info, message, arguments);
        }

        /// <summary>Logs an information with exception.</summary>
        /// <param name="logger">Target logger.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Message format string.</param>
        /// <param name="arguments">Message format string arguments.</param>
        public static void Info(this ILogger logger, Exception exception, string message, params object[] arguments)
        {
            if (logger == null)
            {
                return;
            }

            logger.Log(LogLevel.Info, exception, message, arguments);
        }

        /// <summary>A trace log.</summary>
        /// <param name="logger">Target logger.</param>
        /// <param name="message">Message format string.</param>
        /// <param name="arguments">Message format string arguments.</param>
        public static void Trace(this ILogger logger, string message, params object[] arguments)
        {
            if (logger == null)
            {
                return;
            }

            logger.Log(LogLevel.Trace, message, arguments);
        }

        /// <summary>Traces with exception.</summary>
        /// <param name="logger">Target logger.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Message format string.</param>
        /// <param name="arguments">Message format string arguments.</param>
        public static void Trace(this ILogger logger, Exception exception, string message, params object[] arguments)
        {
            if (logger == null)
            {
                return;
            }

            logger.Log(LogLevel.Trace, exception, message, arguments);
        }

        /// <summary>A debug log.</summary>
        /// <param name="logger">Target logger.</param>
        /// <param name="message">Message format string.</param>
        /// <param name="arguments">Message format string arguments.</param>
        public static void Debug(this ILogger logger, string message, params object[] arguments)
        {
            if (logger == null)
            {
                return;
            }

            logger.Log(LogLevel.Debug, message, arguments);
        }

        /// <summary>Debug with exception.</summary>
        /// <param name="logger">Target logger.</param>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="message">Message format string.</param>
        /// <param name="arguments">Message format string arguments.</param>
        public static void Debug(this ILogger logger, Exception exception, string message, params object[] arguments)
        {
            if (logger == null)
            {
                return;
            }

            logger.Log(LogLevel.Debug, exception, message, arguments);
        }
    }
}
