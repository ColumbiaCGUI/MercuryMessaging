# INVALID DATA WARNING

**Date Identified:** 2025-11-28
**Status:** INVALID - Do not use for analysis

---

## Issue

The performance data in this folder is **INVALID** due to improperly populated routing tables.

### Problem
- LargeScale showed **faster** frame times than SmallScale (physically impossible)
- Routing tables only contained 1-5 items instead of expected 10/50/100+
- Performance appeared artificially good because messages weren't being routed to all responders

### Evidence
```
SmallScale:  frame_time = 4.25ms (expected: higher than LargeScale)
MediumScale: frame_time = 4.84ms
LargeScale:  frame_time = 3.66ms (IMPOSSIBLE - should be slowest!)
```

### Root Cause
1. `PerformanceSceneBuilder.cs` had incorrect scene paths (missing "Framework/" prefix)
2. `EditorUtility.SetDirty()` was not being called after modifying routing tables
3. MmRelayNode constructor creates empty RoutingTable, overwriting editor values

### Fix Applied
1. Fixed scene paths in PerformanceSceneBuilder.cs
2. Added EditorUtility.SetDirty() calls to RefreshHierarchy method
3. Scenes rebuilt with proper routing table population

---

## Validation Criteria

Valid performance data MUST satisfy:
1. **Large > Small frame time** (more responders = more work)
2. **Throughput matches targets** (100/500/1000 msg/sec)
3. **Memory remains stable** (no unbounded growth)

---

## Valid Data Location

Use data from `dev/performance-results/` (2025-11-28) instead.

Validated results:
| Scale | Frame Time | FPS | Throughput |
|-------|------------|-----|------------|
| Small | 14.54ms | 68.8 | 100 msg/sec |
| Medium | 14.29ms | 70.0 | 500 msg/sec |
| Large | 17.17ms | 58.3 | 1000 msg/sec |

---

## What This Data CAN Be Used For

- Historical reference only
- Understanding the bug's characteristics
- NOT for performance comparisons or analysis
