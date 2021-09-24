namespace RJCP.Diagnostics.Trace
{
    using System.Diagnostics;
    using Internal;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A <see cref="TraceListener"/> wrapper for a .NET Core <see cref="ILogger"/>.
    /// </summary>
    /// <remarks>
    /// This class is not intended to be instantiated within a <c>app.config</c> file. It is provided so that a user of
    /// .NET Core can provide an <see cref="ILogger"/> object, and the code in this library can continue to use the
    /// <see cref="TraceSource"/> classes for logging.
    /// </remarks>
    internal class LoggerTraceListener : TraceListener
    {
        private readonly ILogger m_Logger;
        private readonly LineSplitter m_Lines = new LineSplitter();

        public LoggerTraceListener(string name, ILogger logger) : base(name)
        {
            m_Logger = logger;
        }

        public ILogger Logger { get { return m_Logger; } }

        public override void Fail(string message, string detailMessage)
        {
            if (m_Lines.IsCached) m_Lines.NewLine();

            string logMessage;
            if (detailMessage == null) {
                logMessage = message;
            } else {
                logMessage = string.Format("{0}: {1}", message, detailMessage);
            }
            m_Lines.AppendLine(logMessage);

            foreach (string line in m_Lines) {
                m_Logger.LogInformation(line);
            }
        }

        public override void Write(string message)
        {
            m_Lines.Append(message);
            foreach (string line in m_Lines) {
                m_Logger.LogInformation(line);
            }
        }

        public override void WriteLine(string message)
        {
            m_Lines.AppendLine(message);
            foreach (string line in m_Lines) {
                m_Logger.LogInformation(line);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            TraceEvent(eventCache, source, eventType, id, string.Empty);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            string message = string.Format(format, args);
            TraceEvent(eventCache, source, eventType, id, message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (m_Lines.IsCached) m_Lines.NewLine();
            m_Lines.AppendLine(message);

            switch (eventType) {
            case TraceEventType.Critical:
                foreach (string line in m_Lines) {
                    m_Logger.LogCritical(id, line);
                }
                break;
            case TraceEventType.Error:
                foreach (string line in m_Lines) {
                    m_Logger.LogError(id, line);
                }
                break;
            case TraceEventType.Warning:
                foreach (string line in m_Lines) {
                    m_Logger.LogWarning(id, line);
                }
                break;
            case TraceEventType.Information:
                foreach (string line in m_Lines) {
                    m_Logger.LogInformation(id, line);
                }
                break;
            case TraceEventType.Verbose:
                foreach (string line in m_Lines) {
                    m_Logger.LogDebug(id, line);
                }
                break;
            default:
                foreach (string line in m_Lines) {
                    m_Logger.LogTrace(id, line);
                }
                break;
            }
        }

        public override void Flush()
        {
            if (m_Lines.IsCached) m_Lines.NewLine();
            foreach (string line in m_Lines) {
                m_Logger.LogInformation(line);
            }
        }
    }
}
