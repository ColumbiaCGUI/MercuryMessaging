# Visual Network Composer - Task Checklist

**Last Updated:** 2025-12-01
**Status:** Ready to Start
**Total Effort:** 316 hours (8 weeks)

---

## Technology Stack (Updated 2025-12-01)

- **2D Graph Editor:** GraphViewBase (MIT, already imported)
- **3D Scene Visualization:** Custom GL-based solution (ALINE-inspired patterns)
- **Runtime Debugger:** Custom Mercury Inspector (lightweight, focused)

---

## Task Organization

Tasks are organized by tool in chronological order. Each task includes effort estimate, acceptance criteria, and dependencies.

**Progress Tracking:**
- [ ] Not started
- [🔨] In progress
- [✅] Complete

---

## Testing Standards

All tests for this project MUST follow these patterns:

### Required Approach
- ✅ Use **Unity Test Framework** (PlayMode or EditMode)
- ✅ Create **GameObjects programmatically** in `[SetUp]` methods
- ✅ All components added via `GameObject.AddComponent<T>()`
- ✅ Clean up in `[TearDown]` with `Object.DestroyImmediate()`

### Prohibited Patterns
- ❌ NO manual scene creation or loading
- ❌ NO manual UI element prefabs
- ❌ NO prefab dependencies from Resources folder

### Example Test Pattern
```csharp
[Test]
public void TestHierarchyMirroring()
{
    // Arrange - create hierarchy programmatically
    GameObject root = new GameObject("Root");
    GameObject child = new GameObject("Child");
    child.transform.SetParent(root.transform);

    // Act - apply hierarchy mirroring
    MmHierarchyMirror.MirrorHierarchy(root);

    // Assert - verify MmRelayNodes created
    Assert.IsNotNull(root.GetComponent<MmRelayNode>());
    Assert.IsNotNull(child.GetComponent<MmRelayNode>());

    // Cleanup
    Object.DestroyImmediate(root);
}
```

---

## Tool 1: Hierarchy Mirroring (36 hours)

### Task 1.1: Design UI Architecture (8h)
- [ ] Define EditorWindow layout and sections
- [ ] Design preview panel with hierarchy visualization
- [ ] Create configuration panel mockups
- [ ] Design rule-based filtering system
- [ ] Plan undo/redo integration

**Acceptance:** UI design document approved, mockups ready

**Dependencies:** None

---

### Task 1.2: Implement Hierarchy Traversal (16h)
- [ ] Create MmHierarchyMirror EditorWindow class
- [ ] Implement recursive GameObject traversal
- [ ] Add MmRelayNode component creation logic
- [ ] Implement parent-child connection logic
- [ ] Add MmAddToRoutingTable calls based on hierarchy
- [ ] Handle inactive GameObject filtering
- [ ] Add dry-run preview mode

**Acceptance:** Traversal correctly maps Unity hierarchy to Mercury network

**Dependencies:** Task 1.1

**Code Location:** `Assets/MercuryMessaging/Support/Editor/`

---

### Task 1.3: Add Configuration Options (12h)
- [ ] Create configuration UI (tags, filters, naming)
- [ ] Implement MirrorRule system for selective inclusion
- [ ] Add regex-based GameObject name filtering
- [ ] Implement tag override rules
- [ ] Add level filter override rules
- [ ] Create configuration save/load system
- [ ] Add validation for configuration

**Acceptance:** Users can configure mirroring behavior through UI

**Dependencies:** Task 1.2

---

## Tool 2: Network Templates (52 hours)

### Task 2.1: Create Template Base Class (12h)
- [ ] Design MmNetworkTemplate abstract base class
- [ ] Define template interface (CreateNetwork, parameters)
- [ ] Implement template registration system
- [ ] Create MmTemplateLibrary manager class
- [ ] Add template icon/thumbnail support
- [ ] Implement DrawConfiguration() for custom params

**Acceptance:** Template base class functional, extensible

**Dependencies:** None

**Code Location:** `Assets/MercuryMessaging/Support/Templates/`

---

### Task 2.2: Implement Core Templates (20h)
- [ ] **Hub-and-Spoke** template (4h)
  - Configurable spoke count
  - Circular positioning
  - Hub-to-spoke connections
- [ ] **Chain** template (4h)
  - Configurable chain length
  - Bidirectional option
  - Linear positioning
- [ ] **Broadcast Tree** template (4h)
  - Configurable depth and branch factor
  - Hierarchical positioning
  - Top-down connections
- [ ] **Event Aggregator** template (4h)
  - Many-to-one pattern
  - Configurable source count
  - Fan-in positioning
- [ ] **Peer Network (Mesh)** template (4h)
  - Fully connected peers
  - Configurable peer count
  - Circular positioning with cross-connections

**Acceptance:** All 5 templates create functional networks

**Dependencies:** Task 2.1

---

### Task 2.3: Add Template UI (20h)
- [ ] Create template selection window
- [ ] Implement template preview/thumbnails
- [ ] Add parameter configuration UI per template
- [ ] Implement template instantiation in scene
- [ ] Add template search/filter
- [ ] Create template documentation viewer
- [ ] Add template export/import

**Acceptance:** Users can browse, configure, and instantiate templates

**Dependencies:** Task 2.2

---

## Tool 3: Visual Network Composer (80 hours)

### Task 3.1: Study GraphViewBase Architecture (8h)
- [ ] Read GraphViewBase source code in `Library/PackageCache/`
- [ ] Understand `GraphView`, `BaseNode`, `BasePort`, `BaseEdge` classes
- [ ] Study action callback pattern (`OnActionExecuted`)
- [ ] Understand port connection validation (`CanConnectTo`)
- [ ] Review drag-and-drop event system
- [ ] Document key extension points

**Acceptance:** Understanding documented, ready to implement

**Dependencies:** None

**Key Files to Read:**
- `Library/PackageCache/com.gentlymad.graphviewbase@.../Editor/Elements/GraphView.cs`
- `Library/PackageCache/com.gentlymad.graphviewbase@.../Editor/Elements/Graph/BaseNode.cs`
- `Library/PackageCache/com.gentlymad.graphviewbase@.../Editor/Elements/Graph/BasePort.cs`

---

### Task 3.2: Design MmNetworkComposer Architecture (8h)
- [ ] Design MmNetworkComposer EditorWindow structure
- [ ] Design MmGraphView class extending GraphViewBase's GraphView
- [ ] Design MmNodeView extending BaseNode
- [ ] Design MmEdgeView extending BaseEdge
- [ ] Plan data model (MmNetworkGraph)
- [ ] Design serialization format
- [ ] Plan bidirectional sync with scene

**Acceptance:** Architecture document complete, classes outlined

**Dependencies:** Task 3.1

**References:** GraphViewBase samples, existing MmRelayNode structure

---

### Task 3.3: Implement Graph Editor Core (40h)
- [ ] Create MmNetworkComposer EditorWindow (8h)
  - Toolbar (New, Save, Load, Export, Validate)
  - GraphViewBase container
  - Inspector panel
  - Minimap panel
- [ ] Implement MmGraphView extending GraphViewBase's GraphView (16h)
  - Override `OnActionExecuted` for node/edge events
  - Node creation via drag-drop
  - Edge creation via port connections
  - Node selection and multi-select
  - Graph navigation (pan, zoom)
- [ ] Create MmNodeView extending BaseNode (8h)
  - Override port creation with Mercury-specific ports
  - Input/output ports for routing directions
  - Property display (Name, Tag, Filters)
  - Visual styling with Mercury colors
  - Tag/filter indicators
- [ ] Implement MmNetworkGraph data model (8h)
  - Node storage
  - Connection storage
  - Serialization (JSON)
  - Deserialization
  - Scene sync events

**Acceptance:** Graph editor functional with node/edge creation

**Dependencies:** Task 3.2

**Code Location:** `Assets/MercuryMessaging/Support/Editor/NetworkComposer/`

---

### Task 3.4: Add Node Configuration (8h)
- [ ] Create node inspector panel
- [ ] Add property editing (name, tag, filters)
- [ ] Implement real-time node updates
- [ ] Add custom property drawers
- [ ] Implement node search/filter in inspector

**Acceptance:** Users can configure all node properties via inspector

**Dependencies:** Task 3.3

---

### Task 3.5: Implement Bidirectional Sync (8h)
- [ ] Implement scene → graph sync (8h)
  - Detect MmRelayNode changes in scene
  - Update corresponding nodes in graph
  - Handle node creation/deletion in scene
- [ ] Implement graph → scene sync (included above)
  - Create GameObjects from nodes
  - Add MmRelayNode components
  - Set up routing table connections

**Acceptance:** Changes in graph reflect in scene and vice versa

**Dependencies:** Task 3.4

---

### Task 3.6: Integration Testing (8h)
- [ ] Test with simple networks (hub-spoke, chain)
- [ ] Test with complex networks (100+ nodes)
- [ ] Test bidirectional sync workflows
- [ ] Test undo/redo functionality
- [ ] Performance testing with large graphs
- [ ] Bug fixing and polish

**Acceptance:** Composer works reliably for all use cases

**Dependencies:** Task 3.5

---

## Tool 4: Network Validator (48 hours)

### Task 4.1: Create Validator Core (20h)
- [ ] Create MmNetworkValidator class (8h)
  - Validation API design
  - ValidationResult structure
  - Error/warning/info categorization
- [ ] Implement circular dependency detection (8h)
  - Depth-first search algorithm
  - Cycle path extraction
  - Suggestion generation
- [ ] Implement reachability analysis (4h)
  - Find unreachable nodes (no incoming)
  - Find isolated nodes (no outgoing)
  - Suggest fixes

**Acceptance:** Core validation algorithms functional

**Dependencies:** None

**Code Location:** `Assets/MercuryMessaging/Support/Validation/`

---

### Task 4.2: Implement Validation Rules (16h)
- [ ] Performance estimation (6h)
  - Estimate routing latency
  - Calculate max depth
  - Identify bottleneck nodes
- [ ] Best practices checker (6h)
  - Too many connections per node
  - Deep hierarchy warnings
  - Inefficient filter usage
  - Tag conflicts
- [ ] Configuration validation (4h)
  - Missing required components
  - Invalid filter combinations
  - Network type mismatches

**Acceptance:** All validation rules detect issues correctly

**Dependencies:** Task 4.1

---

### Task 4.3: Create Validation UI (12h)
- [ ] Design validation report window (4h)
  - Error/warning/info sections
  - Expandable details
  - Navigation to problem nodes
- [ ] Implement real-time validation in composer (4h)
  - Visual indicators on nodes
  - Inline error messages
  - Automatic validation triggers
- [ ] Add validation toolbar in composer (2h)
  - Validate button
  - Status indicator
  - Quick-fix suggestions
- [ ] Create validation report export (2h)
  - Export to text file
  - Export to HTML

**Acceptance:** Validation results clearly displayed with actionable feedback

**Dependencies:** Task 4.2

---

## Tool 5: 3D Scene Visualization (40 hours) - NEW

### Task 5.1: Study ALINE Patterns (4h)
- [ ] Read `Assets/Plugins/Plugins/ALINE/Draw.cs` - scope-based API
- [ ] Read `Assets/Plugins/Plugins/ALINE/CommandBuilder.cs` - batching
- [ ] Understand camera injection pattern (`CameraEvent`, `CommandBuffer`)
- [ ] Document patterns to replicate

**Acceptance:** Understanding documented, ready to implement custom solution

**Dependencies:** None

**Key Files to Study:**
- `Assets/Plugins/Plugins/ALINE/Draw.cs`
- `Assets/Plugins/Plugins/ALINE/CommandBuilder.cs`
- `Assets/Plugins/Plugins/ALINE/DrawingManager.cs`

---

### Task 5.2: Implement MmConnectionDrawer (16h)
- [ ] Create `MmConnectionDrawer` component
- [ ] Implement GL-based line drawing for connections
- [ ] Add URP/HDRP support via `RenderPipelineManager.endCameraRendering`
- [ ] Batch all lines into single draw call
- [ ] Implement scope-based API inspired by ALINE:
  ```csharp
  using (MmDraw.WithColor(Color.cyan))
  {
      MmDraw.Line(nodeA.position, nodeB.position);
  }
  ```

**Acceptance:** Connection lines visible in Scene view

**Dependencies:** Task 5.1

**Code Location:** `Assets/MercuryMessaging/Support/Editor/SceneVisualization/`

---

### Task 5.3: Add Visual Enhancements (12h)
- [ ] Color-code connections by message type/direction (4h)
  - Blue: Child direction
  - Green: Parent direction
  - Yellow: Bidirectional
- [ ] Implement message pulse animation (4h)
  - Animated "packet" along connection line
  - Triggered when message is sent
- [ ] Add hierarchy depth visualization (4h)
  - Vary line thickness by depth
  - Add depth labels

**Acceptance:** Connections visually distinguish message types

**Dependencies:** Task 5.2

---

### Task 5.4: Scene Overlay Integration (8h)
- [ ] Create SceneView overlay toggle
- [ ] Add settings panel for visualization options
- [ ] Implement frustum culling for performance
- [ ] Add legend/key for color meanings
- [ ] Test performance with 100+ nodes

**Acceptance:** Visualization toggleable, performant at scale

**Dependencies:** Task 5.3

---

## Tool 6: Runtime Debugger (60 hours) - NEW

### Task 6.1: Study EPO Outline Patterns (4h)
- [ ] Read `Assets/Plugins/Plugins/EasyPerformantOutline/Scripts/Outlinable.cs`
- [ ] Understand component marking pattern
- [ ] Study static registry pattern (`HashSet<Outlinable>`)
- [ ] Document patterns for `MmHighlightable` component

**Acceptance:** Understanding documented, ready to implement

**Dependencies:** None

**Key Files to Study:**
- `Assets/Plugins/Plugins/EasyPerformantOutline/Scripts/Outlinable.cs`
- `Assets/Plugins/Plugins/EasyPerformantOutline/Scripts/Outliner.cs`

---

### Task 6.2: Create MmRuntimeDebugger Component (20h)
- [ ] Create `MmRuntimeDebugger` MonoBehaviour
- [ ] Implement IMGUI-based debug panel (12h)
  - Collapsible sections
  - Scroll views for message list
  - Filter dropdowns
- [ ] Hook into `MmRelayNode.MmInvoke` for message capture (4h)
- [ ] Add enable/disable toggle (keyboard shortcut) (4h)

**Acceptance:** Debug panel shows when enabled during Play mode

**Dependencies:** Task 6.1

**Code Location:** `Assets/MercuryMessaging/Support/RuntimeDebugger/`

---

### Task 6.3: Implement Message Stream View (16h)
- [ ] Create scrolling message log (8h)
  - Timestamp
  - Source node
  - Target node(s)
  - Method type
  - Payload summary
- [ ] Add message filtering (4h)
  - By MmMethod type
  - By tag
  - By level filter
- [ ] Implement pause/resume message capture (2h)
- [ ] Add clear button (2h)

**Acceptance:** Users can see and filter live message stream

**Dependencies:** Task 6.2

---

### Task 6.4: Implement Node Hierarchy Browser (12h)
- [ ] Create collapsible tree view of MmRelayNodes (6h)
- [ ] Show message counts per node (2h)
- [ ] Click to select node in scene (2h)
- [ ] Show routing table for selected node (2h)

**Acceptance:** Users can browse node hierarchy at runtime

**Dependencies:** Task 6.2

---

### Task 6.5: Export and Testing (8h)
- [ ] Implement message trace export (JSON/CSV) (4h)
- [ ] Test with various network configurations (2h)
- [ ] Performance testing during high message volume (2h)

**Acceptance:** Debugger works reliably without impacting performance

**Dependencies:** Tasks 6.3, 6.4

---

## Integration & Documentation (16 hours)

### Task 7.1: Tool Integration (8h)
- [ ] Integrate hierarchy mirroring with templates
- [ ] Integrate templates with visual composer
- [ ] Add validation to all tools
- [ ] Create unified menu system (Mercury → ...)
- [ ] Integrate scene visualization with composer
- [ ] Link runtime debugger to scene selection

**Acceptance:** All tools work together seamlessly

**Dependencies:** All previous tasks

---

### Task 7.2: Documentation (8h)
- [ ] Write tool documentation (4h)
  - Hierarchy Mirroring guide
  - Template usage guide
  - Visual Composer manual (GraphViewBase)
  - Scene Visualization guide
  - Runtime Debugger guide
  - Validator reference
- [ ] Create video tutorials (2h)
  - Screen recordings
  - Voiceover narration
- [ ] Write API reference (2h)
  - Template API
  - Validator API
  - MmDraw API

**Acceptance:** Complete documentation published

**Dependencies:** Task 7.1

---

## Summary

**Total Tasks:** 32
**Total Effort:** 316 hours (8 weeks)

**Phase Breakdown:**
- **Phase 1: Editor Tools (216h)**
  - Tool 1 (Hierarchy Mirroring): 36h
  - Tool 2 (Network Templates): 52h
  - Tool 3 (Visual Composer): 80h
  - Tool 4 (Network Validator): 48h
- **Phase 2: Scene Visualization (40h)**
  - Tool 5 (3D Scene Viz): 40h
- **Phase 3: Runtime Debugging (60h)**
  - Tool 6 (Runtime Debugger): 60h

**Critical Path:** Tool 3 (Visual Composer) must be done first as it establishes the node/edge data model

**Dependencies:** Phase 2-3 can start after Phase 1 core is complete

---

## Getting Started

**First 3 tasks to complete:**
1. Task 3.1: Study GraphViewBase Architecture (8h) - Required foundation
2. Task 5.1: Study ALINE Patterns (4h) - Can be parallel
3. Task 6.1: Study EPO Outline Patterns (4h) - Can be parallel

These study tasks establish the patterns for all custom implementations.

**Key Files to Read First:**
- `Library/PackageCache/com.gentlymad.graphviewbase@.../Editor/Elements/GraphView.cs`
- `Assets/Plugins/Plugins/ALINE/Draw.cs`
- `Assets/Plugins/Plugins/EasyPerformantOutline/Scripts/Outlinable.cs`

---

**Document Version:** 2.0
**Last Updated:** 2025-12-01
**Maintained By:** Developer Tools Team
**Major Changes:** Added Tools 5-6, updated to GraphViewBase, revised effort estimates
