# Core Architecture

MercuryMessaging is built on three fundamental patterns that work together to enable hierarchical message routing.

## 1. Responder Pattern

Components implement the `IMmResponder` interface to receive messages:

```
IMmResponder (interface)
    ↓
MmResponder (abstract base)
    ↓
MmBaseResponder (implements method routing)
    ↓
Your Custom Responders
```

**Key Files:**
- `Assets/MercuryMessaging/Protocol/IMmResponder.cs` - Core interface
- `Assets/MercuryMessaging/Protocol/MmResponder.cs` - Base implementation
- `Assets/MercuryMessaging/Protocol/MmBaseResponder.cs` - Method routing with switch statements

## 2. Relay Node Pattern

`MmRelayNode` acts as the central message router:

```
Message → MmRelayNode → MmRoutingTable → Filtered Responders
                ↓
        Applies Filters:
        - Level (Parent/Child/Self)
        - Active (Active only vs All)
        - Tag (Multi-tag system)
        - Network (Local/Network/All)
```

**Key File:**
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs` (1422 lines - THE MOST IMPORTANT CLASS)

## 3. Hierarchical Message Flow

Messages propagate through Unity's GameObject hierarchy:

```
RootNode (MmRelaySwitchNode)
  ├── State1 (MmRelayNode) ← LevelFilter.Parent reaches here
  │   ├── Component1 (MmBaseResponder) ← LevelFilter.Child reaches here
  │   └── ChildNode (MmRelayNode)
  │       └── DeepComponent (MmBaseResponder)
  └── State2 (MmRelayNode)
```

**Direction Control:**
- `MmLevelFilter.Self` - Only the originating node
- `MmLevelFilter.Child` - Down the hierarchy
- `MmLevelFilter.Parent` - Up the hierarchy
- `MmLevelFilter.SelfAndChildren` - Self + descendants (default)
- `MmLevelFilter.SelfAndBidirectional` - All connected nodes

---

## Architecture Patterns

### Pattern 1: State Machine Hierarchy

Use `MmRelaySwitchNode` for state-based game structure:

```
GameManager (MmRelaySwitchNode)
  ├── MainMenu (MmRelayNode) - State 1
  │   ├── MenuUI (MmBaseResponder)
  │   └── MenuCamera (MmBaseResponder)
  ├── Gameplay (MmRelayNode) - State 2
  │   ├── Player (MmRelayNode)
  │   ├── Enemies (MmRelayNode)
  │   └── GameplayUI (MmBaseResponder)
  └── Pause (MmRelayNode) - State 3
      └── PauseUI (MmBaseResponder)
```

Only the current state's responders receive "Selected" filtered messages.

### Pattern 2: Effect Hierarchies

Group related effects under a relay node:

```
EffectManager (MmRelayNode)
  ├── ParticleSystem (MmBaseResponder)
  ├── AudioSource (MmBaseResponder)
  ├── AnimationController (MmBaseResponder)
  └── LightController (MmBaseResponder)
```

Send one message to enable/disable entire effect group:
```csharp
effectManager.MmInvoke(MmMethod.SetActive, false, MmMetadataBlock.Default);
```

### Pattern 3: Bidirectional Communication

Child notifies parent of events:

```csharp
// In child component
public class ButtonResponder : MmBaseResponder {
    public void OnClick() {
        // Notify parent that button was clicked
        GetComponent<MmRelayNode>().MmInvoke(
            MmMethod.MessageString,
            "ButtonClicked",
            new MmMetadataBlock(MmLevelFilter.Parent)
        );
    }
}

// In parent component
public class MenuController : MmBaseResponder {
    protected override void ReceivedMessage(MmMessageString message) {
        if (message.value == "ButtonClicked") {
            // Handle button click
        }
    }
}
```

---

## Best Practices

1. **Use Hierarchy Wisely**: Organize GameObjects to match message flow patterns
2. **Tag Strategically**: Use tags to separate concerns (UI, Gameplay, Network, etc.)
3. **Prefer SelfAndChildren**: Most messages should target self and descendants
4. **Network-Aware**: Always consider network implications when designing messages
5. **Custom Methods**: Keep custom method numbers > 1000 to avoid conflicts
6. **Debug Logging**: Enable MmLogger categories during development
7. **FSM for States**: Use MmRelaySwitchNode for game states, menus, and phases
8. **Task System**: Use MmTaskManager for user studies and experimental workflows
9. **Loose Coupling**: Avoid storing responder references - always use messages
10. **Test Hierarchy**: Use `RefreshParents()` after runtime hierarchy changes
