# Standard Library Task - Merged into DSL Overhaul

**Archived:** 2025-11-25
**Merged Into:** `dev/active/dsl-overhaul/`

## Reason for Merge

The Standard Library task (228 hours) was merged into the DSL Overhaul plan because they are complementary:
- **DSL Overhaul** = HOW to send messages (fluent API, extension methods)
- **Standard Library** = WHAT message types exist (40+ standardized types)

## Merge Strategy

| Standard Library Phase | Merged Into | Status |
|------------------------|-------------|--------|
| Phase 1: Versioning System | DSL Phase 1 (Optional) | Deferred |
| Phase 2: UI Messages | New Phase 9 | Future |
| Phase 3: AppState Messages | DSL Phase 8 | Future |
| Phase 4: Input Messages | New Phase 10 | Future |
| Phase 5: Task Messages | DSL Phase 4 | Future |
| Phase 6: Compatibility | Deferred | Future |
| Phase 7: Documentation | Ongoing | Continuous |

## Key Concepts Retained

1. Message versioning attribute (`[MmMessageVersion(1, 0)]`)
2. Namespace structure (`MercuryMessaging.StandardLibrary.{UI, AppState, Input, Task}`)
3. Example responders for common patterns

## Reference

See `.claude/plans/compressed-wandering-fog.md` for the full merged plan.
