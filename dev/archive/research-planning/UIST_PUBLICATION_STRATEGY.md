# UIST Publication Strategy for MercuryMessaging

**Date:** 2025-11-27 (Updated from 2025-11-21)
**Author:** Research Planning Analysis
**Target Venues:** UIST 2026, CHI 2026, ICSE 2026

---

## Update Notice (2025-11-27)

**Priority Order Changed:** Production Engineering + Networking tasks now come BEFORE research publications. See `dev/IMPROVEMENT_TRACKER.md` for current roadmap.

**Timeline Adjusted:** All targets moved to 2026 venues (UIST 2026 deadline ~April 2026).

**New Task Added:** DSL/DX Improvements (120h) - potential CHI LBW 2025 publication on developer productivity.

---

## Executive Summary

After analyzing the MercuryMessaging framework architecture, UIST evaluation criteria, and competitive landscape (MessagePipe, UniRx, Zenject, DOTS), this document recommends **3 strong UIST contributions** that align with UIST's systems focus, can be implemented within 6-12 weeks, and offer novel technical contributions to the UI software community.

**Mercury's Unique Differentiation:** ONLY framework combining hierarchical + spatial + networked routing.

**Top 3 Recommendations (Updated for 2026):**
1. **Spatial Indexing** - Hybrid spatial-hierarchical routing for metaverse-scale XR (9 weeks, 360h) - UIST 2026
2. **Parallel FSMs** - Multi-modal XR interaction via parallel state machines (7 weeks, 280h) - UIST 2026
3. **Network Prediction** - Hierarchical client-side prediction for distributed XR (10 weeks, 400h) - CHI 2026

**Bonus:** DSL Developer Productivity Study - CHI LBW 2025 (40h)

---

## Table of Contents

1. [Current Framework Strengths](#current-framework-strengths)
2. [Recommended Publications](#recommended-publications)
3. [Task Categorization](#task-categorization)
4. [Publication Timeline](#publication-timeline)
5. [Success Criteria](#success-criteria)
6. [Next Steps](#next-steps)

---

## Current Framework Strengths

### What MercuryMessaging Already Has

✅ **Hierarchical message routing** through Unity scene graphs (1422-line MmRelayNode)
✅ **Multi-dimensional filtering** (level, tag, active, network, FSM state)
✅ **Performance baseline:** 980 msg/sec at 53.5 FPS with 100+ responders
✅ **Validated optimizations:** 2-2.2x frame time improvement, 3-35x throughput improvement
✅ **FSM integration** via MmRelaySwitchNode
✅ **Network-aware routing** with Photon integration
✅ **Comprehensive test infrastructure** with automated validation

### Gap vs. State-of-the-Art

❌ Current systems (Unity's EventSystem, URP/HDRP, React/Redux) don't combine hierarchical routing + spatial indexing + network prediction
❌ No existing framework optimizes for XR-scale message volumes (1000+ msg/sec)
❌ Traditional UI frameworks lack multi-modal interaction support (gesture + voice + gaze)

---

## Recommended Publications

### 🥇 RECOMMENDATION 1: Hybrid Spatial-Hierarchical Message Routing

**Target:** UIST 2025 (October deadline ~April 2025) or UIST 2026
**Effort:** 360 hours (9 weeks)
**Priority:** P0 (Primary Target)

#### Core Innovation

First messaging framework combining spatial indexing with scene graph hierarchies for **O(log n) routing at metaverse scale (10,000+ users)**.

#### Why This is Novel for UIST

1. **Systems Contribution:** Hybrid spatial-hierarchical index structure (octree + scene graph cross-reference)
2. **Performance Breakthrough:** O(n) → O(log n) routing complexity for spatial queries
3. **Real-World Impact:** Enables shared XR experiences at unprecedented scale
4. **Technical Depth:** GPU-accelerated queries, LOD-based message filtering, adaptive octree
5. **UIST Fit:** Directly addresses UI software architecture for next-gen spatial interfaces

#### Technical Highlights

**Novel Algorithm:** Cross-referenced dual index

```
Scene Graph (parent-child) ←→ Spatial Octree (3D proximity)
                ↓
        Query Engine (hybrid traversal)
                ↓
    O(log n) message delivery
```

**Performance Targets:**
- 10,000+ concurrent responders (vs. current 100)
- <1ms query time for 100k objects (vs. current O(n) scan)
- 60 FPS sustained with spatial routing
- GPU acceleration: 10-100x speedup

#### Evaluation Plan

1. **Microbenchmarks:** Spatial query performance vs. scene graph depth
2. **Scaling Study:** 10 → 1000 → 10,000 responders, measure frame time/throughput
3. **Comparison:** vs. Unity's hierarchical queries, vs. flat spatial hash
4. **Case Study:** Metaverse-scale demo (1000+ user avatars with proximity-based messaging)

#### Implementation Timeline (9 weeks = 360h)

- **Phase 1 (3 weeks):** Adaptive octree + cross-reference system
- **Phase 2 (3 weeks):** GPU compute shaders + LOD filtering
- **Phase 3 (3 weeks):** Integration + benchmarking + demo scene

#### Why This Wins at UIST

✅ **Novelty:** No prior work combines spatial + hierarchical for message routing
✅ **Impact:** Enables 10-100x scale improvement (100 → 10,000 users)
✅ **Systems Focus:** Core contribution is architectural + algorithmic
✅ **Evaluation:** Strong quantitative + qualitative (demo video)
✅ **Fit:** Spatial interfaces are hot topic (AR/VR/Metaverse)

#### Paper Structure

- **Title:** "Hybrid Spatial-Hierarchical Message Routing for Metaverse-Scale XR Applications"
- **Abstract:** GPU-accelerated dual-index system achieving O(log n) routing
- **Sections:** Architecture, Algorithms, Implementation, Evaluation (4 studies), Discussion
- **Page Count:** 10 pages (UIST format)

---

### 🥈 RECOMMENDATION 2: Parallel Hierarchical FSMs for Multi-Modal XR

**Target:** UIST 2025 or CHI 2026
**Effort:** 280 hours (7 weeks)
**Priority:** P1 (Secondary Target)

#### Core Innovation

First parallel orthogonal state machine system for coordinating **multi-modal inputs (gesture + voice + gaze)** in XR via message-based synchronization.

#### Why This is Novel for UIST

1. **Interaction Technique:** New way to compose complex multi-modal interactions
2. **Systems Contribution:** Lock-free parallel FSM execution with message-based coordination
3. **Developer Tool:** Enables 3-5x faster development of multi-modal XR interfaces
4. **UIST Fit:** Multi-modal interaction is core UIST research area

#### Technical Highlights

**Novel Architecture:** Parallel orthogonal state regions communicating via Mercury messages

```
Root FSM
├── Interaction Layer (parallel)
│   ├── Gesture FSM (MmRelaySwitchNode)
│   ├── Voice FSM (MmRelaySwitchNode)
│   └── Gaze FSM (MmRelaySwitchNode)
└── Message Bus (conflict resolution)
```

**Key Innovation:** No shared memory, pure message-passing synchronization
- Prevents race conditions through message isolation
- Event ordering guarantees maintained
- Priority-based conflict resolution

**Performance Targets:**
- <1ms state transition overhead
- Linear scaling to 10+ parallel regions
- Zero deadlocks (formal verification)

#### Evaluation Plan

1. **Developer Study:** 12 developers build multi-modal XR app, measure dev time
2. **Performance Benchmarks:** State transition overhead, scaling, memory usage
3. **Comparison:** vs. monolithic FSM, vs. Unity Animator, vs. manual coordination
4. **Case Studies:** 3 applications (VR painting, gesture+voice UI, gaze+hand manipulation)

#### Implementation Timeline (7 weeks = 280h)

- **Phase 1 (2 weeks):** Parallel region infrastructure + message synchronization
- **Phase 2 (3 weeks):** Conflict resolution + cross-FSM communication
- **Phase 3 (2 weeks):** Developer tools + user study

#### Why This Wins at UIST

✅ **Novelty:** First message-based parallel FSM for multi-modal XR
✅ **User Study:** Developer experience evaluation (critical for UIST)
✅ **Impact:** Reduces multi-modal development time by 3-5x
✅ **Technical Depth:** Lock-free synchronization + conflict resolution
✅ **Fit:** Multi-modal interaction is UIST sweet spot

#### Paper Structure

- **Title:** "Parallel Hierarchical State Machines for Multi-Modal XR Interaction"
- **Abstract:** Message-based parallel FSMs enabling rapid multi-modal prototyping
- **Sections:** Motivation, Architecture, Implementation, Developer Study (N=12), Case Studies
- **Page Count:** 10 pages

---

### 🥉 RECOMMENDATION 3: Hierarchical Client-Side Prediction

**Target:** UIST 2026 or CHI 2026
**Effort:** 400 hours (10 weeks)
**Priority:** P2 (Tertiary Target / Backup)

#### Core Innovation

Extends traditional client-side prediction to hierarchical scene graphs, achieving **100-200ms perceived latency reduction** in distributed XR.

#### Why This is Novel for UIST

1. **Systems Contribution:** Hierarchical state prediction + scene-graph-aware reconciliation
2. **Performance Impact:** 50-150ms latency compensation (critical for XR)
3. **Real-World Need:** Enables responsive multi-user XR under network latency
4. **UIST Fit:** Network performance for distributed UIs

#### Technical Highlights

**Novel Algorithm:** Route prediction + hierarchical reconciliation

```
Client predicts message routes through scene graph
   ↓
Applies state changes optimistically
   ↓
Server validates and broadcasts authoritative state
   ↓
Client reconciles differences with smooth interpolation
```

**Key Innovation:** Predicts entire message routing paths (not just object states)
- Uses MmMetadataBlock filters to predict affected nodes
- Confidence scoring based on routing table stability
- Partial rollback for isolated conflicts

**Performance Targets:**
- 100-200ms perceived latency reduction
- >90% prediction accuracy for common patterns
- <2ms reconciliation overhead
- 40% bandwidth reduction via delta compression

#### Evaluation Plan

1. **Controlled Study:** 24 users, measure task completion time under 50-200ms latency
2. **Network Simulation:** Varied latency/packet loss, measure prediction accuracy
3. **Comparison:** vs. no prediction, vs. naive object prediction, vs. lockstep
4. **Bandwidth Analysis:** Delta compression effectiveness

#### Implementation Timeline (10 weeks = 400h)

- **Phase 1 (3 weeks):** Route prediction + state history
- **Phase 2 (4 weeks):** Reconciliation + interpolation
- **Phase 3 (3 weeks):** Network optimization + user study

#### Why This Wins at UIST

✅ **Novelty:** First hierarchical prediction for scene graphs
✅ **User Study:** Perceived latency evaluation (24 participants)
✅ **Impact:** 100-200ms latency compensation (critical for XR)
✅ **Technical Depth:** Prediction algorithms + reconciliation strategies
✅ **Fit:** Network performance for distributed UIs

#### Paper Structure

- **Title:** "Hierarchical Client-Side Prediction for Low-Latency Distributed XR"
- **Abstract:** Scene-graph-aware prediction achieving 100-200ms latency reduction
- **Sections:** Background, Prediction Algorithm, Reconciliation, User Study (N=24), Discussion
- **Page Count:** 10 pages

---

## Why Other Options Were Not Recommended

### ❌ Parallel Message Dispatch (Not Recommended for First Paper)

**Location:** `dev/active/parallel-dispatch/`
**Effort:** 480 hours (12 weeks)

**Concerns:**
- Performance gain (10-50x) impressive but **limited novelty**
- Job system parallelization is well-studied (Unity's ECS, URP, etc.)
- Too low-level systems (better for SIGGRAPH/IEEE VR)
- 12 weeks too long for incremental improvement

**Recommendation:** Defer to second wave after establishing framework

---

### ❌ Error Recovery (Not Recommended)

**Location:** `dev/active/error-recovery/`
**Effort:** 320 hours (8 weeks)

**Concerns:**
- Self-healing is interesting but **hard to evaluate objectively**
- Requires failure injection, chaos testing, 99.99% uptime metrics
- More suitable for reliability conferences (ICSE/FSE)
- Evaluation challenges outweigh novelty

**Recommendation:** Consider for safety-critical HCI venues

---

### ❌ Visual Programming Interface (Not Recommended for First Paper)

**Location:** `dev/active/visual-composer/`
**Effort:** 212 hours (5-6 weeks)

**Concerns:**
- Tool paper without strong **algorithmic contribution**
- UIST prefers tools WITH novel algorithms
- 5-6 weeks feasible but insufficient novelty alone

**Recommendation:** Good as **second paper** after establishing framework, or combine with spatial indexing

---

## Task Categorization

### 🏆 Major Paper Contributions (Research-Grade)

| Task | Category | Effort | Priority | Venue | Status |
|------|----------|--------|----------|-------|--------|
| **Spatial Indexing** | 🏆 Paper | 360h | **P0** | UIST 2025 | **PRIMARY** |
| **Parallel FSMs** | 🏆 Paper | 280h | **P1** | UIST 2025/26 | **SECONDARY** |
| **Network Prediction** | 🏆 Paper | 400h | **P2** | CHI 2026 | **TERTIARY** |
| Parallel Dispatch | 📝 Paper | 480h | P3 | SIGGRAPH | Deferred |
| Error Recovery | 📝 Paper | 320h | P4 | ICSE/FSE | Deferred |
| Visual Composer | 📝 Paper | 212h | P5 | UIST (2nd) | Deferred |

### ⚡ Quick Wins / Engineering Improvements

| Task | Category | Effort | Priority | Impact | Status |
|------|----------|--------|----------|--------|--------|
| Routing Optimization | ⚡ Infrastructure | 530h* | P6 | Foundational | Break into chunks |
| Network Performance | ⚡ Production | 292h* | P7 | Production-ready | Break into chunks |
| Standard Library | ⚡ DevEx | 228h | P8 | Adoption | Low priority |
| Thread Safety | ⚡ Future-proof | 4-6h | P9 | Deferred | Very low |

**Note:** Tasks marked with * should be broken into smaller 50-100h incremental wins rather than one massive project.

### 📊 Supporting Infrastructure

| Task | Status | Notes |
|------|--------|-------|
| Performance Analysis | ✅ Complete | QW-1 through QW-6 validated |
| User Study | 🔧 Active | Task management infrastructure |

---

## Publication Timeline (Updated 2025-11-27)

### Current Priority: Production Engineering First

Per user-approved roadmap (see `dev/IMPROVEMENT_TRACKER.md`):

**Q1 2025 (Now - March):**
- **P1:** Performance Optimization (300h) - MessagePipe parity
- **P2:** FishNet Networking (200h) - Production multiplayer
- **P3:** DSL/DX Improvements (120h) - Tutorials, shorter syntax

**Q2 2025 (April - June):**
- **P4:** Fusion 2 Networking (200h) - Alternative backend
- **P5:** User Study (40h) - Submit to CHI LBW 2025 (April deadline)
- Begin Spatial Indexing prototype (100h)

**Q3-Q4 2025 (July - December):**
- **P6:** Spatial Indexing (360h) - Core implementation
- **P7:** Parallel FSMs (280h) - Core implementation
- Paper drafts for both

**Q1 2026 (January - March):**
- Finalize evaluations and user studies
- Submit to UIST 2026 (deadline ~April 2026)

**Q2-Q3 2026:**
- **P8:** Network Prediction (400h) - Implementation
- **P9:** Visual Composer (360h) - Implementation
- Submit to CHI 2027 / UIST 2027

### Result: 4+ papers over 24 months

1. **CHI LBW 2025:** User Study (Mercury vs UnityEvents)
2. **UIST 2026:** Spatial Indexing
3. **UIST 2026:** Parallel FSMs
4. **CHI 2027:** Network Prediction
5. **UIST 2027:** Visual Composer (or combine with another)

---

## Success Criteria for UIST Acceptance

### Technical Novelty (Critical)

For **Spatial Indexing:**
- ✅ Hybrid spatial-hierarchical index (no prior work)
- ✅ GPU-accelerated query system (10-100x speedup)
- ✅ O(log n) routing complexity (vs. O(n) baseline)

For **Parallel FSMs:**
- ✅ Message-based parallel FSM coordination
- ✅ Lock-free synchronization
- ✅ Conflict resolution for multi-modal inputs

For **Network Prediction:**
- ✅ Hierarchical route prediction (not just state)
- ✅ Scene-graph-aware reconciliation
- ✅ 100-200ms latency reduction

### Evaluation Rigor (Critical)

- ✅ **Quantitative benchmarks** (frame time, throughput, scaling)
- ✅ **Comparison study** (vs. 2-3 baselines)
- ✅ **User study** (when applicable - FSMs, Network Prediction)
- ✅ **Case study** (real-world demo with video)

### Impact (Important)

- ✅ **Scale improvement** (10-100x for Spatial, 3-5x for FSMs)
- ✅ **Open-source release** (reproducibility)
- ✅ **Real-world applicability** (Unity integration)

### Writing Quality (Important)

- ✅ **Clear motivation** (why this problem matters)
- ✅ **Accessible explanation** (non-expert readers)
- ✅ **Strong figures** (architecture diagrams, performance graphs)
- ✅ **Video figure** (60-90 second demo)

---

## UIST Evaluation Criteria (Official Guidelines)

Based on UIST Author Guide research:

### Accepted Evaluation Strategies

1. **Demonstration** - Novel/replicated examples, case studies, scenarios
2. **Usage Studies** - Usability, A/B comparisons, walkthroughs, take-home studies
3. **Technical Performance** - Benchmarking against thresholds or state of art
4. **Heuristics** - Checklist approaches, discussion based on heuristics

### Common Biases to Avoid

❌ Assuming work leveraging known techniques lacks novelty
❌ Characterizing technical novelty as mere engineering
❌ Expecting complete system when core contribution is one part
❌ Dismissing end-to-end implementations as lacking novelty

### Novelty Requirements

- ~70% new material over existing archival work
- Significant new developments (not incremental updates)
- Novel functionality through bricolage is acceptable

### Key Frameworks

- **Olsen's UIST 2007 heuristics:** "Evaluating Interface Systems Research"
- **Ledo et al.'s CHI 2018:** "Evaluation Strategies for HCI Toolkit Research"

---

## Next Steps

### Immediate Actions (This Week)

1. **Choose Primary Target:** Recommend **Spatial Indexing** (Recommendation #1)
   - Highest novelty + impact
   - Strong quantitative evaluation
   - 9-week timeline achievable
   - Metaverse scale is hot topic

2. **Create Implementation Plan:**
   - Break down 360 hours into weekly milestones
   - Identify technical risks early
   - Set up benchmarking infrastructure
   - Design evaluation studies upfront

3. **Build Proof-of-Concept (Week 1-3):**
   - Basic adaptive octree
   - Scene graph cross-reference
   - Simple spatial queries
   - Performance baseline measurements

### Month 1: Prototype & Validation

**Week 1-2:** Core algorithm implementation
- Adaptive octree data structure
- Scene graph traversal integration
- Cross-reference management

**Week 3-4:** Initial benchmarking
- Query time measurements
- Scaling tests (10 → 100 → 1000 responders)
- Compare vs. O(n) baseline

### Month 2: Optimization & GPU Acceleration

**Week 5-6:** GPU compute shaders
- Spatial query kernels
- Batch processing
- Memory optimization

**Week 7-8:** LOD and filtering
- Level-of-detail message filtering
- Frustum culling integration
- Performance tuning

### Month 3: Evaluation & Paper Writing

**Week 9:** Case study implementation
- Metaverse-scale demo scene
- 1000+ user simulation
- Video capture

**Week 10:** Comparison studies
- vs. Unity built-in queries
- vs. flat spatial hash
- vs. pure scene graph

**Week 11-12:** Paper writing
- Draft all sections
- Generate figures and graphs
- Internal review
- Submit to UIST

---

## Research Questions

### For Spatial Indexing

**RQ1:** Can hybrid spatial-hierarchical indexing achieve O(log n) routing complexity for spatial queries in Unity scene graphs?

**RQ2:** How does query performance scale from 10 to 10,000 responders across varying scene graph depths (3, 5, 7, 10 levels)?

**RQ3:** What is the GPU acceleration overhead vs. benefit threshold for spatial queries in XR applications?

**RQ4:** Can LOD-based message filtering maintain 60 FPS at metaverse scale (1000+ concurrent users)?

### For Parallel FSMs

**RQ1:** Does message-based parallel FSM coordination reduce multi-modal XR development time compared to monolithic state machines?

**RQ2:** What is the state transition overhead for parallel orthogonal regions vs. single-threaded FSMs?

**RQ3:** How effective is priority-based conflict resolution for simultaneous gesture, voice, and gaze inputs?

**RQ4:** Can developers successfully compose complex multi-modal interactions using parallel FSMs without training?

### For Network Prediction

**RQ1:** What prediction accuracy can hierarchical route prediction achieve compared to naive object-based prediction?

**RQ2:** How much perceived latency reduction do users experience with scene-graph-aware client-side prediction?

**RQ3:** What is the reconciliation overhead for hierarchical state conflicts vs. flat state correction?

**RQ4:** How does prediction accuracy degrade under varying network conditions (latency 50-200ms, packet loss 0-5%)?

---

## Risk Assessment & Mitigation

### Technical Risks

**Risk 1:** Spatial indexing overhead exceeds query performance gains
- **Mitigation:** Early benchmarking, adaptive threshold switching
- **Fallback:** Hybrid mode (spatial for dense, hierarchical for sparse)

**Risk 2:** GPU compute shader incompatibility across platforms
- **Mitigation:** CPU fallback implementation, platform testing
- **Fallback:** Focus on desktop VR (Quest, PCVR)

**Risk 3:** Parallel FSM deadlocks or race conditions
- **Mitigation:** Formal verification, extensive testing
- **Fallback:** Message queue isolation, timeout mechanisms

**Risk 4:** Network prediction accuracy too low (<70%)
- **Mitigation:** Confidence-based switching, conservative prediction
- **Fallback:** Hybrid approach (high-confidence only)

### Research Risks

**Risk 1:** Papers rejected due to insufficient novelty
- **Mitigation:** Focus on unique hybrid approaches, not incremental improvements
- **Backup Venues:** SIGGRAPH, IEEE VR, CHI, ICSE

**Risk 2:** User studies fail to recruit enough participants
- **Mitigation:** Start recruitment early, offer compensation
- **Fallback:** Case studies + expert evaluation

**Risk 3:** Implementation takes longer than estimated
- **Mitigation:** Aggressive early prototyping, weekly milestones
- **Fallback:** Pivot to Parallel FSMs (shorter timeline)

### Timeline Risks

**Risk 1:** Miss UIST 2025 deadline (April)
- **Mitigation:** Start NOW, weekly progress tracking
- **Fallback:** Target UIST 2026 instead

**Risk 2:** Overcommitment to multiple papers
- **Mitigation:** Focus on ONE paper at a time
- **Fallback:** Sequential publication strategy

---

## Budget & Resources

### Development Time

**Spatial Indexing:** 360 hours (9 weeks full-time)
- Implementation: 200h
- Evaluation: 100h
- Paper writing: 60h

**Parallel FSMs:** 280 hours (7 weeks full-time)
- Implementation: 150h
- User study: 80h
- Paper writing: 50h

**Network Prediction:** 400 hours (10 weeks full-time)
- Implementation: 220h
- User study: 120h
- Paper writing: 60h

### Equipment & Software

**Required:**
- Unity Pro license (for profiling tools)
- GPU with compute shader support (RTX 2060+)
- VR headset (Quest 2/3 or PCVR)

**Recommended:**
- Multiple VR headsets (for multi-user testing)
- High-end GPU (RTX 4080+) for scaling tests
- Cloud compute for large-scale simulations

### User Study Costs

**Parallel FSMs Developer Study (N=12):**
- Compensation: $50/developer × 12 = $600
- Duration: 2 hours per participant
- Total: ~$600 + recruitment overhead

**Network Prediction User Study (N=24):**
- Compensation: $30/participant × 24 = $720
- Duration: 1 hour per participant
- Total: ~$720 + recruitment overhead

---

## Long-Term Vision

### Publication Roadmap (2025-2027)

**2025:**
- UIST 2025: Spatial Indexing
- UIST 2026: Parallel FSMs
- CHI 2026: Network Prediction

**2026:**
- SIGGRAPH 2026: Parallel Dispatch (performance focus)
- ICSE 2026: Error Recovery (safety-critical)
- UIST 2027: Visual Composer (combining spatial + visual)

**2027:**
- CHI 2027: AI-Assisted Routing Optimization
- FSE 2027: Formal Verification System

### Impact Goal

**Establish MercuryMessaging as THE framework for scalable XR interaction systems**

**Success Metrics:**
- 5+ tier 1 publications (UIST, CHI, SIGGRAPH)
- 1000+ GitHub stars
- 100+ real-world deployments
- Integration into Unity Asset Store
- Adoption by major VR/XR studios

---

## Conclusion

The MercuryMessaging framework is positioned for significant research contributions to UIST and related top-tier venues. By focusing on **Spatial Indexing** as the first major contribution, followed by **Parallel FSMs** and **Network Prediction**, the project can establish a strong publication record over 18-24 months.

**Key Success Factors:**
1. Focus on ONE paper at a time (no parallel paper development)
2. Start with strong prototype validation (3 weeks)
3. Design evaluation studies BEFORE implementation
4. Write paper incrementally (don't wait until end)
5. Get early feedback from advisors/colleagues
6. Submit to backup venues if needed

**Recommended Immediate Action:**
Begin Spatial Indexing prototype THIS WEEK. Build proof-of-concept octree + scene graph integration in 3 weeks to validate feasibility before committing to full implementation.

---

**Questions for Discussion:**

1. Do you agree with prioritizing **Spatial Indexing** as the first paper target?
2. What is your realistic timeline? Can you dedicate 9 weeks (360h) to implementation + evaluation?
3. Do you have access to Unity Pro + GPU compute shaders for implementation?
4. Can you recruit developers for user studies (Parallel FSMs would need N=12)?
5. Should we create a detailed week-by-week implementation plan for Spatial Indexing?
6. Are there specific UIST reviewers or prior work we should be aware of?
7. What backup venues should we prepare for if UIST rejects?

---

*Last Updated: 2025-11-27*
*Next Review: End of Q1 2025*
*Status: Production Engineering Phase - Research begins Q3 2025*

---

## Competitive Analysis (Added 2025-11-27)

| Framework | Performance | Hierarchical | Spatial | Network | Mercury Advantage |
|-----------|-------------|--------------|---------|---------|-------------------|
| **MessagePipe** | 78x faster | No | No | No | Mercury adds hierarchy + network |
| **UniRx/R3** | Good | No | No | No | Mercury adds structure |
| **Zenject** | Moderate | No | No | No | Mercury adds routing |
| **Unity DOTS** | Best | No | Via queries | No | Mercury easier adoption |
| **SendMessage** | 10x slower | Via hierarchy | No | No | Mercury adds filtering |

**Key Research Differentiation:** No existing framework combines all three:
1. Hierarchical routing through scene graph
2. Spatial routing via 3D proximity
3. Network-aware message synchronization

This unique combination is the core publication narrative.
