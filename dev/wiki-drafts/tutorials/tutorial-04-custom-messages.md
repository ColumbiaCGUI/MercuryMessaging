# Tutorial 4: Creating Custom Messages

## Overview

While Tutorial 3 covered custom **methods** (custom actions), this tutorial covers custom **message types**—messages that carry custom data payloads beyond the built-in types (string, int, float, Vector3, etc.).

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
    public MmMethod MmMethod;           // What action to perform
    public MmMessageType MessageType;   // Type identifier for serialization
    public MmMetadataBlock MetadataBlock; // Routing control

    // Required override for routing
    public virtual MmMessage Copy() { ... }

    // Required overrides for networking
    public virtual void Serialize(ref MmWriter writer) { ... }
    public virtual void Deserialize(ref MmReader reader) { ... }
}
```

---

## Step-by-Step: Creating a Custom Message

### Example: ColorIntensityMessage

Let's create a message that carries both a Color and an intensity value.

### Step 1: Define Message Type ID

Custom message types use IDs **1100 and above**:

```csharp
// MyMessageTypes.cs
namespace MyGame
{
    public static class MyMessageTypes
    {
        // Start at 1100 to avoid conflicts
        public const int ColorIntensity = 1100;
        public const int EnemyState = 1101;
        public const int PlayerStats = 1102;
    }
}
```

### Step 2: Create the Message Class

```csharp
using UnityEngine;
using MercuryMessaging;

namespace MyGame
{
    public class ColorIntensityMessage : MmMessage
    {
        // Custom payload fields
        public Color color;
        public float intensity;

        // Default constructor (required)
        public ColorIntensityMessage() : base()
        {
            MmMethod = (MmMethod)MyMethods.ChangeColor;
            MessageType = (MmMessageType)MyMessageTypes.ColorIntensity;
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
                MessageType = this.MessageType,
                MetadataBlock = this.MetadataBlock,

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
            result[baseSerialized.Length + 0] = color.r;
            result[baseSerialized.Length + 1] = color.g;
            result[baseSerialized.Length + 2] = color.b;
            result[baseSerialized.Length + 3] = color.a;
            result[baseSerialized.Length + 4] = intensity;

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
}
```

### Step 3: Handle the Message in a Responder

Using **MmExtendableResponder** (recommended):

```csharp
using UnityEngine;
using MercuryMessaging;

public class LightResponder : MmExtendableResponder
{
    [SerializeField] private Light targetLight;
    [SerializeField] private Renderer targetRenderer;

    protected override void Awake()
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

        Debug.Log($"{name}: Color={msg.color}, Intensity={msg.intensity}");
    }

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log($"{name} light responder initialized");
    }
}
```

**Expected Output (when pressing R for red):**
```
SpotLight: Color=RGBA(1.000, 0.000, 0.000, 1.000), Intensity=2
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
    }

    public void SetLightColor(Color color, float intensity)
    {
        var message = new ColorIntensityMessage(color, intensity);
        relay.MmInvoke(message);
    }

    // With DSL and metadata
    public void SetLightColorDSL(Color color, float intensity)
    {
        var message = new ColorIntensityMessage(color, intensity)
        {
            MetadataBlock = new MmMetadataBlock(MmLevelFilter.Child)
        };
        relay.MmInvoke(message);
    }
}
```

---

## More Examples

### Example 2: EnemyStateMessage

A message with multiple fields for enemy state:

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
        MmMethod = (MmMethod)MyMethods.UpdateEnemyState;
        MessageType = (MmMessageType)MyMessageTypes.EnemyState;
    }

    public override MmMessage Copy()
    {
        return new EnemyStateMessage
        {
            MmMethod = this.MmMethod,
            MessageType = this.MessageType,
            MetadataBlock = this.MetadataBlock,
            health = this.health,
            isAlerted = this.isAlerted,
            lastKnownPlayerPosition = this.lastKnownPlayerPosition,
            currentBehavior = this.currentBehavior
        };
    }

    public override void Serialize(ref MmWriter writer)
    {
        base.Serialize(ref writer);
        writer.WriteInt(health);
        writer.WriteBool(isAlerted);
        writer.WriteVector3(lastKnownPlayerPosition);
        writer.WriteString(currentBehavior);
    }

    public override void Deserialize(ref MmReader reader)
    {
        base.Deserialize(ref reader);
        health = reader.ReadInt();
        isAlerted = reader.ReadBool();
        lastKnownPlayerPosition = reader.ReadVector3();
        currentBehavior = reader.ReadString();
    }
}
```

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
        MmMethod = (MmMethod)MyMethods.UpdateStats;
        MessageType = (MmMessageType)MyMessageTypes.PlayerStats;
    }

    public override MmMessage Copy()
    {
        return new PlayerStatsMessage
        {
            MmMethod = this.MmMethod,
            MessageType = this.MessageType,
            MetadataBlock = this.MetadataBlock,
            score = this.score,
            lives = this.lives,
            playTime = this.playTime,
            level = this.level
        };
    }

    public override void Serialize(ref MmWriter writer)
    {
        base.Serialize(ref writer);
        writer.WriteInt(score);
        writer.WriteInt(lives);
        writer.WriteFloat(playTime);
        writer.WriteInt(level);
    }

    public override void Deserialize(ref MmReader reader)
    {
        base.Deserialize(ref reader);
        score = reader.ReadInt();
        lives = reader.ReadInt();
        playTime = reader.ReadFloat();
        level = reader.ReadInt();
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

## Serialization Reference

### Available Write Methods

```csharp
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
```

### Available Read Methods

```csharp
bool value = reader.ReadBool();
int value = reader.ReadInt();
float value = reader.ReadFloat();
double value = reader.ReadDouble();
string value = reader.ReadString();
Vector2 value = reader.ReadVector2();
Vector3 value = reader.ReadVector3();
Vector4 value = reader.ReadVector4();
Quaternion value = reader.ReadQuaternion();
Color value = reader.ReadColor();
byte[] value = reader.ReadBytes();
```

---

## Common Mistakes

| Mistake | Solution |
|---------|----------|
| Forgot `Copy()` | Message won't route correctly—always implement `Copy()` |
| Serialize/Deserialize order mismatch | Read fields in the same order you write them |
| Using ID < 1100 | Custom message types should use 1100+ |
| Not calling `base.Serialize()` | Base fields won't serialize—always call base methods |
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

## Complete Example: UI Stats Display

### Hierarchy

```
GameManager (MmRelayNode + StatsController)
  └── UI (MmRelayNode)
        ├── ScoreDisplay (MmRelayNode + StatsDisplayResponder)
        ├── LivesDisplay (MmRelayNode + StatsDisplayResponder)
        └── LevelDisplay (MmRelayNode + StatsDisplayResponder)
```

### StatsController.cs

```csharp
using UnityEngine;
using MercuryMessaging;

public class StatsController : MonoBehaviour
{
    private MmRelayNode relay;
    private int score = 0;
    private int lives = 3;
    private int level = 1;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
        BroadcastStats();
    }

    public void AddScore(int points)
    {
        score += points;
        BroadcastStats();
    }

    public void LoseLife()
    {
        lives--;
        BroadcastStats();
    }

    public void NextLevel()
    {
        level++;
        BroadcastStats();
    }

    private void BroadcastStats()
    {
        var msg = new PlayerStatsMessage
        {
            score = this.score,
            lives = this.lives,
            level = this.level,
            playTime = Time.time
        };
        relay.MmInvoke(msg);
    }
}
```

### StatsDisplayResponder.cs

```csharp
using UnityEngine;
using UnityEngine.UI;
using MercuryMessaging;

public class StatsDisplayResponder : MmExtendableResponder
{
    [SerializeField] private Text displayText;
    [SerializeField] private StatType statToDisplay;

    public enum StatType { Score, Lives, Level, PlayTime }

    protected override void Awake()
    {
        base.Awake();
        RegisterCustomHandler((MmMethod)MyMethods.UpdateStats, OnStatsUpdate);
    }

    private void OnStatsUpdate(MmMessage message)
    {
        var stats = (PlayerStatsMessage)message;

        string text = statToDisplay switch
        {
            StatType.Score => $"Score: {stats.score}",
            StatType.Lives => $"Lives: {stats.lives}",
            StatType.Level => $"Level: {stats.level}",
            StatType.PlayTime => $"Time: {stats.playTime:F1}s",
            _ => ""
        };

        if (displayText != null)
            displayText.text = text;
    }
}
```

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
