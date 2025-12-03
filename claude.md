# MercuryMessaging Framework Documentation

**MercuryMessaging** is a hierarchical message routing framework for Unity developed by Columbia University's CGUI lab. It enables loosely-coupled communication between GameObjects through a message-based architecture.

*Last Updated: 2025-11-25*
*Framework Version: Based on Unity 2021.3+ with VR/XR support*

---

## Documentation

@./Documentation/OVERVIEW.md

@./Documentation/ARCHITECTURE.md

@./Documentation/API_REFERENCE.md

@./Documentation/WORKFLOWS.md

@./Documentation/STANDARD_LIBRARY.md

@./Documentation/TESTING.md

@./Documentation/PERFORMANCE.md

---

## Critical References

- [Frequent Errors](dev/FREQUENT_ERRORS.md) - Common bugs & debugging patterns (MUST READ)
- [DSL API Guide](Documentation/DSL/API_GUIDE.md) - Comprehensive Fluent API documentation
- [FILE_REFERENCE.md](FILE_REFERENCE.md) - Complete list of important files with descriptions
- [CONTRIBUTING.md](CONTRIBUTING.md) - Development standards, naming conventions, testing guidelines

---

## Development Resources

- [dev/WORKFLOW.md](dev/WORKFLOW.md) - Feature development, bug fixes, testing, release workflows
- [dev/IMPROVEMENT_TRACKER.md](dev/IMPROVEMENT_TRACKER.md) - Completed improvements, active development, and research opportunities
- [.claude/ASSISTANT_GUIDE.md](.claude/ASSISTANT_GUIDE.md) - Guidelines for AI assistants

---

## Development Standards

For complete development standards, guidelines, and contribution process, see [CONTRIBUTING.md](CONTRIBUTING.md).

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
- Create task folders in `dev/active/` for large tasks (README, context, tasks)
- See [dev/WORKFLOW.md](dev/WORKFLOW.md) for development workflow
