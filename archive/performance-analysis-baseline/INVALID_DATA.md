# INVALID DATA WARNING

**Date Identified:** 2025-11-28
**Status:** INVALID - Do not use for analysis

---

## Issue

The performance data in this folder is **INVALID** due to a message generator bug.

### Problem
- Throughput was capped at ~30 msg/sec regardless of target
- All three scales (Small, Medium, Large) showed nearly identical throughput
- Expected: 100 / 500 / 1000 msg/sec
- Actual: ~30 / ~30 / ~28 msg/sec

### Root Cause
The `MessageGenerator.cs` was using coroutine-based `WaitForSeconds` which artificially limited message generation rate to approximately 30 messages per second.

### Evidence
```
SmallScale:  throughput = 30.50 msg/sec (expected: 100)
MediumScale: throughput = 29.81 msg/sec (expected: 500)
LargeScale:  throughput = 27.93 msg/sec (expected: 1000)
```

### Fix Applied
Frame-based message accumulator replaced coroutine in `MessageGenerator.cs`.

---

## Valid Data Location

Use data from `dev/performance-results/` (2025-11-28) or `dev/archive/performance-analysis-final/` instead.

---

## What This Data CAN Be Used For

- Historical reference only
- Demonstrating the bug's characteristics
- NOT for performance comparisons or analysis
