# AI Assistant Development Guidelines

This document provides guidelines for AI assistants (like Claude Code, GitHub Copilot, etc.) working on the MercuryMessaging framework.

---

## Table of Contents

1. [Git Commit Authorship Policy](#git-commit-authorship-policy)
2. [Commit Message Guidelines](#commit-message-guidelines)
3. [Example Good Commits](#example-good-commits)
4. [Starting Large Tasks](#starting-large-tasks)
5. [Continuing Tasks](#continuing-tasks)

---

## Git Commit Authorship Policy

### CRITICAL: Do NOT Include AI Co-Authorship

**❌ NEVER add AI attribution in git commits:**

- **NEVER** add `Co-Authored-By: Claude <noreply@anthropic.com>`
- **NEVER** add `🤖 Generated with [Claude Code]` attribution
- **NEVER** add any AI attribution or footer in commit messages

### Rationale

- Git commits must reflect human authorship only
- GitHub displays co-authors as repository contributors (incorrect for AI assistance)
- User preference is for clean commit history without AI attribution
- AI assistance should be invisible in version control history

### ✅ Correct Commit Format

```bash
git commit -m "feat: Add feature description

Detailed explanation of changes made.

Technical details:
- Implementation notes
- Performance impact
- Testing approach"
```

### ❌ INCORRECT Format (NEVER Use)

```bash
git commit -m "feat: Add feature

Details...

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

---

## Commit Message Guidelines

### Format: Conventional Commits

Use the format: `type(scope): description`

**Types:**
- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation only
- `style` - Formatting, missing semicolons, etc.
- `refactor` - Code restructuring without behavior change
- `perf` - Performance improvement
- `test` - Adding or updating tests
- `chore` - Maintenance tasks

**Example:** `feat(relay): Add hop limit protection`

### Writing Clear Messages

1. **First Line: Concise Summary (50-72 characters)**
   - Imperative mood ("Add feature" not "Added feature")
   - No period at the end
   - Clear and specific

2. **Body: Detailed Technical Explanation**
   - Explain what changed and why
   - Include implementation approach
   - Note performance implications
   - List breaking changes (if any)
   - Reference related files/line numbers

3. **Include Technical Details**
   - Implementation approach
   - Performance implications
   - Breaking changes (if any)
   - Related files/line numbers

4. **Do NOT Add**
   - AI attribution or co-authorship
   - Marketing-style language ("Amazing!", "Revolutionary!")
   - Emoji in commit title (body is OK if contextually appropriate)
   - Vague descriptions ("Updated stuff", "Fixed things")

---

## Example Good Commits

### Feature Addition

```bash
git commit -m "feat: Implement lazy message copying optimization

Reduces message allocation overhead by 20-30% through intelligent
copy-on-demand algorithm. Only creates message copies when routing
in multiple directions.

Changes:
- Added direction scanning in MmRelayNode.MmInvoke()
- Single-direction routing reuses original message (0 copies)
- Multi-direction creates only necessary copies (1-2 instead of 2)

Performance: 20-30% fewer allocations in typical scenarios."
```

### Bug Fix

```bash
git commit -m "fix: Prevent infinite loops with hop limit checking

Added hop counter and cycle detection to MmMessage to prevent
infinite message propagation in circular hierarchies.

Implementation:
- HopCount field tracks relay depth
- VisitedNodes HashSet detects cycles
- Both configurable via MmRelayNode inspector

Fixes potential Unity crashes in complex message graphs."
```

### Documentation

```bash
git commit -m "docs: Update framework analysis context

Captured implementation details for QW-4, QW-1, QW-2.
Added code patterns, design decisions, and continuation notes
for seamless context handoff."
```

### Test Addition

```bash
git commit -m "test: Add comprehensive FSM state transition tests

Created FsmStateTransitionTests.cs with 20 automated tests covering:
- Basic transitions (5 tests)
- Event ordering (3 tests)
- Async transitions (4 tests)
- MercuryMessaging integration (5 tests)
- Edge cases (3 tests)

All tests pass. Resolves Priority 3 Technical Debt item."
```

### Refactoring

```bash
git commit -m "refactor: Extract tag filtering logic to separate method

Improves code clarity by isolating tag check logic from main
routing loop. No behavior changes.

Changes:
- New TagCheck() method in MmRelayNode
- ResponderCheck() now calls TagCheck() internally
- All existing tests pass (117/117)"
```

---

## Starting Large Tasks

When working on a large task that requires planning:

### 1. Create Task Directory

```bash
mkdir -p ~/git/project/dev/active/[task-name]/
```

### 2. Create Three Documents

**Required files:**

- `[task-name]-plan.md` - The overall plan/approach
- `[task-name]-context.md` - Technical context, key files, design decisions
- `[task-name]-tasks.md` - Detailed checklist of work items

**Example structure:**

```
dev/active/thread-safety/
├── README.md              # Executive summary
├── thread-safety-context.md   # Technical details
└── thread-safety-tasks.md     # Implementation checklist
```

### 3. Update Regularly

- Mark tasks complete **immediately** after finishing
- Update "Last Updated" timestamps
- Document any deviations from the plan
- Add notes for future continuations

### Document Templates

**README.md Template:**

```markdown
# [Task Name]

**Status:** [Ready to Start / In Progress / Complete]
**Priority:** [P1-P4]
**Estimated Effort:** [X-Y hours]

## Overview
Brief description of the task and its goals.

## Approach
High-level approach or solution options.

## Dependencies
- List of blocking dependencies

## Next Steps
1. First action item
2. Second action item
```

**context.md Template:**

```markdown
# [Task Name] Context

## Current Implementation
Description of current state with code references.

## Proposed Changes
Detailed technical design with code examples.

## Design Decisions
Key decisions made and rationale.

## Testing Strategy
How to validate the changes.

## Open Questions
List of unresolved questions.

## References
- Links to related documentation
- Relevant file paths with line numbers
```

**tasks.md Template:**

```markdown
# [Task Name] Tasks

## Phase 1: [Phase Name]
- [ ] Task 1
- [ ] Task 2
- [ ] Task 3

## Phase 2: [Phase Name]
- [ ] Task 4
- [ ] Task 5

## Acceptance Criteria
- ✅ Criterion 1
- ✅ Criterion 2
```

---

## Continuing Tasks

When resuming work on an existing task:

### 1. Check `/dev/active/` for Existing Tasks

```bash
ls dev/active/
```

Look for task folders that match your work:
- `thread-safety/` - Thread safety improvements
- `routing-optimization/` - Routing table performance
- `network-performance/` - Network message optimization
- `custom-method-extensibility/` - Custom method system
- `framework-analysis/` - Framework analysis and optimization

### 2. Read All Three Files Before Proceeding

**Order:**
1. Read `README.md` - Get high-level overview and current status
2. Read `context.md` - Understand technical details and decisions
3. Read `tasks.md` - See what's been completed and what's next

### 3. Update Timestamps

When making changes to task documents, update the "Last Updated" field:

```markdown
**Last Updated:** 2025-11-20
```

### 4. Mark Tasks Complete Immediately

Don't batch task completions - mark each one complete as soon as finished:

```markdown
## Phase 1: Implementation
- [x] Task 1 (✅ Completed 2025-11-20)
- [x] Task 2 (✅ Completed 2025-11-20)
- [ ] Task 3 (⏳ In Progress)
```

### 5. Document Deviations

If the actual implementation differs from the plan, document why:

```markdown
## Deviations from Plan

**Original Plan:** Use ReaderWriterLockSlim for better concurrency
**Actual Implementation:** Simple lock statement
**Rationale:** Profiling showed negligible contention, simpler approach sufficient
```

---

## Best Practices for AI Assistants

### Code Changes

1. **Read Before Writing**: Always read existing code before making changes
2. **Consult Error Reference**: Check [`dev/FREQUENT_ERRORS.md`](../dev/FREQUENT_ERRORS.md) for:
   - Level filter transformation patterns
   - Routing table registration requirements
   - Runtime component addition patterns
   - Common debugging approaches
3. **Test After Changes**: Run tests after every significant change
4. **Incremental Commits**: Make small, focused commits
5. **Clear Descriptions**: Explain the "why" not just the "what"

### Documentation

1. **Update Inline**: Update XML comments when changing APIs
2. **Keep CLAUDE.md Current**: Update main docs when architecture changes
3. **Link References**: Use file:line syntax for code references
4. **Version Dates**: Update "Last Updated" timestamps

### Communication

1. **Be Precise**: Use exact file paths and line numbers
2. **Show Evidence**: Include code snippets to support claims
3. **Acknowledge Uncertainty**: Say "I'm not sure" when appropriate
4. **Offer Options**: Present multiple approaches when relevant

### Error Handling

1. **Report Clearly**: Explain what failed and why
2. **Provide Context**: Include relevant error messages and stack traces
3. **Suggest Fixes**: Offer concrete solutions, not just descriptions
4. **Test Fixes**: Verify that proposed fixes actually work

### No Silent Fallbacks Policy

**CRITICAL:** This project follows a strict "fail-fast" policy. Silent fallbacks mask failures and make debugging extremely difficult.

#### MUST Rules (Violations = Reject PR)

- **EH-1 (MUST)**: Log full exception with `Debug.LogException(e)` or `MmLogger.LogError(e.ToString())` - never just `e.Message`
- **EH-2 (MUST)**: Bare `catch` blocks without exception type are forbidden
- **EH-3 (MUST)**: If catching and returning null/default, log warning with context first
- **EH-4 (NEVER)**: Empty catch blocks `catch { }`
- **EH-5 (NEVER)**: Catch-and-swallow without stack trace

#### DO Instead (Alternatives)

| ❌ Anti-Pattern | ✅ Do Instead |
|----------------|---------------|
| Return null on failure | Log warning first, then return null |
| Catch all exceptions | Catch specific exception types |
| Continue after error | Rethrow after logging (or let it propagate) |
| `catch (Exception e) { Log(e.Message); }` | `catch (Exception e) { Debug.LogException(e); throw; }` |

#### Examples

**❌ WRONG - Silent Fallback:**
```csharp
try { /* operation */ }
catch (Exception e) { Debug.LogWarning(e.Message); return null; }
```

**✅ CORRECT - Fail Fast:**
```csharp
try { /* operation */ }
catch (SpecificException e) {
    Debug.LogException(e);  // Full stack trace
    throw;  // Let caller handle it
}
```

---

## Common Pitfalls to Avoid

### ❌ DON'T: Make Assumptions About File Locations

**Wrong:**
```
"I'll update the routing table in MmRelayNode.cs around line 400"
```

**Right:**
```
"Let me first read MmRelayNode.cs to locate the routing table code"
→ Read file
→ "The routing table code is at lines 395-420"
```

### ❌ DON'T: Batch Multiple Unrelated Changes

**Wrong:**
```bash
git commit -m "Fixed bug and added feature and updated docs"
```

**Right:**
```bash
git commit -m "fix: Prevent null reference in MmInvoke"
git commit -m "feat: Add async message support"
git commit -m "docs: Update CLAUDE.md with async examples"
```

### ❌ DON'T: Use Vague Language

**Wrong:**
```
"The code should be faster now"
```

**Right:**
```
"Performance improved by 25% (measured with Unity Profiler):
- Before: 40ms per 1000 messages
- After: 30ms per 1000 messages
- Benchmark: SmallScale scene, 10 responders, 3 levels"
```

### ❌ DON'T: Forget to Clean Up Test Code

**Wrong:**
```csharp
[Test]
public void TestSomething()
{
    var obj = new GameObject("Test");
    // ... test code ...
    // Missing cleanup!
}
```

**Right:**
```csharp
[Test]
public void TestSomething()
{
    var obj = new GameObject("Test");
    // ... test code ...
    Object.DestroyImmediate(obj);  // Always clean up!
}
```

---

## Debugging Workflow

When encountering errors:

1. **Check Frequent Errors Reference**
   - **FIRST**, consult [`dev/FREQUENT_ERRORS.md`](../dev/FREQUENT_ERRORS.md)
   - Check if error matches known bugs (5 documented patterns)
   - Review debugging checklists for your error type
   - Apply known fixes before investigating further

2. **Read Error Message Completely**
   - Don't skim - read every line
   - Note file paths and line numbers
   - Identify error type (compilation, runtime, test failure)

3. **Locate Relevant Code**
   - Read the file at the error line
   - Read surrounding context (±10 lines)
   - Check recent changes to that area

4. **Form Hypothesis**
   - What caused the error?
   - Why did it occur now?
   - What changed recently?
   - Does it match a pattern in FREQUENT_ERRORS.md?

5. **Test Hypothesis**
   - Make minimal change to test theory
   - Run tests or compile
   - Verify if hypothesis was correct

6. **Document Fix**
   - Explain what was wrong
   - Explain what fixed it
   - Note how to prevent similar issues
   - **If new bug pattern**: Add to FREQUENT_ERRORS.md

---

## Example Session Flow

### Good Session Flow

```
1. User: "Add async message support"
2. Assistant: "Let me read the current MmInvoke implementation"
   → Read MmRelayNode.cs
3. Assistant: "I see the sync implementation at lines 630-760. Here's my plan..."
   → Present plan with 3 phases
4. User: "Approved"
5. Assistant: "Starting Phase 1: Add lock field"
   → Make change
   → Run tests
   → "Tests pass (117/117). Committing Phase 1..."
   → Commit with clear message
6. Assistant: "Phase 1 complete. Moving to Phase 2..."
   → Continue incrementally
```

### Bad Session Flow

```
1. User: "Add async message support"
2. Assistant: "I'll add async support now"
   → Makes large changes without reading existing code
   → Commits everything at once with vague message
   → Tests fail
   → "Oops, let me fix that"
   → More changes without understanding root cause
   → More test failures
```

---

## Running and Checking Tests

### Running Tests via Unity MCP

```
# Run PlayMode tests (most common)
mcp__UnityMCP__run_tests mode=PlayMode

# Check for completion
mcp__UnityMCP__read_console action=get count=5
```

### Automated Test Result Export

Test results are **automatically exported** to `dev/test-results/` after each test run:

- `TestResults_YYYYMMDD_HHMMSS.xml` - Full NUnit-compatible XML
- `TestResults_YYYYMMDD_HHMMSS_summary.txt` - Quick summary with failed tests

**To check latest test results:**

```bash
# Find latest test result file
find dev/test-results -name "*.xml" -type f | sort -r | head -1

# Read the summary file
cat dev/test-results/TestResults_*_summary.txt | head -20

# Search for failed tests
grep -A5 "result=\"Failed\"" dev/test-results/TestResults_*.xml | head -50
```

### Common Test Patterns

**Creating test hierarchies:** Use `MmTestHierarchy` for clean test setup:

```csharp
// Simple single node with responder
using var hierarchy = MmTestHierarchy.CreateSingle<TestResponder>("TestNode");

// Complex hierarchy with builder pattern
using var hierarchy = MmTestHierarchy.Build("Root")
    .WithResponder<TestResponder>()
    .AddChild<TestResponder>("Child")
    .Build();

// Access nodes and responders
var relay = hierarchy.Root;
var responder = hierarchy.GetRootResponder<TestResponder>();
```

**Important:** `MmBaseResponder` has `[RequireComponent(typeof(MmRelayNode))]`, so responders always have a relay node automatically added.

---

## Questions to Ask Yourself

Before making changes:
- [ ] Have I read the existing code?
- [ ] Do I understand the current implementation?
- [ ] Is my approach consistent with the codebase style?
- [ ] Will this break any existing functionality?

Before committing:
- [ ] Do all tests pass?
- [ ] Is my commit message clear and descriptive?
- [ ] Did I remove any debug code?
- [ ] Is my commit focused on one logical change?

Before marking a task complete:
- [ ] Does the implementation match the plan?
- [ ] Are there any edge cases I didn't test?
- [ ] Is the documentation updated?
- [ ] Would another developer understand this code?

---

## Editor Menu Structure

**IMPORTANT:** All editor menus should be consolidated under the `MercuryMessaging/` top-level menu. Do NOT create separate tabs like "Mercury", "UserStudy", etc.

### Centralized Menu Definition

All editor menus are defined in: `Assets/MercuryMessaging/Editor/MmEditorMenus.cs`

### Current Menu Structure (as of 2025-12-12)

```
MercuryMessaging/
├── Validation/
│   ├── Validate Hierarchy          (MmHierarchyValidator.cs)
│   └── Validate Selected           (MmHierarchyValidator.cs)
├── Network Tests/
│   ├── Build Fusion 2 Test Scene   (MmEditorMenus.cs) - Creates complete Fusion 2 test scene
│   ├── Build FishNet Test Scene    (MmEditorMenus.cs) - Creates FishNet test scene
│   ├── Build Test Hierarchy        (MmEditorMenus.cs) - Builds hierarchy in current scene
│   └── Verify Hierarchy            (MmEditorMenus.cs) - Validates network test setup
└── Performance/
    └── Build Test Scenes           (PerformanceSceneBuilder.cs)
```

---

## Fusion 2 Integration Setup

**IMPORTANT:** Setting up Fusion 2 requires several configuration steps beyond installing the package.

### Prerequisites

1. Install Photon Fusion 2 via Package Manager
2. Install ParrelSync for multi-editor testing (optional but recommended)

### Required Configuration Steps

#### Step 1: Add MercuryMessaging Assembly to Fusion Weaver

Fusion 2 uses IL weaving for NetworkBehaviour. MercuryMessaging must be registered:

1. Open `Assets/Photon/Fusion/Resources/NetworkProjectConfig.asset`
2. Find **Assemblies to Weave** section
3. Add `MercuryMessaging` to the list

Without this step, you'll get: `InvalidOperationException: Type MmFusion2Bridge has not been weaved`

#### Step 2: Enable Unsafe Code (Already Done)

The `MercuryMessaging.asmdef` has `allowUnsafeCode: true` which is required for Fusion IL weaving.

#### Step 3: Create MmFusion2Bridge Prefab

A spawnable NetworkObject prefab is required for Fusion 2 RPCs:

1. Create empty GameObject
2. Add `NetworkObject` component
3. Add `MmFusion2Bridge` component
4. Save as prefab in `MercuryMessaging/Examples/MmFusion2Bridge.prefab`
5. Assign prefab to `Fusion2TestManager.mmFusion2BridgePrefab` field

### Test Scene Structure (Fusion2Test.unity)

```
Scene Hierarchy:
├── NetworkRunner (NetworkRunner + NetworkSceneManagerDefault)
├── MmNetworkBridge (MmNetworkBridge + Fusion2BridgeSetup)
├── TestRoot (MmRelayNode)
│   ├── ChildA (MmRelayNode + NetworkTestResponder)
│   │   └── GrandchildA1 (MmRelayNode + NetworkTestResponder)
│   └── ChildB (MmRelayNode + NetworkTestResponder)
│       └── GrandchildB1 (MmRelayNode + NetworkTestResponder)
└── Fusion2TestManager (Fusion2TestManager - Test GUI)
```

### Testing with ParrelSync

1. Open ParrelSync: `ParrelSync > Clones Manager`
2. Create a clone if none exists
3. Open clone in separate Unity Editor
4. In main editor: Click "Start Host" button in game view
5. In clone editor: Click "Start Client" button in game view
6. Use GUI buttons to send messages between host and client

### Key Differences from FishNet

| Feature | FishNet | Fusion 2 |
|---------|---------|----------|
| Message Transport | Broadcasts (stateless) | RPCs on NetworkBehaviour |
| Requires NetworkObject | No | Yes (for MmFusion2Bridge) |
| Host Initialization | OnServerStarted callback | Auto-detect in Update() |
| Client Initialization | OnClientStarted callback | OnConnectedToServer callback |
| IL Weaving | Not required | Required (NetworkProjectConfig) |

### Troubleshooting

**"Type has not been weaved"**
→ Add MercuryMessaging to NetworkProjectConfig assemblies

**"Assembly does not allow unsafe code"**
→ Set `allowUnsafeCode: true` in MercuryMessaging.asmdef

**"RpcSendToServer - no backend connected"**
→ MmFusion2Bridge couldn't find Fusion2Backend. Ensure:
- MmNetworkBridge has Fusion2BridgeSetup component
- Fusion2BridgeSetup.Configure() was called (auto on Start)
- MmFusion2Bridge prefab is spawned after network starts

**Host shows "Not connected" when sending**
→ Host doesn't receive OnConnectedToServer. Fixed by auto-init in Update() when runner is running.

### Adding New Menu Items

When adding new editor functionality:

1. **Add to MmEditorMenus.cs** for new menus
2. **Use consistent paths**: Always start with `MercuryMessaging/`
3. **Group logically**: Use submenus for related functionality
4. **Use priority values**: Lower numbers appear higher in menu

```csharp
// Example: Adding a new menu item
[MenuItem("MercuryMessaging/Tools/My New Tool", false, 100)]
public static void MyNewTool()
{
    // Implementation
}
```

### Menu Files Reference

| File | Menu Items |
|------|------------|
| `Editor/MmEditorMenus.cs` | Network test scene builders, test hierarchy |
| `Editor/MmHierarchyValidator.cs` | Hierarchy validation tools |
| `Tests/Performance/Editor/PerformanceSceneBuilder.cs` | Performance test scene builder |

### DO NOT Create New Top-Level Menus

❌ **Wrong:**
```csharp
[MenuItem("Mercury/Something")]
[MenuItem("UserStudy/Something")]
[MenuItem("Framework/Something")]
```

✅ **Correct:**
```csharp
[MenuItem("MercuryMessaging/Category/Something")]
```

---

**Last Updated:** 2025-12-12
**Maintained By:** Framework Team
**Applies To:** All AI assistants working on MercuryMessaging
