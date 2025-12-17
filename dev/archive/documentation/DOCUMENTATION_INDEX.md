# MercuryMessaging Complete Documentation Index

**Last Updated:** 2025-11-20
**Purpose:** Central navigation hub for all project documentation

---

## Quick Navigation

**🚀 New to the project?** Start here:
1. [`README.md`](../README.md) - Project overview
2. [`CLAUDE.md`](../CLAUDE.md) - Framework documentation
3. [`Assets/MercuryMessaging/Examples/`](../Assets/MercuryMessaging/Examples/) - Code examples

**💻 Developer starting work?** Start here:
1. [`dev/active/MASTER-SUMMARY.md`](./active/MASTER-SUMMARY.md) - All active tasks (5 planning + 1 in-progress)
2. [`dev/active/STATUS-REPORT.md`](./active/STATUS-REPORT.md) - Current status (Nov 20)
3. [`dev/archive/framework-analysis/`](./archive/framework-analysis/) - Performance analysis (completed)

**📊 Project manager tracking progress?** Start here:
1. [`dev/active/MASTER-SUMMARY.md`](./active/MASTER-SUMMARY.md) - Timeline and effort estimates
2. [`dev/active/STATUS-REPORT.md`](./active/STATUS-REPORT.md) - Current completion status
3. Individual task READMEs for details

---

## Root Level Documentation

### Project Documentation

| File | Size | Purpose | Audience |
|------|------|---------|----------|
| [README.md](../README.md) | 13,000 words | Project overview, features, installation, examples, FAQ | Everyone |
| [CLAUDE.md](../CLAUDE.md) | 24KB | Comprehensive framework documentation, API reference, tutorials | Developers |
| [BRANCHES.md](../BRANCHES.md) | 11KB | Git workflow, branch management, merge strategies | Contributors |

### Historical Documentation

| File | Purpose | Status |
|------|---------|--------|
| [REORGANIZATION_SUMMARY.md](../REORGANIZATION_SUMMARY.md) | Assets reorganization (Nov 18, 2025) | ✅ Complete |
| [REORGANIZATION_COMPLETE.txt](../REORGANIZATION_COMPLETE.txt) | Completion report | ✅ Complete |

---

## Assets/ Folder Documentation

### Framework Documentation

**Location:** `Assets/MercuryMessaging/`

| File | Purpose |
|------|---------|
| [Readme.md](../Assets/MercuryMessaging/Readme.md) | Framework quick-start (lightweight version) |

**Note:** For comprehensive framework docs, see [CLAUDE.md](../CLAUDE.md)

### Project Assets

| File | Purpose |
|------|---------|
| [Assets/_Project/README.md](../Assets/_Project/README.md) | Custom project assets guide |
| [Assets/ThirdParty/README.md](../Assets/ThirdParty/README.md) | Third-party plugins documentation |
| [Assets/XRConfiguration/README.md](../Assets/XRConfiguration/README.md) | VR/XR configuration guide |

### User Study

| File | Size | Purpose |
|------|------|---------|
| [Assets/UserStudy/README.md](../Assets/UserStudy/README.md) | 11,000 words | Traffic simulation scene documentation |

**Related:** See [`dev/active/user-study/`](./active/user-study/) for development planning

---

## Dev/ Folder Documentation

### Development Infrastructure

| File | Purpose | When to Read |
|------|---------|--------------|
| [dev/README.md](./README.md) | Dev docs pattern explanation | First time in dev/ |
| [dev/TECHNICAL_DEBT.md](./TECHNICAL_DEBT.md) | Active technical debt tracking | Planning work |
| [dev/DOCUMENTATION_INVENTORY.md](./DOCUMENTATION_INVENTORY.md) | Complete file catalog | Finding specific docs |
| [dev/DOCUMENTATION_INDEX.md](./DOCUMENTATION_INDEX.md) | This file | Navigation |

### Historical Session Handoffs (Archived)

| File | Purpose |
|------|---------|
| [dev/archive/session-handoffs/SESSION_HANDOFF_2025-11-18.md](./archive/session-handoffs/SESSION_HANDOFF_2025-11-18.md) | Nov 18 session handoff |
| [dev/archive/session-handoffs/CONTEXT_RESET_READY_2025-11-18.md](./archive/session-handoffs/CONTEXT_RESET_READY_2025-11-18.md) | Nov 18 context summary |
| [dev/archive/documentation-history/DOCUMENTATION_COMPLETE_2025-11-18.md](./archive/documentation-history/DOCUMENTATION_COMPLETE_2025-11-18.md) | Nov 18 documentation milestone |

---

## Active Tasks (dev/active/)

**Active Folders:** 6 (5 planning + 1 in-progress)
**Archived Tasks:** 7 (see archive section below)

### Master Documentation

| File | Purpose |
|------|---------|
| [MASTER-SUMMARY.md](./active/MASTER-SUMMARY.md) | Overview of all active tasks (v2.0 - Nov 20) |
| [STATUS-REPORT.md](./active/STATUS-REPORT.md) | Current status (Nov 20 consolidation) |
| [SESSION_HANDOFF.md](./active/SESSION_HANDOFF.md) | Active tasks status (Nov 18) |

---

### Task #1: User Study (Traffic Simulation)

**Priority:** IN PROGRESS | **Effort:** ~100h invested, 140h remaining

| File | Purpose |
|------|---------|
| [user-study-context.md](./active/user-study/user-study-context.md) | Architecture, 11 scripts documented (11,000+ words) |
| [user-study-tasks.md](./active/user-study/user-study-tasks.md) | 240+ tasks across 8 intersections |

**Status:** 1 of 8 intersections complete

**Related:** See [Assets/UserStudy/README.md](../Assets/UserStudy/README.md) for implementation guide

---

### Task #2: Routing Optimization

**Priority:** CRITICAL | **Effort:** 420h (10-11 weeks)

| File | Purpose |
|------|---------|
| [README.md](./active/routing-optimization/README.md) | Overview, goals, scope |
| [routing-optimization-context.md](./active/routing-optimization/routing-optimization-context.md) | Advanced routing patterns, topology analysis |
| [routing-optimization-tasks.md](./active/routing-optimization/routing-optimization-tasks.md) | 50+ tasks with acceptance criteria |

**Key Features:** Sibling/cousin routing, custom paths, routing table optimization (3-5x speedup)

---

### Task #3: Network Performance

**Priority:** HIGH | **Effort:** 500h (12-13 weeks)

| File | Purpose |
|------|---------|
| [README.md](./active/network-performance/README.md) | Overview, goals, scope |
| [network-performance-context.md](./active/network-performance/network-performance-context.md) | Delta sync, priority queuing, zero GC allocation |
| [network-performance-tasks.md](./active/network-performance/network-performance-tasks.md) | 29 tasks across 2 phases |

**Key Features:** 50-80% bandwidth reduction, 4-tier priority queuing, object pooling

---

### Task #4: Visual Composer

**Priority:** MEDIUM | **Effort:** 360h (9 weeks)

| File | Purpose |
|------|---------|
| [README.md](./active/visual-composer/README.md) | Overview, goals, scope |
| [visual-composer-context.md](./active/visual-composer/visual-composer-context.md) | Unity GraphView architecture |
| [visual-composer-tasks.md](./active/visual-composer/visual-composer-tasks.md) | 22 tasks (212 hours) |

**Key Features:** Hierarchy mirroring, network templates, drag-drop visual composer, validator

---

### Task #5: Standard Library

**Priority:** MEDIUM | **Effort:** 290h (7 weeks)

| File | Purpose |
|------|---------|
| [README.md](./active/standard-library/README.md) | Overview, goals, scope |
| [standard-library-context.md](./active/standard-library/standard-library-context.md) | 40+ message types, versioning system (11,000+ chars) |
| [standard-library-tasks.md](./active/standard-library/standard-library-tasks.md) | 37 tasks across 7 phases (228 hours) |

**Key Features:** UI, AppState, Input, Task message libraries with versioning

---

### Task #6: Performance Analysis (📊 SCRIPTS ONLY)

**Status:** Analysis complete (Nov 20), scripts kept for future re-runs

| File | Purpose |
|------|---------|
| [README.md](./active/performance-analysis/README.md) | Usage guide for analysis tools |
| [analyze_performance.py](./active/performance-analysis/analyze_performance.py) | Python analysis script |
| [requirements.txt](./active/performance-analysis/requirements.txt) | Python dependencies |

**Purpose:** Reusable performance testing infrastructure for future validation

**Related:** See [dev/archive/performance-analysis-final/](./archive/performance-analysis-final/) for optimization results

---

## Archived Tasks (dev/archive/)

**Total Archived:** 7 completed tasks with full documentation

### Completed Implementation Tasks

| Folder | Completion Date | Effort | Status |
|--------|----------------|--------|--------|
| [framework-analysis/](./archive/framework-analysis/) | Nov 18, 2025 | Analysis complete | ✅ 10 bottlenecks identified, 6 quick wins documented |
| [custom-method-extensibility/](./archive/custom-method-extensibility/) | Nov 20, 2025 | 30-40h | ✅ MmExtendableResponder implemented (commit 01893adf) |
| [performance-analysis-final/](./archive/performance-analysis-final/) | Nov 20, 2025 | 15.5h | ✅ 2-2.2x frame time improvement validated |
| [performance-analysis-baseline/](./archive/performance-analysis-baseline/) | Nov 20, 2025 | N/A | ✅ Pre-optimization baseline data preserved |

### Completed Organizational Tasks

| Folder | Completion Date | Purpose |
|--------|----------------|---------|
| [reorganization/](./archive/reorganization/) | Nov 18, 2025 | Assets folder restructure (29 → 10 folders) |
| [mercury-improvements-original/](./archive/mercury-improvements-original/) | Nov 18, 2025 | Original master plan (split into 6 focused tasks) |
| [quick-win-scene/](./archive/quick-win-scene/) | Earlier | Scene setup for performance testing |

### Historical Documentation

| Folder | Purpose |
|--------|---------|
| [session-handoffs/](./archive/session-handoffs/) | Session handoff documents (Nov 18) |
| [documentation-history/](./archive/documentation-history/) | Documentation milestone records |

---

## Claude Custom Files

### Custom Agents (.claude/agents/)

| Agent | Purpose |
|-------|---------|
| `code-architecture-reviewer.md` | Review code adherence to best practices |
| `code-refactor-master.md` | Comprehensive refactoring agent |
| `documentation-architect.md` | Documentation planning and creation |
| `plan-reviewer.md` | Review implementation plans |
| `refactor-planner.md` | Plan refactoring work |
| `web-research-specialist.md` | Web research and investigation |

### Custom Commands (.claude/commands/)

| Command | Purpose |
|---------|---------|
| `dev-docs.md` | Create dev documentation structure |
| `dev-docs-update.md` | Update existing dev documentation |

---

## Documentation by Purpose

### For Learning the Framework

1. [README.md](../README.md) - What is MercuryMessaging?
2. [CLAUDE.md](../CLAUDE.md) - Complete framework reference
3. [Assets/MercuryMessaging/Examples/](../Assets/MercuryMessaging/Examples/) - Code examples
4. Tutorial scenes in Unity

### For Contributing Code

1. [BRANCHES.md](../BRANCHES.md) - Git workflow
2. [CLAUDE.md](../CLAUDE.md) - Architecture and patterns
3. [dev/archive/framework-analysis/](./archive/framework-analysis/) - Performance considerations
4. Task-specific context files

### For Project Planning

1. [dev/active/MASTER-SUMMARY.md](./active/MASTER-SUMMARY.md) - All tasks overview (v2.0)
2. [dev/active/STATUS-REPORT.md](./active/STATUS-REPORT.md) - Current status (Nov 20)
3. Individual task READMEs - Effort estimates
4. Individual task files - Detailed breakdowns

### For Understanding Past Work

1. [dev/archive/](./archive/) - 7 completed tasks with full documentation
2. [dev/archive/framework-analysis/](./archive/framework-analysis/) - Performance analysis findings
3. [dev/archive/custom-method-extensibility/](./archive/custom-method-extensibility/) - MmExtendableResponder implementation
4. [dev/archive/performance-analysis-final/](./archive/performance-analysis-final/) - Optimization validation results
5. [REORGANIZATION_SUMMARY.md](../REORGANIZATION_SUMMARY.md) - Assets reorganization history

---

## Documentation Maintenance

### When to Update

**After Each Session:**
- Update `SESSION_HANDOFF.md` with current status
- Update task progress in relevant task files
- Update `CONTEXT_RESET_READY.md` if major milestones reached

**After Completing a Task:**
- Move task folder to `dev/archive/`
- Update `MASTER-SUMMARY.md`
- Create completion archive report

**After Major Changes:**
- Update `CLAUDE.md` if framework changed
- Update README.md if project scope changed
- Update this index if structure changed

### Who Maintains What

**Root Documentation:**
- README.md - Project maintainers
- CLAUDE.md - Framework developers
- BRANCHES.md - Git workflow team

**Dev Documentation:**
- Active task files - Task owners
- MASTER-SUMMARY.md - Project manager
- SESSION_HANDOFF.md - Session lead

**Assets Documentation:**
- Component READMEs - Component owners
- User study docs - User study team

---

## File Statistics

**Total Documentation:** 70+ markdown files (~150,000 words)
**Active Tasks:** 6 folders (5 planning + 1 in-progress)
**Archived Tasks:** 7 completed tasks
**Coverage:** 100% of active work documented

**Active Development:**
- User-study: 2 files (in progress)
- Planning tasks: 12 files (4 tasks × 3 files each)
- Performance scripts: 3 files (reusable tools)

**Archive:**
- Completed tasks: 70+ files
- Session handoffs: 2 files
- Documentation history: 1 file

**Root Documentation:**
- Project docs: 3 files (README, CLAUDE, BRANCHES)
- Dev infrastructure: 4 files

---

## Quick Links by Role

### I'm a...

**New Developer:**
1. [README.md](../README.md)
2. [CLAUDE.md](../CLAUDE.md)
3. Examples folder

**Framework Contributor:**
1. [CLAUDE.md](../CLAUDE.md)
2. [dev/archive/framework-analysis/](./archive/framework-analysis/)
3. [dev/active/routing-optimization/](./active/routing-optimization/)

**Project Manager:**
1. [dev/active/MASTER-SUMMARY.md](./active/MASTER-SUMMARY.md)
2. [dev/active/STATUS-REPORT.md](./active/STATUS-REPORT.md)
3. [dev/TECHNICAL_DEBT.md](./TECHNICAL_DEBT.md)

**User Study Researcher:**
1. [Assets/UserStudy/README.md](../Assets/UserStudy/README.md)
2. [dev/active/user-study/](./active/user-study/)

**Performance Engineer:**
1. [dev/archive/framework-analysis/](./archive/framework-analysis/)
2. [dev/archive/performance-analysis-final/](./archive/performance-analysis-final/)
3. [dev/active/routing-optimization/](./active/routing-optimization/)
4. [dev/active/network-performance/](./active/network-performance/)

---

## Need Help Finding Something?

**Use this decision tree:**

```
What do you need?
├─ Learn about the project → README.md
├─ Learn the framework → CLAUDE.md
├─ See code examples → Assets/MercuryMessaging/Examples/
├─ Start development work → dev/active/MASTER-SUMMARY.md
├─ Understand current status → dev/active/STATUS-REPORT.md
├─ Understand a specific task → dev/active/[task]/README.md
├─ See implementation details → dev/active/[task]/[task]-context.md
├─ Track task progress → dev/active/[task]/[task]-tasks.md
├─ Review completed work → dev/archive/
├─ Understand git workflow → BRANCHES.md
└─ Find a specific file → dev/DOCUMENTATION_INVENTORY.md
```

---

**Document Version:** 2.0
**Last Updated:** 2025-11-20 (Consolidation - 3 tasks archived, 6 active)
**Maintained By:** Development Team

**This file:** Central navigation hub - bookmark this!
