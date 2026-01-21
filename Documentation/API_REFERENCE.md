# API Reference

This document covers the core messaging concepts, types, and quick reference for MercuryMessaging.

## Message Structure

Every message contains:
```csharp
MmMessage {
    MmMethod method;              // What action to perform
    MmMessageType messageType;    // Type identifier for serialization
    MmMetadataBlock MetadataBlock; // Routing control
    int NetId;                    // Network identifier
    bool IsDeserialized;          // Came from network?
}
```

## MmMethod Enum (Standard Methods 0-18)

```csharp
NoOp = 0,
Initialize = 1,
SetActive = 2,
Refresh = 3,
Switch = 4,
Complete = 5,
TaskInfo = 6,
Message = 7,
MessageBool = 8,
MessageInt = 9,
MessageFloat = 10,
MessageString = 11,
MessageVector3 = 12,
MessageVector4 = 13,
MessageQuaternion = 14,
MessageTransform = 15,
MessageTransformList = 16,
MessageByteArray = 17,
MessageGameObject = 18
// Custom methods: Use values > 1000
```

---

## Filtering System

**MmMetadataBlock** combines four orthogonal filters:

### 1. Level Filter - Direction
- `Self` - Only originating node
- `Child` - Descendants only
- `Parent` - Ancestors only
- `SelfAndChildren` - Default
- `SelfAndBidirectional` - Everything

### 2. Active Filter - GameObject state
- `Active` - Only active GameObjects
- `All` - Both active and inactive

### 3. Selected Filter - FSM state
- `All` - All responders
- `Selected` - Only currently selected in FSM

### 4. Network Filter - Network origin
- `Local` - Local messages only
- `Network` - Network messages only
- `All` - Both

### 5. Tag System - 8-bit flag system
- `Tag0` through `Tag7`
- `Everything` (-1) - Matches all
- `Nothing` (0) - Matches none
- Bitwise combinations supported

## Tag Checking

Components can filter messages by tag:
```csharp
public MmTag Tag = MmTag.Tag0;
public bool TagCheckEnabled = true;
```

Message must have matching tag bits to be received:
```csharp
if (TagCheckEnabled && (message.MetadataBlock.Tag & Tag) == 0) {
    return; // Tag mismatch, ignore message
}
```

---

## Quick Reference

### Sending Messages

```csharp
// Simple message
relay.MmInvoke(MmMethod.Initialize);

// With parameters
relay.MmInvoke(MmMethod.SetActive, true);
relay.MmInvoke(MmMethod.MessageString, "Hello");
relay.MmInvoke(MmMethod.MessageInt, 42);

// With metadata
relay.MmInvoke(
    MmMethod.Switch,
    new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All)
);

// Full control
relay.MmInvoke(
    MmMethod.MessageFloat,
    3.14f,
    new MmMetadataBlock(
        MmLevelFilter.SelfAndChildren,
        MmActiveFilter.Active,
        MmSelectedFilter.All,
        MmNetworkFilter.Local,
        MmTag.Tag0
    )
);
```

### Creating Custom Responders

```csharp
public class MyResponder : MmBaseResponder
{
    // Override specific message handlers
    protected override void ReceivedMessage(MmMessageBool msg) { }
    protected override void ReceivedMessage(MmMessageInt msg) { }
    protected override void ReceivedMessage(MmMessageFloat msg) { }
    protected override void ReceivedMessage(MmMessageString msg) { }

    // Override standard methods
    protected override void ReceivedSetActive(bool active) { }
    protected override void ReceivedInitialize() { }
    protected override void ReceivedSwitch(int stateIndex) { }
    protected override void ReceivedRefresh() { }

    // Handle custom methods (>1000)
    public override void MmInvoke(MmMessage message) {
        if (message.method == (MmMethod)1001) {
            // Handle custom method
        } else {
            base.MmInvoke(message); // Call base for standard methods
        }
    }
}
```

### FSM Setup

```csharp
// Component on GameObject with MmRelaySwitchNode
void Start() {
    var fsm = GetComponent<MmRelaySwitchNode>().RespondersFSM;

    // Jump to state
    fsm.JumpTo("StateName");

    // Transition with events
    fsm.StartTransitionTo("StateName");

    // Listen to state changes
    fsm.GlobalEnter += (fromState, toState) => {
        Debug.Log($"Switched from {fromState} to {toState}");
    };
}
```
