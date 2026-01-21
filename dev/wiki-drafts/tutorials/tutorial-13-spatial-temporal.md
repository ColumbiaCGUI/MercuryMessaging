# Tutorial 13: Spatial & Temporal Filtering

> **Coming Soon**
>
> This tutorial is under development. Check back for updates.

## Planned Content

This tutorial will cover advanced message filtering based on spatial and temporal criteria.

### Spatial Filtering

Filter messages based on distance and direction:

```csharp
// Target objects within radius (planned)
relay.Send("Pulse")
    .ToDescendants()
    .Within(10f)  // Meters
    .Execute();

// Target objects in a cone (planned)
relay.Send("Alert")
    .ToDescendants()
    .InCone(transform.forward, 45f, 20f)  // Direction, angle, range
    .Execute();

// Target objects in bounds (planned)
relay.Send("Activate")
    .ToDescendants()
    .InBounds(myBounds)
    .Execute();
```

### Temporal Filtering

Control message timing and frequency:

```csharp
// Delayed execution (available)
relay.After(2f, MmMethod.Initialize);

// Repeating messages (available)
relay.Every(1f, MmMethod.Refresh, repeatCount: 5);

// Conditional execution (available)
relay.When(() => isReady, MmMethod.Initialize);

// Throttling (planned)
relay.Send("Update")
    .ToDescendants()
    .Throttle(0.1f)  // Max once per 100ms
    .Execute();

// Debouncing (planned)
relay.Send("Search")
    .ToDescendants()
    .Debounce(0.5f)  // Wait for input to settle
    .Execute();
```

### Use Cases

- **Proximity Detection**: Alert nearby enemies
- **Area Effects**: Apply damage in radius
- **Cooldowns**: Prevent ability spam
- **Delayed Actions**: Timed bombs, countdowns
- **Animation**: Sequenced effects

---

## Current Status

| Feature | Status |
|---------|--------|
| `After()` | Available |
| `Every()` | Available |
| `When()` | Available |
| `Within()` | Planned |
| `InCone()` | Planned |
| `InBounds()` | Planned |
| `Throttle()` | Planned |
| `Debounce()` | Planned |

---

## Try This

Practice with the available temporal features:

1. **Create a countdown timer** - Use `After()` to implement a 3-2-1-Go countdown that sends different messages at each second, then triggers a "Start" message.

2. **Build a heartbeat system** - Use `Every()` to send a "Heartbeat" message every 2 seconds to all descendants. Create a responder that logs when it receives heartbeats.

3. **Implement conditional activation** - Use `When()` to wait for a player to enter a trigger zone before sending an "ActivateTrap" message. Test that the message only fires once the condition is met.

4. **Combine temporal methods** - Create a sequence that waits 2 seconds, then sends 5 pulses at 0.5 second intervals, then waits for a condition before sending a final "Complete" message.

---

## Related Resources

- **[Tutorial 5: Fluent DSL API](Tutorial-5-Fluent-DSL-API)** - Current DSL features
- **[API Reference](../Documentation/API_REFERENCE.md)** - Filtering system details

---

*Tutorial 13 of 14 - MercuryMessaging Wiki*
