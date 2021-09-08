# ilogger-analyzer

*Analyze ILogger usage at build time and raise warnings.*

> **BEWARE!** This analyzer is currently experimental.

This analyzer parses C# code, looking for `ILogger` calls such as `LogDebug` or `LogWarning`, that use a non-constant string as a message. These are reported as a warning (`ZB1011`). This is a good way to ensure, at build time, that you log efficiently.

For instance, the following code triggers a warning:

```csharp
_logger.LogDebug(GetExpensiveLogMessage());
```

Indeed, the `GetExpensiveLogMessage` method gets invoked each time the line of code is executed, regardless of the log level, which means that in production it is quite probably wasted.

The log message should be a constant string, or anything that evaluates to a constant string at compile time.

Alternatively, the call to the logging method can be made conditional. No warning is raised if:
* The call to the logging method is conditional
* The logger supporting the logging method is provided by an XXXXX FIXME explain exactly what is needed

Therefore, assuming the following extension method is defined:

```csharp
public static ILogger IfDebug(this ILogger logger)
  => logger.IsEnabled(LogLevel.Debug) ? logger : null;
```

Then the following code does *not* raise a warning:

```csharp
_logger.IfDebug()?.LogDebug(GetExpensiveLogMessage());
```

## NOTES

The analyzer still has issues and problems.

The analyzer handles `ILogger` but not `ILogger<T>` and we need to fix this.

The analyzer does not handle 
* `logger.If(LogLevel.Debug).LogDebug(...)`
* `logger.IfDebug().Log(LogLevel.Debug, ...)`
* `logger.If(LogLevel.Debug).Log(LogLevel.Debug, ...)`

A non-conditional call to a logging method such as `logger.IfDebug().LogDebug(...)` would trigger a warning but only if the debug message is not a string constant. If it *is* a string constant, no warning is raised, although this can cause a `NullReferenceException`. We should report (as `ZB1012`) such calls.

Finally, the idea of supporting conditional calls may or may not be a good idea.

Inspiration: [Serilog Analyzer](https://github.com/Suchiman/SerilogAnalyzer) by [Suchiman](https://github.com/Suchiman/) - without depending on Serilog, we could add more warnings...