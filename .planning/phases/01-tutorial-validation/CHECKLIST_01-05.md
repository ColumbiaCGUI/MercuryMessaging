# Tutorial 1-5 Validation Checklist

**Instructions:** Open each tutorial scene in Unity Editor, follow the steps below, and report PASS or FAIL per tutorial. For FAILs, describe the specific issue (which step, what happened vs what was expected).

---

## Tutorial 1: Introduction

- [ ] Open scene: `Assets/MercuryMessaging/Examples/Tutorials/Tutorial1/Tutorial1_Base.unity`
- [ ] Check console: zero errors, zero warnings on scene load
- [ ] Enter Play mode
- [ ] Check console: should show `[T1_Child] Initialized!` on start (BroadcastInitialize in T1_ParentController.Start)
- [ ] Press **I**: expect `[Parent] Sent BroadcastInitialize()` and `[T1_Child] Initialized!`
- [ ] Press **1**: expect `[T1_Child] Received string: Message from parent!` and `[Parent] Sent string via .Send().ToChildren().Execute()`
- [ ] Press **2**: expect `[T1_Child] Received int: 100` and `[Parent] Sent int via .Send().ToChildren().Execute()`
- [ ] Press **3**: expect `[T1_Child] Received int: 42` and `[Parent] Sent BroadcastValue(42) - auto-detects type`
- [ ] Press **R**: expect `[T1_Child] Refreshed` and `[Parent] Sent BroadcastRefresh()`
- [ ] Press **Space**: check for SetActive toggle or string message (depends on which controller is attached)
- [ ] Exit Play mode

**Result:** PASS / FAIL (describe issue if FAIL)

---

## Tutorial 2: Basic Routing

- [ ] Open scene: `Assets/MercuryMessaging/Examples/Tutorials/Tutorial2/Tutorial2_Base.unity`
- [ ] Check console: zero errors, zero warnings on scene load
- [ ] Enter Play mode
- [ ] Press **C**: expect `[Routing] Sent .ToChildren() - direct children only` and child responders log received string
- [ ] Press **P**: expect `[Routing] Sent .ToParents() - direct parents only`
- [ ] Press **D**: expect `[Routing] Sent .ToDescendants() - all children, grandchildren, etc.`
- [ ] Press **A**: expect `[Routing] Sent .ToAncestors() - all parents, grandparents, etc.`
- [ ] Press **S**: expect `[Routing] Sent .ToSiblings() - nodes sharing same parent`
- [ ] Press **F**: expect `[Routing] Sent .ToAll() - all connected nodes`
- [ ] Verify messages reach different numbers of responders depending on direction
- [ ] Exit Play mode

**Result:** PASS / FAIL (describe issue if FAIL)

---

## Tutorial 3: Custom Responders

- [ ] Open scene: `Assets/MercuryMessaging/Examples/Tutorials/Tutorial3/Tutorial3_Base.unity`
- [ ] Check console: zero errors, zero warnings on scene load
- [ ] Enter Play mode
- [ ] Check console: should show Initialize messages for all enemies on start
- [ ] Press **D**: expect `[GameController] Dealt 10 damage to all enemies` and each enemy logs damage/health
- [ ] Press **C**: expect `[GameController] Changed enemy colors to ...` and enemies log color change
- [ ] Press **G**: expect gravity toggle log messages from enemies
- [ ] Press **H**: expect `[GameController] Healed all enemies for 25` and heal messages from enemies
- [ ] Press **I**: expect re-initialize messages
- [ ] Press **D** multiple times until an enemy dies: expect `[EnemyX] Died!` message
- [ ] Exit Play mode

**Result:** PASS / FAIL (describe issue if FAIL)

---

## Tutorial 4: Custom Messages

- [ ] Open scene: `Assets/MercuryMessaging/Examples/Tutorials/Tutorial4/Tutorial4_Base.unity`
- [ ] Check console: zero errors, zero warnings on scene load
- [ ] Enter Play mode
- [ ] Check console: should show Initialize messages for lights on start
- [ ] Press **R**: expect `[LightController] Set color to RGBA(1.000, 0.000, 0.000, 1.000) with intensity 2` and light responders log color/intensity
- [ ] Press **G**: expect green light at intensity 1.5
- [ ] Press **B**: expect blue light at intensity 1.0
- [ ] Press **W**: expect white light at intensity 3.0
- [ ] Press **0** (zero): expect lights turn off (black, intensity 0)
- [ ] Press **I**: expect lights re-initialize to defaults
- [ ] Verify visual: lights/materials should change color when keys are pressed
- [ ] Exit Play mode

**Result:** PASS / FAIL (describe issue if FAIL)

---

## Tutorial 5: Fluent DSL API

- [ ] Open scene: `Assets/MercuryMessaging/Examples/Tutorials/Tutorial5/Tutorial5_Base.unity`
- [ ] Check console: zero errors, zero warnings on scene load
- [ ] Enter Play mode
- [ ] Check console: should show `[DSL Demo] Press keys 1-5, T, Y, U to see different DSL features` and hierarchy creation message
- [ ] Press **1**: expect `=== Demo 1: Traditional vs Fluent ===` with both traditional and fluent messages sent, responders logging receipt
- [ ] Press **2**: expect `=== Demo 2: Routing Targets ===` with ToChildren, ToDescendants, ToAll messages
- [ ] Press **3**: expect `=== Demo 3: Typed Values ===` with int, float, string, bool, Vector3 messages
- [ ] Press **4**: expect `=== Demo 4: Combined Filters ===` with Tag0 and Tag1 filtered messages (only matching responders receive)
- [ ] Press **5**: expect `=== Demo 5: Tier 1 Auto-Execute Methods ===` with BroadcastInitialize, BroadcastRefresh, BroadcastValue, BroadcastSetActive
- [ ] Press **T**: expect `[Delay] Message will be sent in 2 seconds...` then after 2 seconds `[Delay] Message sent after 2 second delay!`
- [ ] Press **Y**: expect `[Repeat] Cycling through 4 colors...` then 4 color messages over 2 seconds
- [ ] Press **U**: expect `[Conditional] Press SPACE to trigger the message!`, then press **Space**: expect `[DSL Demo] Conditional triggered! Sending message...`
- [ ] Exit Play mode

**Result:** PASS / FAIL (describe issue if FAIL)

---

## Summary

| Tutorial | Result |
|----------|--------|
| 1: Introduction | |
| 2: Basic Routing | |
| 3: Custom Responders | |
| 4: Custom Messages | |
| 5: Fluent DSL API | |

**Notes / Issues Found:**

(Describe any issues here)
