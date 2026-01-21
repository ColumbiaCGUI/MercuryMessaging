// Copyright (c) 2017-2025, Columbia University
// MM008: SetParent without routing table registration

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Analyzer that warns when transform.SetParent() is called
    /// but MmAddToRoutingTable() and AddParent() are not called.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM008SetParentWithoutRoutingAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM008_SetParentWithoutRouting);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;
            if (method.Body == null)
                return;

            // Check if the containing class is a MercuryMessaging responder or uses relay nodes
            if (!IsMercuryMessagingContext(method))
                return;

            // Find all SetParent calls in this method
            var setParentCalls = method.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(IsSetParentCall)
                .ToList();

            if (setParentCalls.Count == 0)
                return;

            // Check if routing table methods are called
            var hasRoutingTableCall = method.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(IsRoutingTableCall);

            if (!hasRoutingTableCall)
            {
                // Report diagnostic for each SetParent call
                foreach (var call in setParentCalls)
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.MM008_SetParentWithoutRouting,
                        call.GetLocation());

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private bool IsMercuryMessagingContext(MethodDeclarationSyntax method)
        {
            var containingClass = method.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (containingClass == null)
                return false;

            // Check base types for MercuryMessaging classes
            if (containingClass.BaseList != null)
            {
                foreach (var baseType in containingClass.BaseList.Types)
                {
                    var baseTypeName = baseType.Type.ToString();
                    if (baseTypeName.Contains("MmResponder") ||
                        baseTypeName.Contains("MmBaseResponder") ||
                        baseTypeName.Contains("MmExtendableResponder") ||
                        baseTypeName.Contains("MmRelayNode") ||
                        baseTypeName.Contains("MonoBehaviour"))
                    {
                        return true;
                    }
                }
            }

            // Check if the class has any MercuryMessaging member access
            var hasMMUsage = containingClass.DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .Any(m => m.ToString().Contains("MmRelayNode") ||
                          m.ToString().Contains("MmResponder") ||
                          m.ToString().Contains("MmInvoke") ||
                          m.ToString().Contains("GetComponent<MmRelayNode>"));

            return hasMMUsage;
        }

        private bool IsSetParentCall(InvocationExpressionSyntax invocation)
        {
            var methodName = GetMethodName(invocation);

            // Check for transform.SetParent() or .SetParent()
            if (methodName == "SetParent")
            {
                // Verify it's on a Transform (not some other SetParent)
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    var expressionText = memberAccess.Expression.ToString();
                    // Common patterns: transform.SetParent, child.transform.SetParent, etc.
                    if (expressionText.Contains("transform") ||
                        expressionText.EndsWith("Transform"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsRoutingTableCall(InvocationExpressionSyntax invocation)
        {
            var methodName = GetMethodName(invocation);

            // Check for routing table registration methods
            return methodName == "MmAddToRoutingTable" ||
                   methodName == "AddParent" ||
                   methodName == "RefreshParents" ||
                   methodName == "MmRefreshResponders";
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
