namespace RJCP.Diagnostics.Trace
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;

    // These test cases test explicitly the usage with ILogger, and so are .NET Core relevant only.

    [TestFixture]
    public class LogSource_ILoggerTest
    {
        [SetUp]
        public void TestSetup()
        {
            // We need to clear the factory before each test case, as it's static, and would otherwise cause sideeffects
            // between tests if they don't set it.
            LogSource.ClearLoggerFactory();
            LogSource.ClearLogSource();
        }

        [Test]
        public void SetLogSource_LoggerFactory()
        {
            Assert.That(LogSource.SetLoggerFactory(ILoggerUtils.GetLoggerFactory()), Is.False);

            LogSource log = new LogSource("RJCP.ILoggerTest1");
            log.TraceEvent(TraceEventType.Information, "Log Message");

            Assert.That(log.Logger, Is.Not.Null);
            Thread.Sleep(10);  // A log.Logger can't flush, and a ConsoleLogger needs time to write.
        }

        [Test]
        public void SetLogSource_LoggerFactory_GetTwice()
        {
            Assert.That(LogSource.SetLoggerFactory(ILoggerUtils.GetLoggerFactory()), Is.False);

            LogSource log = new LogSource("RJCP.ILoggerTest1");
            log.TraceEvent(TraceEventType.Information, "Log Message");
            Assert.That(log.Logger, Is.Not.Null);

            LogSource log2 = new LogSource("RJCP.ILoggerTest1");
            log2.TraceEvent(TraceEventType.Information, "Log Message 2");
            Assert.That(log2.Logger, Is.Not.Null);
        }

        [Test]
        public void SetLogSource_LoggerFactory_SetTwice()
        {
            Assert.That(LogSource.SetLoggerFactory(ILoggerUtils.GetLoggerFactory()), Is.False);
            Assert.That(LogSource.SetLoggerFactory(ILoggerUtils.GetLoggerFactory2()), Is.True);

            LogSource log = new LogSource("RJCP.ILoggerTest1");
            log.TraceEvent(TraceEventType.Verbose, "Log Message");

            Assert.That(log.Logger, Is.Not.Null);
            Thread.Sleep(10);  // A log.Logger can't flush, and a ConsoleLogger needs time to write.
        }

        [Test]
        public void SetLogSource_LoggerFactoryNull()
        {
            Assert.That(() => {
                LogSource.SetLoggerFactory(null);
            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void SetLogSource_Logger()
        {
            LogSource.SetLogSource("RJCP.ILoggerSetTest", new MemoryLogger());

            LogSource log = new LogSource("RJCP.ILoggerSetTest");
            Assert.That(log.Logger, Is.Not.Null);

            int count = ((MemoryLogger)log.Logger).Count;
            log.TraceEvent(TraceEventType.Information, "Log Message");
            Assert.That(log.Logger, Is.Not.Null);
            Assert.That(((MemoryLogger)log.Logger).Count, Is.EqualTo(count + 1));
        }

        [Test]
        public void GetLogSource_DefaultListener()
        {
            LogSource log = new LogSource("RJCP.DefaultLogger");
            Assert.That(log, Is.Not.Null);
            Assert.That(log.Logger, Is.Null);
            Assert.That(log.TraceSource, Is.Not.Null);
            Assert.That(log.TraceSource.Listeners, Has.Count.EqualTo(1));
            Assert.That(log.TraceSource.Listeners[0], Is.TypeOf<DefaultTraceListener>());
        }

        [Test]
        public void GetLogSource()
        {
            Assert.That(LogSource.SetLoggerFactory(new MemoryLoggerFactory()), Is.False);
            LogSource log = new LogSource("RJCP.MemoryLogger");
            Assert.That(log.Logger, Is.Not.Null);

            int count = ((MemoryLogger)log.Logger).Count;
            log.TraceEvent(TraceEventType.Information, "Message");
            log.TraceEvent(TraceEventType.Information, "Message {0}", 2);

            Assert.That(((MemoryLogger)log.Logger).Count, Is.EqualTo(count + 2));
        }

        [Test]
        public void GetLogSourceDispose()
        {
            Assert.That(LogSource.SetLoggerFactory(new MemoryLoggerFactory()), Is.False);
            using (LogSource log = new LogSource("RJCP.MemoryLogger")) {
                Assert.That(log.Logger, Is.Not.Null);

                int count = ((MemoryLogger)log.Logger).Count;
                log.TraceEvent(TraceEventType.Information, "Message");
                log.TraceEvent(TraceEventType.Information, "Message {0}", 2);

                Assert.That(((MemoryLogger)log.Logger).Count, Is.EqualTo(count + 2));
            }
        }

        [Test]
        public void GetLogSourceUseAfterDispose()
        {
            Assert.That(LogSource.SetLoggerFactory(new MemoryLoggerFactory()), Is.False);
            LogSource log = new LogSource("RJCP.MemoryLogger");
            Assert.That(log.Logger, Is.Not.Null);

            int count = ((MemoryLogger)log.Logger).Count;
            log.TraceEvent(TraceEventType.Information, "Message");
            log.TraceEvent(TraceEventType.Information, "Message {0}", 2);

            Assert.That(((MemoryLogger)log.Logger).Count, Is.EqualTo(count + 2));
            log.Dispose();

            Assert.That(() => {
                log.TraceEvent(TraceEventType.Information, "Log after Dispose");
            }, Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void GetLogSourceDependencyInjection()
        {
            ILogger logger = new MemoryLogger();
            LogSource log = new LogSource("RJCP.MemoryLoggerDI", logger);
            Assert.That(log.Logger, Is.Not.Null);

            Assert.That(((MemoryLogger)log.Logger).Count, Is.EqualTo(0));
            log.TraceEvent(TraceEventType.Information, "Message");
            log.TraceEvent(TraceEventType.Information, "Message {0}", 2);

            Assert.That(((MemoryLogger)log.Logger).Count, Is.EqualTo(2));
            log.Dispose();

            Assert.That(() => {
                log.TraceEvent(TraceEventType.Information, "Log after Dispose");
            }, Throws.TypeOf<ObjectDisposedException>());
        }
    }
}
