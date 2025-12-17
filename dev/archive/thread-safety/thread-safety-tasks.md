# Thread Safety Implementation Tasks

**Created:** 2025-11-20
**Status:** Implementation Checklist
**Approach:** Option A (Lock-Based) - 4-6 hours
**Related:** [README.md](README.md), [thread-safety-context.md](thread-safety-context.md)

---

## Overview

This document tracks the step-by-step implementation of thread-safe message processing using a lock-based approach. Tasks are organized into 4 phases with clear acceptance criteria.

**Total Estimated Effort:** 4-6 hours
- Phase 1: Add Lock (1-2 hours)
- Phase 2: Remove Flag (0.5 hours)
- Phase 3: Add Async API (1-2 hours)
- Phase 4: Testing & Documentation (1-2 hours)

---

## Phase 1: Add Lock (No Behavior Change)

**Goal:** Add lock without changing existing behavior

**Duration:** 1-2 hours

### Tasks

- [ ] **1.1 Add Lock Field to MmRelayNode**
  - Location: `Assets/MercuryMessaging/Protocol/MmRelayNode.cs` (near line 142)
  - Code: `private readonly object _routingTableLock = new object();`
  - Notes: Add after `doNotModifyRoutingTable` declaration

- [ ] **1.2 Wrap MmInvoke with Lock**
  - Location: `MmRelayNode.cs` (lines 630-760)
  - Wrap entire method body in `lock (_routingTableLock) { ... }`
  - Keep existing `doNotModifyRoutingTable` flag (redundant but safe)
  - Preserve all existing behavior

- [ ] **1.3 Wrap MmAddToRoutingTable with Lock**
  - Location: `MmRelayNode.cs` (line ~400)
  - Wrap method body in `lock (_routingTableLock) { ... }`
  - Keep existing `if (doNotModifyRoutingTable)` check
  - Preserve queue mechanism

- [ ] **1.4 Wrap MmRefreshResponders with Lock**
  - Location: `MmRelayNode.cs` (line ~460)
  - Wrap method body in `lock (_routingTableLock) { ... }`
  - Ensure nested calls work correctly

- [ ] **1.5 Wrap UnRegisterResponder with Lock**
  - Location: `MmRelayNode.cs` (line ~430)
  - Wrap routing table removal in lock
  - Maintain consistency

### Acceptance Criteria

- ✅ All existing unit tests pass (117/117)
- ✅ No compilation errors
- ✅ No behavior changes (tests verify correctness)
- ✅ Lock field is `readonly` (immutable reference)
- ✅ All routing table access is within lock

### Code Template

```csharp
// Add field (near line 142)
/// <summary>
/// Lock object for thread-safe routing table access
/// </summary>
private readonly object _routingTableLock = new object();

// Wrap MmInvoke (lines 630-760)
public override void MmInvoke(MmMessage message)
{
    lock (_routingTableLock)
    {
        doNotModifyRoutingTable = true;  // Keep existing flag

        // ... existing implementation ...

        doNotModifyRoutingTable = false;

        // Process queue
        while (MmRespondersToAdd.Count > 0)
        {
            var item = MmRespondersToAdd.Dequeue();
            MmAddToRoutingTable(item.Responder, item.Level);
        }
    }
}
```

---

## Phase 2: Remove Flag (Simplification)

**Goal:** Remove redundant `doNotModifyRoutingTable` flag

**Duration:** 0.5 hours

### Tasks

- [ ] **2.1 Remove Flag from MmInvoke**
  - Location: `MmRelayNode.cs` (lines 630, 760)
  - Remove `doNotModifyRoutingTable = true;`
  - Remove `doNotModifyRoutingTable = false;`
  - Lock provides protection now

- [ ] **2.2 Simplify MmAddToRoutingTable**
  - Location: `MmRelayNode.cs` (line ~414)
  - Remove `if (doNotModifyRoutingTable)` check
  - Keep queue mechanism for nested calls (still useful)
  - OR remove queue if direct add is acceptable

- [ ] **2.3 Remove Flag Declaration**
  - Location: `MmRelayNode.cs` (line 142)
  - Remove `public bool doNotModifyRoutingTable;`
  - Clean up XML comments

- [ ] **2.4 Update MmRespondersToAdd Usage**
  - Decide: Keep queue for nested calls OR remove entirely?
  - If keeping: Document that it handles nested registration
  - If removing: Ensure direct add works for all cases

### Acceptance Criteria

- ✅ All tests still pass (117/117)
- ✅ No references to `doNotModifyRoutingTable` remain
- ✅ Queue decision documented (keep or remove)
- ✅ Code is simpler and cleaner

### Decision Point: Queue Retention

**Option A: Keep Queue (Recommended)**
- Handles nested calls (responder registers another responder during message)
- Existing pattern, low risk
- Minimal code changes

**Option B: Remove Queue**
- Simpler code (fewer moving parts)
- Direct add is now safe with lock
- Requires testing nested scenarios

**Recommendation:** Keep queue for safety (Option A)

---

## Phase 3: Add Async API (Opt-In)

**Goal:** Add async/await support without breaking existing code

**Duration:** 1-2 hours

### Tasks

- [ ] **3.1 Create IMmAsyncResponder Interface**
  - Location: New file `Assets/MercuryMessaging/Protocol/IMmAsyncResponder.cs`
  - Extends `IMmResponder`
  - Adds `Task MmInvokeAsync(MmMessage message)` method

- [ ] **3.2 Implement MmInvokeAsync in MmRelayNode**
  - Location: `MmRelayNode.cs` (new method)
  - Snapshot responders within lock
  - Release lock before async operations
  - Handle both sync and async responders

- [ ] **3.3 Create MmAsyncBaseResponder Base Class**
  - Location: New file `Assets/MercuryMessaging/Protocol/MmAsyncBaseResponder.cs`
  - Extends `MmBaseResponder`
  - Implements `IMmAsyncResponder`
  - Provides async overloads for standard methods

- [ ] **3.4 Add Async Message Type Overloads**
  - Add `MmInvokeAsync(MmMethod method, value, metadata)` overloads
  - Support all standard message types (Bool, Int, Float, String, etc.)
  - Maintain sync API compatibility

### Acceptance Criteria

- ✅ Existing sync tests pass (no breaking changes)
- ✅ New async tests pass (see Phase 4)
- ✅ Lock held only during snapshot (not during await)
- ✅ Backward compatible (opt-in via interface)
- ✅ Both sync and async responders supported

### Code Template

```csharp
// IMmAsyncResponder.cs (new file)
using System.Threading.Tasks;

namespace MercuryMessaging
{
    /// <summary>
    /// Interface for responders supporting async message processing
    /// </summary>
    public interface IMmAsyncResponder : IMmResponder
    {
        /// <summary>
        /// Asynchronously process a message
        /// </summary>
        Task MmInvokeAsync(MmMessage message);
    }
}

// MmRelayNode.cs (new method)
/// <summary>
/// Asynchronously invoke message processing on filtered responders
/// </summary>
public async Task MmInvokeAsync(MmMessage message)
{
    // Cycle detection (same as sync)
    int nodeInstanceId = gameObject.GetInstanceID();
    if (message.VisitedNodes.Contains(nodeInstanceId))
        return;
    message.VisitedNodes.Add(nodeInstanceId);

    // Snapshot responders (lock held briefly)
    List<IMmResponder> respondersToInvoke;

    lock (_routingTableLock)
    {
        respondersToInvoke = RoutingTable
            .Where(item => ResponderCheck(
                message.MetadataBlock.LevelFilter,
                message.MetadataBlock.ActiveFilter,
                message.MetadataBlock.SelectedFilter,
                message.MetadataBlock.NetworkFilter,
                message.MetadataBlock.Tag,
                item))
            .Select(item => item.Responder)
            .ToList();
    }

    // Process asynchronously (lock released)
    foreach (var responder in respondersToInvoke)
    {
        if (responder is IMmAsyncResponder asyncResponder)
        {
            await asyncResponder.MmInvokeAsync(message);
        }
        else
        {
            responder.MmInvoke(message);  // Fallback to sync
        }
    }
}
```

---

## Phase 4: Testing & Documentation

**Goal:** Comprehensive testing and documentation updates

**Duration:** 1-2 hours

### Testing Tasks

- [ ] **4.1 Create ThreadSafetyTests.cs**
  - Location: New file `Assets/MercuryMessaging/Tests/ThreadSafetyTests.cs`
  - 10-15 tests covering concurrency scenarios
  - Follow existing test patterns (programmatic setup)

- [ ] **4.2 Concurrent Invocation Tests**
  - Test: 10 threads invoke simultaneously
  - Test: 100 messages from multiple threads
  - Assert: No collection modification exceptions

- [ ] **4.3 Concurrent Registration Tests**
  - Test: Register responders while messages processing
  - Test: 50 dynamic registrations during 100 messages
  - Assert: All responders registered correctly

- [ ] **4.4 Reentrancy Tests**
  - Test: Responder calls back into relay during message
  - Test: Nested MmInvoke calls (responder → relay → responder)
  - Assert: No deadlocks

- [ ] **4.5 Async/Await Tests**
  - Test: MmInvokeAsync with async responders
  - Test: Mixed sync and async responders
  - Test: Concurrent async operations
  - Assert: Messages delivered correctly

- [ ] **4.6 Stress Tests**
  - Test: 1000 messages from 10 threads
  - Test: 100 responders, 1000 messages
  - Assert: No failures, reasonable performance

- [ ] **4.7 Performance Regression Tests**
  - Benchmark: Single-threaded overhead < 5%
  - Benchmark: Lock acquisition time < 100ns
  - Benchmark: Multi-threaded throughput > 80%

### Documentation Tasks

- [ ] **4.8 Update CLAUDE.md**
  - Add "Thread Safety" section under Architecture
  - Document async/await support
  - Add IMmAsyncResponder usage examples
  - Note performance characteristics

- [ ] **4.9 Update MmRelayNode.cs XML Comments**
  - Document thread-safe behavior
  - Note lock usage and performance
  - Add `<threadsafety>` XML tag

- [ ] **4.10 Create Async Usage Examples**
  - Location: `Assets/MercuryMessaging/Examples/AsyncMessaging/`
  - Create example scene with async responders
  - Demonstrate async workflows
  - Show error handling patterns

- [ ] **4.11 Update TECHNICAL_DEBT.md**
  - Mark thread safety item as ✅ Complete
  - Document implementation approach (Option A)
  - Add performance results
  - Move to "Completed Items" section

### Acceptance Criteria

- ✅ All 127+ tests pass (117 existing + 10+ new)
- ✅ Performance tests show < 5% overhead
- ✅ Documentation updated with async examples
- ✅ TECHNICAL_DEBT.md reflects completion
- ✅ Code review completed (self or peer)

---

## Test Implementation Guide

### Test File Template

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for thread-safe message processing
    /// </summary>
    public class ThreadSafetyTests
    {
        private GameObject testRoot;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("TestRoot");
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null)
            {
                Object.DestroyImmediate(testRoot);
            }
        }

        [Test]
        public async Task ConcurrentInvoke_MultipleThreads_NoExceptions()
        {
            // Arrange
            var relay = testRoot.AddComponent<MmRelayNode>();
            var responder = testRoot.AddComponent<TestResponder>();
            relay.MmRefreshResponders();

            var message = new MmMessageBool { MmMethod = MmMethod.Initialize };

            // Act - 10 threads invoke simultaneously
            var tasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                tasks[i] = Task.Run(() => relay.MmInvoke(message));
            }

            await Task.WhenAll(tasks);

            // Assert - No exceptions thrown
            Assert.Pass();
        }

        [Test]
        public async Task ConcurrentRegistration_DuringInvoke_AllRegistered()
        {
            // Arrange
            var relay = testRoot.AddComponent<MmRelayNode>();

            // Pre-populate with 50 responders
            for (int i = 0; i < 50; i++)
            {
                var child = new GameObject($"Responder{i}");
                child.transform.SetParent(testRoot.transform);
                child.AddComponent<TestResponder>();
            }
            relay.MmRefreshResponders();

            var message = new MmMessageBool { MmMethod = MmMethod.Initialize };

            // Act - Invoke messages while adding responders
            var invokeTask = Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    relay.MmInvoke(message);
                    Thread.Sleep(1);
                }
            });

            var registerTask = Task.Run(() =>
            {
                for (int i = 0; i < 25; i++)
                {
                    var newChild = new GameObject($"DynamicResponder{i}");
                    newChild.transform.SetParent(testRoot.transform);
                    var responder = newChild.AddComponent<TestResponder>();
                    relay.RegisterAwakenedResponder(responder);
                    Thread.Sleep(2);
                }
            });

            await Task.WhenAll(invokeTask, registerTask);

            // Assert - All responders registered
            Assert.AreEqual(75, relay.RoutingTable.Count, "All responders should be registered");
        }

        // Helper responder for tests
        private class TestResponder : MmBaseResponder
        {
            public int InitializeCallCount = 0;

            protected override void ReceivedInitialize()
            {
                Interlocked.Increment(ref InitializeCallCount);
            }
        }
    }
}
```

---

## Review Checklist

Before marking complete, verify:

### Code Quality

- [ ] No compilation warnings
- [ ] No TODO comments remain
- [ ] XML documentation complete
- [ ] Code follows project style guide
- [ ] No magic numbers (use named constants)

### Thread Safety

- [ ] All routing table access within lock
- [ ] No lock held during async operations
- [ ] Reentrancy handled correctly
- [ ] No deadlock scenarios possible

### Performance

- [ ] Single-threaded overhead < 5%
- [ ] Lock acquisition time measured
- [ ] No performance regressions in existing tests
- [ ] Profiler markers added (Unity Profiler integration)

### Testing

- [ ] All existing tests pass (117+)
- [ ] New thread safety tests pass (10+)
- [ ] Stress tests pass (1000+ messages)
- [ ] Performance benchmarks meet targets

### Documentation

- [ ] CLAUDE.md updated with async examples
- [ ] MmRelayNode.cs XML comments updated
- [ ] TECHNICAL_DEBT.md marked complete
- [ ] Example scene created (optional but recommended)

---

## Rollback Plan

If issues arise during implementation:

### Phase 1 Rollback
- Remove lock statements
- Keep `doNotModifyRoutingTable` flag
- Restore original implementation
- All tests should still pass

### Phase 2 Rollback
- Re-add `doNotModifyRoutingTable` flag
- Restore if-checks in MmAddToRoutingTable
- Keep lock for safety

### Phase 3 Rollback
- Remove async API files
- Keep sync API unchanged
- No impact on existing code

**Note:** Each phase is designed to be reversible without breaking existing functionality.

---

## Known Risks & Mitigation

### Risk 1: Performance Degradation
**Impact:** Lock overhead > 5% in single-threaded scenarios
**Mitigation:**
- Benchmark early (Phase 1)
- If overhead too high, upgrade to ReaderWriterLockSlim (Option B)
- Add profiler markers for visibility

### Risk 2: Deadlock in Nested Calls
**Impact:** Responders calling back into relay cause deadlock
**Mitigation:**
- C# Monitor is reentrant by default (same thread can re-acquire)
- Test reentrancy scenarios explicitly (Phase 4.4)
- Document safe patterns

### Risk 3: Unity Main Thread Requirements
**Impact:** Unity APIs may not work from background threads
**Mitigation:**
- Document that responders must handle Unity API calls carefully
- Async responders should use `UnityMainThreadDispatcher` if needed
- Add examples showing proper async patterns

### Risk 4: Jobs System Incompatibility
**Impact:** Lock-based approach doesn't work with Unity Jobs
**Mitigation:**
- Document limitation clearly
- Jobs System integration deferred to separate task
- Current implementation sufficient for async/await scenarios

---

## Success Criteria (Overall)

✅ **Implementation Complete When:**
1. All 4 phases completed and tested
2. All tests pass (127+ tests)
3. Performance targets met (< 5% overhead)
4. Documentation updated (CLAUDE.md, XML comments)
5. TECHNICAL_DEBT.md marked complete
6. Code reviewed and approved
7. Committed to feature branch: `feature/thread-safety`

✅ **Ready to Merge When:**
1. Feature branch builds successfully
2. All Unity tests pass (EditMode + PlayMode)
3. Performance benchmarks meet targets
4. Documentation reviewed for accuracy
5. Example scene demonstrates async usage
6. PR approved by maintainer

---

## Next Steps After Completion

**Future Enhancements (Optional):**
1. **Performance Profiling** - Measure lock overhead in production scenarios
2. **Unity Jobs Integration** - Separate task for NativeArray-based routing
3. **Network Async** - Extend async support to network responders
4. **Visual Debugging** - Show async message flow in editor tools

**Immediate Next Steps:**
1. Create feature branch: `git checkout -b feature/thread-safety`
2. Start Phase 1: Add lock field to MmRelayNode.cs
3. Run tests after each phase to verify correctness
4. Update this file with actual time spent per phase

---

**Document Version:** 1.0
**Last Updated:** 2025-11-20
**Owner:** Framework Team
**Status:** Ready for Implementation
**Estimated Total Time:** 4-6 hours
