# Routing Optimization - Technical Context

**Last Updated:** 2025-11-21

---

## Current Session Summary (2025-11-21)

**Status:** Phase 2.1 Advanced Routing - IMPLEMENTED AND TESTED ✅

### What Was Completed This Session

1. **Foundation Classes (60h):**
   - ✅ MmRoutingOptions configuration class (280 lines)
   - ✅ MmMessageHistoryCache LRU implementation (320 lines)
   - ✅ Extended MmLevelFilter enum (5 new modes: Siblings, Cousins, Descendants, Ancestors, Custom)
   - ✅ Extended MmMetadataBlock with Options and ExplicitRoutePath fields
   - ✅ MessageHistoryCacheTests (30+ test cases)

2. **Routing Logic Implementation (56h):**
   - ✅ HandleAdvancedRouting() - Main dispatcher for advanced filters
   - ✅ CollectSiblings() - Find same-parent nodes
   - ✅ CollectCousins() - Find parent's-sibling's-children
   - ✅ CollectDescendants() - Recursive all children
   - ✅ CollectAncestors() - Recursive all parents
   - ✅ RouteLateral() - Route to siblings/cousins
   - ✅ RouteRecursive() - Route to descendants/ancestors
   - ✅ ApplyCustomFilter() - Predicate-based filtering
   - ✅ AdvancedRoutingTests (9 test cases covering all modes)

3. **Critical Fixes Applied:**
   - ✅ Fixed MessageCounterResponder method signatures (compilation errors)
   - ✅ Fixed test hierarchy creation (explicit routing table registration)
   - ✅ **CRITICAL: Fixed level filter transformation in routing methods**

### Key Technical Discoveries

**MOST IMPORTANT - Level Filter Transformation Pattern:**
```csharp
// When forwarding messages to other relay nodes, MUST transform level filter:
var forwardedMessage = message.Copy();
forwardedMessage.MetadataBlock.LevelFilter = MmLevelFilterHelper.SelfAndChildren;
node.MmInvoke(forwardedMessage);
```

**Why This Is Critical:**
- Target nodes' responders are registered as `MmLevelFilter.Self` (done automatically)
- LevelCheck uses bitwise AND: `(messageFilter & responderLevel) > 0`
- Without transformation: `(Siblings & Self) = (0x08 & 0x01) = 0` → FALSE (rejected)
- With transformation: `(SelfAndChildren & Self) = (0x03 & 0x01) = 1` → TRUE (accepted)
- Standard routing code (lines 705-722) already does this for Parent/Child messages
- Advanced routing MUST follow the same pattern

**Programmatic Hierarchy Creation:**
- Setting Unity Transform parent does NOT register children in routing tables
- Must explicitly call: `parentRelay.MmAddToRoutingTable(child, MmLevelFilter.Child)`
- Must establish bidirectional relationship: `childRelay.AddParent(parentRelay)`
- Critical for tests creating GameObjects with `new GameObject()` at runtime
- Scene hierarchies and prefabs work automatically via MmRelayNode.Awake()

### Commits Made This Session

1. `d64d81f6` - feat: Add Phase 2.1 Advanced Message Routing foundation
2. `eb840ae9` - feat: Implement Phase 2.1 Advanced Routing logic
3. `db8dc342` - fix: Correct method signatures in MessageCounterResponder
4. `5cacfa45` - fix: Properly register child relay nodes in test hierarchy
5. `7dd86891` - fix: Transform level filters in advanced routing methods

### Files Modified

**Created:**
- Assets/MercuryMessaging/Protocol/MmRoutingOptions.cs
- Assets/MercuryMessaging/Support/Data/MmMessageHistoryCache.cs
- Assets/MercuryMessaging/Tests/MessageHistoryCacheTests.cs
- Assets/MercuryMessaging/Tests/AdvancedRoutingTests.cs

**Modified:**
- Assets/MercuryMessaging/Protocol/MmLevelFilter.cs (extended enum, added helpers)
- Assets/MercuryMessaging/Protocol/MmMetadataBlock.cs (added Options, ExplicitRoutePath)
- Assets/MercuryMessaging/Protocol/MmRelayNode.cs (added ~300 lines of routing methods)

### Test Status

**All Tests Passing:** 159/159 (100%) ✅
- MessageHistoryCacheTests: 30+ tests (O(1) operations, time-based eviction)
- AdvancedRoutingTests: 9 tests (all routing modes validated)

---

## Technical Overview

This document provides the technical architecture and design decisions for Mercury's routing optimization initiative, combining advanced message routing (Phase 2.1) and routing table optimization (Phase 3.1).

---

## Current State Analysis

### Existing Architecture

**MmRelayNode.cs** (1422 lines) is the central message router with:
- `MmRoutingTable` - List-based collection of responders
- Hierarchical parent-child relationships via Transform hierarchy
- Level filters: Self, Child, Parent, SelfAndChildren, SelfAndBidirectional
- Tag-based filtering (8-bit flags)
- Network message handling

**Current Limitations:**
1. **Cannot route to siblings** - No same-parent node communication
2. **Cannot route to cousins** - No parent's-sibling's-children communication
3. **"All" converts to "self-and-parents"** - Propagation restrictions
4. **Generic routing table** - No optimization for topology patterns
5. **No message history** - Can't prevent circular routing reliably
6. **Circular prevention too restrictive** - Blocks valid complex graphs

### Performance Characteristics (Current)

- **Simple routing:** ~0.00006ms (competitive with Unity Events at 0.00005ms)
- **Hierarchical traversal:** O(n) where n = number of responders
- **Memory per node:** ~200-300 bytes (MonoBehaviour + routing table)
- **Scales to:** ~1000 nodes before noticeable slowdown

---

## Architecture Design

### Phase 2.1: Advanced Message Routing

#### 1. Message History Tracking System

```csharp
public class MmRoutingOptions {
    public bool EnableHistoryTracking = false;
    public int HistoryCacheSizeMs = 100;
    public int MaxRoutingHops = 50;
    public bool AllowLateralRouting = false;
    public Func<MmRelayNode, bool> CustomFilter = null;
}

public class MmMetadataBlock {
    // Existing fields...
    public List<int> VisitedNodeIds;  // Track visited nodes
    public MmRoutingOptions Options;
    public string ExplicitRoutePath;  // "parent/sibling/child"
}
```

**Design Rationale:**
- LRU cache keeps memory bounded (default 100ms = ~1000 messages at 10K msg/sec)
- Message IDs track visited nodes to detect cycles
- Configurable hop count prevents runaway propagation
- Performance impact < 5% verified through benchmarking

**Implementation Strategy:**
- Use `GetInstanceID()` for node tracking (avoids GC)
- Cache cleanup on timer (background coroutine)
- Option to disable for production if not needed

#### 2. Extended Level Filters

```csharp
public enum MmLevelFilter {
    Self = 0,
    Child = 1,
    Parent = 2,
    SelfAndChildren = 3,
    SelfAndBidirectional = 4,
    // NEW:
    Siblings = 5,              // Same parent nodes
    Cousins = 6,               // Parent's siblings' children
    Descendants = 7,           // Recursive all children
    Ancestors = 8,             // Recursive all parents
    Custom = 9                 // User-defined predicate
}
```

**Routing Logic:**

**Siblings:**
1. Get current node's parent
2. Iterate parent's children (excluding self)
3. Apply remaining filters (active, tag, etc.)

**Cousins:**
1. Get current node's parent
2. Get parent's siblings (grandparent's children)
3. For each uncle/aunt, get their children
4. Apply remaining filters

**Custom:**
```csharp
relay.MmInvoke(message, new MmMetadataBlock {
    Level = MmLevelFilter.Custom,
    Options = new MmRoutingOptions {
        CustomFilter = (node) => node.tag == "Player" && node.activeInHierarchy
    }
});
```

#### 3. Path Specification System

**Grammar:**
```
path := segment ('/' segment)*
segment := 'parent' | 'sibling' | 'child' | 'self' | 'ancestor' | 'descendant'
```

**Examples:**
```csharp
// Notify sibling's child
relay.MmInvokeWithPath("parent/sibling/child", message);

// Complex path: up to grandparent, then down to cousin
relay.MmInvokeWithPath("parent/parent/child/child", message);
```

**Parser Design:**
- Recursive descent parser
- Compile to execution plan (cache for performance)
- Validate at runtime with clear error messages
- Support wildcards: "parent/*/child" (all siblings' children)

---

### Phase 3.1: Routing Table Optimization

#### 1. Routing Table Interface

```csharp
public interface IMmRoutingTable {
    void Add(MmRoutingTableItem item);
    void Remove(MmRoutingTableItem item);
    IEnumerable<MmRoutingTableItem> GetRecipients(MmMessage message);
    void InvalidateCache();
    RoutingStatistics GetStatistics();
}

public class MmRoutingTableItem {
    public IMmResponder Responder;
    public MmLevelFilter Level;
    public MmTag Tag;
    public bool IsActive;
    // Cached data for optimization
    public int NodeId;
    public Transform Transform;
}
```

**Design Rationale:**
- Interface allows swapping implementations
- Statistics support profiling and debugging
- Cache invalidation triggered by hierarchy changes
- Items store cached data to avoid repeated lookups

#### 2. Specialized Implementations

**A. FlatNetworkRoutingTable (O(1) lookup)**

Use case: Flat networks with many nodes at same level (UI systems, peer-to-peer)

```csharp
public class FlatNetworkRoutingTable : IMmRoutingTable {
    private Dictionary<int, MmRoutingTableItem> _nodeMap;
    private Dictionary<MmTag, List<MmRoutingTableItem>> _tagIndex;

    public IEnumerable<MmRoutingTableItem> GetRecipients(MmMessage message) {
        // Direct hash lookup by tag or node ID
        if (message.MetadataBlock.Tag != MmTag.Everything) {
            return _tagIndex[message.MetadataBlock.Tag];
        }
        return _nodeMap.Values;
    }
}
```

**Performance:** O(1) lookup, best for < 3 levels deep, high branching factor

**B. HierarchicalRoutingTable (O(log n) traversal)**

Use case: Deep hierarchies with moderate branching (scene graphs, org charts)

```csharp
public class HierarchicalRoutingTable : IMmRoutingTable {
    private TreeNode<MmRoutingTableItem> _root;
    private Dictionary<MmLevelFilter, Func<TreeNode, IEnumerable<TreeNode>>> _traversals;

    public IEnumerable<MmRoutingTableItem> GetRecipients(MmMessage message) {
        var traversal = _traversals[message.MetadataBlock.Level];
        var nodes = traversal(_root);
        return nodes.Select(n => n.Data).Where(ApplyFilters);
    }
}
```

**Performance:** O(log n) tree traversal, best for > 10 levels deep, tree structure

**C. MeshRoutingTable (Graph-based with caching)**

Use case: Complex mesh networks with cross-links (multiplayer, distributed systems)

```csharp
public class MeshRoutingTable : IMmRoutingTable {
    private Graph<MmRelayNode> _graph;
    private Dictionary<(int source, int target), List<MmRelayNode>> _pathCache;

    public IEnumerable<MmRoutingTableItem> GetRecipients(MmMessage message) {
        // Dijkstra's algorithm for shortest path
        // Cache results for repeated queries
        var key = (message.SourceNodeId, message.TargetNodeId);
        if (!_pathCache.ContainsKey(key)) {
            _pathCache[key] = ComputeShortestPath(key.source, key.target);
        }
        return _pathCache[key].Select(node => node.RoutingTableItem);
    }
}
```

**Performance:** O(E log V) path computation, O(1) cached lookup, best for complex graphs

#### 3. Topology Analyzer

```csharp
public class MmTopologyAnalyzer {
    public TopologyMetrics Analyze(MmRelayNode root) {
        return new TopologyMetrics {
            MaxDepth = ComputeMaxDepth(root),
            AvgBranching = ComputeAvgBranching(root),
            IsTree = CheckIfTree(root),
            CrossLinkCount = CountCrossLinks(root),
            TotalNodes = CountNodes(root)
        };
    }

    public IMmRoutingTable CreateOptimal(MmRelayNode root) {
        var metrics = Analyze(root);

        // Heuristics:
        if (metrics.MaxDepth <= 2 && metrics.AvgBranching < 5)
            return new FlatNetworkRoutingTable();
        else if (metrics.MaxDepth > 10 && metrics.IsTree)
            return new HierarchicalRoutingTable();
        else
            return new MeshRoutingTable();
    }
}
```

**Auto-Selection Heuristics:**
- **Flat:** Depth ≤ 2, Branching < 5, Node count < 1000
- **Hierarchical:** Depth > 10, Is tree structure, Node count > 100
- **Mesh:** Has cross-links, Complex connectivity, Node count > 500
- **Default:** Mesh (most general, handles all cases)

#### 4. Routing Path Caching

```csharp
public class MmRoutingCache {
    private LRUCache<RouteCacheKey, List<MmRoutingTableItem>> _cache;

    public List<MmRoutingTableItem> GetOrCompute(
        MmMessage message,
        Func<List<MmRoutingTableItem>> computeFunc)
    {
        var key = new RouteCacheKey(
            message.method,
            message.MetadataBlock.Level,
            message.MetadataBlock.Tag
        );

        if (!_cache.TryGet(key, out var result)) {
            result = computeFunc();
            _cache.Add(key, result);
        }
        return result;
    }

    public void InvalidateAll() {
        _cache.Clear();
    }
}
```

**Cache Invalidation Triggers:**
- GameObject hierarchy changes (parent/child modifications)
- Responder added/removed from routing table
- Tag changes on responders
- Manual invalidation via API

---

## Design Decisions

### Decision 1: Message History vs. Hop Count

**Options:**
A. Track full message path (all visited nodes)
B. Only count hops with max limit
C. Hybrid: track recent messages in time window

**Decision:** C (Hybrid approach)

**Rationale:**
- Full path tracking too expensive for memory
- Hop count alone can't detect all circular patterns
- Time-windowed LRU cache balances performance and safety
- Configurable window allows tuning per application

### Decision 2: Routing Table Selection

**Options:**
A. Manual selection by developer
B. Automatic detection via topology analysis
C. Both (auto with manual override)

**Decision:** C (Auto with override)

**Rationale:**
- Most developers want "it just works" (auto-detection)
- Power users may have specific performance needs (manual override)
- Topology can change at runtime (support hot-swapping)
- Provides best of both worlds

### Decision 3: Path Specification Syntax

**Options:**
A. Programmatic (method chaining): `relay.ToParent().ToSibling().ToChild()`
B. String-based: `"parent/sibling/child"`
C. Enum-based: Path enum with predefined patterns

**Decision:** B (String-based)

**Rationale:**
- More concise than method chaining
- Easier to serialize/deserialize (save to file, network)
- Familiar to developers (filesystem-like paths)
- Can be parsed and validated at runtime
- Enum-based too limiting for complex patterns

### Decision 4: Backward Compatibility Strategy

**Approach:**
1. All new features are opt-in (default to legacy behavior)
2. Deprecation warnings for 2 versions before removal
3. Compatibility layer wraps old API
4. Migration scripts for upgrading

**Example:**
```csharp
// Old API still works
relay.MmInvoke(MmMethod.SetActive, true, MmMetadataBlock.Default);

// New API available
relay.MmInvoke(MmMethod.SetActive, true, new MmMetadataBlock {
    Level = MmLevelFilter.Siblings,  // NEW
    Options = new MmRoutingOptions { EnableHistoryTracking = true }  // NEW
});
```

---

## Implementation Notes

### Performance Profiling Hooks

```csharp
public class MmRoutingProfiler {
    public static bool Enabled = false;

    public static void BeginSample(string sampleName) {
        if (Enabled) Profiler.BeginSample(sampleName);
    }

    public static void EndSample() {
        if (Enabled) Profiler.EndSample();
    }
}

// Usage in MmRelayNode
MmRoutingProfiler.BeginSample("MmRelayNode.MmInvoke");
// ... routing logic ...
MmRoutingProfiler.EndSample();
```

### Testing Strategy

**Unit Tests (90%+ coverage):**
- Each routing table implementation independently
- Topology analyzer with various graph shapes
- Path parser with valid and invalid inputs
- Message history tracking edge cases

**Integration Tests:**
- Routing across complex hierarchies
- Performance benchmarks at scale (100, 1K, 10K nodes)
- Memory leak detection
- Cache invalidation correctness

**Regression Tests:**
- All existing functionality still works
- Performance doesn't degrade for legacy patterns
- Backward compatibility verified

### Error Handling

**Invalid Path Specification:**
```csharp
try {
    relay.MmInvokeWithPath("parent/invalid/child", message);
} catch (MmInvalidPathException e) {
    Debug.LogError($"Invalid path: {e.Message}");
    // Clear error message: "Segment 'invalid' not recognized.
    // Valid segments: parent, child, sibling, self, ancestor, descendant"
}
```

**Circular Routing Detection:**
```csharp
if (message.MetadataBlock.VisitedNodeIds.Contains(currentNodeId)) {
    MmLogger.LogWarning($"Circular routing detected at node {currentNodeId}.
                         Path: {string.Join(" -> ", visitedNodeIds)}");
    return; // Stop propagation
}
```

---

## Migration Path

### For Existing Code

**Step 1:** No changes required - existing code continues to work

**Step 2:** Opt-in to new features gradually
```csharp
// Enable history tracking for complex graphs
relay.MmInvoke(message, new MmMetadataBlock {
    Options = new MmRoutingOptions { EnableHistoryTracking = true }
});
```

**Step 3:** Optimize routing tables for your topology
```csharp
// Manual override for specific optimization
var flatTable = new FlatNetworkRoutingTable();
relay.SetRoutingTable(flatTable);

// Or use auto-detection
var optimalTable = MmTopologyAnalyzer.CreateOptimal(relay);
relay.SetRoutingTable(optimalTable);
```

---

## Open Questions

1. **Should path specification support regex patterns?**
   - Complexity vs. power tradeoff
   - Could support later in 2.1

2. **How to handle routing table hot-swapping at runtime?**
   - Need to migrate items without message loss
   - May require double-buffering

3. **Should topology analysis run automatically on hierarchy changes?**
   - Could be expensive for large scenes
   - Maybe only on manual trigger or editor-time

---

## References

### Key Files
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs` - Core router (1422 lines)
- `Assets/MercuryMessaging/Protocol/MmMetadataBlock.cs` - Message routing metadata
- `Assets/MercuryMessaging/Protocol/MmRoutingTable.cs` - Current routing table
- `Assets/MercuryMessaging/Protocol/MmLevelFilter.cs` - Direction filters

### Related Documents
- Master Plan: Phase 2.1 (Advanced Routing) and Phase 3.1 (Table Optimization)
- Tasks Checklist: Detailed implementation tasks
- Performance Benchmarks: Current baseline metrics

---

**Document Version:** 1.0
**Last Updated:** 2025-11-18
**Owner:** Routing Optimization Team
