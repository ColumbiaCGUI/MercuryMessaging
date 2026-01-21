// Copyright (c) 2017-2025, Columbia University
// MM003: Network message without OverNetwork filter

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Analyzer that warns when a message appears to be intended for network
    /// but MmNetworkFilter.Local is used.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM003NetworkWithoutFilterAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM003_NetworkWithoutFilter);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            // Check for MmInvoke calls with MmMetadataBlock that has NetworkFilter.Local
            // but appears to be in a network context
            if (!IsNetworkContextualMmInvoke(invocation))
                return;

            // Check if MmNetworkFilter.Local is explicitly used
            if (HasLocalNetworkFilter(invocation))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.MM003_NetworkWithoutFilter,
                    invocation.GetLocation());

                context.ReportDiagnostic(diagnostic);
            }
        }

        private bool IsNetworkContextualMmInvoke(InvocationExpressionSyntax invocation)
        {
            // Check if this is an MmInvoke call
            var methodName = GetMethodName(invocation);
            if (methodName != "MmInvoke")
                return false;

            // Check if we're in a network-related class or method
            var containingMethod = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (containingMethod != null)
            {
                var methodNameText = containingMethod.Identifier.Text;
                // Look for network-related method names
                if (methodNameText.Contains("Network") ||
                    methodNameText.Contains("Sync") ||
                    methodNameText.Contains("Rpc") ||
                    methodNameText.Contains("Remote") ||
                    methodNameText.Contains("Server") ||
                    methodNameText.Contains("Client"))
                {
                    return true;
                }
            }

            var containingClass = invocation.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (containingClass != null)
            {
                var className = containingClass.Identifier.Text;
                // Look for network-related class names
                if (className.Contains("Network") ||
                    className.Contains("Sync") ||
                    className.Contains("Multiplayer") ||
                    className.Contains("Remote"))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasLocalNetworkFilter(InvocationExpressionSyntax invocation)
        {
            // Check arguments for MmNetworkFilter.Local
            foreach (var arg in invocation.ArgumentList.Arguments)
            {
                var argText = arg.ToString();
                if (argText.Contains("MmNetworkFilter.Local") ||
                    argText.Contains("NetworkFilter.Local"))
                {
                    return true;
                }
            }

            // Check if MmMetadataBlock is used without network filter (defaults to Local)
            foreach (var arg in invocation.ArgumentList.Arguments)
            {
                if (arg.Expression is ObjectCreationExpressionSyntax creation)
                {
                    var typeText = creation.Type.ToString();
                    if (typeText.Contains("MmMetadataBlock"))
                    {
                        // Check if MmNetworkFilter is specified
                        var hasNetworkFilter = false;
                        if (creation.ArgumentList != null)
                        {
                            foreach (var metaArg in creation.ArgumentList.Arguments)
                            {
                                var metaArgText = metaArg.ToString();
                                if (metaArgText.Contains("MmNetworkFilter") ||
                                    metaArgText.Contains("NetworkFilter"))
                                {
                                    hasNetworkFilter = true;
                                    // Check if it's Local
                                    if (metaArgText.Contains(".Local"))
                                        return true;
                                    break;
                                }
                            }
                        }

                        // If in network context but no network filter specified, warn
                        // (default is Local)
                        if (!hasNetworkFilter)
                            return true;
                    }
                }
            }

            return false;
        }

        private string GetMethodName(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                return memberAccess.Name.Identifier.Text;
            if (invocation.Expression is IdentifierNameSyntax identifier)
                return identifier.Identifier.Text;
            return string.Empty;
        }
    }
}
