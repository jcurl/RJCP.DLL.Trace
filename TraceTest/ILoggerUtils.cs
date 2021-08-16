namespace RJCP.Diagnostics.Trace
{
    using Microsoft.Extensions.Logging;

    internal static class ILoggerUtils
    {
        internal static ILoggerFactory GetConsoleFactory()
        {
            return LoggerFactory.Create(builder => {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("RJCP", LogLevel.Debug)
                    .AddConsole();
            });
        }
    }
}
