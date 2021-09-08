// Copyright (c) 2021, ZpqrtBnk. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = ZpqrtBnk.ILoggerAnalyzer.Test.CSharpCodeFixVerifier<
    ZpqrtBnk.ILoggerAnalyzer.Analyzer,
    ZpqrtBnk.ILoggerAnalyzer.ZpqrtBnkILoggerAnalyzerCodeFixProvider>;

namespace ZpqrtBnk.ILoggerAnalyzer.Tests
{
    [TestClass]
    public class AnalyzerTests
    {
        // the minimal code for ILogger, copied from MS, with all comments stripped
        private const string LoggerCode = @"
public interface ILogger
{
    void Log<TState>(
      LogLevel logLevel,
      EventId eventId,
      TState state,
      Exception exception,
      Func<TState, Exception, string> formatter);

    bool IsEnabled(LogLevel logLevel);

    IDisposable BeginScope<TState>(TState state);
}

public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical,
    None,
}

public readonly struct EventId
{
    public static implicit operator EventId(int i)
    {
        return new EventId();
    }
}

public sealed class FormattedLogValues
{
    public FormattedLogValues(string message, object[] args)
    { }
}
";

        // the code for LoggerExtensions, copied from MS, with all comments stripped
        private const string LoggerExtensionsCode = @"
public static class LoggerExtensions
{
    private static readonly Func<FormattedLogValues, Exception, string> _messageFormatter = MessageFormatter;

    public static void LogDebug(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args)
    {
        logger.Log(LogLevel.Debug, eventId, exception, message, args);
    }

    public static void LogDebug(this ILogger logger, EventId eventId, string message, params object[] args)
    {
        logger.Log(LogLevel.Debug, eventId, message, args);
    }

    public static void LogDebug(this ILogger logger, Exception exception, string message, params object[] args)
    {
        logger.Log(LogLevel.Debug, exception, message, args);
    }

    public static void LogDebug(this ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Debug, message, args);
    }

    public static void LogTrace(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args)
    {
        logger.Log(LogLevel.Trace, eventId, exception, message, args);
    }

    public static void LogTrace(this ILogger logger, EventId eventId, string message, params object[] args)
    {
        logger.Log(LogLevel.Trace, eventId, message, args);
    }

    public static void LogTrace(this ILogger logger, Exception exception, string message, params object[] args)
    {
        logger.Log(LogLevel.Trace, exception, message, args);
    }

    public static void LogTrace(this ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Trace, message, args);
    }

    public static void LogInformation(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args)
    {
        logger.Log(LogLevel.Information, eventId, exception, message, args);
    }

    public static void LogInformation(this ILogger logger, EventId eventId, string message, params object[] args)
    {
        logger.Log(LogLevel.Information, eventId, message, args);
    }

    public static void LogInformation(this ILogger logger, Exception exception, string message, params object[] args)
    {
        logger.Log(LogLevel.Information, exception, message, args);
    }

    public static void LogInformation(this ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Information, message, args);
    }

    public static void LogWarning(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args)
    {
        logger.Log(LogLevel.Warning, eventId, exception, message, args);
    }

    public static void LogWarning(this ILogger logger, EventId eventId, string message, params object[] args)
    {
        logger.Log(LogLevel.Warning, eventId, message, args);
    }

    public static void LogWarning(this ILogger logger, Exception exception, string message, params object[] args)
    {
        logger.Log(LogLevel.Warning, exception, message, args);
    }

    public static void LogWarning(this ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Warning, message, args);
    }

    public static void LogError(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args)
    {
        logger.Log(LogLevel.Error, eventId, exception, message, args);
    }

    public static void LogError(this ILogger logger, EventId eventId, string message, params object[] args)
    {
        logger.Log(LogLevel.Error, eventId, message, args);
    }

    public static void LogError(this ILogger logger, Exception exception, string message, params object[] args)
    {
        logger.Log(LogLevel.Error, exception, message, args);
    }

    public static void LogError(this ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Error, message, args);
    }

    public static void LogCritical(this ILogger logger, EventId eventId, Exception exception, string message, params object[] args)
    {
        logger.Log(LogLevel.Critical, eventId, exception, message, args);
    }

    public static void LogCritical(this ILogger logger, EventId eventId, string message, params object[] args)
    {
        logger.Log(LogLevel.Critical, eventId, message, args);
    }

    public static void LogCritical(this ILogger logger, Exception exception, string message, params object[] args)
    {
        logger.Log(LogLevel.Critical, exception, message, args);
    }

    public static void LogCritical(this ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Critical, message, args);
    }

    public static void Log(this ILogger logger, LogLevel logLevel, string message, params object[] args)
    {
        logger.Log(logLevel, 0, null, message, args);
    }

    public static void Log(this ILogger logger, LogLevel logLevel, EventId eventId, string message, params object[] args)
    {
        logger.Log(logLevel, eventId, null, message, args);
    }

    public static void Log(this ILogger logger, LogLevel logLevel, Exception exception, string message, params object[] args)
    {
        logger.Log(logLevel, 0, exception, message, args);
    }

    public static void Log(this ILogger logger, LogLevel logLevel, EventId eventId, Exception exception, string message, params object[] args)
    {
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        logger.Log(logLevel, eventId, new FormattedLogValues(message, args), exception, _messageFormatter);
    }

    public static IDisposable BeginScope(
        this ILogger logger,
        string messageFormat,
        params object[] args)
    {
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        return logger.BeginScope(new FormattedLogValues(messageFormat, args));
    }

    private static string MessageFormatter(FormattedLogValues state, Exception error)
    {
        return state.ToString();
    }
}
";

        // the code for LoggerMaybeExtensions, that one may want to drop in their project
        private const string MaybeLoggerCode = @"
public static class LoggerMaybeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ILogger If(this ILogger logger, LogLevel level) => logger.IsEnabled(level) ? logger : null;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ILogger IfTrace(this ILogger logger) => logger.If(LogLevel.Trace);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ILogger IfDebug(this ILogger logger) => logger.If(LogLevel.Debug);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ILogger IfInformation(this ILogger logger) => logger.If(LogLevel.Information);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ILogger IfWarning(this ILogger logger) => logger.If(LogLevel.Warning);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ILogger IfError(this ILogger logger) => logger.If(LogLevel.Error);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ILogger IfCritical(this ILogger logger) => logger.If(LogLevel.Critical);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ILogger IfNone(this ILogger logger) => logger.If(LogLevel.None);
}
";

        //
        public string TestCode(string code) => @"
using System;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices; // for [MethodImpl(...)]

" + code + @"

#pragma warning disable ZB1011
namespace Microsoft.Extensions.Logging
{
" + LoggerCode + LoggerExtensionsCode + @"
" + MaybeLoggerCode + @"
}
#pragma warning restore ZB1011
";

        //
        private static LinePosition DiagnosticPosition(string code, int index = -1)
        {
            var x = code.Split('\n');
            var line = 0;
            var character = 0;
            var mark = index < 0 ? "‼" : $"‼{index}";
            while (line < x.Length && (character = x[line].IndexOf(mark, StringComparison.InvariantCulture)) < 0) line++;
            if (line == x.Length || character < 0) return new LinePosition();
            return new LinePosition(line + 1, character);
        }

        [TestMethod]
        public async Task AnalyzeConstant()
        {

            var code = TestCode(@"
namespace Tests
{
  public class Test
  {
    public void Method(ILogger logger)
    {
      logger.LogDebug(""hello"");
    }
  }
}
");

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [TestMethod]
        public async Task AnalyzeStringEmpty()
        {

            var code = TestCode(@"
namespace Tests
{
  public class Test
  {
    public void Method(ILogger logger)
    {
      logger.LogDebug(string.Empty);
    }
  }
}
");

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [TestMethod]
        public async Task AnalyzeComputableConstant()
        {

            var code = TestCode(@"
namespace Tests
{
  public class Test
  {
    private const string World = ""world"";
    public void Method(ILogger logger)
    {
      logger.LogDebug(""hello"" + "" "" + World);
    }
  }
}
");

            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [TestMethod]
        public async Task AnalyzeMethodCall()
        {

            var code = TestCode(@"
namespace Tests
{
  public class Test
  {
    private string World() => ""world"";
    public void Method(ILogger logger)
    {
      //              ‼
      logger.LogDebug(""hello"" + "" "" + World());
    }
  }
}
");

            var expected1 = new DiagnosticResult("ZB1011", DiagnosticSeverity.Warning)
                .WithMessage("Non-constant log message.")
                .WithLocation(DiagnosticPosition(code));

            await VerifyCS.VerifyAnalyzerAsync(code, expected1);
        }

        [TestMethod]
        public async Task AnalyzeProperty()
        {

            var code = TestCode(@"
namespace Tests
{
  public class Test
  {
    private string World => ""world"";
    public void Method(ILogger logger)
    {
      //              ‼
      logger.LogDebug(""hello"" + "" "" + World);
    }
  }
}
");

            var expected1 = new DiagnosticResult("ZB1011", DiagnosticSeverity.Warning)
                .WithMessage("Non-constant log message.")
                .WithLocation(DiagnosticPosition(code));

            await VerifyCS.VerifyAnalyzerAsync(code, expected1);
        }

        [TestMethod]
        public async Task AnalyzeInterpolatedString1()
        {

            var code = TestCode(@"
namespace Tests
{
  public class Test
  {
    public void Method(ILogger logger)
    {
      //              ‼
      logger.LogDebug($""hello"");
    }
  }
}
");

            var expected1 = new DiagnosticResult("ZB1011", DiagnosticSeverity.Warning)
                .WithMessage("Non-constant log message.")
                .WithLocation(DiagnosticPosition(code));

            await VerifyCS.VerifyAnalyzerAsync(code, expected1);
        }

        [TestMethod]
        public async Task AnalyzeInterpolatedString2()
        {

            var code = TestCode(@"
namespace Tests
{
  public class Test
  {
    public void Method(ILogger logger)
    {
      //              ‼
      logger.LogDebug($""hello {DateTime.Now}"");
    }
  }
}
");

            var expected1 = new DiagnosticResult("ZB1011", DiagnosticSeverity.Warning)
                .WithMessage("Non-constant log message.")
                .WithLocation(DiagnosticPosition(code));

            await VerifyCS.VerifyAnalyzerAsync(code, expected1);
        }

        [TestMethod]
        public async Task AnalyzeMaybeLogger()
        {

            var code = TestCode(@"
namespace Tests
{
  public class Test
  {
    public void Method(ILogger logger)
    {
      logger.IfDebug()?.LogDebug($""hello {DateTime.Now}"");
      //                           ‼1
      logger.IfWarning()?.LogDebug($""hello2{DateTime.Now}"");
      //                     ‼2
      Self(logger)?.LogDebug($""hello {DateTime.Now}"");
    }
    public T Self<T>(T obj) => obj;
  }
}
");

            var expected1 = new DiagnosticResult("ZB1011", DiagnosticSeverity.Warning)
                .WithMessage("Non-constant log message.")
                .WithLocation(DiagnosticPosition(code, 1));

            var expected2 = new DiagnosticResult("ZB1011", DiagnosticSeverity.Warning)
                .WithMessage("Non-constant log message.")
                .WithLocation(DiagnosticPosition(code, 2));

            await VerifyCS.VerifyAnalyzerAsync(code, expected1, expected2);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        [Ignore("We don't have a CodeFix for now.")]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class {|#0:TypeName|}
        {   
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";

            var expected = VerifyCS.Diagnostic("ZpqrtBnkILoggerAnalyzer").WithLocation(0).WithArguments("TypeName");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
