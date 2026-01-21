# Contributing to MercuryMessaging

Thank you for your interest in contributing to the MercuryMessaging framework! This document outlines the development standards and best practices for maintaining code quality and consistency.

---

## Table of Contents

1. [External Dependency Policy](#external-dependency-policy)
2. [Naming Convention Policy](#naming-convention-policy)
3. [Testing Standards Policy](#testing-standards-policy)
4. [Code Quality Guidelines](#code-quality-guidelines)
5. [Commit Message Guidelines](#commit-message-guidelines)

---

## External Dependency Policy

**CRITICAL:** The MercuryMessaging core framework MUST minimize external dependencies.

### Allowed Dependencies (ONLY)

- ✅ **UnityEngine** - Core Unity runtime
- ✅ **UnityEditor** - Unity Editor APIs (editor scripts only)
- ✅ **System.*** - .NET Standard libraries (Collections, Linq, IO, etc.)

### Optional Dependencies

Optional dependencies MUST be isolated outside the core framework with conditional compilation:

- ✅ **FishNet** - Wrapped in `#if FISH_NET`
- ✅ **Photon Fusion 2** - Wrapped in `#if FUSION_WEAVER`

### Adding New Dependencies

If you need to add a new external dependency, you MUST:

1. Justify it as essential to core functionality
2. Wrap all usage in conditional compilation directives
3. Ensure framework gracefully degrades when dependency is missing

**Example:**

```csharp
#if FISH_NET
using FishNet;
using FishNet.Managing;
#endif

namespace MercuryMessaging.Network.Backends
{
    public class FishNetBackend : IMmNetworkBackend
    {
#if FISH_NET
        // Implementation using FishNet
        public bool IsConnected => InstanceFinder.IsServerStarted || InstanceFinder.IsClientStarted;
#else
        public bool IsConnected => false;
        // Graceful degradation when FishNet not available
#endif
    }
}
```

---

## Naming Convention Policy

**CRITICAL:** Core framework files MUST follow the "Mm" prefix convention to avoid naming conflicts and maintain clear framework boundaries.

### Folders Requiring "Mm" Prefix

All files in these folders MUST start with "Mm" or "IMm":

- ✅ `Assets/MercuryMessaging/AppState/` - All files must start with "Mm"
- ✅ `Assets/MercuryMessaging/Protocol/` - All files must start with "Mm"
- ✅ `Assets/MercuryMessaging/Support/Data/` - All files must start with "Mm"
- ✅ `Assets/MercuryMessaging/Task/` - All files must start with "Mm" or "IMm"

### Folders EXEMPT from "Mm" Prefix

Generic utilities that are not framework-specific do not require the prefix:

- `Support/Extensions/` - Generic C# extension methods
- `Support/FiniteStateMachine/` - Generic FSM implementation
- `Support/Interpolators/` - Generic animation utilities
- `Support/Input/` - Generic input handling
- `Support/GUI/` - GUI utilities
- `Support/Editor/` - Editor utilities
- `Support/ThirdParty/` - Third-party integrations
- `Examples/` - Tutorial and demo code
- `Tests/` - Test files

### Naming Rules

- ✅ All classes in core folders: `MmClassName.cs`
- ✅ All interfaces in core folders: `IMmInterfaceName.cs`
- ✅ All structs in core folders: `MmStructName.cs`
- ✅ All enums in core folders: `MmEnumName.cs`

### Examples

**Correct:**

```
Protocol/Nodes/MmRelayNode.cs
Protocol/Message/MmMessage.cs
Support/Data/MmCircularBuffer.cs
Support/Data/MmDataCollector.cs
Task/MmTaskManager.cs
Task/IMmTaskInfoCollectionLoader.cs
Support/Extensions/GameObjectExtensions.cs (exempt folder - no prefix needed)
```

**Incorrect:**

```
Support/Data/CircularBuffer.cs (must be MmCircularBuffer.cs)
Protocol/RelayNode.cs (must be MmRelayNode.cs)
Task/TaskManager.cs (must be MmTaskManager.cs)
```

---

## Testing Standards Policy

**CRITICAL:** All tests MUST be fully automated without manual scene setup or UI creation.

### Required Patterns

#### 1. Use Unity Test Framework

- **PlayMode tests** for runtime behavior (most common)
- **EditMode tests** for editor-only code

#### 2. Programmatic GameObject Creation

All test setup must be done programmatically:

```csharp
[SetUp]
public void Setup()
{
    testRoot = new GameObject("TestRoot");
    relay = testRoot.AddComponent<MmRelayNode>();
    relay.MmRefreshResponders(); // CRITICAL: Explicit registration for dynamic components
}
```

#### 3. No Manual Assets

- ❌ NO scene files in test folders
- ❌ NO prefab dependencies
- ❌ NO manual UI element creation
- ✅ Create all objects via `new GameObject()` and `AddComponent<T>()`

#### 4. Proper Cleanup

Always clean up created GameObjects:

```csharp
[TearDown]
public void Teardown()
{
    if (testRoot != null)
    {
        Object.DestroyImmediate(testRoot);
    }
}
```

#### 5. Test Isolation

- Each test creates its own hierarchy
- No shared state between tests
- Clean up all created objects

### Test Pattern Examples

**Good Examples:**

- ✅ `MmCircularBufferTests.cs` - 30 tests, all programmatic
- ✅ `MmHopLimitValidationTests.cs` - Creates 50-node chain programmatically
- ✅ `MmLazyCopyValidationTests.cs` - Complex hierarchies created in code

**Bad Examples:**

- ❌ Tests that require loading "TestScene.unity"
- ❌ Tests that reference prefabs from Resources folder
- ❌ Tests that create UI elements manually in editor

### Complete Test Template

```csharp
using NUnit.Framework;
using UnityEngine;

namespace MercuryMessaging.Tests
{
    public class MmExampleTests
    {
        private GameObject testRoot;
        private MmRelayNode relay;

        [SetUp]
        public void SetUp()
        {
            // Arrange - create hierarchy programmatically
            testRoot = new GameObject("TestRoot");
            relay = testRoot.AddComponent<MmRelayNode>();
            relay.MmRefreshResponders(); // CRITICAL: Explicit registration required
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup - destroy all created objects
            if (testRoot != null)
            {
                Object.DestroyImmediate(testRoot);
            }
        }

        [Test]
        public void TestMessageRouting()
        {
            // Arrange - add responders
            var child = new GameObject("ChildResponder");
            child.transform.SetParent(testRoot.transform);
            var responder = child.AddComponent<TestResponder>();
            relay.MmRefreshResponders();

            // Act - perform test
            relay.MmInvoke(MmMethod.Initialize);

            // Assert - verify behavior
            Assert.IsTrue(responder.InitializeCalled);
        }

        // Helper responder for testing
        private class TestResponder : MmBaseResponder
        {
            public bool InitializeCalled = false;

            protected override void ReceivedInitialize()
            {
                InitializeCalled = true;
            }
        }
    }
}
```

---

## Code Quality Guidelines

### General Principles

1. **Clarity Over Cleverness**: Write code that is easy to understand and maintain
2. **Consistency**: Follow existing patterns in the codebase
3. **Documentation**: Add XML comments for all public APIs
4. **Performance**: Minimize allocations in hot paths
5. **Error Handling**: Log errors clearly with MmLogger

### XML Documentation

All public classes, methods, and properties must have XML documentation:

```csharp
/// <summary>
/// Invokes a message on all filtered responders in the routing table.
/// </summary>
/// <param name="message">The message to process</param>
/// <returns>True if any responders received the message</returns>
public virtual bool MmInvoke(MmMessage message)
{
    // Implementation
}
```

### Performance Considerations

**DO:**
- ✅ Cache component references
- ✅ Use object pooling for frequently allocated objects
- ✅ Prefer foreach over LINQ in hot paths
- ✅ Use CircularBuffer for bounded collections

**DON'T:**
- ❌ Allocate in Update/FixedUpdate loops
- ❌ Use string concatenation in loops (use StringBuilder)
- ❌ Perform expensive lookups repeatedly (cache results)

### Logging Guidelines

Use MmLogger for all framework logging:

```csharp
MmLogger.LogFramework("Responder registered: " + responder.name);
MmLogger.LogApplication("User completed task: " + taskName);
MmLogger.LogError("Failed to load configuration: " + error);
```

Enable/disable categories in `MmLogger.cs`:

```csharp
public static bool logFramework = false;   // Framework internals
public static bool logResponder = false;   // Responder activity
public static bool logApplication = true;  // Application logic
public static bool logNetwork = false;     // Network messages
```

---

## Commit Message Guidelines

### Format

Use Conventional Commits format:

```
type(scope): brief description

Detailed explanation of changes made.

Technical details:
- Implementation notes
- Performance impact
- Testing approach
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `style`: Formatting, missing semicolons, etc.
- `refactor`: Code change that neither fixes a bug nor adds a feature
- `perf`: Performance improvement
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

### Examples

**Feature Addition:**

```bash
git commit -m "feat(relay): Add hop limit protection

Prevents infinite message propagation in circular hierarchies by
tracking hop count and visited nodes.

Implementation:
- Added HopCount field to MmMessage
- Added VisitedNodes HashSet for cycle detection
- Configurable max hops (default: 50) in MmRelayNode inspector

Performance: < 1% overhead for typical hierarchies (< 10 levels)
Testing: Added 6 automated tests covering deep hierarchies and cycles"
```

**Bug Fix:**

```bash
git commit -m "fix(routing): Initialize Tags field during responder registration

Tag filtering was broken because MmAddToRoutingTable never initialized
the Tags field in routing table items. All tag checks failed.

Changes:
- MmRelayNode.cs:406: Added Tags = mmResponder.Tag during item creation
- MmRelayNode.cs:471-479: Added Tags update in MmRefreshResponders

Fixes: Tag filtering integration tests now pass (2/2)"
```

**Documentation:**

```bash
git commit -m "docs: Move thread safety improvements to active task folder

Comprehensive documentation created with 3 solution options, technical
design, and 4-phase implementation checklist.

Files:
- dev/active/thread-safety/README.md (executive summary)
- dev/active/thread-safety/thread-safety-context.md (technical design)
- dev/active/thread-safety/thread-safety-tasks.md (implementation checklist)

Status: Ready for implementation when async/await support is needed"
```

### What NOT to Include

**NEVER** include:

- ❌ AI attribution or co-authorship (`Co-Authored-By: Claude`)
- ❌ Marketing-style language ("Amazing new feature!")
- ❌ Emoji in commit title (body is okay if contextually appropriate)
- ❌ Vague descriptions ("Updated stuff", "Fixed things")

---

## Pull Request Process

### Before Submitting

1. **Run All Tests**: Ensure all Unity tests pass (EditMode + PlayMode)
2. **Check Compilation**: Zero compilation errors or warnings
3. **Update Documentation**: Add/update XML comments and CLAUDE.md if needed
4. **Performance Check**: Run performance tests if touching hot paths
5. **Review Your Changes**: Self-review all diffs before submitting

### PR Description Template

```markdown
## Summary
Brief description of what this PR accomplishes.

## Changes
- Bullet point list of key changes
- Include file paths and line numbers for significant changes

## Testing
- Describe how you tested the changes
- List new tests added (if any)
- Confirm all existing tests pass

## Performance Impact
- Describe any performance implications
- Include benchmark results if relevant

## Breaking Changes
- List any breaking API changes
- Provide migration guide if needed

## Related Issues
- Closes #123
- Related to #456
```

### Review Process

1. Automated tests must pass
2. Code review by maintainer
3. Documentation review if CLAUDE.md changed
4. Performance review if hot paths modified
5. Approval required before merge

---

## Getting Help

- **Documentation**: See [CLAUDE.md](CLAUDE.md) for framework overview
- **File Reference**: See [FILE_REFERENCE.md](Documentation/FILE_REFERENCE.md) for key files
- **Workflow**: See [dev/WORKFLOW.md](dev/WORKFLOW.md) for development processes
- **Technical Debt**: See [dev/TECHNICAL_DEBT.md](dev/TECHNICAL_DEBT.md) for known issues
- **Issues**: Report bugs at https://github.com/yourrepo/mercurymessaging/issues

---

## License

By contributing to MercuryMessaging, you agree that your contributions will be licensed under the same license as the project (see LICENSE file).

---

**Last Updated:** 2025-11-20
**Maintained By:** Framework Team
