// Copyright (c) 2017-2025, Columbia University
// MM007: Potential infinite loop - sending same message type to Self

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Analyzer that warns when a message handler sends the same message type
    /// to Self, which can cause infinite recursion.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM007PotentialInfiniteLoopAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM007_PotentialInfiniteLoop);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;

            // Check if this is a message handler method
            var handlerInfo = GetHandlerInfo(method);
            if (handlerInfo == null)
                return;

            // Find any Send/MmInvoke calls in this method
            var invocations = method.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .ToList();

            foreach (var invocation in invocations)
            {
                if (IsPotentialInfiniteLoop(invocation, handlerInfo))
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.MM007_PotentialInfiniteLoop,
                        invocation.GetLocation(),
                        handlerInfo.MessageType);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private HandlerInfo GetHandlerInfo(MethodDeclarationSyntax method)
        {
            var methodName = method.Identifier.Text;

            // Check for ReceivedMessage overloads
            if (methodName == "ReceivedMessage" && method.ParameterList.Parameters.Count == 1)
            {
                var paramType = method.ParameterList.Parameters[0].Type?.ToString();
                if (paramType != null)
                {
                    // Extract the message type (e.g., MmMessageString -> MessageString)
                    var messageType = paramType.Replace("Mm", "");
                    return new HandlerInfo { MethodName = methodName, MessageType = messageType, FullParamType = paramType };
                }
            }

            // Check for standard handlers
            if (methodName == "ReceivedInitialize")
                return new HandlerInfo { MethodName = methodName, MessageType = "Initialize", MmMethod = "MmMethod.Initialize" };
            if (methodName == "ReceivedRefresh")
                return new HandlerInfo { MethodName = methodName, MessageType = "Refresh", MmMethod = "MmMethod.Refresh" };
            if (methodName == "ReceivedSetActive")
                return new HandlerInfo { MethodName = methodName, MessageType = "SetActive", MmMethod = "MmMethod.SetActive" };
            if (methodName == "ReceivedSwitch")
                return new HandlerInfo { MethodName = methodName, MessageType = "Switch", MmMethod = "MmMethod.Switch" };
            if (methodName == "ReceivedComplete")
                return new HandlerInfo { MethodName = methodName, MessageType = "Complete", MmMethod = "MmMethod.Complete" };

            // Check for MmInvoke override
            if (methodName == "MmInvoke" && method.ParameterList.Parameters.Count == 1)
            {
                var paramType = method.ParameterList.Parameters[0].Type?.ToString();
                if (paramType == "MmMessage")
                    return new HandlerInfo { MethodName = methodName, MessageType = "Any", IsMmInvokeOverride = true };
            }

            return null;
        }

        private bool IsPotentialInfiniteLoop(InvocationExpressionSyntax invocation, HandlerInfo handlerInfo)
        {
            var methodName = GetMethodName(invocation);

            // Check for Send() with same message type
            if (methodName == "Send")
            {
                if (SendsSameMessageType(invocation, handlerInfo) && TargetsSelf(invocation))
                    return true;
            }

            // Check for MmInvoke with same method
            if (methodName == "MmInvoke")
            {
                if (InvokesSameMethod(invocation, handlerInfo) && MmInvokeTargetsSelf(invocation))
                    return true;
            }

            // Check for BroadcastXxx methods that could reach self
            if (methodName.StartsWith("Broadcast") && MatchesBroadcastToHandler(methodName, handlerInfo))
            {
                // Broadcast methods target descendants by default, which includes self
                return true;
            }

            return false;
        }

        private bool SendsSameMessageType(InvocationExpressionSyntax invocation, HandlerInfo handlerInfo)
        {
            if (invocation.ArgumentList.Arguments.Count == 0)
                return false;

            var firstArg = invocation.ArgumentList.Arguments[0].ToString();

            // Check for MmMethod enum
            if (handlerInfo.MmMethod != null && firstArg.Contains(handlerInfo.MmMethod))
                return true;

            // Check for type-based inference (e.g., Send("hello") in ReceivedMessage(MmMessageString))
            if (handlerInfo.FullParamType != null)
            {
                if (handlerInfo.FullParamType.Contains("String") && IsStringLiteral(firstArg))
                    return true;
                if (handlerInfo.FullParamType.Contains("Int") && IsIntLiteral(firstArg))
                    return true;
                if (handlerInfo.FullParamType.Contains("Float") && IsFloatLiteral(firstArg))
                    return true;
                if (handlerInfo.FullParamType.Contains("Bool") && IsBoolLiteral(firstArg))
                    return true;
            }

            return false;
        }

        private bool TargetsSelf(InvocationExpressionSyntax invocation)
        {
            // Walk up the fluent chain to check for ToSelf() or no routing (defaults to SelfAndChildren)
            var current = invocation;
            bool hasExplicitRouting = false;

            while (current != null)
            {
                var methodName = GetMethodName(current);

                // If ToSelf is called, it definitely targets self
                if (methodName == "ToSelf")
                    return true;

                // If any routing other than ToSelf is explicitly set, check if it includes self
                if (methodName == "ToChildren" || methodName == "ToParents" ||
                    methodName == "ToSiblings" || methodName == "ToAncestors")
                {
                    hasExplicitRouting = true;
                    // These don't include self
                    return false;
                }

                if (methodName == "ToDescendants" || methodName == "ToAll")
                {
                    hasExplicitRouting = true;
                    // These include self
                    return true;
                }

                // Move up the chain
                if (current.Expression is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression is InvocationExpressionSyntax parentInvocation)
                {
                    current = parentInvocation;
                }
                else
                {
                    break;
                }
            }

            // Default routing (SelfAndChildren) includes self
            return !hasExplicitRouting;
        }

        private bool InvokesSameMethod(InvocationExpressionSyntax invocation, HandlerInfo handlerInfo)
        {
            if (handlerInfo.MmMethod == null)
                return false;

            foreach (var arg in invocation.ArgumentList.Arguments)
            {
                if (arg.ToString().Contains(handlerInfo.MmMethod))
                    return true;
            }

            return false;
        }

        private bool MmInvokeTargetsSelf(InvocationExpressionSyntax invocation)
        {
            // Check if MmMetadataBlock has LevelFilter.Self or SelfAndChildren
            foreach (var arg in invocation.ArgumentList.Arguments)
            {
                var argText = arg.ToString();

                // If Parent or Child only is used, self is not included
                if (argText.Contains("LevelFilter.Child") && !argText.Contains("SelfAnd"))
                    return false;
                if (argText.Contains("LevelFilter.Parent") && !argText.Contains("SelfAnd"))
                    return false;

                // Self or SelfAndChildren includes self
                if (argText.Contains("LevelFilter.Self"))
                    return true;
            }

            // Default includes self
            return true;
        }

        private bool MatchesBroadcastToHandler(string broadcastMethod, HandlerInfo handlerInfo)
        {
            if (broadcastMethod == "BroadcastInitialize" && handlerInfo.MessageType == "Initialize")
                return true;
            if (broadcastMethod == "BroadcastRefresh" && handlerInfo.MessageType == "Refresh")
                return true;
            if (broadcastMethod == "BroadcastSetActive" && handlerInfo.MessageType == "SetActive")
                return true;
            if (broadcastMethod == "BroadcastValue")
            {
                // Could be any value type
                if (handlerInfo.FullParamType != null &&
                    (handlerInfo.FullParamType.Contains("Int") ||
                     handlerInfo.FullParamType.Contains("Float") ||
                     handlerInfo.FullParamType.Contains("String") ||
                     handlerInfo.FullParamType.Contains("Bool")))
                {
                    return true;
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

        private bool IsStringLiteral(string arg) => arg.StartsWith("\"") || arg.StartsWith("@\"") || arg.StartsWith("$\"");
        private bool IsIntLiteral(string arg) => int.TryParse(arg, out _);
        private bool IsFloatLiteral(string arg) => arg.EndsWith("f") || arg.EndsWith("F") || float.TryParse(arg.TrimEnd('f', 'F'), out _);
        private bool IsBoolLiteral(string arg) => arg == "true" || arg == "false";

        private class HandlerInfo
        {
            public string MethodName { get; set; }
            public string MessageType { get; set; }
            public string MmMethod { get; set; }
            public string FullParamType { get; set; }
            public bool IsMmInvokeOverride { get; set; }
        }
    }
}
