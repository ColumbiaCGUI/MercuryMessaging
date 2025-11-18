# Visual Network Composer

**Status:** Ready to Start
**Priority:** MEDIUM-HIGH
**Estimated Effort:** ~360 hours (9 weeks)
**Phase:** 4.2 (Network Construction Tools)

---

## Overview

Visual tools for constructing Mercury message networks, including hierarchy mirroring, template library, drag-and-drop composer, and network validation.

**Core Problem:** Setting up complex message networks is tedious and error-prone. No templates for common patterns. Hard to visualize network structure before runtime.

**Solution:** One-click hierarchy mirroring, template library for common patterns, Unity GraphView-based visual composer, and automatic validation.

---

## Goals

1. One-click GameObject hierarchy → Mercury network mirroring
2. Template library (hub-spoke, chain, broadcast, aggregator, 5+ patterns)
3. Drag-and-drop visual network composer using Unity GraphView
4. Network validator detecting circular dependencies and unreachable nodes
5. 50% reduction in network setup time

---

## Scope

### In Scope
- MmHierarchyMirror editor window
- 5+ network templates (HubAndSpoke, Chain, BroadcastTree, EventAggregator, Custom)
- Visual composer with node-based editing
- Validator with error/warning reporting
- Export to Unity scene
- Performance estimation

### Out of Scope
- Runtime network construction
- Code generation from visual graph
- Integration with visual scripting (Bolt, etc.)

---

## Success Metrics

- [ ] 50% reduction in setup time (measured)
- [ ] Hierarchy mirror works for 100+ node scenes
- [ ] Templates create valid networks 100% of time
- [ ] Visual composer intuitive (user testing)
- [ ] Validator catches 95%+ of common mistakes

---

## Quick Start

See `visual-composer-context.md` for UI mockups and GraphView architecture, then `visual-composer-tasks.md` for editor tool implementation.

Key deliverables: Hierarchy mirroring tool, 5+ templates, visual composer, network validator.

---

**See full details in:**
- `visual-composer-context.md`
- `visual-composer-tasks.md`
