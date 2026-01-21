// Copyright (c) 2017-2025, Columbia University
// MM012: Tag check without TagCheckEnabled

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Analyzer that warns when Tag property is assigned but
    /// TagCheckEnabled is not set to true.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM012TagWithoutEnabledAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM012_TagWithoutEnabled);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            var classDecl = (ClassDeclarationSyntax)context.Node;

            // Check if this is a responder class
            if (!IsResponderClass(classDecl))
                return;

            // Find Tag assignments
            var tagAssignments = classDecl.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Where(IsTagAssignment)
                .ToList();

            if (tagAssignments.Count == 0)
                return;

            // Check if TagCheckEnabled is set to true
            var hasTagCheckEnabled = classDecl.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Any(IsTagCheckEnabledTrue);

            // Also check field initializers
            var hasTagCheckEnabledField = classDecl.Members
                .OfType<FieldDeclarationSyntax>()
                .Any(IsTagCheckEnabledFieldTrue);

            if (!hasTagCheckEnabled && !hasTagCheckEnabledField)
            {
                // Report diagnostic for each Tag assignment
                foreach (var assignment in tagAssignments)
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.MM012_TagWithoutEnabled,
                        assignment.GetLocation());

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private bool IsResponderClass(ClassDeclarationSyntax classDecl)
        {
            if (classDecl.BaseList == null)
                return false;

            foreach (var baseType in classDecl.BaseList.Types)
            {
                var typeName = baseType.Type.ToString();
                if (typeName.Contains("MmResponder") ||
                    typeName.Contains("MmBaseResponder") ||
                    typeName.Contains("MmExtendableResponder") ||
                    typeName.Contains("MmUIResponder") ||
                    typeName.Contains("MmInputResponder"))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsTagAssignment(AssignmentExpressionSyntax assignment)
        {
            // Check for: Tag = MmTag.Tag0 or this.Tag = MmTag.Tag0
            var leftText = assignment.Left.ToString();
            if (leftText == "Tag" || leftText == "this.Tag")
            {
                var rightText = assignment.Right.ToString();
                // Make sure it's an MmTag assignment, not some other "Tag"
                if (rightText.Contains("MmTag") || rightText.Contains("Tag0") ||
                    rightText.Contains("Tag1") || rightText.Contains("Tag."))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsTagCheckEnabledTrue(AssignmentExpressionSyntax assignment)
        {
            var leftText = assignment.Left.ToString();
            if (leftText == "TagCheckEnabled" || leftText == "this.TagCheckEnabled")
            {
                var rightText = assignment.Right.ToString();
                return rightText == "true";
            }

            return false;
        }

        private bool IsTagCheckEnabledFieldTrue(FieldDeclarationSyntax field)
        {
            var variable = field.Declaration.Variables.FirstOrDefault();
            if (variable == null)
                return false;

            if (variable.Identifier.Text == "TagCheckEnabled")
            {
                if (variable.Initializer != null)
                {
                    return variable.Initializer.Value.ToString() == "true";
                }
            }

            return false;
        }
    }
}
