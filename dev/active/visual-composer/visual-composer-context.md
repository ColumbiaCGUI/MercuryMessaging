# Visual Network Composer - Technical Context

**Last Updated:** 2025-11-18
**Status:** Planning - Not Started
**Priority:** MEDIUM (Phase 4.2)

---

## Status

**Current State:** Not Started
**Blockers:** None - can begin anytime
**Dependencies:** None (standalone developer tool)
**Estimated Timeline:** 212 hours (5-6 weeks)

---

## Quick Resume

**Where to start if beginning this task:**
1. Read this document completely
2. Review `visual-composer-tasks.md` for detailed checklist
3. Study Unity's GraphView API documentation
4. Review existing hierarchy mirroring needs
5. Prototype simple node-based editor

**First 3 steps to take:**
1. Design hierarchy mirroring architecture (8h)
2. Implement hierarchy traversal and node creation (16h)
3. Create basic Unity Editor window (12h)

**Key files to read first:**
- Unity GraphView documentation
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs`
- Examples of Unity node-based editors (Shader Graph, Visual Scripting)

---

## Technical Overview

This initiative creates visual tools for constructing Mercury networks, eliminating manual setup and reducing errors. Four core tools:

1. **Hierarchy Mirroring** - Auto-convert Unity hierarchy to Mercury network
2. **Network Templates** - Pre-built patterns (hub-spoke, chain, broadcast)
3. **Visual Composer** - Drag-and-drop node graph editor
4. **Network Validator** - Detect issues before runtime

**Expected Impact:**
- 50% reduction in network setup time
- 70% fewer configuration errors
- Visual debugging of message flow
- Reusable network patterns

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

### Decision 1: GraphView vs Custom Editor

**Options:**
A. Unity GraphView (used by Shader Graph)
B. Custom node editor (full control)
C. Third-party library (odin, NodeCanvas)

**Decision:** A (Unity GraphView)

**Rationale:**
- Familiar to Unity developers
- Robust and well-tested
- Future-proof (Unity actively developing)
- Good performance
- Native look and feel

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

---

## Implementation Strategy

### Week 1-2: Hierarchy Mirroring (36h)

1. Design UI (8h)
2. Implement traversal logic (16h)
3. Add configuration options (12h)

### Week 3-4: Templates (52h)

4. Create template base class (12h)
5. Implement 5 core templates (20h)
6. Add template UI (20h)

### Week 4-6: Visual Composer (96h)

7. Design GraphView architecture (16h)
8. Implement graph editor (40h)
9. Add export functionality (16h)
10. Integration testing (20h)

### Week 6: Validation (48h)

11. Create validator class (20h)
12. Implement validation rules (16h)
13. Create validation UI (12h)

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
- Unity GraphView API
- Unity UIElements
- Unity EditorWindow

### Related Documents
- Master Plan: Phase 4.2
- Tasks: `visual-composer-tasks.md`

---

**Document Version:** 1.0
**Last Updated:** 2025-11-18
**Owner:** Developer Tools Team
