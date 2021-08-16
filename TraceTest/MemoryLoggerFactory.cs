namespace RJCP.Diagnostics.Trace
{
    using Microsoft.Extensions.Logging;

    internal class MemoryLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
            // There is no provider, as this is a specialized logging interface for the .NET Core logging in this
            // specific unit test environment.
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new MemoryLogger();
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
