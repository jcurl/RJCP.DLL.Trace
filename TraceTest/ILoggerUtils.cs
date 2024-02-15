namespace RJCP.Diagnostics.Trace
{
    using Microsoft.Extensions.Logging;
    using RJCP.CodeQuality.NUnitExtensions.Trace;

    internal static class ILoggerUtils
    {
        private static readonly object s_LoggerFactoryLock = new();
        private static ILoggerFactory s_LoggerFactory;
        private static ILoggerFactory s_LoggerFactory2;

        internal static ILoggerFactory GetLoggerFactory()
        {
            if (s_LoggerFactory is null) {
                lock (s_LoggerFactoryLock) {
                    s_LoggerFactory ??= LoggerFactory.Create(builder => {
                        builder
                            .AddFilter("Microsoft", LogLevel.Warning)
                            .AddFilter("System", LogLevel.Warning)
                            .AddFilter("RJCP", LogLevel.Debug)
                            .AddNUnitLogger();
                    });
                }
            }
            return s_LoggerFactory;
        }

        internal static ILoggerFactory GetLoggerFactory2()
        {
            if (s_LoggerFactory2 is null) {
                lock (s_LoggerFactoryLock) {
                    s_LoggerFactory2 ??= LoggerFactory.Create(builder => {
                        builder
                            .AddFilter("Microsoft", LogLevel.Warning)
                            .AddFilter("System", LogLevel.Warning)
                            .AddFilter("RJCP", LogLevel.Information)
                            .AddNUnitLogger();
                    });
                }
            }
            return s_LoggerFactory;
        }
    }
}
