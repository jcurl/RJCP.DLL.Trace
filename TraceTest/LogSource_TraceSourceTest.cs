﻿namespace RJCP.Diagnostics.Trace
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
            LogSource log = new("RJCP.TestSource");

            MemoryTraceListener listener = LogSourceTest.CheckLogSource(log);
            int count = listener.Logs.Count;   // A listener via the app.config has only one instance
            log.TraceEvent(TraceEventType.Information, "Message");
            log.TraceEvent(TraceEventType.Information, "Message {0}", 2);

            Assert.That(listener.Logs, Has.Count.EqualTo(count + 2));
        }

        [Test]
        public void GetLogSourceDispose()
        {
            using (LogSource log = new("RJCP.TestSource")) {
                MemoryTraceListener listener = LogSourceTest.CheckLogSource(log);
                int count = listener.Logs.Count;   // A listener via the app.config has only one instance
                log.TraceEvent(TraceEventType.Information, "Message");
                log.TraceEvent(TraceEventType.Information, "Message {0}", 2);

                Assert.That(listener.Logs, Has.Count.EqualTo(count + 2));
            }
        }

        [Test]
        public void GetLogSourceUseAfterDispose()
        {
            LogSource log = new("RJCP.TestSource");

            MemoryTraceListener listener = LogSourceTest.CheckLogSource(log);
            int count = listener.Logs.Count;   // A listener via the app.config has only one instance
            log.TraceEvent(TraceEventType.Information, "Message");
            log.TraceEvent(TraceEventType.Information, "Message {0}", 2);

            Assert.That(listener.Logs, Has.Count.EqualTo(count + 2));
            log.Dispose();

            Assert.That(() => {
                log.TraceEvent(TraceEventType.Information, "Log after Dispose");
            }, Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void GetLogSourceNewAfterDispose()
        {
            LogSource log = new("RJCP.TestSource");

            MemoryTraceListener listener = LogSourceTest.CheckLogSource(log);
            int count = listener.Logs.Count;   // A listener via the app.config has only one instance
            log.TraceEvent(TraceEventType.Information, "Message");
            log.TraceEvent(TraceEventType.Information, "Message {0}", 2);

            Assert.That(listener.Logs, Has.Count.EqualTo(count + 2));
            log.Dispose();

            LogSource log2 = new("RJCP.TestSource");

            MemoryTraceListener listener2 = LogSourceTest.CheckLogSource(log2);
            int count2 = listener2.Logs.Count;
            log2.TraceEvent(TraceEventType.Information, "log2 message");

            Assert.That(listener2.Logs, Has.Count.EqualTo(count2 + 1));
        }

        [Test]
        public void GetLogSourceNotDefined()
        {
            LogSource log = new("RJCP.TestSourceNotDefind");
            Assert.That(log, Is.Not.Null);
            Assert.That(log.TraceSource, Is.Not.Null);
            Assert.That(log.TraceSource.Listeners, Has.Count.EqualTo(1));
            Assert.That(log.TraceSource.Listeners[0], Is.TypeOf<DefaultTraceListener>());
            Assert.That(log.TraceSource.Switch.Level, Is.EqualTo(SourceLevels.Off));
        }
    }
}
