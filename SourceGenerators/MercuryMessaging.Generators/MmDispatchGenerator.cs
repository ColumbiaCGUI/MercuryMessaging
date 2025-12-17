// Copyright (c) 2017-2025, Columbia University
// MercuryMessaging Source Generator - Phase 4 Performance Optimization
// Enhanced with [MmHandler] support for custom method dispatch

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MercuryMessaging.Generators
{
    /// <summary>
    /// Source generator that creates optimized MmInvoke dispatch code for responders
    /// marked with [MmGenerateDispatch] attribute.
    ///
    /// This eliminates virtual dispatch overhead by generating compile-time switch statements
    /// based on MmMessageType, reducing message handling from ~8-10 ticks to ~2-4 ticks.
    ///
    /// Phase 4 Enhancement: Also supports [MmHandler(methodId)] attributes on methods
    /// for custom method dispatch without dictionary lookup overhead.
    /// </summary>
    [Generator]
    public class MmDispatchGenerator : IIncrementalGenerator
    {
        private const string DispatchAttributeFullName = "MercuryMessaging.MmGenerateDispatchAttribute";
        private const string HandlerAttributeFullName = "MercuryMessaging.MmHandlerAttribute";

        // Diagnostic IDs for generator errors
        private static readonly DiagnosticDescriptor InvalidMethodIdDiagnostic = new(
            id: "MMG001",
            title: "Invalid MmHandler method ID",
            messageFormat: "MmHandler method ID {0} is invalid because custom methods must use IDs >= 1000",
            category: "MercuryMessaging.Generators",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor DuplicateMethodIdDiagnostic = new(
            id: "MMG002",
            title: "Duplicate MmHandler method ID",
            messageFormat: "MmHandler method ID {0} is already used by method '{1}'",
            category: "MercuryMessaging.Generators",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor InvalidHandlerSignatureDiagnostic = new(
            id: "MMG003",
            title: "Invalid MmHandler method signature",
            messageFormat: "MmHandler method '{0}' must accept exactly one MmMessage parameter",
            category: "MercuryMessaging.Generators",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor HandlerWithoutDispatchDiagnostic = new(
            id: "MMG004",
            title: "MmHandler without MmGenerateDispatch",
            messageFormat: "Method '{0}' has [MmHandler] but class '{1}' is missing [MmGenerateDispatch]",
            category: "MercuryMessaging.Generators",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register syntax provider to find classes with [MmGenerateDispatch]
            var classDeclarations = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    DispatchAttributeFullName,
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, _) => GetClassInfo(ctx))
                .Where(static classInfo => classInfo is not null);

            // Register source output
            context.RegisterSourceOutput(classDeclarations,
                static (spc, classInfo) => Execute(spc, classInfo!.Value));
        }

        private static ResponderClassInfo? GetClassInfo(GeneratorAttributeSyntaxContext context)
        {
            if (context.TargetNode is not ClassDeclarationSyntax classDeclaration)
                return null;

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
            if (classSymbol is null)
                return null;

            // Get attribute data
            var attributeData = context.Attributes.FirstOrDefault(
                a => a.AttributeClass?.ToDisplayString() == DispatchAttributeFullName);

            if (attributeData is null)
                return null;

            // Extract attribute properties
            bool includeStandardMethods = true;
            bool enableDebugLogging = false;
            bool generateNullChecks = true;

            foreach (var namedArg in attributeData.NamedArguments)
            {
                switch (namedArg.Key)
                {
                    case "IncludeStandardMethods":
                        includeStandardMethods = (bool)namedArg.Value.Value!;
                        break;
                    case "EnableDebugLogging":
                        enableDebugLogging = (bool)namedArg.Value.Value!;
                        break;
                    case "GenerateNullChecks":
                        generateNullChecks = (bool)namedArg.Value.Value!;
                        break;
                }
            }

            // Collect handlers
            var messageHandlers = new List<MessageHandlerInfo>();
            var standardHandlers = new List<StandardHandlerInfo>();
            var customHandlers = new List<CustomHandlerInfo>();
            var diagnostics = new List<DiagnosticInfo>();

            // Track custom handler IDs to detect duplicates
            var usedCustomIds = new Dictionary<int, string>();

            foreach (var member in classSymbol.GetMembers())
            {
                if (member is not IMethodSymbol method)
                    continue;

                // Check for [MmHandler] attribute on methods
                var handlerAttr = method.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == HandlerAttributeFullName);

                if (handlerAttr != null)
                {
                    ProcessMmHandlerAttribute(method, handlerAttr, customHandlers, usedCustomIds, diagnostics);
                    continue; // Don't process as other handler type
                }

                // Check for ReceivedMessage overrides (typed message handlers)
                if (method.Name == "ReceivedMessage" && method.IsOverride && method.Parameters.Length == 1)
                {
                    var paramType = method.Parameters[0].Type;
                    var messageTypeName = GetMessageTypeName(paramType.Name);

                    if (messageTypeName != null)
                    {
                        messageHandlers.Add(new MessageHandlerInfo
                        {
                            ParameterTypeName = paramType.ToDisplayString(),
                            MessageTypeEnumValue = messageTypeName
                        });
                    }
                }

                // Check for standard method overrides (ReceivedSetActive, ReceivedInitialize, etc.)
                if (includeStandardMethods && method.IsOverride)
                {
                    var standardMethod = GetStandardMethodInfo(method.Name);
                    if (standardMethod != null)
                    {
                        standardHandlers.Add(standardMethod.Value);
                    }
                }
            }

            // Skip if no handlers found
            if (messageHandlers.Count == 0 && standardHandlers.Count == 0 && customHandlers.Count == 0)
                return null;

            return new ResponderClassInfo
            {
                Namespace = classSymbol.ContainingNamespace?.ToDisplayString() ?? "",
                ClassName = classSymbol.Name,
                MessageHandlers = messageHandlers,
                StandardHandlers = standardHandlers,
                CustomHandlers = customHandlers,
                IncludeStandardMethods = includeStandardMethods,
                EnableDebugLogging = enableDebugLogging,
                GenerateNullChecks = generateNullChecks,
                Diagnostics = diagnostics
            };
        }

        private static void ProcessMmHandlerAttribute(
            IMethodSymbol method,
            AttributeData handlerAttr,
            List<CustomHandlerInfo> customHandlers,
            Dictionary<int, string> usedCustomIds,
            List<DiagnosticInfo> diagnostics)
        {
            // Get method ID from attribute
            int methodId = 0;
            if (handlerAttr.ConstructorArguments.Length > 0)
            {
                var arg = handlerAttr.ConstructorArguments[0];
                methodId = arg.Value is int i ? i : 0;
            }

            // Get optional name
            string? handlerName = null;
            foreach (var namedArg in handlerAttr.NamedArguments)
            {
                if (namedArg.Key == "Name" && namedArg.Value.Value is string s)
                {
                    handlerName = s;
                }
            }

            // Validate method ID >= 1000
            if (methodId < 1000)
            {
                diagnostics.Add(new DiagnosticInfo
                {
                    Descriptor = InvalidMethodIdDiagnostic,
                    Location = method.Locations.FirstOrDefault(),
                    Args = new object[] { methodId }
                });
                return;
            }

            // Check for duplicates
            if (usedCustomIds.TryGetValue(methodId, out var existingMethod))
            {
                diagnostics.Add(new DiagnosticInfo
                {
                    Descriptor = DuplicateMethodIdDiagnostic,
                    Location = method.Locations.FirstOrDefault(),
                    Args = new object[] { methodId, existingMethod }
                });
                return;
            }

            // Validate method signature (must accept MmMessage or derived)
            if (method.Parameters.Length != 1)
            {
                diagnostics.Add(new DiagnosticInfo
                {
                    Descriptor = InvalidHandlerSignatureDiagnostic,
                    Location = method.Locations.FirstOrDefault(),
                    Args = new object[] { method.Name }
                });
                return;
            }

            var paramType = method.Parameters[0].Type;
            if (!IsMmMessageType(paramType))
            {
                diagnostics.Add(new DiagnosticInfo
                {
                    Descriptor = InvalidHandlerSignatureDiagnostic,
                    Location = method.Locations.FirstOrDefault(),
                    Args = new object[] { method.Name }
                });
                return;
            }

            // Add to handlers
            usedCustomIds[methodId] = method.Name;
            customHandlers.Add(new CustomHandlerInfo
            {
                MethodName = method.Name,
                MethodId = methodId,
                HandlerName = handlerName ?? method.Name,
                ParameterTypeName = paramType.ToDisplayString()
            });
        }

        private static bool IsMmMessageType(ITypeSymbol type)
        {
            // Check if type is MmMessage or derives from it
            var current = type;
            while (current != null)
            {
                if (current.Name == "MmMessage")
                    return true;
                current = current.BaseType;
            }
            return false;
        }

        private static string? GetMessageTypeName(string parameterTypeName)
        {
            // Map MmMessage types to MmMessageType enum values
            return parameterTypeName switch
            {
                "MmMessage" => "MmVoid",
                "MmMessageBool" => "MmBool",
                "MmMessageInt" => "MmInt",
                "MmMessageFloat" => "MmFloat",
                "MmMessageString" => "MmString",
                "MmMessageVector3" => "MmVector3",
                "MmMessageVector4" => "MmVector4",
                "MmMessageQuaternion" => "MmQuaternion",
                "MmMessageTransform" => "MmTransform",
                "MmMessageTransformList" => "MmTransformList",
                "MmMessageByteArray" => "MmByteArray",
                "MmMessageSerializable" => "MmSerializable",
                "MmMessageGameObject" => "MmGameObject",
                _ => null
            };
        }

        private static StandardHandlerInfo? GetStandardMethodInfo(string methodName)
        {
            return methodName switch
            {
                "ReceivedSetActive" => new StandardHandlerInfo { MethodName = methodName, MmMethodValue = "SetActive" },
                "ReceivedInitialize" => new StandardHandlerInfo { MethodName = methodName, MmMethodValue = "Initialize" },
                "ReceivedRefresh" => new StandardHandlerInfo { MethodName = methodName, MmMethodValue = "Refresh" },
                "ReceivedSwitch" => new StandardHandlerInfo { MethodName = methodName, MmMethodValue = "Switch", HasParameter = true, ParameterType = "int" },
                "ReceivedComplete" => new StandardHandlerInfo { MethodName = methodName, MmMethodValue = "Complete" },
                _ => null
            };
        }

        private static void Execute(SourceProductionContext context, ResponderClassInfo classInfo)
        {
            // Report any diagnostics
            foreach (var diag in classInfo.Diagnostics)
            {
                if (diag.Location != null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(diag.Descriptor, diag.Location, diag.Args));
                }
            }

            var source = GenerateDispatchCode(classInfo);
            context.AddSource($"{classInfo.ClassName}.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        private static string GenerateDispatchCode(ResponderClassInfo classInfo)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// This code was generated by MercuryMessaging.Generators");
            sb.AppendLine("// Phase 4 Performance Optimization: Source-generated dispatch");
            sb.AppendLine("// Enhanced with [MmHandler] support for custom method dispatch");
            sb.AppendLine("// Changes to this file may be overwritten.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();
            sb.AppendLine("#nullable enable");
            sb.AppendLine();

            // Usings
            sb.AppendLine("using System;");
            sb.AppendLine("using MercuryMessaging;");
            sb.AppendLine();

            // Namespace
            if (!string.IsNullOrEmpty(classInfo.Namespace))
            {
                sb.AppendLine($"namespace {classInfo.Namespace}");
                sb.AppendLine("{");
            }

            // Partial class declaration
            var indent = string.IsNullOrEmpty(classInfo.Namespace) ? "" : "    ";
            sb.AppendLine($"{indent}public partial class {classInfo.ClassName}");
            sb.AppendLine($"{indent}{{");

            // Generate MmInvoke override
            GenerateMmInvokeMethod(sb, classInfo, indent + "    ");

            sb.AppendLine($"{indent}}}");

            if (!string.IsNullOrEmpty(classInfo.Namespace))
            {
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        private static void GenerateMmInvokeMethod(StringBuilder sb, ResponderClassInfo classInfo, string indent)
        {
            sb.AppendLine($"{indent}/// <summary>");
            sb.AppendLine($"{indent}/// Source-generated optimized message dispatch.");
            sb.AppendLine($"{indent}/// Eliminates virtual dispatch overhead via compile-time switch.");
            if (classInfo.CustomHandlers.Count > 0)
            {
                sb.AppendLine($"{indent}/// Includes {classInfo.CustomHandlers.Count} custom [MmHandler] method(s).");
            }
            sb.AppendLine($"{indent}/// </summary>");
            sb.AppendLine($"{indent}public override void MmInvoke(MmMessage message)");
            sb.AppendLine($"{indent}{{");

            var innerIndent = indent + "    ";

            // Null check
            if (classInfo.GenerateNullChecks)
            {
                sb.AppendLine($"{innerIndent}if (message == null)");
                sb.AppendLine($"{innerIndent}{{");
                sb.AppendLine($"{innerIndent}    base.MmInvoke(message!);");
                sb.AppendLine($"{innerIndent}    return;");
                sb.AppendLine($"{innerIndent}}}");
                sb.AppendLine();
            }

            // Debug logging
            if (classInfo.EnableDebugLogging)
            {
                sb.AppendLine($"{innerIndent}UnityEngine.Debug.Log($\"[MmDispatch] {{GetType().Name}}: {{message.MmMethod}} ({{message.MmMessageType}})\");");
                sb.AppendLine();
            }

            // Generate switch on MmMessageType for typed handlers
            if (classInfo.MessageHandlers.Count > 0)
            {
                sb.AppendLine($"{innerIndent}// Type-based dispatch for ReceivedMessage handlers");
                sb.AppendLine($"{innerIndent}switch (message.MmMessageType)");
                sb.AppendLine($"{innerIndent}{{");

                foreach (var handler in classInfo.MessageHandlers)
                {
                    sb.AppendLine($"{innerIndent}    case MmMessageType.{handler.MessageTypeEnumValue}:");
                    sb.AppendLine($"{innerIndent}        ReceivedMessage(({handler.ParameterTypeName})message);");
                    sb.AppendLine($"{innerIndent}        return;");
                }

                sb.AppendLine($"{innerIndent}}}");
                sb.AppendLine();
            }

            // Generate switch on MmMethod for standard handlers
            if (classInfo.IncludeStandardMethods && classInfo.StandardHandlers.Count > 0)
            {
                sb.AppendLine($"{innerIndent}// Method-based dispatch for standard handlers");
                sb.AppendLine($"{innerIndent}switch (message.MmMethod)");
                sb.AppendLine($"{innerIndent}{{");

                foreach (var handler in classInfo.StandardHandlers)
                {
                    sb.AppendLine($"{innerIndent}    case MmMethod.{handler.MmMethodValue}:");
                    if (handler.HasParameter)
                    {
                        // For methods with parameters, cast message and extract value
                        if (handler.MmMethodValue == "Switch")
                        {
                            sb.AppendLine($"{innerIndent}        if (message is MmMessageString switchMsg)");
                            sb.AppendLine($"{innerIndent}        {{");
                            sb.AppendLine($"{innerIndent}            // Switch can take string (state name) - convert to index via base");
                            sb.AppendLine($"{innerIndent}            base.MmInvoke(message);");
                            sb.AppendLine($"{innerIndent}            return;");
                            sb.AppendLine($"{innerIndent}        }}");
                        }
                    }
                    else
                    {
                        // Simple methods without parameters
                        sb.AppendLine($"{innerIndent}        {handler.MethodName}();");
                        sb.AppendLine($"{innerIndent}        return;");
                    }
                }

                sb.AppendLine($"{innerIndent}}}");
                sb.AppendLine();
            }

            // Generate switch for custom [MmHandler] methods
            if (classInfo.CustomHandlers.Count > 0)
            {
                sb.AppendLine($"{innerIndent}// Custom method dispatch for [MmHandler] methods");
                sb.AppendLine($"{innerIndent}var methodId = (int)message.MmMethod;");
                sb.AppendLine($"{innerIndent}if (methodId >= 1000)");
                sb.AppendLine($"{innerIndent}{{");
                sb.AppendLine($"{innerIndent}    switch (methodId)");
                sb.AppendLine($"{innerIndent}    {{");

                foreach (var handler in classInfo.CustomHandlers.OrderBy(h => h.MethodId))
                {
                    sb.AppendLine($"{innerIndent}        case {handler.MethodId}: // {handler.HandlerName}");
                    // Check if handler expects a specific type
                    if (handler.ParameterTypeName != "MmMessage" && !handler.ParameterTypeName.EndsWith(".MmMessage"))
                    {
                        sb.AppendLine($"{innerIndent}            {handler.MethodName}(({handler.ParameterTypeName})message);");
                    }
                    else
                    {
                        sb.AppendLine($"{innerIndent}            {handler.MethodName}(message);");
                    }
                    sb.AppendLine($"{innerIndent}            return;");
                }

                sb.AppendLine($"{innerIndent}    }}");
                sb.AppendLine($"{innerIndent}    // Custom method ID >= 1000 without handler - silently ignore");
                sb.AppendLine($"{innerIndent}    // (don't pass to base which would throw ArgumentOutOfRangeException)");
                sb.AppendLine($"{innerIndent}    return;");
                sb.AppendLine($"{innerIndent}}}");
                sb.AppendLine();
            }

            // Fall through to base
            sb.AppendLine($"{innerIndent}// Fall through to base implementation for unhandled types");
            sb.AppendLine($"{innerIndent}base.MmInvoke(message);");

            sb.AppendLine($"{indent}}}");
        }

        private struct ResponderClassInfo
        {
            public string Namespace;
            public string ClassName;
            public List<MessageHandlerInfo> MessageHandlers;
            public List<StandardHandlerInfo> StandardHandlers;
            public List<CustomHandlerInfo> CustomHandlers;
            public bool IncludeStandardMethods;
            public bool EnableDebugLogging;
            public bool GenerateNullChecks;
            public List<DiagnosticInfo> Diagnostics;
        }

        private struct MessageHandlerInfo
        {
            public string ParameterTypeName;
            public string MessageTypeEnumValue;
        }

        private struct StandardHandlerInfo
        {
            public string MethodName;
            public string MmMethodValue;
            public bool HasParameter;
            public string ParameterType;
        }

        private struct CustomHandlerInfo
        {
            public string MethodName;
            public int MethodId;
            public string HandlerName;
            public string ParameterTypeName;
        }

        private struct DiagnosticInfo
        {
            public DiagnosticDescriptor Descriptor;
            public Location? Location;
            public object[] Args;
        }
    }
}
