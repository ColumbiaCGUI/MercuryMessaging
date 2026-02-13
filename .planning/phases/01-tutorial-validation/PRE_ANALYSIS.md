# Tutorial Pre-Analysis Report

**Generated:** 2026-02-13
**Method:** Static comparison of wiki code blocks against actual tutorial scripts
**Scope:** Tutorials 1-12 (full analysis) + Tutorials 13-14 (stub verification)

## Environment Status

- **Render Pipeline:** Built-in (m_CustomRenderPipeline: {fileID: 0})
- **Shader.Find("Standard") safe:** YES -- Built-in pipeline includes Standard shader
- **Photon AppId configured:** YES -- AppIdFusion: `09716863-7e85-417f-a092-4878c1c088d2`
- **Wiki repo cloned:** `C:/Users/yangb/Research/MercuryMessaging.wiki/` (exists, Windows colon-in-filename issue prevents `git pull` but files are readable)
- **XR Device Simulator imported:** NO -- `Assets/Samples/XR Interaction Toolkit/` directory does not exist. Must import before Tutorial 12 validation.
- **GH label created:** YES -- `tutorial-validation` label created on ColumbiaCGUI/MercuryMessaging

---

## Tutorial 1: Introduction
**Health: GREEN**

Wiki and code are well-aligned. The wiki uses generic names as pedagogical examples while the actual scripts use T1_ prefixed names, which is the expected pattern across all tutorials.

### Wiki Step 2: Create a Simple Responder

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `MyResponder` | `T1_ChildResponder` | NO | NO -- wiki is pedagogical example, code is actual implementation |
| Base class | `MmBaseResponder` | `MmBaseResponder` | YES | |
| ReceivedMessage(MmMessageString) | Present | Present (identical) | YES | |
| ReceivedMessage(MmMessageInt) | Present | Present (identical) | YES | |
| Initialize() | `base.Initialize(); Debug.Log(...)` | `base.Initialize(); Debug.Log(...)` | YES | |
| Refresh() override | Not shown in Step 2 | Present in actual code | N/A | NO -- extra method in code is fine |

### Wiki Step 3: Send Messages (Traditional API)

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `MessageSender` | `T1_TraditionalApiExample` | NO | NO -- pedagogical name vs actual |
| Space key | Sends MessageString | Sends MessageString | YES | |
| I key | Sends Initialize | Not in traditional script | PARTIAL | NO -- T1_ParentController has I key |
| S key | Not in wiki | Sends with full metadata | N/A | NO -- extra feature in code |

### Wiki Step 4: Send Messages (DSL API)

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `MessageSenderDSL` | `T1_ParentController` | NO | NO -- pedagogical name vs actual |
| Space key | `Send("Hello from parent!").ToChildren().Execute()` | Not present | NO | NO -- code uses I, 1, 2, R, 3 instead |
| I key | `BroadcastInitialize()` | `BroadcastInitialize()` | YES | |
| N key | `Send(42).ToChildren().Execute()` | Not present (uses Alpha3 for BroadcastValue) | PARTIAL | NO -- different key, similar concept |

### Wiki Complete Example

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Parent class name | `ParentController` | `T1_ParentController` | NO | NO -- T1_ prefix expected |
| Child class name | `ChildResponder` | `T1_ChildResponder` | NO | NO -- T1_ prefix expected |
| Alpha1 key | `Send("Message 1").ToChildren().Execute()` | `Send("Message from parent!").ToChildren().Execute()` | YES (logic match) | NO -- minor string difference |
| Alpha2 key | `Send(100).ToChildren().Execute()` | `Send(100).ToChildren().Execute()` | YES | |
| Alpha3 key | `BroadcastRefresh()` | `BroadcastValue(42)` | NO | YES -- wiki says Refresh, code sends BroadcastValue(42) |

### Keyboard Controls

| Key | Wiki Docs | Code Implementation | Match? |
|-----|-----------|---------------------|--------|
| Space | "Send string message" (Step 3 traditional) | T1_TraditionalApiExample: MmInvoke MessageString / T1_SceneController: SetActive toggle | PARTIAL -- different scripts |
| I | "Initialize all children" (Step 4) | T1_ParentController: BroadcastInitialize() | YES |
| 1 | "Send Message 1" (Complete Example) | T1_ParentController: Send string to children | YES |
| 2 | "Send 100" (Complete Example) | T1_ParentController: Send int to children | YES |
| 3 | "BroadcastRefresh" (Complete Example) | T1_ParentController: BroadcastValue(42) | NO |
| R | Not in wiki | T1_ParentController: BroadcastRefresh() | N/A |
| S | Not in wiki | T1_TraditionalApiExample: Full metadata send | N/A |

### Console Output

| Wiki Expected | Code Debug.Log | Match? |
|---------------|----------------|--------|
| `ChildResponder: Initialized` | `[T1_Child] Initialized!` | PARTIAL -- name prefix differs |
| `ChildResponder: Got string 'Message 1'` | `[T1_Child] Received string: Message from parent!` | PARTIAL -- format and string differ |
| `ChildResponder: Got int 100` | `[T1_Child] Received int: 100` | PARTIAL -- format differs |
| `ChildResponder: Refreshed` | `[T1_Child] Refreshed` | PARTIAL -- name prefix differs |

### Issues Found

1. **Key 3 mismatch** -- Wiki says Alpha3 triggers `BroadcastRefresh()`, code triggers `BroadcastValue(42)`. The R key in code does BroadcastRefresh. DECISION NEEDED: Update wiki to match code (Alpha3 = BroadcastValue, R = Refresh)?
2. **Console output format** -- Wiki shows `ChildResponder: Got string 'x'`, code shows `[T1_Child] Received string: x`. AUTO-FIX: Update wiki to show actual output format.
3. **Prerequisites** -- Wiki says "Unity 2021.3 or later". AUTO-FIX: Update to "Unity 6000.3 or later (Unity 6)".
4. **T1_SceneController.cs** -- Exists in code but is not documented in the wiki at all. It handles Space key for SetActive toggle. DECISION NEEDED: Document in wiki or remove from scene?

---

## Tutorial 2: Basic Routing
**Health: GREEN**

Wiki is primarily conceptual/reference and the actual scripts implement the documented patterns correctly.

### Wiki: RoutingExamples

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `RoutingExamples` / `RoutingExamplesDSL` | `T2_RoutingExamples` | NO | NO -- T2_ prefix expected |
| ToChildren | Present | Present (C key) | YES | |
| ToParents | Present | Present (P key) | YES | |
| ToDescendants | Present | Present (D key) | YES | |
| ToAncestors | Present | Present (A key) | YES | |
| ToSiblings | Present | Present (S key) | YES | |
| ToAll | Present | Present (F key) | YES | |

### Wiki: Property-Based Routing

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| `relay.To.Children.Send("Hello")` | Documented | Not in tutorial code | N/A | YES -- Wiki documents property-based routing (`relay.To.X`), but does this API actually exist in the framework? If not, it should be removed from wiki. |

### Wiki: Menu System Example

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class names | `MenuController`, `ButtonResponder` | `T2_MenuController`, `T2_ButtonResponder` | NO | NO -- T2_ prefix expected |
| MenuController logic | Tag-based menu switching | Tag-based menu switching (identical) | YES | |
| ButtonResponder | `OnButtonClick()` with DSL send | `OnClick()` and `SetActive()` handler | YES (both present) | |

### Keyboard Controls

| Key | Wiki Docs | Code Implementation | Match? |
|-----|-----------|---------------------|--------|
| C | Not explicitly in wiki (described conceptually) | T2_RoutingExamples: ToChildren() | PARTIAL |
| P | Not explicitly in wiki | T2_RoutingExamples: ToParents() | PARTIAL |
| D | Not explicitly in wiki | T2_RoutingExamples: ToDescendants() | PARTIAL |
| A | Not explicitly in wiki | T2_RoutingExamples: ToAncestors() | PARTIAL |
| S | Not explicitly in wiki | T2_RoutingExamples: ToSiblings() | PARTIAL |
| F | Not explicitly in wiki | T2_RoutingExamples: ToAll() | PARTIAL |

### Issues Found

1. **No keyboard controls section in wiki** -- Wiki describes routing conceptually but does not list specific keyboard shortcuts. The actual T2_RoutingExamples.cs has C/P/D/A/S/F keys. AUTO-FIX: Add keyboard controls section to wiki.
2. **Property-based routing API** -- Wiki shows `relay.To.Children.Send("Hello")` syntax. DECISION NEEDED: Does this API exist in the framework? If not, remove from wiki. If yes, no action needed.

---

## Tutorial 3: Custom Responders
**Health: GREEN**

Code and wiki are well-aligned. Both approaches (MmBaseResponder and MmExtendableResponder) match.

### Wiki: MyMethods Constants

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `MyMethods` (namespace MyGame) | `T3_MyMethods` (no namespace) | NO | NO -- T3_ prefix expected |
| ChangeColor | 1000 | 1001 | NO | YES -- Wiki says ChangeColor=1000, code says TakeDamage=1000. Values shifted. |
| TakeDamage | 1001 | 1000 | NO | YES -- Same issue, reversed order |
| EnableGravity | 1003 | 1002 | NO | YES -- Same shift |
| PlaySound | 1002 | 1004 | NO | YES -- Different value |
| Heal | Not in wiki | 1003 | N/A | NO -- extra in code |

### Wiki: EnemyResponder (Traditional)

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `EnemyResponder` | `T3_EnemyResponder` | NO | NO -- T3_ prefix expected |
| Switch on `(int)message.MmMethod` | Yes | `(int)message.MmMethod` | YES | |
| Calls `base.MmInvoke(message)` first | Yes | Yes | YES | |
| HandleDamage, HandleColorChange, HandleGravity | Present | Present (identical logic) | YES | |
| Heal handler | Not in wiki | Present in code | N/A | |

### Wiki: EnemyResponderExtendable

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `EnemyResponderExtendable` | `T3_EnemyResponderExtendable` | NO | NO -- T3_ prefix expected |
| RegisterCustomHandler calls | 3 handlers | 4 handlers (includes Heal) | MINOR | NO -- extra handler in code |
| Handler logic | Identical patterns | Identical patterns | YES | |

### Wiki: GameController

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `GameController` | `T3_GameController` | NO | NO -- T3_ prefix expected |
| D key | DamageAllEnemies | DamageAllEnemies | YES | |
| C key | ChangeEnemyColors | ChangeEnemyColors | YES | |
| G key | Not in wiki | ToggleGravity | N/A | NO -- extra feature |
| H key | Not in wiki | HealAllEnemies | N/A | NO -- extra feature |

### Keyboard Controls

| Key | Wiki Docs | Code Implementation | Match? |
|-----|-----------|---------------------|--------|
| D | "Press D to damage all enemies" (in example) | D: DamageAllEnemies(10) | YES |
| C | "Press C to change enemy colors to red" (in example) | C: ChangeEnemyColors(Color.red) | YES |
| G | Not documented | G: ToggleGravity | N/A |
| H | Not documented | H: HealAllEnemies(25) | N/A |
| I | Not documented | I: BroadcastInitialize | N/A |

### Issues Found

1. **Method constant values mismatch** -- Wiki defines ChangeColor=1000, TakeDamage=1001. Code defines TakeDamage=1000, ChangeColor=1001. DECISION NEEDED: Update wiki to match code values, or update code to match wiki values?
2. **Extra keyboard shortcuts** -- Code has G (gravity), H (heal), I (initialize) not in wiki. AUTO-FIX: Add to wiki keyboard controls section.

---

## Tutorial 4: Custom Messages
**Health: GREEN**

Code closely matches wiki. Custom message class implementation is functionally identical.

### Wiki: MyMessageTypes Constants

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `MyMessageTypes` (namespace MyGame) | `T4_MyMessageTypes` (no namespace) | NO | NO -- T4_ prefix expected |
| ColorIntensity | 1100 | 1100 | YES | |
| EnemyState | 1101 | 1101 | YES | |
| PlayerStats | 1102 | 1102 | YES | |

### Wiki: ColorIntensityMessage

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `ColorIntensityMessage` | `T4_ColorIntensityMessage` | NO | NO -- T4_ prefix expected |
| Default constructor sets MmMethod | `(MmMethod)MyMethods.ChangeColor` | `(MmMethod)T3_MyMethods.ChangeColor` | YES (same ref) | |
| Serialize/Deserialize | object[] pattern | object[] pattern (identical) | YES | |
| Copy() | Copies base + custom fields | Copies base + custom fields + NetId, TimeStamp, HopCount | YES (code more complete) | |

### Wiki: LightController

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `LightController` | `T4_LightController` | NO | NO -- T4_ prefix expected |
| R/G/B keys | Red/Green/Blue | Red/Green/Blue | YES | |
| W key | Not in wiki | White (intensity 3.0) | N/A | |
| 0 key | Not in wiki | Turn off (intensity 0) | N/A | |

### Wiki: LightResponder

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `LightResponder` | `T4_LightResponder` | NO | NO -- T4_ prefix expected |
| Base class | `MmExtendableResponder` | `MmExtendableResponder` | YES | |
| Handler registration | `RegisterCustomHandler(...)` | `RegisterCustomHandler(...)` | YES | |

### Keyboard Controls

| Key | Wiki Docs | Code Implementation | Match? |
|-----|-----------|---------------------|--------|
| R | "Press R for red" | R: SetLightColor(Color.red, 2.0f) | YES |
| G | "Press G for green" | G: SetLightColor(Color.green, 1.5f) | YES |
| B | "Press B for blue" | B: SetLightColor(Color.blue, 1.0f) | YES |
| W | Not documented | W: SetLightColor(Color.white, 3.0f) | N/A |
| 0 | Not documented | 0: SetLightColor(Color.black, 0f) | N/A |
| I | Not documented | I: BroadcastInitialize | N/A |

### Issues Found

1. **Wiki Serialize/Deserialize inconsistency** -- Wiki Step 2 shows object[] pattern for ColorIntensityMessage but Example 2 (EnemyStateMessage) shows MmWriter/MmReader pattern. DECISION NEEDED: Which serialization API is correct? The actual code uses object[] pattern. If MmWriter/MmReader does not exist, remove those examples from wiki.
2. **Extra keyboard shortcuts** -- Code has W, 0, I not documented. AUTO-FIX: Add to wiki.

---

## Tutorial 5: Fluent DSL API
**Health: GREEN**

The DSL demo scene setup and responder match wiki documentation closely.

### Wiki: Interactive Demo

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Setup class | `DSLSceneSetup` | `T5_DSLSceneSetup` | NO | NO -- T5_ prefix expected |
| Key 1 | Traditional vs Fluent | Demo1_TraditionalVsFluent() | YES | |
| Key 2 | Routing targets | Demo2_RoutingTargets() | YES | |
| Key 3 | Typed values | Demo3_TypedValues() | YES | |
| Key 4 | Combined filters | Demo4_CombinedFilters() | YES | |
| Key 5 | Auto-execute methods | Demo5_AutoExecute() | YES | |
| Key T | Delayed execution | DemoT_DelayedExecution() | YES | |
| Key Y | Repeating messages | DemoY_RepeatingMessages() | YES | |
| Key U | Conditional trigger | DemoU_ConditionalTrigger() | YES | |

### Wiki: Using DSL from Responders

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| `this.To().Parents.Send("done")` | Documented | Not tested in demo code | N/A | YES -- Same question as Tutorial 2: does `this.To()` API exist? |

### Keyboard Controls

| Key | Wiki Docs | Code Implementation | Match? |
|-----|-----------|---------------------|--------|
| 1 | Traditional vs Fluent comparison | Demo1_TraditionalVsFluent | YES |
| 2 | Routing targets | Demo2_RoutingTargets | YES |
| 3 | Typed values | Demo3_TypedValues | YES |
| 4 | Combined filters | Demo4_CombinedFilters | YES |
| 5 | Auto-execute methods | Demo5_AutoExecute | YES |
| T | Delayed execution (2s delay) | DemoT_DelayedExecution (coroutine) | YES |
| Y | Repeating messages | DemoY_RepeatingMessages (coroutine) | YES |
| U | Conditional trigger (Space activates) | DemoU_ConditionalTrigger + Space check | YES |

### Issues Found

1. **Wiki shows `relay.After()`, `relay.Every()`, `relay.When()` temporal methods** -- Code uses coroutines instead. DECISION NEEDED: Do `After()`, `Every()`, `When()` extension methods exist in the framework? If not, wiki should show the coroutine approach the code actually uses.
2. **T5_SceneController.cs** exists in Scripts/ subfolder but is not documented. Need to check its content during runtime validation.
3. **Property-based routing** -- Wiki shows `relay.To.Children.Send("Hello")` and `this.To().Parents.Send("done")`. DECISION NEEDED: Does this API exist in the framework? (Affects Tutorials 2, 5)

---

## Tutorial 6: FishNet Networking
**Health: YELLOW**

Wiki structure matches code pattern but several wiki-described APIs may not exist in the framework.

### Wiki: NetworkSetup

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `NetworkSetup` | `T6_NetworkSetup` | NO | NO -- T6_ prefix expected |
| MmNetworkBridge.Instance usage | `bridge.SetBackend(new FishNetBackend())` | Need to verify at runtime | UNKNOWN | YES -- Does MmNetworkBridge, FishNetBackend, FishNetResolver exist? |

### Wiki: NetworkGameController

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `NetworkGameController` | `T6_NetworkGameController` | NO | NO -- T6_ prefix expected |
| #if FISHNET_AVAILABLE guard | Not shown in wiki | Present in code | MINOR | AUTO-FIX: Wiki should mention conditional compilation |
| Backend.OnClientConnected event | Present | Present | YES | |
| S key (server sync) | Present | Present | YES | |
| I key (initialize over network) | Not in wiki | Present in code | N/A | |
| R key (refresh over network) | Not in wiki | Present in code | N/A | |

### Keyboard Controls

| Key | Wiki Docs | Code Implementation | Match? |
|-----|-----------|---------------------|--------|
| S | Sync game state | S: SyncGameState() (server only) | YES |
| I | Not documented | I: Initialize over network | N/A |
| R | Not documented | R: Refresh over network | N/A |

### Issues Found

1. **Network API existence** -- Wiki documents `MmNetworkBridge`, `FishNetBackend`, `FishNetResolver`, `IMmNetworkBackend`, `IMmGameObjectResolver`. DECISION NEEDED: Do these classes exist in the framework? They are critical for Tutorials 6, 7, 11. Must verify during runtime validation.
2. **Conditional compilation** -- Code uses `#if FISHNET_AVAILABLE` guards. AUTO-FIX: Wiki should mention this requirement.
3. **`H` key for Host and `C` key for Client** -- Research inventory says H/C/D/WASD are controls but actual code has S/I/R. The research inventory may be wrong. Need runtime verification.

---

## Tutorial 7: Fusion 2 Networking
**Health: YELLOW**

Same pattern as Tutorial 6 -- wiki documents APIs that need runtime verification.

### Wiki: Fusion2Setup

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `Fusion2Setup` | `T7_Fusion2Setup` | NO | NO -- T7_ prefix expected |
| MmFusion2Bridge | Documented as spawned NetworkBehaviour | Need to verify existence | UNKNOWN | YES -- Does MmFusion2Bridge exist? |

### Wiki: GameController

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `GameController` | `T7_Fusion2GameController` | NO | NO -- T7_ prefix expected |
| S key | Sync | Need runtime verification | UNKNOWN | |
| Space key | Client action request | Need runtime verification | UNKNOWN | |

### Issues Found

1. **Same network API concerns as Tutorial 6** -- MmNetworkBridge, Fusion2Backend, MmFusion2Bridge existence needs verification.
2. **Photon AppId confirmed** -- `09716863-7e85-417f-a092-4878c1c088d2` is configured, so Fusion 2 connectivity should work.

---

## Tutorial 8: Switch Nodes & FSM
**Health: GREEN**

Wiki and code are very well-aligned. The T8_GameStateController implements exactly the pattern described.

### Wiki: GameStateController

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `GameStateController` | `T8_GameStateController` | NO | NO -- T8_ prefix expected |
| State names | MainMenu, Gameplay, PauseMenu, GameOver | MainMenu, Gameplay, PauseMenu, GameOver | YES | |
| FSM access | `switchNode.RespondersFSM` | `switchNode.RespondersFSM` | YES | |
| JumpTo pattern | Loop RoutingTable, find by name | Loop RoutingTable, find by name | YES | |
| GlobalEnter/Exit | Callbacks registered | Callbacks registered | YES | |
| Per-state Enter/Exit | Gameplay and Pause callbacks | Gameplay and Pause callbacks | YES | |

### Keyboard Controls

| Key | Wiki Docs | Code Implementation | Match? |
|-----|-----------|---------------------|--------|
| Return/Enter | Start game / Restart | Return: Start from Menu, MainMenu from GameOver | YES |
| Escape | Pause / Resume | Escape: Pause from Gameplay, Resume from Pause | YES |
| Q | Not in wiki main example | Q: Quit to menu | N/A |
| G | Not in wiki main example | G: Trigger game over | N/A |
| 1-4 | Not in wiki | 1-4: Direct state jump | N/A |

### Issues Found

1. **Extra keyboard shortcuts in code** -- Q, G, 1-4 are in code but not wiki. AUTO-FIX: Add to wiki keyboard controls section.
2. **Wiki mentions `MmSwitchResponder`** in some places but Tutorial 8 code uses raw FSM access on `MmRelaySwitchNode`. The wiki FSM navigation pattern matches the code.

---

## Tutorial 9: Task Management
**Health: YELLOW**

Wiki describes the conceptual pattern well, but actual code implementation differs in specifics.

### Wiki: ExperimentManager

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `ExperimentManager` | `T9_ExperimentManager` | NO | NO -- T9_ prefix expected |
| Base class | `MmTaskManager<MyTaskInfo>` | `MmTaskManager<T9_MyTaskInfo>` | YES (same pattern) | |
| Task loading | From JSON file | Creates sample tasks programmatically + supports file loading | PARTIAL | NO -- code has fallback |
| Navigation | GoToNextTask, GoToPreviousTask, GoToTask | Same methods present | YES | |

### Wiki: MyTaskInfo

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `MyTaskInfo` | `T9_MyTaskInfo` | NO | NO -- T9_ prefix expected |
| Interface | `IMmTaskInfo` | `IMmTaskInfo` | YES | |
| Custom fields | StimulusType, TargetValue, TrialNumber, Condition | StimulusType, TargetValue, TrialNumber, Condition + Instructions, TimeLimit | YES (superset) | |

### Wiki: JsonTaskLoader

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `JsonTaskLoader` | `T9_JsonTaskLoader` | NO | NO -- T9_ prefix expected |
| Interface | `IMmTaskInfoCollectionLoader<MyTaskInfo>` | Need to verify | UNKNOWN | YES -- Does this interface exist? |

### Keyboard Controls

| Key | Wiki Docs | Code Implementation | Match? |
|-----|-----------|---------------------|--------|
| N | Next task | N: GoToNextTask() | YES |
| P | Previous task | P: GoToPreviousTask() | YES |
| R | Restart current task | R: RestartCurrentTask() | YES |
| Space | Record trial response | Not in T9_ExperimentManager directly | PARTIAL -- May be in T9_TrialResponder |

### Issues Found

1. **MmTaskManager<T> API** -- Wiki describes `AdvanceToNextTask()` but code uses custom `GoToNextTask()`. DECISION NEEDED: Does MmTaskManager have an `AdvanceToNextTask()` method, or is the code's approach correct?
2. **MmDataCollector** -- Wiki documents SetHeaders/AddRow/SaveToFile API. DECISION NEEDED: Does MmDataCollector class exist in the framework? If not, wiki examples need updating.
3. **IMmTaskInfoCollectionLoader<T>** -- DECISION NEEDED: Does this interface exist?

---

## Tutorial 10: Application State
**Health: GREEN**

Code implements the documented pattern closely.

### Wiki: MyAppController

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `MyAppController` (extends MmSwitchResponder) | `T10_MyAppController` (extends MmAppStateSwitchResponder) | NO | YES -- Wiki base class is `MmSwitchResponder`, code base class is `MmAppStateSwitchResponder`. Which is correct? |
| State switching | `SwitchTo(name)` calling `MmRelaySwitchNode.MmInvoke(Switch, name)` | Same pattern | YES | |
| Initial state | `InitialState = "MainMenu"` | `InitialState = startState` (serialized field defaulting to "MainMenu") | YES | |
| SetupAppStates | Override documented | Override present | YES | |

### Wiki: State Behaviors

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| MenuBehavior | Extends `MmAppStateResponder` | `T10_MenuBehavior` -- need to verify base class | UNKNOWN | |
| LoadingBehavior | Extends `MmAppStateResponder` | `T10_LoadingBehavior` -- need to verify | UNKNOWN | |
| GameplayBehavior | Extends `MmAppStateResponder` | `T10_GameplayBehavior` -- need to verify | UNKNOWN | |
| PauseBehavior | Extends `MmAppStateResponder` | `T10_PauseBehavior` -- need to verify | UNKNOWN | |

### Keyboard Controls

| Key | Wiki Docs | Code Implementation | Match? |
|-----|-----------|---------------------|--------|
| Escape | Toggle Pause from Gameplay / Resume from Pause | Escape: Toggle pause | YES |
| M | Go to Main Menu | M: GoToMainMenu() | YES |

### Issues Found

1. **Base class discrepancy** -- Wiki says `MmSwitchResponder`, code extends `MmAppStateSwitchResponder`. DECISION NEEDED: Are both valid? Does `MmAppStateSwitchResponder` exist? Which should wiki document?
2. **`MmAppStateResponder` existence** -- Wiki documents this class for state behaviors. DECISION NEEDED: Does it exist in the framework?

---

## Tutorial 11: Advanced Networking
**Health: YELLOW**

Code is a simple loopback demo; wiki covers much broader network architecture.

### Wiki: Architecture

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| MmBinarySerializer | Detailed binary format documented | Not used in tutorial code | N/A | NO -- wiki is reference, code is demo |
| MmNetworkBridge API | SendToServer, SendToAllClients, etc. | MmLoopbackBackend used directly | PARTIAL | |
| MmLoopbackBackend | Not prominently documented | Used in T11_LoopbackDemo | YES | |

### Wiki: NetworkDiagnostics

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| D key | Prints network diagnostics | Not in T11_LoopbackDemo | NO | NO -- wiki example is supplementary |

### Keyboard Controls

| Key | Wiki Docs | Code Implementation | Match? |
|-----|-----------|---------------------|--------|
| Space | Not explicitly in wiki as keyboard | Space: SendTestMessage() | PARTIAL |
| S | Not explicitly | S: SendNetworkFilteredMessage() | PARTIAL |
| I | Not explicitly | I: BroadcastInitialize() | PARTIAL |

### Issues Found

1. **MmLoopbackBackend** -- Code uses it. DECISION NEEDED: Does this class exist in the framework?
2. **Wiki breadth vs code simplicity** -- Wiki covers full network architecture but tutorial code is a simple loopback demo. This is acceptable (wiki is reference), but wiki should note the tutorial is a simplified introduction.
3. **MmBinarySerializer, MmWriter, MmReader** -- Wiki documents these in detail. DECISION NEEDED: Do they exist? (Affects Tutorials 4, 11)

---

## Tutorial 12: VR Experiment
**Health: GREEN**

Code is a self-contained Go/No-Go task controller that works with keyboard fallback.

### Wiki: Architecture

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| Class name | `ExperimentController` | `T12_GoNoGoController` | NO | NO -- different naming convention |
| Uses MmTaskManager | Yes (wiki hierarchy) | No -- standalone MonoBehaviour | NO | YES -- Wiki shows MmTaskManager-based hierarchy, code is self-contained. Which approach should tutorial teach? |
| VR Input | XR Interaction Toolkit InputActionReference | `#if UNITY_XR_AVAILABLE` with legacy XR API | NO | YES -- Wiki shows modern XRI InputSystem approach, code uses legacy `InputDevices.GetDeviceAtXRNode`. Which is correct? |
| Keyboard fallback | Not documented | Space key for response | N/A | AUTO-FIX: Document keyboard fallback |

### Wiki: StimulusController / VRInputHandler

| Aspect | Wiki Says | Code Says | Match? | Decision Needed? |
|--------|-----------|-----------|--------|------------------|
| StimulusController | Separate class with message-based control | Inline stimulus control in T12_GoNoGoController | NO | YES -- Wiki separates concerns via messaging, code is monolithic. |
| VRInputHandler | Separate class with InputActionReference | Inline input handling | NO | YES -- Same issue |
| DataLogger | Separate MmDataCollector-based export | T12_DataLogger exists (need to read) | UNKNOWN | |

### Keyboard Controls

| Key | Wiki Docs | Code Implementation | Match? |
|-----|-----------|---------------------|--------|
| Return | Not documented | Return: Start experiment | N/A |
| Space | "Press button for Go stimuli" (VR context) | Space: Record response (keyboard fallback) | PARTIAL |

### Issues Found

1. **Architecture mismatch** -- Wiki shows MmTaskManager + separate StimulusController + VRInputHandler + MmDataCollector. Code is a single monolithic T12_GoNoGoController. DECISION NEEDED: Should wiki be updated to match the simpler code approach, or should code be refactored to match the wiki's message-based architecture?
2. **VR input approach** -- Wiki uses XRI InputActionReference (modern), code uses `#if UNITY_XR_AVAILABLE` with legacy XR API. DECISION NEEDED: Update code to use modern XRI approach, or update wiki?
3. **XR Device Simulator** -- Not imported. Must be imported before runtime validation.
4. **MmDataCollector** -- Wiki uses it extensively. Need to verify existence.

---

## Tutorials 13-14 (Stubs)

### Tutorial 13: Spatial & Temporal Filtering
- **Coming Soon banner present:** YES
- **Broken links found:** None -- links to Tutorial 5 and API Reference use relative wiki format
- **Unimplemented API references:**
  - `.Within(radius)` -- Planned, not available
  - `.InCone(direction, angle, range)` -- Planned, not available
  - `.InBounds(bounds)` -- Planned, not available
  - `.Throttle(interval)` -- Planned, not available
  - `.Debounce(delay)` -- Planned, not available
  - `After()`, `Every()`, `When()` -- Marked as "Available" in status table
- **Status table present:** YES -- clearly shows Available vs Planned
- **Try This exercises:** Present -- use available temporal features

### Tutorial 14: Performance Optimization
- **Coming Soon banner present:** YES
- **Broken links found:** None
- **Unimplemented API references:**
  - `[MmGenerateDispatch]` -- Marked as Available
  - `[MmHandler]` -- Marked as Available
  - `PerformanceMode` -- Marked as Available
  - All items marked as Available (none Planned)
- **Status table present:** YES
- **Try This exercises:** Present

### Issues Found (Stubs)

1. **Tutorial 13 temporal methods** -- `After()`, `Every()`, `When()` are listed as "Available" but Tutorial 5 code uses coroutines instead. DECISION NEEDED: Do these temporal extension methods actually exist in the framework? If not, status should be "Planned".
2. **Tutorial 14 source generators** -- `[MmGenerateDispatch]` and `[MmHandler]` are listed as "Available". Need to verify they exist.

---

## Cross-Tutorial Issues

### Issue A: Property-Based Routing API (`relay.To.Children`)
**Affected tutorials:** 2, 5
**Question:** Does the property-based routing API (`relay.To.Children.Send("Hello")`) exist in the framework?
**Impact:** If it does not exist, these wiki sections document non-existent API and will confuse users.
**Recommendation:** Grep framework source for `public.*To ` property on MmRelayNode. If absent, remove from wiki.

### Issue B: Network API Classes
**Affected tutorials:** 6, 7, 11
**Question:** Do these classes exist: MmNetworkBridge, FishNetBackend, FishNetResolver, Fusion2Backend, MmFusion2Bridge, MmLoopbackBackend, MmBinarySerializer, IMmNetworkBackend, IMmGameObjectResolver?
**Impact:** If they don't exist, network tutorials are teaching against non-existent API.
**Recommendation:** Verify during runtime validation. If classes don't exist, major wiki rewrite needed.

### Issue C: Data Collection API
**Affected tutorials:** 9, 12
**Question:** Does `MmDataCollector` exist with SetHeaders/AddRow/SaveToFile API?
**Impact:** If it doesn't exist, experiment tutorials document unavailable functionality.

### Issue D: Temporal Extensions
**Affected tutorials:** 5, 13
**Question:** Do `relay.After()`, `relay.Every()`, `relay.When()` extension methods exist?
**Impact:** Tutorial 5 code uses coroutines as a workaround; Tutorial 13 lists them as "Available".

### Issue E: MmWriter/MmReader Binary API
**Affected tutorials:** 4, 11
**Question:** Do `MmWriter`/`MmReader` classes exist for binary serialization?
**Impact:** Tutorial 4 wiki Example 2 (EnemyStateMessage) and Tutorial 11 use these for serialization.
**Note:** Tutorial 4 actual code uses the object[] pattern, not MmWriter/MmReader.

### Issue F: Task System Classes
**Affected tutorials:** 9
**Question:** Do `MmTaskManager<T>`, `MmTaskUserConfigurator`, `IMmTaskInfo`, `IMmTaskInfoCollectionLoader<T>` exist?
**Impact:** Tutorial 9 code extends MmTaskManager, so it likely exists. Sub-interfaces need verification.

### Issue G: App State Classes
**Affected tutorials:** 10
**Question:** Do `MmSwitchResponder`, `MmAppStateSwitchResponder`, `MmAppStateResponder` exist?
**Impact:** Tutorial 10 code extends MmAppStateSwitchResponder, so it likely exists.

---

## Summary

| Tutorial | Health | Issues | Decisions Needed |
|----------|--------|--------|------------------|
| 1: Introduction | GREEN | 4 | 2 |
| 2: Basic Routing | GREEN | 2 | 1 |
| 3: Custom Responders | GREEN | 2 | 1 |
| 4: Custom Messages | GREEN | 2 | 1 |
| 5: Fluent DSL API | GREEN | 3 | 2 |
| 6: FishNet Networking | YELLOW | 3 | 1 |
| 7: Fusion 2 Networking | YELLOW | 2 | 1 |
| 8: Switch Nodes & FSM | GREEN | 2 | 0 |
| 9: Task Management | YELLOW | 3 | 3 |
| 10: Application State | GREEN | 2 | 2 |
| 11: Advanced Networking | YELLOW | 3 | 2 |
| 12: VR Experiment | GREEN | 4 | 3 |
| 13: Stub | N/A | 1 | 1 |
| 14: Stub | N/A | 0 | 0 |
| **Total** | | **33** | **20** |

### Decision Summary

The 20 decisions needed fall into these categories:

**Category 1: API Existence Verification (7 decisions)**
These will be resolved during runtime validation by checking if classes/methods exist:
- A: Property-based routing API (`relay.To.X`)
- B: Network API classes (MmNetworkBridge, etc.)
- C: MmDataCollector
- D: Temporal extensions (After/Every/When)
- E: MmWriter/MmReader
- F: Task system interfaces
- G: App state classes

**Category 2: Wiki-vs-Code Direction (8 decisions)**
User must decide which side to fix:
- T1: Key 3 mapping (wiki=Refresh, code=BroadcastValue)
- T1: T1_SceneController documentation
- T3: Method constant values (wiki ChangeColor=1000, code TakeDamage=1000)
- T4: Serialization API pattern (object[] vs MmWriter/MmReader)
- T9: MmTaskManager.AdvanceToNextTask() vs custom navigation
- T10: Base class (MmSwitchResponder vs MmAppStateSwitchResponder)
- T12: Monolithic vs message-based architecture
- T12: VR input approach (legacy vs XRI)

**Category 3: Temporal Features Status (1 decision)**
- T13: Are After/Every/When truly "Available"?

**Category 4: Class Name Convention (0 decisions)**
All tutorials use T{N}_ prefix consistently. Wiki uses generic pedagogical names. This is the expected pattern and requires NO user decision -- wiki should use generic names for teaching, and note the actual filename with T{N}_ prefix.

### AUTO-FIX Items (no decision needed)

1. Update "Unity 2021.3 or later" prerequisite to "Unity 6000.3 or later" across all tutorials
2. Add keyboard controls sections where missing (Tutorials 2, 4, 8)
3. Add missing keyboard shortcuts to wiki (extra keys in code not documented)
4. Update console output format in wiki to match actual `[GameObjectName]` format
5. Add `#if FISHNET_AVAILABLE` / `#if FUSION2_AVAILABLE` notes to networking tutorials
6. Document keyboard fallback for Tutorial 12
7. Import XR Device Simulator sample before Tutorial 12 validation
