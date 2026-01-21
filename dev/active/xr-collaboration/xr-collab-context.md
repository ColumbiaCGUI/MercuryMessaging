# XR Collaboration - Technical Context

This document provides technical context for implementing multi-user XR collaboration using MercuryMessaging.

---

## Core Concept: Hierarchical vs Flat User Models

### Traditional XR Collaboration (Flat)
```
Session
├── User1
├── User2
├── User3
└── User4
// All users receive all messages (broadcast)
```

### Mercury XR Collaboration (Hierarchical)
```
Session (MmRelaySwitchNode)
├── Room_A (MmRelayNode)
│   ├── Team_Red (Tag.TeamRed)
│   │   ├── Alice (Tag.Instructor)
│   │   └── Bob (Tag.Student)
│   └── Team_Blue (Tag.TeamBlue)
│       └── Carol (Tag.Student)
└── Room_B (MmRelayNode)
    └── ...
// Targeted messages by room, team, role, proximity
```

---

## User Representation

### XR User Responder

```csharp
public class MmXRUserResponder : MmBaseResponder
{
    [Header("User Identity")]
    public string UserId;
    public string DisplayName;
    public MmTag Role = XRRoles.Student;

    [Header("Presence")]
    public bool IsPresent = true;
    public Vector3 HeadPosition;
    public Quaternion HeadRotation;

    protected override void Awake()
    {
        base.Awake();
        Tag = Role;
        TagCheckEnabled = true;
    }

    // Handle incoming messages based on role
    protected override void ReceivedMessage(MmMessageString message)
    {
        // Display chat message, notification, etc.
    }

    // Broadcast position updates
    public void BroadcastPresence()
    {
        GetComponent<MmRelayNode>()?.MmInvoke(
            new PresenceMessage {
                UserId = UserId,
                Position = HeadPosition,
                Rotation = HeadRotation
            },
            new MmMetadataBlock(
                MmLevelFilter.Parent,
                MmNetworkFilter.All
            )
        );
    }
}
```

---

## Room and Team Management

### Room Controller

```csharp
public class MmXRRoom : MonoBehaviour
{
    public string RoomId;
    public int MaxCapacity = 20;
    public List<MmXRUserResponder> Users { get; } = new();

    private MmRelayNode _relay;

    public bool JoinRoom(MmXRUserResponder user)
    {
        if (Users.Count >= MaxCapacity) return false;

        user.transform.SetParent(transform);
        Users.Add(user);
        _relay.MmRefreshResponders();

        // Announce to room
        _relay.MmInvoke(new UserJoinedMessage { User = user });
        return true;
    }

    public void LeaveRoom(MmXRUserResponder user)
    {
        Users.Remove(user);
        user.transform.SetParent(null);
        _relay.MmRefreshResponders();

        _relay.MmInvoke(new UserLeftMessage { UserId = user.UserId });
    }

    // Room-wide broadcast
    public void Broadcast(MmMessage message, MmTag? roleFilter = null)
    {
        var metadata = new MmMetadataBlock(
            MmLevelFilter.SelfAndChildren,
            tag: roleFilter ?? MmTag.Everything
        );
        _relay.MmInvoke(message, metadata);
    }
}
```

---

## Role Permission Matrix

| Action | Admin | Instructor | Student | Observer |
|--------|-------|------------|---------|----------|
| Create Room | ✓ | ✓ | ✗ | ✗ |
| Delete Room | ✓ | ✗ | ✗ | ✗ |
| Broadcast to All | ✓ | ✓ | ✗ | ✗ |
| Broadcast to Students | ✓ | ✓ | ✗ | ✗ |
| Send to Team | ✓ | ✓ | ✓ | ✗ |
| Send to Nearby | ✓ | ✓ | ✓ | ✗ |
| Change User Role | ✓ | ✗ | ✗ | ✗ |
| View Only | ✗ | ✗ | ✗ | ✓ |

---

## Spatial Collaboration Patterns

### Proximity Voice Chat

```csharp
// Voice attenuates with distance
public void SendVoice(AudioClip clip)
{
    var spatialMsg = new SpatialVoiceMessage {
        Audio = clip,
        Origin = transform.position,
        MaxRange = 10f,
        FalloffCurve = AnimationCurve.EaseInOut(0, 1, 10, 0)
    };

    GetComponent<MmRelayNode>().MmInvokeSpatial(
        spatialMsg,
        new SpatialFilter { Radius = 10f }
    );
}
```

### Shared Object Manipulation

```csharp
// Only nearby users can manipulate object
public class SharedObject : MmBaseResponder
{
    public float InteractionRadius = 2f;

    public void RequestGrab(MmXRUserResponder requester)
    {
        // Check if requester is within range
        if (Vector3.Distance(transform.position, requester.transform.position) > InteractionRadius)
        {
            return; // Too far
        }

        // Notify nearby users of grab
        GetComponent<MmRelayNode>().MmInvokeSpatial(
            new GrabMessage { ObjectId = gameObject.name, GrabberId = requester.UserId },
            new SpatialFilter { Radius = InteractionRadius * 2 }
        );
    }
}
```

---

## Network Synchronization

### Presence Updates

```csharp
// Periodic presence broadcast (30Hz)
IEnumerator BroadcastPresenceLoop()
{
    while (IsPresent)
    {
        BroadcastPresence();
        yield return new WaitForSeconds(1f / 30f);
    }
}
```

### State Synchronization

```csharp
// Room state sync on join
public void SyncNewUser(MmXRUserResponder newUser)
{
    // Send current room state to new user only
    newUser.GetComponent<MmRelayNode>().MmInvoke(
        new RoomStateMessage {
            Users = Users.Select(u => u.GetState()).ToList(),
            Objects = SharedObjects.Select(o => o.GetState()).ToList()
        }
    );
}
```

---

## Session State Flow

```
┌─────────┐     ┌───────────┐     ┌────────┐     ┌──────────┐
│  Lobby  │────►│ InSession │────►│ Review │────►│ Complete │
└─────────┘     └───────────┘     └────────┘     └──────────┘
     │                │                │
     │                │                │
  Waiting         Active            Paused
  Users          Collaboration      Summary
```

Each state has different active responders and message routing rules.

---

*Last Updated: 2025-12-17*
