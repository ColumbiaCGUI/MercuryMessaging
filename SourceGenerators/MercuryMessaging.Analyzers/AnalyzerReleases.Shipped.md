; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
MM001 | MercuryMessaging | Info | Suggest using fluent DSL for verbose MmInvoke calls
MM005 | MercuryMessaging | Warning | Missing .Execute() on fluent message builder
MM010 | MercuryMessaging | Error | [MmGenerateDispatch] on non-partial class
