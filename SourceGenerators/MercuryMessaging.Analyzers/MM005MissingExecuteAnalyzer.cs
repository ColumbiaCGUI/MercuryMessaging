// Copyright (c) 2017-2025, Columbia University
// MercuryMessaging Analyzers - MM005: Missing Execute() Analyzer
// DSL/DX Phase 3: Roslyn Analyzers

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Analyzer that detects MmFluentMessage or MmDeferredRoutingBuilder chains
    /// that don't end with Execute() or Send() call.
    ///
    /// Examples of code that triggers this analyzer:
    /// <code>
    /// // Warning: MmFluentMessage chain without Execute()
    /// relay.Send("Hello").ToChildren();
    ///
    /// // Warning: Builder chain without Execute()
    /// relay.Build().ToChildren().Send("Hello");
    ///
    /// // OK: Proper execution
    /// relay.Send("Hello").ToChildren().Execute();
    /// relay.Build().ToChildren().Send("Hello").Execute();
    /// </code>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM005MissingExecuteAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM005_MissingExecute);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Analyze expression statements (statements that are just expressions)
            context.RegisterSyntaxNodeAction(AnalyzeExpressionStatement, SyntaxKind.ExpressionStatement);
        }

        private void AnalyzeExpressionStatement(SyntaxNodeAnalysisContext context)
        {
            var expressionStatement = (ExpressionStatementSyntax)context.Node;
            var expression = expressionStatement.Expression;

            // We're looking for invocation expressions that return MmFluentMessage or MmDeferredRoutingBuilder
            // but the result is discarded (i.e., it's used as a statement, not assigned or passed)
            if (expression is not InvocationExpressionSyntax invocation)
                return;

            // Get the return type of the expression
            var typeInfo = context.SemanticModel.GetTypeInfo(invocation, context.CancellationToken);
            var returnType = typeInfo.Type;

            if (returnType == null)
                return;

            var typeName = returnType.ToDisplayString();

            // Check if it's one of our builder types
            bool isMmFluentMessage = typeName == "MercuryMessaging.MmFluentMessage" ||
                                     typeName == "MmFluentMessage";
            bool isMmDeferredRoutingBuilder = typeName == "MercuryMessaging.MmDeferredRoutingBuilder" ||
                                               typeName == "MmDeferredRoutingBuilder";

            if (!isMmFluentMessage && !isMmDeferredRoutingBuilder)
                return;

            // Check if the invocation ends with Execute() or Send()
            // Get the method name being called
            string methodName = GetMethodName(invocation);

            // If the method IS Execute() or Send(), then we're good
            if (methodName == "Execute" || methodName == "Send")
                return;

            // Otherwise, warn about missing Execute()
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.MM005_MissingExecute,
                invocation.GetLocation());

            context.ReportDiagnostic(diagnostic);
        }

        private static string GetMethodName(InvocationExpressionSyntax invocation)
        {
            return invocation.Expression switch
            {
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,
                IdentifierNameSyntax identifier => identifier.Identifier.Text,
                _ => string.Empty
            };
        }
    }
}
