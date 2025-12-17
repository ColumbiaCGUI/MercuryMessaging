// Copyright (c) 2017-2025, Columbia University
// MercuryMessaging Analyzers - MM010: Non-Partial Class Analyzer
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
    /// Analyzer that detects classes with [MmGenerateDispatch] attribute
    /// that are not declared as partial.
    ///
    /// The source generator creates a partial class file with optimized
    /// dispatch code. Without the 'partial' keyword, the generated code
    /// cannot be merged with the user's class, causing compilation errors.
    ///
    /// Examples:
    /// <code>
    /// // Error: Class must be partial
    /// [MmGenerateDispatch]
    /// public class MyResponder : MmBaseResponder { }
    ///
    /// // OK: Partial class
    /// [MmGenerateDispatch]
    /// public partial class MyResponder : MmBaseResponder { }
    /// </code>
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM010NonPartialClassAnalyzer : DiagnosticAnalyzer
    {
        private const string MmGenerateDispatchAttributeName = "MmGenerateDispatch";
        private const string MmGenerateDispatchAttributeFullName = "MercuryMessaging.MmGenerateDispatchAttribute";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM010_NonPartialClass);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Analyze class declarations
            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            // Check if the class has [MmGenerateDispatch] attribute
            bool hasMmGenerateDispatch = false;

            foreach (var attributeList in classDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name.ToString();

                    // Check for both short and full attribute names
                    if (attributeName == MmGenerateDispatchAttributeName ||
                        attributeName == "MmGenerateDispatchAttribute" ||
                        attributeName == MmGenerateDispatchAttributeFullName)
                    {
                        hasMmGenerateDispatch = true;
                        break;
                    }

                    // Also check semantic model for full type name
                    var symbolInfo = context.SemanticModel.GetSymbolInfo(attribute, context.CancellationToken);
                    if (symbolInfo.Symbol is IMethodSymbol attributeConstructor)
                    {
                        var attributeType = attributeConstructor.ContainingType?.ToDisplayString();
                        if (attributeType == MmGenerateDispatchAttributeFullName)
                        {
                            hasMmGenerateDispatch = true;
                            break;
                        }
                    }
                }

                if (hasMmGenerateDispatch)
                    break;
            }

            if (!hasMmGenerateDispatch)
                return;

            // Check if the class is declared partial
            bool isPartial = classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

            if (isPartial)
                return;

            // Report diagnostic
            var className = classDeclaration.Identifier.Text;
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.MM010_NonPartialClass,
                classDeclaration.Identifier.GetLocation(),
                className);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
