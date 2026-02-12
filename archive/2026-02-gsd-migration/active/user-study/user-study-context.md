# User Study Development Context

**Last Updated:** 2025-11-21

This document captures key files, architectural decisions, dependencies, and important context for the user study development task.

---

## Key Files and Locations

### Planning Documentation (Current Location)

```
dev/active/user-study/
├── README.md                          # Overview and scene inventory
├── user-study-plan.md                 # Comprehensive strategic plan (THIS TASK)
├── user-study-context.md              # This file - key decisions and context
├── user-study-tasks.md                # Checklist for tracking progress
├── 01-smart-home-control.md           # ✅ Complete (1,200 lines, detailed)
├── 02-music-mixing-board.md           # ✅ Complete (900 lines, detailed)
├── 03-tower-defense-waves.md          # ❌ TODO (outline only)
├── 04-modular-puzzle-room.md          # ❌ TODO (outline only)
├── 05-factory-assembly-line.md        # ❌ TODO (outline only)
├── comparison-matrix.md               # ❌ TODO (code comparisons)
└── user-study-design.md               # ❌ TODO (methodology, IRB materials)
```

### MercuryMessaging Framework (Core)

**Location:** `Assets/MercuryMessaging/`

**Critical Files:**
- `Protocol/MmRelayNode.cs` (1,422 lines) - Central message router, **MOST IMPORTANT**
- `Protocol/MmBaseResponder.cs` (383 lines) - Base responder with method routing
- `Protocol/MmRelaySwitchNode.cs` (188 lines) - FSM-enabled relay node
- `Protocol/MmMessage.cs` - Message type definitions
- `Protocol/IMmResponder.cs` - Core interface
- `CLAUDE.md` - Comprehensive framework documentation

**Key Concepts:**
- Hierarchical message routing through Unity scene graph
- Tag-based filtering (8-bit system: Tag0-Tag7)
- FSM integration for state management
- Zero external dependencies
- Performance: 53-66 FPS in Editor, excellent scalability

**Documentation:**
- `CLAUDE.md` - Framework overview, architecture patterns, common workflows
- `FILE_REFERENCE.md` - Important files with descriptions
- `CONTRIBUTING.md` - Development standards, naming conventions

---

## Implementation Locations (Future)

### Scene Files (To Be Created)

```
Assets/UserStudy/
├── Scenes/
│   ├── SmartHome_Mercury.unity        # Phase 2.1
│   ├── SmartHome_Events.unity         # Phase 2.2
│   ├── MusicMixer_Mercury.unity       # Phase 3.1
│   ├── MusicMixer_Events.unity        # Phase 3.2
│   ├── TowerDefense_Mercury.unity     # Phase 4.1
│   ├── TowerDefense_Events.unity      # Phase 4.2
│   ├── PuzzleRoom_Mercury.unity       # Phase 5.1
│   ├── PuzzleRoom_Events.unity        # Phase 5.2
│   ├── AssemblyLine_Mercury.unity     # Phase 5.2
│   └── AssemblyLine_Events.unity      # Phase 5.2
```

### Script Organization

```
Assets/UserStudy/Scripts/
├── Mercury/                           # MercuryMessaging implementations
│   ├── SmartHome/
│   │   ├── SmartHomeHub.cs           # MmRelaySwitchNode root
│   │   ├── ControlPanel.cs           # MmBaseResponder UI
│   │   ├── SmartLight.cs             # MmBaseResponder device
│   │   ├── Thermostat.cs             # MmBaseResponder device
│   │   ├── SmartBlinds.cs            # MmBaseResponder device
│   │   └── MusicPlayer.cs            # MmBaseResponder device
│   ├── MusicMixer/
│   │   ├── MixerHub.cs               # MmRelaySwitchNode
│   │   ├── AudioTrack.cs             # MmBaseResponder
│   │   ├── AudioEffect.cs            # MmBaseResponder base
│   │   ├── ReverbEffect.cs           # Inherits AudioEffect
│   │   ├── DelayEffect.cs            # Inherits AudioEffect
│   │   ├── EQEffect.cs               # Inherits AudioEffect
│   │   └── UI_MixerPanel.cs          # MmBaseResponder
│   ├── TowerDefense/ (8 scripts)
│   ├── PuzzleRoom/ (6 scripts)
│   └── AssemblyLine/ (7 scripts)
│
└── UnityEvents/                       # Unity Events implementations
    ├── SmartHome/
    │   ├── SmartHomeController.cs    # Central controller (large)
    │   ├── SmartLightBehaviour.cs
    │   ├── ThermostatBehaviour.cs
    │   ├── SmartBlindsBehaviour.cs
    │   ├── MusicPlayerBehaviour.cs
    │   ├── RoomController.cs         # Optional grouping
    │   └── ISmartDevice.cs           # Interface
    ├── MusicMixer/ (8 scripts)
    ├── TowerDefense/ (10 scripts)
    ├── PuzzleRoom/ (8 scripts)
    └── AssemblyLine/ (9 scripts)
```

### Study Infrastructure

```
Assets/UserStudy/StudyInfrastructure/
├── MetricsCollector.cs               # Automatic LOC counting, timing
├── DataLogger.cs                     # CSV export
├── NASATLXQuestionnaire.cs           # 6-dimension workload survey
├── TaskInstructions/
│   ├── SmartHome_Task1.md
│   ├── SmartHome_Task2.md
│   ├── ... (25 task files total)
│   └── AssemblyLine_Task5.md
├── TaskUI.cs                         # In-Unity task display
└── RandomizationManager.cs           # Counterbalancing
```

---

## Architectural Decisions

### Decision 1: Within-Subject Design (Mercury vs Events)

**Date:** 2025-11-21
**Status:** ✅ Confirmed

**Context:**
User study needs to compare two approaches (MercuryMessaging vs Unity Events) on multiple metrics.

**Decision:**
Use within-subject design where each participant tries BOTH approaches on different scenes.

**Rationale:**
- Reduces individual variability (each person is their own control)
- Higher statistical power with fewer participants (15-20 vs 30-40)
- Captures subjective preference directly
- Standard for HCI comparison studies

**Alternatives Considered:**
- Between-subjects (half use Mercury, half use Events) - requires more participants
- Mixed design (some tasks within, some between) - too complex

**Implications:**
- Need counterbalancing (randomize order to avoid learning effects)
- Each participant needs 2-3 hours (tries multiple scenes)
- Must ensure scenes are comparable in difficulty

---

### Decision 2: Progressive Complexity (Simple → Medium)

**Date:** 2025-11-21
**Status:** ✅ Confirmed

**Context:**
5 scenes needed with varying complexity to avoid ceiling/floor effects.

**Decision:**
Order scenes by complexity:
1. Smart Home (Simple, 30-45min)
2. Music Mixer (Simple, 30-45min)
3. Tower Defense (Medium, 45-90min)
4. Puzzle Room (Simple-Medium, 30-60min)
5. Assembly Line (Medium, 60-90min)

**Rationale:**
- Simple scenes establish baseline, show clear differences
- Medium scenes show scalability and challenge
- Avoid "too easy" (ceiling effect) or "too hard" (floor effect)
- Progressive difficulty keeps participants engaged

**Implications:**
- Participants start with simple scene (Smart Home recommended)
- Can adjust which scenes used based on time constraints
- Each scene should be piloted to validate difficulty

---

### Decision 3: Implement Mercury Version First

**Date:** 2025-11-21
**Status:** ✅ Confirmed

**Context:**
For each scene pair, need to decide implementation order.

**Decision:**
Always implement MercuryMessaging version first, then Unity Events version.

**Rationale:**
- Mercury version establishes feature requirements
- Easier to ensure feature parity (Events matches Mercury)
- Avoids accidentally adding Mercury-only features later
- Documents "ground truth" for comparison

**Alternatives Considered:**
- Parallel implementation (both at once) - harder to ensure parity
- Events first - risk Mercury version having extra features

**Implications:**
- Mercury scripts serve as specification for Events version
- Must document all features clearly
- Events version may take longer (intentional)

---

### Decision 4: Zero Inspector Connections for Mercury

**Date:** 2025-11-21
**Status:** ✅ Confirmed (Design Goal)

**Context:**
MercuryMessaging supports automatic registration, but could theoretically use some Inspector references.

**Decision:**
Mercury implementations will use ZERO manual Inspector connections (only hierarchy parenting).

**Rationale:**
- Demonstrates loose coupling advantage
- Clear metric for comparison (0 vs 30-50)
- Shows "pure" Mercury approach
- Matches framework design philosophy

**Exceptions:**
- UI references (buttons, text fields) allowed - not messaging-related
- Prefab references for instantiation allowed
- Material/asset references allowed

**Implications:**
- All device discovery via hierarchy traversal
- All communication via messages
- Tag-based filtering for targeting
- May require small refactors to eliminate references

---

### Decision 5: Feature Parity Over Performance

**Date:** 2025-11-21
**Status:** ✅ Confirmed

**Context:**
Unity Events version may be slower/less elegant, but must have same features.

**Decision:**
Prioritize feature parity over performance or elegance. Unity Events versions should be "reasonably well-written" but may be more verbose/complex.

**Rationale:**
- Fair comparison requires identical functionality
- Differences in code complexity are part of findings
- Avoid accusations of "straw man" Events implementation
- Show realistic Unity Events usage

**Guidelines:**
- Use standard Unity Events patterns (not anti-patterns)
- Don't artificially inflate LOC in Events version
- Do show necessary complexity (controllers, lists, wiring)
- Follow Unity best practices

**Implications:**
- Events implementations will naturally be longer
- Some patterns (tag filtering) require workarounds
- Inspector wiring is legitimate Events workflow

---

### Decision 6: Tag Assignment Strategy

**Date:** 2025-11-21
**Status:** ✅ Confirmed

**Context:**
MercuryMessaging has 8-bit tag system (Tag0-Tag7). Need consistent strategy.

**Decision:**
Standard tag assignments across all scenes:
- **Tag0** - Primary objects (Lights, Drums, Enemies, Switches)
- **Tag1** - Secondary objects (Climate, Bass, Towers, Doors)
- **Tag2** - Tertiary objects (Entertainment, Melody, Spawners, Lights)
- **Tag3** - Quaternary objects (Effects, UI, Conveyors, Items)
- **Tag4-7** - Scene-specific as needed

**Rationale:**
- Consistency aids learning
- Documents common usage pattern
- Easy to explain to participants
- Scalable (8 tags sufficient for all scenes)

**Implications:**
- Document tag meanings in each scene
- Participants learn tag concept early
- Events versions need equivalent grouping mechanism

---

### Decision 7: Pilot Testing Required for Each Scene

**Date:** 2025-11-21
**Status:** ✅ Confirmed

**Context:**
User study tasks must be validated for difficulty and clarity.

**Decision:**
Each scene must be pilot tested with 1-2 developers before inclusion in main study.

**Pilot Testing Protocol:**
1. Recruit 1-2 Unity developers (not final participants)
2. Have them complete all 5 tasks for scene
3. Collect completion times, errors, feedback
4. Refine task instructions, difficulty, planted bugs
5. Re-test if major changes made

**Acceptance Criteria:**
- Task completion times within ±30% of estimates
- Clear instructions (no confusion)
- Planted bugs appropriately challenging
- Metrics collection works correctly

**Implications:**
- Add 2-4 hours pilot time per scene
- Budget for pilot participant compensation
- May need to redesign tasks based on feedback

---

## Dependencies

### Internal Dependencies (Within This Project)

#### Phase Dependencies
- Phase 2 (Smart Home) → Phase 3 (Music Mixer)
  - Establishes implementation pattern
  - Validates metrics collection
  - Proves concept works

- Phase 3 (Music Mixer) → Phase 4 (Tower Defense)
  - Confidence in workflow
  - Refined task design

- Phase 4 (Tower Defense) → Phase 5 (Puzzle + Assembly)
  - All patterns established
  - Can move faster

- Phases 1-5 → Phase 6 (Infrastructure)
  - Need scenes to test metrics on
  - Infrastructure must support all scenes

- Phase 6 (Infrastructure) → Phase 7 (Study Execution)
  - Cannot run study without data collection
  - Must have piloted at least one scene

#### Task Dependencies (Within Phases)
- Mercury implementation → Events implementation (feature parity)
- Both implementations → Task design (need working scenes)
- Task design → Pilot testing (need tasks designed)
- Pilot testing → Refinement (based on feedback)

#### File Dependencies
- `user-study-plan.md` → All detailed planning docs (references plan structure)
- `01-smart-home-control.md` → Task 2.1-2.3 (implementation guide)
- `02-music-mixing-board.md` → Task 3.1-3.3 (implementation guide)
- `comparison-matrix.md` → All planning docs (synthesizes comparisons)
- `user-study-design.md` → Phase 6-7 (methodology for study execution)

---

### External Dependencies

#### MercuryMessaging Framework
**Status:** ✅ Complete (Production-ready)
**Version:** Based on latest (2025-11-21)
**Location:** `Assets/MercuryMessaging/`

**Required Components:**
- MmRelayNode (message routing)
- MmBaseResponder (responder base class)
- MmRelaySwitchNode (FSM support)
- MmMessage types (MessageBool, MessageFloat, MessageString, etc.)
- MmTag enum (8-bit tag system)

**Risk:** Low (framework stable and tested)

---

#### Unity Engine
**Status:** ✅ Available
**Version Required:** Unity 2021.3+ LTS
**Current Version:** 2021.3+ (project uses this)

**Required Packages:**
- TextMeshPro (built-in) - for UI text
- Unity UI (built-in) - for buttons, sliders
- Input System (optional) - for user input

**Risk:** Low (standard Unity, no exotic features)

---

#### Statistical Analysis Tools
**Status:** ⚠️ TBD (Need to choose)
**Options:**
- R with tidyverse (free, powerful)
- Python with pandas/scipy (free, familiar)
- SPSS (paid, $99/month, university license?)
- Excel (limited, but accessible)

**Required Analyses:**
- Paired t-tests (within-subject comparison)
- Effect size calculation (Cohen's d)
- Descriptive statistics (mean, SD)
- Visualization (boxplots, bar charts)

**Decision Needed:** Phase 6 (Week 15)
**Risk:** Low (many options available)

---

#### Participant Recruitment Channels
**Status:** ⚠️ TBD (Need to establish)
**Options:**
- University game dev program (local)
- Unity forums (r/Unity3D, Discord servers)
- Twitter/social media outreach
- Paid platforms (Prolific, UserTesting)

**Requirements:**
- 15-20 participants
- Unity experience (6+ months)
- C# scripting ability
- 2-3 hour availability

**Decision Needed:** Phase 1-2 (start early)
**Risk:** Medium (recruitment can be challenging)

---

#### IRB Approval (If Required)
**Status:** ⚠️ Unknown (Check with institution)
**Timeline:** 2-6 weeks typical
**Required Materials:**
- Study protocol document
- Informed consent form
- Recruitment materials
- Data management plan

**Decision Needed:** Phase 1 (Week 1-2)
**Risk:** Medium (delays possible, requirement unclear)

---

## Important Constraints

### Technical Constraints

**Unity Version:**
- Must use Unity 2021.3+ LTS (MercuryMessaging requirement)
- Cannot use features not in 2021.3 (for compatibility)

**C# Version:**
- C# 9.0 (Unity 2021.3 uses this)
- No C# 10+ features

**Platform:**
- Development on Windows/Mac (Unity Editor)
- No mobile-specific features (keep desktop-focused)

**Performance:**
- Target 60 FPS in Editor (realistic for user study)
- Avoid performance-critical scenarios (not the focus)

### Study Design Constraints

**Time Per Participant:**
- Maximum 3 hours (attention span, fatigue)
- Minimum 1.5 hours (need sufficient data)
- Target: 2-2.5 hours

**Tasks Per Scene:**
- Exactly 5 tasks per scene (consistency)
- Task duration: 5-18 minutes each
- Total per scene: 30-90 minutes

**Scenes Per Participant:**
- Minimum 2 scenes (one Mercury, one Events)
- Recommended 3-4 scenes (more data per person)
- Maximum 5 scenes (time constraint)

**Counterbalancing:**
- Half start with Mercury, half with Events
- Randomize scene order
- Ensure balanced exposure

### Resource Constraints

**Budget:**
- Maximum $5,000 for full study
- Participant compensation: $600-800 (priority)
- Software licenses: $0-$500 (use free tools if possible)
- Publication fees: $0-$3,000 (optional)

**Personnel:**
- Single developer (primary) - 12-17 weeks full-time
- Study coordinator (part-time) - 4-5 weeks at 15h/week
- Statistical analyst (consulting) - 30-45 hours

**Timeline:**
- Target: 16 weeks (realistic)
- Maximum: 24 weeks (with buffer)
- Minimum: 12 weeks (aggressive, high risk)

---

## Key Metrics Definitions

### Lines of Code (LOC)
**Definition:** Count of non-empty, non-comment lines in all C# scripts for a scene implementation.

**Exclusions:**
- Unity-generated code (e.g., Assembly definitions)
- Third-party code (e.g., TextMeshPro scripts)
- Framework code (MercuryMessaging itself)

**Measurement:**
- Automatic via `MetricsCollector.cs`
- Manual verification possible
- Count only scripts in scene-specific folders

**Expected Range:**
- Mercury: 300-450 LOC per scene
- Events: 430-750 LOC per scene
- Delta: 20-40% reduction

---

### Inspector Connections
**Definition:** Count of manual connections in Unity Inspector including:
1. Serialized field references (drag-and-drop)
2. UnityEvent listener connections
3. List/array element additions

**Measurement:**
- Manual count during implementation
- Document in spreadsheet per scene
- Verify during code review

**Expected Range:**
- Mercury: 0 connections (design goal)
- Events: 30-50+ connections per scene
- Delta: 100% reduction

---

### Development Time
**Definition:** Total minutes from task start to task completion.

**Measurement:**
- Automatic timer in `TaskUI.cs`
- Starts when task instruction displayed
- Stops when participant marks task complete
- Pauses for breaks (if needed)

**Expected Range:**
- Simple tasks: 5-12 minutes
- Medium tasks: 8-15 minutes
- Complex tasks: 12-18 minutes

---

### Code Coupling (Efferent Coupling, Ce)
**Definition:** Number of external dependencies for a class, measured as:
- `GetComponent<T>()` calls
- `FindObjectOfType<T>()` calls
- Serialized field references to other classes
- Constructor/method parameters of other class types

**Measurement:**
- Static analysis via grep/regex
- Count per class, average per scene
- Document in coupling matrix

**Expected Range:**
- Mercury: 0-2 dependencies per class (mostly MmMessage types)
- Events: 3-8 dependencies per class (controllers, device lists)
- Delta: 40-60% reduction

---

### NASA-TLX Workload
**Definition:** Subjective workload measurement across 6 dimensions (7-point scale):
1. **Mental Demand** - How much mental effort required?
2. **Physical Demand** - How much physical effort required?
3. **Temporal Demand** - How much time pressure felt?
4. **Performance** - How successful were you?
5. **Effort** - How hard did you work?
6. **Frustration** - How frustrated/stressed did you feel?

**Measurement:**
- Post-task questionnaire via `NASATLXQuestionnaire.cs`
- 6 sliders, 0-7 scale
- Average across dimensions for total workload

**Expected Range:**
- Mercury: 3-4 average (moderate workload)
- Events: 4-5 average (higher workload)
- Delta: 0.5-1.5 point reduction

---

## Contact Information

**Project Lead:** TBD
**Institution:** Columbia University CGUI Lab
**Email:** TBD
**Repository:** (This repository)

**Key Personnel:**
- Primary Developer: TBD
- Study Coordinator: TBD
- Statistical Analyst: TBD
- Faculty Advisor: TBD

**Study Materials Location:** `dev/active/user-study/`
**Implementation Location:** `Assets/UserStudy/` (future)

---

## Version History

**Version 1.0** (2025-11-21)
- Initial context document created
- Architectural decisions documented
- Dependencies identified
- Metrics definitions established

---

**Last Updated:** 2025-11-21
