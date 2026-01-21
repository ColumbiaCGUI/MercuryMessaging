# Testing & Debugging

This document covers testing, debugging, common gotchas, and tutorial resources.

## Frequent Errors & Debugging Reference

**CRITICAL:** When working with message routing, ALWAYS consult [`dev/FREQUENT_ERRORS.md`](../dev/FREQUENT_ERRORS.md) to avoid common mistakes.

### Top 3 Critical Patterns (Must Follow)

#### 1. Level Filter Transformation When Forwarding Messages
When forwarding messages between relay nodes, **ALWAYS transform the level filter** to include the Self bit:
```csharp
var forwardedMessage = message.Copy();
forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndChildren;
node.MmInvoke(forwardedMessage);
```
**Why:** Target responders register with `Self` (0x01). Without transformation, bitwise AND check fails: `(Siblings & Self) = 0` → rejected.

**Exception:** Recursive routing (Descendants/Ancestors) uses `MmLevelFilter.Self` to prevent double-delivery.

#### 2. Routing Table Registration for Runtime Hierarchies
`transform.SetParent()` ONLY updates Unity's Transform hierarchy. MercuryMessaging requires **explicit routing table registration**:
```csharp
child.transform.SetParent(parent.transform);
var parentRelay = parent.GetComponent<MmRelayNode>();
var childRelay = child.GetComponent<MmRelayNode>();
parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
childRelay.AddParent(parentRelay);
```

#### 3. Runtime Component Registration
Adding responder components at runtime requires **explicit refresh**:
```csharp
var responder = gameObject.AddComponent<MyResponder>();
gameObject.GetComponent<MmRelayNode>().MmRefreshResponders();
yield return null; // Extra frame for safety
```

### Quick Debugging Checklist

**Message not reaching responder?**
- [ ] Routing table registered? (`MmAddToRoutingTable` called?)
- [ ] Level filter includes Self bit? (Transformed to `SelfAndChildren`?)
- [ ] Responder refreshed after runtime addition? (`MmRefreshResponders` called?)
- [ ] Tag matching correct? (Check `TagCheckEnabled` and tag bits)

**See [`dev/FREQUENT_ERRORS.md`](../dev/FREQUENT_ERRORS.md) for complete bug reference, code patterns, and debugging guides.**

---

## Development Notes

### Important Implementation Details

1. **MmRelayNode Registration**
   - Responders automatically register with relay nodes in their hierarchy
   - `RegisterAwakenedResponder()` called during Awake
   - `UnRegisterResponder()` called on Destroy

2. **Network Considerations**
   - Host machines run as both Client + Server
   - Use `IsDeserialized` flag to detect network messages
   - `FlipNetworkFlagOnSend` prevents deep network propagation
   - Override `AllowNetworkPropagationLocally` for custom behavior

3. **Performance**
   - `doNotModifyRoutingTable` flag prevents modification during iteration
   - Queue system for adding responders during message processing
   - Message history tracking can be disabled for production

4. **Hierarchy Changes**
   - Call `RefreshParents()` if GameObject hierarchy changes at runtime
   - Parent-child relationships are cached for performance
   - Circular dependencies are prevented

5. **Custom Methods**
   - Use method values > 1000 for custom application-specific methods
   - Handle custom methods by overriding `MmInvoke(MmMessage)` directly

### Common Gotchas

1. **Tag Checking**: Remember to disable `TagCheckEnabled` if not using tags
2. **Level Filters**: Default is `SelfAndChildren`, not `Self` alone
3. **Active Filter**: Inactive GameObjects ignored by default (use `MmActiveFilter.All` to include)
4. **FSM Selection**: `SelectedFilter.Selected` only works with `MmRelaySwitchNode`
5. **Network Doubles**: Host receives messages twice (local + network) - check `IsDeserialized`

### Debugging Features

MmRelayNode includes visual debugging:
- Message history tracking (`messageInList`, `messageOutList`)
- Visual node connections with colors
- Signal animation for message flow
- Integration with EPOOutline for highlights
- ALINE path drawing for message paths

Enable/disable in MmLogger:
```csharp
MmLogger.logFramework = true;  // Framework logging
MmLogger.logResponder = true;  // Responder logging
MmLogger.logApplication = true; // Application logging
MmLogger.logNetwork = true;    // Network logging
```

---

## Running Tests

The project uses Unity Test Framework for automated testing. Quick Win optimizations (QW-1, QW-2, QW-4) have comprehensive test coverage.

**Location:** `Assets/MercuryMessaging/Tests/`

**Test Files:**
- `CircularBufferTests.cs` - CircularBuffer implementation tests (30+ tests)
- `CircularBufferMemoryTests.cs` - QW-4 memory stability validation (6 tests)
- `HopLimitValidationTests.cs` - QW-1 hop limit enforcement (6 tests)
- `CycleDetectionValidationTests.cs` - QW-1 cycle detection (6 tests)
- `LazyCopyValidationTests.cs` - QW-2 lazy copying optimization (7 tests)

**How to Run:**
1. Open Unity Editor
2. Window > General > Test Runner
3. Select **PlayMode** tab (most tests require runtime context)
4. Click **Run All** to execute all tests
5. Tests should pass with green checkmarks

**Test Coverage:**
- **QW-1 Hop Limits:** Validates messages stop after configured hop count in deep hierarchies
- **QW-1 Cycle Detection:** Validates VisitedNodes tracking prevents infinite loops
- **QW-2 Lazy Copying:** Validates single-direction reuses messages, multi-direction creates necessary copies
- **QW-4 CircularBuffer:** Validates bounded memory footprint over high message volumes (10K+ messages)

**Running from Command Line (CI/CD):**
```bash
# Run PlayMode tests in batch mode
Unity.exe -runTests -batchmode -projectPath . \
  -testResults ./test-results.xml \
  -testPlatform PlayMode
```

---

## Tutorial Scenes

### SimpleScene
`Assets/MercuryMessaging/Tutorials/SimpleScene/`

Basic light switch example demonstrating:
- Message passing between components
- GUI interaction with responders
- VR hand controller integration
- Performance comparison with traditional Unity methods

**Key Scripts:**
- `LightSwitchHandler.cs` - Updates material color based on messages
- `LightSwitchResponder.cs` - Handles switch state messages
- `LightGUIHandler.cs` / `LightGUIResponder.cs` - GUI interaction
- `HandController.cs` - VR hand interaction demo

### Tutorial 1-5
`Assets/MercuryMessaging/Tutorials/Tutorial1-5/`

Progressive tutorial series covering:
- Tutorial 1: Basic message sending
- Tutorial 2: Hierarchy setup (with custom materials)
- Tutorial 3: Filtering and tags (with custom materials)
- Tutorial 4: State machines (with scripts)
- Tutorial 5: Advanced patterns (with scripts)

### Demo Scene
`Assets/MercuryMessaging/Demo/TrafficLights.unity`

Traffic light simulation demonstrating real-world usage.

---

## Additional Testing Documentation

For performance testing details:

@../Assets/MercuryMessaging/Tests/Performance/README.md
