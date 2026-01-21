# User Study: MercuryMessaging vs Unity Events Comparison

## Overview

This directory contains planning documentation for a user study comparing **MercuryMessaging** framework against **Unity Events** for multi-object communication in Unity 3D scenes.

**Study Goal:** Quantitatively measure the differences between MercuryMessaging and Unity Events in terms of:
- **Lines of code** (LOC)
- **Development time** (minutes)
- **Code coupling** (number of Inspector references)
- **Maintainability** (ease of adding new objects/features)
- **Developer experience** (NASA-TLX workload, subjective preference)

**For business context and research objectives, see [`USE_CASE.md`](./USE_CASE.md)**

---

## Scene Selection Rationale

We selected **5 scene types** that demonstrate MercuryMessaging's key advantages:

1. **Hierarchical message routing** - Messages flow through GameObject hierarchy naturally
2. **Tag-based filtering** - 8-bit tag system for flexible targeting without references
3. **FSM integration** - State-based responder activation with MmRelaySwitchNode
4. **Loose coupling** - No Inspector references required between components
5. **Broadcast to multiple recipients** - Single message reaches all matching responders

Each scene is designed to be:
- ✅ **Implementable with both approaches** (fair comparison)
- ✅ **Completable in reasonable time** (30-90 minutes)
- ✅ **Demonstrative of real-world patterns** (not artificial examples)
- ✅ **Progressively complex** (simple → medium difficulty)

---

## Scene Inventory

### Priority 1: Smart Home Control Panel ⭐ **RECOMMENDED START**
**File:** [`01-smart-home-control.md`](./01-smart-home-control.md)

**Complexity:** Simple
**Estimated Time:** 30-45 minutes
**Key Patterns:** Hierarchical broadcasting, tag-based filtering, FSM states

**Why This Scene:**
- Simplest to implement (best for first-time participants)
- Clearest demonstration of Mercury advantages
- Familiar domain (everyone understands smart homes)
- Excellent scalability for adding tasks

**Mercury Advantages:**
- Single broadcast reaches all devices in hierarchy
- Room-level control via parent/child messages
- Tag system for device types (lights, climate, entertainment)
- FSM for mode switching (Home/Away/Sleep)

**Unity Events Challenges:**
- Must wire each device separately in Inspector
- Room-level control requires individual event connections
- No native tag system - need separate events per device type
- State management requires custom logic

---

### Priority 2: Music Mixing Board ⭐ **MOST UNIQUE**
**File:** [`02-music-mixing-board.md`](./02-music-mixing-board.md)

**Complexity:** Simple
**Estimated Time:** 30-45 minutes
**Key Patterns:** Real-time state synchronization, hierarchical audio mixing, effect chains

**Why This Scene:**
- Unique scenario (not typical game dev)
- Excellent for real-time state synchronization
- Hierarchical effect chains very clear
- Creative and engaging for participants

**Mercury Advantages:**
- Master volume broadcasts to all tracks
- Effect chain (reverb → delay → distortion) via hierarchy
- Mute groups (drums, bass, melody) via tags
- Preset states (Intro/Verse/Chorus) via FSM

**Unity Events Challenges:**
- Each track needs manual connection to master
- Effect chain requires manual wiring
- Mute groups need individual event connections
- Preset switching requires custom state management

---

### Priority 3: Tower Defense Wave System ⭐ **MOST GAME-LIKE**
**File:** [`03-tower-defense-waves.md`](./03-tower-defense-waves.md)

**Complexity:** Medium
**Estimated Time:** 45-90 minutes
**Key Patterns:** Event aggregation, multi-level coordination, tag-based filtering

**Why This Scene:**
- Familiar to game developers (common genre)
- Demonstrates all Mercury patterns comprehensively
- Medium complexity (good challenge level)
- Scalable (easy to add waves, towers, enemies)

**Mercury Advantages:**
- WaveController broadcasts wave start to all spawners
- Enemies report death to wave counter (parent notification)
- Towers receive "New enemy" message without direct references
- Tag system for enemy types (Ground, Air)

**Unity Events Challenges:**
- Each spawner needs manual connection to controller
- Enemy death requires reference to wave counter
- Tower enemy detection requires manual setup
- Enemy type filtering needs separate events

---

### Priority 4: Modular Puzzle Room
**File:** [`04-modular-puzzle-room.md`](./04-modular-puzzle-room.md)

**Complexity:** Simple-Medium
**Estimated Time:** 30-60 minutes
**Key Patterns:** State coordination, multi-trigger logic, conditional unlocking

**Why This Scene:**
- Clear goal (unlock doors via switches)
- State coordination very visible
- Easy to add complexity (more switches, complex conditions)
- Common game pattern

**Mercury Advantages:**
- Switches broadcast state changes
- Door listens for "all switches active" without tracking references
- Room state machine (Locked/Unlocked/Complete)
- Tag-based puzzle logic (RedPuzzle, BluePuzzle)

**Unity Events Challenges:**
- Door needs manual references to all switches
- Complex AND/OR logic requires custom scripts
- State tracking needs manual implementation
- Adding new switches requires Inspector rewiring

---

### Priority 5: Factory Assembly Line
**File:** [`05-factory-assembly-line.md`](./05-factory-assembly-line.md)

**Complexity:** Medium
**Estimated Time:** 60-90 minutes
**Key Patterns:** Sequential message flow, state progression, quality control

**Why This Scene:**
- Sequential workflow very clear
- Demonstrates message passing through stations
- Quality control shows broadcast patterns
- Industrial automation is relatable

**Mercury Advantages:**
- Item progression via parent notifications
- Station broadcasts completion to next station
- Quality control checks all stations via child messages
- Production state (Running/Paused/Maintenance) via FSM

**Unity Events Challenges:**
- Each station needs manual connection to next
- Item state tracking requires references
- Quality control needs individual connections
- State management needs custom implementation

---

## Implementation Priority Order

### Phase 1: Planning & Design (CURRENT)
- ✅ Scene selection and research
- ✅ Planning documentation structure
- 🔨 Detailed scene planning documents
- 🔨 Task design and metrics definition

### Phase 2: Smart Home Implementation (RECOMMENDED START)
1. Build Smart Home scene with MercuryMessaging
2. Build Smart Home scene with Unity Events (parallel implementation)
3. Design 4-5 user study tasks
4. Pilot test with 1-2 developers
5. Refine based on feedback

### Phase 3: Additional Scenes (OPTIONAL)
- Music Mixing Board (if time allows)
- Tower Defense (if time allows)
- Puzzle Room (if needed for variety)
- Assembly Line (if comprehensive study needed)

### Phase 4: User Study Execution
- Recruit 10-20 Unity developers
- Counterbalanced design (half start with Mercury, half with Events)
- Collect quantitative and qualitative data
- Statistical analysis

---

## Quick Reference Links

### Planning Documents
- [01-smart-home-control.md](./01-smart-home-control.md) - Smart Home Control Panel
- [02-music-mixing-board.md](./02-music-mixing-board.md) - Music Mixing Board
- [03-tower-defense-waves.md](./03-tower-defense-waves.md) - Tower Defense Wave System
- [04-modular-puzzle-room.md](./04-modular-puzzle-room.md) - Modular Puzzle Room
- [05-factory-assembly-line.md](./05-factory-assembly-line.md) - Factory Assembly Line
- [comparison-matrix.md](./comparison-matrix.md) - Side-by-side comparison data
- [user-study-design.md](./user-study-design.md) - Study methodology

### Related Documentation
- [../../CLAUDE.md](../../CLAUDE.md) - MercuryMessaging framework documentation
- [../../Documentation/FILE_REFERENCE.md](../../Documentation/FILE_REFERENCE.md) - Important files reference
- [../../CONTRIBUTING.md](../../CONTRIBUTING.md) - Development standards

---

## Metrics to Collect

For each scene implementation (Mercury vs Events), collect:

### Quantitative Metrics
- **Lines of code (LOC)** - Total lines written (excluding boilerplate)
- **Inspector connections** - Number of manual references/event wiring
- **Development time** - Minutes from start to completion
- **Debug time** - Minutes spent fixing bugs/issues
- **Compilation errors** - Count of errors encountered
- **Scene hierarchy depth** - Average depth of GameObject tree

### Code Coupling Metrics
- **Afferent coupling (Ca)** - Number of classes that depend on this class
- **Efferent coupling (Ce)** - Number of classes this class depends on
- **Direct references** - Count of GetComponent<>() or serialized field references
- **Event connections** - Count of UnityEvent.AddListener() calls

### Qualitative Metrics
- **NASA-TLX workload** (6 dimensions):
  - Mental demand
  - Physical demand
  - Temporal demand
  - Performance
  - Effort
  - Frustration
- **Subjective preference** - Which approach did you prefer? (5-point Likert)
- **Perceived maintainability** - How easy to modify? (5-point Likert)
- **Open-ended feedback** - What did you like/dislike?

---

## Expected Outcomes

### Hypothesis: MercuryMessaging will show...

✅ **Advantages:**
- Fewer lines of code (20-40% reduction)
- Faster development time for multi-object scenarios
- Lower coupling (fewer direct references)
- Better maintainability (easier to add objects)
- Higher scalability (adding objects doesn't require rewiring)

⚠️ **Trade-offs:**
- Steeper initial learning curve
- Less discoverable (can't see connections in Inspector)
- Requires understanding of hierarchical message flow
- Performance overhead (compared to direct calls, but not vs UnityEvents)

### Statistical Analysis Plan
- **Paired t-tests** for within-subject comparisons (Mercury vs Events per participant)
- **Effect size (Cohen's d)** to measure practical significance
- **Correlation analysis** between experience level and performance
- **Thematic analysis** of qualitative feedback

---

## Status

**Last Updated:** 2025-11-21
**Status:** Planning phase - creating detailed scene documentation
**Next Step:** Complete individual scene planning documents

---

## Notes for Researchers

### Participant Recruitment
- **Target:** Unity developers with 6+ months experience
- **Sample size:** 10-20 participants (aim for 15-20 for statistical power)
- **Counterbalancing:** Randomize order (Mercury first vs Events first)
- **Compensation:** Consider compensation for 2-3 hour study

### Ethical Considerations
- IRB approval may be required (check institution)
- Informed consent required
- Data anonymization and privacy
- Right to withdraw at any time

### Pilot Testing
- Run 1-2 pilot sessions before full study
- Test task clarity and time estimates
- Refine instructions and materials
- Check data collection tools

---

*Planning documentation created: 2025-11-21*
