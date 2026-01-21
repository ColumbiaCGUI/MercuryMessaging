// Copyright (c) 2017-2025, Columbia University
// MM004: Suggest Broadcast convenience methods

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Analyzer that suggests using Broadcast convenience methods
    /// like BroadcastInitialize() instead of verbose fluent chains.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM004SuggestBroadcastAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM004_SuggestBroadcast);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            // Check for patterns like: relay.Send(MmMethod.Initialize).ToDescendants().Execute()
            // or: relay.MmInvoke(MmMethod.Initialize)
            var suggestion = GetBroadcastSuggestion(invocation);
            if (suggestion != null)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.MM004_SuggestBroadcast,
                    invocation.GetLocation(),
                    suggestion);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private string GetBroadcastSuggestion(InvocationExpressionSyntax invocation)
        {
            var methodName = GetMethodName(invocation);

            // Check for Execute() calls on fluent chains
            if (methodName == "Execute" || methodName == "Send")
            {
                var chain = GetFluentChain(invocation);
                if (chain != null)
                {
                    return AnalyzeFluentChain(chain);
                }
            }

            // Check for direct MmInvoke with MmMethod.Initialize etc.
            if (methodName == "MmInvoke")
            {
                return AnalyzeMmInvoke(invocation);
            }

            return null;
        }

        private string AnalyzeFluentChain(FluentChainInfo chain)
        {
            // Pattern: .Send(MmMethod.Initialize).ToDescendants().Execute()
            // Suggest: BroadcastInitialize()
            if (chain.Method == "Initialize" && chain.IsToDescendants)
                return "BroadcastInitialize()";

            if (chain.Method == "Refresh" && chain.IsToDescendants)
                return "BroadcastRefresh()";

            if (chain.Method == "Complete" && chain.IsToParentsOrAncestors)
                return "NotifyComplete()";

            // Pattern: .Send(value).ToDescendants().Execute()
            // Suggest: BroadcastValue(value)
            if (chain.HasValue && chain.IsToDescendants)
                return $"BroadcastValue({chain.Value})";

            if (chain.HasValue && chain.IsToParentsOrAncestors)
                return $"NotifyValue({chain.Value})";

            return null;
        }

        private string AnalyzeMmInvoke(InvocationExpressionSyntax invocation)
        {
            if (invocation.ArgumentList.Arguments.Count == 0)
                return null;

            var firstArg = invocation.ArgumentList.Arguments[0].ToString();

            // Simple MmInvoke with just MmMethod
            if (invocation.ArgumentList.Arguments.Count == 1)
            {
                if (firstArg.Contains("MmMethod.Initialize"))
                    return "BroadcastInitialize()";
                if (firstArg.Contains("MmMethod.Refresh"))
                    return "BroadcastRefresh()";
            }

            // MmInvoke with MmMetadataBlock that targets children/descendants
            if (invocation.ArgumentList.Arguments.Count >= 2)
            {
                var hasDescendantFilter = false;
                var hasParentFilter = false;

                foreach (var arg in invocation.ArgumentList.Arguments)
                {
                    var argText = arg.ToString();
                    if (argText.Contains("LevelFilter.Child") ||
                        argText.Contains("LevelFilter.SelfAndChildren") ||
                        argText.Contains("MmLevelFilter.Child") ||
                        argText.Contains("MmLevelFilter.SelfAndChildren"))
                    {
                        hasDescendantFilter = true;
                    }
                    if (argText.Contains("LevelFilter.Parent") ||
                        argText.Contains("MmLevelFilter.Parent"))
                    {
                        hasParentFilter = true;
                    }
                }

                if (hasDescendantFilter)
                {
                    if (firstArg.Contains("MmMethod.Initialize"))
                        return "BroadcastInitialize()";
                    if (firstArg.Contains("MmMethod.Refresh"))
                        return "BroadcastRefresh()";
                }

                if (hasParentFilter)
                {
                    if (firstArg.Contains("MmMethod.Complete"))
                        return "NotifyComplete()";
                }
            }

            return null;
        }

        private FluentChainInfo GetFluentChain(InvocationExpressionSyntax invocation)
        {
            var info = new FluentChainInfo();
            var current = invocation;

            // Walk up the chain to find Send() and routing methods
            while (current != null)
            {
                var methodName = GetMethodName(current);

                switch (methodName)
                {
                    case "ToDescendants":
                    case "ToChildren":
                        info.IsToDescendants = true;
                        break;
                    case "ToParents":
                    case "ToAncestors":
                        info.IsToParentsOrAncestors = true;
                        break;
                    case "Send":
                        // Check what's being sent
                        if (current.ArgumentList.Arguments.Count > 0)
                        {
                            var arg = current.ArgumentList.Arguments[0].ToString();
                            if (arg.Contains("MmMethod.Initialize"))
                                info.Method = "Initialize";
                            else if (arg.Contains("MmMethod.Refresh"))
                                info.Method = "Refresh";
                            else if (arg.Contains("MmMethod.Complete"))
                                info.Method = "Complete";
                            else
                            {
                                info.HasValue = true;
                                info.Value = arg;
                            }
                        }
                        return info;
                }

                // Move to the expression that this was called on
                if (current.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    if (memberAccess.Expression is InvocationExpressionSyntax parentInvocation)
                    {
                        current = parentInvocation;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return info.Method != null || info.HasValue ? info : null;
        }

        private string GetMethodName(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                return memberAccess.Name.Identifier.Text;
            if (invocation.Expression is IdentifierNameSyntax identifier)
                return identifier.Identifier.Text;
            return string.Empty;
        }

        private class FluentChainInfo
        {
            public string Method { get; set; }
            public bool IsToDescendants { get; set; }
            public bool IsToParentsOrAncestors { get; set; }
            public bool HasValue { get; set; }
            public string Value { get; set; }
        }
    }
}
