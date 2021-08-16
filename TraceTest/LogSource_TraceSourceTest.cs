namespace RJCP.Diagnostics.Trace
{
    using System;
    using System.Diagnostics;
    using NUnit.Framework;

    // These tests only run on .NET Framework, that .NET Core doesn't automatically instantiate from the application
    // configuration file.

    [TestFixture]
    public class LogSource_TraceSourceTest
    {
        [Test]
        public void GetLogSource()
        {
            LogSource log = new LogSource("RJCP.TestSource");

            MemoryTraceListener listener = LogSourceTest.CheckLogSource(log);
            int count = listener.Logs.Count;   // A listener via the app.config has only one instance
            log.TraceEvent(TraceEventType.Information, "Message");
            log.TraceEvent(TraceEventType.Information, "Message {0}", 2);

            Assert.That(listener.Logs.Count, Is.EqualTo(count + 2));
        }

        [Test]
        public void GetLogSourceDispose()
        {
            using (LogSource log = new LogSource("RJCP.TestSource")) {
                MemoryTraceListener listener = LogSourceTest.CheckLogSource(log);
                int count = listener.Logs.Count;   // A listener via the app.config has only one instance
                log.TraceEvent(TraceEventType.Information, "Message");
                log.TraceEvent(TraceEventType.Information, "Message {0}", 2);

                Assert.That(listener.Logs.Count, Is.EqualTo(count + 2));
            }
        }

        [Test]
        public void GetLogSourceUseAfterDispose()
        {
            LogSource log = new LogSource("RJCP.TestSource");

            MemoryTraceListener listener = LogSourceTest.CheckLogSource(log);
            int count = listener.Logs.Count;   // A listener via the app.config has only one instance
            log.TraceEvent(TraceEventType.Information, "Message");
            log.TraceEvent(TraceEventType.Information, "Message {0}", 2);

            Assert.That(listener.Logs.Count, Is.EqualTo(count + 2));
            log.Dispose();

            Assert.That(() => {
                log.TraceEvent(TraceEventType.Information, "Log after Dispose");
            }, Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void GetLogSourceNewAfterDispose()
        {
            LogSource log = new LogSource("RJCP.TestSource");

            MemoryTraceListener listener = LogSourceTest.CheckLogSource(log);
            int count = listener.Logs.Count;   // A listener via the app.config has only one instance
            log.TraceEvent(TraceEventType.Information, "Message");
            log.TraceEvent(TraceEventType.Information, "Message {0}", 2);

            Assert.That(listener.Logs.Count, Is.EqualTo(count + 2));
            log.Dispose();

            LogSource log2 = new LogSource("RJCP.TestSource");

            MemoryTraceListener listener2 = LogSourceTest.CheckLogSource(log2);
            int count2 = listener2.Logs.Count;
            log2.TraceEvent(TraceEventType.Information, "log2 message");

            Assert.That(listener2.Logs.Count, Is.EqualTo(count2 + 1));
        }

        [Test]
        public void GetLogSourceNotDefined()
        {
            LogSource log = new LogSource("RJCP.TestSourceNotDefind");
            Assert.That(log, Is.Not.Null);
            Assert.That(log.TraceSource, Is.Not.Null);
            Assert.That(log.TraceSource.Listeners.Count, Is.EqualTo(1));
            Assert.That(log.TraceSource.Listeners[0], Is.TypeOf<DefaultTraceListener>());
            Assert.That(log.TraceSource.Switch.Level, Is.EqualTo(SourceLevels.Off));
        }
    }
}
