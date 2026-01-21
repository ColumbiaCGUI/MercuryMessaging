; Unshipped analyzer changes
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
MM002 | MercuryMessaging | Warning | Self-only level filter may be unintentional
MM003 | MercuryMessaging | Warning | Network message may not propagate correctly
MM004 | MercuryMessaging | Info | Consider using Broadcast convenience method
MM006 | MercuryMessaging | Warning | Missing MmRefreshResponders() after adding responder
MM007 | MercuryMessaging | Warning | Potential infinite message loop
MM008 | MercuryMessaging | Warning | SetParent without routing table update
MM009 | MercuryMessaging | Warning | Override may need base.MmInvoke() call
MM011 | MercuryMessaging | Info | Consider using MmExtendableResponder
MM012 | MercuryMessaging | Warning | Tag assignment without TagCheckEnabled
MM013 | MercuryMessaging | Info | Responder may need MmRelayNode
MM014 | MercuryMessaging | Warning | Possible misspelled message handler
MM015 | MercuryMessaging | Warning | Incorrect filter comparison (use bitwise AND)
