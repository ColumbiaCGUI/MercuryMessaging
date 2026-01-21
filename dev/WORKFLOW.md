# MercuryMessaging Development Workflow

This document describes the development workflows for the MercuryMessaging framework.

---

## Table of Contents

1. [Feature Development Workflow](#feature-development-workflow)
2. [Bug Fix Workflow](#bug-fix-workflow)
3. [Testing Workflow](#testing-workflow)
4. [Documentation Workflow](#documentation-workflow)
5. [Task Management Workflow](#task-management-workflow)
6. [Release Workflow](#release-workflow)

---

## Feature Development Workflow

### 1. Planning Phase

**For Small Features (< 2 hours):**
- Create issue or document in dev/IMPROVEMENT_TRACKER.md
- Skip feature branch, work on current branch
- Single commit when complete

**For Large Features (> 2 hours):**
- Create task folder: `dev/active/[feature-name]/`
- Create three documents:
  - `README.md` - Executive summary
  - `[feature-name]-context.md` - Technical design
  - `[feature-name]-tasks.md` - Implementation checklist
- Create feature branch: `git checkout -b feature/[feature-name]`

### 2. Implementation Phase

**Incremental Development:**

```bash
# 1. Make small, focused changes
# Edit code...

# 2. Run tests after each change
# Window > General > Test Runner > Run All

# 3. Commit frequently
git add [files]
git commit -m "feat(scope): description

Detailed explanation..."

# 4. Update task checklist
# Mark completed items in [feature-name]-tasks.md

# 5. Repeat until feature complete
```

**Best Practices:**
- ✅ Test after every significant change
- ✅ Commit every 30-60 minutes of work
- ✅ Keep commits focused on single logical change
- ✅ Update task checklist immediately after completing items

### 3. Testing Phase

**Run All Tests:**

```bash
# In Unity Editor:
# Window > General > Test Runner > Run All (PlayMode + EditMode)

# Expected: All tests pass (117+)
```

**Add New Tests:**

```csharp
// Create test file: Assets/MercuryMessaging/Tests/[Feature]Tests.cs

using NUnit.Framework;
using UnityEngine;

namespace MercuryMessaging.Tests
{
    public class MyFeatureTests
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
                Object.DestroyImmediate(testRoot);
        }

        [Test]
        public void Feature_Scenario_ExpectedBehavior()
        {
            // Arrange
            // Act
            // Assert
        }
    }
}
```

### 4. Documentation Phase

**Update Documentation:**

```bash
# 1. Update XML comments for all new public APIs
# 2. Update CLAUDE.md if architecture changed
# 3. Update CONTRIBUTING.md if new patterns added
# 4. Update Documentation/FILE_REFERENCE.md if new important files added
# 5. Update dev/IMPROVEMENT_TRACKER.md if tracking new work

# Commit documentation separately
git add CLAUDE.md
git commit -m "docs: Update CLAUDE.md with [feature] examples"
```

### 5. Review Phase

**Self-Review Checklist:**
- [ ] All tests pass (117+)
- [ ] Zero compilation errors or warnings
- [ ] Code follows naming conventions (Mm prefix)
- [ ] XML comments added for public APIs
- [ ] No TODOs or debug code remaining
- [ ] Documentation updated
- [ ] Performance impact assessed

**For Feature Branches:**

```bash
# Merge to main branch
git checkout master
git merge feature/[feature-name]

# Or create Pull Request (if using GitHub)
gh pr create --title "feat: [feature-name]" --body "..."
```

---

## Bug Fix Workflow

### 1. Reproduce the Bug

```bash
# 1. Create test that reproduces the bug (it should FAIL)

[Test]
public void BugRepro_[Description]_ShouldFail()
{
    // Arrange - create scenario that triggers bug
    // Act - perform action
    // Assert - verify incorrect behavior
    Assert.Fail("Bug reproduced - this test should fail until fixed");
}

# 2. Run test to confirm it fails
# 3. Document the failure in code comments
```

### 2. Fix the Bug

```bash
# 1. Identify root cause
# Read relevant code, add logging if needed

# 2. Implement fix
# Make minimal changes to fix the specific issue

# 3. Update test to verify fix
[Test]
public void BugFix_[Description]_ShouldPass()
{
    // Arrange - same scenario
    // Act - same action
    // Assert - verify CORRECT behavior
    Assert.IsTrue(...);  // Should now pass
}

# 4. Run all tests to ensure no regressions
```

### 3. Commit Fix

```bash
git add [files]
git commit -m "fix: [brief description]

Root cause: [explanation of what was wrong]

Changes:
- File.cs:123: [what changed]

Fixes: [issue number or description]
Testing: Added test in [TestFile.cs:45]"
```

---

## Testing Workflow

### Running Tests Locally

**Unity Test Runner:**

```
1. Open Unity Editor
2. Window > General > Test Runner
3. Select PlayMode tab (most tests)
4. Click "Run All"
5. Verify all tests pass (green checkmarks)
```

**Test Output:**
- ✅ Green checkmark: Test passed
- ❌ Red X: Test failed (click for details)
- ⚠️ Yellow warning: Test skipped or ignored

### Writing New Tests

**Test File Location:**
- All tests in `Assets/MercuryMessaging/Tests/`
- Follow naming: `[Feature]Tests.cs` or `[Class]Tests.cs`

**Test Patterns:**

```csharp
// 1. PlayMode test (most common - requires Unity runtime)
[Test]
public void Feature_Scenario_ExpectedBehavior()
{
    // Test implementation
}

// 2. UnityTest (for tests requiring multiple frames)
[UnityTest]
public IEnumerator Feature_Scenario_OverTime()
{
    // Setup
    yield return null;  // Wait one frame
    // Assert
}

// 3. EditMode test (for editor-only code)
[Test]
public void EditorFeature_Scenario_ExpectedBehavior()
{
    // Test implementation (no runtime required)
}
```

### Test Categories

**Unit Tests:**
- Test single class or method
- No dependencies on other classes
- Fast execution (< 10ms)

**Integration Tests:**
- Test multiple classes working together
- May require complex setup
- Moderate execution (10-100ms)

**Performance Tests:**
- Measure execution time or memory
- Use Stopwatch or Unity Profiler
- Document baseline expectations

### Automated Test Result Export

**Location:** Test results are automatically exported to `dev/test-results/`

**Files Generated:**
- `TestResults_YYYYMMDD_HHMMSS.xml` - Full NUnit-compatible XML results
- `TestResults_YYYYMMDD_HHMMSS_summary.txt` - Quick summary with failed tests

**Implementation:** `Assets/MercuryMessaging/Editor/MmTestResultExporter.cs`

The `MmTestResultExporter` class automatically:
1. Registers with Unity Test Framework via `ICallbacks`
2. Exports results on test run completion
3. Generates both XML and summary files

**For AI Assistants (Claude Code, etc.):**
To check test results programmatically:
```bash
# Find latest test results
find dev/test-results -name "*.xml" -type f | sort -r | head -1

# Read summary file
cat dev/test-results/TestResults_*_summary.txt | head -20

# Search for failed tests in XML
grep -A5 "result=\"Failed\"" dev/test-results/TestResults_*.xml | head -50
```

**Via Unity MCP:**
```
# Run tests (results auto-exported)
mcp__UnityMCP__run_tests mode=PlayMode

# Check console for export confirmation
mcp__UnityMCP__read_console action=get count=5
```

---

## Documentation Workflow

### When to Update Documentation

**Always Update:**
- ✅ Adding new public API → Update XML comments + CLAUDE.md
- ✅ Changing architecture → Update CLAUDE.md
- ✅ Tracking improvements → Update IMPROVEMENT_TRACKER.md
- ✅ Adding new important file → Update Documentation/FILE_REFERENCE.md
- ✅ Changing development process → Update CONTRIBUTING.md or WORKFLOW.md

**Optional:**
- ⚠️ Internal implementation changes → XML comments only
- ⚠️ Bug fixes → XML comments if API changed
- ⚠️ Refactoring → XML comments if behavior clarified

### Documentation Files

**CLAUDE.md** (Main Documentation)
- Project overview and architecture
- Key features and concepts
- Common workflows and patterns
- Performance characteristics
- Quick reference guide

**CONTRIBUTING.md** (Development Standards)
- External dependency policy
- Naming conventions
- Testing standards
- Code quality guidelines
- Commit message format

**Documentation/FILE_REFERENCE.md** (Important Files)
- Core protocol files with descriptions
- Message system files
- Filtering and routing files
- Task management files
- Support system files

**dev/IMPROVEMENT_TRACKER.md** (Roadmap & Tracking)
- Track 1: Production Engineering (performance, networking)
- Track 2: User Study planning
- Track 3: Research publications
- Completed Improvements log

**dev/WORKFLOW.md** (This Document)
- Feature development workflow
- Bug fix workflow
- Testing workflow
- Documentation workflow

**.claude/ASSISTANT_GUIDE.md** (AI Assistant Guide)
- Git commit authorship policy
- Commit message guidelines
- Starting large tasks
- Continuing tasks

### Updating CLAUDE.md

**Small Changes (< 20 lines):**

```bash
# 1. Edit CLAUDE.md directly
# 2. Update "Last Updated" timestamp
# 3. Commit with docs: type

git add CLAUDE.md
git commit -m "docs: Update [section] with [changes]"
```

**Large Changes (> 20 lines):**

```bash
# 1. Consider extracting content to separate file
# 2. Update CLAUDE.md to link to new file
# 3. Keep CLAUDE.md focused and concise

git add CLAUDE.md [new-file.md]
git commit -m "docs: Extract [section] to [new-file.md]

CLAUDE.md was getting large ([X] lines). Extracted [section]
to separate file for better organization.

Files:
- CLAUDE.md: Reduced by [Y] lines
- [new-file.md]: New file with extracted content"
```

---

## Task Management Workflow

### Active Tasks Directory Structure

```
dev/active/
├── [task-name]/
│   ├── README.md              # Executive summary
│   ├── [task-name]-context.md # Technical details
│   └── [task-name]-tasks.md   # Implementation checklist
```

### Creating a New Task

```bash
# 1. Create task folder
mkdir -p dev/active/[task-name]/

# 2. Create README.md (executive summary)
cat > dev/active/[task-name]/README.md <<EOF
# [Task Name]

**Status:** Ready to Start
**Priority:** P1-P4
**Estimated Effort:** X-Y hours

## Overview
Brief description

## Approach
High-level solution

## Next Steps
1. First action
EOF

# 3. Create context.md (technical details)
# - Current implementation analysis
# - Proposed changes with code examples
# - Design decisions and rationale
# - Testing strategy

# 4. Create tasks.md (implementation checklist)
# - Phase-by-phase breakdown
# - Acceptance criteria
# - Checkboxes for tracking

# 5. Update IMPROVEMENT_TRACKER.md to reference new task folder
```

### Working on an Existing Task

```bash
# 1. List active tasks
ls dev/active/

# 2. Read task documentation
cat dev/active/[task-name]/README.md
cat dev/active/[task-name]/[task-name]-context.md
cat dev/active/[task-name]/[task-name]-tasks.md

# 3. Update task checklist as you work
# Mark items as complete: [x] instead of [ ]

# 4. Commit progress regularly
git add dev/active/[task-name]/[task-name]-tasks.md
git commit -m "docs: Update [task-name] progress (Phase X complete)"

# 5. When complete, move to dev/archive/
mv dev/active/[task-name]/ dev/archive/[task-name]/
```

### Archiving Completed Tasks

```bash
# 1. Ensure all tasks are marked complete
# 2. Update final status in README.md
# 3. Move to archive

mkdir -p dev/archive/
mv dev/active/[task-name]/ dev/archive/[task-name]/

git add dev/active/ dev/archive/
git commit -m "docs: Archive [task-name] (complete)"

# 4. Update IMPROVEMENT_TRACKER.md to mark task as complete
```

---

## Release Workflow

### Version Numbering

MercuryMessaging follows Semantic Versioning (SemVer):

- **Major (X.0.0)**: Breaking changes to public API
- **Minor (0.X.0)**: New features, backward compatible
- **Patch (0.0.X)**: Bug fixes, backward compatible

### Release Checklist

**Pre-Release:**

- [ ] All tests pass (117+)
- [ ] Zero compilation errors or warnings
- [ ] Performance benchmarks meet targets
- [ ] Documentation up to date
- [ ] IMPROVEMENT_TRACKER.md reflects current state
- [ ] No P1 (critical) items in technical debt

**Release Process:**

```bash
# 1. Update version number
# Edit appropriate version files or Unity Package.json

# 2. Update CHANGELOG.md
cat >> CHANGELOG.md <<EOF
## [X.Y.Z] - $(date +%Y-%m-%d)

### Added
- New feature descriptions

### Changed
- Changed feature descriptions

### Fixed
- Bug fix descriptions

### Performance
- Performance improvement descriptions
EOF

# 3. Commit release
git add CHANGELOG.md [version-files]
git commit -m "chore: Release version X.Y.Z"

# 4. Tag release
git tag -a vX.Y.Z -m "Release version X.Y.Z"

# 5. Push to remote
git push origin master
git push origin vX.Y.Z
```

**Post-Release:**

- [ ] Update Unity Asset Store listing (if applicable)
- [ ] Update documentation website (if applicable)
- [ ] Announce release in relevant channels
- [ ] Close related GitHub issues

---

## Common Scenarios

### Scenario 1: Quick Bug Fix

```bash
# 1. Reproduce bug with test
# 2. Fix bug
# 3. Verify test passes
# 4. Run all tests
# 5. Commit fix

git add [files]
git commit -m "fix: [description]"
```

**Time: 15-60 minutes**

### Scenario 2: Small Feature (< 2 hours)

```bash
# 1. Implement feature
# 2. Add tests
# 3. Update XML comments
# 4. Update CLAUDE.md if needed
# 5. Commit feature

git add [files]
git commit -m "feat: [description]"
```

**Time: 1-2 hours**

### Scenario 3: Large Feature (> 2 hours)

```bash
# 1. Create task folder (dev/active/[feature]/)
# 2. Create README, context, tasks documents
# 3. Create feature branch
# 4. Implement incrementally (commit every 30-60 min)
# 5. Update task checklist as you go
# 6. Add comprehensive tests
# 7. Update documentation
# 8. Self-review checklist
# 9. Merge to master or create PR

git checkout -b feature/[name]
# ... work ...
git checkout master
git merge feature/[name]
```

**Time: 2+ hours**

### Scenario 4: Performance Optimization

```bash
# 1. Establish baseline (run performance tests)
# 2. Profile to identify bottleneck
# 3. Implement optimization
# 4. Measure improvement
# 5. Verify no regressions (all tests pass)
# 6. Document performance impact
# 7. Commit with perf: type

git commit -m "perf: [description]

Baseline: [X ms/frame or Y msg/sec]
Optimized: [A ms/frame or B msg/sec]
Improvement: [Z%]

Profiling: [Unity Profiler / Stopwatch]
Testing: All tests pass, no regressions"
```

**Time: 2-8 hours**

### Scenario 5: Refactoring

```bash
# 1. Ensure all tests pass (baseline)
# 2. Refactor code
# 3. Ensure all tests still pass (no behavior change)
# 4. Update XML comments if needed
# 5. Commit with refactor: type

git commit -m "refactor: [description]

No behavior changes. All tests pass (117/117).

Changes:
- [File:Line]: [what changed]"
```

**Time: 1-4 hours**

---

## Getting Help

If you're unsure about any workflow:

1. **Check Documentation:**
   - [CLAUDE.md](../CLAUDE.md) - Framework overview
   - [CONTRIBUTING.md](../CONTRIBUTING.md) - Development standards
   - [FILE_REFERENCE.md](../Documentation/FILE_REFERENCE.md) - Important files
   - [IMPROVEMENT_TRACKER.md](IMPROVEMENT_TRACKER.md) - Roadmap & tracking

2. **Check Active Tasks:**
   - `ls dev/active/` - See what's being worked on
   - Read task documentation for similar work

3. **Review Commit History:**
   - `git log --oneline -20` - Recent commits
   - `git show [commit]` - Detailed commit info

4. **Ask for Guidance:**
   - Create GitHub issue
   - Reach out to maintainers

---

**Last Updated:** 2025-11-27
**Maintained By:** Framework Team
