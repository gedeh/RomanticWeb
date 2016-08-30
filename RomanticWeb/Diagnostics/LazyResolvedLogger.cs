using System;

namespace RomanticWeb.Diagnostics
{
    /// <summary>Provides a lazily resolved logger.</summary>
    public class LazyResolvedLogger : ILogger
    {
        private Func<ILogger> _loggerFactoryMethod;
        private ILogger _log;

        /// <summary>Initializes a new instance of the <see cref="LazyResolvedLogger" /> class.</summary>
        /// <param name="loggerFactoryMethod">Logging facility factory method.</param>
        public LazyResolvedLogger(Func<ILogger> loggerFactoryMethod)
        {
            _loggerFactoryMethod = loggerFactoryMethod;
        }

        /// <inheritdoc />
        public void Log(LogLevel level, string messageFormat, params object[] arguments)
        {
            (_log ?? (_log = _loggerFactoryMethod())).Log(level, messageFormat, arguments);
        }

        /// <inheritdoc />
        public void Log(LogLevel level, Exception exception, string messageFormat, params object[] arguments)
        {
            (_log ?? (_log = _loggerFactoryMethod())).Log(level, messageFormat, arguments);
        }
    }
}
