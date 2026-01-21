// Copyright (c) 2017-2025, Columbia University
// MM015: Bitwise filter equality check instead of AND

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Analyzer that warns when filter enums (MmTag, MmLevelFilter, etc.)
    /// are compared using == instead of bitwise AND operations.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM015BitwiseEqualityAnalyzer : DiagnosticAnalyzer
    {
        // Filter types that use bitwise operations
        private static readonly string[] BitwiseFilterTypes = new[]
        {
            "MmTag",
            "MmLevelFilter",
            "MmActiveFilter",
            "MmSelectedFilter",
            "MmNetworkFilter"
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM015_BitwiseEquality);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.EqualsExpression);
            context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.NotEqualsExpression);
        }

        private void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
        {
            var binary = (BinaryExpressionSyntax)context.Node;

            // Check if either side involves a filter type
            var leftText = binary.Left.ToString();
            var rightText = binary.Right.ToString();

            bool leftIsFilter = IsFilterExpression(leftText);
            bool rightIsFilter = IsFilterExpression(rightText);

            if (!leftIsFilter && !rightIsFilter)
                return;

            // Check if this is a problematic equality check
            // Problematic: tag == MmTag.Tag0 (only matches exact value)
            // Correct: (tag & MmTag.Tag0) != 0 (checks if flag is set)

            // Skip if comparing to 0, None, Nothing, Everything, or All
            // These are valid equality checks
            if (IsValidEqualityTarget(leftText) || IsValidEqualityTarget(rightText))
                return;

            // Skip if already part of a bitwise expression
            if (IsPartOfBitwiseExpression(binary))
                return;

            // Skip common valid patterns like: filter == default
            if (leftText == "default" || rightText == "default")
                return;

            // Check for specific tag flag check patterns that are wrong
            if (IsSpecificFlagCheck(leftText, rightText))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.MM015_BitwiseEquality,
                    binary.GetLocation());

                context.ReportDiagnostic(diagnostic);
            }
        }

        private bool IsFilterExpression(string text)
        {
            foreach (var filterType in BitwiseFilterTypes)
            {
                if (text.Contains(filterType))
                    return true;
            }

            // Also check for common variable names
            if (text.Contains("tag") || text.Contains("Tag") ||
                text.Contains("filter") || text.Contains("Filter"))
            {
                return true;
            }

            return false;
        }

        private bool IsValidEqualityTarget(string text)
        {
            // These are valid targets for equality comparison
            return text == "0" ||
                   text.Contains("Nothing") ||
                   text.Contains("None") ||
                   text.Contains("Everything") ||
                   text.Contains("All") ||
                   text.Contains("Default") ||
                   text == "default" ||
                   text == "-1";
        }

        private bool IsPartOfBitwiseExpression(BinaryExpressionSyntax binary)
        {
            // Check if either operand is already a bitwise AND result
            if (binary.Left is BinaryExpressionSyntax leftBinary)
            {
                if (leftBinary.Kind() == SyntaxKind.BitwiseAndExpression)
                    return true;
            }

            if (binary.Right is BinaryExpressionSyntax rightBinary)
            {
                if (rightBinary.Kind() == SyntaxKind.BitwiseAndExpression)
                    return true;
            }

            // Check if this is the != 0 part of (x & y) != 0
            var parent = binary.Parent;
            while (parent != null)
            {
                if (parent is ParenthesizedExpressionSyntax)
                {
                    parent = parent.Parent;
                    continue;
                }

                if (parent is BinaryExpressionSyntax parentBinary)
                {
                    if (parentBinary.Kind() == SyntaxKind.BitwiseAndExpression)
                        return true;
                }

                break;
            }

            return false;
        }

        private bool IsSpecificFlagCheck(string leftText, string rightText)
        {
            // Check for patterns like:
            // tag == MmTag.Tag0 (wrong - should be (tag & MmTag.Tag0) != 0)
            // filter == MmLevelFilter.Child (wrong - should use bitwise AND)

            // Left side is variable, right side is specific flag
            foreach (var filterType in BitwiseFilterTypes)
            {
                // Pattern: variable == FilterType.SpecificValue
                if (rightText.StartsWith(filterType + ".") &&
                    !IsValidEqualityTarget(rightText) &&
                    !rightText.Contains("Everything") &&
                    !rightText.Contains("All") &&
                    !rightText.Contains("None") &&
                    !rightText.Contains("Nothing"))
                {
                    // Left side should be a variable (not containing the filter type directly)
                    if (!leftText.Contains(filterType + "."))
                        return true;
                }

                // Reverse: FilterType.SpecificValue == variable
                if (leftText.StartsWith(filterType + ".") &&
                    !IsValidEqualityTarget(leftText) &&
                    !leftText.Contains("Everything") &&
                    !leftText.Contains("All") &&
                    !leftText.Contains("None") &&
                    !leftText.Contains("Nothing"))
                {
                    if (!rightText.Contains(filterType + "."))
                        return true;
                }
            }

            return false;
        }
    }
}
