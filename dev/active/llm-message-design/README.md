# LLM-Assisted Message Network Design

**Status:** Planning
**Priority:** MEDIUM (Research Opportunity)
**Estimated Effort:** ~200 hours (5 weeks)
**Target Venues:** CHI, UIST
**Novelty Assessment:** MEDIUM (narrowed scope required)

---

## Research Contribution

### Problem Statement

Designing Mercury message routing configurations requires understanding:
- Hierarchy organization (which nodes go where)
- Tag allocation (which tags for which components)
- Filter configuration (Level, Active, Selected, Network)
- Routing table setup (parent-child relationships)

This is tedious and error-prone, especially for complex scenarios.

### CRITICAL: Narrow Scope

**This research focuses ONLY on message routing configuration**, NOT general code generation.

LLM→game code is a **crowded research area** (2024-2025 explosion):
- DreamGarden: NL→Unreal general code
- UniGen: Multi-agent zero-code Unity
- Chat Game Engine: NL→game mechanics

**Our differentiation:** LLM for **message network topology authoring only**.

### Novel Technical Approach

We propose **LLM-Assisted Mercury Configuration** — natural language to routing configuration:

**Input:** "When player enters room, notify lights and play ambient sound"

**Output:**
```csharp
// Generated Mercury routing configuration
// Room: MmRelayNode, Tag.Room
// Lights: MmBaseResponder, Tag.Lighting, parent=Room
// AmbientAudio: MmBaseResponder, Tag.Audio, parent=Room

// Routing rule:
room.MmInvoke(
    new PlayerEnterMessage(),
    new MmMetadataBlock(
        MmLevelFilter.SelfAndChildren,
        tag: Tag.Lighting | Tag.Audio
    )
);
```

**Key Differentiation:**
- NOT general code generation (crowded)
- ONLY message routing configuration
- Mercury-specific constraints and patterns
- Validation against Mercury semantics

---

## Literature Analysis (2020-2025)

### Competing/Related Work

| Paper | Year | Venue | Focus | Limitation | Mercury Differentiation |
|-------|------|-------|-------|------------|-------------------------|
| DreamGarden | 2024 | CHI | NL→Unreal general code | General code generation | Message routing only |
| UniGen | 2025 | arXiv | Multi-agent Unity | Zero-code full games | Configuration only |
| Chat Game Engine | 2024 | ACL | NL→game mechanics | General mechanics | Routing configuration |
| MicroVerse | 2025 | ACS | NL→Unity biological | Domain-specific | Framework-specific |
| Automated Unity GDD | 2025 | arXiv | GDD→Unity templates | Template-based | Live configuration |

### Literature Gap Analysis

**What exists (VERY CROWDED):**
- NL→Unreal code (DreamGarden)
- NL→Unity games (UniGen, MicroVerse)
- NL→game mechanics (Chat Game Engine)
- Template-based generation

**What doesn't exist:**
- LLM for **message routing topology** specifically
- NL→Mercury configuration (framework-specific)
- Validation against **message system semantics**
- Integration with **existing Mercury codebase**

### Novelty Claims (Narrowed)

1. **FIRST** LLM assistant for message routing configuration (not general code)
2. **FIRST** NL→Mercury-specific configuration generation
3. **FIRST** validation against hierarchical message system semantics
4. **Novel** integration with Mercury's filtering system
5. **Differentiated** by narrow scope vs broad code generation

### Key Citations

```bibtex
@inproceedings{dreamgarden2024,
  title={DreamGarden: A Generative AI System for Game Development},
  booktitle={CHI Conference on Human Factors in Computing Systems},
  year={2024}
}

@article{unigen2025,
  title={UniGen: Multi-Agent Zero-Code Unity Game Development},
  journal={arXiv preprint},
  year={2025}
}

@inproceedings{chatgameengine2024,
  title={Chat Game Engine: Natural Language to Game Mechanics},
  booktitle={Association for Computational Linguistics (ACL)},
  year={2024}
}
```

---

## Technical Architecture

### Input Processing

```
Natural Language Input
         │
         ▼
┌─────────────────┐
│  Intent Parser  │ ── "when X happens, notify Y"
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Entity Extractor│ ── player, room, lights, sound
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Relationship    │ ── spatial (enters), hierarchical (in)
│ Analyzer        │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Mercury Config  │ ── Nodes, Tags, Filters, Routes
│ Generator       │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Semantic        │ ── Validate against Mercury rules
│ Validator       │
└─────────────────┘
```

### LLM Prompt Engineering

```
System: You are a Mercury Messaging configuration assistant.
Mercury uses hierarchical message routing with these concepts:
- MmRelayNode: Routes messages through scene graph
- MmBaseResponder: Receives and handles messages
- MmMetadataBlock: Filters by Level, Active, Tag, Network
- Tags: Tag0-Tag7 for categorization
- Level filters: Self, Child, Parent, SelfAndChildren

Given a natural language description, generate:
1. Hierarchy structure (which nodes, parent-child)
2. Tag assignments (which entities get which tags)
3. Routing configuration (filters, directions)

User: "When player collects coin, update score display and play sound"
```

---

## Implementation Plan

### Phase 1: Proof of Concept (60 hours)
- Design LLM prompt templates for Mercury concepts
- Implement basic NL→configuration mapping
- Create validation against Mercury semantics
- Test with simple routing scenarios

### Phase 2: Configuration Generator (60 hours)
- Build hierarchy structure generator
- Implement tag assignment logic
- Create filter configuration engine
- Add routing table generation

### Phase 3: Unity Integration (40 hours)
- Create Editor window for LLM interaction
- Implement scene modification from generated config
- Add preview/undo functionality
- Create component placement system

### Phase 4: Validation & Testing (40 hours)
- Semantic validation against Mercury rules
- User study with game developers
- Compare against manual configuration time
- Document best practices and limitations

---

## Success Metrics

- [ ] 80%+ accuracy on routing configuration generation
- [ ] 50%+ time reduction vs manual configuration
- [ ] Support for 5+ common game patterns
- [ ] Semantic validation catches 90%+ errors
- [ ] User study with 10+ participants

---

## Dependencies

- P9 Visual Composer (optional) - Could provide visual feedback
- Mercury core framework (required)
- LLM API access (OpenAI/Anthropic)

---

## Related Files

- [llm-design-context.md](llm-design-context.md) - Technical design details
- [llm-design-tasks.md](llm-design-tasks.md) - Implementation checklist

---

*Created: 2025-12-17*
*Last Updated: 2025-12-17*
