# Tutorial 3: Creating Custom Responders

## Overview

While MercuryMessaging provides standard message types (string, int, float, etc.), real applications often need custom behavior. This tutorial shows how to create responders that handle custom methods--your own application-specific actions.

## What You'll Learn

- Creating custom **MmMethod** values (IDs 1000+)
- Handling custom methods with `MmInvoke` override
- **Modern approach**: Using `MmExtendableResponder` (50% less code)
- Best practices for custom responders

## Prerequisites

- Completed [Tutorial 1](Tutorial-1-Introduction) and [Tutorial 2](Tutorial-2-Basic-Routing)
- Understanding of basic message routing

---

## Understanding Custom Methods

MercuryMessaging reserves method IDs 0-99 for standard operations. For custom methods, use IDs **1000 and above**:

| Range | Purpose | Examples |
|-------|---------|----------|
| 0-18 | Standard MmMethod | Initialize, SetActive, MessageString, etc. |
| 100-199 | UI Messages | Click, Hover, Drag (StandardLibrary) |
| 200-299 | Input Messages | 6DOF, Gesture, Haptic (StandardLibrary) |
| **1000+** | **Your Custom Methods** | TakeDamage, ChangeColor, PlaySound, etc. |

---

## Approach 1: Traditional Override (MmBaseResponder)

### Step 1: Define Custom Method Constants

Create a file for your custom methods:

```csharp
// MyCustomMethods.cs
public static class MyMethods
{
    // Start at 1000 to avoid conflicts
    public const int TakeDamage = 1000;
    public const int ChangeColor = 1001;
    public const int EnableGravity = 1002;
    public const int Heal = 1003;
    public const int PlaySound = 1004;
}
```

> **Tutorial Scene:** The actual script is `T3_MyMethods.cs` (with `T3_` prefix per tutorial naming convention).

### Step 2: Override MmInvoke

Handle your custom methods by overriding `MmInvoke`:

```csharp
using UnityEngine;
using MercuryMessaging;

public class EnemyResponder : MmBaseResponder
{
    [SerializeField] private int health = 100;
    [SerializeField] private Renderer meshRenderer;

    public override void MmInvoke(MmMessage message)
    {
        // IMPORTANT: Always call base first for standard methods!
        base.MmInvoke(message);

        // Handle custom methods
        switch ((int)message.MmMethod)
        {
            case MyMethods.TakeDamage:
                HandleDamage((MmMessageInt)message);
                break;

            case MyMethods.ChangeColor:
                HandleColorChange((MmMessageVector3)message);
                break;

            case MyMethods.EnableGravity:
                HandleGravity((MmMessageBool)message);
                break;

            case MyMethods.Heal:
                HandleHeal((MmMessageInt)message);
                break;
        }
    }

    private void HandleDamage(MmMessageInt msg)
    {
        health -= msg.value;
        Debug.Log($"[{gameObject.name}] Took {msg.value} damage. Health: {health}");

        if (health <= 0)
        {
            Debug.Log($"[{gameObject.name}] Died!");
            // Notify parent of death
            var relay = GetComponent<MmRelayNode>();
            if (relay != null)
            {
                relay.Send("EnemyDied").ToParents().Execute();
            }
            Destroy(gameObject);
        }
    }

    private void HandleColorChange(MmMessageVector3 msg)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = new Color(msg.value.x, msg.value.y, msg.value.z);
            Debug.Log($"[{gameObject.name}] Color changed to ({msg.value.x}, {msg.value.y}, {msg.value.z})");
        }
    }

    private void HandleGravity(MmMessageBool msg)
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = msg.value;
            Debug.Log($"[{gameObject.name}] Gravity {(msg.value ? "enabled" : "disabled")}");
        }
    }

    private void HandleHeal(MmMessageInt msg)
    {
        health += msg.value;
        Debug.Log($"[{gameObject.name}] Healed {msg.value}. Health: {health}");
    }

    // Standard methods still work via base class
    public override void Initialize()
    {
        base.Initialize();
        Debug.Log($"[{gameObject.name}] Initialized with {health} health");
    }
}
```

> **Tutorial Scene:** The actual script is `T3_EnemyResponder.cs` (with `T3_` prefix).

**Expected Output (when TakeDamage is sent with value 10):**
```
[Enemy1] Took 10 damage. Health: 90
```

### Step 3: Send Custom Messages

```csharp
using UnityEngine;
using MercuryMessaging;

public class GameManager : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
    }

    public void DamageAllEnemies(int damage)
    {
        // Send custom TakeDamage method with int payload
        relay.Send((MmMethod)MyMethods.TakeDamage, damage)
            .ToDescendants()
            .Execute();
    }

    public void ChangeEnemyColors(Color color)
    {
        relay.Send((MmMethod)MyMethods.ChangeColor,
            new Vector3(color.r, color.g, color.b))
            .ToDescendants()
            .Execute();
    }
}
```

---

## Approach 2: MmExtendableResponder (Recommended)

**`MmExtendableResponder`** provides a cleaner, registration-based approach that eliminates switch statements and reduces boilerplate by 50%.

### Benefits

- No switch statements - Cleaner code
- Can't forget base.MmInvoke() - Automatic delegation
- Dynamic handlers - Add/remove at runtime
- 50% less code - No boilerplate
- Same performance - Fast path < 200ns, slow path < 500ns

### Step 1: Extend MmExtendableResponder

```csharp
using UnityEngine;
using MercuryMessaging;

public class EnemyResponderExtendable : MmExtendableResponder
{
    [SerializeField] private int health = 100;
    [SerializeField] private Renderer meshRenderer;

    public override void Awake()
    {
        base.Awake();

        // Register custom handlers - clean and explicit!
        RegisterCustomHandler((MmMethod)MyMethods.TakeDamage, OnTakeDamage);
        RegisterCustomHandler((MmMethod)MyMethods.ChangeColor, OnChangeColor);
        RegisterCustomHandler((MmMethod)MyMethods.EnableGravity, OnEnableGravity);
        RegisterCustomHandler((MmMethod)MyMethods.Heal, OnHeal);
    }

    private void OnTakeDamage(MmMessage message)
    {
        var msg = (MmMessageInt)message;
        health -= msg.value;
        Debug.Log($"[{gameObject.name}] Took {msg.value} damage. Health: {health}");

        if (health <= 0)
        {
            Debug.Log($"[{gameObject.name}] Died!");
            this.Send("EnemyDied").ToParents().Execute();
            Destroy(gameObject);
        }
    }

    private void OnChangeColor(MmMessage message)
    {
        var msg = (MmMessageVector3)message;
        if (meshRenderer != null)
        {
            meshRenderer.material.color = new Color(msg.value.x, msg.value.y, msg.value.z);
            Debug.Log($"[{gameObject.name}] Color changed to ({msg.value.x}, {msg.value.y}, {msg.value.z})");
        }
    }

    private void OnEnableGravity(MmMessage message)
    {
        var msg = (MmMessageBool)message;
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = msg.value;
            Debug.Log($"[{gameObject.name}] Gravity {(msg.value ? "enabled" : "disabled")}");
        }
    }

    private void OnHeal(MmMessage message)
    {
        var msg = (MmMessageInt)message;
        health += msg.value;
        Debug.Log($"[{gameObject.name}] Healed {msg.value}. Health: {health}");
    }

    // Standard methods still work normally
    public override void Initialize()
    {
        base.Initialize();
        Debug.Log($"[{gameObject.name}] Initialized with {health} health");
    }
}
```

> **Tutorial Scene:** The actual script is `T3_EnemyResponderExtendable.cs` (with `T3_` prefix).

### Dynamic Handler Registration

You can add or remove handlers at runtime:

```csharp
public class DynamicResponder : MmExtendableResponder
{
    private bool isInvulnerable = false;

    public override void Awake()
    {
        base.Awake();
        RegisterCustomHandler((MmMethod)MyMethods.TakeDamage, OnTakeDamage);
    }

    private void OnTakeDamage(MmMessage message)
    {
        if (!isInvulnerable)
        {
            var damage = ((MmMessageInt)message).value;
            Debug.Log($"Took {damage} damage");
        }
    }

    public void EnableInvulnerability()
    {
        isInvulnerable = true;
        // Or completely remove the handler
        UnregisterCustomHandler((MmMethod)MyMethods.TakeDamage);
    }

    public void DisableInvulnerability()
    {
        isInvulnerable = false;
        // Re-register the handler
        RegisterCustomHandler((MmMethod)MyMethods.TakeDamage, OnTakeDamage);
    }
}
```

---

## Comparison: Traditional vs Extendable

| Aspect | MmBaseResponder | MmExtendableResponder |
|--------|-----------------|----------------------|
| Custom methods | Override `MmInvoke()` + switch | `RegisterCustomHandler()` |
| Code required | ~40 lines | ~25 lines |
| Risk of forgetting base call | Yes | No (automatic) |
| Runtime handler changes | Manual flag logic | `Register`/`Unregister` |
| Performance | Same | Same |

### Code Comparison

**Traditional (MmBaseResponder):**
```csharp
public override void MmInvoke(MmMessage message)
{
    base.MmInvoke(message);  // Don't forget this!

    switch ((int)message.MmMethod)
    {
        case MyMethods.TakeDamage:
            HandleDamage((MmMessageInt)message);
            break;
        case MyMethods.ChangeColor:
            HandleColor((MmMessageVector3)message);
            break;
        // More cases...
    }
}
```

**Modern (MmExtendableResponder):**
```csharp
public override void Awake()
{
    base.Awake();
    RegisterCustomHandler((MmMethod)MyMethods.TakeDamage, OnDamage);
    RegisterCustomHandler((MmMethod)MyMethods.ChangeColor, OnColor);
}
```

---

## Sending Custom Methods with DSL

The DSL works with custom methods too:

```csharp
// Traditional
relay.MmInvoke((MmMethod)MyMethods.TakeDamage, 25, metadata);

// DSL - send custom method with value
relay.Send((MmMethod)MyMethods.TakeDamage, 25).ToChildren().Execute();

// DSL - custom method to descendants
relay.Send((MmMethod)MyMethods.ChangeColor, new Vector3(1, 0, 0))
    .ToDescendants()
    .Active()
    .Execute();
```

---

## Complete Example: Damage System

### Hierarchy

```
GameWorld (MmRelayNode + GameController)
  ├── Player (MmRelayNode + PlayerResponder)
  └── Enemies (MmRelayNode)
        ├── Enemy1 (MmRelayNode + EnemyResponderExtendable)
        ├── Enemy2 (MmRelayNode + EnemyResponderExtendable)
        └── Enemy3 (MmRelayNode + EnemyResponderExtendable)
```

### GameController.cs

```csharp
using UnityEngine;
using MercuryMessaging;

public class GameController : MonoBehaviour
{
    private MmRelayNode relay;
    private bool gravityEnabled = true;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
        relay.BroadcastInitialize();
    }

    void Update()
    {
        // Press D to damage all enemies
        if (Input.GetKeyDown(KeyCode.D))
            DamageAllEnemies(10);

        // Press C to change enemy colors to red
        if (Input.GetKeyDown(KeyCode.C))
            ChangeEnemyColors(Color.red);

        // Press G to toggle gravity
        if (Input.GetKeyDown(KeyCode.G))
        {
            gravityEnabled = !gravityEnabled;
            ToggleGravity(gravityEnabled);
        }

        // Press H to heal all enemies
        if (Input.GetKeyDown(KeyCode.H))
            HealAllEnemies(25);

        // Press I to re-initialize
        if (Input.GetKeyDown(KeyCode.I))
            relay.BroadcastInitialize();
    }

    public void DamageAllEnemies(int damage)
    {
        relay.Send((MmMethod)MyMethods.TakeDamage, damage)
            .ToDescendants()
            .Execute();
    }

    public void ChangeEnemyColors(Color color)
    {
        relay.Send((MmMethod)MyMethods.ChangeColor,
            new Vector3(color.r, color.g, color.b))
            .ToDescendants()
            .Execute();
    }

    public void ToggleGravity(bool enabled)
    {
        relay.Send((MmMethod)MyMethods.EnableGravity, enabled)
            .ToDescendants()
            .Execute();
    }

    public void HealAllEnemies(int amount)
    {
        relay.Send((MmMethod)MyMethods.Heal, amount)
            .ToDescendants()
            .Execute();
    }
}
```

> **Tutorial Scene:** The actual script is `T3_GameController.cs` (with `T3_` prefix).

---

## Tutorial Scene Keyboard Controls

| Key | Action | Description |
|-----|--------|-------------|
| **D** | DamageAllEnemies(10) | Deal 10 damage to all enemies |
| **C** | ChangeEnemyColors(Color.red) | Change enemy colors to red |
| **G** | ToggleGravity | Toggle gravity on/off for enemies |
| **H** | HealAllEnemies(25) | Heal all enemies for 25 HP |
| **I** | BroadcastInitialize | Re-initialize all enemies |

**Expected Console Output (press D):**
```
[GameController] Dealt 10 damage to all enemies
[Enemy1] Took 10 damage. Health: 90
[Enemy2] Took 10 damage. Health: 90
[Enemy3] Took 10 damage. Health: 90
```

---

## Common Mistakes

| Mistake | Solution |
|---------|----------|
| Forgetting `base.MmInvoke()` | Use `MmExtendableResponder` (handles automatically) |
| Using method ID < 1000 | Always use 1000+ for custom methods |
| Wrong message type cast | Match the message type to your payload (int -> MmMessageInt) |
| Handler not called | Ensure `RegisterCustomHandler()` is called in Awake() |

---

## Best Practices

1. **Use MmExtendableResponder** for custom methods (cleaner, safer)
2. **Define method IDs in one place** (constants class)
3. **Start custom IDs at 1000** to avoid conflicts
4. **Document your custom methods** for team members
5. **Use descriptive handler names** (OnTakeDamage, not Handle1)

---

## Try This

Test your understanding of custom responders:

1. **Add a HealDamage method** - Create `MyMethods.HealDamage = 1005` and implement a handler that increases health (capped at max).

2. **Use MmExtendableResponder** - Convert the traditional EnemyResponder to use MmExtendableResponder. Notice how much cleaner the code becomes.

3. **Dynamic handler switching** - Create a shield pickup that temporarily unregisters the TakeDamage handler for 5 seconds (invulnerability).

4. **Chain reactions** - When an enemy dies, have it send a message to nearby enemies to make them alerted (`isAlerted = true`).

---

## Next Steps

- **[Tutorial 4: Custom Messages](Tutorial-4-Custom-Messages)** - Create custom message types with payloads
- **[Tutorial 5: Fluent DSL API](Tutorial-5-Fluent-DSL-API)** - Complete DSL reference
- **[Tutorial 8: Switch Nodes & FSM](Tutorial-8-Switch-Nodes-FSM)** - State machine integration

---

*Tutorial 3 of 14 - MercuryMessaging Wiki*
