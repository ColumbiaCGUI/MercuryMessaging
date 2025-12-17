// Copyright (c) 2017-2025, Columbia University
// MM014: Misspelled message handler method name

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MercuryMessaging.Analyzers
{
    /// <summary>
    /// Analyzer that warns when a method name looks like it's trying to be
    /// a message handler but is misspelled.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MM014MisspelledHandlerAnalyzer : DiagnosticAnalyzer
    {
        // Known correct handler method names
        private static readonly Dictionary<string, string> KnownHandlers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "ReceivedInitialize", "ReceivedInitialize" },
            { "ReceivedRefresh", "ReceivedRefresh" },
            { "ReceivedSetActive", "ReceivedSetActive" },
            { "ReceivedSwitch", "ReceivedSwitch" },
            { "ReceivedComplete", "ReceivedComplete" },
            { "ReceivedMessage", "ReceivedMessage" },
            { "ReceivedTaskInfo", "ReceivedTaskInfo" },
        };

        // Common misspellings and their corrections
        private static readonly Dictionary<string, string> CommonMisspellings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "RecievedInitialize", "ReceivedInitialize" },
            { "RecievedRefresh", "ReceivedRefresh" },
            { "RecievedSetActive", "ReceivedSetActive" },
            { "RecievedSwitch", "ReceivedSwitch" },
            { "RecievedComplete", "ReceivedComplete" },
            { "RecievedMessage", "ReceivedMessage" },
            { "RecievedTaskInfo", "ReceivedTaskInfo" },
            { "ReceiveInitialize", "ReceivedInitialize" },
            { "ReceiveRefresh", "ReceivedRefresh" },
            { "ReceiveSetActive", "ReceivedSetActive" },
            { "ReceiveSwitch", "ReceivedSwitch" },
            { "ReceiveComplete", "ReceivedComplete" },
            { "ReceiveMessage", "ReceivedMessage" },
            { "OnReceivedInitialize", "ReceivedInitialize" },
            { "OnReceivedRefresh", "ReceivedRefresh" },
            { "OnReceivedSetActive", "ReceivedSetActive" },
            { "OnReceivedSwitch", "ReceivedSwitch" },
            { "OnReceivedComplete", "ReceivedComplete" },
            { "OnReceivedMessage", "ReceivedMessage" },
            { "HandleInitialize", "ReceivedInitialize" },
            { "HandleRefresh", "ReceivedRefresh" },
            { "HandleSetActive", "ReceivedSetActive" },
            { "HandleSwitch", "ReceivedSwitch" },
            { "HandleComplete", "ReceivedComplete" },
            { "HandleMessage", "ReceivedMessage" },
            { "OnInitialize", "ReceivedInitialize" },
            { "OnRefresh", "ReceivedRefresh" },
            { "OnSetActive", "ReceivedSetActive" },
            { "OnSwitch", "ReceivedSwitch" },
            { "OnComplete", "ReceivedComplete" },
            { "OnMessage", "ReceivedMessage" },
            { "ReceivedInit", "ReceivedInitialize" },
            { "RecievedInit", "ReceivedInitialize" },
            { "RecievedMesage", "ReceivedMessage" },
            { "RecievedMessge", "ReceivedMessage" },
            { "ReceivedMessge", "ReceivedMessage" },
            { "ReceivedMesage", "ReceivedMessage" },
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(DiagnosticDescriptors.MM014_MisspelledHandler);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var method = (MethodDeclarationSyntax)context.Node;
            var methodName = method.Identifier.Text;

            // Check if this is a known correct handler
            if (KnownHandlers.ContainsKey(methodName))
                return;

            // Check for common misspellings
            if (CommonMisspellings.TryGetValue(methodName, out var correction))
            {
                ReportMisspelling(context, method, methodName, correction);
                return;
            }

            // Check for fuzzy matches with handler patterns
            var fuzzyMatch = FindFuzzyMatch(methodName);
            if (fuzzyMatch != null)
            {
                ReportMisspelling(context, method, methodName, fuzzyMatch);
            }
        }

        private void ReportMisspelling(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax method,
            string methodName, string correction)
        {
            // Only report if in a responder class
            var containingClass = method.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (containingClass == null || !IsResponderClass(containingClass))
                return;

            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.MM014_MisspelledHandler,
                method.Identifier.GetLocation(),
                methodName,
                correction);

            context.ReportDiagnostic(diagnostic);
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

        private string FindFuzzyMatch(string methodName)
        {
            // Check if it looks like it might be a handler (contains handler-related words)
            var lowerName = methodName.ToLower();

            if (!lowerName.Contains("receiv") && !lowerName.Contains("reciev") &&
                !lowerName.Contains("handle") && !lowerName.Contains("on"))
            {
                return null;
            }

            // Check for message type keywords
            string[] messageTypes = { "initialize", "refresh", "setactive", "switch", "complete", "message", "taskinfo" };

            foreach (var msgType in messageTypes)
            {
                if (lowerName.Contains(msgType))
                {
                    // Find the correct handler name
                    var correctName = "Received" + char.ToUpper(msgType[0]) + msgType.Substring(1);
                    if (msgType == "setactive")
                        correctName = "ReceivedSetActive";
                    else if (msgType == "taskinfo")
                        correctName = "ReceivedTaskInfo";

                    // Only suggest if it's clearly wrong
                    if (!KnownHandlers.Values.Contains(methodName))
                        return correctName;
                }
            }

            // Calculate Levenshtein distance for close matches
            foreach (var handler in KnownHandlers.Values)
            {
                var distance = LevenshteinDistance(methodName.ToLower(), handler.ToLower());
                // If within 3 edits, it's probably a typo
                if (distance > 0 && distance <= 3)
                {
                    return handler;
                }
            }

            return null;
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s1.Length, s2.Length];
        }
    }
}
