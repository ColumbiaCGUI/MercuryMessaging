# MercuryMessaging Framework Documentation

**MercuryMessaging** is a hierarchical message routing framework for Unity developed by Columbia University's CGUI lab. It enables loosely-coupled communication between GameObjects through a message-based architecture.

*Last Updated: 2026-02-11*
*Framework Version: Based on Unity 2021.3+ with VR/XR support*

---

## Wiki Tutorials (Learning Path)

Step-by-step tutorials for learning MercuryMessaging:

**[GitHub Wiki](https://github.com/ColumbiaCGUI/MercuryMessaging/wiki)**

| # | Tutorial | Description |
|---|----------|-------------|
| 1 | Introduction | Getting started, basic concepts |
| 2 | Basic Routing | Message direction, level filters |
| 3 | Custom Responders | Creating message handlers (`MmBaseResponder`) |
| 4 | Custom Messages | Creating message types (`MmMessage`) |
| 5 | **Fluent DSL API** | Modern API with 77% code reduction |
| 6 | **FishNet Networking** | Primary networking solution |
| 7 | Fusion 2 Networking | Alternative networking with Photon |
| 8 | Switch Nodes & FSM | State machines with `MmRelaySwitchNode` |
| 9 | Task Management | User studies and experimental workflows |
| 10 | Application State | Global state with `MmAppState` |
| 11 | Advanced Networking | Custom backends, binary serialization |
| 12 | VR Experiment | Complete VR behavioral experiment |
| 13-14 | (Coming Soon) | Spatial/Temporal Filtering, Performance |

---

## Technical Reference (Documentation/)

@./Documentation/OVERVIEW.md

@./Documentation/ARCHITECTURE.md

@./Documentation/API_REFERENCE.md

@./Documentation/WORKFLOWS.md

@./Documentation/STANDARD_LIBRARY.md

@./Documentation/TESTING.md

@./Documentation/PERFORMANCE.md

---

## Critical References

@./CONTRIBUTING.md

- [Frequent Errors](Documentation/FREQUENT_ERRORS.md) - Common bugs & debugging patterns (MUST READ)
- [DSL API Guide](Documentation/DSL/API_GUIDE.md) - Comprehensive Fluent API documentation
- [FILE_REFERENCE.md](Documentation/FILE_REFERENCE.md) - Complete list of important files with descriptions
- [Source Generators](Documentation/SourceGenerators/README.md) - `[MmGenerateDispatch]` and `[MmHandler]` attributes

---

## Development Resources

- [Documentation/WORKFLOW.md](Documentation/WORKFLOW.md) - Feature development, bug fixes, testing, release workflows
- [.claude/ASSISTANT_GUIDE.md](.claude/ASSISTANT_GUIDE.md) - Guidelines for AI assistants
- [.planning/ROADMAP.md](.planning/ROADMAP.md) - UIST 2026 project roadmap (GSD)

---

## Development Standards

**Key Standards:**
- Core framework files must use "Mm" prefix (MmRelayNode, MmMessage, etc.)
- Minimize external dependencies (Unity, System.* only)
- All tests must be fully automated (no manual scenes or prefabs)
- Use Conventional Commits format (`feat:`, `fix:`, `docs:`, etc.)

---

## Guidelines for AI Assistants

For AI assistants working on this project, see [.claude/ASSISTANT_GUIDE.md](.claude/ASSISTANT_GUIDE.md).

**Critical Policy:**
- Use Conventional Commits format (`feat:`, `fix:`, `docs:`, etc.)
- Use `.planning/` (GSD) for project planning and phase execution
- See [Documentation/WORKFLOW.md](Documentation/WORKFLOW.md) for development workflow
