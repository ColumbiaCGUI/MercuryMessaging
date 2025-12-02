---
description: Check uncommitted changes against project rules and run tests
---

Check all uncommitted changes against project rules and requirements.

1. Run `git status` to see all changes
2. Run `git diff` to review actual changes
3. Check for violations of:
   - Coding standards (no bare catch, full exception logging)
   - Security vulnerabilities
   - Silent fallback patterns
   - Incomplete implementations
4. Run the project's test suite
5. Report any issues found with file:line references
