# Project Renaming Options

**Purpose:** Distinguish new research contributions from the original CHI 2018 MercuryMessaging paper.

**Date:** 2025-12-17

---

## New Capabilities Being Developed

The following active development tasks represent significant expansions beyond CHI 2018:

| Task | Contribution |
|------|-------------|
| Visual Composer | Live visual authoring & message flow debugging |
| Time-Travel Debugging | Message replay and "why didn't it arrive?" queries |
| Spatial Indexing | GPU-accelerated spatial queries for message routing |
| Parallel Dispatch/FSM | Concurrent processing and multi-modal state machines |
| Digital Twin Layer | MQTT/Mercury bridge for IoT systems |
| XR Collaboration | Multi-user hierarchical routing with role-based filtering |
| Static Analysis | Hybrid safety verification (Tarjan + Bloom filters) |
| Accessibility Framework | Accessibility-first game development patterns |
| LLM Message Design | Natural language to routing configuration |

---

## Naming Options

### Option 1: Evolve the Mercury Brand

| Name | Rationale |
|------|-----------|
| **Mercury 2** | Simple version increment, clear lineage |
| **MercuryLive** | Emphasizes live introspection/debugging |
| **MercuryGraph** | Emphasizes visual graph authoring |
| **MercuryVis** | Emphasizes visual tooling contribution |

**Pros:** Brand recognition, clear evolution from CHI 2018
**Cons:** May not sufficiently distinguish the new contributions

---

### Option 2: Mythology-Based (Same Domain)

| Name | Rationale |
|------|-----------|
| **Hermes** | Greek equivalent of Mercury, messenger god |
| **Iris** | Greek goddess of messages/rainbows (visual!) |
| **Caduceus** | Mercury's staff (symbol of communication) |

**Pros:** Thematic continuity, memorable
**Cons:** "Hermes" is overused in tech

---

### Option 3: Descriptive Compound Names (Acronyms)

| Name | Expansion | Emphasizes |
|------|-----------|------------|
| **PRISM** | Parallel Routing with Interactive Spatial Messaging | Spatial + parallel + visual |
| **HIVE** | Hierarchical Interactive Visual Environment | Visual composer focus |
| **SCOPE** | Scene-graph Communication with Observable Path Execution | Debugging/introspection |
| **FLOW** | Flexible Live Observable Workflow | Live debugging |

**Pros:** Acronym is memorable, describes contribution
**Cons:** Can feel contrived

---

## Recommendation

### Primary: **MercuryLive** or **Mercury Live**

- Maintains brand recognition from CHI 2018
- "Live" captures the key new contributions:
  - **Live** visual authoring (Visual Composer)
  - **Live** message introspection (Time-travel debugging)
  - **Live** runtime manipulation
- Clean, professional, not contrived
- Easy subtitle: *"MercuryLive: Live Visual Authoring and Introspection for Hierarchical Message Systems"*

### Alternative: **PRISM**

If complete separation from original work is desired:
- Captures three major new pillars: **P**arallel processing, **S**patial indexing, and visual **I**ntrospection
- Fresh identity for new research direction
- Subtitle: *"PRISM: A Visual Development Environment for Hierarchical Message Systems"*

---

## Decision Factors

Consider which aspects to emphasize:

1. **Continuity with CHI 2018** → Mercury-based names
2. **Visual tooling focus** → MercuryVis, HIVE, MercuryGraph
3. **Live debugging focus** → MercuryLive, SCOPE, FLOW
4. **Spatial/parallel focus** → PRISM
5. **Complete fresh start** → Non-Mercury names (PRISM, Hermes, Iris)

---

## Active Task Priority Analysis

### High Priority Tasks

| Task | Priority | Effort | Description |
|------|----------|--------|-------------|
| **visual-composer** | HIGH (UIST Contribution I) | ~316h | Live visual authoring & message flow debugging |
| **digital-twin-layer** | HIGH | ~250h | MQTT/Mercury bridge for IoT/DT systems |
| **tutorial-validation** | ESSENTIAL | 8-12h | Validate 12 tutorials against wiki docs |

---

## Related Task Clusters

### Cluster 1: Debugging Tools (Strongly Related)

- `visual-composer` ↔ `time-travel-debugging`
- Both provide message introspection/debugging
- Time-travel explicitly mentions "Integration with Visual Composer (future)"
- **Recommendation:** Implement together or in sequence

### Cluster 2: Spatial-Dependent Systems (Shared Dependency)

- `spatial-indexing` → `digital-twin-layer` (optional dependency)
- `spatial-indexing` → `xr-collaboration` (optional dependency)
- Both DT and XR explicitly list spatial-indexing as optional
- **Recommendation:** Build spatial-indexing first if pursuing DT or XR

### Cluster 3: Parallel Processing (Related Infrastructure)

- `parallel-dispatch` ↔ `parallel-fsm`
- Both deal with concurrent execution
- Parallel FSM needs thread-safe message passing
- **Recommendation:** Parallel-dispatch provides foundation for parallel-fsm

### Cluster 4: User Evaluation (Sequential Dependency)

- `tutorial-validation` → `user-study`
- User study needs working tutorials
- **Recommendation:** Complete tutorial-validation first (only 8-12h)

### Cluster 5: Research Contributions (UIST)

- `visual-composer` (UIST I) ↔ `static-analysis` (UIST III)
- Both provide safety/debugging for Mercury
- Could share hierarchy graph infrastructure

---

## Priority + Effort Quadrant

```
                    HIGH PRIORITY
                         ↑
    visual-composer   |   tutorial-validation
       (~316h)        |       (8-12h)
    digital-twin      |
       (~250h)        |
 ←─────────────────────┼──────────────────────→
  HIGH EFFORT         |            LOW EFFORT
                      |
    spatial-indexing  |   time-travel-debugging
       (360h)         |       (~150h)
    parallel-dispatch |
       (360h)         |
                         ↓
                   MEDIUM PRIORITY
```

---

## Recommended Implementation Sequence

1. **tutorial-validation** (8-12h) - Essential, quick win, enables user study
2. **visual-composer** (316h) - HIGH priority UIST contribution
3. **time-travel-debugging** (150h) - Complements visual-composer
4. **spatial-indexing** (360h) - Enables DT and XR features
5. **digital-twin-layer** (250h) - HIGH priority, needs spatial-indexing

The **visual-composer + time-travel-debugging** cluster is the highest-impact combination for improving developer experience and generating publishable research contributions.

---

*Created: 2025-12-17*
