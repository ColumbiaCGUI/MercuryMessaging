# Dev Docs Pattern

A methodology for maintaining project context across Claude Code sessions and context resets.

---

## The Problem

**Context resets lose everything:**
- Implementation decisions
- Key files and their purposes
- Task progress
- Technical constraints
- Why certain approaches were chosen

**After a reset, Claude has to rediscover everything.**

---

## The Solution: Persistent Dev Docs

A three-file structure that captures everything needed to resume work:

```
dev/active/[task-name]/
├── [task-name]-plan.md      # Strategic plan
├── [task-name]-context.md   # Key decisions & files
└── [task-name]-tasks.md     # Checklist format
```

**These files survive context resets** - Claude reads them to get back up to speed instantly.

---

## Three-File Structure

### 1. [task-name]-plan.md

**Purpose:** Strategic plan for the implementation

**Contains:**
- Executive summary
- Current state analysis
- Proposed future state
- Implementation phases
- Detailed tasks with acceptance criteria
- Risk assessment
- Success metrics
- Timeline estimates

**When to create:** At the start of a complex task

**When to update:** When scope changes or new phases discovered

**Example:**
```markdown
# Feature Name - Implementation Plan

## Executive Summary
What we're building and why

## Current State
Where we are now

## Implementation Phases

### Phase 1: Infrastructure (2 hours)
- Task 1.1: Set up database schema
  - Acceptance: Schema compiles, relationships correct
- Task 1.2: Create service structure
  - Acceptance: All directories created

### Phase 2: Core Functionality (3 hours)
...
```

---

### 2. [task-name]-context.md

**Purpose:** Key information for resuming work

**Contains:**
- SESSION PROGRESS section (updated frequently!)
- What's completed vs in-progress
- Key files and their purposes
- Important decisions made
- Technical constraints discovered
- Links to related files
- Quick resume instructions

**When to create:** Start of task

**When to update:** **FREQUENTLY** - after major decisions, completions, or discoveries

**Example:**
```markdown
# Feature Name - Context

## SESSION PROGRESS (2025-10-29)

### ✅ COMPLETED
- Database schema created (User, Post, Comment models)
- PostController implemented with BaseController pattern
- Sentry integration working

### 🟡 IN PROGRESS
- Creating PostService with business logic
- File: src/services/postService.ts

### ⚠️ BLOCKERS
- Need to decide on caching strategy

## Key Files

**src/controllers/PostController.ts**
- Extends BaseController
- Handles HTTP requests for posts
- Delegates to PostService

**src/services/postService.ts** (IN PROGRESS)
- Business logic for post operations
- Next: Add caching

## Quick Resume
To continue:
1. Read this file
2. Continue implementing PostService.createPost()
3. See tasks file for remaining work
```

**CRITICAL:** Update the SESSION PROGRESS section every time significant work is done!

---

### 3. [task-name]-tasks.md

**Purpose:** Checklist for tracking progress

**Contains:**
- Phases broken down by logical sections
- Tasks in checkbox format
- Status indicators (✅/🟡/⏳)
- Acceptance criteria
- Quick resume section

**When to create:** Start of task

**When to update:** After completing each task or discovering new tasks

**Example:**
```markdown
# Feature Name - Task Checklist

## Phase 1: Setup ✅ COMPLETE
- [x] Create database schema
- [x] Set up controllers
- [x] Configure Sentry

## Phase 2: Implementation 🟡 IN PROGRESS
- [x] Create PostController
- [ ] Create PostService (IN PROGRESS)
- [ ] Create PostRepository
- [ ] Add validation with Zod

## Phase 3: Testing ⏳ NOT STARTED
- [ ] Unit tests for service
- [ ] Integration tests
- [ ] Manual API testing
```

---

## When to Use Dev Docs

**Use for:**
- ✅ Complex multi-day tasks
- ✅ Features with many moving parts
- ✅ Tasks likely to span multiple sessions
- ✅ Work that needs careful planning
- ✅ Refactoring large systems

**Skip for:**
- ❌ Simple bug fixes
- ❌ Single-file changes
- ❌ Quick updates
- ❌ Trivial modifications

**Rule of thumb:** If it takes more than 2 hours or spans multiple sessions, use dev docs.

---

## Workflow with Dev Docs

### Starting a New Task

1. **Use /dev-docs slash command:**
   ```
   /dev-docs refactor authentication system
   ```

2. **Claude creates the three files:**
   - Analyzes requirements
   - Examines codebase
   - Creates comprehensive plan
   - Generates context and tasks files

3. **Review and adjust:**
   - Check if plan makes sense
   - Add any missing considerations
   - Adjust timeline estimates

### During Implementation

1. **Refer to plan** for overall strategy
2. **Update context.md** frequently:
   - Mark completed work
   - Note decisions made
   - Add blockers
3. **Check off tasks** in tasks.md as you complete them

### After Context Reset

1. **Claude reads all three files**
2. **Understands complete state** in seconds
3. **Resumes exactly where you left off**

No need to explain what you were doing - it's all documented!

---

## File Organization

```
dev/
├── README.md                  # This file
├── IMPROVEMENT_TRACKER.md     # Master priority list and roadmap
├── WORKFLOW.md                # Development workflow guidelines
├── FREQUENT_ERRORS.md         # Common bugs & debugging patterns
├── active/                    # Current work (prioritized P1-P10)
│   ├── performance-optimization/  # P1 - MessagePipe parity
│   ├── networking/                # P2/P4 - FishNet + Fusion 2
│   ├── dsl-dx-improvements/       # P3 - Shorter syntax, tutorials
│   ├── user-study/                # P5 - CHI LBW 2025
│   ├── spatial-indexing/          # P6 - UIST 2026
│   ├── parallel-fsm/              # P7 - UIST 2026
│   ├── visual-composer/           # P9 - UIST 2026
│   ├── static-analysis/           # P10 - ICSE 2026
│   └── parallel-dispatch/         # Deferred - SIGGRAPH 2026
└── archive/                   # Completed work
    ├── session-handoffs/          # All session handoff files
    ├── 2025-11-25-language-dsl/   # Completed DSL implementation
    ├── performance-analysis/       # Performance baseline data
    ├── framework-analysis/         # Architecture analysis
    └── ...
```

**active/**: Work in progress (see IMPROVEMENT_TRACKER.md for priorities)
**archive/**: Completed tasks (for reference)

## Current Active Tasks (2025-11-27)

See **IMPROVEMENT_TRACKER.md** for the full prioritized roadmap.

| Priority | Task | Hours | Status |
|----------|------|-------|--------|
| P1 | Performance Optimization | 300h | APPROVED |
| P2 | FishNet Networking | 200h | APPROVED |
| P3 | DSL/DX Improvements | 120h | Planning |
| P4 | Fusion 2 Networking | 200h | Planning |
| P5 | User Study | 40h | ACTIVE |
| P6+ | Research Publications | 1600h+ | Planning |

---

## Example: Real Usage

See **dev/active/public-infrastructure-repo/** in this repository for a real example:
- **plan.md** - 700+ line strategic plan for creating this showcase
- **context.md** - Tracks what's completed, decisions made, what's next
- **tasks.md** - Checklist of all phases and tasks

This is the actual dev docs used to build this showcase!

---

## Best Practices

### Update Context Frequently

**Bad:** Update only at end of session
**Good:** Update after each major milestone

**SESSION PROGRESS section should always reflect reality:**
```markdown
## SESSION PROGRESS (YYYY-MM-DD)

### ✅ COMPLETED (list everything done)
### 🟡 IN PROGRESS (what you're working on RIGHT NOW)
### ⚠️ BLOCKERS (what's preventing progress)
```

### Make Tasks Actionable

**Bad:** "Fix the authentication"
**Good:** "Implement JWT token validation in AuthMiddleware.ts (Acceptance: Tokens validated, errors to Sentry)"

**Include:**
- Specific file names
- Clear acceptance criteria
- Dependencies on other tasks

### Keep Plan Current

If scope changes:
- Update the plan
- Add new phases
- Adjust timeline estimates
- Note why scope changed

---

## For Claude Code

**When user asks to create dev docs:**

1. **Use the /dev-docs slash command** if available
2. **Or create manually:**
   - Ask about the task scope
   - Analyze relevant codebase files
   - Create comprehensive plan
   - Generate context and tasks

3. **Structure the plan with:**
   - Clear phases
   - Actionable tasks
   - Acceptance criteria
   - Risk assessment

4. **Make context file resumable:**
   - SESSION PROGRESS at top
   - Quick resume instructions
   - Key files list with explanations

**When resuming from dev docs:**

1. **Read all three files** (plan, context, tasks)
2. **Start with context.md** - has current state
3. **Check tasks.md** - see what's done and what's next
4. **Refer to plan.md** - understand overall strategy

**Update frequently:**
- Mark tasks complete immediately
- Update SESSION PROGRESS after significant work
- Add new tasks as discovered