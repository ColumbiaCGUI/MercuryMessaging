# Visual Network Composer - Technical Context

**Last Updated:** 2025-12-01
**Status:** Planning - Not Started
**Priority:** MEDIUM (Phase 4.2)

---

## Status

**Current State:** Not Started
**Blockers:** None - can begin anytime
**Dependencies:** None (standalone developer tool)
**Estimated Timeline:** 316 hours (8 weeks)

---

## Technology Stack Decision (2025-12-01)

### 2D Graph Editor: GraphViewBase (MIT)
- **Package:** `com.gentlymad.graphviewbase` (already imported via git)
- **Rationale:** MIT licensed, already imported, UIToolkit-based, not deprecated
- **Documentation:** https://github.com/Gentlymad-Studios/GraphViewBase

### 3D Scene Visualization: Custom GL-Based
- **Approach:** Built-in Unity GL API with patterns from ALINE
- **Reference:** eppz.Lines (MIT) for implementation patterns
- **Rationale:** Zero paid dependencies, URP/HDRP compatible

### Runtime Debugger: Custom Mercury Inspector
- **Approach:** Lightweight IMGUI/UIToolkit solution focused on message flow
- **Reference:** UnityRuntimeInspector (MIT) for UI patterns
- **Rationale:** Focused solution more useful than general-purpose inspector

### Reference Implementations (Study Only)
- **ALINE:** Scope-based drawing API, CommandBuilder pattern, camera injection
- **EPO Outline:** Component marking, static registry, layer filtering
- **GraphViewBase:** Direct use (MIT licensed)

---

## Quick Resume

**Where to start if beginning this task:**
1. Read this document completely
2. Review `visual-composer-tasks.md` for detailed checklist
3. Study GraphViewBase source code (already imported)
4. Study ALINE patterns for 3D visualization approach
5. Prototype simple node-based editor extending GraphViewBase

**First 3 steps to take:**
1. Create prototype extending GraphViewBase's `GraphView` and `BaseNode` classes
2. Implement MmRelayNode ↔ NodeView bidirectional sync
3. Add GL-based scene visualization for connections

**Key files to read first:**
- `Library/PackageCache/com.gentlymad.graphviewbase@.../Editor/Elements/GraphView.cs`
- `Library/PackageCache/com.gentlymad.graphviewbase@.../Editor/Elements/Graph/BaseNode.cs`
- `Assets/Plugins/Plugins/ALINE/Draw.cs` (for GL patterns)
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs`

---

## Technical Overview

This initiative creates visual tools for constructing and debugging Mercury networks. **Six core tools:**

### Editor Tools (Phase 1)
1. **Hierarchy Mirroring** - Auto-convert Unity hierarchy to Mercury network
2. **Network Templates** - Pre-built patterns (hub-spoke, chain, broadcast)
3. **Visual Composer** - Drag-and-drop node graph editor (GraphViewBase)
4. **Network Validator** - Detect issues before runtime

### Visualization & Debugging (Phase 2-3)
5. **3D Scene Visualization** - Real-time connection lines in Scene view (custom GL)
6. **Runtime Debugger** - In-game message flow inspector (custom UI)

**Expected Impact:**
- 50% reduction in network setup time
- 70% fewer configuration errors
- Visual debugging of message flow
- Reusable network patterns
- **NEW:** Live message path visualization in Scene view
- **NEW:** Runtime debugging without stopping play mode

---

## Current State Analysis

### Existing Setup Process (Manual)

Developers currently must:
1. Add MmRelayNode components manually
2. Call `MmAddToRoutingTable` in code for each connection
3. Configure level filters, tags manually
4. No visual feedback until runtime
5. Errors only discovered during play mode

**Pain Points:**
- Tedious for complex hierarchies
- Easy to miss connections
- Hard to visualize message flow
- No validation until runtime
- Difficult to share patterns

---

## Architecture Design

### Tool 1: Hierarchy Mirroring

**Purpose:** One-click conversion of Unity hierarchy to Mercury network

**UI Location:** Unity Editor window (Menu: Mercury → Mirror Hierarchy)

**Workflow:**
```
User selects root GameObject
↓
Tool analyzes hierarchy
↓
Preview shows proposed network structure
↓
User configures options (filters, tags, naming)
↓
Tool creates MmRelayNode components
↓
Tool connects nodes based on hierarchy
↓
Validation report shows results
```

**Architecture:**

```csharp
public class MmHierarchyMirror : EditorWindow {
    // Configuration
    public bool IncludeInactive = false;
    public string NodeNameSuffix = "_MercuryNode";
    public MmTag DefaultTag = MmTag.Everything;
    public MmLevelFilter DefaultLevel = MmLevelFilter.SelfAndChildren;

    // Filter rules
    public List<MirrorRule> Rules = new List<MirrorRule>();

    [MenuItem("Mercury/Mirror Hierarchy")]
    public static void ShowWindow() {
        GetWindow<MmHierarchyMirror>("Hierarchy Mirror");
    }

    public void OnGUI() {
        // 1. GameObject selection
        rootObject = EditorGUILayout.ObjectField("Root Object", rootObject, typeof(GameObject), true);

        // 2. Configuration options
        IncludeInactive = EditorGUILayout.Toggle("Include Inactive", IncludeInactive);
        DefaultTag = (MmTag)EditorGUILayout.EnumPopup("Default Tag", DefaultTag);

        // 3. Preview hierarchy
        if (GUILayout.Button("Preview")) {
            previewData = AnalyzeHierarchy(rootObject);
        }
        DrawPreview(previewData);

        // 4. Apply button
        if (GUILayout.Button("Apply")) {
            MirrorHierarchy(rootObject);
        }
    }

    private MmRelayNode MirrorHierarchy(GameObject obj) {
        // Add relay node if not exists
        var node = obj.GetComponent<MmRelayNode>();
        if (node == null) {
            node = obj.AddComponent<MmRelayNode>();
        }

        // Configure node
        ConfigureNode(node, obj);

        // Recursively mirror children
        foreach (Transform child in obj.transform) {
            if (ShouldInclude(child.gameObject)) {
                var childNode = MirrorHierarchy(child.gameObject);
                // Add to routing table
                node.MmAddToRoutingTable(childNode.gameObject, DefaultLevel);
            }
        }

        return node;
    }
}

public class MirrorRule {
    public string ObjectNamePattern; // Regex
    public bool Include = true;
    public MmTag? TagOverride = null;
    public MmLevelFilter? LevelOverride = null;
}
```

**Features:**
- Preview before apply
- Undo support
- Selective mirroring (filter by name, tag, layer)
- Rule-based configuration
- Dry-run mode

---

### Tool 2: Network Templates

**Purpose:** Pre-built network patterns for common use cases

**Templates Included:**

#### A. Hub-and-Spoke Pattern
```
         Hub
       /  |  \
    S1   S2   S3
```

Use case: Central coordinator with multiple independ spokes (UI menu with buttons)

```csharp
public class HubAndSpokeTemplate : MmNetworkTemplate {
    public int SpokeCount = 4;
    public string HubName = "Hub";
    public string SpokePrefix = "Spoke";

    public override GameObject CreateNetwork(Vector3 position) {
        var hub = new GameObject(HubName);
        var hubNode = hub.AddComponent<MmRelayNode>();
        hub.transform.position = position;

        float angleStep = 360f / SpokeCount;
        for (int i = 0; i < SpokeCount; i++) {
            var spoke = new GameObject($"{SpokePrefix}_{i}");
            var spokeNode = spoke.AddComponent<MmRelayNode>();
            spoke.transform.SetParent(hub.transform);

            // Position in circle around hub
            float angle = angleStep * i * Mathf.Deg2Rad;
            spoke.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * 2f,
                0,
                Mathf.Sin(angle) * 2f
            );

            // Connect to hub
            hubNode.MmAddToRoutingTable(spoke, MmLevelFilter.Child);
        }

        return hub;
    }
}
```

#### B. Chain Pattern
```
A → B → C → D
```

Use case: Sequential processing (wizard steps, state machine)

```csharp
public class ChainTemplate : MmNetworkTemplate {
    public int ChainLength = 4;
    public string NodePrefix = "Step";
    public bool Bidirectional = false;

    public override GameObject CreateNetwork(Vector3 position) {
        GameObject firstNode = null;
        GameObject prevNode = null;

        for (int i = 0; i < ChainLength; i++) {
            var node = new GameObject($"{NodePrefix}_{i}");
            var relayNode = node.AddComponent<MmRelayNode>();
            node.transform.position = position + Vector3.right * i * 2f;

            if (i == 0) firstNode = node;

            if (prevNode != null) {
                // Forward connection
                var prevRelay = prevNode.GetComponent<MmRelayNode>();
                prevRelay.MmAddToRoutingTable(node, MmLevelFilter.Child);

                // Backward connection (if bidirectional)
                if (Bidirectional) {
                    relayNode.MmAddToRoutingTable(prevNode, MmLevelFilter.Parent);
                }
            }

            prevNode = node;
        }

        return firstNode;
    }
}
```

#### C. Broadcast Tree
```
      Root
     /    \
   L1a    L1b
   / \    / \
 L2a L2b L2c L2d
```

Use case: Hierarchical propagation (scene graph, organization chart)

#### D. Event Aggregator
```
Source1 →\
Source2 → Aggregator → Sink
Source3 →/
```

Use case: Many-to-one messaging (UI events to controller)

#### E. Peer Network (Mesh)
```
A ←→ B
↑ ×  ↑
C ←→ D
```

Use case: Fully connected peers (multiplayer, collaboration)

**Template System Architecture:**

```csharp
public abstract class MmNetworkTemplate : ScriptableObject {
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Texture2D Icon { get; }

    public abstract GameObject CreateNetwork(Vector3 position);

    // Optional: parameter configuration UI
    public virtual void DrawConfiguration() {
        // Override to show custom GUI
    }
}

public class MmTemplateLibrary {
    private static List<MmNetworkTemplate> _templates;

    public static List<MmNetworkTemplate> GetAllTemplates() {
        if (_templates == null) {
            _templates = new List<MmNetworkTemplate> {
                ScriptableObject.CreateInstance<HubAndSpokeTemplate>(),
                ScriptableObject.CreateInstance<ChainTemplate>(),
                ScriptableObject.CreateInstance<BroadcastTreeTemplate>(),
                ScriptableObject.CreateInstance<EventAggregatorTemplate>(),
                ScriptableObject.CreateInstance<PeerNetworkTemplate>()
            };
        }
        return _templates;
    }
}
```

---

### Tool 3: Visual Network Composer

**Purpose:** Drag-and-drop node graph editor for complex networks

**Technology:** Unity GraphView API (used by Shader Graph, Visual Scripting)

**UI Layout:**
```
┌─────────────────────────────────────────────┐
│ File Edit View Help               [Validate]│
├─────────┬───────────────────────────────────┤
│Template │                                   │
│Library  │        Graph Canvas               │
│         │                                   │
│Hub-Spoke│   [Node]──→[Node]                │
│Chain    │      │        │                   │
│Tree     │      ↓        ↓                   │
│Mesh     │   [Node]   [Node]                │
│Custom   │                                   │
│         │                                   │
├─────────┼───────────────────────────────────┤
│Inspector│        Minimap                    │
│         │   [══════════]                    │
│ Node:   │   [   ▪️     ]                    │
│ Name    │   [      ▪️  ]                    │
│ Tag     │                                   │
│ Level   │                                   │
└─────────┴───────────────────────────────────┘
```

**Features:**
- Drag nodes from template library
- Draw connections between nodes
- Configure node properties in inspector
- Real-time validation (circular dependencies, unreachable nodes)
- Minimap for navigation
- Export to Unity scene
- Import existing scene
- Save/load network configurations

**Architecture:**

```csharp
public class MmNetworkComposer : EditorWindow {
    private GraphView _graphView;
    private NodeView _selectedNode;
    private MmNetworkGraph _graph;

    [MenuItem("Mercury/Network Composer")]
    public static void ShowWindow() {
        GetWindow<MmNetworkComposer>("Network Composer");
    }

    private void OnEnable() {
        // Create GraphView
        _graphView = new MmGraphView();
        _graphView.graphViewChanged += OnGraphChanged;

        rootVisualElement.Add(_graphView);

        // Add toolbar
        var toolbar = new Toolbar();
        toolbar.Add(new ToolbarButton(NewNetwork) { text = "New" });
        toolbar.Add(new ToolbarButton(SaveNetwork) { text = "Save" });
        toolbar.Add(new ToolbarButton(LoadNetwork) { text = "Load" });
        toolbar.Add(new ToolbarButton(ExportToScene) { text = "Export" });
        toolbar.Add(new ToolbarButton(Validate) { text = "Validate" });
        rootVisualElement.Add(toolbar);
    }

    private GraphViewChange OnGraphChanged(GraphViewChange change) {
        // Handle node creation
        if (change.elementsToCreate != null) {
            foreach (var element in change.elementsToCreate) {
                if (element is NodeView node) {
                    _graph.AddNode(node.NodeData);
                }
            }
        }

        // Handle edge creation (connections)
        if (change.edgesToCreate != null) {
            foreach (var edge in change.edgesToCreate) {
                _graph.AddConnection(edge.output.node, edge.input.node);
            }
        }

        // Validate after changes
        ValidateGraph();

        return change;
    }

    public GameObject ExportToScene() {
        var root = new GameObject("MercuryNetwork");
        var nodeMap = new Dictionary<NodeView, GameObject>();

        // Create GameObjects for all nodes
        foreach (var node in _graph.Nodes) {
            var go = new GameObject(node.Name);
            var relay = go.AddComponent<MmRelayNode>();

            // Configure from node data
            relay.Tag = node.Tag;
            relay.ActiveFilter = node.ActiveFilter;

            nodeMap[node] = go;
        }

        // Create connections
        foreach (var connection in _graph.Connections) {
            var sourceGo = nodeMap[connection.Source];
            var targetGo = nodeMap[connection.Target];
            var sourceRelay = sourceGo.GetComponent<MmRelayNode>();

            sourceRelay.MmAddToRoutingTable(targetGo, connection.LevelFilter);
        }

        return root;
    }
}

public class NodeView : Node {
    public MmNodeData NodeData { get; private set; }

    public NodeView(MmNodeData data) {
        NodeData = data;
        title = data.Name;

        // Create input port
        var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        inputPort.portName = "In";
        inputContainer.Add(inputPort);

        // Create output port
        var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
        outputPort.portName = "Out";
        outputContainer.Add(outputPort);

        // Style
        RefreshExpandedState();
        RefreshPorts();
    }
}
```

---

### Tool 4: Network Validator

**Purpose:** Detect issues before runtime

**Validation Checks:**

1. **Circular Dependencies**
   - Detect message loops
   - Suggest fixes (add hop limit, change filters)

2. **Unreachable Nodes**
   - Find nodes with no incoming connections
   - Find nodes with no outgoing connections
   - Suggest fixes (add connections, remove nodes)

3. **Performance Estimation**
   - Estimate routing latency
   - Detect potential bottlenecks
   - Suggest optimizations

4. **Best Practice Warnings**
   - Too many connections (> 100 per node)
   - Deep hierarchies (> 20 levels)
   - Inefficient filter usage
   - Suggest improvements

**Architecture:**

```csharp
public class MmNetworkValidator {
    public ValidationResult Validate(MmRelayNode root) {
        var result = new ValidationResult();

        // Check 1: Circular dependencies
        var circularPaths = DetectCircularDependencies(root);
        foreach (var path in circularPaths) {
            result.Errors.Add(new ValidationError {
                Severity = ErrorSeverity.Error,
                Message = $"Circular dependency: {string.Join(" → ", path)}",
                Suggestion = "Add hop limit or change level filters"
            });
        }

        // Check 2: Unreachable nodes
        var unreachable = FindUnreachableNodes(root);
        foreach (var node in unreachable) {
            result.Warnings.Add(new ValidationWarning {
                Severity = ErrorSeverity.Warning,
                Message = $"Unreachable node: {node.name}",
                Suggestion = "Add incoming connection or remove node"
            });
        }

        // Check 3: Performance estimation
        result.PerformanceMetrics = EstimatePerformance(root);
        if (result.PerformanceMetrics.EstimatedLatency > 10.0f) {
            result.Warnings.Add(new ValidationWarning {
                Severity = ErrorSeverity.Warning,
                Message = "High estimated latency (> 10ms)",
                Suggestion = "Optimize routing table or reduce depth"
            });
        }

        // Check 4: Best practices
        CheckBestPractices(root, result);

        return result;
    }

    private List<List<string>> DetectCircularDependencies(MmRelayNode root) {
        var visited = new HashSet<int>();
        var recursionStack = new HashSet<int>();
        var path = new List<string>();
        var cycles = new List<List<string>>();

        void DFS(MmRelayNode node) {
            int nodeId = node.GetInstanceID();
            visited.Add(nodeId);
            recursionStack.Add(nodeId);
            path.Add(node.name);

            foreach (var item in node.RoutingTable.Responders) {
                var childRelay = item.Responder as MmRelayNode;
                if (childRelay != null) {
                    int childId = childRelay.GetInstanceID();

                    if (!visited.Contains(childId)) {
                        DFS(childRelay);
                    } else if (recursionStack.Contains(childId)) {
                        // Cycle detected
                        int cycleStart = path.IndexOf(childRelay.name);
                        cycles.Add(path.GetRange(cycleStart, path.Count - cycleStart));
                    }
                }
            }

            recursionStack.Remove(nodeId);
            path.RemoveAt(path.Count - 1);
        }

        DFS(root);
        return cycles;
    }

    private PerformanceMetrics EstimatePerformance(MmRelayNode root) {
        return new PerformanceMetrics {
            EstimatedLatency = EstimateMaxLatency(root),
            EstimatedThroughput = EstimateThroughput(root),
            BottleneckNodes = FindBottlenecks(root)
        };
    }
}

public class ValidationResult {
    public List<ValidationError> Errors = new List<ValidationError>();
    public List<ValidationWarning> Warnings = new List<ValidationWarning>();
    public PerformanceMetrics PerformanceMetrics;

    public bool IsValid => Errors.Count == 0;
}
```

**UI Display:**

```
Validation Results
══════════════════

❌ ERRORS (2)
  • Circular dependency: Hub → Node1 → Node2 → Hub
    Fix: Add hop limit or change level filters
  • Missing component: Node3 has no MmRelayNode

⚠️ WARNINGS (3)
  • Unreachable node: OrphanNode
    Fix: Add incoming connection or remove node
  • High estimated latency: 15.2ms
    Fix: Optimize routing table or reduce depth
  • Too many connections: Hub has 150 connections
    Fix: Consider splitting into sub-hubs

✅ BEST PRACTICES (2)
  • Deep hierarchy: 25 levels detected
    Suggestion: Consider flattening or using mesh routing
  • Inefficient filter: Node5 uses SelfAndBidirectional unnecessarily
    Suggestion: Change to SelfAndChildren for performance

PERFORMANCE ESTIMATE
Estimated Latency: 15.2ms
Estimated Throughput: 8500 messages/sec
Bottleneck Nodes: Hub, Controller
```

---

## Design Decisions

### Decision 1: Graph Editor Framework (Updated 2025-12-01)

**Options Evaluated:**
A. Unity GraphView (`Experimental.GraphView`) - deprecated, minimal docs
B. Unity Graph Toolkit (Unity 6.2+) - experimental, locked to Unity 6.2+
C. GraphViewBase (MIT) - UIToolkit-based, already imported
D. xNode - IMGUI-based, older
E. Custom node editor - high effort

**Decision:** C (GraphViewBase)

**Rationale:**
- **MIT licensed** - can redistribute without paid dependencies
- **Already imported** in project via git package
- **UIToolkit-based** - not deprecated like `Experimental.GraphView`
- **Works on any Unity version** with UIToolkit (not locked to Unity 6.2+)
- **Source code available** - can customize as needed
- **Actively maintained** by Gentlymad Studios

**Rejected:**
- Unity GraphView: Deprecated, minimal documentation, risky for long-term
- Unity Graph Toolkit: Experimental, Unity 6.2+ only, API still changing
- xNode: IMGUI-based (older technology), less active maintenance

### Decision 2: Export Format

**Options:**
A. Export to scene (create GameObjects)
B. Export to ScriptableObject (data only)
C. Both

**Decision:** C (Both)

**Rationale:**
- Scene export for immediate use
- ScriptableObject for sharing and version control
- Can load ScriptableObject to recreate scene
- Flexible workflow

### Decision 3: Validation Timing

**Options:**
A. Real-time (as you edit)
B. On-demand (click button)
C. Both

**Decision:** C (Both)

**Rationale:**
- Real-time for immediate feedback
- On-demand for detailed report
- Toggleable to avoid performance impact

### Decision 4: 3D Scene Visualization (New 2025-12-01)

**Options:**
A. ALINE (paid asset) - professional, feature-rich
B. Popcron Gizmos (free) - SRP compatibility issues
C. Custom GL solution - zero dependencies

**Decision:** C (Custom GL solution)

**Rationale:**
- **Zero paid dependencies** - anyone can use without purchasing
- **URP/HDRP compatible** via `RenderPipelineManager.endCameraRendering`
- **Learn patterns from ALINE** - scope-based API, CommandBuilder batching
- **Simpler than full library** - only need line drawing for connections

**Implementation Approach:**
```csharp
// Inspired by ALINE's scope pattern
using (MmDraw.WithColor(Color.cyan))
{
    MmDraw.Line(nodeA.position, nodeB.position);
    MmDraw.Arrow(nodeB.position, direction);
}
```

### Decision 5: Runtime Debugging (New 2025-12-01)

**Options:**
A. Fork UnityRuntimeInspector (MIT) - full-featured, GC-optimized
B. Lightweight custom solution - focused on Mercury

**Decision:** B (Lightweight custom solution)

**Rationale:**
- **Focused on Mercury messaging** - more useful than general inspector
- **Smaller footprint** - less code to maintain
- **Study UnityRuntimeInspector patterns** for UI best practices

---

## Implementation Strategy (Updated 2025-12-01)

**Priority Order:** Graph Editor → Scene Visualization → Runtime Debugger

### Phase 1: Editor Tools (216h) - Weeks 1-6

**Week 1-2: Visual Composer Core (80h)**
1. Study GraphViewBase architecture (8h)
2. Create MmNetworkComposer EditorWindow (16h)
3. Implement MmNodeView extending BaseNode (16h)
4. Implement MmEdgeView extending BaseEdge (16h)
5. Add bidirectional sync with scene (16h)
6. Integration testing (8h)

**Week 2-3: Hierarchy Mirroring (36h)**
7. Design UI (8h)
8. Implement traversal logic (16h)
9. Add configuration options (12h)

**Week 3-4: Templates (52h)**
10. Create template base class (12h)
11. Implement 5 core templates (20h)
12. Add template UI (20h)

**Week 5-6: Validation (48h)**
13. Create validator class (20h)
14. Implement validation rules (16h)
15. Create validation UI (12h)

### Phase 2: Scene Visualization (40h) - Week 7

**3D Connection Visualization**
16. Create MmConnectionDrawer using GL API (16h)
17. Add color-coding by message type (8h)
18. Implement message pulse animation (8h)
19. Add scene overlay toggle and settings (8h)

### Phase 3: Runtime Debugger (60h) - Week 8

**Mercury Message Inspector**
20. Create MmRuntimeDebugger component (20h)
21. Implement message stream view (16h)
22. Add filtering by method/tag/level (12h)
23. Create node hierarchy browser (12h)

---

## Open Questions

1. **Should templates be extensible by users?**
   - Could allow custom templates via ScriptableObject
   - Needs clear API and documentation

2. **How to handle prefab variants in mirroring?**
   - Should prefab structure be preserved?
   - Or flatten everything?

3. **Should visual composer support runtime editing?**
   - Could be powerful for debugging
   - Performance implications

---

## References

### Key Technologies
- **GraphViewBase** (MIT) - https://github.com/Gentlymad-Studios/GraphViewBase
- **Unity GL API** - Built-in line drawing
- **Unity UIToolkit** - Modern UI framework
- **Unity EditorWindow** - Editor extension base

### Reference Implementations (Study Only)
- `Assets/Plugins/Plugins/ALINE/Draw.cs` - Scope-based drawing API
- `Assets/Plugins/Plugins/ALINE/CommandBuilder.cs` - Batch rendering
- `Assets/Plugins/Plugins/EasyPerformantOutline/Scripts/Outlinable.cs` - Component marking
- `Assets/Plugins/Plugins/EasyPerformantOutline/Scripts/Outliner.cs` - Render pipeline integration

### Open Source Resources
- [eppz.Lines](https://github.com/eppz/Unity.Library.eppz.Lines) (MIT) - GL drawing patterns
- [UnityRuntimeInspector](https://github.com/yasirkula/UnityRuntimeInspector) (MIT) - Runtime UI patterns

### Related Documents
- Master Plan: Phase 4.2
- Tasks: `visual-composer-tasks.md`
- Technology Decision: `.claude/plans/crispy-strolling-dawn.md`

---

**Document Version:** 2.0
**Last Updated:** 2025-12-01
**Owner:** Developer Tools Team
**Major Changes:** Technology stack revision (GraphViewBase, custom GL, runtime debugger)
