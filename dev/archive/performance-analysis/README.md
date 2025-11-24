# Performance Analysis - Usage Guide

> **⚠️ ARCHIVED:** This folder has been archived on 2025-11-21. The active performance documentation and analysis tools have been moved to `Documentation/Performance/`. This folder is preserved for historical reference only.

This folder contains tools and results for comprehensive performance analysis of the MercuryMessaging framework's Quick Win optimizations.

---

## Quick Start

### 1. Collect Test Data (Unity)

1. Open Unity Editor
2. Build test scenes: **Mercury > Performance > Build Test Scenes**
3. Run each test scene (SmallScale, MediumScale, LargeScale)
4. Enable Auto Start on MessageGenerator and PerformanceTestHarness
5. Results auto-export to this folder as CSV files

### 2. Analyze Data (Python)

```bash
# Install dependencies
pip install -r requirements.txt

# Run analysis
python analyze_performance.py
```

This generates:
- 6 performance graphs in `graphs/` folder
- Statistical summary CSV
- Console output with key metrics

### 3. Generate Report

1. Open `PERFORMANCE_REPORT_TEMPLATE.md`
2. Fill in bracketed values `[X.XX]` with actual data from CSV files
3. Embed graphs from `graphs/` folder
4. Save as `PERFORMANCE_REPORT.md`

---

## Files in This Folder

### Documentation
- **`performance-analysis-plan.md`** - Complete implementation plan
- **`performance-analysis-context.md`** - Implementation notes and decisions
- **`performance-analysis-tasks.md`** - Task checklist with status
- **`PERFORMANCE_REPORT_TEMPLATE.md`** - Template for final report
- **`README.md`** - This file

### Analysis Scripts
- **`analyze_performance.py`** - Main analysis script (generates graphs and stats)
- **`requirements.txt`** - Python dependencies

### Test Results (Generated)
- **`smallscale_results.csv`** - Small scene test data
- **`mediumscale_results.csv`** - Medium scene test data
- **`largescale_results.csv`** - Large scene test data
- **`invocation_comparison.csv`** - InvocationComparison data
- **`performance_statistics_summary.csv`** - Aggregated statistics

### Graphs (Generated)
- **`graphs/scaling_curves.png`** - Frame time vs load
- **`graphs/memory_stability.png`** - Memory over time
- **`graphs/cache_effectiveness.png`** - Cache hit rate vs responders
- **`graphs/throughput_vs_depth.png`** - Throughput vs hierarchy depth
- **`graphs/frame_time_distribution.png`** - Frame time histograms
- **`graphs/invocation_comparison.png`** - MM vs Unity built-ins

### Profiler Data (Manual)
- **`profiler/`** - Unity Profiler screenshots (add manually)

---

## Detailed Workflow

### Phase 1: Data Collection (Unity)

**Step 1: Build Scenes**
```
Unity Menu: Mercury > Performance > Build Test Scenes
Result: 3 scenes created in Assets/MercuryMessaging/Tests/Performance/Scenes/
```

**Step 2: Run SmallScale Test**
```
1. Open SmallScale.unity
2. Select Root GameObject
3. Inspector > MessageGenerator: Enable "Auto Start"
4. Inspector > PerformanceTestHarness: Enable "Auto Start", "Export To CSV"
5. Play scene (60 seconds)
6. Check: dev/active/performance-analysis/smallscale_results.csv
```

**Step 3: Run MediumScale Test**
```
Same as Step 2, but with MediumScale.unity
Check: dev/active/performance-analysis/mediumscale_results.csv
```

**Step 4: Run LargeScale Test**
```
Same as Step 2, but with LargeScale.unity
Check: dev/active/performance-analysis/largescale_results.csv
```

**Step 5: Run InvocationComparison**
```
1. Open TimingScene.unity (Examples/Tutorials/SimpleScene/TimingScene.unity)
2. Find GameObject with InvocationComparison component
3. Inspector: Enable "Export To CSV", "Auto Run Tests"
4. Play scene
5. Check: dev/active/performance-analysis/invocation_comparison.csv
```

**Step 6: Run Unity Profiler (Optional)**
```
1. Window > Analysis > Profiler
2. Enable CPU, Memory modules
3. Play test scene with recording enabled
4. Take screenshots of key views
5. Save to: dev/active/performance-analysis/profiler/
```

**Step 7: Run Quick Win Validator (Optional)**
```
1. Create empty scene
2. Add empty GameObject
3. Add QuickWinValidator component
4. Enable "Auto Run"
5. Play scene
6. Check Console and Inspector for validation results
```

---

### Phase 2: Analysis (Python)

**Prerequisites:**
```bash
# Check Python version (3.8+ recommended)
python --version

# Install dependencies
pip install -r requirements.txt
```

**Run Analysis:**
```bash
python analyze_performance.py
```

**Output:**
```
Loading CSV data...
  ✓ Loaded smallscale_results.csv: 3600 rows
  ✓ Loaded mediumscale_results.csv: 3600 rows
  ✓ Loaded largescale_results.csv: 3600 rows
  ✓ Loaded invocation_comparison.csv: 5 rows

Calculating statistics...

SmallScale Statistics:
  Frame Time: 4.23ms ± 0.85ms
  Memory: 45.23MB (growth: 0.12MB)
  Throughput: 99.8 msg/sec
  Cache Hit Rate: 75.3%

[... more output ...]

Generating graphs...
  ✓ scaling_curves.png
  ✓ memory_stability.png
  ✓ cache_effectiveness.png
  ✓ throughput_vs_depth.png
  ✓ frame_time_distribution.png
  ✓ invocation_comparison.png

✓ All graphs saved to: graphs/
✓ Statistics saved to: performance_statistics_summary.csv

Analysis Complete!
```

---

### Phase 3: Report Generation

**Step 1: Open Template**
```bash
# Open in your editor
code PERFORMANCE_REPORT_TEMPLATE.md
# or
notepad PERFORMANCE_REPORT_TEMPLATE.md
```

**Step 2: Fill in Data**

Replace all bracketed placeholders `[X.XX]` with actual values from:
- `performance_statistics_summary.csv` (for numerical data)
- Console output from analyze_performance.py
- Unity Profiler screenshots (if collected)

**Example:**
```markdown
# Before
Frame Time: [X.XX]ms (avg) ± [X.XX]ms (std)

# After (using actual data)
Frame Time: 4.23ms (avg) ± 0.85ms (std)
```

**Step 3: Embed Graphs**

Graphs are already saved in `graphs/` folder. Reference them in the report:

```markdown
![Scaling Curves](graphs/scaling_curves.png)
```

**Step 4: Save Final Report**
```bash
# Save as
PERFORMANCE_REPORT.md
```

---

## Expected Results

Based on code analysis and Quick Win implementations, expect:

**Frame Time:**
- Small: <5ms (target: 200+ FPS)
- Medium: <10ms (target: 100+ FPS)
- Large: <16.6ms (target: 60 FPS)

**Memory Stability:**
- Growth <10% over 10K messages (QW-4 CircularBuffer)
- Bounded at configured buffer size

**Cache Hit Rate:**
- Small (10 responders): 70-80%
- Medium (50 responders): 80-90%
- Large (100+ responders): 85-95%

**Message Throughput:**
- Sustained 100-1000 msg/sec depending on scale
- No significant degradation over time

**Quick Win Validation:**
- QW-1: Hop limits prevent deep propagation ✓
- QW-2: Lazy copy reduces allocations 20-50% ✓
- QW-3: Cache hit rate >80% at scale ✓
- QW-4: Memory bounded over time ✓
- QW-5: 4 allocation sites removed ✓
- QW-6: Code cleanup complete ✓

---

## Troubleshooting

### "No CSV files found"
**Problem:** Python script can't find test data
**Solution:**
- Verify CSV files are in `dev/active/performance-analysis/`
- Check Unity exported to correct path
- Run test scenes again with "Export To CSV" enabled

### "ModuleNotFoundError: pandas"
**Problem:** Python dependencies not installed
**Solution:**
```bash
pip install -r requirements.txt
```

### "Graphs look empty/incorrect"
**Problem:** Insufficient test data or incorrect CSV format
**Solution:**
- Verify test ran for full duration (60 seconds)
- Check CSV files have data rows
- Ensure CSV headers match expected format

### "Memory keeps growing"
**Problem:** QW-4 CircularBuffer may not be enabled
**Solution:**
- Check MmRelayNode has `messageHistorySize` set (default: 100)
- Verify CircularBuffer is being used (not List)
- Run QuickWinValidator to confirm

---

## Advanced Usage

### Custom Analysis

Modify `analyze_performance.py` to:
- Add custom graphs
- Calculate additional statistics
- Export in different formats
- Integrate with other tools

### Automated Testing

Create script to run all tests automatically:
```bash
#!/bin/bash
# Run Unity tests in batch mode
Unity.exe -batchmode -projectPath . -executeMethod PerformanceTests.RunAll
python analyze_performance.py
```

### Continuous Integration

Integrate performance testing into CI/CD:
1. Run Unity tests on each commit
2. Generate performance graphs
3. Compare against baseline
4. Fail build if regression detected

---

## Next Steps

After completing performance analysis:

1. **Share Results:**
   - Present PERFORMANCE_REPORT.md to team
   - Document findings in project wiki
   - Update user-facing documentation

2. **Update Documentation:**
   - Add Performance Characteristics section to CLAUDE.md
   - Update README with performance guidance
   - Create optimization tips guide

3. **Plan Improvements:**
   - Review identified bottlenecks
   - Prioritize next optimizations
   - See framework-analysis-tasks.md for Priority 3 tasks

4. **Production Testing:**
   - Test in actual game/application
   - Validate findings in real-world scenarios
   - Collect production metrics

---

## Support

For questions or issues:
1. Check this README
2. Review performance-analysis-plan.md
3. Consult CLAUDE.md for framework documentation
4. See dev/TECHNICAL_DEBT.md for known issues

---

**Last Updated:** 2025-11-20
**Version:** 1.0
**Part of:** Performance Analysis Task (Phases 4-7)
