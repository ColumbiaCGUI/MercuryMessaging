# User Study - Traffic Simulation Scene

**Status:** 🔨 In Active Development
**Branch:** `user_study`
**Scene:** `Scenario1.unity`
**Purpose:** UIST 2025 Paper (comparing MercuryMessaging vs Unity Events)

---

## Overview

This directory contains a complex traffic simulation scene designed to evaluate the MercuryMessaging framework against Unity's built-in event system. The scene will be used in a controlled user study for a UIST 2025 conference paper submission.

### Research Question
"Do hierarchical message-based architectures provide measurable benefits over traditional event systems for implementing complex game behaviors?"

---

## Scene Description

**Scenario1.unity** simulates an urban traffic environment with:
- **8-12 intersections** with traffic lights and crossings
- **100+ pedestrians** with fear-factor AI (cautious vs jaywalkers)
- **50+ vehicles** with recklessness meters (lawful vs reckless drivers)
- **Cross-intersection coordination** (emergency vehicles, green waves, congestion management)
- **Sentiment tracking** (aggregate crowd mood and traffic flow metrics)

---

## Directory Structure

```
Assets/UserStudy/
├── Scenes/
│   ├── Scenario1.unity               # MercuryMessaging implementation
│   └── Scenario1_UnityEvents.unity   # Unity Events comparison (not yet created)
│
└── Scripts/
    ├── TrafficLightController.cs     # Traffic light state machine
    ├── HubController.cs               # Central coordinator (MmBaseResponder)
    ├── Pedestrian.cs                  # Pedestrian AI with fear factor
    ├── CarController.cs               # Vehicle AI with recklessness meter
    ├── SentimentController.cs         # Aggregate sentiment tracking
    ├── TrafficEventManager.cs         # System-wide event broadcasting
    ├── SpawnManager.cs                # Agent spawning and object pooling
    ├── StreetInfo.cs                  # Street metadata (lanes, speed limits)
    ├── CameraManager.cs               # Camera control system
    ├── FollowCamera.cs                # Follow mode camera
    └── MaintainScale.cs               # UI scale maintenance
```

---

## Current Implementation Status

### ✅ Completed (11 Scripts)
1. **TrafficLightController** - 4-way traffic light coordination with timing
2. **HubController** - Central coordinator for all intersections
3. **Pedestrian** - Autonomous AI with fear factor (0-1 scale)
4. **CarController** - Autonomous vehicle with recklessness meter (0-1 scale)
5. **SentimentController** - Aggregate crowd sentiment and traffic metrics
6. **TrafficEventManager** - System-wide event broadcasting and coordination
7. **SpawnManager** - Object pooling and density-based spawning
8. **StreetInfo** - Street metadata and configuration
9. **CameraManager** - Free camera and top-down modes
10. **FollowCamera** - Agent following camera mode
11. **MaintainScale** - UI scale maintenance utility

### 🔨 In Progress
- **Intersection_01** - First complete intersection (traffic lights done, crossings pending)
- **Scene Integration** - Connecting all systems together
- **Performance Optimization** - Targeting 60+ FPS

### ⚠️ Blocked
- **Unity Verification** - Must verify project after Nov 18 reorganization
- **Prefab Creation** - Need to create reusable intersection prefabs

### ❌ Not Started (Critical Path)
- **7 Additional Intersections** - Need 8 total for study
- **Unity Events Implementation** - Complete reimplementation (100 hours estimated)
- **Performance Benchmarking** - Metrics collection and comparison
- **User Study Tasks** - 5-7 implementation tasks for participants
- **Data Collection System** - Automatic logging and metrics

---

## How to Use

### Opening the Scene

1. **Open Unity project**
2. **Navigate to** `Assets/UserStudy/Scenes/`
3. **Open** `Scenario1.unity`
4. **Enter Play mode** to test

### Camera Controls

- **WASD** - Move camera
- **Mouse** - Look around
- **Scroll Wheel** - Zoom in/out
- **F** - Follow selected agent
- **T** - Top-down overview mode
- **Esc** - Return to free camera

### Spawning Agents

- Pedestrians and vehicles spawn automatically via **SpawnManager**
- Spawn rates adjust based on scene density
- Object pooling ensures good performance

### Testing Traffic Flow

- Traffic lights cycle automatically (configurable timing)
- Pedestrians wait at red lights (unless low fear factor)
- Vehicles stop at red lights (unless high recklessness)
- Emergency vehicle events can be triggered via Hub

---

## MercuryMessaging Integration

### Hierarchy Structure

```
Hub (HubController - MmRelaySwitchNode)
  ├── Intersection_01 (MmRelayNode)
  │   ├── TrafficLights/ (4 lights with TrafficLightController)
  │   ├── CrossingZones/
  │   └── SpawnPoints/
  ├── Intersection_02 through 08 (To be created)
  ├── Pedestrians/ (Container)
  │   └── PedestrianPool/ (Pooled instances)
  ├── Vehicles/ (Container)
  │   └── VehiclePool/ (Pooled instances)
  └── SentimentSystem/
      └── SentimentController
```

### Message Flow Examples

**Example 1: Traffic Light Change**
```
Hub → Intersection_01 → TrafficLight_North
  MmMethod.MessageString("SetGreen")
```

**Example 2: Emergency Vehicle**
```
Hub → All Intersections (MmLevelFilter.Child)
  → All Lights → "AllRed" command
```

**Example 3: Pedestrian Crossing Request**
```
Pedestrian → CrossingZone (MmLevelFilter.Parent)
  → TrafficLight → Prioritize pedestrian phase
```

### Tags Used

- **Tag0** - Traffic lights
- **Tag1** - Pedestrians
- **Tag2** - Vehicles
- **Tag3** - Spawners
- **Tag4** - UI elements
- **Tag5** - Cameras

---

## AI Behavior Parameters

### Pedestrian AI
- **Fear Factor** (0-1): How cautious the pedestrian is
  - 0.0 = Fearless jaywalker
  - 0.5 = Average caution
  - 1.0 = Extremely cautious, never jaywalks
- **Walk Speed**: Movement speed (adjustable)
- **Crossing Threshold**: When to cross (light status + gap size)

### Vehicle AI
- **Recklessness Meter** (0-1): How much driver follows rules
  - 0.0 = Law-abiding driver
  - 0.5 = Average driver
  - 1.0 = Extremely reckless, runs red lights
- **Top Speed**: Maximum velocity
- **Acceleration/Braking**: How quickly vehicle speeds up/slows down

---

## Performance Targets

- **Frame Rate:** 60+ FPS sustained
- **Agent Count:** 100+ pedestrians, 50+ vehicles
- **Intersection Count:** 8-12 active intersections
- **Message Throughput:** Measured via performance profiler
- **Memory Usage:** < 2 GB total

---

## User Study Details

### Study Design
- **Within-subjects:** All participants use both MercuryMessaging and Unity Events
- **Counterbalanced:** Half start with MercuryMessaging, half with Unity Events
- **Duration:** 2 hours per participant
- **Sample Size:** 20-30 participants

### Tasks (Planned)
1. Add a new intersection
2. Implement a new agent behavior
3. Add cross-intersection coordination feature
4. Debug an existing behavior
5. Optimize performance

### Measures
- **Task Completion Time** - Minutes per task
- **Code Quality** - Coupling metrics, complexity, LOC
- **Error Rate** - Compilation errors, runtime errors, logic bugs
- **Cognitive Load** - NASA-TLX questionnaire
- **Qualitative Feedback** - Post-task interviews

---

## Known Issues and TODOs

### Critical
- [ ] **Unity verification after reorganization** (must be done next)
- [ ] **7 more intersections needed** (only 1 of 8 complete)
- [ ] **Unity Events implementation** (not started - 100 hours)
- [ ] **Performance benchmarking** (not started)

### High Priority
- [ ] Complete Intersection_01 (add crossings and spawn points)
- [ ] Create intersection prefabs for reuse
- [ ] Implement cross-intersection coordination
- [ ] Add emergency vehicle priority system
- [ ] Implement green wave coordination

### Medium Priority
- [ ] Visual polish (better models, particle effects)
- [ ] UI improvements (real-time stats, heat maps)
- [ ] Sound effects (traffic sounds, horns)
- [ ] Day/night cycle

### Low Priority
- [ ] Weather system (rain, fog, snow)
- [ ] Advanced intersection types (roundabouts, highway ramps)
- [ ] Additional agent types (bicycles, motorcycles)

---

## Timeline

**See:** `dev/active/user-study/user-study-tasks.md` for detailed timeline

**Critical Milestones:**
- Week 1-2: Complete 8 intersections
- Week 3-4: Unity Events implementation
- Week 5-6: Performance benchmarking and task design
- Week 9-16: User study data collection
- Week 17-18: Data analysis
- Week 19-21: Paper writing
- **April 9, 2025:** UIST submission deadline

---

## Related Documentation

- **`dev/active/user-study/user-study-context.md`** - Comprehensive context and planning
- **`dev/active/user-study/user-study-tasks.md`** - Detailed task breakdown
- **`dev/active/uist-paper/uist-paper-context.md`** - UIST paper planning
- **`CLAUDE.md`** - MercuryMessaging framework documentation

---

## Questions or Issues?

- Check `/dev/active/user-study/` for detailed planning docs
- Review MercuryMessaging documentation in `/CLAUDE.md`
- Contact Columbia CGUI Lab for research questions

---

**Last Updated:** November 18, 2025
**Next Action:** Verify Unity project after reorganization
**Status:** Active Development
