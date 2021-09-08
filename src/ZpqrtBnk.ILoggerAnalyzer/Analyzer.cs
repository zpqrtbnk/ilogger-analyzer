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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ZpqrtBnk.ILoggerAnalyzer
{
    // TODO:
    // add file headers (via R#)
    // create the Testing project and move Verifiers code there
    // analyzer
    //  ZB0101 - non-constant string blah blah

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class Analyzer : DiagnosticAnalyzer
    {
        private const string Category = "ZpqrtBnk";
        private const string HelpLinkUri = "https://github.com/zpqrtbnk/Zpqrtbnk.ILoggerAnalyzer";

        private static readonly LocalizableString Title = "Comments Analyzer."; // Resources.GetLocalizableString(nameof(Resources.AnalyzerTitle));

        public const string WarnOnNonConstantDiagnosticId = "ZB1011";
        private static readonly DiagnosticDescriptor WarnOnNonConstantMessageRule = new DiagnosticDescriptor(
            WarnOnNonConstantDiagnosticId,
            Title,
            "Non-constant log message.",
            Category,
            DiagnosticSeverity.Warning, // report as a warning
            true,
            description: "A non-constant log message is evaluated regardless of the log level, and can have an impact on performances, " +
                "especially for low-level (LogTrace, LogDebug) operations that can be verbose but are usually not written out. Either replace " +
                "the log message with a constant string and parameters, or use a construct such as logger.IfDebug()?.LogDebug(...) in order " +
                "to introduce a conditional evaluation of the log message.",
            helpLinkUri: HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(WarnOnNonConstantMessageRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            //// TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            //// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);

            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            // only for method invocations
            if (!(context.Node is InvocationExpressionSyntax invocation)) return;
            var info = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
            if (!(info.Symbol is IMethodSymbol method)) return;

            // only for extension methods
            if (!method.IsExtensionMethod) return;

            // is ILogger even present in the compilation?
            var iloggerType = context.SemanticModel.Compilation.GetTypeByMetadataName("Microsoft.Extensions.Logging.ILogger");
            if (iloggerType == null) return;

            // only for ILogger extension methods
            // FIXME what about ILogger<T>?
            if (!SymbolEqualityComparer.Default.Equals(method.ReceiverType, iloggerType)) return;

            // first arg which is a string...
            var stringType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.String");
            var i = 0;
            while (i < method.Parameters.Length && !SymbolEqualityComparer.Default.Equals(method.Parameters[i].Type, stringType)) i++;
            if (i == method.Parameters.Length) return;

            var stringArg = invocation.ArgumentList.Arguments[i];

            // simple string literal?
            if (stringArg.Expression is LiteralExpressionSyntax) return; // TODO: validate the template and args?

            // not a simple string literal
            // can we at least get a computed constant value for it?
            var constantValue = context.SemanticModel.GetConstantValue(stringArg.Expression, context.CancellationToken);
            if (constantValue.HasValue && constantValue.Value is string) return; // TODO: validate the template and args?

            // not at compile time :(
            // at least we can rule out string.Empty
            if (context.SemanticModel.GetSymbolInfo(stringArg.Expression, context.CancellationToken).Symbol is IFieldSymbol field &&
                SymbolEqualityComparer.Default.Equals(field.ContainingType, stringType) &&
                field.Name == "Empty" &&
                SymbolEqualityComparer.Default.Equals(field.Type, stringType)) return;

            // TODO: detect the IfDebug()?.LogDebug() construct = in this case it's OK to NOT be constant
            // so we need to know... what's the origin of the invocation?
            var parent = invocation.Parent;
            if (parent is ConditionalAccessExpressionSyntax conditional)
            {
                // is a xxx?.Method() invocation and so maybe it's OK
                // we need to verify that what's before the ?. is OK don't we?

                var xxx = conditional.Expression;
                if (xxx is InvocationExpressionSyntax invocation2)
                {
                    var info2 = context.SemanticModel.GetSymbolInfo(invocation2, context.CancellationToken);
                    if (info2.Symbol is IMethodSymbol method2 &&
                        method2.IsExtensionMethod &&
                        SymbolEqualityComparer.Default.Equals(method2.ReceiverType, iloggerType) &&
                        Match(method.Name, method2.Name))
                        return; // this is OK

                }
            }

            // ok, report
            context.ReportDiagnostic(Diagnostic.Create(WarnOnNonConstantMessageRule, stringArg.Expression.GetLocation(), stringArg.Expression.ToString()));
        }

        static bool Match(string logMethod, string ifMethod)
        {
            if (!logMethod.StartsWith("Log")) return false;
            if (!ifMethod.StartsWith("If")) return false;
            if (logMethod.Length - ifMethod.Length != 1) return false;
            for (var i = 3; i < logMethod.Length; i++)
                if (logMethod[i] != ifMethod[i - 1])
                    return false;
            return true;
        }
    }
}
