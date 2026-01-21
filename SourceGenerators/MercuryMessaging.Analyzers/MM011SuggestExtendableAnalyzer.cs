// Copyright (c) 2017-2025, Columbia University
// MM011: Suggest MmExtendableResponder for many custom handlers

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Analyzer that suggests using MmExtendableResponder when a class
    /// has many custom message handlers in a switch statement.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM011SuggestExtendableAnalyzer : DiagnosticAnalyzer
    {
        private const int SwitchCaseThreshold = 3;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM011_SuggestExtendable);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            var classDecl = (ClassDeclarationSyntax)context.Node;

            // Skip if already using MmExtendableResponder
            if (InheritsFromExtendable(classDecl))
                return;

            // Check if this class inherits from MmBaseResponder or MmResponder
            if (!InheritsFromBaseResponder(classDecl))
                return;

            // Find MmInvoke override
            var mmInvokeMethod = classDecl.Members
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.Text == "MmInvoke" &&
                                     m.Modifiers.Any(SyntaxKind.OverrideKeyword));

            if (mmInvokeMethod == null)
                return;

            // Count custom method cases in switch statements
            var customCaseCount = CountCustomMethodCases(mmInvokeMethod);

            if (customCaseCount > SwitchCaseThreshold)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.MM011_SuggestExtendable,
                    classDecl.Identifier.GetLocation(),
                    customCaseCount);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private bool InheritsFromExtendable(ClassDeclarationSyntax classDecl)
        {
            if (classDecl.BaseList == null)
                return false;

            foreach (var baseType in classDecl.BaseList.Types)
            {
                var typeName = baseType.Type.ToString();
                if (typeName.Contains("MmExtendableResponder") ||
                    typeName.Contains("ExtendableResponder"))
                {
                    return true;
                }
            }

            return false;
        }

        private bool InheritsFromBaseResponder(ClassDeclarationSyntax classDecl)
        {
            if (classDecl.BaseList == null)
                return false;

            foreach (var baseType in classDecl.BaseList.Types)
            {
                var typeName = baseType.Type.ToString();
                if (typeName.Contains("MmBaseResponder") ||
                    typeName.Contains("MmResponder"))
                {
                    return true;
                }
            }

            return false;
        }

        private int CountCustomMethodCases(MethodDeclarationSyntax method)
        {
            var customCaseCount = 0;

            // Find switch statements
            var switchStatements = method.DescendantNodes()
                .OfType<SwitchStatementSyntax>()
                .ToList();

            foreach (var switchStmt in switchStatements)
            {
                // Check if switch is on message.method or similar
                var switchExpr = switchStmt.Expression.ToString();
                if (switchExpr.Contains("method") || switchExpr.Contains("Method"))
                {
                    foreach (var section in switchStmt.Sections)
                    {
                        foreach (var label in section.Labels)
                        {
                            if (label is CaseSwitchLabelSyntax caseLabel)
                            {
                                var caseValue = caseLabel.Value.ToString();
                                // Check for custom method values (> 1000 or cast patterns)
                                if (IsCustomMethodCase(caseValue))
                                {
                                    customCaseCount++;
                                }
                            }
                        }
                    }
                }
            }

            // Also check if-else chains checking method enum
            var ifStatements = method.DescendantNodes()
                .OfType<IfStatementSyntax>()
                .ToList();

            foreach (var ifStmt in ifStatements)
            {
                var conditionText = ifStmt.Condition.ToString();
                if (conditionText.Contains("method") && IsCustomMethodCheck(conditionText))
                {
                    customCaseCount++;
                }
            }

            return customCaseCount;
        }

        private bool IsCustomMethodCase(string caseValue)
        {
            // Check for (MmMethod)1000 pattern
            if (caseValue.Contains("(MmMethod)"))
                return true;

            // Check for MmMethod.Custom pattern (if any)
            if (caseValue.Contains("Custom"))
                return true;

            // Check for numeric values >= 1000
            if (int.TryParse(caseValue, out int value) && value >= 1000)
                return true;

            // Check for casted enum values
            if (caseValue.Contains("1000") || caseValue.Contains("1001") ||
                caseValue.Contains("1002") || caseValue.Contains("1003"))
                return true;

            return false;
        }

        private bool IsCustomMethodCheck(string conditionText)
        {
            // Check for patterns like: message.method == (MmMethod)1000
            if (conditionText.Contains("(MmMethod)1") ||
                conditionText.Contains("(MmMethod)2") ||
                conditionText.Contains(">= 1000") ||
                conditionText.Contains("> 999"))
            {
                return true;
            }

            return false;
        }
    }
}
