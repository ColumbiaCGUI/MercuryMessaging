# LLM Message Design - Technical Context

This document provides technical context for implementing LLM-assisted message routing configuration using MercuryMessaging.

---

## Core Concept: NL→Configuration Mapping

### Traditional Manual Configuration

```csharp
// Developer must manually:
// 1. Design hierarchy structure
// 2. Assign appropriate tags
// 3. Configure filter settings
// 4. Set up routing rules

// Example: Player-Enemy interaction system
// Manual setup takes 30-60 minutes for complex scenarios
```

### LLM-Assisted Configuration

```
Input: "When player attacks enemy, enemy takes damage and shows hit effect"

Output:
- Hierarchy: GameRoot > Entities > Player, Enemy
- Tags: Player=Tag.Player, Enemy=Tag.Enemy
- Message: AttackMessage → Enemy responders via Tag filter
- Filter: SelfAndChildren, Active, Tag.Enemy
```

---

## Mercury Concept Mapping for LLM

### Entity Types

| Natural Language | Mercury Concept | Example |
|------------------|-----------------|---------|
| "object", "thing" | MmBaseResponder | "the light" |
| "group", "all X" | MmRelayNode | "all enemies" |
| "area", "room" | MmRelayNode (spatial) | "the dungeon" |
| "state", "mode" | MmRelaySwitchNode | "when paused" |

### Relationship Types

| Natural Language | Mercury Relationship | Configuration |
|------------------|---------------------|---------------|
| "when X happens" | Message trigger | MmInvoke call |
| "notify", "tell" | Message target | LevelFilter setting |
| "in", "inside" | Parent-child hierarchy | SetParent |
| "all", "every" | Broadcast | SelfAndChildren |
| "nearby" | Spatial filter | Custom filter |

### Filter Mapping

| Natural Language | Mercury Filter |
|------------------|----------------|
| "only active" | MmActiveFilter.Active |
| "including disabled" | MmActiveFilter.All |
| "children only" | MmLevelFilter.Child |
| "everyone" | MmLevelFilter.SelfAndBidirectional |
| "locally" | MmNetworkFilter.Local |
| "across network" | MmNetworkFilter.All |

---

## LLM Prompt Engineering

### System Prompt Template

```
You are a Mercury Messaging configuration assistant for Unity game development.

MERCURY CONCEPTS:
- MmRelayNode: Routes messages, can have children
- MmBaseResponder: Receives and handles messages
- MmRelaySwitchNode: FSM-enabled router (states)
- MmMetadataBlock: Routing filters (Level, Active, Tag, Network)
- Tags: Tag0-Tag7 for categorization (8 available)
- Level Filters: Self, Child, Parent, SelfAndChildren

OUTPUT FORMAT:
1. HIERARCHY: Parent-child structure
2. COMPONENTS: Which Mercury components on each GameObject
3. TAGS: Tag assignments (Tag0-Tag7)
4. ROUTING: MmInvoke calls with complete MmMetadataBlock

CONSTRAINTS:
- Maximum hierarchy depth: 10 levels
- Maximum tags per project: 8 (Tag0-Tag7)
- Prefer SelfAndChildren over bidirectional
- Use specific tags over Tag.Everything
```

### Few-Shot Examples

**Example 1: UI Notification**
```
User: "When player health changes, update the health bar"

Output:
HIERARCHY:
  GameUI (MmRelayNode)
  └── HealthBar (MmBaseResponder, Tag.UI)

COMPONENTS:
- GameUI: MmRelayNode (routes UI messages)
- HealthBar: Custom HealthBarResponder extends MmBaseResponder

TAGS:
- Tag0 = UI elements

ROUTING:
gameUI.MmInvoke(
    new HealthUpdateMessage { value = newHealth },
    new MmMetadataBlock(
        MmLevelFilter.SelfAndChildren,
        tag: MmTag.Tag0
    )
);
```

**Example 2: Spatial Interaction**
```
User: "When player enters a room, turn on the room's lights"

Output:
HIERARCHY:
  WorldRoot (MmRelayNode)
  └── Room (MmRelayNode, Tag.Room)
      ├── RoomTrigger (MmBaseResponder)
      └── Lights (MmRelayNode, Tag.Lighting)
          └── Light1..N (MmBaseResponder)

ROUTING (in RoomTrigger.OnTriggerEnter):
GetComponentInParent<MmRelayNode>().MmInvoke(
    MmMethod.SetActive,
    true,
    new MmMetadataBlock(
        MmLevelFilter.SelfAndChildren,
        tag: MmTag.Tag1  // Lighting tag
    )
);
```

---

## Validation Rules

### Semantic Validation

The system validates generated configurations against Mercury rules:

```csharp
public class MercuryConfigValidator
{
    public ValidationResult Validate(GeneratedConfig config)
    {
        var errors = new List<string>();

        // Rule 1: Hierarchy depth
        if (config.MaxDepth > 10)
            errors.Add("Hierarchy exceeds 10 levels");

        // Rule 2: Tag exhaustion
        if (config.UsedTags.Count > 8)
            errors.Add("More than 8 tags used");

        // Rule 3: Circular references
        if (HasCircularReferences(config.Hierarchy))
            errors.Add("Circular parent-child reference detected");

        // Rule 4: Missing relay nodes
        foreach (var responder in config.Responders)
        {
            if (!HasParentRelay(responder))
                errors.Add($"{responder.Name} has no parent MmRelayNode");
        }

        // Rule 5: Invalid filter combinations
        foreach (var route in config.Routes)
        {
            if (route.LevelFilter == MmLevelFilter.Parent &&
                route.Tag != MmTag.Everything)
                errors.Add("Tag filtering ineffective with Parent-only direction");
        }

        return new ValidationResult(errors);
    }
}
```

### Common Error Patterns

| Error | Detection | Suggestion |
|-------|-----------|------------|
| Orphaned responder | No MmRelayNode in parent chain | Add MmRelayNode to parent |
| Tag conflict | Same tag for unrelated systems | Use different tags |
| Over-broadcasting | SelfAndBidirectional everywhere | Use specific direction |
| Missing receiver | Tag set but no responders match | Add TagCheckEnabled |

---

## Output Formats

### JSON Configuration Format

```json
{
  "hierarchy": [
    {
      "name": "GameRoot",
      "component": "MmRelayNode",
      "children": [
        {
          "name": "Player",
          "component": "MmBaseResponder",
          "tag": "Tag0"
        },
        {
          "name": "Enemies",
          "component": "MmRelayNode",
          "tag": "Tag1",
          "children": [...]
        }
      ]
    }
  ],
  "routes": [
    {
      "source": "Player",
      "message": "AttackMessage",
      "levelFilter": "SelfAndChildren",
      "tag": "Tag1",
      "activeFilter": "Active"
    }
  ]
}
```

### C# Code Generation

```csharp
// Auto-generated Mercury configuration
// Source: "When player attacks, damage nearby enemies"

public class GeneratedPlayerAttack : MmBaseResponder
{
    public void OnAttack()
    {
        var relay = GetComponentInParent<MmRelayNode>();
        relay.MmInvoke(
            new AttackMessage { damage = 10 },
            new MmMetadataBlock(
                MmLevelFilter.SelfAndChildren,
                MmActiveFilter.Active,
                MmSelectedFilter.All,
                MmNetworkFilter.Local,
                MmTag.Tag1  // Enemy tag
            )
        );
    }
}
```

---

## Integration Points

### Unity Editor Window

```csharp
public class LLMConfigWindow : EditorWindow
{
    private string _userInput;
    private GeneratedConfig _lastConfig;

    void OnGUI()
    {
        EditorGUILayout.LabelField("Describe your message routing:");
        _userInput = EditorGUILayout.TextArea(_userInput, GUILayout.Height(100));

        if (GUILayout.Button("Generate Configuration"))
        {
            _lastConfig = LLMConfigGenerator.Generate(_userInput);
        }

        if (_lastConfig != null)
        {
            DrawHierarchyPreview(_lastConfig);

            if (GUILayout.Button("Apply to Scene"))
            {
                ApplyConfigToScene(_lastConfig);
            }
        }
    }
}
```

### Scene Modification

```csharp
public static class SceneConfigurator
{
    public static void ApplyConfig(GeneratedConfig config)
    {
        Undo.RegisterCompleteObjectUndo(/* scene objects */);

        foreach (var node in config.Hierarchy)
        {
            var go = new GameObject(node.Name);
            go.AddComponent(node.ComponentType);

            if (node.Tag.HasValue)
            {
                var responder = go.GetComponent<MmBaseResponder>();
                responder.Tag = node.Tag.Value;
                responder.TagCheckEnabled = true;
            }

            if (node.Parent != null)
            {
                go.transform.SetParent(node.Parent.transform);
            }
        }

        // Refresh Mercury routing
        foreach (var relay in FindObjectsOfType<MmRelayNode>())
        {
            relay.MmRefreshResponders();
        }
    }
}
```

---

## Game Pattern Templates

### Pattern 1: Event Broadcast
```
"When X happens, notify all Y"
→ Source broadcasts to children with tag filter
```

### Pattern 2: State Change
```
"When game enters X state, enable/disable Y"
→ MmRelaySwitchNode with child states
→ SetActive messages on state transition
```

### Pattern 3: Proximity Interaction
```
"When X approaches Y, do Z"
→ Trigger collider + message relay
→ Parent-scoped message routing
```

### Pattern 4: Request-Response
```
"X asks Y for data, Y responds"
→ Query/Response pattern with callbacks
→ Bidirectional routing
```

### Pattern 5: Chain Reaction
```
"When X happens, Y triggers, which causes Z"
→ Linear message chain
→ Sequential responder hierarchy
```

---

*Last Updated: 2025-12-17*
