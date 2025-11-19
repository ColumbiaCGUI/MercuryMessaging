# Quick Win Validation Scene Setup Instructions

## Overview

This document provides step-by-step instructions for creating a Unity scene to validate the Priority 1 Quick Win implementations (QW-1, QW-2, QW-4).

**Scene Name**: `QuickWinValidation.unity`
**Location**: `Assets/MercuryMessaging/Examples/Demo/`

---

## Scene Hierarchy

Create the following GameObject hierarchy in Unity:

```
QuickWinValidation (Scene)
├── TestManager (Empty GameObject)
│   ├── TestManagerScript.cs
│   ├── QuickWinConfigHelper.cs
│   └── Test Components (Children):
│       ├── CircularBufferTest (GameObject)
│       │   └── CircularBufferMemoryTest.cs
│       ├── HopLimitTestObj (GameObject)
│       │   └── HopLimitTest.cs
│       ├── CycleDetectionTestObj (GameObject)
│       │   └── CycleDetectionTest.cs
│       └── LazyCopyTestObj (GameObject)
│           └── LazyCopyPerformanceTest.cs
│
├── Canvas (UI Canvas)
│   ├── Canvas Scaler (Scale with Screen Size, Reference Resolution 1920x1080)
│   ├── Background Panel
│   │   └── Title Text: "Mercury Messaging - Quick Win Validation"
│   ├── Control Panel (Left side)
│   │   ├── Run All Tests Button
│   │   ├── Run QW-4 Button (CircularBuffer)
│   │   ├── Run QW-1 Hop Button
│   │   ├── Run QW-1 Cycle Button
│   │   └── Run QW-2 Button (Lazy Copy)
│   ├── Results Panel (Center)
│   │   ├── QW-4 Result Text
│   │   ├── QW-1 Hop Result Text
│   │   ├── QW-1 Cycle Result Text
│   │   └── QW-2 Result Text
│   ├── Memory Panel (Right side)
│   │   ├── Current Memory Text
│   │   └── Peak Memory Text
│   └── Log Panel (Bottom)
│       ├── Scroll View
│       │   └── Log Text (TextMeshProUGUI)
│       └── Scrollbar
│
└── EventSystem (UI Event System)
```

---

## Step-by-Step Setup

### 1. Create New Scene

1. In Unity, go to `File > New Scene`
2. Choose "Basic (Built-In)" or "Basic (URP)" depending on your render pipeline
3. Save as `Assets/MercuryMessaging/Examples/Demo/QuickWinValidation.unity`

### 2. Create TestManager GameObject

1. Create empty GameObject: `GameObject > Create Empty`
2. Name it "TestManager"
3. Add components:
   - `Add Component > TestManagerScript`
   - `Add Component > QuickWinConfigHelper`

### 3. Create Test Component GameObjects

Create four child GameObjects under TestManager:

**CircularBufferTest:**
```
1. Right-click TestManager > Create Empty
2. Name: "CircularBufferTest"
3. Add Component > CircularBufferMemoryTest
```

**HopLimitTestObj:**
```
1. Right-click TestManager > Create Empty
2. Name: "HopLimitTestObj"
3. Add Component > HopLimitTest
```

**CycleDetectionTestObj:**
```
1. Right-click TestManager > Create Empty
2. Name: "CycleDetectionTestObj"
3. Add Component > CycleDetectionTest
```

**LazyCopyTestObj:**
```
1. Right-click TestManager > Create Empty
2. Name: "LazyCopyTestObj"
3. Add Component > LazyCopyPerformanceTest
```

### 4. Create UI Canvas

1. Right-click in Hierarchy > `UI > Canvas`
2. Configure Canvas:
   - Render Mode: Screen Space - Overlay
   - Add Canvas Scaler component:
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920 x 1080
     - Match: 0.5 (Width/Height)

### 5. Create Background Panel

1. Right-click Canvas > `UI > Panel`
2. Name: "BackgroundPanel"
3. Configure RectTransform:
   - Anchor: Stretch/Stretch
   - Left: 0, Top: 0, Right: 0, Bottom: 0
4. Configure Image:
   - Color: Dark gray (R:40, G:40, B:40, A:255)

### 6. Create Title Text

1. Right-click BackgroundPanel > `UI > Text - TextMeshPro`
2. Name: "TitleText"
3. Configure RectTransform:
   - Anchor: Top/Center
   - Pos X: 0, Pos Y: -40
   - Width: 800, Height: 60
4. Configure TextMeshProUGUI:
   - Text: "Mercury Messaging - Quick Win Validation"
   - Font Size: 36
   - Alignment: Center/Middle
   - Color: White

### 7. Create Control Panel

1. Right-click Canvas > `UI > Panel`
2. Name: "ControlPanel"
3. Configure RectTransform:
   - Anchor: Left/Stretch
   - Pos X: 150, Pos Y: 0
   - Width: 250, Height: Full screen offset (-100 top, -20 bottom)
4. Add Vertical Layout Group:
   - Child Alignment: Upper Center
   - Spacing: 10
   - Padding: 10 all sides

### 8. Create Buttons (5 buttons)

For each button, right-click ControlPanel > `UI > Button - TextMeshPro`:

**Run All Tests Button:**
- Name: "RunAllTestsButton"
- Text: "Run All Tests"
- Colors: Normal (Green), Highlighted (Light Green), Pressed (Dark Green)

**Run QW-4 Button:**
- Name: "RunQW4Button"
- Text: "QW-4: CircularBuffer"

**Run QW-1 Hop Button:**
- Name: "RunQW1HopButton"
- Text: "QW-1: Hop Limit"

**Run QW-1 Cycle Button:**
- Name: "RunQW1CycleButton"
- Text: "QW-1: Cycle Detection"

**Run QW-2 Button:**
- Name: "RunQW2Button"
- Text: "QW-2: Lazy Copy"

### 9. Create Results Panel

1. Right-click Canvas > `UI > Panel`
2. Name: "ResultsPanel"
3. Configure RectTransform:
   - Anchor: Center/Stretch
   - Pos X: 0, Pos Y: 100
   - Width: 800, Height: 400
4. Add Vertical Layout Group:
   - Spacing: 15
   - Padding: 20 all sides

### 10. Create Result Text Fields (4 fields)

For each result field, right-click ResultsPanel > `UI > Text - TextMeshPro`:

**QW-4 Result:**
- Name: "QW4ResultText"
- Text: "Pending..."
- Color: Gray
- Font Size: 24

**QW-1 Hop Result:**
- Name: "QW1HopResultText"
- Text: "Pending..."
- Color: Gray
- Font Size: 24

**QW-1 Cycle Result:**
- Name: "QW1CycleResultText"
- Text: "Pending..."
- Color: Gray
- Font Size: 24

**QW-2 Result:**
- Name: "QW2ResultText"
- Text: "Pending..."
- Color: Gray
- Font Size: 24

### 11. Create Memory Panel

1. Right-click Canvas > `UI > Panel`
2. Name: "MemoryPanel"
3. Configure RectTransform:
   - Anchor: Right/Top
   - Pos X: -150, Pos Y: -120
   - Width: 250, Height: 150
4. Add Vertical Layout Group:
   - Spacing: 10
   - Padding: 15 all sides

### 12. Create Memory Text Fields

**Current Memory Text:**
- Right-click MemoryPanel > `UI > Text - TextMeshPro`
- Name: "CurrentMemoryText"
- Text: "Current: 0 MB"
- Font Size: 18

**Peak Memory Text:**
- Right-click MemoryPanel > `UI > Text - TextMeshPro`
- Name: "PeakMemoryText"
- Text: "Peak: 0 MB"
- Font Size: 18

### 13. Create Log Panel

1. Right-click Canvas > `UI > Panel`
2. Name: "LogPanel"
3. Configure RectTransform:
   - Anchor: Bottom/Stretch
   - Pos Y: 100
   - Height: 200
   - Left: 20, Right: 20, Bottom: 20

### 14. Create Scroll View

1. Right-click LogPanel > `UI > Scroll View`
2. Name: "LogScrollView"
3. Configure:
   - Remove Horizontal Scrollbar
   - Keep only Vertical Scrollbar
4. In Viewport > Content:
   - Add Content Size Fitter:
     - Vertical Fit: Preferred Size

### 15. Create Log Text

1. Right-click Content > `UI > Text - TextMeshPro`
2. Name: "LogText"
3. Configure:
   - Alignment: Top/Left
   - Font Size: 14
   - Color: Light Gray
   - Enable Rich Text
   - Wrapping: Enabled

---

## Component Wiring

### TestManagerScript Configuration

Select TestManager GameObject and wire up the TestManagerScript component:

**Button References:**
- Run All Tests Button → RunAllTestsButton
- Run QW4 Button → RunQW4Button
- Run QW1 Hop Button → RunQW1HopButton
- Run QW1 Cycle Button → RunQW1CycleButton
- Run QW2 Button → RunQW2Button

**Test Component References:**
- Circular Buffer Test → CircularBufferTest
- Hop Limit Test → HopLimitTestObj
- Cycle Detection Test → CycleDetectionTestObj
- Lazy Copy Test → LazyCopyTestObj

**Display Component Reference:**
- Result Display → TestResultDisplay component (created in next step)

### Create TestResultDisplay Component

1. Select TestManager GameObject
2. Add Component > TestResultDisplay
3. Wire up references:

**Result Text Fields:**
- QW4 Result → QW4ResultText
- QW1 Hop Result → QW1HopResultText
- QW1 Cycle Result → QW1CycleResultText
- QW2 Result → QW2ResultText

**Memory Display:**
- Current Memory Text → CurrentMemoryText
- Peak Memory Text → PeakMemoryText

**Log Display:**
- Log Text → LogText
- Log Scroll Rect → LogScrollView

**Colors:**
- Pass Color → Green (0, 255, 0)
- Fail Color → Red (255, 0, 0)

### Button Event Configuration

For each button, configure the OnClick() event:

**RunAllTestsButton:**
- Add event: TestManager > TestManagerScript > RunAllTests()

**RunQW4Button:**
- Add event: TestManager > TestManagerScript > RunQW4Test()

**RunQW1HopButton:**
- Add event: TestManager > TestManagerScript > RunQW1HopTest()

**RunQW1CycleButton:**
- Add event: TestManager > TestManagerScript > RunQW1CycleTest()

**RunQW2Button:**
- Add event: TestManager > TestManagerScript > RunQW2Test()

---

## QuickWinConfigHelper Configuration

Select TestManager and configure the QuickWinConfigHelper component with test parameters:

**CircularBuffer Test:**
- Circular Buffer Message Count: 10000
- Expected Buffer Size: 100
- Max Memory Delta MB: 10

**Hop Limit Test:**
- Hop Limit Chain Depth: 50
- Max Hops: 25

**Cycle Detection Test:**
- Enable Cycle Detection: True (checked)

**Lazy Copy Test:**
- Lazy Copy Iterations: 1000
- Expected Improvement: 20%

---

## Running the Tests

1. Save the scene
2. Enter Play Mode
3. Click "Run All Tests" or individual test buttons
4. Observe results in the Results Panel:
   - Green text = PASS
   - Red text = FAIL
   - Gray text = Pending/Running
5. Check log output in the Log Panel for detailed information
6. Monitor memory usage in the Memory Panel

---

## Expected Results

**QW-4: CircularBuffer Memory Test**
- Should show: "PASS - Buffer stable at 100 items"
- Memory growth should be minimal (<10 MB)
- Confirms circular buffer prevents unbounded growth

**QW-1: Hop Limit Test**
- Should show: "PASS - Message stopped at hop 25/50"
- Message should not reach the end of the 50-node chain
- Confirms hop limit protection works

**QW-1: Cycle Detection Test**
- Should show: "PASS - Visited 3 nodes, no infinite loop"
- No freezing or infinite loop
- Confirms cycle detection prevents circular routing

**QW-2: Lazy Copy Performance Test**
- Should show: "PASS - Routing scenarios tested with 1000 messages each"
- No errors during execution
- Confirms lazy copying works correctly

---

## Troubleshooting

**Issue: Buttons don't respond**
- Check that EventSystem exists in the scene
- Verify button OnClick() events are properly wired

**Issue: No results displayed**
- Check TestResultDisplay component is attached to TestManager
- Verify all text field references are properly assigned

**Issue: Tests don't run**
- Check that test component GameObjects are children of TestManager
- Verify QuickWinConfigHelper is applying configuration in Awake()

**Issue: Memory values show 0**
- This is normal if tests haven't run yet
- Memory updates every second (60 frames)

**Issue: Log panel doesn't scroll**
- Check that Content has Content Size Fitter component
- Verify LogScrollRect reference is assigned

---

## Next Steps

After validating tests pass:

1. **Commit the scene file**:
   ```bash
   git add Assets/MercuryMessaging/Examples/Demo/QuickWinValidation.unity
   git add Assets/MercuryMessaging/Examples/Demo/QuickWinValidation.unity.meta
   git commit -m "feat: Add Quick Win validation scene

   Complete Unity scene for testing QW-1, QW-2, and QW-4 implementations.
   Includes UI for running individual tests and viewing results."
   ```

2. **Test with different configurations**:
   - Adjust QuickWinConfigHelper parameters
   - Increase message counts to stress test
   - Modify hop limits and chain depths

3. **Document results**:
   - Take screenshots of passing tests
   - Record any performance observations
   - Note any edge cases discovered

4. **Continue with remaining Quick Wins**:
   - QW-3: Parent lookup caching
   - QW-5: Tag checking optimization
   - QW-6: Level filter optimization

---

## File References

**Test Scripts:**
- `Assets/_Project/Scripts/Testing/TestManagerScript.cs`
- `Assets/_Project/Scripts/Testing/CircularBufferMemoryTest.cs`
- `Assets/_Project/Scripts/Testing/HopLimitTest.cs`
- `Assets/_Project/Scripts/Testing/CycleDetectionTest.cs`
- `Assets/_Project/Scripts/Testing/LazyCopyPerformanceTest.cs`
- `Assets/_Project/Scripts/Testing/QuickWinConfigHelper.cs`
- `Assets/_Project/Scripts/Testing/TestResultDisplay.cs`

**Implementation Files:**
- `Assets/MercuryMessaging/Protocol/MmRelayNode.cs`
- `Assets/MercuryMessaging/Protocol/Message/MmMessage.cs`
- `Assets/MercuryMessaging/Support/Data/CircularBuffer.cs`

**Documentation:**
- `CLAUDE.md` - Project guidelines (updated with AI attribution warning)
- `dev/active/framework-analysis/framework-analysis-context.md` - Full implementation context

---

*Created: 2025-11-18*
*Scene Location: Assets/MercuryMessaging/Examples/Demo/QuickWinValidation.unity*
