# VR Visual Composer Design

**Date:** 2026-02-19
**Status:** Approved (Rev 2)
**Timeline:** 9-12 months (staged: UIST prototype first, then production)
**Approach:** Introspection-First (Approach A) with rendering ladder (2D → 3D → VR)

---

## Problem Statement

MercuryMessaging networks are invisible at runtime. When a message doesn't reach its target, developers have no way to see why it was filtered, which path it took, what the responder can actually handle, or how the network topology connects. Existing debugging is limited to Inspector-visible string buffers (`messageInList`/`messageOutList`) on MmRelayNode. No tool explains *why* a message was blocked or whether a responder even handles that message type.

The goal is to allow someone to **view, debug, and change Mercury networks with actual logic links immersively in VR**.

## Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Primary audience | UIST 2026 paper first, then production tool | Staged approach: research contributions validated via user study, then iterate toward production |
| VR hardware | Quest 3 standalone + PC VR (Quest Link) | Standalone for user studies, PC VR for development workflow |
| Interaction model | Full bi-directional editing | Create/delete nodes, wire connections, change filters in VR, reflected in Unity scene |
| Spatial representation | Hybrid: scene-anchored overlays + detachable overview graph | Overlays preserve spatial context; overview graph for full topology at a glance |
| Build order | Rendering ladder: 2D → 3D Scene View → VR | Each rung validates the previous and delivers standalone value |
| Timeline | 9-12 months | Full vision: editing, time-travel, blockage indicators, user study |
| Storage | In-memory ring buffers (hot path) + optional SQLite/JSON export (persistence) | Ring buffers for real-time; persistence layer for user studies and session sharing |

## Architecture: Rendering Ladder

```
Layer 0: Introspection Subsystem (framework-level, zero XR dependency)
         6 event streams + responder capability reflection
              ↓ data flows down
Layer 1: 2D Graph Editor (GraphViewBase/GTK, flat-screen editor window)
         Validates data model, editing model, bi-directional sync
              ↓ same data model
Layer 2: 3D Scene View Visualization (GL/ALINE in Unity editor Scene View)
         Lines between GameObjects showing connections + message flow
              ↓ port to VR
Layer 3: VR Rendering (XRI, scene-anchored overlays + detachable overview)
         Message particles, blockage cards, node overlays
              ↓ add interaction
Layer 4: VR Interaction & Editing (tool belt, bi-directional scene edits)
         Create/delete/wire/edit nodes with hands
              ↓ add time dimension
Layer 5: Time-Travel & Timeline (scrubber, replay, historical state)
```

Each layer delivers standalone value. The 2D editor is useful without VR. The 3D Scene View is useful without a headset. VR adds immersion. Each layer validates the one below it.

### Layer 0: Introspection Subsystem

Framework-level foundation. Records *what happened*, *why*, and *what each component can do*. Zero VR/XR dependencies.

**6 Event Streams:**

| Stream | What it captures | Hook point in MmRelayNode | Used by |
|--------|-----------------|--------------------------|---------|
| 1. Message Routing | Full per-responder filter audit trail (which filters passed AND failed) | `ResponderCheck()` line 1540 | Message particles, blockage cards, test message traces, timeline |
| 2. Topology Changes | Node/responder added/removed, parent/child changes | `MmAddToRoutingTable()`, `UnRegisterResponder()`, `AddParent()` | Overview graph live updates, 2D editor sync |
| 3. FSM Transitions | State enter/exit on MmRelaySwitchNode | `FiniteStateMachine.GlobalEnter` event | State cluster highlighting, timeline bands |
| 4. Network Events | Serialization/deserialization, network filter decisions | Network responder path (line 872-888) | Network edge styling, bandwidth badges |
| 5. Per-Node Metrics | Messages/sec, rejection rate, top rejection reason (rolling counters) | Incremented in `MmInvoke` | Node color coding, edge thickness, heatmap |
| 6. Responder Capabilities | Which methods each responder handles (reflection, cached on registration) | `RegisterAwakenedResponder()`, `MmRefreshResponders()` | "Logic links" display, capability mismatch warnings |

**Stream 6 (Responder Capabilities) — "actual logic links":**

```csharp
public struct MmResponderCapability
{
    public string ResponderName;
    public string ResponderType;              // MmBaseResponder, MmExtendableResponder, etc.
    public MmMethod[] HandledMethods;         // which ReceivedMessage overrides exist
    public MmMethod[] RegisteredCustomHandlers; // MmExtendableResponder registrations
    public bool HasListeners;                 // DSL listeners subscribed?
    public MmTag Tag;
    public bool TagCheckEnabled;
}
```

Gathered via reflection once per responder (on registration, not per-message). Enables:
- "This responder handles MessageString and MessageInt, but NOT MessageFloat"
- Visual distinction in the graph: solid edge = responder handles this type, dashed = structural connection only
- Capability mismatch warnings: "You're sending MessageFloat to children, but none of them handle it"

**Unified Event Bus:**

```csharp
public static class MmIntrospectionBus
{
    public static event Action<MmRoutingEvent> OnRouting;
    public static event Action<MmTopologyEvent> OnTopologyChange;
    public static event Action<MmFsmEvent> OnFsmTransition;
    public static event Action<MmNetworkEvent> OnNetworkEvent;
    public static MmNodeMetrics GetMetrics(MmRelayNode node);
    public static MmResponderCapability GetCapabilities(IMmResponder responder);
}
```

### Layer 1: 2D Graph Editor (flat-screen, editor window)

**Purpose:** Validate the introspection data model, bi-directional editing model, and graph layout before going to 3D/VR. Ships standalone value — useful for any MercuryMessaging developer without VR hardware.

**Technology:** GraphViewBase (already installed, MIT) or Unity Graph Toolkit (experimental, Unity 6.2+). Decision: start with GraphViewBase, migrate to GTK when stable.

**Features:**
- Node-link graph mirroring the Mercury hierarchy
- Live message flow animation (colored pulses along edges)
- Blockage indicators on edges (red X with tooltip)
- Responder capability badges (which methods each node handles)
- Bi-directional editing: drag to rearrange, right-click to add/delete nodes, drag to wire
- Graph changes update Unity scene; scene changes update graph
- Search/filter: "show only Tag0 nodes", "highlight path from A to B"
- Selection syncs with Unity Hierarchy and Inspector

**Reference implementations to study:**
- Unity Shader Graph source (viewable in Library/PackageCache/)
- NodeGraphProcessor (MIT, github.com/alelievr/NodeGraphProcessor)
- Unity C# Reference GraphView.cs

### Layer 2: 3D Scene View Visualization

**Purpose:** Render Mercury connections directly in Unity's Scene View as 3D lines overlaid on the actual GameObjects. Bridge between 2D graph and VR.

**Technology:** ALINE (already in project as plugin, paid but installed) for line rendering. Falls back to GL.Begin/GL.End if ALINE not available.

**Features:**
- Colored bezier curves between connected MmRelayNodes in Scene View
- Color coding: blue = parent→child, green = child→parent, white = bidirectional
- Thickness encodes message volume
- Animated particles along edges during Play Mode (live messages)
- Node labels floating above GameObjects
- Blockage indicators: red X markers on edges where messages are filtered
- Responder capability icons on nodes (what methods they handle)
- Toggle visibility via Scene View overlay menu
- Selection in Scene View syncs with 2D graph editor

**Reference implementations:**
- ALINE drawing API (scope-based: `using (Draw.InLocalSpace(transform)) { ... }`)
- A* Pathfinding Project (uses ALINE for path visualization — similar pattern)

### Layer 3: VR Rendering

Port the 3D Scene View visualization into VR with XRI interaction.

**Two synchronized modes:**

**Mode 1: Scene-Anchored View (default)**
- `MmVRNodeOverlay`: floating label + color-coded ring (green/orange/red) + capability badges
- `MmVREdgeRenderer`: 3D bezier curves. Color/thickness/animation same as Scene View
- Blockage cards: floating explanation cards near rejection points
- Capability cards: "This responder handles: MessageString, MessageInt" (from Stream 6)
- Message particles: color-coded by type, pooled (200), shatter on rejection

**Mode 2: Overview Graph (detachable)**
- ~0.5m floating 3D graph, hierarchical layout
- Simplified spheres + thin lines, same color coding
- Grabbable/rotatable, pinch node to teleport
- Search/filter: voice or hand menu to filter by tag, type, capability

**Performance budget:** 72 FPS on Quest 3, up to 50 visible nodes.

### Layer 4: VR Interaction & Editing

Tool belt (wrist-mounted radial menu):

| Tool | Function |
|------|----------|
| Inspect | Select nodes, view properties, capabilities, read blockage cards |
| Wire | Draw connections between nodes (with filter type popup) |
| Edit | Modify filters, tags, active state, FSM state |
| Create | Spawn new relay nodes, switch nodes, responders |
| Delete | Remove nodes/connections with confirmation |
| Timeline | Time-travel scrubber |
| Test Message | Fire a test message and watch it propagate in slow motion with per-filter audit |
| Search | Find nodes by name, tag, type, capability |

### Layer 5: Time-Travel & Timeline

Timeline scrubber using MmMessageRecorder data. Replay historical state with blockage cards, message particles, FSM transitions, and topology changes.

## Novel Research Contributions

1. **Blockage indicators** (strongest claim): First tool to explain WHY messages were filtered, with spatial causal visualization
2. **Responder capability introspection**: First tool to show what each component *can handle* vs what it's being sent
3. **Rendering ladder** (2D → 3D → VR): Progressive visualization approach with standalone value at each level
4. **Immersive message-flow debugging**: First VR-native message routing debugger for a game engine framework
5. **Bi-directional VR graph editing**: Edit message network topology in VR with changes reflected in Unity scene
6. **Message-centric time-travel**: Scrub through message history (not code history) with full filter audit replay

## Key Tools & References

### Data Layer & Message Visualization Inspiration

| Tool | What to study | Link | Open Source |
|------|--------------|------|-------------|
| Foxglove Studio | Topic graph panel, message stream, timeline scrubber — best reference for message flow viz | https://foxglove.dev/ | Yes |
| Rerun | Time-aware ECS data model, SDK-based logging, multi-rate data sync | https://rerun.io/ | Yes (MIT) |
| AGDebugger (CHI 2025) | Interactive message stepping and agent steering | https://dl.acm.org/doi/full/10.1145/3706598.3713581 | Research |
| UE5 Rewind Debugger | Best reference for time-travel debugging UX in a game engine | UE5 docs | No |
| Replay.io | Time-travel debugging UX for web — excellent scrubber design | https://replay.io/ | Partial |

### 2D Graph Editor Inspiration

| Tool | What to study | Link | Open Source |
|------|--------------|------|-------------|
| Unity Shader Graph | How Unity builds node editors — source viewable in PackageCache | Library/PackageCache/com.unity.shadergraph/ | Viewable |
| Unity Graph Toolkit (GTK) | Official successor to GraphView (experimental, Unity 6.2+) | Unity Discussions | No (Unity) |
| GraphViewBase | MIT graph framework, already installed in project | https://github.com/Gentlymad-Studios/GraphViewBase | Yes (MIT) |
| NodeGraphProcessor | Mature open-source graph editor with runtime + data processing | https://github.com/alelievr/NodeGraphProcessor | Yes (MIT) |
| Unity C# Reference | GraphView.cs source shows how the API works internally | https://github.com/Unity-Technologies/UnityCsReference | Viewable |

### 3D Visualization & Line Drawing

| Tool | What to study | Link | Open Source |
|------|--------------|------|-------------|
| ALINE | Scope-based 3D line/shape drawing, works in editor + runtime + Burst jobs | https://arongranberg.com/aline/ | No (paid, already in project) |
| Shapes | Anti-aliased vector graphics for Unity, works in editor + runtime | https://acegikmo.com/shapes | No (paid) |
| Speja | Burst-compiled force-directed graph layout (Fruchterman-Reingold) | https://github.com/StefanTerdell/Speja | Yes |
| ForceDirectedNodeGraph3DUnity | 3D force-directed graph with physics engine, 100-300 nodes | https://github.com/Bamfax/ForceDirectedNodeGraph3DUnity | Yes |
| Popcron/Gizmos | Runtime gizmo drawing in builds (not just editor) | https://github.com/popcron/gizmos | Yes |

### VR/Immersive Debugging

| Tool | What to study | Link | Open Source |
|------|--------------|------|-------------|
| Meta Immersive Debugger | In-headset hierarchy inspection, property editing, console viewer | https://developers.meta.com/horizon/documentation/unity/immersivedebugger-overview/ | No |
| ExplorViz | Live software trace visualization in VR, city metaphor, WebXR multi-user | https://explorviz.dev/ | Yes |
| IATK | GPU-accelerated immersive data viz, scales to millions of points, VR+AR | https://github.com/MaximeCordeil/IATK | Yes |
| DXR | Declarative immersive data viz (Vega-Lite inspired), AR/MR/VR | https://github.com/ronellsicat/DxR | Yes |
| SecCityVR (EASE 2025) | VR code city for security visualization — positive usability results | https://arxiv.org/html/2504.18238v1 | Research |

### Academic Gap (our opportunity)

| Venue | Finding |
|-------|---------|
| CHI 2025 | AGDebugger does message stepping for LLM agents, but not spatial/VR |
| UIST 2025 | No immersive debugging papers |
| IEEE VR 2025 | No software debugging papers |
| EASE 2025 | SecCityVR proves VR+debugging is viable, but city metaphor not message-flow |

**No tool anywhere explains WHY messages were filtered. No VR-native graph editor exists for Unity. No immersive message-flow debugger exists for any game engine.**

## Phased Delivery (Revised)

| Phase | Duration | Delivers | Standalone Value |
|-------|----------|----------|-----------------|
| 7A: Introspection Subsystem | 8 weeks | 6 event streams, MmIntrospectionBus, MmMessageRecorder, MmTopologySnapshot, responder capability reflection | Framework-level debugging data, usable from code/tests |
| 7B: 2D Graph Editor | 8 weeks | GraphViewBase editor window, node-link graph, live animation, blockage indicators, capability badges, bi-directional editing, search/filter | Full debugging tool for anyone without VR |
| 7C: 3D Scene View | 4-6 weeks | GL/ALINE connection lines in Scene View, message particles, blockage markers, node labels | Visual debugging overlay in Unity editor |
| 7D: VR Rendering | 6-8 weeks | Scene-anchored overlays, overview graph, VR blockage cards, message particles | Immersive visualization (view-only) |
| 7E: VR Interaction & Editing | 6-8 weeks | Tool belt, wire/create/delete, test message, undo system | Full immersive authoring |
| 7F: Time-Travel & Timeline | 4-6 weeks | Scrubber, playback, historical state replay, session export | Debugging across time |
| 7G: Polish & User Study | 6-8 weeks | Performance optimization, user study (N=20), paper figures | UIST submission |

Total: ~42-52 weeks (10-13 months)

## Identified Gaps (Opportunities)

- No tool explains WHY messages were filtered (blockage indicators)
- No tool shows what each responder can actually handle (capability introspection)
- No VR-native node graph editor exists for Unity
- No immersive debugging papers at CHI/UIST/IEEE VR 2025
- Unity has no built-in time-travel debugging (unlike UE5 Rewind Debugger)
- No progressive 2D→3D→VR visualization approach in the literature

---

*Design approved: 2026-02-19 (Rev 2 — expanded introspection, rendering ladder, revised tools)*
