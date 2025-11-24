# Live Visual Authoring and Introspection Environment

**Status:** Ready to Start
**Priority:** HIGH (UIST Major Contribution I)
**Estimated Effort:** ~360 hours (9 weeks)
**Research Impact:** Novel tool for programming interactive systems

---

## Research Contribution (UIST Major Contribution I)

### Problem Statement: The "Gulf of Evaluation"

The Mercury framework suffers from **"Invisible Logic"** — a critical usability gap where the flow of messages is obscured from developers. To understand what happens when `Relay.MmInvoke()` is called, developers must:

1. Check the Unity Inspector to see the Routing Table configuration
2. Inspect message metadata (Routing Block filters)
3. Determine runtime state of target objects (Active/Selected/Tagged)

This creates a **Gulf of Evaluation** where logic is distributed across data files (scenes/prefabs) rather than centralized in code. As noted in the UIST analysis: *"It can be hard to maintain a mental model of all of this... even with a UML diagram, we'd need to represent different types of propagation possibilities."*

### Novel Technical Approach

We propose a **Live Visual Authoring and Introspection Environment** — not merely a static visualizer, but a **bi-directional reflection of runtime state** integrated into the Unity Editor.

#### Core Innovations

1. **Bi-Directional Graph Editing**
   - Visual graph editor where nodes = MmRelayNodes, edges = Routing connections
   - Edit graph → updates scene objects (add/remove routing table entries)
   - Edit scene → updates graph (automatic synchronization)
   - Enables "code ↔ visual representation" duality

2. **Live Message Path Visualization**
   - Real-time highlighting as messages propagate through network
   - Color-coded edges based on filter criteria:
     - Blue: "Children Only"
     - Red: "Active Only"
     - Green: "Selected Only"
     - Yellow: "Tagged"
   - Animation shows message flow direction and timing

3. **Blockage Indicators**
   - When message stops propagating, visual indicator shows *why*
   - Examples:
     - "Rejected by Active Filter (GameObject inactive)"
     - "Rejected by Selected Filter (not current FSM state)"
     - "Rejected by Tag Filter (Tag mismatch: expected Tag0, found Tag1)"
   - Eliminates guesswork in debugging

4. **Runtime Topology Manipulation**
   - Drag connections between nodes **during gameplay** to rewire logic
   - Enables "live coding" of interaction topology
   - Changes persist to scene (save during play mode)
   - Supports rapid prototyping and experimentation

---

## Technical Architecture

### Graph Rendering System

```
Unity GraphView Framework
    ↓
Custom MmNodeView (renders Relay Nodes)
    ↓
Custom MmEdgeView (renders Routing connections)
    ↓
Bi-Directional Sync Layer
    ↓
Scene Manipulation (add/remove routing entries)
```

### Live Introspection Hook

```csharp
// Hook into MmRelayNode.MmInvoke
public class MmIntrospectionHook {
    public static event Action<MmRelayNode, MmMessage, List<MmRelayNode>> OnMessagePropagation;

    // Called after each routing decision
    public static void RecordPropagation(
        MmRelayNode source,
        MmMessage message,
        List<MmRelayNode> targets,
        List<MmRelayNode> rejected,
        Dictionary<MmRelayNode, string> rejectionReasons
    ) {
        // Visual graph consumes this to highlight paths
        OnMessagePropagation?.Invoke(source, message, targets);

        // Display blockage indicators for rejected nodes
        foreach (var rejected in rejected) {
            DisplayBlockageIndicator(rejected, rejectionReasons[rejected]);
        }
    }
}
```

### Runtime Manipulation

- Graph changes trigger scene modification via SerializedObject API
- Changes marked dirty, saved with scene
- Undo/Redo support through Unity's EditorUtility

---

## Evaluation Methodology

### User Study Design

**Hypothesis**: Live Visual Authoring significantly reduces debugging time and cognitive load compared to traditional Inspector-based workflow.

**Study Protocol**:
- **Participants**: N=20 developers (10 novice Mercury users, 10 experienced)
- **Task**: Fix 5 "broken" routing paths in increasingly complex networks
  - Condition A: Standard Unity Inspector only
  - Condition B: Live Visual Authoring Environment
- **Measures**:
  - **Time-to-fix** (primary): Duration from problem presentation to correct solution
  - **Error rate**: Number of incorrect fixes attempted
  - **Cognitive load**: NASA-TLX questionnaire (6 dimensions)
  - **Subjective preference**: Likert scale (1-7)

**Expected Results**:
- **50% reduction in debugging time** (Condition B vs A)
- **Lower cognitive load** scores (NASA-TLX: Mental Demand, Effort, Frustration)
- **Higher subjective preference** for visual environment

**Statistical Analysis**:
- Repeated measures ANOVA for time-to-fix
- Wilcoxon signed-rank test for NASA-TLX scores
- Alpha = 0.05, power = 0.8

---

## Research Impact

### Novelty
- **First bi-directional visual authoring tool** for hierarchical message-passing systems
- **First live introspection environment** for runtime message flow in game engines
- **Novel application of graph editing** to interactive system debugging

### Significance
- Solves the "Gulf of Evaluation" problem identified in Mercury analysis
- Enables non-programmers (designers, researchers) to construct message networks
- Reduces barrier to entry for Mercury adoption
- Applicable to other message-passing frameworks (ROS, Actor models)

### Broader Impact
- Influences future Unity editor tooling design
- Establishes pattern for "live coding" in compiled languages
- Provides template for other declarative UI systems

---

## Implementation Goals

### Phase 1: Core Graph Visualization (120h)
- Unity GraphView integration
- Render MmRelayNodes as nodes
- Render Routing Table entries as edges
- Basic zoom/pan/selection

### Phase 2: Bi-Directional Editing (80h)
- Drag-to-connect nodes creates routing entries
- Delete edge removes routing entry
- Sync graph ↔ scene in real-time
- Undo/Redo support

### Phase 3: Live Introspection (100h)
- Hook into MmInvoke call chain
- Animate message propagation
- Highlight active paths
- Display blockage indicators with reasons

### Phase 4: Runtime Manipulation (60h)
- Enable graph editing during Play Mode
- Apply changes immediately (hot-reload)
- Save changes back to scene
- Performance optimization

---

## Success Metrics

### Technical Metrics
- [ ] Graph renders 100+ node networks at 60 FPS
- [ ] Bi-directional sync latency < 100ms
- [ ] Live introspection overhead < 5% frame time
- [ ] Runtime manipulation applies changes < 50ms

### Usability Metrics (User Study)
- [ ] **50% reduction in debugging time** vs Inspector
- [ ] **30% reduction in NASA-TLX cognitive load**
- [ ] **>6/7 average subjective preference** rating
- [ ] **95%+ task completion rate** with visual environment

### Adoption Metrics
- [ ] Hierarchy mirror works for 100+ node scenes
- [ ] Templates create valid networks 100% of time
- [ ] Validator catches 95%+ of common mistakes
- [ ] Developer survey: 80%+ would recommend tool

---

## Comparison with Related Work

| Tool | Visualization | Editing | Live Introspection | Runtime Manipulation |
|------|---------------|---------|-------------------|---------------------|
| Unity Inspector | No | Manual | No | No |
| Visual Scripting (Bolt) | Static graph | Yes | No | No |
| Unreal Blueprints | Static graph | Yes | Limited | No |
| **Mercury Visual Composer** | **Dynamic graph** | **Yes** | **Yes** | **Yes** |

### Key Differentiators
- **Only tool** with live message path visualization
- **Only tool** enabling runtime topology changes
- **Only tool** with blockage indicator debugging
- **Integrated** with existing Mercury codebase (no migration needed)

---

## Deliverables

1. **Visual Authoring Tool**
   - Unity Editor Window with GraphView
   - Bi-directional sync system
   - Node/edge manipulation UI

2. **Live Introspection System**
   - Message flow animation
   - Blockage indicator overlay
   - Performance monitoring

3. **Runtime Manipulation**
   - Hot-reload connection changes
   - Play Mode editing support
   - Scene persistence

4. **User Study Materials**
   - Test scenarios (5 broken networks)
   - NASA-TLX questionnaire
   - Data collection scripts
   - Statistical analysis code

5. **Documentation**
   - User guide with examples
   - Video tutorials
   - API reference for extensibility

---

## Quick Start

See `USE_CASE.md` for business context, target scenarios, and expected benefits.
See `visual-composer-context.md` for detailed UI mockups and GraphView architecture.
See `visual-composer-tasks.md` for granular implementation tasks.

---

## Key Publications to Reference

- Norman, D. (1988). "The Design of Everyday Things" (Gulf of Evaluation)
- Shneiderman, B. (1983). "Direct Manipulation" (visual programming)
- Tanimoto, S. (1990). "VIVA: A visual language for image processing"
- Burnett, M. (1999). "Visual programming" (user studies)

---

**See full details in:**
- `USE_CASE.md` - Business context and use case analysis
- `visual-composer-context.md` - Technical architecture and UI mockups
- `visual-composer-tasks.md` - Implementation task breakdown

**UIST Contribution**: This work directly addresses Contribution I (Live Visual Authoring and Introspection Environment) from the UIST Research Contributions analysis, providing a comprehensive solution to the "Invisible Logic" problem that limits Mercury's usability and adoption.