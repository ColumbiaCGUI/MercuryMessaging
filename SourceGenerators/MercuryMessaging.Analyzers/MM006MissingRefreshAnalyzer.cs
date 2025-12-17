// Copyright (c) 2017-2025, Columbia University
// MM006: Missing MmRefreshResponders after AddComponent

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Analyzer that warns when AddComponent is called to add a responder
    /// but MmRefreshResponders() is not called afterward.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM006MissingRefreshAnalyzer : DiagnosticAnalyzer
    {
        // Known responder base types
        private static readonly string[] ResponderTypes = new[]
        {
            "MmBaseResponder",
            "MmResponder",
            "MmExtendableResponder",
            "MmUIResponder",
            "MmInputResponder",
            "MmNetworkResponder",
            "MmAppStateResponder"
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM006_MissingRefresh);

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

            // Find all AddComponent<T>() calls in this method
            var addComponentCalls = method.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(IsResponderAddComponent)
                .ToList();

            if (addComponentCalls.Count == 0)
                return;

            // Check if MmRefreshResponders is called anywhere in the method
            var hasRefreshCall = method.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(IsRefreshRespondersCall);

            if (!hasRefreshCall)
            {
                // Report diagnostic for each AddComponent call
                foreach (var call in addComponentCalls)
                {
                    var typeName = GetAddComponentTypeName(call);
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.MM006_MissingRefresh,
                        call.GetLocation(),
                        typeName);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private bool IsResponderAddComponent(InvocationExpressionSyntax invocation)
        {
            // Check for pattern: something.AddComponent<ResponderType>()
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Name is GenericNameSyntax genericName &&
                    genericName.Identifier.Text == "AddComponent")
                {
                    // Check the type argument
                    var typeArg = genericName.TypeArgumentList?.Arguments.FirstOrDefault();
                    if (typeArg != null)
                    {
                        var typeName = typeArg.ToString();
                        // Check if it's a known responder type or ends with "Responder"
                        if (ResponderTypes.Any(rt => typeName.Contains(rt)) ||
                            typeName.EndsWith("Responder"))
                        {
                            return true;
                        }
                    }
                }
            }

            // Check for pattern: AddComponent(typeof(ResponderType))
            var methodName = GetMethodName(invocation);
            if (methodName == "AddComponent")
            {
                foreach (var arg in invocation.ArgumentList.Arguments)
                {
                    var argText = arg.ToString();
                    if (ResponderTypes.Any(rt => argText.Contains(rt)) ||
                        argText.Contains("Responder"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsRefreshRespondersCall(InvocationExpressionSyntax invocation)
        {
            var methodName = GetMethodName(invocation);
            return methodName == "MmRefreshResponders";
        }

        private string GetAddComponentTypeName(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Name is GenericNameSyntax genericName)
                {
                    var typeArg = genericName.TypeArgumentList?.Arguments.FirstOrDefault();
                    if (typeArg != null)
                        return typeArg.ToString();
                }
            }

            // Try to extract from typeof() argument
            foreach (var arg in invocation.ArgumentList.Arguments)
            {
                if (arg.Expression is TypeOfExpressionSyntax typeOf)
                {
                    return typeOf.Type.ToString();
                }
            }

            return "Responder";
        }

        private string GetMethodName(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Name is GenericNameSyntax genericName)
                    return genericName.Identifier.Text;
                return memberAccess.Name.Identifier.Text;
            }
            if (invocation.Expression is IdentifierNameSyntax identifier)
                return identifier.Identifier.Text;
            return string.Empty;
        }
    }
}
