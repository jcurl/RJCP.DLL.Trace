namespace RJCP.Diagnostics.Trace
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal class MemoryTraceListener : TraceListener
    {
        public class LogEntry
        {
            public TraceEventType EventType { get; set; }

            public string Source { get; set; }

            public int Id { get; set; }

            public DateTime DateTime { get; set; }

            public string Message { get; set; }
        }

        private readonly object m_Lock = new();

        public List<LogEntry> Logs { get; private set; } = new List<LogEntry>();

        public override void Fail(string message, string detailMessage)
        {
            LogEntry entry;
            if (detailMessage is null) {
                entry = new LogEntry() {
                    EventType = TraceEventType.Warning,
                    Source = null,
                    Id = 0,
                    DateTime = DateTime.Now,
                    Message = message
                };
            } else {
                entry = new LogEntry() {
                    EventType = TraceEventType.Warning,
                    Source = null,
                    Id = 0,
                    DateTime = DateTime.Now,
                    Message = string.Format("{0}: {1}", message, detailMessage)
                };
            }
            lock (m_Lock) {
                Logs.Add(entry);
            }
        }

        public override void Write(string message)
        {
            LogEntry entry = new() {
                EventType = TraceEventType.Verbose,
                Source = null,
                Id = 0,
                DateTime = DateTime.Now,
                Message = message
            };
            lock (m_Lock) {
                Logs.Add(entry);
            }
        }

        public override void WriteLine(string message)
        {
            Write(message);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            LogEntry entry = new() {
                EventType = eventType,
                Source = source,
                Id = id,
                DateTime = DateTime.Now,
                Message = string.Empty
            };
            lock (m_Lock) {
                Logs.Add(entry);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            LogEntry entry = new() {
                EventType = eventType,
                Source = source,
                Id = id,
                DateTime = DateTime.Now,
                Message = message
            };
            lock (m_Lock) {
                Logs.Add(entry);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            LogEntry entry = new() {
                EventType = eventType,
                Source = source,
                Id = id,
                DateTime = DateTime.Now,
                Message = string.Format(format, args)
            };
            lock (m_Lock) {
                Logs.Add(entry);
            }
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            LogEntry entry = new() {
                EventType = eventType,
                Source = source,
                Id = id,
                DateTime = DateTime.Now,
                Message = data is null ? string.Empty : data.ToString()
            };
            lock (m_Lock) {
                Logs.Add(entry);
            }
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            LogEntry entry = new() {
                EventType = eventType,
                Source = source,
                Id = id,
                DateTime = DateTime.Now,
                Message = data is null ? string.Empty : data.ToString()
            };
            lock (m_Lock) {
                Logs.Add(entry);
            }
        }

        public override void Flush()
        {
            // Nothing to do
        }
    }
}
