namespace RJCP.Diagnostics.Trace
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    internal class MemoryLogger : ILogger
    {
        private sealed class SimpleDisposable : IDisposable
        {
            public void Dispose() { /* Nothing to dispose */ }
        }

        public List<string> Logs { get; private set; } = new List<string>();

        public IDisposable BeginScope<TState>(TState state)
        {
            return new SimpleDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Logs.Add(formatter(state, exception));
        }

        public int Count { get { return Logs.Count; } }
    }
}
