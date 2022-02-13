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

There is nothing in particular that needs to be done, other than to define the
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

### Using ILogger with NUnit Tests

Using the .NET Core `ConsoleLogger` will not work in NUnit Test cases. Use my
`RJCP.DLL.CodeQuality` library to get a `NUnitLogger`.

The `ConsoleLogger` doesn't work in NUnit, as it creates a thread in the
background that does the logging. This thread maintains a handle to the console,
and is incompatible with the way that NUnit also tries to capture th console and
redirect itself. That's why the workaround for console logging was to completely
instantiate the factory on every test case, which is inefficient, but would
reset the background thread allowing NUnit to capture the console again at the
start of each test. But it still didn't solve the race conditions that might
sometimes occur due to console logging actually being done in another thread,
and so some console output might actually be captured for a different test case.

See [GitHub Issue #3919](https://github.com/nunit/nunit/issues/3919).
