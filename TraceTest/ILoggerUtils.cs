namespace RJCP.Diagnostics.Trace
{
    using Microsoft.Extensions.Logging;
    using RJCP.CodeQuality.NUnitExtensions.Trace;

    internal static class ILoggerUtils
    {
        private static readonly object s_LoggerFactoryLock = new object();
        private static ILoggerFactory s_LoggerFactory;

        internal static ILoggerFactory GetLoggerFactory()
        {
            if (s_LoggerFactory == null) {
                lock (s_LoggerFactoryLock) {
                    if (s_LoggerFactory == null) {
                        s_LoggerFactory = LoggerFactory.Create(builder => {
                            builder
                                .AddFilter("Microsoft", LogLevel.Warning)
                                .AddFilter("System", LogLevel.Warning)
                                .AddFilter("RJCP", LogLevel.Debug)
                                .AddNUnitLogger();
                        });
                    }
                }
            }
            return s_LoggerFactory;
        }
    }
}
