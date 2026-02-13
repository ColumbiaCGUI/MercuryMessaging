# Tutorial 4: Creating Custom Messages

## Overview

While Tutorial 3 covered custom **methods** (custom actions), this tutorial covers custom **message types**--messages that carry custom data payloads beyond the built-in types (string, int, float, Vector3, etc.).

## What You'll Learn

- When to create custom message types vs using built-in types
- Extending `MmMessage` for custom payloads
- Implementing required methods: `Copy()` and `Serialize()`/`Deserialize()`
- Best practices for message design

## Prerequisites

- Completed [Tutorial 3: Custom Responders](Tutorial-3-Custom-Responders)
- Understanding of custom methods

---

## When to Create Custom Messages

**Use built-in message types when:**
- Your data fits a single value (string, int, float, bool, Vector3, etc.)
- You're prototyping quickly

**Create custom messages when:**
- You need multiple fields (e.g., Color + Intensity)
- You have complex data structures
- You want type safety for specific message kinds
- You need network serialization for custom data

---

## Understanding MmMessage

All messages in MercuryMessaging inherit from `MmMessage`:

```csharp
public class MmMessage
{
    public MmMethod MmMethod;             // What action to perform
    public MmMessageType MmMessageType;   // Type identifier for serialization
    public MmMetadataBlock MetadataBlock;  // Routing control

    // Required override for routing
    public virtual MmMessage Copy() { ... }

    // Required overrides for networking (object[] pattern)
    public virtual object[] Serialize() { ... }
    public virtual int Deserialize(object[] data) { ... }
}
```

> **Note:** The primary serialization API uses `object[]` arrays. For binary serialization, `MmWriter`/`MmReader` classes are also available (see [Tutorial 11: Advanced Networking](Tutorial-11-Advanced-Networking)).

---

## Step-by-Step: Creating a Custom Message

### Example: ColorIntensityMessage

Let's create a message that carries both a Color and an intensity value.

### Step 1: Define Message Type ID

Custom message types use IDs **1100 and above**:

```csharp
// MyMessageTypes.cs
public static class MyMessageTypes
{
    // Start at 1100 to avoid conflicts
    public const int ColorIntensity = 1100;
    public const int EnemyState = 1101;
    public const int PlayerStats = 1102;
}
```

> **Tutorial Scene:** The actual script is `T4_MyMessageTypes.cs` (with `T4_` prefix per tutorial naming convention).

### Step 2: Create the Message Class

```csharp
using UnityEngine;
using MercuryMessaging;

public class ColorIntensityMessage : MmMessage
{
    // Custom payload fields
    public Color color;
    public float intensity;

    // Default constructor (required)
    public ColorIntensityMessage() : base()
    {
        MmMethod = (MmMethod)MyMethods.ChangeColor;
        MmMessageType = (MmMessageType)MyMessageTypes.ColorIntensity;
    }

    // Convenience constructor
    public ColorIntensityMessage(Color color, float intensity) : this()
    {
        this.color = color;
        this.intensity = intensity;
    }

    // REQUIRED: Copy method for message routing
    public override MmMessage Copy()
    {
        var copy = new ColorIntensityMessage
        {
            // Copy base fields
            MmMethod = this.MmMethod,
            MmMessageType = this.MmMessageType,
            MetadataBlock = this.MetadataBlock,
            NetId = this.NetId,
            TimeStamp = this.TimeStamp,
            HopCount = this.HopCount,

            // Copy custom fields
            color = this.color,
            intensity = this.intensity
        };
        return copy;
    }

    // REQUIRED for networking: Serialize to object array
    public override object[] Serialize()
    {
        // Get base serialized data (method, type, netId, metadata)
        object[] baseSerialized = base.Serialize();

        // Allocate result: base + 5 (color r,g,b,a + intensity)
        object[] result = new object[baseSerialized.Length + 5];

        // Copy base data
        System.Array.Copy(baseSerialized, 0, result, 0, baseSerialized.Length);

        // Write custom fields
        int idx = baseSerialized.Length;
        result[idx++] = color.r;
        result[idx++] = color.g;
        result[idx++] = color.b;
        result[idx++] = color.a;
        result[idx++] = intensity;

        return result;
    }

    // REQUIRED for networking: Deserialize from object array
    public override int Deserialize(object[] data)
    {
        // Deserialize base fields first
        int index = base.Deserialize(data);

        // Read custom fields (same order as Serialize!)
        float r = System.Convert.ToSingle(data[index++]);
        float g = System.Convert.ToSingle(data[index++]);
        float b = System.Convert.ToSingle(data[index++]);
        float a = System.Convert.ToSingle(data[index++]);
        color = new Color(r, g, b, a);
        intensity = System.Convert.ToSingle(data[index++]);

        return index;
    }
}
```

> **Tutorial Scene:** The actual script is `T4_ColorIntensityMessage.cs` (with `T4_` prefix).

### Step 3: Handle the Message in a Responder

Using **MmExtendableResponder** (recommended):

```csharp
using UnityEngine;
using MercuryMessaging;

public class LightResponder : MmExtendableResponder
{
    [SerializeField] private Light targetLight;
    [SerializeField] private Renderer targetRenderer;

    public override void Awake()
    {
        base.Awake();
        RegisterCustomHandler((MmMethod)MyMethods.ChangeColor, OnColorIntensity);
    }

    private void OnColorIntensity(MmMessage message)
    {
        var msg = (ColorIntensityMessage)message;

        if (targetLight != null)
        {
            targetLight.color = msg.color;
            targetLight.intensity = msg.intensity;
        }

        if (targetRenderer != null)
        {
            targetRenderer.material.color = msg.color * msg.intensity;
        }

        Debug.Log($"[{gameObject.name}] Color={msg.color}, Intensity={msg.intensity}");
    }

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log($"[{gameObject.name}] Initialized with default color/intensity");
    }
}
```

> **Tutorial Scene:** The actual script is `T4_LightResponder.cs` (with `T4_` prefix).

**Expected Output (when pressing R for red):**
```
[LightController] Set color to RGBA(1.000, 0.000, 0.000, 1.000) with intensity 2
[Light1] Color=RGBA(1.000, 0.000, 0.000, 1.000), Intensity=2
```

### Step 4: Send the Custom Message

```csharp
using UnityEngine;
using MercuryMessaging;

public class LightController : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
        relay.BroadcastInitialize();
    }

    void Update()
    {
        // Press R for red, G for green, B for blue
        if (Input.GetKeyDown(KeyCode.R))
            SetLightColor(Color.red, 2.0f);
        if (Input.GetKeyDown(KeyCode.G))
            SetLightColor(Color.green, 1.5f);
        if (Input.GetKeyDown(KeyCode.B))
            SetLightColor(Color.blue, 1.0f);
        if (Input.GetKeyDown(KeyCode.W))
            SetLightColor(Color.white, 3.0f);
        if (Input.GetKeyDown(KeyCode.Alpha0))
            SetLightColor(Color.black, 0f);
        if (Input.GetKeyDown(KeyCode.I))
            relay.BroadcastInitialize();
    }

    public void SetLightColor(Color color, float intensity)
    {
        var message = new ColorIntensityMessage(color, intensity)
        {
            MetadataBlock = new MmMetadataBlock(MmLevelFilter.Child)
        };
        relay.MmInvoke(message);
    }
}
```

> **Tutorial Scene:** The actual script is `T4_LightController.cs` (with `T4_` prefix).

---

## Tutorial Scene Keyboard Controls

| Key | Action | Description |
|-----|--------|-------------|
| **R** | SetLightColor(red, 2.0) | Set lights to red, intensity 2.0 |
| **G** | SetLightColor(green, 1.5) | Set lights to green, intensity 1.5 |
| **B** | SetLightColor(blue, 1.0) | Set lights to blue, intensity 1.0 |
| **W** | SetLightColor(white, 3.0) | Set lights to white, intensity 3.0 |
| **0** | SetLightColor(black, 0) | Turn off all lights |
| **I** | BroadcastInitialize | Reset lights to defaults |

---

## More Examples

### Example 2: EnemyStateMessage (Binary Serialization)

For binary serialization (useful for network-heavy scenarios), you can use `MmWriter`/`MmReader` as an alternative to `object[]`:

```csharp
using UnityEngine;
using MercuryMessaging;

public class EnemyStateMessage : MmMessage
{
    public int health;
    public bool isAlerted;
    public Vector3 lastKnownPlayerPosition;
    public string currentBehavior;

    public EnemyStateMessage() : base()
    {
        MmMethod = (MmMethod)1005; // Custom method ID
        MmMessageType = (MmMessageType)MyMessageTypes.EnemyState;
    }

    public override MmMessage Copy()
    {
        return new EnemyStateMessage
        {
            MmMethod = this.MmMethod,
            MmMessageType = this.MmMessageType,
            MetadataBlock = this.MetadataBlock,
            NetId = this.NetId,
            TimeStamp = this.TimeStamp,
            HopCount = this.HopCount,
            health = this.health,
            isAlerted = this.isAlerted,
            lastKnownPlayerPosition = this.lastKnownPlayerPosition,
            currentBehavior = this.currentBehavior
        };
    }

    // Using object[] pattern (primary approach, matches ColorIntensityMessage)
    public override object[] Serialize()
    {
        object[] baseSerialized = base.Serialize();
        object[] result = new object[baseSerialized.Length + 6];
        System.Array.Copy(baseSerialized, 0, result, 0, baseSerialized.Length);

        int idx = baseSerialized.Length;
        result[idx++] = health;
        result[idx++] = isAlerted;
        result[idx++] = lastKnownPlayerPosition.x;
        result[idx++] = lastKnownPlayerPosition.y;
        result[idx++] = lastKnownPlayerPosition.z;
        result[idx++] = currentBehavior;

        return result;
    }

    public override int Deserialize(object[] data)
    {
        int index = base.Deserialize(data);
        health = System.Convert.ToInt32(data[index++]);
        isAlerted = System.Convert.ToBoolean(data[index++]);
        float x = System.Convert.ToSingle(data[index++]);
        float y = System.Convert.ToSingle(data[index++]);
        float z = System.Convert.ToSingle(data[index++]);
        lastKnownPlayerPosition = new Vector3(x, y, z);
        currentBehavior = (string)data[index++];
        return index;
    }
}
```

> **Alternative:** For binary-optimized serialization, see `MmWriter`/`MmReader` in [Tutorial 11: Advanced Networking](Tutorial-11-Advanced-Networking).

### Example 3: PlayerStatsMessage

```csharp
using MercuryMessaging;

public class PlayerStatsMessage : MmMessage
{
    public int score;
    public int lives;
    public float playTime;
    public int level;

    public PlayerStatsMessage() : base()
    {
        MmMethod = (MmMethod)1006; // Custom method ID
        MmMessageType = (MmMessageType)MyMessageTypes.PlayerStats;
    }

    public override MmMessage Copy()
    {
        return new PlayerStatsMessage
        {
            MmMethod = this.MmMethod,
            MmMessageType = this.MmMessageType,
            MetadataBlock = this.MetadataBlock,
            NetId = this.NetId,
            TimeStamp = this.TimeStamp,
            HopCount = this.HopCount,
            score = this.score,
            lives = this.lives,
            playTime = this.playTime,
            level = this.level
        };
    }

    public override object[] Serialize()
    {
        object[] baseSerialized = base.Serialize();
        object[] result = new object[baseSerialized.Length + 4];
        System.Array.Copy(baseSerialized, 0, result, 0, baseSerialized.Length);

        int idx = baseSerialized.Length;
        result[idx++] = score;
        result[idx++] = lives;
        result[idx++] = playTime;
        result[idx++] = level;

        return result;
    }

    public override int Deserialize(object[] data)
    {
        int index = base.Deserialize(data);
        score = System.Convert.ToInt32(data[index++]);
        lives = System.Convert.ToInt32(data[index++]);
        playTime = System.Convert.ToSingle(data[index++]);
        level = System.Convert.ToInt32(data[index++]);
        return index;
    }
}
```

---

## Tutorial 3 vs Tutorial 4 Summary

| Aspect | Tutorial 3 (Custom Responders) | Tutorial 4 (Custom Messages) |
|--------|-------------------------------|------------------------------|
| **Focus** | How to HANDLE messages | How to CREATE message TYPES |
| **Creates** | Custom responder classes | Custom message classes |
| **Extends** | `MmBaseResponder` / `MmExtendableResponder` | `MmMessage` |
| **Custom ID** | `MmMethod` (1000+) | `MmMessageType` (1100+) |
| **Key Override** | `MmInvoke()` or `RegisterCustomHandler()` | `Copy()`, `Serialize()`, `Deserialize()` |
| **Use Case** | Custom actions | Custom data payloads |

---

## Serialization Approaches

### Primary: object[] Pattern

Used by the tutorial code and most common scenarios:

```csharp
public override object[] Serialize()
{
    object[] baseSerialized = base.Serialize();
    object[] result = new object[baseSerialized.Length + N];
    System.Array.Copy(baseSerialized, 0, result, 0, baseSerialized.Length);
    // Write custom fields...
    return result;
}

public override int Deserialize(object[] data)
{
    int index = base.Deserialize(data);
    // Read custom fields...
    return index;
}
```

### Alternative: MmWriter/MmReader (Binary)

For binary-optimized serialization over networks:

```csharp
// Available Write Methods
writer.WriteBool(bool value);
writer.WriteInt(int value);
writer.WriteFloat(float value);
writer.WriteDouble(double value);
writer.WriteString(string value);
writer.WriteVector2(Vector2 value);
writer.WriteVector3(Vector3 value);
writer.WriteVector4(Vector4 value);
writer.WriteQuaternion(Quaternion value);
writer.WriteColor(Color value);
writer.WriteBytes(byte[] value);

// Available Read Methods
bool value = reader.ReadBool();
int value = reader.ReadInt();
float value = reader.ReadFloat();
// ... etc.
```

See [Tutorial 11: Advanced Networking](Tutorial-11-Advanced-Networking) for binary serialization details.

---

## Common Mistakes

| Mistake | Solution |
|---------|----------|
| Forgot `Copy()` | Message won't route correctly--always implement `Copy()` |
| Serialize/Deserialize order mismatch | Read fields in the same order you write them |
| Using ID < 1100 | Custom message types should use 1100+ |
| Not calling `base.Serialize()` | Base fields won't serialize--always call base methods |
| Missing default constructor | Deserialization needs a parameterless constructor |

---

## Best Practices

1. **Always implement `Copy()`** - Required for message routing
2. **Match Serialize/Deserialize order** - Same sequence of writes and reads
3. **Call base methods** - Don't forget `base.Serialize()` and `base.Deserialize()`
4. **Use meaningful type IDs** - Document them in a constants class
5. **Keep messages small** - Only include necessary data
6. **Test serialization** - Verify network roundtrip works

---

## Try This

Practice creating custom messages:

1. **Create a DamageMessage** - Create a custom message with `int damage`, `string damageType` (fire, ice, physical), and `Vector3 hitPosition`. Implement Copy() and serialization.

2. **Test network serialization** - Use the MmLoopbackBackend to verify your custom message serializes and deserializes correctly (compare values before/after).

3. **Add validation** - In your custom message constructor, add validation (e.g., intensity must be between 0-10). Log a warning if invalid values are passed.

4. **Create a InventoryItemMessage** - Design a message for inventory systems with `string itemId`, `int quantity`, `bool isStackable`. Handle it in an InventoryResponder.

---

## Next Steps

- **[Tutorial 5: Fluent DSL API](Tutorial-5-Fluent-DSL-API)** - Complete DSL reference
- **[Tutorial 6: FishNet Networking](Tutorial-6-FishNet-Networking)** - Use custom messages over the network
- **[Tutorial 11: Advanced Networking](Tutorial-11-Advanced-Networking)** - Network architecture deep dive

---

*Tutorial 4 of 14 - MercuryMessaging Wiki*
