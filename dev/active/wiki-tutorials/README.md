# Wiki Tutorials Task

**Status:** Active
**Priority:** P3 (DSL/DX Improvements)
**Estimated Effort:** 40 hours
**Created:** 2025-12-01
**Last Updated:** 2025-12-01

---

## Overview

Add new tutorials to the existing MercuryMessaging wiki to document new features (FishNet, Fluent DSL, Spatial/Temporal filtering, etc.) while preserving the existing tutorial structure.

**Wiki Location:** https://github.com/ColumbiaCGUI/MercuryMessaging/wiki

---

## Existing Wiki Structure (Keep Unchanged)

| Tutorial | Topic |
|----------|-------|
| 1 | Introduction to Mercury Messaging |
| 2 | Basic Routing |
| 3 | Creating your own Mercury responders |
| 4 | Creating new Mercury Messages |
| 5a | Basic networking with Photon Fusion V2 |
| 5b | Basic networking with Photon PUN2 |
| 6 | Switch Nodes and Advanced Routing |
| 7 | Task Management |
| 8 | Application State Management |
| 9 | Advanced Networking |
| 10 | VR Behavioral Experiment (Go No-Go Task) |

---

## New Tutorials to Create

| Tutorial | Topic | Features |
|----------|-------|----------|
| **5c** | Basic networking with FishNet | FishNetBackend, FishNetResolver |
| **11** | Fluent DSL API | `relay.To.Children.Send()`, 86% verbosity reduction |
| **12** | Spatial & Temporal Filtering | Within(), InCone(), After(), Every() |
| **13** | MmExtendableResponder Pattern | Registration-based handlers, no switch statements |
| **14** | Performance Optimization | PerformanceMode, source generators |

---

## New Topic Pages to Create

| Topic | Description |
|-------|-------------|
| Hierarchy Query DSL | MmQuery, FindDescendant<T>() |
| Message Listeners | Listen<T>().OnReceived() pattern |
| Migration to Fluent DSL | Traditional → DSL conversion guide |

---

## Setup Instructions

```bash
# Clone wiki repo
cd C:\Users\yangb\Research
git clone https://github.com/ColumbiaCGUI/MercuryMessaging.wiki.git

# After edits, push changes
cd MercuryMessaging.wiki
git add .
git commit -m "docs: Add new tutorials for FishNet, DSL, etc."
git push
```

---

## Source Files for Content

| Category | Files |
|----------|-------|
| FishNet | Tests/Network/README.md, networking-context.md |
| DSL | Protocol/DSL/DSL_API_GUIDE.md, README.md |
| Tutorials | Examples/Tutorials/DSL/*.cs |
| Core | Documentation/OVERVIEW.md, ARCHITECTURE.md |

---

*Task created from plan: functional-sniffing-mitten.md*
