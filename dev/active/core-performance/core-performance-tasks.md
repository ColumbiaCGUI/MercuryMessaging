# Core Performance Tasks

**Last Updated:** 2025-12-03
**Total Effort:** ~26h
**Status:** Planning Complete

---

## D1: Remove SerialExecutionQueue Dead Code (15 min) ✅ COMPLETE

**Completed:** 2025-12-03
**Lines Removed:** 25

### Goal
Remove experimental code that was never completed and takes up ~50 bytes per relay node.

### Implementation

**File:** `Assets/MercuryMessaging/Protocol/Nodes/MmRelayNode.cs`

Delete the following:

1. **Field declaration (~line 130):**
```csharp
// DELETE
protected Queue<KeyValuePair<MmMessageType, MmMessage>> SerialExecutionQueue =
    new Queue<KeyValuePair<MmMessageType, MmMessage>>();
```

2. **Flag declaration (~line 210):**
```csharp
// DELETE
private bool serialExecution = false;
```

3. **Commented code block (~lines 1018-1027):**
```csharp
// DELETE entire commented block
//if (serialExecution)
//{
//    if (SerialExecutionQueue.Count != 0)
//    {
//        MmLogger.LogFramework("%%%%%%%%%%%Dequeueing%%%%%%%%%");
//        KeyValuePair<MmMessageType, MmMessage> DequeuedMessage = SerialExecutionQueue.Dequeue();
//        MmInvoke(DequeuedMessage.Key, DequeuedMessage.Value);
//    }
//    _executing = false;
//}
```

### Testing
- [x] Verify no compilation errors after removal
- [x] Run existing test suite - no regressions (EditMode: 0 failures)
- [x] Verify `MmRespondersToAdd` queue still works (responder-during-dispatch scenario)

### Note
Future async/thread-safe messaging will use `ConcurrentQueue<T>`, Job System, or proper async/await - not this incomplete implementation.

---

## E1-E3: Handled Flag Early Termination (1-2h) ✅ COMPLETE

**Completed:** 2025-12-03

### Goal
Add WPF-style `Handled` flag to stop message propagation after a handler consumes the message.

### E1: Add Handled Property to MmMessage (15 min)

**File:** `Assets/MercuryMessaging/Protocol/Message/MmMessage.cs`

```csharp
/// <summary>
/// When set to true, stops message propagation to remaining responders.
/// Useful for event-style messages where only one handler should respond.
/// </summary>
public bool Handled { get; set; } = false;
```

### E2: Check Handled Flag in Routing Loop (30 min)

**File:** `Assets/MercuryMessaging/Protocol/Nodes/MmRelayNode.cs`

In the main routing loop (around line 1000-1015), add check:

```csharp
foreach (var routingTableItem in GetMmRoutingTableItems(...))
{
    // Early termination if message was handled
    if (message.Handled) break;

    if (ResponderCheck(...))
    {
        if (PerformanceMode)
            routingTableItem.Responder.MmInvoke(message);
        else
            routingTableItem.Responder.MmInvoke(message);
    }
}
```

### E3: Optional - Add ReceiveHandledMessages Parameter (1h)

For handlers that need to see messages even if already handled (like WPF's `handledEventsToo`):

**File:** `Assets/MercuryMessaging/Protocol/Responders/MmResponder.cs`

```csharp
/// <summary>
/// When true, this responder receives messages even if Handled=true.
/// Default: false (responder skipped after message is handled).
/// </summary>
public bool ReceiveHandledMessages = false;
```

Update routing check:
```csharp
if (message.Handled && !routingTableItem.Responder.ReceiveHandledMessages)
    continue;
```

### Testing
**File:** `Assets/MercuryMessaging/Tests/Protocol/Core/HandledFlagTests.cs`

- [x] Test default `Handled = false` (no change to existing behavior)
- [x] Test `Handled = true` stops propagation to remaining responders
- [x] Test `Handled` flag resets for new messages (not polluted across messages)
- [x] Test with different routing patterns (ToChildren)
- [x] Test ReceiveHandledMessages=true allows receiving handled messages
- [x] Test Handled flag preserved in message copies

---

## P1-P3: MmRelaySwitchNode Caching (4h)

### Goal
Make MmRelaySwitchNode more performant for large FSMs (15+ states).

### Critical Files
- `Assets/MercuryMessaging/Protocol/Nodes/MmRelaySwitchNode.cs`
- `Assets/MercuryMessaging/Protocol/FSM/FiniteStateMachine.cs`

### P1: Cache Current State for SelectedCheck (High Impact)

**Problem:** `SelectedCheck()` is called per-responder during message routing:
```csharp
responder.MmGameObject == RespondersFSM.Current.Responder.MmGameObject
```

**Solution:** Cache the current state's GameObject when state changes:
```csharp
private GameObject _currentStateGameObject;

private void OnStateChanged()
{
    _currentStateGameObject = RespondersFSM.Current?.Responder?.MmGameObject;
}

protected override bool SelectedCheck(MmSelectedFilter selectedFilter, IMmResponder responder)
{
    return selectedFilter == MmSelectedFilter.All
           || responder.MmGameObject == _currentStateGameObject;
}
```

### P2: Remove LINQ Allocation in FSM Build

**Problem:** `Awake()` and `RebuildFSM()` use `.Where().ToList()` which allocates.

**Solution:** Use non-allocating loop with reusable list:
```csharp
private readonly List<MmRoutingTableItem> _fsmStateBuffer = new List<MmRoutingTableItem>();

private List<MmRoutingTableItem> GetChildRelayNodes()
{
    _fsmStateBuffer.Clear();
    foreach (var item in RoutingTable)
    {
        if (item.Responder is MmRelayNode && item.Level == MmLevelFilter.Child)
            _fsmStateBuffer.Add(item);
    }
    return _fsmStateBuffer;
}
```

### P3: Cache Current/CurrentName Properties

**Problem:** Every access to `Current` calls `GetRelayNode()`.

**Solution:** Cache on state change, invalidate on rebuild:
```csharp
private MmRelayNode _cachedCurrent;
private string _cachedCurrentName;

public MmRelayNode Current => _cachedCurrent;
public string CurrentName => _cachedCurrentName;

private void UpdateCurrentCache()
{
    var currentItem = RespondersFSM.Current;
    _cachedCurrent = currentItem?.Responder?.GetRelayNode();
    _cachedCurrentName = currentItem?.Name;
    _currentStateGameObject = currentItem?.Responder?.MmGameObject;
}
```

### Testing
**File:** `Assets/MercuryMessaging/Tests/MmRelaySwitchNodePerformanceTests.cs`

- [ ] Test cache invalidation on state change
- [ ] Test cache invalidation on FSM rebuild
- [ ] Test SelectedCheck performance with 15+ states
- [ ] Test correct state returned after rapid state changes
- [ ] Test null handling (no current state)

---

## Q1-Q4: MmRoutingChecks Consolidation (4h)

### Goal
Eliminate MmQuickNode by adding granular routing check controls to MmRelayNode.

### Critical Files
- `Assets/MercuryMessaging/Protocol/Nodes/MmRelayNode.cs`
- `Assets/MercuryMessaging/Protocol/Nodes/MmQuickNode.cs` (DELETE after)

### Q1: Add MmRoutingChecks Flags Enum

**File:** `MmRelayNode.cs` (add near top)

```csharp
/// <summary>
/// Flags controlling which routing checks to perform during message dispatch.
/// Disable checks for performance at the cost of filtering capability.
/// </summary>
[Flags]
public enum MmRoutingChecks
{
    None = 0,
    Level = 1,      // Level filter (Child/Parent/Self)
    Active = 2,     // Active filter (active GameObjects only)
    Selected = 4,   // Selected filter (FSM state)
    Tag = 8,        // Tag filter
    Network = 16,   // Network filter
    All = Level | Active | Selected | Tag | Network
}
```

### Q2: Add EnabledChecks Field to MmRelayNode

```csharp
/// <summary>
/// Controls which routing checks are performed during message dispatch.
/// Default: All checks enabled. Disable specific checks for performance.
/// WARNING: Disabling checks means messages bypass those filters entirely.
/// </summary>
[Tooltip("Which routing checks to perform. Disable for performance at cost of filtering.")]
public MmRoutingChecks EnabledChecks = MmRoutingChecks.All;
```

### Q3: Modify ResponderCheck to Use EnabledChecks

**Location:** `ResponderCheck()` method (~line 1489)

```csharp
protected virtual bool ResponderCheck(MmLevelFilter levelFilter, MmActiveFilter activeFilter,
    MmSelectedFilter selectedFilter, MmNetworkFilter networkFilter,
    MmRoutingTableItem mmRoutingTableItem, MmMessage message)
{
    // Tag check (only if enabled)
    if ((EnabledChecks & MmRoutingChecks.Tag) != 0
        && HasTagCheckEnabledResponders
        && message.MetadataBlock.Tag != MmTagHelper.Everything)
    {
        if (!TagCheck(mmRoutingTableItem, message)) return false;
    }

    // Level check (only if enabled)
    if ((EnabledChecks & MmRoutingChecks.Level) != 0)
    {
        if ((levelFilter & mmRoutingTableItem.Level) == 0) return false;
    }

    // Active check (only if enabled)
    if ((EnabledChecks & MmRoutingChecks.Active) != 0 && activeFilter != MmActiveFilter.All)
    {
        if (!ActiveCheck(activeFilter, mmRoutingTableItem.Responder)) return false;
    }

    // Selected check (only if enabled)
    if ((EnabledChecks & MmRoutingChecks.Selected) != 0)
    {
        if (!SelectedCheck(selectedFilter, mmRoutingTableItem.Responder)) return false;
    }

    // Network check (only if enabled)
    if ((EnabledChecks & MmRoutingChecks.Network) != 0 && message.IsDeserialized)
    {
        if (!NetworkCheck(mmRoutingTableItem, message)) return false;
    }

    return true;
}
```

### Q4: Delete MmQuickNode.cs

After consolidation is complete and tested:
- Delete `Assets/MercuryMessaging/Protocol/Nodes/MmQuickNode.cs`

### Usage Examples

```csharp
// Full filtering (default, backward compatible)
relay.EnabledChecks = MmRoutingChecks.All;

// Skip tag checks only (common optimization)
relay.EnabledChecks = MmRoutingChecks.All & ~MmRoutingChecks.Tag;

// Skip ALL checks (equivalent to old MmQuickNode behavior)
relay.EnabledChecks = MmRoutingChecks.None;

// Only level filtering (fast broadcast)
relay.EnabledChecks = MmRoutingChecks.Level;
```

### Testing
**File:** `Assets/MercuryMessaging/Tests/MmRoutingChecksTests.cs`

- [ ] Test default `MmRoutingChecks.All` matches current behavior
- [ ] Test each check can be individually disabled
- [ ] Test `MmRoutingChecks.None` bypasses all filters
- [ ] Test flag combinations (e.g., Level | Tag only)
- [ ] Test messages reach correct responders with various configs
- [ ] Performance benchmark: measure improvement when checks disabled

---

## S1-S7: Serialization System Overhaul (16h)

### Goal
Replace legacy `IMmSerializable` (object[] based) with high-performance `IMmBinarySerializable` (direct binary, zero-allocation).

### Critical Files to Modify
- `Assets/MercuryMessaging/Task/IMmSerializable.cs` - DELETE
- `Assets/MercuryMessaging/Protocol/Message/MmMessageSerializable.cs` - Rewrite
- `Assets/MercuryMessaging/Protocol/Network/MmBinarySerializer.cs` - Optimize

### Files to Create
- `Assets/MercuryMessaging/Protocol/Network/IMmBinarySerializable.cs`
- `Assets/MercuryMessaging/Protocol/Network/MmWriter.cs`
- `Assets/MercuryMessaging/Protocol/Network/MmReader.cs`
- `Assets/MercuryMessaging/Protocol/Network/MmTypeRegistry.cs`

### S1: New IMmBinarySerializable Interface

```csharp
namespace MercuryMessaging.Network
{
    public interface IMmBinarySerializable
    {
        void WriteTo(MmWriter writer);
        void ReadFrom(MmReader reader);
    }
}
```

### S2: Pooled MmWriter (zero-allocation)

Uses `ArrayPool<byte>.Shared` for buffer pooling. Supports primitives, Unity types (Vector3, Quaternion, Color), nested objects, and collections.

### S3: Pooled MmReader

Matching reader for MmWriter output with same type support.

### S4: Type Registry for Polymorphic Deserialization

```csharp
public static class MmTypeRegistry
{
    public static void Register<T>(ushort typeId) where T : IMmBinarySerializable, new();
    public static IMmBinarySerializable Create(ushort typeId);
    public static ushort GetTypeId<T>() where T : IMmBinarySerializable;
}
```

### S5: Update MmBinarySerializer to Use Pooled Writer

```csharp
public static ArraySegment<byte> SerializePooled(MmMessage message, out MmWriter writer)
{
    writer = MmWriter.Create(256);
    // ... serialize header and payload
    return writer.GetBuffer();
}
```

### S6: Delete Legacy Files

- `Assets/MercuryMessaging/Task/IMmSerializable.cs`
- `Assets/MercuryMessaging/Protocol/Network/Backends/Pun2Backend.cs` (deprecated)
- `Assets/MercuryMessaging/Protocol/Network/MmNetworkResponderPhoton.cs` (deprecated)

### S7: Migrate Task Classes

Update to implement `IMmBinarySerializable`:
- `MmTaskInfo`
- `MmTransformTaskThreshold`
- `MmTransformationTaskInfo`
- `MmLookAtTaskInfo`

### Performance Comparison

| Metric | Old (IMmSerializable) | New (IMmBinarySerializable) |
|--------|----------------------|----------------------------|
| Allocations/message | 3-5 | 0 (steady state) |
| Boxing operations | Many (object[]) | 0 |
| Buffer reuse | No | Yes (ArrayPool) |
| Estimated speedup | - | 3-5x |

### Testing
**File:** `Assets/MercuryMessaging/Tests/MmBinarySerializerTests.cs` (extend existing)

- [ ] Test IMmBinarySerializable interface with custom types
- [ ] Test MmWriter/MmReader primitives (int, float, bool, string)
- [ ] Test Unity types (Vector3, Quaternion, Color)
- [ ] Test nested serializable objects
- [ ] Test null handling
- [ ] Test List<T> serialization
- [ ] Test MmTypeRegistry registration and lookup
- [ ] Memory test: verify zero allocations in steady state (ArrayPool reuse)
- [ ] Round-trip test: serialize then deserialize all message types
- [ ] Performance benchmark: compare old vs new serialization speed

---

## Test Standards

All tests follow project standards from `Documentation/TESTING.md`:

```csharp
[Test]
public void Feature_Scenario_ExpectedBehavior()
{
    // Arrange - create hierarchy programmatically
    GameObject root = new GameObject("TestRoot");
    MmRelayNode relay = root.AddComponent<MmRelayNode>();

    // Act
    relay.MmInvoke(MmMethod.Initialize);

    // Assert
    Assert.IsTrue(condition);

    // Cleanup
    Object.DestroyImmediate(root);
}
```

**Required:**
- Use Unity Test Framework (PlayMode or EditMode)
- Create GameObjects programmatically in [SetUp]
- Clean up in [TearDown] with Object.DestroyImmediate()

**Prohibited:**
- NO manual scene creation
- NO prefab dependencies
