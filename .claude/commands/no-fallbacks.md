---
description: Audit code for silent fallback anti-patterns
---

Audit the current file or recent changes for silent fallback patterns.

Search for these anti-patterns:
1. Empty catch blocks: `catch { }` or `catch (Exception) { }`
2. Catch returning null without logging: `catch { return null; }`
3. Bare catch without exception type: `catch { ... }`
4. Swallowing exceptions: `catch (Exception e) { Log(e.Message); }` (missing stack trace)
5. Excessive null coalescing hiding errors: `?? default`
6. Silent early returns that skip important logic

For each found:
- Report file:line
- Show the problematic code
- Suggest the proper fail-fast fix

Use grep patterns:
- `catch\s*\{`
- `catch\s*\(\s*\)`
- `e\.Message[^}]*\}`
