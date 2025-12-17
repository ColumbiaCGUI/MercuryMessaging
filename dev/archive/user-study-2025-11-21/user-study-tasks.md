# User Study Tasks - Traffic Simulation

**Last Updated:** 2025-11-18
**Status:** 🔨 In Active Development
**Branch:** `user_study`

---

## Task Status Summary

- ✅ Complete: 11 tasks (Core scripts implemented)
- 🔨 In Progress: 2 tasks (Scene integration, testing)
- ⚠️ Blocked: 3 tasks (Awaiting Unity verification)
- ❌ Not Started: 30+ tasks (Additional intersections, Unity Events, user study prep)

---

## Phase 1: Core Implementation ✅ MOSTLY COMPLETE

### 1.1 Traffic Light System ✅
- [x] Create `TrafficLightController.cs` script
- [x] Implement state machine (Green → Yellow → Red → Green)
- [x] Add timing configuration (green duration, yellow duration, red duration)
- [x] Implement material color changes for visual feedback
- [x] Add MercuryMessaging integration (receive commands)
- [x] Implement opposite light synchronization (N-S vs E-W)
- [x] Add pedestrian crossing coordination
- [x] Test basic functionality

**Acceptance Criteria:**
- ✅ Lights cycle through states correctly
- ✅ Timing is configurable
- ✅ Visual feedback works (colors change)
- ✅ Responds to MercuryMessaging commands
- ✅ Opposite lights synchronized

### 1.2 Hub Controller ✅
- [x] Create `HubController.cs` script
- [x] Extend `MmBaseResponder` for messaging
- [x] Implement system-wide event broadcasting
- [x] Add intersection registration system
- [x] Implement global parameter management (rush hour, weather, etc.)
- [x] Add emergency vehicle priority system
- [x] Implement cross-intersection coordination
- [x] Add sentiment monitoring integration
- [x] Test hub message propagation

**Acceptance Criteria:**
- ✅ Hub can broadcast to all intersections
- ✅ Intersections register correctly
- ✅ Emergency vehicle priority works
- ✅ Global parameters affect all intersections
- ✅ Cross-intersection coordination functional

### 1.3 Pedestrian System ✅
- [x] Create `Pedestrian.cs` script
- [x] Implement basic movement (waypoint following)
- [x] Add fear factor parameter (0-1 scale)
- [x] Implement traffic light awareness
- [x] Add crossing behavior (wait at red, cross at green)
- [x] Implement gap-in-traffic crossing (for jaywalkers)
- [x] Add collision avoidance (other pedestrians)
- [x] Implement vehicle awareness (don't get hit)
- [x] Add crowd behavior (following, clustering)
- [x] Implement MercuryMessaging integration
- [x] Test pedestrian AI

**Acceptance Criteria:**
- ✅ Pedestrians move along waypoints
- ✅ Fear factor affects behavior appropriately
- ✅ Wait at red lights, cross at green
- ✅ High-fear pedestrians never jaywalk
- ✅ Low-fear pedestrians jaywalk when safe
- ✅ Collision avoidance works
- ✅ Crowd behaviors emerge

### 1.4 Vehicle System ✅
- [x] Create `CarController.cs` script
- [x] Implement lane following
- [x] Add recklessness meter parameter (0-1 scale)
- [x] Implement traffic light obedience
- [x] Add yellow light decision logic
- [x] Implement red light running (for reckless drivers)
- [x] Add acceleration and braking
- [x] Implement collision avoidance (vehicles)
- [x] Add pedestrian yielding behavior
- [x] Implement speed limit awareness
- [x] Add MercuryMessaging integration
- [x] Test vehicle AI

**Acceptance Criteria:**
- ✅ Vehicles follow lanes correctly
- ✅ Recklessness affects behavior appropriately
- ✅ Law-abiding drivers stop at red lights
- ✅ Reckless drivers may run red lights
- ✅ Collision avoidance prevents crashes
- ✅ Vehicles yield to pedestrians (usually)
- ✅ Speed varies based on recklessness and traffic

### 1.5 Sentiment System ✅
- [x] Create `SentimentController.cs` script
- [x] Implement aggregate sentiment tracking
- [x] Add wait time monitoring (pedestrians at crosswalks)
- [x] Add congestion detection (vehicle queues)
- [x] Implement near-miss tracking (close calls)
- [x] Add overall satisfaction calculation
- [x] Implement feedback to hub (for adjustments)
- [x] Add visualization hooks (for UI)
- [x] Test sentiment calculations

**Acceptance Criteria:**
- ✅ Sentiment reflects traffic conditions
- ✅ Long waits decrease sentiment
- ✅ Congestion decreases sentiment
- ✅ Near-misses decrease sentiment
- ✅ Smooth flow increases sentiment
- ✅ Hub receives sentiment feedback

### 1.6 Event Management ✅
- [x] Create `TrafficEventManager.cs` script
- [x] Implement event queue system
- [x] Add event types (emergency, breakdown, weather, rush hour)
- [x] Implement event broadcasting
- [x] Add event history tracking
- [x] Implement priority event handling
- [x] Add cross-intersection event coordination
- [x] Test event propagation

**Acceptance Criteria:**
- ✅ Events queue and broadcast correctly
- ✅ Priority events preempt normal flow
- ✅ Event history tracked
- ✅ All intersections receive relevant events
- ✅ Coordination between intersections works

### 1.7 Spawning System ✅
- [x] Create `SpawnManager.cs` script
- [x] Implement object pooling (pedestrians)
- [x] Implement object pooling (vehicles)
- [x] Add spawn point management
- [x] Implement density-based spawning
- [x] Add time-of-day spawn patterns
- [x] Implement dynamic spawn rate adjustment
- [x] Add pool preheating (on scene load)
- [x] Implement despawning (when far from camera)
- [x] Test spawning and pooling performance

**Acceptance Criteria:**
- ✅ Object pooling works efficiently
- ✅ Spawn rates adjust based on density
- ✅ No overcrowding occurs
- ✅ Performance remains good (60+ FPS)
- ✅ Agents despawn correctly

### 1.8 Camera System ✅
- [x] Create `CameraManager.cs` script
- [x] Implement free camera movement (WASD + mouse)
- [x] Add top-down overview mode
- [x] Create `FollowCamera.cs` for follow mode
- [x] Implement smooth camera transitions
- [x] Add zoom controls
- [x] Create `MaintainScale.cs` for UI elements
- [x] Test all camera modes

**Acceptance Criteria:**
- ✅ Free camera movement smooth and responsive
- ✅ Top-down view shows entire scene
- ✅ Follow mode tracks selected agent
- ✅ Transitions are smooth
- ✅ UI elements maintain scale

### 1.9 Street Information ✅
- [x] Create `StreetInfo.cs` script
- [x] Add lane configuration data
- [x] Add speed limit information
- [x] Implement intersection connection metadata
- [x] Add street type classification
- [x] Test metadata access

**Acceptance Criteria:**
- ✅ Streets have correct metadata
- ✅ Vehicles access speed limits
- ✅ Intersection connections defined
- ✅ Data easily accessible by agents

---

## Phase 2: Scene Expansion 🔨 IN PROGRESS / ⚠️ BLOCKED

### 2.1 Unity Verification ✅ COMPLETE
- [x] **Open Unity project**
- [x] **Wait for asset reimport (after reorganization)**
- [x] **Load `Scenario1.unity` scene**
- [x] **Check for missing references in console**
- [x] **Test play mode - verify scene functions**
- [x] **Fix any broken references discovered**
- [x] **Commit fixes if needed**

**Acceptance Criteria:**
- [x] Unity opens without errors
- [x] Scenario1.unity loads correctly
- [x] No missing script references
- [x] No missing prefab references
- [x] Scene runs in play mode
- [x] All UserStudy scripts compile

**Status:** ✅ COMPLETE (verified 2025-11-18)

### 2.2 First Intersection Complete 🔨
- [x] Create Intersection_01 GameObject hierarchy
- [x] Add 4 traffic lights (N, S, E, W)
- [ ] Add 4 crossing zones
- [ ] Add 4 vehicle spawn points (one per direction)
- [ ] Add 4 pedestrian spawn points (one per corner)
- [ ] Test single intersection functionality
- [ ] Verify all scripts work together
- [ ] Ensure 60+ FPS with full intersection active

**Acceptance Criteria:**
- [ ] All lights cycle correctly
- [ ] Pedestrians cross when safe
- [ ] Vehicles obey lights (mostly)
- [ ] No deadlocks occur
- [ ] Performance acceptable (60+ FPS)

**Status:** 🔨 Partial - Traffic lights done, crossings/spawn points needed

### 2.3 Intersection Prefab Creation ❌
- [ ] Extract Intersection_01 as prefab template
- [ ] Parameterize intersection (4-way, 3-way, etc.)
- [ ] Create `Intersection_4Way.prefab`
- [ ] Create `Intersection_3Way.prefab` (T-junction)
- [ ] Create `Intersection_Roundabout.prefab` (stretch)
- [ ] Test prefab instantiation
- [ ] Document prefab customization

**Acceptance Criteria:**
- [ ] Prefabs instantiate correctly
- [ ] Parameters expose key settings
- [ ] Can create new intersections quickly
- [ ] Prefabs maintain MercuryMessaging connections

**Status:** ❌ Not started - Depends on 2.2 completion

### 2.4 Create 7 Additional Intersections ❌
- [ ] **Intersection_02** - Standard 4-way
- [ ] **Intersection_03** - Standard 4-way
- [ ] **Intersection_04** - T-junction (3-way)
- [ ] **Intersection_05** - T-junction (3-way)
- [ ] **Intersection_06** - Standard 4-way
- [ ] **Intersection_07** - Standard 4-way
- [ ] **Intersection_08** - Standard 4-way
- [ ] Position intersections in scene with proper spacing
- [ ] Connect with road segments
- [ ] Test traffic flow between intersections

**Acceptance Criteria:**
- [ ] 8 total intersections in scene
- [ ] Roads connect intersections logically
- [ ] Vehicles can travel between intersections
- [ ] Pedestrians can walk between intersections
- [ ] No overlapping spawn points
- [ ] 60+ FPS with all 8 active

**Status:** ❌ Not started - Depends on 2.3

**Effort Estimate:** ~40 hours

### 2.5 Advanced Intersections (Stretch Goal) ❌
- [ ] **Intersection_09** - Roundabout
- [ ] **Intersection_10** - 5-way complex intersection
- [ ] **Intersection_11** - Highway on-ramp
- [ ] **Intersection_12** - Highway off-ramp
- [ ] Implement special logic for advanced types
- [ ] Test advanced intersection behaviors

**Acceptance Criteria:**
- [ ] Advanced intersections function correctly
- [ ] Unique behaviors implemented
- [ ] Performance remains acceptable

**Status:** ❌ Not started - Stretch goal

**Effort Estimate:** ~30 hours

---

## Phase 3: Cross-Intersection Coordination ❌ NOT STARTED

### 3.1 Emergency Vehicle Priority ❌
- [ ] Create emergency vehicle prefab
- [ ] Implement priority message propagation
- [ ] Add "all red" mode for intersections
- [ ] Implement path clearing (vehicles pull over)
- [ ] Add emergency vehicle routing
- [ ] Test emergency scenarios
- [ ] Verify message flow from Hub → all intersections

**Acceptance Criteria:**
- [ ] Emergency vehicle triggers all-red at nearby intersections
- [ ] Vehicles pull over correctly
- [ ] Pedestrians wait during emergency
- [ ] Emergency vehicle can traverse scene quickly
- [ ] Normal traffic resumes after emergency passes

**Status:** ❌ Not started

**Effort Estimate:** ~20 hours

### 3.2 Green Wave Coordination ❌
- [ ] Implement synchronized timing algorithm
- [ ] Add "green wave" mode to Hub
- [ ] Calculate optimal offsets between intersections
- [ ] Implement timing propagation via messages
- [ ] Test green wave on straight road (multiple intersections)
- [ ] Verify vehicles can travel without stopping

**Acceptance Criteria:**
- [ ] Green wave activates correctly
- [ ] Timing offsets calculated properly
- [ ] Vehicles experience fewer stops
- [ ] Works for primary traffic direction

**Status:** ❌ Not started

**Effort Estimate:** ~25 hours

### 3.3 Congestion Detection and Routing ❌
- [ ] Implement vehicle queue detection at intersections
- [ ] Add congestion message broadcasting
- [ ] Implement alternative route suggestions
- [ ] Add dynamic spawn rate reduction (high congestion areas)
- [ ] Implement light timing adjustments for congestion
- [ ] Test congestion scenarios
- [ ] Verify system reduces congestion over time

**Acceptance Criteria:**
- [ ] Congestion detected accurately
- [ ] Messages propagate to Hub and other intersections
- [ ] Spawn rates adjust appropriately
- [ ] Light timing adapts to traffic flow
- [ ] Congestion resolves (doesn't deadlock)

**Status:** ❌ Not started

**Effort Estimate:** ~30 hours

### 3.4 Crowd Flow Between Intersections ❌
- [ ] Implement pedestrian destination selection
- [ ] Add pathfinding between intersections
- [ ] Implement crowd density awareness
- [ ] Add pedestrian message propagation (crowd events)
- [ ] Test large crowd movements
- [ ] Verify performance with 100+ pedestrians

**Acceptance Criteria:**
- [ ] Pedestrians navigate between intersections
- [ ] Crowds form and disperse naturally
- [ ] Pathfinding avoids congested areas
- [ ] Performance acceptable (60+ FPS)

**Status:** ❌ Not started

**Effort Estimate:** ~20 hours

### 3.5 Weather and Environmental Effects ❌
- [ ] Implement weather system (rain, fog, snow)
- [ ] Add weather message broadcasting from Hub
- [ ] Implement visibility reduction (fog)
- [ ] Add slippery roads effect (rain/snow)
- [ ] Reduce pedestrian/vehicle speeds in bad weather
- [ ] Add visual effects (particle systems)
- [ ] Test weather scenarios

**Acceptance Criteria:**
- [ ] Weather changes propagate to all intersections
- [ ] Agent behavior adapts to weather
- [ ] Visual effects enhance realism
- [ ] Performance impact minimal

**Status:** ❌ Not started - Low priority

**Effort Estimate:** ~15 hours

---

## Phase 4: Performance and Polish ❌ NOT STARTED

### 4.1 Performance Profiling ❌
- [ ] Install Unity Profiler (Pro license)
- [ ] Profile scene with 8 intersections, 100+ agents
- [ ] Identify CPU bottlenecks
- [ ] Identify memory bottlenecks
- [ ] Identify GPU bottlenecks
- [ ] Create performance baseline document

**Acceptance Criteria:**
- [ ] Profiling data collected
- [ ] Bottlenecks identified
- [ ] Baseline performance documented
- [ ] Areas for optimization prioritized

**Status:** ❌ Not started

**Effort Estimate:** ~15 hours

### 4.2 Performance Optimization ❌
- [ ] Optimize identified bottlenecks
- [ ] Implement LOD for distant agents
- [ ] Add occlusion culling for off-screen agents
- [ ] Optimize collision detection (spatial partitioning)
- [ ] Reduce draw calls (batching)
- [ ] Optimize MercuryMessaging message filtering
- [ ] Re-profile after optimizations
- [ ] Verify 60+ FPS achieved

**Acceptance Criteria:**
- [ ] 60+ FPS sustained with 8 intersections
- [ ] 100+ pedestrians, 50+ vehicles running
- [ ] Memory usage acceptable (<2 GB)
- [ ] No stuttering or frame drops

**Status:** ❌ Not started

**Effort Estimate:** ~40 hours

### 4.3 Visual Polish ❌
- [ ] Import better vehicle models
- [ ] Add animated pedestrian models
- [ ] Implement particle effects (exhaust, dust)
- [ ] Add sound effects (traffic sounds, horns)
- [ ] Implement day/night cycle
- [ ] Add weather visual effects
- [ ] Test visuals on target hardware

**Acceptance Criteria:**
- [ ] Scene looks professional
- [ ] Agents are visually distinct
- [ ] Effects enhance realism
- [ ] Performance not degraded

**Status:** ❌ Not started - Medium priority

**Effort Estimate:** ~25 hours

### 4.4 UI Development ❌
- [ ] Create HUD layout
- [ ] Add real-time traffic statistics display
- [ ] Implement sentiment heat map
- [ ] Add intersection efficiency graphs
- [ ] Create user control panel (spawn rates, weather, time)
- [ ] Add debug visualization toggles
- [ ] Implement help/tutorial overlay
- [ ] Test UI usability

**Acceptance Criteria:**
- [ ] UI is clear and readable
- [ ] Real-time data updates correctly
- [ ] Controls are intuitive
- [ ] UI doesn't impact performance

**Status:** ❌ Not started

**Effort Estimate:** ~20 hours

---

## Phase 5: Unity Events Comparison Implementation ❌ CRITICAL - NOT STARTED

### 5.1 Scene Duplication ❌
- [ ] Duplicate `Scenario1.unity` → `Scenario1_UnityEvents.unity`
- [ ] Verify duplication successful
- [ ] Test duplicated scene runs
- [ ] Document differences to track during reimplementation

**Acceptance Criteria:**
- [ ] Duplicated scene identical to original
- [ ] Both scenes run independently
- [ ] No shared dependencies that could cause issues

**Status:** ❌ Not started

**Effort Estimate:** ~2 hours

### 5.2 Remove MercuryMessaging References ❌
- [ ] Identify all MmBaseResponder components
- [ ] Remove MmRelayNode components
- [ ] Remove MmRelaySwitchNode (Hub)
- [ ] Document what was removed (for reimplementation)
- [ ] Verify scene still loads (but won't function)

**Acceptance Criteria:**
- [ ] All MercuryMessaging components removed
- [ ] Scene loads without MercuryMessaging
- [ ] No compile errors
- [ ] Documentation of removed components complete

**Status:** ❌ Not started

**Effort Estimate:** ~8 hours

### 5.3 Reimplement with Unity Events ❌
- [ ] Create UnityEvent equivalents for all messages
- [ ] Reimplement TrafficLightController with Unity Events
- [ ] Reimplement HubController with Unity Events
- [ ] Reimplement Pedestrian with Unity Events
- [ ] Reimplement CarController with Unity Events
- [ ] Reimplement SentimentController with Unity Events
- [ ] Reimplement TrafficEventManager with Unity Events
- [ ] Reimplement SpawnManager with Unity Events
- [ ] Wire up all UnityEvent connections in Inspector
- [ ] Test reimplemented scene

**Acceptance Criteria:**
- [ ] Feature parity with MercuryMessaging version
- [ ] All behaviors identical
- [ ] Visual appearance identical
- [ ] Performance similar (within 10%)
- [ ] No MercuryMessaging dependencies

**Status:** ❌ Not started - CRITICAL

**Effort Estimate:** ~100 hours (largest single task)

### 5.4 Document Implementation Differences ❌
- [ ] Count Unity Event connections in Inspector
- [ ] Count lines of code changed
- [ ] Document coupling differences (references required)
- [ ] Measure implementation time
- [ ] Photograph Inspector for both versions
- [ ] Write comparison document

**Acceptance Criteria:**
- [ ] Quantitative comparison complete
- [ ] Coupling metrics calculated
- [ ] Implementation complexity documented
- [ ] Screenshots/evidence collected

**Status:** ❌ Not started

**Effort Estimate:** ~10 hours

### 5.5 Expert Review ❌
- [ ] Find Unity expert (not involved in project)
- [ ] Have expert review both implementations
- [ ] Document expert feedback
- [ ] Address any fairness concerns raised
- [ ] Get expert sign-off on equivalence

**Acceptance Criteria:**
- [ ] Expert review completed
- [ ] Fairness concerns addressed
- [ ] Both implementations validated as equivalent
- [ ] Expert willing to be acknowledged in paper

**Status:** ❌ Not started

**Effort Estimate:** ~10 hours (mostly coordination)

---

## Phase 6: User Study Preparation ❌ CRITICAL - NOT STARTED

### 6.1 Task Design ❌
- [ ] Design Task 1: Add new intersection
- [ ] Design Task 2: Implement new agent behavior
- [ ] Design Task 3: Add cross-intersection feature
- [ ] Design Task 4: Debug existing behavior
- [ ] Design Task 5: Performance optimization
- [ ] (Optional) Design Task 6-7: Advanced tasks
- [ ] Write detailed task instructions
- [ ] Create task evaluation rubrics
- [ ] Pilot test tasks with 2-3 developers

**Acceptance Criteria:**
- [ ] 5-7 tasks designed
- [ ] Tasks are clear and unambiguous
- [ ] Tasks are achievable in 15-20 minutes each
- [ ] Tasks test relevant skills
- [ ] Rubrics allow objective evaluation

**Status:** ❌ Not started - CRITICAL

**Effort Estimate:** ~40 hours

### 6.2 Data Collection Infrastructure ❌
- [ ] Design data collection schema
- [ ] Implement automatic action logging
- [ ] Add time tracking per task
- [ ] Implement error/bug detection and logging
- [ ] Add code complexity metrics collection
- [ ] Create NASA-TLX questionnaire integration
- [ ] Implement data export to CSV/JSON
- [ ] Test data collection system

**Acceptance Criteria:**
- [ ] All user actions logged
- [ ] Time tracked accurately
- [ ] Errors detected and logged
- [ ] Metrics calculated correctly
- [ ] NASA-TLX scores captured
- [ ] Data exports successfully

**Status:** ❌ Not started - CRITICAL

**Effort Estimate:** ~30 hours

### 6.3 Recruitment Materials ❌
- [ ] Write recruitment email
- [ ] Create study flyer/poster
- [ ] Write informed consent form
- [ ] Create pre-study questionnaire (demographics, experience)
- [ ] Create post-study debrief questionnaire
- [ ] Design compensation mechanism ($30-50/participant)
- [ ] Get materials approved (IRB if required)

**Acceptance Criteria:**
- [ ] All materials written
- [ ] Materials are clear and professional
- [ ] IRB approved (if required)
- [ ] Compensation mechanism ready

**Status:** ❌ Not started

**Effort Estimate:** ~20 hours

### 6.4 IRB Application (If Required) ❌
- [ ] Determine if IRB approval needed
- [ ] Prepare IRB application
- [ ] Write research protocol
- [ ] Submit IRB application
- [ ] Address IRB feedback
- [ ] Receive IRB approval
- [ ] Document IRB approval number

**Acceptance Criteria:**
- [ ] IRB approval obtained (if required)
- [ ] Or determination that IRB not required (with documentation)

**Status:** ❌ Not started - May be critical

**Effort Estimate:** ~30 hours + waiting time

### 6.5 Pilot Study ❌
- [ ] Recruit 2-3 pilot participants
- [ ] Run pilot sessions
- [ ] Collect pilot data
- [ ] Identify issues with tasks or procedures
- [ ] Refine tasks based on pilot feedback
- [ ] Update data collection based on pilot
- [ ] Document pilot results

**Acceptance Criteria:**
- [ ] 2-3 pilot participants complete study
- [ ] Issues identified and documented
- [ ] Tasks refined based on feedback
- [ ] Ready for full study

**Status:** ❌ Not started

**Effort Estimate:** ~20 hours + participant time

---

## Phase 7: User Study Execution ❌ NOT STARTED

### 7.1 Full Recruitment ❌
- [ ] Post recruitment materials (email lists, bulletin boards)
- [ ] Screen respondents (Unity experience, availability)
- [ ] Schedule 20-30 participants
- [ ] Send confirmation emails with logistics
- [ ] Prepare compensation (gift cards, cash, etc.)

**Acceptance Criteria:**
- [ ] 20-30 participants recruited
- [ ] All scheduled
- [ ] Logistics confirmed

**Status:** ❌ Not started

**Effort Estimate:** ~30 hours over 2-3 weeks

### 7.2 Run Study Sessions ❌
- [ ] Prepare study room (computer, recording equipment)
- [ ] Conduct 20-30 study sessions (2 hours each)
- [ ] Monitor for issues during sessions
- [ ] Collect all data
- [ ] Compensate participants
- [ ] Debrief participants
- [ ] Thank participants and get consent for data use

**Acceptance Criteria:**
- [ ] 20-30 sessions completed
- [ ] All data collected
- [ ] No data loss
- [ ] Participants compensated

**Status:** ❌ Not started

**Effort Estimate:** ~100 hours (40-60 hours of sessions + prep/debrief)

### 7.3 Data Quality Assurance ❌
- [ ] Check all data files collected
- [ ] Verify no missing data
- [ ] Identify any data anomalies
- [ ] Re-contact participants if needed (missing questionnaires)
- [ ] Organize data for analysis
- [ ] Backup all data

**Acceptance Criteria:**
- [ ] All data complete
- [ ] Data organized
- [ ] Data backed up
- [ ] Ready for analysis

**Status:** ❌ Not started

**Effort Estimate:** ~10 hours

---

## Phase 8: Data Analysis and Paper Writing ❌ NOT STARTED

### 8.1 Statistical Analysis ❌
- [ ] Import data into analysis software (R, Python, SPSS)
- [ ] Perform descriptive statistics
- [ ] Run inferential tests (t-tests, ANOVA, etc.)
- [ ] Calculate effect sizes
- [ ] Check assumptions (normality, homogeneity)
- [ ] Run post-hoc tests if needed
- [ ] Generate figures and tables
- [ ] Write results section

**Acceptance Criteria:**
- [ ] Statistical tests completed
- [ ] Significant results identified
- [ ] Effect sizes calculated
- [ ] Figures/tables publication-ready
- [ ] Results section drafted

**Status:** ❌ Not started

**Effort Estimate:** ~40 hours

### 8.2 Qualitative Analysis ❌
- [ ] Transcribe participant feedback (if recorded)
- [ ] Code qualitative responses
- [ ] Identify themes
- [ ] Select representative quotes
- [ ] Integrate with quantitative results

**Acceptance Criteria:**
- [ ] Qualitative data coded
- [ ] Themes identified
- [ ] Quotes selected
- [ ] Integrated into discussion

**Status:** ❌ Not started

**Effort Estimate:** ~20 hours

### 8.3 Paper Writing ❌
- [ ] Write introduction
- [ ] Write related work
- [ ] Write methods section
- [ ] Write results section (using 8.1 output)
- [ ] Write discussion
- [ ] Write conclusion
- [ ] Create abstract
- [ ] Format references
- [ ] Create all figures and tables
- [ ] Polish writing

**Acceptance Criteria:**
- [ ] Full draft complete
- [ ] All sections written
- [ ] Figures/tables included
- [ ] References formatted
- [ ] Word count within limit

**Status:** ❌ Not started

**Effort Estimate:** ~60 hours

### 8.4 Internal Review ❌
- [ ] Send draft to co-authors
- [ ] Collect feedback
- [ ] Revise based on feedback
- [ ] Iterate until co-authors approve
- [ ] Proofread final version
- [ ] Check formatting against UIST requirements

**Acceptance Criteria:**
- [ ] All co-authors approve
- [ ] Revisions complete
- [ ] Formatting correct
- [ ] Ready to submit

**Status:** ❌ Not started

**Effort Estimate:** ~20 hours + waiting time

### 8.5 Submission ❌
- [ ] Prepare supplementary materials
- [ ] Create video figure (if required)
- [ ] Upload to submission system
- [ ] Pay submission fee (if any)
- [ ] Verify submission received
- [ ] Celebrate! 🎉

**Acceptance Criteria:**
- [ ] Submitted by April 9, 2025 deadline
- [ ] Confirmation received
- [ ] All materials uploaded

**Status:** ❌ Not started

**Deadline:** April 9, 2025 (Abstract: April 2, 2025)

---

## Timeline and Milestones

### Critical Path to UIST Submission

**Week 1-2 (Now - Dec 2, 2025):**
- ✅ Verify Unity project after reorganization
- 🔨 Complete Intersection_01 (crossings, spawn points)
- 🔨 Create intersection prefabs
- 🔨 Add 7 more intersections

**Week 3-4 (Dec 2-16, 2025):**
- Implement cross-intersection coordination
- Performance profiling and optimization
- Begin Unity Events implementation

**Week 5-8 (Dec 16 - Jan 13, 2025):**
- Complete Unity Events implementation
- Expert review
- Task design
- Pilot study

**Week 9-12 (Jan 13 - Feb 10, 2025):**
- IRB application (if needed)
- Recruitment materials
- Recruit participants
- Begin user study sessions

**Week 13-16 (Feb 10 - Mar 10, 2025):**
- Complete user study sessions (20-30 participants)
- Data quality assurance
- Begin data analysis

**Week 17-18 (Mar 10-24, 2025):**
- Complete statistical analysis
- Generate figures/tables
- Write results section

**Week 19-20 (Mar 24 - Apr 7, 2025):**
- Complete full paper draft
- Internal review and revisions
- Proofread and format

**Week 21 (Apr 7-9, 2025):**
- Final polishing
- Submit to UIST (April 9 deadline)

---

## Risk Assessment

### High-Risk Tasks (Could Derail Timeline)

1. **Unity Events Implementation** (100 hours)
   - Largest single task
   - Could reveal unforeseen issues
   - Mitigation: Start early, allocate extra buffer time

2. **Participant Recruitment** (30 hours + time)
   - Hard to predict success
   - Could take longer than expected
   - Mitigation: Multiple recruitment channels, start early, offer compensation

3. **IRB Approval** (30 hours + waiting)
   - Unknown timeline
   - Could delay by weeks/months
   - Mitigation: Submit early, check if actually required

4. **Performance with 8-12 Intersections** (40 hours)
   - Could be harder than expected
   - Mitigation: Profile early, simplify if needed

### Medium-Risk Tasks

5. **Pilot Study Issues** (20 hours)
   - May reveal task problems
   - Mitigation: Build in time for refinement

6. **Data Collection System** (30 hours)
   - Technical issues could arise
   - Mitigation: Test thoroughly before full study

### Low-Risk Tasks

7. **Additional Intersections** (40 hours)
   - Straightforward with prefabs
   - Well-understood work

---

## Success Criteria (Overall)

### Minimum Viable Product for UIST Submission

- [ ] 8 intersections functioning in both implementations
- [ ] 100+ pedestrians, 50+ vehicles
- [ ] 60+ FPS sustained
- [ ] Unity Events version feature-complete
- [ ] 20-30 participants complete user study
- [ ] Statistically significant results (p < 0.05)
- [ ] Paper submitted by April 9, 2025

### Stretch Goals

- [ ] 12 intersections (including advanced types)
- [ ] Visual polish (better models, effects)
- [ ] 30+ participants
- [ ] Multiple papers (UIST + journal extension)
- [ ] Open-source release of scene

---

## Resources and Dependencies

### Personnel Needed
- 1 primary developer (scene implementation) - 300 hours
- 1 assistant developer (Unity Events) - 100 hours
- 1 study coordinator (recruitment, admin) - 100 hours
- 1 data analyst (statistics) - 80 hours

### Infrastructure
- Unity Pro license (profiling)
- Participant computers (standardized)
- Data collection server
- IRB approval (if required)

### Budget
- Participant compensation: $30 × 30 = $900
- Software licenses: $200
- Equipment: $500
- Contingency: $400
- **Total:** ~$2,000

---

## Next Actions (Priority Order)

1. ⚠️ **CRITICAL: Verify Unity project after reorganization**
2. 🔨 **Complete Intersection_01 (crossings, spawn points)**
3. 🔨 **Create intersection prefabs**
4. ❌ **Add 7 more intersections**
5. ❌ **Begin Unity Events implementation (ASAP)**
6. ❌ **Design user study tasks**
7. ❌ **Check IRB requirements**
8. ❌ **Start recruitment planning**

---

**Status:** 🔨 In Active Development
**Next Immediate Action:** Verify Unity project, complete Intersection_01
**Critical Deadline:** UIST Submission (April 9, 2025) - **4.5 months away**
**Last Updated:** 2025-11-18
