# Wiki Tutorials Tasks

**Last Updated:** 2025-12-12
**Total Estimated Effort:** ~31 hours

---

## Phase A: Setup & PUN2 Cleanup (2h) ✅ COMPLETE

- [x] Create `dev/wiki-drafts/` folder structure
- [x] Create `dev/wiki-drafts/README.md` with push instructions
- [x] Delete `Pun2Backend.cs` and `Pun2Backend.cs.meta`
- [x] Delete `MmNetworkResponderPhoton.cs` and `.meta`
- [x] Update `MercuryThirdPartyUtils.cs` (remove PUN2 detection)
- [x] Update comment references in network files (IMmNetworkBackend, etc.)
- [x] Update `Documentation/OVERVIEW.md` (remove Photon mention)
- [x] Update `FILE_REFERENCE.md` (remove PUN2 files)
- [x] Update `CONTRIBUTING.md` (update PHOTON_AVAILABLE example to FishNet)

---

## Phase B: Update CLAUDE.md (1h) ✅ COMPLETE

- [x] Restructure documentation section with wiki links
- [x] Add new tutorial numbering (1-14)
- [x] Reference wiki for tutorials, Documentation/ for tech reference
- [x] Update @import references (kept existing, added Source Generators link)

---

## Phase C: Draft Tutorials 1-4 Updates (6h) ✅ COMPLETE

### Tutorial 1: Introduction to MercuryMessaging
- [x] Create `dev/wiki-drafts/tutorials/tutorial-01-introduction.md`
- [x] Add DSL quick preview section
- [x] Update code examples with both traditional and DSL syntax
- [x] Follow research-based tutorial template

### Tutorial 2: Basic Routing
- [x] Create `dev/wiki-drafts/tutorials/tutorial-02-basic-routing.md`
- [x] Add DSL routing examples alongside traditional
- [x] Document direction methods (ToChildren, ToDescendants, etc.)

### Tutorial 3: Creating Custom Responders
- [x] Create `dev/wiki-drafts/tutorials/tutorial-03-custom-responders.md`
- [x] Add MmExtendableResponder section as "Modern Alternative"
- [x] Show RegisterCustomHandler() pattern
- [x] Compare switch statement vs handler registration

### Tutorial 4: Creating Custom Messages
- [x] Create `dev/wiki-drafts/tutorials/tutorial-04-custom-messages.md`
- [x] Modernize code examples with DSL
- [x] Add Copy()/Serialize() best practices
- [x] Cross-reference with Tutorial 3

---

## Phase D: Draft Tutorial 5 - Fluent DSL (4h) ✅ COMPLETE

- [x] Create `dev/wiki-drafts/tutorials/tutorial-05-fluent-dsl.md`
- [x] Part 1: Introduction (Why DSL? 77% code reduction)
- [x] Part 2: Tier 1 - Auto-Execute Methods (BroadcastX, NotifyX)
- [x] Part 3: Tier 2 - Fluent Chains (.Send().ToX().Execute())
- [x] Part 4: Property-Based Syntax (relay.To.Children.Send())
- [x] Part 5: Direction Targeting Table
- [x] Part 6: Filter Methods
- [x] Part 7: From Responders (myResponder.BroadcastX())
- [x] Part 8: Migration Examples Table
- [x] Part 9: Interactive Demo Instructions (keyboard controls)

---

## Phase E: Draft Tutorials 6-7 - Networking (4h) ✅ COMPLETE

### Tutorial 6: Networking with FishNet
- [x] Create `dev/wiki-drafts/tutorials/tutorial-06-fishnet.md`
- [x] Overview of FishNet integration
- [x] Setup instructions (Package Manager, define symbols)
- [x] MmNetworkBridge + FishNetBackend configuration
- [x] Bidirectional messaging code examples
- [x] ParrelSync testing instructions
- [x] Deterministic path-based IDs explanation
- [x] Common mistakes table

### Tutorial 7: Networking with Photon Fusion 2
- [x] Create `dev/wiki-drafts/tutorials/tutorial-07-fusion2.md`
- [x] Adapt from existing 5a wiki content
- [x] Update code examples for consistency
- [x] Add Fusion2Backend configuration
- [x] Cross-reference with Tutorial 6

---

## Phase F: Draft Tutorials 8-11 - Advanced (8h) ✅ COMPLETE

### Tutorial 8: Switch Nodes & FSM
- [x] Create `dev/wiki-drafts/tutorials/tutorial-08-switch-nodes-fsm.md`
- [x] MmRelaySwitchNode introduction
- [x] FSM-enabled relay behavior
- [x] SelectedFilter.Selected explanation
- [x] ConfigureStates() fluent API
- [x] GoTo(), GoToPrevious(), IsInState() methods
- [x] Practical example: Game state machine

### Tutorial 9: Task Management
- [x] Create `dev/wiki-drafts/tutorials/tutorial-09-task-management.md`
- [x] MmTaskManager overview
- [x] Task sequencing and workflows
- [x] IMmTaskInfoCollectionLoader (JSON/CSV)
- [x] Data collection integration (MmDataCollector)
- [x] User study example

### Tutorial 10: Application State Management
- [x] Create `dev/wiki-drafts/tutorials/tutorial-10-application-state.md`
- [x] MmAppStateResponder introduction
- [x] Global state handling
- [x] MmAppState.Configure() fluent API
- [x] OnEnter/OnExit callbacks
- [x] Scene management integration
- [x] Practical example: Menu → Gameplay → Pause flow

### Tutorial 11: Advanced Networking
- [x] Create `dev/wiki-drafts/tutorials/tutorial-11-advanced-networking.md`
- [x] Network architecture overview (3-layer)
- [x] IMmNetworkBackend interface
- [x] Custom backend implementation guide
- [x] MmBinarySerializer format (17-byte header)
- [x] IMmGameObjectResolver explained
- [x] Network filters (.OverNetwork(), .LocalOnly())
- [x] Troubleshooting guide

---

## Phase G: Draft Tutorial 12 & Stubs (2h) ✅ COMPLETE

### Tutorial 12: VR Behavioral Experiment
- [x] Create `dev/wiki-drafts/tutorials/tutorial-12-vr-experiment.md`
- [x] Complete Go/No-Go cognitive task implementation
- [x] VR input with XR Interaction Toolkit
- [x] Data collection patterns and best practices

### Tutorial 13: Spatial & Temporal Filtering (STUB)
- [x] Create `dev/wiki-drafts/tutorials/tutorial-13-spatial-temporal.md`
- [x] Add "Coming Soon" notice
- [x] List planned features: Within(), InCone(), After(), Every()
- [x] Reference current available features

### Tutorial 14: Performance Optimization (STUB)
- [x] Create `dev/wiki-drafts/tutorials/tutorial-14-performance.md`
- [x] Add "Coming Soon" notice
- [x] List planned topics: PerformanceMode, Source Generators
- [x] Reference Documentation/PERFORMANCE.md for existing info

---

## Phase H: Index & Home Pages (2h) ✅ COMPLETE

- [x] Create `dev/wiki-drafts/pages/home.md`
  - [x] Add "What's New" section (DSL, FishNet, analyzers, etc.)
  - [x] Update getting started section
  - [x] Add feature highlights

- [x] Create `dev/wiki-drafts/pages/tutorials-index.md`
  - [x] New numbering scheme (1-14)
  - [x] Section groupings (Foundation, Networking, Advanced, Specialized, Future)
  - [x] Brief descriptions for each tutorial

- [x] Add cross-links between related tutorials

---

## Phase I: Review & Finalize (2h) ✅ COMPLETE

- [x] Verify all code examples compile
- [x] Check consistency of tone and formatting
- [x] Ensure all internal links work (standardized to short-form)
- [x] Review against research-based best practices
- [x] Ready for wiki push

---

## Progress Summary

| Phase | Status | Tasks |
|-------|--------|-------|
| A: Setup & PUN2 Cleanup | ✅ Complete | 9/9 |
| B: Update CLAUDE.md | ✅ Complete | 4/4 |
| C: Tutorials 1-4 Updates | ✅ Complete | 12/12 |
| D: Tutorial 5 (DSL) | ✅ Complete | 10/10 |
| E: Tutorials 6-7 (Networking) | ✅ Complete | 13/13 |
| F: Tutorials 8-11 (Advanced) | ✅ Complete | 20/20 |
| G: Tutorial 12 & Stubs | ✅ Complete | 8/8 |
| H: Index & Home Pages | ✅ Complete | 6/6 |
| I: Review & Finalize | ✅ Complete | 5/5 |
| **Total** | **✅ Complete** | **87/87** |

---

## Quick Reference: Files to Create

```
dev/wiki-drafts/
├── README.md                               # Push instructions
├── tutorials/
│   ├── tutorial-01-introduction.md
│   ├── tutorial-02-basic-routing.md
│   ├── tutorial-03-custom-responders.md
│   ├── tutorial-04-custom-messages.md
│   ├── tutorial-05-fluent-dsl.md           # NEW - Critical
│   ├── tutorial-06-fishnet.md              # NEW - Critical
│   ├── tutorial-07-fusion2.md              # From 5a
│   ├── tutorial-08-switch-nodes-fsm.md     # NEW
│   ├── tutorial-09-task-management.md      # NEW
│   ├── tutorial-10-application-state.md    # NEW
│   ├── tutorial-11-advanced-networking.md  # NEW
│   ├── tutorial-12-vr-experiment.md        # From old 10
│   ├── tutorial-13-spatial-temporal.md     # STUB
│   └── tutorial-14-performance.md          # STUB
└── pages/
    ├── home.md
    └── tutorials-index.md
```

---

*Task checklist for wiki tutorials - Updated 2025-12-12 - ALL PHASES COMPLETE*
