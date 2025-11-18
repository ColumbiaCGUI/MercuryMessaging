# MercuryMessaging Framework Analysis - Task Checklist

**Last Updated:** 2025-11-18
**Status:** Analysis Complete - Ready for Implementation
**Total Quick Wins Effort:** 38-46 hours (1-2 weeks)

---

## Task Organization

Tasks organized by priority tier. Quick wins (Priority 1) provide immediate 20-30% performance improvement with minimal risk.

**Progress Tracking:**
- [ ] Not started
- [🔨] In progress
- [✅] Complete

---

## PRIORITY 1: QUICK WINS (38-46 hours, 1-2 weeks)

High impact, low risk, immediate returns. Start here.

---

### QW-1: Enable Message History Tracking + Hop Limits (8h)

**Goal:** Prevent infinite message loops in complex hierarchies

#### Subtasks:
- [ ] Uncomment message history code (`MmRelayNode.cs:560-606`) (2h)
- [ ] Add hop count field to MmMessage (1h)
  ```csharp
  public int HopCount = 0;
  public const int MAX_HOPS = 32;
  ```
- [ ] Implement hop limit checking in MmInvoke (2h)
  ```csharp
  if (message.HopCount >= MmMessage.MAX_HOPS) {
      MmLogger.LogFramework($"Max hops reached at {name}");
      return;
  }
  message.HopCount++;
  ```
- [ ] Add visited nodes tracking (HashSet) (2h)
- [ ] Write unit tests for circular detection (1h)

**Acceptance Criteria:**
- Circular loops detected and prevented
- Max hops configurable (default 32)
- Warning logged when limit hit
- No false positives (valid complex graphs work)
- Unit tests pass (circular graph, deep hierarchy, mesh)

**Dependencies:** None

**Risk:** LOW - Adds safeguards, doesn't change existing behavior

**Files Modified:**
- `Assets/MercuryMessaging/Protocol/MmMessage.cs`
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs:560-606, 868-928`

---

### QW-2: Implement Lazy Message Copying (12h)

**Goal:** Reduce 20-30% overhead from unnecessary message copies

#### Subtasks:
- [ ] Analyze current copy behavior (MmRelayNode.cs:850-853) (1h)
- [ ] Implement lazy copy logic (4h)
  ```csharp
  // Only copy if routing BOTH up AND down
  if (shouldSendUp && shouldSendDown) {
      var upMsg = message.Copy();
      upMsg.MetadataBlock = upwardMeta;
      SendMessageUpward(upMsg);

      var downMsg = message.Copy();
      downMsg.MetadataBlock = downwardMeta;
      SendMessageDownward(downMsg);
  } else if (shouldSendUp) {
      message.MetadataBlock = upwardMeta;
      SendMessageUpward(message);  // Reuse original
  } else if (shouldSendDown) {
      message.MetadataBlock = downwardMeta;
      SendMessageDownward(message);  // Reuse original
  }
  ```
- [ ] Add copy-on-write flag to MmMessage (2h)
- [ ] Document message ownership semantics (1h)
- [ ] Profile performance before/after (2h)
- [ ] Write unit tests for edge cases (2h)

**Acceptance Criteria:**
- Copies only created when necessary
- 20-30% faster routing (measured)
- All existing tests pass
- No message mutation bugs
- Clear documentation on ownership

**Dependencies:** None

**Risk:** MEDIUM - Changes message semantics, needs careful testing

**Files Modified:**
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs:850-853`
- `Assets/MercuryMessaging/Protocol/MmMessage.cs`

---

### QW-3: Add Filter Result Caching (8h)

**Goal:** Eliminate 40%+ slowdown at 100+ responders

#### Subtasks:
- [ ] Design cache key structure (1h)
  ```csharp
  struct CacheKey {
      MmLevelFilter level;
      MmActiveFilter active;
      MmTag tag;
      // Hash based on combination
  }
  ```
- [ ] Implement LRU cache for responder lists (4h)
  ```csharp
  private Dictionary<CacheKey, List<IMmResponder>> filterCache;
  private LinkedList<CacheKey> lruOrder;
  private const int MAX_CACHE_SIZE = 100;
  ```
- [ ] Add cache invalidation on add/remove responder (2h)
- [ ] Profile cache hit rate and performance gain (1h)

**Acceptance Criteria:**
- 40%+ speedup at 100+ responders
- Cache invalidates correctly on changes
- Memory bounded (LRU eviction)
- 80%+ cache hit rate in typical usage

**Dependencies:** None

**Risk:** LOW - Pure optimization, doesn't change behavior

**Files Modified:**
- `Assets/MercuryMessaging/Protocol/MmRoutingTable.cs`

---

### QW-4: Replace Unbounded History Lists (6h)

**Goal:** Eliminate memory leaks in long-running sessions

#### Subtasks:
- [ ] Implement CircularBuffer<T> class (3h)
  ```csharp
  public class CircularBuffer<T> {
      private T[] buffer;
      private int head, size, capacity;

      public void Add(T item) {
          buffer[head] = item;
          head = (head + 1) % capacity;
          if (size < capacity) size++;
      }
  }
  ```
- [ ] Replace messageInList/messageOutList (1h)
  ```csharp
  public CircularBuffer<string> messageInList =
      new CircularBuffer<string>(100);
  ```
- [ ] Remove manual truncation code (line 590-591) (0.5h)
- [ ] Test memory usage over 24 hours (1h)
- [ ] Make buffer size configurable (0.5h)

**Acceptance Criteria:**
- Fixed memory footprint (configurable size)
- No memory growth over time
- O(1) add operation (no more RemoveAt)
- Oldest messages automatically overwritten

**Dependencies:** None

**Risk:** LOW - Internal change, doesn't affect API

**Files Modified:**
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs:80-82, 590-591`
- `Assets/MercuryMessaging/Support/CircularBuffer.cs` (NEW)

---

### QW-5: Remove LINQ Allocations (4h)

**Goal:** Reduce GC pressure in hot paths

#### Subtasks:
- [ ] Find all LINQ usage in hot paths (1h)
  - MmRelayNode.cs:704 (RegisterAwakenedResponder)
  - MmRelayNode.cs:728 (RefreshParents)
  - MmRelayNode.cs:1191 (LevelFilterAdjust)
- [ ] Replace with foreach loops (2h)
  ```csharp
  // Before:
  List<MmResponder> responders = GetComponents<MmResponder>()
      .Where(x => !(x is MmRelayNode))
      .ToList();

  // After:
  var components = GetComponents<MmResponder>();
  List<MmResponder> responders = new List<MmResponder>(components.Length);
  foreach (var component in components) {
      if (!(component is MmRelayNode)) {
          responders.Add(component);
      }
  }
  ```
- [ ] Profile GC allocations before/after (1h)

**Acceptance Criteria:**
- Zero LINQ usage in MmRelayNode hot paths
- Measurable reduction in GC allocations
- All tests pass
- Code clarity maintained

**Dependencies:** None

**Risk:** LOW - Straightforward refactoring

**Files Modified:**
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs:704, 728, 1191`

---

### QW-6: Cleanup Technical Debt (6-8h, OPTIONAL)

**Goal:** Improve code maintainability

#### Subtasks:
- [ ] Remove or complete commented code (3h)
  - MmRelayNode.cs:560-606 (or enable via QW-1)
  - MmRelayNode.cs:810-824 (serial queue)
- [ ] Convert TODO comments to GitHub issues (1h)
  - MmRelaySwitchNode.cs:120
  - MmRelayNode.cs:805
- [ ] Extract debug visualization to separate class (3-4h)
  - MmRelayNode.cs:489-558 → MmRelayNodeDebugger.cs
  - Add #if UNITY_EDITOR guards
- [ ] Update documentation (1h)

**Acceptance Criteria:**
- Zero commented production code
- All TODOs tracked in issues
- Debug code optional and isolated
- Documentation up to date

**Dependencies:** QW-1 (if enabling history code)

**Risk:** LOW - Cleanup only

**Files Modified:**
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs`
- `Assets/MercuryMessaging/Protocol/MmRelaySwitchNode.cs`
- `Assets/MercuryMessaging/Support/Debug/MmRelayNodeDebugger.cs` (NEW, optional)

---

## PRIORITY 2: QUICK WIN VALIDATION (6-8 hours)

After implementing quick wins, validate improvements.

---

### QW-TEST-1: Performance Benchmarking (4h)

- [ ] Create benchmark scene with 10/100/1000 responders (1h)
- [ ] Measure baseline performance (1h)
  - Message routing latency
  - GC allocations per 1000 messages
  - Memory usage over time
- [ ] Measure post-optimization performance (1h)
- [ ] Document improvements (1h)

**Target Metrics:**
- 20-30% faster message routing
- 50%+ reduction in GC allocations
- Zero memory growth over 24 hours
- Zero infinite loop crashes

---

### QW-TEST-2: Integration Testing (2-4h)

- [ ] Test with existing scenes (1h)
  - TrafficLights demo
  - Tutorial scenes 1-5
  - User study Scenario1
- [ ] Test complex hierarchies (1h)
  - Deep nesting (20+ levels)
  - Wide fanout (100+ children)
  - Mesh patterns
- [ ] Test edge cases (1h)
  - Circular graphs
  - Dynamic add/remove
  - High message volumes
- [ ] Fix any regressions (1h contingency)

**Acceptance:** All existing functionality works, no new bugs

---

## PRIORITY 3: PLANNED IMPROVEMENTS (1,570 hours, 6-8 months)

Existing documented work. See individual task folders for details.

---

### routing-optimization (420h, CRITICAL)

**Location:** `dev/active/routing-optimization/`

- [ ] Phase 2.1: Advanced Message Routing (256h)
  - Extended level filters (Siblings, Cousins, Custom)
  - Path specification system
  - Message history with LRU cache

- [ ] Phase 3.1: Routing Table Optimization (276h)
  - IMmRoutingTable interface
  - FlatNetworkRoutingTable (O(1))
  - HierarchicalRoutingTable (O(log n))
  - MeshRoutingTable (graph + cache)
  - Topology analyzer

**First Task:** Create MmRoutingOptions class (4h)

---

### network-performance (500h, HIGH)

**Location:** `dev/active/network-performance/`

- [ ] Phase 2.2: Network Synchronization 2.0 (156h)
  - Delta state tracking
  - State delta serialization
  - Compression pipeline
  - Reliability tiers

- [ ] Phase 3.2: Message Processing Optimization (136h)
  - Priority-based queuing (4 tiers)
  - Message batching
  - Object pooling (zero GC)

**First Task:** Design state tracking architecture (8h)

---

### visual-composer (360h, MEDIUM)

**Location:** `dev/active/visual-composer/`

- [ ] Tool 1: Hierarchy Mirroring (36h)
- [ ] Tool 2: Network Templates (52h)
- [ ] Tool 3: Visual Composer (96h)
- [ ] Tool 4: Network Validator (48h)

**First Task:** Design UI architecture (8h)

---

### standard-library (290h, MEDIUM)

**Location:** `dev/active/standard-library/`

- [ ] Phase 1: Architecture & Versioning (32h)
- [ ] Phase 2: UI Messages (40h)
- [ ] Phase 3: AppState Messages (28h)
- [ ] Phase 4: Input Messages (36h)
- [ ] Phase 5: Task Messages (28h)

**First Task:** Design message library architecture (12h)

---

## PRIORITY 4: NEW OPPORTUNITIES (200-300 hours, 2-3 months)

Features identified but not yet documented.

---

### Advanced Filtering (20-30h)

- [ ] Component-based filtering (8h)
  ```csharp
  relay.MmInvoke(
      MmMethod.Message,
      "TakeDamage",
      filterByComponent: typeof(Health)
  );
  ```
- [ ] Custom filter predicates (8h)
  ```csharp
  relay.MmInvoke(
      MmMethod.Message,
      "Update",
      filterPredicate: r => r.Priority > 5
  );
  ```
- [ ] Extended tag system (beyond 8 flags) (4-8h)
- [ ] Testing and documentation (4-6h)

**Acceptance:** Flexible filtering for complex scenarios

---

### Developer Tools (50-80h)

- [ ] Runtime Performance Profiler (20-30h)
  - Message throughput monitoring
  - Bottleneck identification
  - Real-time visualization

- [ ] Message Recording/Playback (20-30h)
  - Record message sequences
  - Replay for testing
  - Export to file

- [ ] Load Testing Framework (10-20h)
  - Simulate high volumes
  - Stress test networks
  - Performance reports

**Acceptance:** Integrated developer experience

---

### State Management (30-50h)

- [ ] Schema Validation (10-15h)
  - Define message schemas
  - Runtime type checking
  - Validation error reporting

- [ ] Conflict Resolution (15-25h)
  - Last-write-wins (existing)
  - Server-authoritative (new)
  - CRDT support (new)
  - Operational transform (new)

- [ ] State Snapshots (5-10h)
  - Checkpoint state
  - Rollback capability
  - Persistence layer

**Acceptance:** Robust state management for complex applications

---

## Summary

**Total Tasks:** 16 quick wins + 4 planned improvements + 3 new opportunities

**Effort Breakdown:**
- Quick Wins (Priority 1): 38-46 hours
- Validation (Priority 2): 6-8 hours
- Planned Improvements (Priority 3): 1,570 hours
- New Opportunities (Priority 4): 200-300 hours
- **TOTAL:** 1,814-1,924 hours

**Critical Path:**
1. Quick wins (2 weeks) → Immediate 20-30% improvement
2. Routing optimization (11 weeks) → 3-5x speedup
3. Network performance (13 weeks) → 50-80% bandwidth reduction
4. Visual tools + library (16 weeks, parallel) → Developer experience

**Recommended Start:**
Begin with Priority 1 (Quick Wins) for immediate ROI, then proceed to routing-optimization (CRITICAL priority in existing plans).

---

**Document Version:** 1.0
**Last Updated:** 2025-11-18
**Maintained By:** Framework Team
