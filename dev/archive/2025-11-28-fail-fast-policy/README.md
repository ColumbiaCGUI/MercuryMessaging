# Fail-Fast Policy Implementation

**Status:** Complete
**Date:** 2025-11-28
**Commit:** `8e6f52d3`

## Overview

Implemented fail-fast policy across the MercuryMessaging framework to eliminate silent fallbacks that mask failures and make debugging difficult.

## Changes Made

### Framework Fixes

| File | Issue | Fix |
|------|-------|-----|
| `MmBinarySerializer.cs` | IMmSerializable.Deserialize() never called - created empty instance | Properly use Serialize()/Deserialize() with object[] serialization |
| `MmNetworkBridge.cs` | Only logged e.Message, lost stack trace | Use Debug.LogException(e) for full stack trace |
| `MmNetworkResponderPhoton.cs` | Same - only e.Message logged | Use Debug.LogException(e) for full stack trace |
| `AppStateBuilder.cs` | Bare catch block returning null | Specific KeyNotFoundException + general Exception with logging |
| `MmRelayNode.cs` | No fail-fast option for development | Added StrictMode flag that throws exceptions |

### Test Fixes

| File | Issue | Fix |
|------|-------|-----|
| `MmListenerTests.cs` | Tests expected error logs but didn't use LogAssert.Expect | Added LogAssert.Expect for expected error logs |

### Documentation

| File | Change |
|------|--------|
| `.claude/ASSISTANT_GUIDE.md` | Added "No Silent Fallbacks Policy" section with MUST rules |

## Dotfiles Configuration

Also configured user-level Claude Code settings in `~/dotfiles/claude/`:

- **CLAUDE.md**: Global fail-fast principles (P-1, P-2, P-3)
- **settings.json**: Deny rules, PreToolUse hooks, TDD Guard plugin
- **Commands**: `/verify`, `/complete-check`, `/no-fallbacks`

Pushed to: `github.com/benplus1/dotfiles` (commit `15811d8`)

## StrictMode Usage

```csharp
// Enable fail-fast behavior during development
MmRelayNode.StrictMode = true;

// When enabled, these scenarios throw exceptions instead of logging:
// - Hop limit exceeded
// - Cycle detection triggered
// - Invalid lateral routing configuration
// - Missing custom filter predicate
```

## Test Results

- 643/646 tests passing after fixes
- 1 remaining failure: `FluentApiPerformanceTests.Benchmark_CompareOverhead_SimpleMessage` (flaky performance test, unrelated)

## Files Modified

```
.claude/ASSISTANT_GUIDE.md
Assets/MercuryMessaging/Protocol/DSL/AppStateBuilder.cs
Assets/MercuryMessaging/Protocol/MmNetworkResponderPhoton.cs
Assets/MercuryMessaging/Protocol/MmRelayNode.cs
Assets/MercuryMessaging/Protocol/Network/MmBinarySerializer.cs
Assets/MercuryMessaging/Protocol/Network/MmNetworkBridge.cs
Assets/MercuryMessaging/Tests/MmListenerTests.cs
```
