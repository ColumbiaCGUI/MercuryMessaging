# Digital Twin Layer - Technical Context

This document provides technical context for implementing Digital Twin communication using MercuryMessaging.

---

## Core Concept: Hierarchical vs Flat IoT Communication

### Traditional MQTT (Flat)
```
Topic: building/floor1/room101/temperature
Payload: {"value": 22.5, "unit": "C"}

// Subscription patterns
building/+/+/temperature    // All temp sensors
building/floor1/#           // Everything on floor1
```
- **Strengths**: Simple, lightweight, well-supported
- **Limitations**: No hierarchy awareness, no filtering beyond topics, no state management

### Mercury DT Layer (Hierarchical)
```csharp
// Hierarchy mirrors physical structure
Building (MmRelaySwitchNode)
└── Floor1 (MmRelayNode, Tag.Floor)
    └── Room101 (MmRelayNode, Tag.Room)
        └── TempSensor (MmBaseResponder, Tag.Temperature)

// Targeted message routing
floor1.MmInvoke(
    new TemperatureQuery(),
    new MmMetadataBlock(
        MmLevelFilter.SelfAndChildren,  // All descendants
        MmActiveFilter.Active,           // Only powered sensors
        tag: MmTag.Tag0                  // Temperature sensors only
    )
);
```
- **Strengths**: Hierarchy awareness, multi-level filtering, FSM state management
- **Additions**: Spatial indexing, built-in state machines, developer tools

---

## MQTT/Mercury Bridge Architecture

### Bridge Components

```
MQTT Broker                    Mercury Bridge                Unity Scene
───────────                    ──────────────                ───────────
                              ┌──────────────┐
Sensors ──────►Topic──────────►│ Topic Parser │
                              └──────┬───────┘
                                     │
                              ┌──────▼───────┐
                              │ Path Mapper  │──────────►Find Relay Node
                              └──────┬───────┘
                                     │
                              ┌──────▼───────┐
                              │ Msg Converter│──────────►MmMessage
                              └──────┬───────┘
                                     │
                              ┌──────▼───────┐
                              │ Router       │──────────►MmInvoke()
                              └──────────────┘
```

### Topic-to-Hierarchy Mapping

```csharp
public class TopicMapper
{
    // Map MQTT topic to Unity hierarchy path
    public string TopicToHierarchy(string topic)
    {
        // building/floor1/room101/sensor
        // → Building/Floor1/Room101/Sensor
        var parts = topic.Split('/');
        return string.Join("/", parts.Select(p => ToPascalCase(p)));
    }

    // Find corresponding relay node
    public MmRelayNode FindRelay(string hierarchyPath)
    {
        var transform = GameObject.Find(hierarchyPath)?.transform;
        return transform?.GetComponent<MmRelayNode>();
    }
}
```

### Message Conversion

```csharp
public class MessageConverter
{
    public MmMessage ConvertFromMqtt(string topic, byte[] payload)
    {
        var sensorType = ExtractSensorType(topic);

        return sensorType switch
        {
            "temperature" => new MmMessageFloat { value = ParseFloat(payload) },
            "motion" => new MmMessageBool { value = ParseBool(payload) },
            "humidity" => new MmMessageFloat { value = ParseFloat(payload) },
            _ => new MmMessageByteArray { value = payload }
        };
    }

    public (string topic, byte[] payload) ConvertToMqtt(MmMessage message)
    {
        // Reverse conversion for outbound messages
        var topic = BuildTopic(message.MetadataBlock);
        var payload = SerializePayload(message);
        return (topic, payload);
    }
}
```

---

## DT Entity Responder Pattern

### Base DT Responder

```csharp
public abstract class MmDigitalTwinResponder : MmBaseResponder
{
    [Header("Digital Twin Settings")]
    public string PhysicalId;        // Physical sensor/device ID
    public float SyncInterval = 1.0f; // Seconds between syncs
    public bool IsOnline = true;      // Connection status

    protected virtual void OnPhysicalUpdate(MmMessage message)
    {
        // Override to handle physical sensor updates
    }

    protected virtual MmMessage GetVirtualState()
    {
        // Override to return current virtual state
        return null;
    }

    public void SyncToPhysical()
    {
        var state = GetVirtualState();
        if (state != null)
        {
            // Send to physical device via bridge
            DigitalTwinBridge.SendToPhysical(PhysicalId, state);
        }
    }
}
```

### Concrete DT Responders

```csharp
public class TemperatureSensorTwin : MmDigitalTwinResponder
{
    public float CurrentTemperature;
    public float TargetTemperature;

    protected override void OnPhysicalUpdate(MmMessage message)
    {
        if (message is MmMessageFloat tempMsg)
        {
            CurrentTemperature = tempMsg.value;
            OnTemperatureChanged?.Invoke(CurrentTemperature);
        }
    }

    protected override void ReceivedMessage(MmMessageFloat message)
    {
        // Handle commands from Mercury
        TargetTemperature = message.value;
        SyncToPhysical();
    }
}

public class MotionSensorTwin : MmDigitalTwinResponder
{
    public bool MotionDetected;
    public DateTime LastMotionTime;

    protected override void OnPhysicalUpdate(MmMessage message)
    {
        if (message is MmMessageBool motionMsg)
        {
            MotionDetected = motionMsg.value;
            if (MotionDetected)
            {
                LastMotionTime = DateTime.Now;
                // Broadcast to siblings (other sensors in room)
                GetComponent<MmRelayNode>()?.MmInvoke(
                    new MmMessageString { value = "motion_detected" },
                    new MmMetadataBlock(MmLevelFilter.Parent)
                );
            }
        }
    }
}
```

---

## FSM for Building Modes

### Mode Configuration

```csharp
public class BuildingModeController : MonoBehaviour
{
    private MmRelaySwitchNode _switchNode;

    void Start()
    {
        _switchNode = GetComponent<MmRelaySwitchNode>();

        // Configure modes
        // Children: DayMode, NightMode, EmergencyMode
    }

    public void SetDayMode()
    {
        _switchNode.RespondersFSM.JumpTo("DayMode");
        // Only DayMode responders receive messages
        // - Full lighting
        // - Normal HVAC
        // - All sensors active
    }

    public void SetNightMode()
    {
        _switchNode.RespondersFSM.JumpTo("NightMode");
        // Only NightMode responders receive messages
        // - Reduced lighting
        // - Energy-saving HVAC
        // - Security sensors prioritized
    }

    public void SetEmergencyMode()
    {
        _switchNode.RespondersFSM.JumpTo("EmergencyMode");
        // Only EmergencyMode responders receive messages
        // - Emergency lighting
        // - HVAC override
        // - All safety sensors active
    }
}
```

### Hierarchy with Modes

```
Building (MmRelaySwitchNode)
├── DayMode (MmRelayNode)
│   ├── Floor1 (MmRelayNode)
│   │   └── ... (day-specific responders)
├── NightMode (MmRelayNode)
│   ├── Floor1 (MmRelayNode)
│   │   └── ... (night-specific responders)
└── EmergencyMode (MmRelayNode)
    ├── Floor1 (MmRelayNode)
    │   └── ... (emergency-specific responders)
```

---

## Tag System for Sensor Types

### Tag Allocation

```csharp
public static class DTTags
{
    public const MmTag Temperature = MmTag.Tag0;
    public const MmTag Humidity = MmTag.Tag1;
    public const MmTag Motion = MmTag.Tag2;
    public const MmTag Light = MmTag.Tag3;
    public const MmTag HVAC = MmTag.Tag4;
    public const MmTag Security = MmTag.Tag5;
    public const MmTag Energy = MmTag.Tag6;
    public const MmTag Custom = MmTag.Tag7;
}
```

### Tag-Based Queries

```csharp
// Query all temperature sensors
building.MmInvoke(
    new MmMessage { method = MmMethod.MessageFloat },
    new MmMetadataBlock(tag: DTTags.Temperature)
);

// Query security sensors only
building.MmInvoke(
    new AlertMessage(),
    new MmMetadataBlock(tag: DTTags.Security)
);

// Query multiple types (bitwise OR)
building.MmInvoke(
    new StatusQuery(),
    new MmMetadataBlock(tag: DTTags.Temperature | DTTags.Humidity)
);
```

---

## Configuration Format

### JSON Configuration

```json
{
  "building": {
    "id": "building_001",
    "name": "Smart Office",
    "floors": [
      {
        "id": "floor_1",
        "name": "Floor 1",
        "rooms": [
          {
            "id": "room_101",
            "name": "Conference Room A",
            "sensors": [
              {
                "id": "temp_101_1",
                "type": "temperature",
                "mqttTopic": "building/floor1/room101/temp1"
              },
              {
                "id": "motion_101_1",
                "type": "motion",
                "mqttTopic": "building/floor1/room101/motion1"
              }
            ]
          }
        ]
      }
    ]
  }
}
```

### Config-to-Hierarchy Generator

```csharp
public class DTHierarchyGenerator
{
    public void GenerateFromConfig(DTConfig config, Transform parent)
    {
        var building = CreateNode<MmRelaySwitchNode>(config.building.name, parent);

        foreach (var floor in config.building.floors)
        {
            var floorNode = CreateNode<MmRelayNode>(floor.name, building.transform);
            floorNode.Tag = DTTags.Floor;

            foreach (var room in floor.rooms)
            {
                var roomNode = CreateNode<MmRelayNode>(room.name, floorNode.transform);

                foreach (var sensor in room.sensors)
                {
                    var sensorNode = CreateSensor(sensor, roomNode.transform);
                }
            }
        }
    }
}
```

---

## Performance Considerations

### Message Throttling
- Aggregate rapid sensor updates
- Configurable sync intervals per sensor type
- Dead-band filtering (ignore small changes)

### Hierarchy Depth
- Recommended: 4-5 levels (Building → Floor → Zone → Room → Sensor)
- Deep hierarchies add routing overhead
- Consider flattening for very large installations

### Memory Management
- Pooled message objects for frequent updates
- Lazy instantiation of rarely-used sensors
- Cleanup of offline/disconnected entities

---

*Last Updated: 2025-12-17*
