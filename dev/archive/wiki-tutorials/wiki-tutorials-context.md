# Wiki Tutorials Context

**Last Updated:** 2025-12-12

---

## Key Decisions

### Wiki Location
- **Correct URL:** https://github.com/ColumbiaCGUI/MercuryMessaging/wiki
- **Clone URL:** `git clone https://github.com/ColumbiaCGUI/MercuryMessaging.wiki.git`
- **Note:** Wiki is at ColumbiaCGUI org, not CGUI-Lab

### Documentation Architecture (HYBRID APPROACH - 2025-12-12)
- **Wiki:** Tutorials & how-to guides (learning path)
- **Documentation/:** Technical reference (API, performance, testing)
- **CLAUDE.md:** References both, provides AI context
- **docs/ folder:** Doxygen-generated API reference (keep unchanged)

### Tutorial Numbering (REVISED 2025-12-12)
**Major change: DSL moved to Tutorial 5, everything renumbered**

| # | Title | Status | Notes |
|---|-------|--------|-------|
| 1 | Introduction | UPDATE | Add DSL preview |
| 2 | Basic Routing | UPDATE | Add DSL examples |
| 3 | Custom Responders | UPDATE | Add MmExtendableResponder |
| 4 | Custom Messages | UPDATE | Modernize with DSL |
| **5** | **Fluent DSL API** | **NEW** | Moved up from planned 11 |
| **6** | **Networking with FishNet** | **NEW** | Primary networking |
| 7 | Networking with Fusion 2 | RENAME | Was 5a |
| 8 | Switch Nodes & FSM | CREATE | Was "Coming Soon" |
| 9 | Task Management | CREATE | Was "Coming Soon" |
| 10 | Application State | CREATE | Was "Coming Soon" |
| 11 | Advanced Networking | CREATE | Was "Coming Soon" |
| 12 | VR Behavioral Experiment | REVIEW | Was 10 |
| 13 | Spatial & Temporal | STUB | Not implemented |
| 14 | Performance Optimization | STUB | Not implemented |

### PUN2 Removal (2025-12-12)
- **DELETE:** Pun2Backend.cs, MmNetworkResponderPhoton.cs
- **DELETE:** Assets/Photon/PhotonLibs/, Assets/Plugins/Photon/PhotonLibs/
- **KEEP:** Assets/Photon/Fusion/ (Fusion 2)
- **KEEP:** Assets/Plugins/Photon/Fusion/

### Drafting Approach
- Draft tutorials in `dev/wiki-drafts/` first
- Review before pushing to wiki
- Allows code review and testing

---

## Current Wiki State (Fetched 2025-12-12)

### Existing Pages
- Tutorials 1-4: Have content
- Tutorial 5a: Photon Fusion V2 (will become Tutorial 7)
- Tutorial 5b: PUN2 (will be DELETED)
- Tutorials 6-9: All marked "(Coming Soon)" - **NO CONTENT EXISTS**
- Tutorial 10: VR Experiment (exists, will become Tutorial 12)

### Tutorial 3 vs Tutorial 4 Difference
- **Tutorial 3:** Creating **Responders** (message HANDLERS)
  - Extend `MmBaseResponder`
  - Override `MmInvoke()` method
  - Custom `MmMethod` enum (100+)
- **Tutorial 4:** Creating **Messages** (message TYPES)
  - Extend `MmMessage`
  - Override `Copy()`, `Serialize()`
  - Custom `MmMessageType` enum (1100+)

---

## Research-Based Best Practices (2025-12-12)

Based on Twilio SDK, Unity docs, and API documentation guides:

1. **Code is the narrative** - Tell the story through code
2. **Instant gratification** - Working results in minutes
3. **Copy-paste ready** - Complete, functional examples
4. **Conversational tone** - Informal, human language
5. **Step-by-step** - Small, digestible steps
6. **Why before How** - Motivation before implementation

### Standard Tutorial Template
```markdown
# Tutorial N: [Title]

## Overview
[2-3 sentences - what and WHY it matters]

## What You'll Learn
- Bullet points

## Prerequisites
- Links to prior tutorials

## Quick Start (Copy-Paste)
[Minimal working example - immediate results]

## Step-by-Step Guide
### Step 1: [Action Verb] the [Thing]
...

## Complete Example
[Full working code]

## Common Mistakes
| Mistake | Solution |

## Next Steps
- Links to related tutorials
```

---

## Files to Create (dev/wiki-drafts/)

```
dev/wiki-drafts/
├── tutorials/
│   ├── tutorial-01-introduction.md
│   ├── tutorial-02-basic-routing.md
│   ├── tutorial-03-custom-responders.md
│   ├── tutorial-04-custom-messages.md
│   ├── tutorial-05-fluent-dsl.md         # NEW
│   ├── tutorial-06-fishnet.md            # NEW
│   ├── tutorial-07-fusion2.md
│   ├── tutorial-08-switch-nodes-fsm.md   # NEW
│   ├── tutorial-09-task-management.md    # NEW
│   ├── tutorial-10-application-state.md  # NEW
│   ├── tutorial-11-advanced-networking.md # NEW
│   ├── tutorial-12-vr-experiment.md
│   ├── tutorial-13-spatial-temporal.md   # STUB
│   └── tutorial-14-performance.md        # STUB
├── pages/
│   ├── home.md
│   └── tutorials-index.md
└── README.md
```

---

## Files to Modify/Delete (Codebase)

### DELETE
```
Assets/MercuryMessaging/Protocol/Network/Backends/Pun2Backend.cs
Assets/MercuryMessaging/Protocol/Network/MmNetworkResponderPhoton.cs
```

### UPDATE
```
CLAUDE.md - Restructure documentation section
Documentation/OVERVIEW.md - Remove Photon mention
FILE_REFERENCE.md - Remove PUN2 files
CONTRIBUTING.md - Update PHOTON_AVAILABLE example
Assets/MercuryMessaging/Editor/MercuryThirdPartyUtils.cs - Remove PUN2 detection
[Network files] - Update comments removing PUN2 references
```

---

## Content Sources for New Tutorials

### Tutorial 5 (Fluent DSL)
- `Documentation/DSL/README.md`
- `Documentation/DSL/API_GUIDE.md`
- `Documentation/Tutorials/DSL_QUICK_START.md`
- `Examples/Tutorials/DSL/*.cs`

### Tutorial 6 (FishNet)
- `Tests/Network/README.md`
- `dev/active/networking/networking-context.md`
- `Protocol/Network/Backends/FishNetBackend.cs`
- `Protocol/Network/Backends/FishNetResolver.cs`

### Tutorial 8-10 (Advanced Topics)
- `Protocol/Nodes/MmRelaySwitchNode.cs`
- `Protocol/DSL/MmAppState.cs`
- `Task/MmTaskManager.cs`
- `AppState/MmAppStateResponder.cs`

---

## Session Notes

### 2025-12-12: Comprehensive Planning Session
- Fetched current wiki content via WebFetch
- Discovered Tutorials 6-9 are "Coming Soon" (no content)
- User decided: Keep Fusion 2, remove PUN2 only
- User decided: Draft in dev/wiki-drafts/ first
- User decided: Update CLAUDE.md now
- User decided: Priority 1-5 updates → 6-10 → DSL
- Renumbered tutorials: DSL at 5, networking at 6-7
- Research on tutorial best practices (Twilio, Unity, API docs)
- Created comprehensive plan at `.claude/plans/transient-giggling-sonnet.md`

### 2025-12-01: Initial Planning
- Discovered wiki at ColumbiaCGUI (not CGUI-Lab)
- Confirmed existing tutorials 1-10 structure
- Initial planning for new tutorials

---

## Next Steps (Start Here)

1. **Phase A:** Create `dev/wiki-drafts/` folder structure, PUN2 cleanup
2. **Phase B:** Update CLAUDE.md with wiki links
3. **Phase C:** Draft tutorials 1-4 updates
4. **Phase D:** Draft tutorial 5 (Fluent DSL)
5. **Phase E:** Draft tutorials 6-7 (Networking)
6. Continue with remaining phases...

**Estimated Total Effort:** ~31 hours

---

*Context file for seamless continuation after context reset*
