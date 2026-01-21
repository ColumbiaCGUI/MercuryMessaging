// Copyright (c) 2017-2025, Columbia University
// MercuryMessaging Analyzers - MM010: Code Fix Provider
// DSL/DX Phase 3: Roslyn Analyzers

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Code fix provider that adds the 'partial' keyword to classes with [MmGenerateDispatch].
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MM010CodeFixProvider)), Shared]
    public class MM010CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create("MM010");

        public override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the class declaration
            var classDeclaration = root.FindToken(diagnosticSpan.Start)
                .Parent?
                .AncestorsAndSelf()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            if (classDeclaration == null)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add 'partial' modifier",
                    createChangedDocument: c => AddPartialModifierAsync(context.Document, classDeclaration, c),
                    equivalenceKey: "AddPartial"),
                diagnostic);
        }

        private async Task<Document> AddPartialModifierAsync(
            Document document,
            ClassDeclarationSyntax classDeclaration,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            // Find where to insert 'partial' - after access modifiers, before 'class'
            var modifiers = classDeclaration.Modifiers;
            var partialToken = SyntaxFactory.Token(SyntaxKind.PartialKeyword)
                .WithTrailingTrivia(SyntaxFactory.Space);

            SyntaxTokenList newModifiers;

            if (modifiers.Count == 0)
            {
                // No modifiers - just add partial
                newModifiers = SyntaxFactory.TokenList(partialToken);
            }
            else
            {
                // Insert partial after last modifier
                // Typically: public partial class, internal partial class, etc.
                var lastModifier = modifiers.Last();
                var partialWithSpace = partialToken.WithLeadingTrivia(SyntaxFactory.Space);

                newModifiers = modifiers.Add(partialWithSpace);
            }

            var newClassDeclaration = classDeclaration.WithModifiers(newModifiers);
            var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
