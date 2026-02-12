# Phase 1: Tutorial Validation - Research

**Researched:** 2026-02-12
**Domain:** Unity tutorial validation, wiki-to-code consistency, Unity 6000.3.7f1 compatibility
**Confidence:** HIGH

## Summary

Phase 1 requires systematically validating 12 published tutorials (plus verifying 2 "Coming Soon" stubs) against their GitHub Wiki documentation in Unity 6000.3.7f1. The project has a well-structured codebase with tutorial scripts already present for all 12 tutorials under `Assets/MercuryMessaging/Examples/Tutorials/`. Each tutorial has corresponding wiki pages with step-by-step instructions, code examples, keyboard controls, and expected console output.

The key risk areas are: (1) wiki code examples diverging from actual tutorial scripts (different class names, methods, or APIs), (2) Unity 6000.3.7f1 compatibility issues from a framework originally targeting Unity 2021.3+, (3) the networking tutorials (6, 7, 11) requiring live multi-instance testing, and (4) the VR tutorial (12) requiring XR Device Simulator which is NOT currently imported into the project.

**Primary recommendation:** Validate tutorials sequentially 1-12, treating each as an atomic unit with its own commit. For each tutorial, perform a 4-step process: (A) open scene, check for errors/warnings, (B) compare wiki code to actual scripts, (C) run scene and test all documented keyboard controls, (D) verify console output matches documentation. Fix issues in-place and file GitHub issues for audit trail.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- Zero tolerance: any error OR warning in the Unity console counts as a failure
- Code examples from the wiki must compile AND produce documented runtime behavior (not just compile)
- Visual results: capture new screenshots in Unity 6000.3.7f1 and update wiki to match current visuals
- Validate tutorials 1-12 fully; for tutorials 13-14 (Coming Soon), verify stubs are clearly marked and have no broken links
- Fix everything: find issues AND fix them in this phase -- deliverable is working tutorials, not just a report
- Framework bugs exposed by tutorials are in scope -- fix MercuryMessaging code if a tutorial surfaces a genuine bug
- Fundamentally broken tutorials get rebuilt from scratch -- every tutorial must work
- Commits are per-tutorial: one atomic commit per tutorial validated (e.g., `fix(tutorial-3): update routing example to match wiki`)
- Validation order: sequential, tutorials 1-12 in order (earlier tutorials are prerequisites for later ones)
- VR tutorials (Tutorial 12): validate using Unity XR Device Simulator only -- no physical headset required
- Networking tutorials (Tutorial 6: FishNet, Tutorial 7: Fusion 2, Tutorial 11: Advanced Networking): full network test with actual networked instances (host + client, localhost)
- Photon Fusion 2 (Tutorial 7): set up Photon account and AppId as part of this phase, then full runtime validation
- Wiki vs Code authority: Case-by-case when wiki and code disagree -- no blanket rule
- Every mismatch gets flagged for user review before fixing -- Claude documents both options, user decides direction
- Wiki changes can be pushed directly to the GitHub Wiki repo (Claude has access)
- Per-step detail: each wiki step validated individually with pass/fail and specific findings
- Report location: `.planning/phases/01-tutorial-validation/VALIDATION_REPORT.md`
- Include estimated completion time per tutorial (informs user study time estimates in Phase 5)
- GitHub issues filed for every failure found, closed with the fix commit (complete audit trail)

### Claude's Discretion
- Exact format and structure of the per-step validation report
- How to organize wiki screenshot updates (inline vs separate commit)
- Prioritization when multiple issues are found in a single tutorial

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope
</user_constraints>

## Standard Stack

### Core (Already Installed)
| Component | Version | Purpose | Status |
|-----------|---------|---------|--------|
| Unity Editor | 6000.3.7f1 | Runtime environment | Installed, confirmed in ProjectVersion.txt |
| MercuryMessaging | ~4.0.0 | Core framework | In project at `Assets/MercuryMessaging/` |
| FishNet | Latest (git) | Networking backend (Tutorial 6, 11) | Installed via Package Manager (git URL) |
| Photon Fusion 2 | ~2.0.9 | Networking backend (Tutorial 7) | Installed as local asset at `Assets/Photon/Fusion/` |
| XR Interaction Toolkit | 3.3.1 | VR input (Tutorial 12) | Installed via Package Manager |
| Input System | (bundled) | New Input System for XR | Installed (referenced by Examples asmdef) |
| ParrelSync | Latest (git) | Multi-editor testing (Tutorial 6) | Installed via Package Manager |
| Unity Test Framework | 1.6.0 | Automated testing | Installed |

### Missing (Must Be Installed)
| Component | Purpose | Needed For |
|-----------|---------|------------|
| XR Device Simulator sample | VR input without physical headset | Tutorial 12 validation |

**Note:** The XR Device Simulator is a sample included with XR Interaction Toolkit 3.3.1 but it must be explicitly imported from the Package Manager Samples tab. It is NOT currently in the project.

### Scripting Define Symbols (Already Configured)
All required symbols are present in `ProjectSettings/ProjectSettings.asset`:
- `FISHNET` (FishNet core)
- `FISHNET_V4` (FishNet version)
- `FISHNET_AVAILABLE` (MercuryMessaging conditional)
- `FUSION_WEAVER`, `FUSION2`, `FUSION_2`, `FUSION2_AVAILABLE` (Photon Fusion 2)

### Assembly Definitions
| Assembly | References | Tutorials Covered |
|----------|-----------|-------------------|
| `MercuryMessaging.Examples` | MercuryMessaging, StandardLibrary, XR.Interaction.Toolkit, InputSystem | All tutorials |
| `MercuryMessaging` | FishNet (GUID), Fusion (GUID) | Framework core |
| `MercuryMessaging.StandardLibrary` | MercuryMessaging | UI/Input message tutorials |

## Architecture Patterns

### Tutorial File Organization
Each tutorial follows a consistent structure:
```
Assets/MercuryMessaging/Examples/Tutorials/
  TutorialN/
    TN_ScriptName.cs          # Tutorial-specific scripts (T1_, T2_, etc.)
    TutorialN_Base.unity       # Pre-built scene
    Materials/                 # Tutorial-specific materials (some tutorials)
    README.md                  # Local setup instructions (tutorials 6-12)
    TutorialN_BaseSettings.lighting  # Lighting data (some tutorials)
```

### Wiki Page Structure (Consistent Pattern)
Each wiki tutorial follows this structure:
1. Overview / What You'll Learn
2. Prerequisites
3. Step-by-Step Guide (numbered steps with code examples)
4. Expected Output (console log examples)
5. Common Mistakes (table format)
6. Try This (exercises)
7. Next Steps (links to related tutorials)

### Tutorial Inventory

| Tutorial | Scene File | Key Scripts | Wiki Controls | Complexity |
|----------|-----------|-------------|---------------|------------|
| 1: Introduction | Tutorial1_Base.unity | T1_ParentController, T1_ChildResponder, T1_SceneController, T1_TraditionalApiExample | I, 1, 2, R, 3, Space, S | LOW |
| 2: Basic Routing | Tutorial2_Base.unity | T2_ButtonResponder, T2_MenuController, T2_RoutingExamples | (wiki suggests no specific keys) | LOW |
| 3: Custom Responders | Tutorial3_Base.unity | T3_EnemyResponder, T3_EnemyResponderExtendable, T3_GameController, T3_MyMethods | D, C | MEDIUM |
| 4: Custom Messages | Tutorial4_Base.unity | T4_ColorIntensityMessage, T4_LightController, T4_LightResponder, T4_MyMessageTypes, + Scripts/ folder | R, G, B | MEDIUM |
| 5: Fluent DSL API | Tutorial5_Base.unity | T5_DemoResponder, T5_DSLSceneSetup, T5_SceneController | 1-5, T, Y, U | MEDIUM |
| 6: FishNet Networking | Tutorial6_FishNet.unity | T6_NetworkGameController, T6_NetworkSetup, T6_PlayerResponder | H, C, D, WASD | HIGH |
| 7: Fusion 2 Networking | Tutorial7_Fusion2.unity | T7_Fusion2GameController, T7_Fusion2Setup, T7_Fusion2PlayerResponder | S, Space | HIGH |
| 8: Switch Nodes & FSM | Tutorial8_FSM.unity | T8_GameStateController, T8_MenuResponder, T8_GameplayResponder, T8_PauseResponder, T8_GameOverResponder | Return, Escape, Q, G, 1-4 | MEDIUM |
| 9: Task Management | Tutorial9_Tasks.unity | T9_ExperimentManager, T9_MyTaskInfo, T9_TrialResponder, T9_JsonTaskLoader | N, P, R, Space | MEDIUM |
| 10: Application State | Tutorial10_AppState.unity | T10_MyAppController, T10_MenuBehavior, T10_LoadingBehavior, T10_GameplayBehavior, T10_PauseBehavior | Escape, M | MEDIUM |
| 11: Advanced Networking | Tutorial11_AdvancedNetwork.unity | T11_LoopbackDemo, T11_NetworkResponder | Space, S, I | MEDIUM |
| 12: VR Experiment | Tutorial12_VRExperiment.unity | T12_GoNoGoController, T12_DataLogger | Return (start), Space (respond) | HIGH |
| 13: Spatial/Temporal (Stub) | (none) | (none) | N/A | VERIFY STUB |
| 14: Performance (Stub) | (none) | (none) | N/A | VERIFY STUB |

### Validation Process Per Tutorial (Recommended)

For each tutorial (1-12), the validation follows 6 stages:

1. **Scene Load Test**: Open the `.unity` scene file. Check Unity console for errors/warnings on load.
2. **Script Compilation Check**: Verify all `TN_*.cs` scripts compile without errors or warnings.
3. **Wiki-to-Code Comparison**: For each code example in the wiki, compare against the actual script. Document mismatches.
4. **Runtime Behavior Test**: Enter Play mode. Test all documented keyboard controls. Verify console output matches wiki documentation.
5. **Screenshot Capture**: Take screenshots of visual state in Unity 6000.3.7f1 for wiki updates.
6. **Fix and Commit**: Fix all issues found, file GitHub issue per failure, close with fix commit.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Multi-editor network testing | Custom build scripts | ParrelSync (already installed) | Handles project cloning, shared assets |
| VR input simulation | Custom input emulation | XR Device Simulator (XRI sample) | Standard Unity tooling, keyboard/mouse to XR input |
| Wiki content fetching | Manual browser copy-paste | `gh api` or WebFetch tool | Programmatic wiki access for comparison |
| GitHub issue management | Manual issue creation | `gh issue create` CLI | Automated audit trail |
| Screenshot capture | Manual screenshotting | Unity `ScreenCapture.CaptureScreenshot()` or manual | Per-tutorial visual evidence |

## Common Pitfalls

### Pitfall 1: Wiki Code vs Actual Script Class Name Mismatch
**What goes wrong:** The wiki shows class `MyResponder` but the actual script is `T1_ChildResponder`. Users copy-paste from wiki and get compilation errors.
**Why it happens:** Wiki was written with generic names; scripts use tutorial-prefixed naming convention (T1_, T2_, etc.).
**How to avoid:** For each tutorial, verify every wiki code block either matches the actual script OR is clearly labeled as a "create from scratch" example with the exact filename specified.
**Warning signs:** Class names in wiki don't match any `.cs` file in the tutorial folder.

### Pitfall 2: MmRelayNode Missing on GameObjects in Scene
**What goes wrong:** Tutorial scene hierarchy doesn't have MmRelayNode on expected GameObjects. Messages don't route.
**Why it happens:** Scenes were created but not fully wired up, or scene was modified after initial setup.
**How to avoid:** For each tutorial, verify the documented hierarchy against the actual scene hierarchy in Unity. Check every node the wiki says should have `MmRelayNode`.
**Warning signs:** "Message not received" at runtime, or NullReferenceException on `GetComponent<MmRelayNode>()`.

### Pitfall 3: Unity 6000 API Deprecations
**What goes wrong:** Code compiles but generates warnings. Or code uses deprecated APIs that behave differently.
**Why it happens:** Framework was built for Unity 2021.3+. Unity 6000.3.7f1 has deprecated some APIs.
**How to avoid:** Watch for deprecation warnings in the console. Key areas: `Physics.autoSyncTransforms` (deprecated in 6.3), render pipeline changes, `Shader.Find("Standard")` behavior (the DSLSceneSetup uses this).
**Warning signs:** Yellow warning messages in console on scene load or play.

### Pitfall 4: Networking Tutorial Requires Live Multi-Instance Testing
**What goes wrong:** Network tutorials appear to work locally but fail with actual host+client setup.
**Why it happens:** FishNet and Fusion require actual network connections. Local-only testing misses serialization bugs.
**How to avoid:** Use ParrelSync for FishNet (Tutorial 6). Build standalone + editor for Fusion 2 (Tutorial 7). Test actual message round-trips, not just compilation.
**Warning signs:** `.OverNetwork()` calls that never actually send data, `IsDeserialized` always false.

### Pitfall 5: Photon Fusion 2 Requires Valid AppId
**What goes wrong:** Tutorial 7 scene loads but networking fails immediately.
**Why it happens:** Fusion 2 requires a Photon Dashboard account and valid AppId configured in the project.
**How to avoid:** Set up Photon account and AppId BEFORE validating Tutorial 7. Verify connection in Photon Dashboard.
**Warning signs:** "Failed to start Fusion" error, session not connecting.

### Pitfall 6: XR Device Simulator Not Imported
**What goes wrong:** Tutorial 12 VR scene can't be tested without a physical headset.
**Why it happens:** XR Device Simulator is a Sample that must be explicitly imported from XR Interaction Toolkit in Package Manager. It is NOT currently in the project.
**How to avoid:** Before validating Tutorial 12, import XR Device Simulator: Package Manager > XR Interaction Toolkit > Samples > Import "XR Device Simulator". Also enable the "Use XR Device Simulator in scenes" option in XR Plug-in Management settings.
**Warning signs:** No XR Device Simulator prefab in project, VR controllers don't work in editor.

### Pitfall 7: Tutorials 13-14 "Coming Soon" But Have Substantive Content
**What goes wrong:** Tutorials 13 and 14 are labeled as "Coming Soon" on the wiki but actually contain significant content (code examples, status tables, exercises).
**Why it happens:** Progressive content development -- pages were partially filled out.
**How to avoid:** Verify the "Coming Soon" banner is present. Check that any code examples on these pages are marked as planned/available correctly. Verify no broken links. Do NOT attempt full validation of unfinished features.
**Warning signs:** Code examples referencing unimplemented methods (e.g., `.Throttle()`, `.Debounce()`).

### Pitfall 8: `Shader.Find("Standard")` in Unity 6000
**What goes wrong:** `Shader.Find("Standard")` may return null in Unity 6000 if the project uses URP or another render pipeline, causing pink/missing materials.
**Why it happens:** Unity 6000 render pipeline changes. The Standard shader may not be included if URP is active.
**How to avoid:** Check what render pipeline the project uses. If URP: `Shader.Find("Standard")` won't work -- need `Shader.Find("Universal Render Pipeline/Lit")` instead.
**Warning signs:** Pink objects in scene, null reference on material creation in DSLSceneSetup.cs and other programmatic scene builders.

### Pitfall 9: Wiki-Code Authority Ambiguity
**What goes wrong:** Wiki says one thing, code does another. Need user decision on which to fix.
**Why it happens:** Wiki and code evolved independently.
**How to avoid:** Per user decision: document BOTH options for every mismatch, flag for user review before fixing. Never silently choose one side.
**Warning signs:** Method names, parameter orders, or class hierarchies differ between wiki and code.

## Code Examples

### Pattern: Validation Script for Automated Console Checking (Recommended Approach)
```csharp
// This pattern could be used to automate validation:
// Attach to scene root, press Play, check console for PASS/FAIL

public class TutorialValidator : MonoBehaviour
{
    void Start()
    {
        // Verify hierarchy
        var relay = GetComponent<MmRelayNode>();
        if (relay == null)
        {
            Debug.LogError("[VALIDATION FAIL] Missing MmRelayNode on root object");
            return;
        }

        // Verify children
        var children = GetComponentsInChildren<MmRelayNode>();
        Debug.Log($"[VALIDATION] Found {children.Length} relay nodes in hierarchy");

        // Verify responders
        var responders = GetComponentsInChildren<MmBaseResponder>();
        Debug.Log($"[VALIDATION] Found {responders.Length} responders in hierarchy");
    }
}
```

### Pattern: GitHub Issue Creation via CLI
```bash
# Create issue for a found problem
gh issue create --title "Tutorial 3: Custom method enum value mismatch" \
  --body "Wiki shows MyMethods.Damage = 1001 but T3_MyMethods.cs uses 1000. Fix: update code to match wiki." \
  --label "tutorial-validation"

# Close issue with fix commit
gh issue close 4 --comment "Fixed in commit abc1234"
```

### Pattern: Wiki Push via Git Clone
```bash
# Clone wiki repo (separate from main repo)
git clone https://github.com/ColumbiaCGUI/MercuryMessaging.wiki.git
# Edit .md files in the clone
# Commit and push
cd MercuryMessaging.wiki
git add .
git commit -m "docs(wiki): update Tutorial 1 screenshots for Unity 6000.3.7f1"
git push
```

### Pattern: ParrelSync for FishNet Testing (Tutorial 6)
```
1. Open ParrelSync > Clones Manager
2. Create a clone (if not exists)
3. Open clone in separate Unity Editor instance
4. Main Editor: Start as Host
5. Clone Editor: Start as Client, connect to localhost
6. Verify message round-trip
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Unity 2021.3 target | Unity 6000.3.7f1 (Unity 6.3 LTS) | Project upgraded | Some API deprecations, render pipeline changes |
| `MmRelayNode.MmInvoke()` (traditional) | Fluent DSL API (`relay.Send().ToX().Execute()`) | Framework v4.0 | Both APIs coexist; tutorials teach both |
| `MmBaseResponder` only | + `MmExtendableResponder` for custom methods | Framework v4.0 | Tutorial 3 covers both approaches |
| No XR Device Simulator | XRI 3.3.1 includes Device Simulator sample | XRI evolution | Must import sample for Tutorial 12 |
| PUN (Photon Unity Networking) classic | Photon Fusion 2 | Photon evolution | Tutorial 7 uses Fusion 2, not PUN |

**Deprecated/outdated:**
- Tutorial wiki says "Prerequisites: Unity 2021.3 or later" -- needs update to reflect Unity 6000.3.7f1
- `Physics.autoSyncTransforms` is deprecated in Unity 6.3 -- check if any tutorial code uses it
- VR Module is deprecated in Unity 6.3 (will be removed in 6.5) -- Tutorial 12 should use XR Interaction Toolkit approach, not legacy VR Module

## Key Findings from Wiki Analysis

### Wiki Content Characteristics
- **No embedded images in any tutorial** -- all visual content is ASCII diagrams and code blocks. This simplifies screenshot work since there are no existing images to replace, only new ones to add.
- Wiki pages reference **generic class names** (e.g., `MyResponder`, `GameController`) while actual scripts use **tutorial-prefixed names** (e.g., `T1_ChildResponder`, `T8_GameStateController`). This is the most likely source of wiki-code mismatches.
- Most tutorials provide **keyboard controls** documentation. Validation must test every documented key.
- Expected **console output** is sparsely documented -- usually 1-3 example lines per tutorial. Some tutorials have detailed output, others have none.

### Tutorial Complexity Distribution
- **LOW (1-2)**: Simple hierarchy, basic message routing. Quick to validate.
- **MEDIUM (3, 4, 5, 8, 9, 10, 11)**: Custom code, FSM, task management. Moderate validation effort.
- **HIGH (6, 7, 12)**: Networking requires multi-instance testing; VR requires simulator setup. Significant validation effort.

### Estimated Time Per Tutorial
| Tutorial | Estimated Time | Notes |
|----------|---------------|-------|
| 1: Introduction | 30-45 min | Simple, baseline validation |
| 2: Basic Routing | 30-45 min | Simple routing patterns |
| 3: Custom Responders | 45-60 min | Two approaches to validate |
| 4: Custom Messages | 45-60 min | Custom message types, multiple scripts |
| 5: Fluent DSL API | 60-90 min | 8 keyboard shortcuts, temporal features |
| 6: FishNet Networking | 90-120 min | ParrelSync multi-instance, network validation |
| 7: Fusion 2 Networking | 120-180 min | Photon account setup, AppId config, multi-instance |
| 8: Switch Nodes & FSM | 45-60 min | 6+ keyboard shortcuts, state transitions |
| 9: Task Management | 60-90 min | Task system, JSON loading |
| 10: Application State | 45-60 min | State management, loading simulation |
| 11: Advanced Networking | 60-90 min | Loopback backend, network filters |
| 12: VR Experiment | 90-120 min | XR Device Simulator setup, Go/No-Go task |
| 13-14: Stubs | 15-30 min | Verify "Coming Soon" labels, check links |
| **Total** | **~12-16 hours** | Across all tutorials |

## Open Questions

1. **Render Pipeline**
   - What we know: `DSLSceneSetup.cs` uses `Shader.Find("Standard")`, which may fail under URP
   - What's unclear: Which render pipeline is active in this project (Built-in, URP, or HDRP)
   - Recommendation: Check `ProjectSettings/GraphicsSettings.asset` on first tutorial validation. If URP, fix all `Shader.Find("Standard")` calls.

2. **Photon AppId Status**
   - What we know: Fusion 2 SDK is installed, `FUSION2_AVAILABLE` is defined
   - What's unclear: Whether a valid Photon AppId is already configured or needs fresh setup
   - Recommendation: Check `Assets/Photon/Fusion/Runtime/Resources/` for existing Fusion configuration. If no AppId, user needs to create Photon account.

3. **Wiki Push Access**
   - What we know: User stated "Wiki changes can be pushed directly to the GitHub Wiki repo"
   - What's unclear: Whether wiki repo has been cloned locally, or if it needs to be cloned
   - Recommendation: Clone `https://github.com/ColumbiaCGUI/MercuryMessaging.wiki.git` at phase start.

4. **Tutorial Scene Completeness**
   - What we know: All 12 tutorial folders have `.unity` scene files and scripts
   - What's unclear: Whether scenes are fully wired up (components assigned, hierarchy correct) or just partially built
   - Recommendation: This is what validation will discover. Expect some scenes to need hierarchy fixes.

## Sources

### Primary (HIGH confidence)
- **Project files** - DirectRelyread of all tutorial scripts, scene files, asmdef files, ProjectVersion.txt, manifest.json, ProjectSettings
- **GitHub Wiki** - Fetched full content of tutorials 1, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 via WebFetch
- **Documentation/** - Read OVERVIEW.md, ARCHITECTURE.md, API_REFERENCE.md, WORKFLOWS.md, TESTING.md, PERFORMANCE.md, FREQUENT_ERRORS.md

### Secondary (MEDIUM confidence)
- **Unity upgrade guides** - WebSearch for Unity 6000 breaking changes from Unity 2021: confirmed Enlighten removal, Physics.autoSyncTransforms deprecation, VR Module deprecation in 6.3
- **XR Device Simulator** - WebSearch + official docs confirm it's a Sample in XRI package, must be explicitly imported

### Tertiary (LOW confidence)
- **Shader.Find("Standard") behavior in Unity 6** - Needs project-level verification based on active render pipeline

## Metadata

**Confidence breakdown:**
- Tutorial inventory: HIGH - directly read all files and wiki pages
- Architecture/patterns: HIGH - based on actual project structure
- Pitfalls: HIGH - based on code analysis and known Unity 6000 changes
- Time estimates: MEDIUM - based on complexity assessment but actual times will vary

**Research date:** 2026-02-12
**Valid until:** 2026-03-12 (stable -- project structure unlikely to change during this phase)
