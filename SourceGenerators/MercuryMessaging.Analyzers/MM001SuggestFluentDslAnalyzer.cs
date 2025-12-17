// Copyright (c) 2017-2025, Columbia University
// MercuryMessaging Analyzers - MM001: Suggest Fluent DSL Analyzer
// DSL/DX Phase 3: Roslyn Analyzers

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Analyzer that suggests using the fluent DSL instead of verbose MmInvoke calls.
    ///
    /// Triggers when detecting patterns like:
    /// <code>
    /// // Info: Consider using fluent DSL
    /// relay.MmInvoke(MmMethod.MessageString, "Hello", new MmMetadataBlock(...));
    ///
    /// // Suggested alternative:
    /// relay.Send("Hello").ToChildren().Execute();
    /// </code>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM001SuggestFluentDslAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM001_SuggestFluentDsl);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Analyze invocation expressions
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            // Check if this is a call to MmInvoke
            string methodName = GetMethodName(invocation);
            if (methodName != "MmInvoke")
                return;

            // Get the method symbol to verify it's from MmRelayNode
            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
                return;

            var containingType = methodSymbol.ContainingType?.ToDisplayString();

            // Check if it's from MmRelayNode or MmResponder types
            if (containingType == null)
                return;

            bool isMmRelayNode = containingType.Contains("MmRelayNode") ||
                                 containingType.Contains("IMmNode") ||
                                 containingType.Contains("MmResponder") ||
                                 containingType.Contains("MmBaseResponder");

            if (!isMmRelayNode)
                return;

            // Check argument count - suggest DSL for calls with MmMetadataBlock (verbose)
            var arguments = invocation.ArgumentList.Arguments;

            // Only suggest for calls with 2+ arguments (method + value/metadata)
            // or calls with explicit MmMetadataBlock
            if (arguments.Count < 2)
                return;

            // Check if any argument is MmMetadataBlock or new MmMetadataBlock(...)
            bool hasMetadataBlock = false;
            string suggestedDsl = null;

            foreach (var arg in arguments)
            {
                var argType = context.SemanticModel.GetTypeInfo(arg.Expression, context.CancellationToken);
                var typeName = argType.Type?.ToDisplayString() ?? "";

                if (typeName.Contains("MmMetadataBlock"))
                {
                    hasMetadataBlock = true;
                    break;
                }

                // Check for object creation of MmMetadataBlock
                if (arg.Expression is ObjectCreationExpressionSyntax objectCreation)
                {
                    var createdTypeName = objectCreation.Type.ToString();
                    if (createdTypeName.Contains("MmMetadataBlock"))
                    {
                        hasMetadataBlock = true;
                        break;
                    }
                }
            }

            // Only suggest DSL for verbose calls (with metadata block)
            if (!hasMetadataBlock)
                return;

            // Generate suggestion based on the call pattern
            suggestedDsl = GenerateDslSuggestion(arguments, context);

            if (string.IsNullOrEmpty(suggestedDsl))
                suggestedDsl = "relay.Send(value).ToChildren().Execute()";

            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.MM001_SuggestFluentDsl,
                invocation.GetLocation(),
                suggestedDsl);

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

        private static string GenerateDslSuggestion(
            SeparatedSyntaxList<ArgumentSyntax> arguments,
            SyntaxNodeAnalysisContext context)
        {
            if (arguments.Count == 0)
                return null;

            var sb = new StringBuilder("relay.");

            // Try to extract method and value from arguments
            string mmMethod = null;
            string value = null;
            string levelFilter = null;

            foreach (var arg in arguments)
            {
                var argText = arg.ToString();

                // Check for MmMethod enum
                if (argText.StartsWith("MmMethod."))
                {
                    mmMethod = argText.Replace("MmMethod.", "");
                    continue;
                }

                // Check for MmMetadataBlock to extract level filter
                if (arg.Expression is ObjectCreationExpressionSyntax objectCreation)
                {
                    var metadataArgs = objectCreation.ArgumentList?.Arguments;
                    if (metadataArgs != null)
                    {
                        foreach (var metaArg in metadataArgs)
                        {
                            var metaArgText = metaArg.ToString();
                            if (metaArgText.Contains("MmLevelFilter.") ||
                                metaArgText.Contains("MmLevelFilterHelper."))
                            {
                                levelFilter = ExtractLevelFilter(metaArgText);
                            }
                        }
                    }
                    continue;
                }

                // Otherwise it's likely a value
                if (value == null && !argText.Contains("MmMetadataBlock"))
                {
                    value = argText;
                }
            }

            // Build DSL suggestion
            if (!string.IsNullOrEmpty(value))
            {
                sb.Append($"Send({value})");
            }
            else if (!string.IsNullOrEmpty(mmMethod))
            {
                // Map MmMethod to DSL method
                switch (mmMethod)
                {
                    case "Initialize":
                        sb.Append("Initialize()");
                        break;
                    case "Refresh":
                        sb.Append("Refresh()");
                        break;
                    case "Complete":
                        sb.Append("Complete()");
                        break;
                    default:
                        sb.Append($"Send({mmMethod})");
                        break;
                }
            }
            else
            {
                sb.Append("Send(value)");
            }

            // Add level filter
            if (!string.IsNullOrEmpty(levelFilter))
            {
                sb.Append($".{levelFilter}");
            }
            else
            {
                sb.Append(".ToChildren()");
            }

            sb.Append(".Execute()");

            return sb.ToString();
        }

        private static string ExtractLevelFilter(string filterText)
        {
            if (filterText.Contains("Child"))
                return "ToChildren()";
            if (filterText.Contains("Parent"))
                return "ToParents()";
            if (filterText.Contains("Descendants"))
                return "ToDescendants()";
            if (filterText.Contains("Ancestors"))
                return "ToAncestors()";
            if (filterText.Contains("Siblings"))
                return "ToSiblings()";
            if (filterText.Contains("Bidirectional"))
                return "ToAll()";

            return "ToChildren()";
        }
    }
}
