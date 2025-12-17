// Copyright (c) 2017-2025, Columbia University
// MM013: Responder without relay node

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Analyzer that warns when a responder class doesn't have any
    /// GetComponent calls for MmRelayNode, suggesting the user
    /// ensure a relay node exists in the hierarchy.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM013ResponderWithoutRelayAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM013_ResponderWithoutRelay);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            var classDecl = (ClassDeclarationSyntax)context.Node;

            // Check if this is a responder class that sends messages
            if (!IsResponderClassThatSendsMessages(classDecl))
                return;

            // Check if there's any reference to MmRelayNode
            var hasRelayNodeReference = HasRelayNodeReference(classDecl);

            // Check if the class is itself a relay node
            if (InheritsFromRelayNode(classDecl))
                return;

            // Check if class uses RequireComponent attribute for MmRelayNode
            if (HasRequireComponentAttribute(classDecl))
                return;

            if (!hasRelayNodeReference)
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.MM013_ResponderWithoutRelay,
                    classDecl.Identifier.GetLocation());

                context.ReportDiagnostic(diagnostic);
            }
        }

        private bool IsResponderClassThatSendsMessages(ClassDeclarationSyntax classDecl)
        {
            // Check if this inherits from a responder base class
            if (!IsResponderClass(classDecl))
                return false;

            // Check if it actually sends messages (has MmInvoke, Send, Broadcast calls)
            var hasSendingCalls = classDecl.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(IsSendingCall);

            return hasSendingCalls;
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

        private bool InheritsFromRelayNode(ClassDeclarationSyntax classDecl)
        {
            if (classDecl.BaseList == null)
                return false;

            foreach (var baseType in classDecl.BaseList.Types)
            {
                var typeName = baseType.Type.ToString();
                if (typeName.Contains("MmRelayNode") ||
                    typeName.Contains("MmRelaySwitchNode"))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsSendingCall(InvocationExpressionSyntax invocation)
        {
            var methodName = GetMethodName(invocation);

            return methodName == "MmInvoke" ||
                   methodName == "Send" ||
                   methodName == "Execute" ||
                   methodName.StartsWith("Broadcast") ||
                   methodName.StartsWith("Notify");
        }

        private bool HasRelayNodeReference(ClassDeclarationSyntax classDecl)
        {
            // Check for GetComponent<MmRelayNode>
            var hasGetComponent = classDecl.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(inv =>
                {
                    var text = inv.ToString();
                    return text.Contains("GetComponent<MmRelayNode>") ||
                           text.Contains("GetComponentInParent<MmRelayNode>") ||
                           text.Contains("GetComponentInChildren<MmRelayNode>");
                });

            if (hasGetComponent)
                return true;

            // Check for MmRelayNode field
            var hasRelayField = classDecl.Members
                .OfType<FieldDeclarationSyntax>()
                .Any(f => f.Declaration.Type.ToString().Contains("MmRelayNode"));

            if (hasRelayField)
                return true;

            // Check for MmRelayNode property
            var hasRelayProperty = classDecl.Members
                .OfType<PropertyDeclarationSyntax>()
                .Any(p => p.Type.ToString().Contains("MmRelayNode"));

            if (hasRelayProperty)
                return true;

            // Check for [SerializeField] MmRelayNode
            var hasSerializedRelay = classDecl.DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .Any(f =>
                {
                    var typeText = f.Declaration.Type.ToString();
                    return typeText.Contains("MmRelayNode") ||
                           typeText.Contains("relay") ||
                           typeText.Contains("Relay");
                });

            return hasSerializedRelay;
        }

        private bool HasRequireComponentAttribute(ClassDeclarationSyntax classDecl)
        {
            foreach (var attrList in classDecl.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    var attrName = attr.Name.ToString();
                    if (attrName == "RequireComponent" || attrName == "RequireComponentAttribute")
                    {
                        // Check if it requires MmRelayNode
                        var args = attr.ArgumentList?.Arguments.ToString() ?? "";
                        if (args.Contains("MmRelayNode"))
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
