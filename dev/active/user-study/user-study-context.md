# User Study Context - Traffic Simulation Scene

**Last Updated:** 2025-11-18
**Status:** 🔨 In Active Development
**Branch:** `user_study`
**Scene:** `Assets/UserStudy/Scenes/Scenario1.unity`

---

## Overview

Development of a complex traffic simulation scene for comparing Unity's built-in event system against the MercuryMessaging framework. This scene serves as a **showcase and validation** of MercuryMessaging's hierarchical message routing capabilities.

**IMPORTANT UPDATE (2025-11-18):** UIST 2025 paper pursuit has been **discontinued**. This is now a demonstration scene showcasing MercuryMessaging features without publication pressure.

The scene simulates a multi-intersection urban environment with autonomous vehicles, pedestrians, traffic lights, and emergent behaviors to demonstrate the advantages of hierarchical message-based architectures over traditional Unity Events.

---

## Purpose and Goals

### Primary Goal
Demonstrate that MercuryMessaging's hierarchical message routing provides superior:
- **Decoupling** - Components don't need direct references
- **Scalability** - Easy to add new intersections and behaviors
- **Maintainability** - Message flow follows scene hierarchy
- **Performance** - Efficient routing through hierarchical filtering

### Future Study (Optional)
Developers using MercuryMessaging *could* demonstrate:
1. Faster implementation times
2. Fewer errors/bugs
3. Lower cognitive load
4. More maintainable code

**Note:** User study is no longer required for UIST. May be conducted as optional research in the future.

---

## Current Implementation State

### Scene Architecture

**Hierarchy Structure:**
```
Scenario1 (Scene)
├── TrafficHub (Central coordinator)
│   └── HubController.cs (MmBaseResponder)
│
├── Intersections/ (Container)
│   ├── Intersection_01 (MmRelayNode)
│   │   ├── TrafficLights/ (4-way lights)
│   │   │   ├── North_Light (TrafficLightController.cs)
│   │   │   ├── South_Light (TrafficLightController.cs)
│   │   │   ├── East_Light (TrafficLightController.cs)
│   │   │   └── West_Light (TrafficLightController.cs)
│   │   ├── CrossingZones/ (Pedestrian crossings)
│   │   └── VehicleSpawnPoints/
│   │
│   ├── Intersection_02 through Intersection_08 (Planned)
│   └── Intersection_09 through Intersection_12 (Stretch goal)
│
├── Pedestrians/ (Container)
│   └── PedestrianPool/ (Object pooling for performance)
│       └── Pedestrian prefab instances (Pedestrian.cs)
│
├── Vehicles/ (Container)
│   └── VehiclePool/ (Object pooling)
│       └── Car prefab instances (CarController.cs)
│
├── Environment/
│   ├── Roads
│   ├── Buildings
│   ├── Sidewalks
│   └── Props
│
├── SentimentSystem/
│   └── SentimentController.cs (Aggregate behavior tracking)
│
├── Managers/
│   ├── SpawnManager.cs (Controls spawning rates)
│   ├── TrafficEventManager.cs (System-wide events)
│   └── CameraManager.cs (Camera controls)
│
└── UI/
    ├── HUD (Traffic stats, sentiment display)
    └── Controls (User input for simulation)
```

### Implemented Features ✅

#### 1. Traffic Light System
**File:** `TrafficLightController.cs`
**Status:** ✅ Implemented

- 4-way intersection traffic light coordination
- Timed state transitions (Green → Yellow → Red)
- Synchronized opposite lights (North-South vs East-West)
- Visual feedback (material color changes)
- Message-based control via MercuryMessaging

**Key Methods:**
- `ReceivedMessage(MmMessageString)` - Handles traffic light commands
- State machine for light timing
- Pedestrian crossing coordination

#### 2. Hub Controller
**File:** `HubController.cs`
**Status:** ✅ Implemented

- Central coordinator for all intersections
- Broadcasts system-wide events
- Manages global traffic flow parameters
- Coordinates cross-intersection dependencies

**Responsibilities:**
- Emergency vehicle priority
- Rush hour simulation
- Weather condition changes
- Global sentiment monitoring

#### 3. Pedestrian System
**File:** `Pedestrian.cs`
**Status:** ✅ Implemented

- Autonomous pedestrian AI
- Fear factor system (affects crossing behavior)
- Waypoint-based pathfinding
- Traffic light awareness
- Crowd behavior (clustering, following)

**Parameters:**
- `fearFactor` (0-1) - How cautious the pedestrian is
- `walkSpeed` - Movement speed
- `crossingThreshold` - When to start crossing
- Target crosswalk selection

**Behaviors:**
- Wait at crosswalk when light is red
- Cross when safe (green light or gap in traffic)
- React to nearby vehicles
- Follow crowd when uncertain

#### 4. Vehicle System
**File:** `CarController.cs`
**Status:** ✅ Implemented

- Autonomous vehicle AI
- Recklessness meter (affects driving behavior)
- Lane following
- Traffic light obedience (or not, if reckless)
- Collision avoidance

**Parameters:**
- `recklessMeter` (0-1) - How much driver follows rules
- `topSpeed` - Maximum speed
- `accelerationRate` - How quickly speeds up
- `brakingDistance` - When to start slowing for lights

**Behaviors:**
- Stop at red lights (unless reckless)
- Slow at yellow lights
- Accelerate at green lights
- Avoid other vehicles
- Yield to pedestrians (unless reckless)

#### 5. Sentiment System
**File:** `SentimentController.cs`
**Status:** ✅ Implemented

- Tracks aggregate crowd sentiment
- Monitors traffic flow efficiency
- Detects congestion and frustration
- Provides feedback to hub for adjustments

**Metrics Tracked:**
- Average wait time at intersections
- Pedestrian crossing delays
- Vehicle congestion levels
- Near-miss incidents
- Overall satisfaction score

#### 6. Event Management
**File:** `TrafficEventManager.cs`
**Status:** ✅ Implemented

- System-wide event broadcasting
- Event history tracking
- Priority event handling
- Cross-intersection coordination

**Event Types:**
- Emergency vehicle approaching
- Pedestrian crossing request
- Vehicle breakdown
- Weather changes
- Rush hour triggers

#### 7. Spawning System
**File:** `SpawnManager.cs`
**Status:** ✅ Implemented

- Controls pedestrian and vehicle spawn rates
- Object pooling for performance
- Spawn point management
- Density-based spawning (prevents overcrowding)

**Features:**
- Time-of-day spawn patterns
- Dynamic spawn rate adjustment
- Pool preheating
- Despawning when far from camera

#### 8. Camera System
**Files:** `CameraManager.cs`, `FollowCamera.cs`, `MaintainScale.cs`
**Status:** ✅ Implemented

- Free camera movement
- Follow mode (track specific vehicle/pedestrian)
- Top-down overview mode
- Smooth transitions

#### 9. Street Information
**File:** `StreetInfo.cs`
**Status:** ✅ Implemented

- Metadata about street segments
- Lane configuration
- Speed limits
- Intersection connections

---

## Recent Development Activity

### Last 10 Commits (user_study branch)

1. **`235d134`** - "Major refactoring changes" (2025-11-18)
   - Assets folder reorganization
   - UserStudy scripts preserved in `Assets/UserStudy/Scripts/`

2. **`0302b27`** - "Upgrade Unity version"
   - Upgraded to newer Unity version
   - Updated XR packages

3. **`2e05717`** - "Fix hub interaction"
   - Fixed HubController message routing
   - Improved cross-intersection coordination

4. **`2ba8bde`** - "refactor: add some more modification to scenes"
   - Scene structure improvements
   - Additional intersection setup

5. **`cb971cc`** - "refactor: modified scene"
   - Further scene refinements

6-10: Earlier commits focused on:
   - Pedestrian fear factor implementation
   - Vehicle recklessness system
   - Multi-intersection dependencies
   - Crowd sentiment tracking
   - Performance optimization

---

## Pending Features 🔨

### High Priority (Required for User Study)

#### 1. Multiple Intersections (Currently: 1, Target: 8-12)
**Status:** ⚠️ Only 1 intersection implemented
**Effort:** ~40 hours
**Dependencies:** None

**Tasks:**
- [ ] Design intersection layouts (straight, T-junction, roundabout)
- [ ] Create intersection prefabs
- [ ] Implement 8 basic intersections
- [ ] Add 4 advanced intersections (stretch goal)
- [ ] Connect intersections with road network
- [ ] Test traffic flow between intersections

#### 2. Cross-Intersection Message Flow
**Status:** ⚠️ Partially implemented in HubController
**Effort:** ~30 hours
**Dependencies:** Multiple intersections

**Tasks:**
- [ ] Emergency vehicle priority propagation
- [ ] Traffic wave coordination (green wave timing)
- [ ] Congestion detection and routing
- [ ] Pedestrian crowd flow between intersections
- [ ] Weather effects propagation

#### 3. Performance Benchmarking
**Status:** ❌ Not started
**Effort:** ~60 hours
**Dependencies:** Complete scene with 8+ intersections

**Tasks:**
- [ ] Implement performance metrics collection
- [ ] Frame rate monitoring
- [ ] Message throughput measurement
- [ ] Memory profiling
- [ ] Comparison with Unity Events implementation
- [ ] Generate performance report

#### 4. Unity Events Comparison Implementation
**Status:** ❌ Not started (CRITICAL)
**Effort:** ~100 hours
**Dependencies:** Complete MercuryMessaging implementation

**Tasks:**
- [ ] Duplicate scene (Scenario1_UnityEvents.unity)
- [ ] Reimplement all messaging using Unity Events
- [ ] Ensure feature parity
- [ ] Match visual appearance
- [ ] Performance parity where possible
- [ ] Document coupling differences

#### 5. User Study Task Design
**Status:** ❌ Not started (CRITICAL)
**Effort:** ~40 hours
**Dependencies:** Both implementations complete

**Tasks:**
- [ ] Design 5-7 implementation tasks for participants
- [ ] Create task instruction documents
- [ ] Develop task evaluation rubrics
- [ ] Pilot test tasks with 2-3 volunteers
- [ ] Refine based on pilot feedback

#### 6. Data Collection Infrastructure
**Status:** ❌ Not started
**Effort:** ~30 hours
**Dependencies:** Task design complete

**Tasks:**
- [ ] Implement automatic logging of user actions
- [ ] Time tracking per task
- [ ] Error/bug tracking
- [ ] Code complexity metrics collection
- [ ] NASA-TLX questionnaire integration
- [ ] Data export to CSV/JSON

### Medium Priority (Nice to Have)

#### 7. Advanced AI Behaviors
**Effort:** ~20 hours

- [ ] Pedestrian social groups (families, friends)
- [ ] Vehicle convoys (multiple cars traveling together)
- [ ] Jaywalking behavior
- [ ] Vehicle lane changing
- [ ] Bicycle and motorcycle agents

#### 8. Visual Polish
**Effort:** ~15 hours

- [ ] Better vehicle models
- [ ] Animated pedestrians
- [ ] Particle effects (exhaust, dust)
- [ ] Day/night cycle visuals
- [ ] Weather effects (rain, fog)

#### 9. UI Improvements
**Effort:** ~10 hours

- [ ] Real-time traffic flow visualization
- [ ] Sentiment heat map
- [ ] Intersection efficiency graphs
- [ ] User control panel
- [ ] Debug visualization toggles

### Low Priority (Future Work)

#### 10. Networking Support
**Effort:** ~50 hours

- [ ] Multi-user observation mode
- [ ] Collaborative control
- [ ] Remote data collection

---

## Technical Challenges

### 1. Performance with 8-12 Intersections
**Challenge:** Maintaining 60+ FPS with hundreds of agents
**Solutions:**
- Object pooling (already implemented)
- LOD for distant agents
- Culling off-screen agents
- Spatial partitioning for collision detection
- Message filtering optimization

### 2. Realistic Traffic Flow
**Challenge:** Avoiding deadlocks and unrealistic behaviors
**Solutions:**
- Traffic flow algorithms from literature
- Pathfinding with A* or similar
- Intersection priority rules
- Deadlock detection and resolution

### 3. Fair Comparison with Unity Events
**Challenge:** Ensuring apples-to-apples comparison
**Solutions:**
- Identical visual appearance
- Same AI behaviors
- Similar performance characteristics
- Document any implementation differences
- Expert review of both implementations

### 4. User Study Recruitment
**Challenge:** Finding 20-30 qualified participants
**Solutions:**
- Columbia CS department students
- Unity developer communities
- Game dev meetups
- Compensate participants ($20-50/session)
- Make study engaging and educational

---

## Key Decisions Made

### 1. Scene Complexity Level
**Decision:** 8-12 intersections with hundreds of agents
**Rationale:**
- Complex enough to show MercuryMessaging advantages
- Simple enough to implement in Unity Events
- Realistic urban simulation
- Challenging but achievable for participants

### 2. Agent AI Sophistication
**Decision:** Simple but believable AI (fear factor, recklessness)
**Rationale:**
- Focus is on messaging architecture, not AI
- Easy to understand for participants
- Still demonstrates emergent behaviors
- Reduces implementation time

### 3. MercuryMessaging Usage Pattern
**Decision:** Heavy use of hierarchical message routing
**Rationale:**
- Showcases framework's strengths
- Natural fit for intersection hierarchy
- Demonstrates decoupling benefits
- Provides clear comparison point with Unity Events

### 4. Performance Target
**Decision:** 60+ FPS on mid-range hardware
**Rationale:**
- Professional game standard
- Avoids performance confounding study results
- Demonstrates practical viability
- Requires optimization work

### 5. Study Duration
**Decision:** ~2 hours per participant
**Rationale:**
- Long enough for meaningful data
- Short enough to recruit participants
- Avoids fatigue effects
- Fits in one session

---

## Integration with MercuryMessaging Framework

### Message Flow Examples

#### Example 1: Traffic Light Change
```
Hub (MmRelaySwitchNode)
  └─> Intersection_01 (MmRelayNode)
      └─> North_Light (TrafficLightController)
          └─> Receives MmMessageString("SetGreen")
              └─> Changes light to green
              └─> Broadcasts MmMessageBool(true) to crossing zone
```

#### Example 2: Emergency Vehicle
```
Hub (broadcasts to all intersections)
  └─> All Intersections (MmLevelFilter.Child)
      └─> All Traffic Lights (MmLevelFilter.Child)
          └─> Switch to red immediately
          └─> Hold until emergency vehicle passes
```

#### Example 3: Pedestrian Crossing Request
```
Pedestrian (at crosswalk)
  └─> Crossing Zone (MmLevelFilter.Parent)
      └─> Traffic Light (MmLevelFilter.Parent)
          └─> Prioritize pedestrian phase
          └─> Notify Hub of crossing request
```

### Tags Used

- **Tag0:** Traffic lights
- **Tag1:** Pedestrians
- **Tag2:** Vehicles
- **Tag3:** Spawners
- **Tag4:** UI elements
- **Tag5:** Cameras
- **Tag6-7:** Reserved for future use

### Filter Patterns

- **Hub → Intersections:** `MmLevelFilter.Child` with `MmActiveFilter.All`
- **Intersection → Lights:** `MmLevelFilter.Child` with tag filtering
- **Agent → Parent:** `MmLevelFilter.Parent` for upward notification
- **System broadcasts:** `MmLevelFilter.SelfAndChildren` from Hub

---

## Files Reference

### Core Scripts (Assets/UserStudy/Scripts/)

| File | Lines | Purpose | Status |
|------|-------|---------|--------|
| `TrafficLightController.cs` | ~200 | Traffic light state machine | ✅ Complete |
| `HubController.cs` | ~300 | Central coordination | ✅ Complete |
| `Pedestrian.cs` | ~400 | Pedestrian AI and behavior | ✅ Complete |
| `CarController.cs` | ~450 | Vehicle AI and driving | ✅ Complete |
| `SentimentController.cs` | ~250 | Aggregate sentiment tracking | ✅ Complete |
| `TrafficEventManager.cs` | ~200 | Event broadcasting | ✅ Complete |
| `SpawnManager.cs` | ~300 | Agent spawning and pooling | ✅ Complete |
| `StreetInfo.cs` | ~100 | Street metadata | ✅ Complete |
| `CameraManager.cs` | ~150 | Camera control | ✅ Complete |
| `FollowCamera.cs` | ~100 | Follow mode camera | ✅ Complete |
| `MaintainScale.cs` | ~50 | UI scale maintenance | ✅ Complete |

**Total:** 11 scripts, ~2,500 lines of code

### Scene Files

| File | Purpose | Status |
|------|---------|--------|
| `Assets/UserStudy/Scenes/Scenario1.unity` | Main traffic simulation | 🔨 In Progress |
| `Assets/UserStudy/Scenes/Scenario1_UnityEvents.unity` | Comparison implementation | ❌ Not created |

### Prefabs (Planned)

- `Pedestrian.prefab` - Base pedestrian with AI
- `Car.prefab` - Base vehicle with AI
- `Intersection_4Way.prefab` - 4-way intersection template
- `Intersection_3Way.prefab` - T-junction template
- `TrafficLight.prefab` - Individual traffic light
- `CrossingZone.prefab` - Pedestrian crossing area

---

## Next Steps (Priority Order)

### Immediate (Next Session)

1. **Verify Unity Reorganization** ✅ COMPLETE
   - Verified on 2025-11-18: Zero errors, zero broken references
   - All UserStudy scripts compile
   - Scenario1.unity loads and runs

2. **Create Additional Intersections** 🔨 IN PROGRESS
   - Design 7 more intersection layouts
   - Create intersection prefabs
   - Place in scene with road connections
   - Test traffic flow
   - **Timeline:** No deadline pressure (UIST removed)

3. **Implement Cross-Intersection Coordination** 📋 PLANNED
   - Emergency vehicle priority across intersections
   - Green wave timing system
   - Congestion-based routing

### Short-Term (Next 2 Weeks)

4. **Performance Benchmarking** 📊
   - Implement metrics collection
   - Profile current implementation
   - Optimize bottlenecks
   - Document performance

5. **Unity Events Implementation** 🔄
   - Duplicate scene
   - Reimplement with Unity Events
   - Ensure feature parity
   - Performance comparison

6. **User Study Task Design** 📝
   - Design 5-7 implementation tasks
   - Write task instructions
   - Create evaluation rubrics
   - Pilot test

### Medium-Term (Next 1-2 Months)

7. **Recruitment and IRB** 👥
   - Prepare IRB application
   - Submit for approval
   - Create recruitment materials
   - Start recruiting participants

8. **Data Collection System** 📈
   - Implement logging infrastructure
   - NASA-TLX integration
   - Metrics collection
   - Data export

9. **Pilot Study** 🧪
   - Run with 2-3 participants
   - Refine tasks and procedures
   - Fix issues discovered
   - Validate data collection

### Long-Term (2-3 Months)

10. **Full User Study** 👥
    - Run 20-30 participants
    - Collect all data
    - Monitor for issues
    - Debrief participants

11. **Data Analysis** 📊
    - Statistical analysis
    - Qualitative feedback analysis
    - Performance comparison
    - Generate figures/tables

12. **Paper Writing** 📄
    - Write first draft
    - Internal review
    - Revisions
    - Submit to UIST 2025 (April 9 deadline)

---

## Blockers and Risks

### Previous Blockers (Now Resolved) ✅

1. **UIST Deadline** - REMOVED (no longer pursuing UIST 2025)
2. **Unity Events Implementation** - OPTIONAL (not required for showcase)
3. **Recruitment Uncertainty** - REMOVED (no user study required)
4. **Unity Reorganization** - COMPLETE (verified Nov 18, zero errors)

### Medium Risks

4. **Performance Issues with 8-12 Intersections**
   - **Risk:** Can't maintain 60 FPS
   - **Mitigation:** Aggressive optimization, reduce scene complexity if needed
   - **Impact:** Performance becomes confounding variable

5. **Fair Comparison Challenges**
   - **Risk:** Implementations not truly equivalent
   - **Mitigation:** Expert review, clear documentation of differences
   - **Impact:** Reviewers question validity of comparison

6. **Participant Task Difficulty**
   - **Risk:** Tasks too hard or too easy
   - **Mitigation:** Pilot testing, task refinement
   - **Impact:** Ceiling/floor effects, no meaningful differences detected

---

## Resources Needed

### Personnel
- 1 primary developer (scene implementation)
- 1 assistant (Unity Events implementation)
- 1 study coordinator (recruitment, administration)
- 1 data analyst (statistical analysis)

### Infrastructure
- Unity Pro license (for performance profiling)
- Participant computers (standardized hardware)
- Data collection server
- IRB approval

### Budget Estimate
- Participant compensation: $30/person × 30 = $900
- Software licenses: $200
- Equipment: $500
- Contingency: $400
- **Total:** ~$2,000

### Time Estimate
- Implementation: 300 hours
- User study: 100 hours (incl recruitment, administration)
- Data analysis: 80 hours
- Paper writing: 120 hours
- **Total:** ~600 hours (over 12 weeks with parallel work)

---

## Success Criteria

### Scene Implementation
- [ ] 8-12 intersections functioning
- [ ] Hundreds of agents (100+ pedestrians, 50+ vehicles)
- [ ] 60+ FPS sustained
- [ ] Realistic traffic flow (no deadlocks)
- [ ] All messaging features working

### Comparison Implementation
- [ ] Unity Events version feature-complete
- [ ] Visual parity with MercuryMessaging version
- [ ] Same AI behaviors
- [ ] Documented implementation differences

### User Study
- [ ] 20-30 participants recruited and completed
- [ ] All data collected successfully
- [ ] Statistically significant results
- [ ] Clear winner or meaningful insights

### Paper
- [ ] Submitted to UIST 2025 by April 9
- [ ] All sections complete (intro, related work, methods, results, discussion)
- [ ] Figures and tables publication-ready
- [ ] Reviewed by co-authors

---

## Related Documentation

- **`dev/active/user-study/user-study-tasks.md`** - Detailed task breakdown
- **`dev/archive/mercury-improvements-original/mercury-improvements-master-plan.md`** - Original master plan
- **`CLAUDE.md`** - MercuryMessaging framework documentation
- **`Assets/UserStudy/README.md`** - Scene usage instructions (to be created)

## Status Summary

**Last Updated:** 2025-11-18

**Current Status:** In Progress - Showcase scene development
**UIST Paper:** Discontinued (no longer pursuing)
**Reorganization:** Complete (verified working)
**Next Action:** Complete Intersection_01, create prefabs, add 7 more intersections
**Timeline:** Flexible (no hard deadline)

---

## Questions for Stakeholders

1. **UIST Pursuit Decision** - Are we committed to April 9 deadline?
2. **Resource Allocation** - Can we get 1-2 additional developers?
3. **IRB Timeline** - How long for approval? Do we need it?
4. **Participant Recruitment** - Access to CS student email lists?
5. **Budget** - Is $2,000 estimate acceptable?
6. **Scope** - 8 intersections sufficient or need all 12?
7. **Comparison Fairness** - Expert review available for Unity Events implementation?

---

**Status:** 🔨 In Active Development
**Next Immediate Action:** Verify Unity project after reorganization, then add 7 more intersections
**Critical Deadline:** UIST Abstract (April 2, 2025), Full Paper (April 9, 2025)
**Last Updated:** 2025-11-18
