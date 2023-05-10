namespace RJCP.Diagnostics.Trace
{
    using System;
    using System.Diagnostics;
    using NUnit.Framework;

    [TestFixture]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", "NUnit1028:The non-test method is public", Justification = "False positive for internal")]
    public class LogSourceTest
    {
        internal static MemoryTraceListener CheckLogSource(LogSource log)
        {
            return CheckLogSource(log, SourceLevels.Information);
        }

        internal static MemoryTraceListener CheckLogSource(LogSource log, SourceLevels expectedLevel)
        {
            Assert.That(log, Is.Not.Null);
            Assert.That(log.TraceSource, Is.Not.Null);
            Assert.That(log.TraceSource.Listeners, Has.Count.EqualTo(1));
            Assert.That(log.TraceSource.Listeners[0], Is.TypeOf<MemoryTraceListener>());
            Assert.That(log.TraceSource.Switch.Level, Is.EqualTo(expectedLevel));
            return (MemoryTraceListener)log.TraceSource.Listeners[0];
        }

        [Test]
        public void GetLogSourceNull()
        {
            Assert.That(() => {
                _ = new LogSource(null);
            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetLogSourceEmptyName()
        {
            Assert.That(() => {
                _ = new LogSource(string.Empty);
            }, Throws.TypeOf<ArgumentException>());
        }

        [TestCase(SourceLevels.Information)]
        [TestCase(SourceLevels.Warning)]
        public void SetLogSource_TraceSource(SourceLevels level)
        {
            TraceSource traceSource = new TraceSource("RJCP.TestTraceSource") {
                Switch = new SourceSwitch("RJCP.TestTraceSource", level.ToString())
            };
            traceSource.Listeners.Clear();
            traceSource.Listeners.Add(new MemoryTraceListener());
            LogSource.SetLogSource(traceSource);

            LogSource log = new LogSource("RJCP.TestTraceSource");
            MemoryTraceListener listener = CheckLogSource(log, level);

            Assert.That(log.ShouldTrace(TraceEventType.Error), Is.EqualTo(level >= SourceLevels.Error));
            Assert.That(log.ShouldTrace(TraceEventType.Warning), Is.EqualTo(level >= SourceLevels.Warning));
            Assert.That(log.ShouldTrace(TraceEventType.Information), Is.EqualTo(level >= SourceLevels.Information));
            Assert.That(log.ShouldTrace(TraceEventType.Verbose), Is.EqualTo(level >= SourceLevels.Verbose));

            int count = listener.Logs.Count;   // A listener via the app.config has only one instance
            log.TraceEvent(TraceEventType.Information, "Message");
            log.TraceEvent(TraceEventType.Information, "Message {0}", 2);

            if (level >= SourceLevels.Information)
                Assert.That(listener.Logs, Has.Count.EqualTo(count + 2));
            else
                Assert.That(listener.Logs, Has.Count.EqualTo(count));
        }

        [TestCase(SourceLevels.Information)]
        [TestCase(SourceLevels.Warning)]
        public void SetLogSource_TraceListener(SourceLevels level)
        {
            LogSource.SetLogSource("RJCP.TestTraceListener", level, new MemoryTraceListener());

            LogSource log = new LogSource("RJCP.TestTraceListener");
            MemoryTraceListener listener = CheckLogSource(log, level);

            Assert.That(log.ShouldTrace(TraceEventType.Error), Is.EqualTo(level >= SourceLevels.Error));
            Assert.That(log.ShouldTrace(TraceEventType.Warning), Is.EqualTo(level >= SourceLevels.Warning));
            Assert.That(log.ShouldTrace(TraceEventType.Information), Is.EqualTo(level >= SourceLevels.Information));
            Assert.That(log.ShouldTrace(TraceEventType.Verbose), Is.EqualTo(level >= SourceLevels.Verbose));

            int count = listener.Logs.Count;   // A listener via the app.config has only one instance
            log.TraceEvent(TraceEventType.Information, "Message");
            log.TraceEvent(TraceEventType.Information, "Message {0}", 2);

            if (level >= SourceLevels.Information)
                Assert.That(listener.Logs, Has.Count.EqualTo(count + 2));
            else
                Assert.That(listener.Logs, Has.Count.EqualTo(count));
        }

        [Test]
        public void SetLogSource_NullTraceSource()
        {
            Assert.That(() => {
                LogSource.SetLogSource(null);
            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void SetLogSource_NullName()
        {
            Assert.That(() => {
                LogSource.SetLogSource(null, SourceLevels.Information, new MemoryTraceListener());
            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void SetLogSource_EmptyName()
        {
            Assert.That(() => {
                LogSource.SetLogSource(string.Empty, SourceLevels.Information, new MemoryTraceListener());
            }, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void SetLogSource_NullListener()
        {
            Assert.That(() => {
                LogSource.SetLogSource("RJCP.NullListener", SourceLevels.Information, null);
            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void LogSourceDefault()
        {
            LogSource log = new LogSource();
            Assert.That(log.TraceSource, Is.Not.Null);
            Assert.That(log.TraceSource.Listeners, Is.Empty);
            Assert.That(log.TraceSource.Switch.Level, Is.EqualTo(SourceLevels.Off));
        }
    }
}
