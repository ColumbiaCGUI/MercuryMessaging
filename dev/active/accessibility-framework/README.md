# Accessibility-First Game Framework

**Status:** Planning
**Priority:** MEDIUM-HIGH (Research Opportunity)
**Estimated Effort:** ~220 hours (5.5 weeks)
**Target Venues:** CHI, ASSETS, CHI PLAY
**Novelty Assessment:** MEDIUM-HIGH

---

## Research Contribution

### Problem Statement

Game accessibility remains challenging due to:
- **Retrofitting accessibility**: Games designed without accessibility in mind
- **Inconsistent implementation**: Each game implements accessibility differently
- **Limited AT integration**: Assistive technologies poorly supported in games
- **No standard patterns**: No framework for accessibility-first game development
- **Input diversity**: Many input modalities not easily supported

### Novel Technical Approach

We propose **Mercury Accessibility Framework** — accessibility-first patterns using hierarchical message routing:

1. **Message Translation Layer**: Automatic conversion for assistive technologies
2. **Tag-Based Accessibility Modes**: Route messages based on accessibility settings
3. **Alternative Input Routing**: Voice → message, gesture → message, switch → message
4. **FSM for Accessibility States**: Manage accessibility mode transitions
5. **Standardized Accessibility Messages**: Common patterns for game accessibility

**Key Differentiation from Existing Work:**
- No existing framework for message-based game accessibility
- Mercury provides infrastructure for accessibility patterns
- First to apply hierarchical messaging to assistive technology integration

---

## Literature Analysis (2020-2025)

### Competing/Related Work

| Paper | Year | Venue | Focus | Limitation | Mercury Differentiation |
|-------|------|-------|-------|------------|-------------------------|
| Salehnamadi et al. | 2023 | CHI | AT-aided testing (15 cit) | Testing focus, not framework | Development framework |
| Accessible Metaverse | 2024 | MTI | Metaverse framework (36 cit) | Conceptual, not implementation | Concrete implementation |
| Mobile Game Accessibility | 2021 | JGGAG | Hand posture recognition | Mobile-specific | Platform-agnostic |
| WCAG Game Guidelines | 2023 | W3C | Guidelines only | No implementation framework | Implementation patterns |
| Xbox Accessibility | 2023 | Industry | Controller remapping | Platform-specific | Engine-integrated |

### Literature Gap Analysis

**What exists:**
- Accessibility testing tools (AT-aided testing) - testing, not development
- Conceptual frameworks (Accessible Metaverse) - no implementation
- Platform-specific solutions (Xbox, PlayStation) - not engine-integrated
- Guidelines (WCAG, CVAA) - no code-level framework

**What doesn't exist:**
- **Message-based accessibility framework** for game development
- **Tag-based accessibility mode filtering** integrated with game logic
- **Alternative input → message translation** standardized approach
- **FSM-based accessibility state management** for mode transitions
- **Engine-integrated** accessibility patterns (Unity, Unreal)

### Novelty Claims

1. **FIRST** message-based accessibility framework for game engines
2. **FIRST** application of tag-based filtering to accessibility modes
3. **FIRST** standardized alternative input → message translation
4. **FIRST** FSM-based accessibility state management
5. **Novel** integration with Unity scene graph for accessibility

### Key Citations

```bibtex
@inproceedings{salehnamadi2023,
  title={AT-aided Testing for Accessible Mobile Games},
  booktitle={CHI Conference on Human Factors in Computing Systems},
  year={2023},
  note={15 citations}
}

@article{accessible_metaverse2024,
  title={Toward an Accessible Metaverse: Frameworks and Challenges},
  journal={Multimodal Technologies and Interaction (MTI)},
  year={2024},
  note={36 citations}
}

@article{mobile_game_accessibility2021,
  title={Hand Posture Recognition for Mobile Game Accessibility},
  journal={Journal of Gaming and Gamification for Accessibility (JGGAG)},
  year={2021}
}
```

---

## Technical Architecture

### Accessibility Mode Hierarchy

```
GameRoot (MmRelaySwitchNode)
├── StandardMode (MmRelayNode)
│   └── StandardResponders...
├── HighContrastMode (MmRelayNode, Tag.Visual)
│   └── HighContrastResponders...
├── ScreenReaderMode (MmRelayNode, Tag.Auditory)
│   └── ScreenReaderResponders...
└── MotorAccessibilityMode (MmRelayNode, Tag.Motor)
    └── MotorAccessibilityResponders...
```

### Tag-Based Accessibility Filtering

```csharp
// Accessibility tags
public static class AccessibilityTags
{
    public const MmTag Visual = MmTag.Tag0;      // Color blind, high contrast
    public const MmTag Auditory = MmTag.Tag1;    // Deaf, hard of hearing
    public const MmTag Motor = MmTag.Tag2;       // Limited mobility
    public const MmTag Cognitive = MmTag.Tag3;   // Cognitive assistance
}

// Send message only to visual accessibility handlers
gameRoot.MmInvoke(
    new UIUpdateMessage { Element = "HealthBar", Value = 75 },
    new MmMetadataBlock(tag: AccessibilityTags.Visual)
);
```

### Alternative Input Translation

```csharp
// Voice → Message translation
public class VoiceToMessageBridge : MonoBehaviour
{
    public void OnVoiceCommand(string command)
    {
        MmMessage message = command switch {
            "jump" => new MmMessageBool { method = MmMethod.MessageBool, value = true },
            "attack" => new MmMessage { method = (MmMethod)1001 },
            "pause" => new MmMessage { method = MmMethod.Switch },
            _ => null
        };

        if (message != null)
        {
            _relay.MmInvoke(message);
        }
    }
}

// Switch control → Message translation
public class SwitchToMessageBridge : MonoBehaviour
{
    public void OnSwitchPress(int switchId)
    {
        var message = _switchMapping[switchId];
        _relay.MmInvoke(message);
    }
}
```

---

## Implementation Plan

### Phase 1: Accessibility Message Types (50 hours)
- Define standard accessibility messages
- Create message translation layer
- Implement accessibility responder base class
- Add accessibility tag definitions
- Create message conversion helpers

### Phase 2: Alternative Input Routing (60 hours)
- Voice command → message bridge
- Switch control → message bridge
- Eye tracking → message bridge
- Gesture recognition → message bridge
- Input configuration system

### Phase 3: Mode Management (40 hours)
- FSM for accessibility modes
- Mode switching UI
- Mode-specific responders
- Settings persistence
- Mode combination support

### Phase 4: AT Integration (50 hours)
- Screen reader output messages
- Haptic feedback messages
- Audio description system
- High contrast mode responders
- Captioning system

### Phase 5: Testing and Documentation (20 hours)
- Automated accessibility tests
- AT compatibility testing
- Example accessible game
- Developer documentation
- Accessibility guidelines

---

## Success Metrics

- [ ] Support 4+ accessibility modes
- [ ] 3+ alternative input methods
- [ ] Screen reader integration working
- [ ] Mode switching < 100ms
- [ ] Example accessible game scene
- [ ] Developer accessibility guide

---

## Related Files

- [accessibility-context.md](accessibility-context.md) - Technical design
- [accessibility-tasks.md](accessibility-tasks.md) - Implementation checklist

---

*Created: 2025-12-17*
*Last Updated: 2025-12-17*
