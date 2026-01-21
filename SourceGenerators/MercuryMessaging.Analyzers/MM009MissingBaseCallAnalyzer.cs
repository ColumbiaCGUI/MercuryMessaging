// Copyright (c) 2017-2025, Columbia University
// MM009: Missing base.MmInvoke call in override

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Analyzer that warns when MmInvoke is overridden but base.MmInvoke()
    /// is not called, which prevents standard message handlers from working.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM009MissingBaseCallAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM009_MissingBaseCall);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;

            // Check if this is an MmInvoke override
            if (!IsOverrideMmInvoke(method))
                return;

            // Check if base.MmInvoke is called
            var hasBaseCall = method.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(IsBaseMmInvokeCall);

            if (!hasBaseCall)
            {
                // Check if there's a good reason not to call base
                // (e.g., the method explicitly returns early for all cases)
                if (!HasIntentionalBaseOmission(method))
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.MM009_MissingBaseCall,
                        method.Identifier.GetLocation());

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private bool IsOverrideMmInvoke(MethodDeclarationSyntax method)
        {
            // Check method name
            if (method.Identifier.Text != "MmInvoke")
                return false;

            // Check for override modifier
            if (!method.Modifiers.Any(SyntaxKind.OverrideKeyword))
                return false;

            // Check parameter (should be MmMessage)
            if (method.ParameterList.Parameters.Count != 1)
                return false;

            var paramType = method.ParameterList.Parameters[0].Type?.ToString();
            if (paramType != "MmMessage" && paramType != "MercuryMessaging.MmMessage")
                return false;

            return true;
        }

        private bool IsBaseMmInvokeCall(InvocationExpressionSyntax invocation)
        {
            // Check for base.MmInvoke(...)
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Expression is BaseExpressionSyntax &&
                    memberAccess.Name.Identifier.Text == "MmInvoke")
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasIntentionalBaseOmission(MethodDeclarationSyntax method)
        {
            // Check for comments indicating intentional omission
            var leadingTrivia = method.GetLeadingTrivia();
            foreach (var trivia in leadingTrivia)
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                    trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    var commentText = trivia.ToString().ToLower();
                    if (commentText.Contains("no base") ||
                        commentText.Contains("skip base") ||
                        commentText.Contains("intentionally") ||
                        commentText.Contains("don't call base") ||
                        commentText.Contains("do not call base"))
                    {
                        return true;
                    }
                }
            }

            // Check for suppression attribute
            var attributes = method.AttributeLists
                .SelectMany(al => al.Attributes)
                .ToList();

            foreach (var attr in attributes)
            {
                var attrName = attr.Name.ToString();
                if (attrName.Contains("SuppressMessage") ||
                    attrName.Contains("Obsolete"))
                {
                    return true;
                }
            }

            // Check if method has a switch statement that handles all cases
            // and explicitly returns or handles each case differently
            var hasExhaustiveSwitch = method.DescendantNodes()
                .OfType<SwitchStatementSyntax>()
                .Any(s => s.Sections.Count > 5); // Heuristic: if many cases, probably intentional

            return hasExhaustiveSwitch;
        }
    }
}
