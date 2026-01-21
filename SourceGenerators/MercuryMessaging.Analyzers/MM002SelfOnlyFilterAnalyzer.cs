// Copyright (c) 2017-2025, Columbia University
// MM002: Self-only level filter warning

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Analyzer that warns when MmLevelFilter.Self is used alone,
    /// as messages won't propagate to children.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM002SelfOnlyFilterAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM002_SelfOnlyFilter);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        }

        private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;

            // Check if this is MmLevelFilter.Self
            if (memberAccess.Name.Identifier.Text != "Self")
                return;

            if (memberAccess.Expression is IdentifierNameSyntax identifier &&
                identifier.Identifier.Text == "MmLevelFilter")
            {
                // Check if this is being used in a context where it's likely intentional
                // Skip if it's part of a bitwise operation (e.g., Self | Child)
                if (IsPartOfBitwiseOperation(memberAccess))
                    return;

                // Skip if it's in an equality comparison (being checked, not set)
                if (IsInEqualityComparison(memberAccess))
                    return;

                // Skip if it's in MmLevelFilterHelper context (internal usage)
                if (IsInHelperContext(memberAccess))
                    return;

                // Report diagnostic
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.MM002_SelfOnlyFilter,
                    memberAccess.GetLocation());

                context.ReportDiagnostic(diagnostic);
            }
        }

        private bool IsPartOfBitwiseOperation(MemberAccessExpressionSyntax memberAccess)
        {
            var parent = memberAccess.Parent;
            while (parent != null)
            {
                if (parent is BinaryExpressionSyntax binary)
                {
                    var kind = binary.Kind();
                    if (kind == SyntaxKind.BitwiseOrExpression ||
                        kind == SyntaxKind.BitwiseAndExpression)
                    {
                        return true;
                    }
                }

                // Stop at statement level
                if (parent is StatementSyntax)
                    break;

                parent = parent.Parent;
            }
            return false;
        }

        private bool IsInEqualityComparison(MemberAccessExpressionSyntax memberAccess)
        {
            var parent = memberAccess.Parent;
            while (parent != null)
            {
                if (parent is BinaryExpressionSyntax binary)
                {
                    var kind = binary.Kind();
                    if (kind == SyntaxKind.EqualsExpression ||
                        kind == SyntaxKind.NotEqualsExpression)
                    {
                        return true;
                    }
                }

                if (parent is StatementSyntax)
                    break;

                parent = parent.Parent;
            }
            return false;
        }

        private bool IsInHelperContext(MemberAccessExpressionSyntax memberAccess)
        {
            // Check if this is inside a class named MmLevelFilterHelper or similar
            var classDecl = memberAccess.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDecl != null)
            {
                var className = classDecl.Identifier.Text;
                if (className.Contains("Helper") || className.Contains("Filter"))
                    return true;
            }
            return false;
        }
    }
}
