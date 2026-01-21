# Multi-User XR Collaboration Framework

**Status:** Planning
**Priority:** MEDIUM-HIGH (Research Opportunity)
**Estimated Effort:** ~200 hours (5 weeks)
**Target Venues:** CHI, IEEE VR, ISMAR
**Novelty Assessment:** MEDIUM-HIGH

---

## Research Contribution

### Problem Statement

Multi-user XR collaboration systems face challenges in managing communication:
- **Flat user models**: No hierarchical organization (rooms, teams, roles)
- **Limited filtering**: Can't target messages by role, proximity, or group
- **State synchronization**: Complex multi-user state management
- **Scalability issues**: Broadcast-based systems don't scale
- **Permission systems**: No built-in role-based message routing

### Novel Technical Approach

We propose **MercuryMessaging for XR Collaboration** — leveraging hierarchical message routing for multi-user XR:

1. **Hierarchical User Organization**: Users organized in hierarchy (Session > Room > Team > User)
2. **Tag-Based Role Permissions**: Route messages by role (Instructor, Student, Observer)
3. **Spatial Filtering**: Proximity-aware collaboration (message only nearby users)
4. **FSM for Session States**: Built-in state machines for session lifecycle
5. **Selective Synchronization**: Filter which users receive which updates

**Key Differentiation from Existing Work:**
- XR MUSE provides open-source XR but lacks hierarchical filtering
- CoMIC focuses on QoE metrics, not message architecture
- Mercury adds hierarchical organization + role filtering + spatial awareness

---

## Literature Analysis (2020-2025)

### Competing/Related Work

| Paper | Year | Venue | Focus | Limitation | Mercury Differentiation |
|-------|------|-------|-------|------------|-------------------------|
| XR MUSE | 2024 | Virtual Worlds | Open-source multi-user XR | No hierarchical filtering | Role-based message routing |
| CoMIC | 2023 | IEEE Network | Multi-user infrastructure (19 cit) | QoE focus, not message architecture | Hierarchical organization |
| SCAXR | 2024 | IEEE Network | Scalable XR (10 cit) | Scalability focus, basic sync | Multi-level filtering |
| Photon Fusion | 2023 | Commercial | Networking framework | Room-level only, no hierarchy | Full hierarchy + tags + FSM |
| Mirror | 2023 | Open-source | Unity networking | No built-in hierarchy | Scene graph integration |

### Literature Gap Analysis

**What exists:**
- Multi-user XR platforms (XR MUSE) - no hierarchical message routing
- QoE-focused infrastructure (CoMIC) - network performance, not message architecture
- Scalable XR systems (SCAXR) - horizontal scaling, not selective routing
- Game networking (Photon, Mirror) - room-level, no fine-grained filtering

**What doesn't exist:**
- **Hierarchical user organization** (Session > Room > Team > User)
- **Role-based message routing** (Instructor broadcasts to Students only)
- **Tag-based permissions** integrated with XR collaboration
- **Spatial filtering** for proximity-aware multi-user messaging
- **FSM-based session management** for collaborative workflows

### Novelty Claims

1. **FIRST** hierarchical message routing for XR collaboration
2. **FIRST** role-based filtering (Tag system) for multi-user XR
3. **FIRST** integration of spatial indexing with collaborative messaging
4. **FIRST** FSM-based session lifecycle management for XR
5. **Novel** combination of hierarchy + roles + spatial in one framework

### Key Citations

```bibtex
@article{xrmuse2024,
  title={XR MUSE: An Open-Source Multi-User XR Framework},
  journal={Virtual Worlds},
  year={2024}
}

@article{comic2023,
  title={CoMIC: Collaborative Multi-user Infrastructure for XR Communication},
  journal={IEEE Network},
  year={2023},
  note={19 citations}
}

@article{scaxr2024,
  title={SCAXR: Scalable Architecture for Extended Reality},
  journal={IEEE Network},
  year={2024},
  note={10 citations}
}
```

---

## Technical Architecture

### Hierarchical User Organization

```
XR Session (MmRelaySwitchNode)
├── Lobby (MmRelayNode)
│   └── WaitingUsers (MmBaseResponder per user)
├── Room_Lab101 (MmRelayNode)
│   ├── Team_Red (MmRelayNode, Tag.TeamRed)
│   │   ├── User_Alice (MmBaseResponder, Tag.Instructor)
│   │   └── User_Bob (MmBaseResponder, Tag.Student)
│   └── Team_Blue (MmRelayNode, Tag.TeamBlue)
│       └── User_Carol (MmBaseResponder, Tag.Student)
└── Room_Lab102 (MmRelayNode)
    └── ...
```

### Role-Based Message Routing

```csharp
// Tag definitions for roles
public static class XRRoles
{
    public const MmTag Instructor = MmTag.Tag0;
    public const MmTag Student = MmTag.Tag1;
    public const MmTag Observer = MmTag.Tag2;
    public const MmTag Admin = MmTag.Tag3;
}

// Instructor broadcasts to all students in room
roomRelay.MmInvoke(
    new LectureMessage { Content = "Welcome to class" },
    new MmMetadataBlock(
        MmLevelFilter.SelfAndChildren,
        tag: XRRoles.Student  // Only students receive
    )
);

// Student asks question (to instructor only)
userRelay.MmInvoke(
    new QuestionMessage { Text = "Can you explain?" },
    new MmMetadataBlock(
        MmLevelFilter.Parent,  // Up to room, then filter
        tag: XRRoles.Instructor
    )
);
```

### Spatial Proximity Filtering

```csharp
// Send voice message to nearby users only
user.MmInvokeSpatial(
    new VoiceMessage { AudioClip = recordedAudio },
    new SpatialFilter {
        Center = user.transform.position,
        Radius = 5.0f,  // 5 meter range
        ActiveOnly = true
    }
);

// Broadcast to all users in line of sight
user.MmInvokeSpatial(
    new GestureMessage { Type = GestureType.Wave },
    new SpatialFilter {
        Origin = user.transform.position,
        Direction = user.transform.forward,
        ConeAngle = 60f,
        MaxDistance = 20f
    }
);
```

### Session State FSM

```csharp
public class SessionController : MonoBehaviour
{
    private MmRelaySwitchNode _session;

    // Session states
    void ConfigureStates()
    {
        // Lobby → InSession → Review → Complete
        // Each state has different responders active
    }

    public void StartSession()
    {
        _session.RespondersFSM.JumpTo("InSession");
        // Only InSession responders receive messages
        // Lobby users see "Session in progress"
    }

    public void PauseForReview()
    {
        _session.RespondersFSM.JumpTo("Review");
        // Review mode: different UI, restricted actions
    }
}
```

---

## Implementation Plan

### Phase 1: Core User Management (60 hours)
- User join/leave handling
- User hierarchy placement (Room/Team assignment)
- User responder component
- Role assignment and tag setup
- User presence tracking

### Phase 2: Room and Team System (50 hours)
- Room creation/destruction
- Team management within rooms
- Cross-room communication policies
- Room capacity and permissions
- Team switching mechanics

### Phase 3: Role-Based Filtering (40 hours)
- Role tag definitions
- Permission-based message routing
- Role hierarchy (Admin > Instructor > Student)
- Role change during session
- Role-specific UI integration

### Phase 4: Spatial Collaboration (30 hours)
- Integration with Spatial Indexing (P6)
- Proximity-based voice/message routing
- Line-of-sight filtering
- Personal space boundaries
- Spatial awareness indicators

### Phase 5: Testing and Documentation (20 hours)
- Multi-user test scenarios
- Performance benchmarks (latency, scalability)
- Example collaboration scenes
- User documentation

---

## Application Domains

### Educational XR
- Virtual classrooms with instructor/student roles
- Lab simulations with team-based collaboration
- Remote tutoring with spatial presence

### Enterprise Training
- Safety training with supervisor oversight
- Team exercises with role-based tasks
- Performance review with observer roles

### Social VR
- Virtual events with host/attendee roles
- Meeting spaces with spatial audio
- Collaborative workspaces

### Healthcare
- Surgical training with instructor guidance
- Therapy sessions with patient/therapist roles
- Medical consultation with spatial privacy

---

## Success Metrics

- [ ] Support 50+ concurrent users per session
- [ ] Role-based routing accuracy 100%
- [ ] Spatial query latency < 5ms
- [ ] Session state transitions < 100ms
- [ ] User join/leave handling < 500ms
- [ ] Example scenarios: Classroom, Meeting, Training

---

## Dependencies

- MercuryMessaging framework
- MercuryMessaging Network (FishNet or Fusion2)
- Spatial Indexing module (P6, optional)
- Unity XR Interaction Toolkit

---

## Related Files

- [xr-collab-context.md](xr-collab-context.md) - Technical design details
- [xr-collab-tasks.md](xr-collab-tasks.md) - Implementation checklist

---

*Created: 2025-12-17*
*Last Updated: 2025-12-17*
