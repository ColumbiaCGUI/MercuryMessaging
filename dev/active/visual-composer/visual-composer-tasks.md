# Visual Network Composer - Task Checklist

**Last Updated:** 2025-11-18
**Status:** Ready to Start
**Total Effort:** 212 hours (5-6 weeks)

---

## Task Organization

Tasks are organized by tool in chronological order. Each task includes effort estimate, acceptance criteria, and dependencies.

**Progress Tracking:**
- [ ] Not started
- [🔨] In progress
- [✅] Complete

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

## Tool 3: Visual Network Composer (96 hours)

### Task 3.1: Design GraphView Architecture (16h)
- [ ] Study Unity GraphView API
- [ ] Design MmNetworkComposer EditorWindow
- [ ] Design MmGraphView class structure
- [ ] Design NodeView (MmRelayNode visualization)
- [ ] Design EdgeView (connection visualization)
- [ ] Plan data model (MmNetworkGraph)
- [ ] Design serialization format

**Acceptance:** Architecture document complete, classes outlined

**Dependencies:** None

**References:** Unity Shader Graph, Visual Scripting source

---

### Task 3.2: Implement Graph Editor Core (40h)
- [ ] Create MmNetworkComposer EditorWindow (8h)
  - Toolbar (New, Save, Load, Export, Validate)
  - GraphView container
  - Inspector panel
  - Minimap panel
- [ ] Implement MmGraphView (16h)
  - Node creation via drag-drop
  - Edge creation via port connections
  - Node selection and multi-select
  - Graph navigation (pan, zoom)
  - Grid and snapping
- [ ] Create NodeView for MmRelayNode (8h)
  - Input/output ports
  - Property display
  - Visual styling
  - Tag/filter indicators
- [ ] Implement MmNetworkGraph data model (8h)
  - Node storage
  - Connection storage
  - Serialization (JSON)
  - Deserialization

**Acceptance:** Graph editor functional with node/edge creation

**Dependencies:** Task 3.1

**Code Location:** `Assets/MercuryMessaging/Support/Editor/NetworkComposer/`

---

### Task 3.3: Add Node Configuration (16h)
- [ ] Create node inspector panel
- [ ] Add property editing (name, tag, filters)
- [ ] Implement real-time node updates
- [ ] Add custom property drawers
- [ ] Implement node search/filter in inspector
- [ ] Add node documentation tooltips

**Acceptance:** Users can configure all node properties via inspector

**Dependencies:** Task 3.2

---

### Task 3.4: Implement Export Functionality (16h)
- [ ] Implement "Export to Scene" (8h)
  - Create GameObjects from nodes
  - Add MmRelayNode components
  - Set up routing table connections
  - Position objects in scene
  - Handle hierarchy parenting
- [ ] Implement "Save Network" (4h)
  - Serialize graph to JSON/ScriptableObject
  - Save to project assets
- [ ] Implement "Load Network" (4h)
  - Deserialize from JSON/ScriptableObject
  - Recreate graph in editor

**Acceptance:** Can export functional Mercury networks to scene

**Dependencies:** Task 3.3

---

### Task 3.5: Integration Testing (20h)
- [ ] Test with simple networks (hub-spoke, chain)
- [ ] Test with complex networks (100+ nodes)
- [ ] Test export and reimport workflows
- [ ] Test template integration
- [ ] Test undo/redo functionality
- [ ] Performance testing with large graphs
- [ ] UI/UX testing and refinement
- [ ] Bug fixing and polish

**Acceptance:** Composer works reliably for all use cases

**Dependencies:** Task 3.4

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

## Integration & Documentation (16 hours)

### Task 5.1: Tool Integration (8h)
- [ ] Integrate hierarchy mirroring with templates
- [ ] Integrate templates with visual composer
- [ ] Add validation to all tools
- [ ] Create unified menu system (Mercury → ...)
- [ ] Add cross-tool workflows

**Acceptance:** All tools work together seamlessly

**Dependencies:** All previous tasks

---

### Task 5.2: Documentation (8h)
- [ ] Write tool documentation (4h)
  - Hierarchy Mirroring guide
  - Template usage guide
  - Visual Composer manual
  - Validator reference
- [ ] Create video tutorials (2h)
  - Screen recordings
  - Voiceover narration
- [ ] Write API reference (2h)
  - Template API
  - Validator API

**Acceptance:** Complete documentation published

**Dependencies:** Task 5.1

---

## Summary

**Total Tasks:** 22
**Total Effort:** 212 hours (5-6 weeks)

**Phase Breakdown:**
- Tool 1 (Hierarchy Mirroring): 36h
- Tool 2 (Network Templates): 52h
- Tool 3 (Visual Composer): 96h
- Tool 4 (Network Validator): 48h
- Integration & Docs: 16h

**Critical Path:** Task 3 (Visual Composer) is the longest and most complex

**Dependencies:** Most tasks can be parallelized, but integration requires all tools complete

---

## Getting Started

**First 3 tasks to complete:**
1. Task 1.1: Design UI Architecture (8h)
2. Task 2.1: Create Template Base Class (12h)
3. Task 4.1: Create Validator Core (20h)

These tasks can be done in parallel and unlock the rest of the work.

---

**Document Version:** 1.0
**Maintained By:** Developer Tools Team
