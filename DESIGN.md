# RJCP.DLL.Trace Design <!-- omit in toc -->

This document describes briefly how the module works, and the structure of the
repository.

- [1. Goal](#1-goal)
- [2. Usage](#2-usage)
- [3. Implementation Details](#3-implementation-details)
  - [3.1. Internal Details](#31-internal-details)
  - [3.2. Instantiating a LogSource object](#32-instantiating-a-logsource-object)
    - [3.2.1. .NET Framework](#321-net-framework)
    - [3.2.2. .NET Core](#322-net-core)
    - [3.2.3. The .NET Core Logger Factory](#323-the-net-core-logger-factory)
    - [3.2.4. Overriding a TraceSource](#324-overriding-a-tracesource)
  - [3.3. Logging with ILogger](#33-logging-with-ilogger)

## 1. Goal

To provide a single class, that behaves like a `TraceSource`, but can also be
used interchangeably with .NET Core's `ILogger`.

Even though .NET Core has a `TraceSource`, it doesn't read the configuration
file associated with an application, so tracing doesn't work. And as existing
applications use the `ILogger` interface, try to reuse existing design where
possible.

Code should be written once, so that it can work on .NET Framework (.NET 4.0 to
4.8) and .NET Core. That means, the lowest common denominator is used, which
provides an interface similar to `TraceSource`, but maps to an `ILogger` in the
background for .NET Core.

## 2. Usage

This is taken from the [README](README.md) file

```csharp
var logger = new LogSource("MyCategory");
logger.TraceEvent(TraceEventType.Information, "Log Message");
```

This will connect to `MyCategory` defind by an XML configuration file for .NET
Framework, or `MyCategory` from a `ILoggerFactory` from .NET Core.

## 3. Implementation Details

### 3.1. Internal Details

The `LogSoruce` is essentially a wrapper around a `TraceSource` object. All
writes to the `LogSource` are translated to the `TraceSource` which contain zero
or more `TraceListener` objects for logging.

### 3.2. Instantiating a LogSource object

#### 3.2.1. .NET Framework

When instantiating a `LogSource(name)` Under .NET Framework, it is essentially a
wrapper for `TraceSource(name)`. The `TraceSource` object is placed in a static
(singleton) dictionary for further lookups by the `LogSource` object. The .NET
Framework does effectively the same thing.

#### 3.2.2. .NET Core

Instantiating under .NET Core causes a `TraceSource` wrapper around an `ILogger`
to be created. The `LogSource.Logger` property is set to be the `ILogger` so
it can be passed to other classes that need an `ILogger` directly.

The `TraceSource` objects listener collection has an internal object
`LoggerTraceListener` added. The `LoggerTraceListener` wraps around the
`ILogger` object. The `SourceSwitch` field `Level` logging level is mapped from
the existing logging levels of the `ILogger`. Any writes to the `TraceSource`
are then passed to the `ILogger`.

#### 3.2.3. The .NET Core Logger Factory

A logger factory, internally the property `LoggerFactory` is set by the method
`SetLoggerFactory` which is available only on .NET Core.

When instantiating the `LogSource` in .NET Core, it uses the `LoggerFactory`
object which the user provides to map a category name to an `ILogger`. If the
`LoggerFactory` is not set, or it returns `null`, a default `TraceSource` with
the category is provided, which may, or may not log, depending on the framework.
At this time, .NET Core and .NET 5.0 do not provide any logging - the
`TraceListener` collection of that object is empty.

The `ILogger` object created is cached into a singleton dictionary. Subsequent
instantiations of the `LogSource` with the same category, will result in the
existing `ILogger` object without the use of the `LoggerFactory`.

#### 3.2.4. Overriding a TraceSource

The method `LogSource.SetLogSource` can be used to provide an instance of a
`TraceSource` object for a specific category name. This is useful if an
application wishes to override a trace source from the XML configuration, or to
provide a custom `TraceSource` for .NET Core without requiring the factory
`LoggerFactory`.

Under .NET Core, the method `LogSource.SetLogSource(string name, ILogger
logger)` can be used to assign a specific `ILogger` without defining the
`LoggerFactory`. This will be used instead.

### 3.3. Logging with ILogger

the internal `LoggerTraceListener` takes each line and splits it into multiple
lines before sending to the `ILogger`.
