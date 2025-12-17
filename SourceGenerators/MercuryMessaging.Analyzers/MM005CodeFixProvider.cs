// Copyright (c) 2017-2025, Columbia University
// MercuryMessaging Analyzers - MM005: Code Fix Provider
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
    /// Code fix provider that adds .Execute() to MmFluentMessage chains.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MM005CodeFixProvider)), Shared]
    public class MM005CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create("MM005");

        public override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
                return;

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the invocation expression
            var invocation = root.FindToken(diagnosticSpan.Start)
                .Parent?
                .AncestorsAndSelf()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault();

            if (invocation == null)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add .Execute()",
                    createChangedDocument: c => AddExecuteAsync(context.Document, invocation, c),
                    equivalenceKey: "AddExecute"),
                diagnostic);
        }

        private async Task<Document> AddExecuteAsync(
            Document document,
            InvocationExpressionSyntax invocation,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
                return document;

            // Create the new invocation with .Execute() appended
            // Original: relay.Send("Hello").ToChildren()
            // New: relay.Send("Hello").ToChildren().Execute()

            var executeAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                invocation,
                SyntaxFactory.IdentifierName("Execute"));

            var executeInvocation = SyntaxFactory.InvocationExpression(executeAccess)
                .WithArgumentList(SyntaxFactory.ArgumentList());

            // Replace the original invocation with the new one
            var newRoot = root.ReplaceNode(invocation, executeInvocation);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
