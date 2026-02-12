# Phase 1: Tutorial Validation - Context

**Gathered:** 2026-02-12
**Status:** Ready for planning

<domain>
## Phase Boundary

Systematically validate all 12 published tutorial scenes against their GitHub Wiki documentation in Unity 6000.3.7f1. Find and fix all issues so every tutorial runs cleanly with zero errors/warnings and matches documented behavior. Produce a per-step validation report. This phase establishes the trustworthy foundation required for user study training materials (Phase 5).

</domain>

<decisions>
## Implementation Decisions

### Pass/Fail Criteria
- Zero tolerance: any error OR warning in the Unity console counts as a failure
- Code examples from the wiki must compile AND produce documented runtime behavior (not just compile)
- Visual results: capture new screenshots in Unity 6000.3.7f1 and update wiki to match current visuals
- Validate tutorials 1-12 fully; for tutorials 13-14 (Coming Soon), verify stubs are clearly marked and have no broken links

### Fix vs Report
- Fix everything: find issues AND fix them in this phase — deliverable is working tutorials, not just a report
- Framework bugs exposed by tutorials are in scope — fix MercuryMessaging code if a tutorial surfaces a genuine bug
- Fundamentally broken tutorials get rebuilt from scratch — every tutorial must work
- Commits are per-tutorial: one atomic commit per tutorial validated (e.g., `fix(tutorial-3): update routing example to match wiki`)
- Validation order: sequential, tutorials 1-12 in order (earlier tutorials are prerequisites for later ones)

### Special Cases
- VR tutorials (Tutorial 12): validate using Unity XR Device Simulator only — no physical headset required
- Networking tutorials (Tutorial 6: FishNet, Tutorial 11: Advanced Networking): full network test with actual networked instances (host + client, localhost)
- Photon Fusion 2 (Tutorial 7): set up Photon account and AppId as part of this phase, then full runtime validation

### Wiki vs Code Authority
- Case-by-case when wiki and code disagree — no blanket rule
- Every mismatch gets flagged for user review before fixing — Claude documents both options, user decides direction
- Wiki changes can be pushed directly to the GitHub Wiki repo (Claude has access)
- All 12 tutorials are currently published on the wiki

### Validation Report
- Per-step detail: each wiki step validated individually with pass/fail and specific findings
- Report location: `.planning/phases/01-tutorial-validation/VALIDATION_REPORT.md`
- Include estimated completion time per tutorial (informs user study time estimates in Phase 5)
- GitHub issues filed for every failure found, closed with the fix commit (complete audit trail)

### Claude's Discretion
- Exact format and structure of the per-step validation report
- How to organize wiki screenshot updates (inline vs separate commit)
- Prioritization when multiple issues are found in a single tutorial

</decisions>

<specifics>
## Specific Ideas

- Screenshots must be updated to Unity 6000.3.7f1 visuals — wiki should show current state, not legacy
- Per-tutorial atomic commits create a clean git history that maps to each tutorial
- Timing estimates per tutorial feed directly into Phase 5 (User Study) session planning
- GitHub issues create audit trail even for fixed problems — useful for tracking quality over time

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 01-tutorial-validation*
*Context gathered: 2026-02-12*
