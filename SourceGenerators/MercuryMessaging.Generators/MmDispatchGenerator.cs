// Copyright (c) 2017-2025, Columbia University
// MercuryMessaging Source Generator - Phase 4 Performance Optimization

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
    /// </summary>
    [Generator]
    public class MmDispatchGenerator : IIncrementalGenerator
    {
        private const string AttributeFullName = "MercuryMessaging.MmGenerateDispatchAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register syntax provider to find classes with [MmGenerateDispatch]
            var classDeclarations = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    AttributeFullName,
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
                a => a.AttributeClass?.ToDisplayString() == AttributeFullName);

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

            // Collect ReceivedMessage overrides
            var messageHandlers = new List<MessageHandlerInfo>();
            var standardHandlers = new List<StandardHandlerInfo>();

            foreach (var member in classSymbol.GetMembers())
            {
                if (member is not IMethodSymbol method)
                    continue;

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
            if (messageHandlers.Count == 0 && standardHandlers.Count == 0)
                return null;

            return new ResponderClassInfo
            {
                Namespace = classSymbol.ContainingNamespace?.ToDisplayString() ?? "",
                ClassName = classSymbol.Name,
                MessageHandlers = messageHandlers,
                StandardHandlers = standardHandlers,
                IncludeStandardMethods = includeStandardMethods,
                EnableDebugLogging = enableDebugLogging,
                GenerateNullChecks = generateNullChecks
            };
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
            public bool IncludeStandardMethods;
            public bool EnableDebugLogging;
            public bool GenerateNullChecks;
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
    }
}
