# Digital Twin Communication Layer

**Status:** Planning
**Priority:** HIGH (Research Opportunity)
**Estimated Effort:** ~250 hours (6 weeks)
**Target Venues:** IEEE IoT, CHI, SIGGRAPH
**Novelty Assessment:** HIGH

---

## Research Contribution

### Problem Statement

Digital Twin (DT) systems require robust communication between physical IoT sensors and their virtual representations. Current approaches face challenges:
- **MQTT/ROS limitations**: Flat publish-subscribe, no hierarchical organization
- **Scalability issues**: Managing thousands of IoT entities
- **State management**: Complex synchronization between physical and virtual
- **No spatial awareness**: Distance-based updates not supported
- **Limited filtering**: Can't target subsets based on hierarchy, state, or tags

### Novel Technical Approach

We propose **MercuryMessaging as Digital Twin Communication Backbone** — leveraging hierarchical message routing for DT systems:

1. **Hierarchical Organization**: DT entities organized in Unity scene graph (Building > Floor > Room > Sensor)
2. **Multi-Level Filtering**: Target messages by Level (floor-only), Tag (sensor type), Active (powered sensors)
3. **Spatial Indexing**: Distance-based updates for proximity-aware DT sync
4. **FSM State Management**: Built-in state machines for DT entity lifecycle
5. **MQTT/Mercury Bridge**: Connect physical sensors to virtual twins via message routing

**Key Differentiation from Existing Work:**
- Existing DT systems use MQTT/ROS (flat, no hierarchy)
- Mercury adds hierarchical organization + filtering + FSM
- First DT backbone using hierarchical message-passing

---

## Literature Analysis (2020-2025)

### Competing/Related Work

| Paper | Year | Venue | Focus | Limitation | Mercury Differentiation |
|-------|------|-------|-------|------------|-------------------------|
| Xi Hu et al. | 2024 | J Building Eng | BIM-based Digital Twin framework | BIM-specific, no real-time message routing | General game engine with filtering |
| Singh et al. | 2024 | Sensors | Unity+ROS Digital Twin | ROS pub/sub only, no hierarchy | Hierarchical message routing |
| Wang et al. | 2023 | IEEE IoT | Blockchain-enhanced DT | Blockchain focus, not communication | Real-time message filtering |
| Park et al. | 2023 | Various | Industry 4.0 DT survey | Survey only, no implementation | Concrete framework |
| Standard MQTT | Various | Various | IoT messaging protocol | Flat topic structure | Hierarchical scene graph |

### Literature Gap Analysis

**What exists:**
- MQTT/ROS-based DT communication - flat publish-subscribe
- BIM-based Digital Twin frameworks - construction specific
- Unity+ROS integration - ROS handles messaging
- Blockchain DT systems - security focus, not communication patterns

**What doesn't exist:**
- **Hierarchical message routing** for DT entity organization
- **Multi-level filtering** for targeted DT updates
- **Spatial indexing** for proximity-based DT synchronization
- **Built-in FSM** for DT entity lifecycle management
- **Game engine native** DT communication (not ROS dependency)

### Novelty Claims

1. **FIRST** Digital Twin backbone using hierarchical message-passing
2. **FIRST** application of scene graph hierarchy to DT organization
3. **FIRST** multi-level filtering (Level, Tag, Active) for DT updates
4. **FIRST** spatial indexing integration for proximity-aware DT sync
5. **Novel** MQTT/Mercury bridge for IoT sensor integration

### Key Citations

```bibtex
@article{hu2024bimdt,
  title={A BIM-based Digital Twin Framework for Building Information Management},
  journal={Journal of Building Engineering},
  year={2024},
  note={47 citations}
}

@article{singh2024unityros,
  title={Unity-ROS Integration for Digital Twin Development},
  journal={Sensors},
  year={2024},
  note={18 citations}
}

@article{wang2023blockchaindt,
  title={Blockchain-Enhanced Digital Twin for IoT Systems},
  journal={IEEE Internet of Things Journal},
  year={2023},
  note={45 citations}
}
```

---

## Technical Architecture

### Hierarchy Mapping: Physical → Virtual

```
Physical World                      Mercury Hierarchy
─────────────────                   ─────────────────
Smart Building                      MmRelaySwitchNode (Building)
├── Floor 1                         ├── MmRelayNode (Floor1)
│   ├── Room 101                    │   ├── MmRelayNode (Room101)
│   │   ├── Temp Sensor            │   │   ├── MmBaseResponder (TempSensor)
│   │   ├── Motion Sensor          │   │   ├── MmBaseResponder (MotionSensor)
│   │   └── Light Controller       │   │   └── MmBaseResponder (LightController)
│   └── Room 102                    │   └── MmRelayNode (Room102)
├── Floor 2                         ├── MmRelayNode (Floor2)
└── HVAC System                     └── MmRelayNode (HVAC)
```

### Message Flow Examples

```csharp
// 1. Update all temperature sensors on Floor 1
floor1Relay.MmInvoke(
    new SensorUpdateMessage { Temperature = 22.5f },
    new MmMetadataBlock(
        MmLevelFilter.SelfAndChildren,
        tag: MmTag.Tag0  // Tag0 = Temperature sensors
    )
);

// 2. Alert only active motion sensors in proximity
buildingRelay.MmInvokeSpatial(
    new AlertMessage { Type = AlertType.Intrusion },
    new SpatialFilter {
        Center = intruderPosition,
        Radius = 10.0f,
        ActiveOnly = true
    }
);

// 3. FSM state transition for building mode
buildingSwitch.RespondersFSM.JumpTo("NightMode");
// Only NightMode responders receive messages
```

### MQTT/Mercury Bridge

```csharp
public class MqttMercuryBridge : MonoBehaviour
{
    private MqttClient _mqttClient;
    private MmRelayNode _rootRelay;

    void Start()
    {
        _mqttClient.Subscribe("building/+/+/sensor");
        _mqttClient.MessageReceived += OnMqttMessage;
    }

    void OnMqttMessage(MqttMessage mqtt)
    {
        // Parse topic: building/floor1/room101/temp_sensor
        var path = ParseTopicPath(mqtt.Topic);
        var relay = FindRelayByPath(path);

        // Convert MQTT payload to Mercury message
        var message = ConvertToMercuryMessage(mqtt.Payload);

        // Route through Mercury hierarchy
        relay.MmInvoke(message);
    }
}
```

---

## Implementation Plan

### Phase 1: Core Bridge Infrastructure (80 hours)
- MQTT client integration (MQTTnet library)
- Topic-to-hierarchy mapping system
- Mercury message conversion layer
- Basic sensor responder templates
- Connection management and reconnection

### Phase 2: Hierarchical DT Organization (60 hours)
- DT entity prefab system
- Automatic hierarchy generation from config
- Dynamic entity creation/destruction
- Parent-child relationship management
- Inspector tools for DT setup

### Phase 3: Filtering and Spatial (60 hours)
- Tag system for sensor types
- Active filter for powered/unpowered sensors
- Spatial indexing integration (from P6)
- Distance-based update throttling
- LOD for message frequency

### Phase 4: State Management (30 hours)
- FSM templates for DT entity lifecycle
- Building mode transitions (Day/Night/Emergency)
- Entity state synchronization
- State persistence and recovery

### Phase 5: Testing and Documentation (20 hours)
- Integration tests with mock MQTT broker
- Performance benchmarks (latency, throughput)
- Example DT scenarios (smart building, factory)
- User documentation and tutorials

---

## Evaluation Methodology

### Performance Benchmarks
- **Message Latency**: MQTT → Mercury → Responder < 10ms
- **Throughput**: 1000 sensor updates/second
- **Scalability**: 10,000 DT entities at 60fps
- **Memory**: < 100MB for 10,000 entities

### Comparison Study
- **Baseline**: Standard MQTT pub/sub
- **Treatment**: Mercury hierarchical routing
- **Metrics**:
  - Developer setup time
  - Message targeting precision
  - Code complexity (LOC)
  - Query response time

### Expected Results
- **50% reduction** in message targeting code
- **10x improvement** in selective updates (hierarchy filtering)
- **Easier state management** with built-in FSM

---

## Application Domains

### Smart Buildings
- HVAC control with zone-based routing
- Security system with proximity alerts
- Lighting control with occupancy detection

### Industrial IoT (Industry 4.0)
- Factory floor monitoring
- Production line state management
- Predictive maintenance alerts

### Smart City
- Traffic system coordination
- Public infrastructure monitoring
- Emergency response systems

### Healthcare
- Hospital room monitoring
- Medical device coordination
- Patient tracking systems

---

## Success Metrics

- [ ] MQTT/Mercury bridge handles 1000 msg/sec
- [ ] Hierarchy creation from config < 1 second
- [ ] Filtering precision 100% (no false positives)
- [ ] Spatial queries < 1ms for 10,000 entities
- [ ] FSM transitions < 10ms
- [ ] Example scenarios: Smart Building, Factory

---

## Dependencies

- MercuryMessaging framework
- MQTTnet (MIT license)
- Spatial Indexing module (P6, optional)
- Unity 2021.3+ LTS

---

## Related Files

- [dt-layer-context.md](dt-layer-context.md) - Technical design details
- [dt-layer-tasks.md](dt-layer-tasks.md) - Implementation checklist

---

*Created: 2025-12-17*
*Last Updated: 2025-12-17*
