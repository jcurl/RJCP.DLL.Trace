# RJCP.Diagnostics.Trace

This assembly is to assist with logging be extending common operations on top of
`System.Diagnostic`.

Projects using this library allow for easy compatibility between .NET Framework
and .NET Core when porting, and allow easy reconfiguration of the trace sources.

## .NET Framework works with .NET Core

This library is intended for usage in your own .NET Framework and .NET Standard
libraries.

With .NET 2.0 to .NET 4.8, the .NET Framework offered the `TraceListener` and
`TraceSource` which read a configuration file and provided a static singleton to
obtain logging. With the advent of .NET Core, this was replaced with the
`ILoggerFactory` and `ILogger` using dependency injection, making it for library
designers and console applications more complicated. Further, compatibility was
lost with .NET Core that no longer would properly instantiate a `TraceListener`
from the application config.

This library provides a `LogSource` class, that when your library uses, will
receive a `TraceListener`. The `TraceListener` is the lowest common denominator
for both .NET Framework and .NET Core.

Users of .NET Standard can then use the `LogSource.SetLoggerFactory` or
`LogSource.SetLogger` with a category name that the library can use to get the
user desired logging, resulting in common code for both frameworks, reducing
fragmentation.

## Examples of Usage

### .NET Framework and .NET Core Code

Your code should get the `TraceListener` from the `LogSource`.

```csharp
var logger = new LogSource("MyCategory");
logger.TraceEvent(TraceEventType.Information, "Log Message");
```

### .NET Framework Client

There is nothing in particular tha tneeds to be done, other than to define the
trace listener in the `app.config` file.

```xml
<configuration>
  <system.diagnostics>
    <sources>
      <source name="MyCategory" switchValue="Information">
        <listeners>
          <clear/>
          <add name="consoleListener"/>
        </listeners>
      </source>
    </sources>

    <sharedListeners>
      <add name="consoleListener" type="System.Diagnostics.ConsoleTraceListener" />
    </sharedListeners>
  </system.diagnostics>
</configuration>
```

### .NET Core Client

Because .NET Core doesn't read the configuration file, the `TraceListener` will
only ever instantiate the `DefaultTraceListener` and logging won't work. We need
to create an `ILoggerFactory`. The easiest way to do this in a console
application:

```csharp
internal static ILoggerFactory GetConsoleFactory()
{
    return LoggerFactory.Create(builder => {
        builder
            .AddFilter("MyCategory", LogLevel.Debug)
            .AddConsole();
    });
}
```

And then assign it:

```csharp
LogSource.SetLoggerFactory(GetConsoleFactory());
```

#### Reading from a Configuration File

The previous section can be modified slightly to read from a configuration file:

```csharp
internal static ILoggerFactory GetConsoleFactory()
{
    IConfigurationRoot config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true, false)
        .Build();

    return LoggerFactory.Create(builder => {
        builder
            .AddConfiguration(config.GetSection("Logging"))
            .AddConsole();
    });
```

And then assign it:

```csharp
LogSource.SetLoggerFactory(GetConsoleFactory());
```

## Known Issues

### Using ILogger with Unit Tests

I use NUnit for testing, and observed two faults:

* On the start of every test case, the `LogSource.LoggerFactory` must be set
  before running the test case, even if it was set once prior. Debugging shows
  that for some reason NUnit doesn't capture the output for any subsequent test,
  only the first test. See [GitHub Issue #3919](https://github.com/nunit/nunit/issues/3919).
* Depending on your logger that you use, it might not flush immediately. Using
  the `ConsoleLoggingProvider` as in the example above, if the last operation of
  a test case is to write a log, it occurs almost always that the log line will
  not be captured by the test case. Add a slight delay (e.g. 10ms seems to be
  enough most of the time) and the log is captured. The `ConsoleLogger` provided
  by Microsoft queues to an internal memory structure and a separate thread is
  responsible for then writing to the console (which will then be captured by
  NUnit) later in time. I couldn't find a way to cause the console to flush. As
  a workaround, you should provide your own test logger and log factory.
