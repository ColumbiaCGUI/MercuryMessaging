# Performance Testing Framework

This folder contains automated performance testing infrastructure for the MercuryMessaging framework.

---

## Quick Start

### Step 1: Build Test Scenes

1. Open Unity Editor
2. Go to **Mercury > Performance > Build Test Scenes**
3. Click **"Build Scenes"** button
4. Three scenes will be created in `Assets/MercuryMessaging/Tests/Performance/Scenes/`

### Step 2: Run Performance Tests

1. Open one of the test scenes:
   - `SmallScale.unity` - 10 responders, 3 levels, 100 msg/sec
   - `MediumScale.unity` - 50 responders, 5 levels, 500 msg/sec
   - `LargeScale.unity` - 100+ responders, 7-10 levels, 1000 msg/sec

2. In the Hierarchy, select the **Root** GameObject

3. In the Inspector:
   - **MessageGenerator** component: Click "Start" button (or enable Auto Start)
   - **PerformanceTestHarness** component: Click "Start Test" button (or enable Auto Start)

4. Watch the real-time metrics display on screen

5. Results automatically export to:
   - `dev/performance-results/*.csv`

---

## Components

### PerformanceTestHarness.cs

Main test orchestration component.

**Configuration:**
- `Test Scenario`: Small/Medium/Large (applies defaults)
- `Responder Count`: Number of responders (overrides scenario)
- `Hierarchy Depth`: Depth of hierarchy (overrides scenario)
- `Message Volume`: Messages per second (overrides scenario)
- `Test Duration`: How long to run test (seconds)
- `Auto Start`: Start test automatically on scene load
- `Export To CSV`: Enable CSV export
- `Export Path`: CSV file path (relative to project dev/ folder)
- `Export To Dev Folder`: Also export to dev/performance-results/

**Metrics Tracked:**
- Frame time (ms) - avg, min, max
- Memory usage (bytes) - via GC.GetTotalMemory()
- Message throughput (messages/second)
- Cache hit rate (0.0-1.0) - from MmRoutingTable
- Hop count (average) - from MmMessage.HopCount
- Messages sent (total count)

**CSV Output Format:**
```csv
timestamp,frame_time_ms,memory_bytes,memory_mb,throughput_msg_sec,cache_hit_rate,avg_hop_count,messages_sent
```

### MessageGenerator.cs

Generates message load at specified rate.

**Configuration:**
- `Messages Per Second`: Rate to generate messages (1-1000)
- `Message Method`: Type of message to send
- `Level Filter`: Direction (Self/Child/Parent/etc)
- `Active Filter`: Active vs All GameObjects
- `Tag`: Tag for messages
- `Auto Start`: Start generating on scene load

**Usage:**
- Attach to GameObject with MmRelayNode
- Click "Start" button to begin generating
- Click "Stop" button to halt generation
- Check `Total Messages Sent` field for count

### TestResponder.cs

Receives and counts messages for validation.

**Configuration:**
- `Log Messages`: Enable console logging (creates overhead)
- `Count Messages`: Track message counts

**Statistics:**
- `Messages Received`: Total count
- Breakdown by message type (Initialize, Refresh, SetActive, String, Int, Float, Bool, Other)

**Context Menu Actions:**
- Right-click component > "Reset Counters"
- Right-click component > "Print Statistics" (logs to console)

---

## Test Scenes

### SmallScale.unity

**Purpose:** Test QW-2 (lazy copy) and QW-5 (LINQ removal) in simple case

**Specifications:**
- 10 responders
- 3 levels hierarchy depth
- 100 messages/second
- Single-direction routing (Child-only broadcasts)

**Expected Results:**
- Frame time: <5ms
- Message copies: 0 (lazy copy working)
- Memory stable

### MediumScale.unity

**Purpose:** Test QW-1 (hop limits), QW-3 (filter cache), QW-4 (circular buffer)

**Specifications:**
- 50 responders
- 5 levels hierarchy depth
- 500 messages/second
- Multi-direction routing (SelfAndBidirectional)
- Multiple tags (Tag0-Tag3) for cache testing

**Expected Results:**
- Frame time: <10ms
- Cache hit rate: 85%+
- Memory bounded over time
- Hop counts reasonable (<10 typical)

### LargeScale.unity

**Purpose:** Stress test all Quick Wins combined

**Specifications:**
- 100+ responders
- 7-10 levels hierarchy depth
- 1000 messages/second
- Complex mesh with potential cycles
- Multiple MmRelaySwitchNode (FSM testing)

**Expected Results:**
- Frame time: <16.6ms (60fps)
- Cache hit rate: 90%+
- Cycle detection prevents crashes
- Memory stable under load
- Throughput sustained

---

## Extended InvocationComparison

The existing `InvocationComparison.cs` has been extended with:

**New Features:**
- CSV export functionality
- Automated test mode (runs on Start if `autoRunTests` enabled)
- Exports to both Resources and dev folders

**Backward Compatibility:**
- Space bar still triggers tests (manual mode)
- All existing functionality preserved

**CSV Output Format:**
```csv
test_type,iterations,total_ms,avg_ms,total_ticks,avg_ticks
Control,1000,X,X,X,X
Mercury,1000,X,X,X,X
SendMessage,1000,X,X,X,X
UnityEvent,1000,X,X,X,X
Execute,1000,X,X,X,X
```

---

## Workflow

### Full Test Run

1. **Build scenes** (Mercury > Performance > Build Test Scenes)

2. **Run SmallScale test:**
   - Open SmallScale.unity
   - Enable Auto Start on both MessageGenerator and PerformanceTestHarness
   - Play scene
   - Wait 60 seconds
   - Check `dev/performance-results/smallscale_results.csv`

3. **Run MediumScale test:**
   - Open MediumScale.unity
   - Enable Auto Start on both components
   - Play scene
   - Wait 60 seconds
   - Check `dev/performance-results/mediumscale_results.csv`

4. **Run LargeScale test:**
   - Open LargeScale.unity
   - Enable Auto Start on both components
   - Play scene
   - Wait 60 seconds
   - Check `dev/performance-results/largescale_results.csv`

5. **Run InvocationComparison:**
   - Open SimpleScene.unity
   - Find GameObject with InvocationComparison component
   - Enable `exportToCSV` and `autoRunTests`
   - Play scene
   - Check `dev/performance-results/invocation_comparison.csv`

6. **Analyze results:**
   - All CSV files are in `dev/performance-results/`
   - Use Python/Excel to create graphs
   - Generate performance report

---

## Unity Profiler Integration

To get additional metrics:

1. Open Unity Profiler (Window > Analysis > Profiler)
2. Enable CPU, Memory, Rendering modules
3. Play test scene
4. Click "Record" button
5. Let test run
6. Stop recording
7. Take screenshots or export data
8. Save to `dev/performance-results/profiler/`

---

## Troubleshooting

### "No MmRelayNode found" warning
- Make sure test scenes were built correctly
- Root GameObject should have MmRelayNode component

### Messages not being received
- Check MessageGenerator has `relayNode` reference set
- Verify TestResponder components are attached to child objects
- Enable `logMessages` on TestResponder to see message flow

### CSV export failed
- Check `dev/performance-results/` folder exists
- Verify write permissions
- Check Unity Console for error messages

### Performance is poor
- Disable `logMessages` on TestResponder (creates overhead)
- Reduce `messageVolume` on MessageGenerator
- Check Unity Profiler for bottlenecks

### Memory keeps growing
- This is expected if QW-4 (CircularBuffer) is disabled
- Verify `messageHistorySize` is set on MmRelayNode
- Check CircularBuffer is being used (not List)

---

## File Structure

```
Assets/MercuryMessaging/Tests/Performance/
├── Editor/
│   └── PerformanceSceneBuilder.cs (Scene builder tool)
├── Scenes/
│   ├── SmallScale.unity (created by builder)
│   ├── MediumScale.unity (created by builder)
│   └── LargeScale.unity (created by builder)
├── Scripts/
│   ├── PerformanceTestHarness.cs (Main test orchestrator)
│   ├── MessageGenerator.cs (Message load generator)
│   └── TestResponder.cs (Message receiver)
└── README.md (this file)

Assets/Resources/performance-results/
└── *.csv (Test results - exported here)

dev/performance-results/
├── *.csv (Test results - also exported here)
├── profiler/ (Unity Profiler screenshots)
└── graphs/ (Performance graphs)
```

---

## Next Steps

After collecting test data:

1. **Generate Performance Report** (`Documentation/Performance/PERFORMANCE_REPORT.md`)
2. **Create Performance Graphs** (5 graphs showing scaling, memory, cache, etc.)
3. **Validate Quick Wins** (QW-1 through QW-5)
4. **Test Existing Scenes** (SimpleScene, TrafficLights, Tutorials)
5. **Update CLAUDE.md** (add Performance Characteristics section)

See `dev/archive/performance-analysis/` for historical analysis data.

---

**Version:** 1.0
**Last Updated:** 2025-11-20
**Part of:** Performance Analysis Task (Phase 3)
