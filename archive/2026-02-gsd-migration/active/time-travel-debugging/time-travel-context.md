# Time Travel Debugging - Technical Context

This document provides technical context for implementing message-centric time travel debugging.

---

## Core Concept: Message-Centric vs Code-Centric

### Traditional Omniscient Debugging (Code-Centric)
```
Frame 1: ExecuteMethod(GameManager.Start)
Frame 2: ExecuteMethod(MmRelayNode.MmInvoke)
Frame 3: ExecuteMethod(MmRoutingTable.GetItems)
Frame 4: ExecuteMethod(MmBaseResponder.MmInvoke)
...
```
- Focus: Which code lines executed
- Granularity: Statement-level
- Question: "What code ran?"

### Mercury Time Travel Debugging (Message-Centric)
```
Frame 1: Message(Initialize) → GameManager
         ├── PASSED: Level=SelfAndChildren ✓
         ├── PASSED: Active=Active ✓
         └── Reached: PlayerController, UIManager, AudioManager

Frame 2: Message(SetActive) → PlayerController
         ├── PASSED: Level=Child ✓
         ├── REJECTED: Tag mismatch (expected Tag0, found Tag1)
         └── Reached: 0 responders (all rejected)
```
- Focus: Which messages flowed where
- Granularity: Message-level
- Question: "Why didn't this message reach that component?"

---

## Recording Architecture

### Hook Point: MmRelayNode.MmInvoke

```csharp
public class MmRelayNode : MonoBehaviour, IMmResponder
{
    public override void MmInvoke(MmMessage message)
    {
        // Hook point for recording
        if (MmMessageRecorder.IsRecording)
        {
            MmMessageRecorder.BeginMessage(this, message);
        }

        // Existing routing logic...
        var items = GetMmRoutingTableItems(message.MetadataBlock);

        // Record each routing decision
        foreach (var item in items)
        {
            var decision = EvaluateFilters(item, message);
            if (MmMessageRecorder.IsRecording)
            {
                MmMessageRecorder.RecordDecision(item, decision);
            }

            if (decision.Passed)
            {
                item.Responder.MmInvoke(message);
            }
        }

        if (MmMessageRecorder.IsRecording)
        {
            MmMessageRecorder.EndMessage();
        }
    }
}
```

### Filter Decision Recording

```csharp
public struct FilterDecision
{
    public MmResponder Responder;
    public bool LevelFilterPassed;
    public bool ActiveFilterPassed;
    public bool SelectedFilterPassed;
    public bool TagFilterPassed;
    public bool NetworkFilterPassed;
    public string RejectionReason;

    public bool Passed =>
        LevelFilterPassed &&
        ActiveFilterPassed &&
        SelectedFilterPassed &&
        TagFilterPassed &&
        NetworkFilterPassed;

    public string GetRejectionReason()
    {
        if (!LevelFilterPassed) return "Level filter: not in target hierarchy";
        if (!ActiveFilterPassed) return "Active filter: GameObject inactive";
        if (!SelectedFilterPassed) return "Selected filter: not current FSM state";
        if (!TagFilterPassed) return $"Tag filter: expected {ExpectedTag}, found {ActualTag}";
        if (!NetworkFilterPassed) return "Network filter: local/network mismatch";
        return "Unknown";
    }
}
```

---

## Memory Management

### Circular Buffer for Bounded Memory

```csharp
public class MmMessageRecorder
{
    private static CircularBuffer<RecordedMessage> _timeline;
    private const int DEFAULT_CAPACITY = 10000;

    public static void Initialize(int capacity = DEFAULT_CAPACITY)
    {
        _timeline = new CircularBuffer<RecordedMessage>(capacity);
    }

    public static void Record(RecordedMessage message)
    {
        _timeline.Add(message); // Oldest messages auto-evicted
    }
}
```

### Memory Estimates
- RecordedMessage: ~200 bytes base + routing decisions
- 10,000 messages × 500 bytes average = ~5 MB
- With rejection details: ~10 MB maximum
- Acceptable for development builds

---

## Timeline UI Design

### Main Window Layout

```
┌─────────────────────────────────────────────────────────────────┐
│ Mercury Time Travel Debugger                              [X]   │
├─────────────────────────────────────────────────────────────────┤
│ Timeline: [<<] [<] [|>] [>] [>>]    Frame: [142] / 1000        │
│ ════════════════════╪═════════════════════════════════════════  │
│                     ↑                                            │
│              Current Position                                    │
├─────────────────────────────────────────────────────────────────┤
│ Message Details                                                  │
│ ─────────────────────────────────────────────────────────────── │
│ Frame: 142                                                       │
│ Time: 2.34s                                                      │
│ Method: MmMethod.Initialize                                      │
│ Source: GameManager (MmRelayNode)                               │
│ Metadata: Level=SelfAndChildren, Active=Active, Tag=Everything  │
│                                                                  │
│ Routing Results:                                                 │
│ ├── ✓ PlayerController (reached)                                │
│ ├── ✓ UIManager (reached)                                       │
│ ├── ✓ AudioManager (reached)                                    │
│ ├── ✗ EnemySpawner (rejected: Inactive)                        │
│ └── ✗ DebugOverlay (rejected: Tag mismatch)                    │
├─────────────────────────────────────────────────────────────────┤
│ Query: [Why didn't message reach...] [EnemySpawner    ] [Find]  │
└─────────────────────────────────────────────────────────────────┘
```

### Visual Indicators

| Symbol | Meaning |
|--------|---------|
| ✓ | Message reached responder |
| ✗ | Message rejected by filter |
| → | Message forwarded to child |
| ← | Message forwarded to parent |
| ⚡ | Message caused state change |

---

## Query System Design

### Query Builder API

```csharp
public class MessageQuery
{
    private MmMethod? _method;
    private string _targetName;
    private int? _startFrame;
    private int? _endFrame;
    private bool _rejectedOnly;

    public MessageQuery Method(MmMethod method) { _method = method; return this; }
    public MessageQuery Target(string name) { _targetName = name; return this; }
    public MessageQuery InFrameRange(int start, int end) { ... return this; }
    public MessageQuery RejectedOnly() { _rejectedOnly = true; return this; }

    public QueryResult Execute()
    {
        // Search timeline for matching messages
        return new QueryResult(matchingMessages, analysisReport);
    }
}
```

### Common Queries

1. **"Why didn't Initialize reach EnemySpawner?"**
   ```csharp
   Recorder.Query()
       .Method(MmMethod.Initialize)
       .Target("EnemySpawner")
       .RejectedOnly()
       .Execute();
   ```

2. **"What messages did PlayerController receive?"**
   ```csharp
   Recorder.Query()
       .Target("PlayerController")
       .Execute();
   ```

3. **"Which messages were rejected by Tag filter?"**
   ```csharp
   Recorder.Query()
       .RejectedBy(FilterType.Tag)
       .Execute();
   ```

---

## Integration Points

### Visual Composer Integration (Future)
- Highlight message path in graph when scrubbing timeline
- Show rejection indicators on graph edges
- Click graph node to query its message history

### Unity Console Integration
- Log query results to Console
- Hyperlink to source relay node
- Export to file for bug reports

### Performance Mode
- Disable recording in production builds
- Optional: Record only errors/rejections
- Configurable buffer size

---

## Performance Considerations

### Recording Overhead
- Target: <5% frame time increase when recording
- Strategy: Minimize allocations in hot path
- Use struct-based recording, pooled lists

### Memory Bounds
- Circular buffer prevents unbounded growth
- Configurable capacity (default: 10,000 messages)
- Clear option for long sessions

### UI Responsiveness
- Timeline scrubbing at 60fps
- Async query execution for large histories
- Virtualized list for long result sets

---

*Last Updated: 2025-12-17*
