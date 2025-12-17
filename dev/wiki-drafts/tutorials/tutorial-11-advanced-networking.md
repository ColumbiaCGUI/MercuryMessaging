# Tutorial 11: Advanced Networking

## Overview

This tutorial explores the MercuryMessaging network architecture in depth. You'll learn how messages are serialized, how network IDs are resolved, and how to implement custom network backends for your preferred networking solution.

## What You'll Learn

- The 3-layer network architecture
- Binary serialization format (`MmBinarySerializer`)
- Network ID resolution (`IMmGameObjectResolver`)
- Creating custom network backends (`IMmNetworkBackend`)
- Network filters and reliability options
- Debugging network issues

## Prerequisites

- Completed [Tutorial 6: FishNet Networking](Tutorial-6-FishNet-Networking)
- Completed [Tutorial 7: Fusion 2 Networking](Tutorial-7-Fusion2-Networking)
- Understanding of basic networking concepts (client/server, RPCs)

---

## Network Architecture Overview

MercuryMessaging uses a 3-layer architecture for network abstraction:

```
┌─────────────────────────────────────────────────┐
│              MmNetworkBridge                    │ ← Orchestrator
│  (Singleton, coordinates all network activity) │
├─────────────────────────────────────────────────┤
│         IMmNetworkBackend                       │ ← Transport Layer
│  (FishNetBackend, Fusion2Backend, Custom...)   │
├─────────────────────────────────────────────────┤
│        IMmGameObjectResolver                    │ ← ID Resolution
│  (FishNetResolver, Fusion2Resolver, Custom...) │
└─────────────────────────────────────────────────┘
```

| Layer | Interface | Purpose |
|-------|-----------|---------|
| **Bridge** | `MmNetworkBridge` | Singleton orchestrator, message routing, registration |
| **Backend** | `IMmNetworkBackend` | Raw byte transport (send/receive) |
| **Resolver** | `IMmGameObjectResolver` | Translate GameObject ↔ Network ID |

---

## Binary Serialization Format

`MmBinarySerializer` converts MmMessage objects to compact byte arrays for network transport.

### Message Format

```
┌──────────────────────────────────────────────────┐
│                    HEADER (17 bytes)             │
├──────────┬────────┬──────┬───────┬───────┬──────┤
│  Magic   │Version │ Type │Method │ NetId │ Meta │
│ 4 bytes  │1 byte  │2 byte│2 bytes│4 bytes│4 byte│
│  "MMSG"  │   1    │ enum │ enum  │ uint  │packed│
├──────────┴────────┴──────┴───────┴───────┴──────┤
│                   PAYLOAD (variable)            │
│        Type-specific data (int, string, etc.)   │
└──────────────────────────────────────────────────┘
```

### Header Fields

| Field | Size | Description |
|-------|------|-------------|
| Magic | 4 bytes | "MMSG" - validates message integrity |
| Version | 1 byte | Format version (currently 1) |
| Type | 2 bytes | MmMessageType enum value |
| Method | 2 bytes | MmMethod enum value |
| NetId | 4 bytes | Target MmRelayNode network ID |
| Metadata | 4 bytes | Packed filters (Level, Active, Selected, Network, Tag) |

### Metadata Packing (18 bits used)

```
Bits 0-3:   LevelFilter (4 bits)
Bits 4-5:   ActiveFilter (2 bits)
Bits 6-7:   SelectedFilter (2 bits)
Bits 8-9:   NetworkFilter (2 bits)
Bits 10-17: Tag (8 bits - supports Tag0-Tag7)
```

### Serialization Example

```csharp
// Serialize a message
MmMessage message = new MmMessageString("Hello Network", MmMetadataBlock.Default);
message.NetId = 12345;  // Target relay node ID

byte[] data = MmBinarySerializer.Serialize(message);
// data is now a compact byte array ready for network transport

// Deserialize on receiving end
MmMessage received = MmBinarySerializer.Deserialize(data);
string value = ((MmMessageString)received).value;  // "Hello Network"
```

### Pooled Serialization (Zero-Allocation)

For high-performance scenarios, use the pooled variants:

```csharp
// Zero-allocation serialization (except final array)
byte[] data = MmBinarySerializer.SerializePooled(message);

// Zero-allocation deserialization
MmMessage received = MmBinarySerializer.DeserializePooled(data);
```

---

## Network ID Resolution

`IMmGameObjectResolver` translates between GameObjects and network IDs.

### The Problem

Different networking solutions use different ID schemes:
- **FishNet**: Uses `NetworkObject.ObjectId` (or path-based hash for scene objects)
- **Fusion 2**: Uses `NetworkObject.Id.Raw`
- **Custom**: Your own ID scheme

### The Solution: Resolver Interface

```csharp
public interface IMmGameObjectResolver
{
    bool TryGetNetworkId(GameObject gameObject, out uint networkId);
    bool TryGetGameObject(uint networkId, out GameObject gameObject);

    uint GetNetworkId(GameObject gameObject);
    GameObject GetGameObject(uint networkId);

    bool TryGetRelayNode(uint networkId, out MmRelayNode relayNode);
    string ResolverName { get; }
}
```

### Path-Based IDs (FishNet Scene Objects)

For scene objects without NetworkObject, FishNet uses deterministic path-based hashes:

```csharp
// FishNetResolver generates deterministic IDs from hierarchy path
// "Root/Level1/Player" → stable hash that's identical on all clients

uint ComputePathHash(GameObject go)
{
    string path = GetFullPath(go);  // "Root/Level1/Player"
    return StableHash(path);        // Same hash on all clients
}
```

This ensures scene objects can be synchronized without requiring NetworkObject components.

---

## Creating a Custom Backend

### Step 1: Implement IMmNetworkBackend

```csharp
using MercuryMessaging.Network;

public class MyCustomBackend : IMmNetworkBackend
{
    // Connection state
    public bool IsConnected { get; private set; }
    public bool IsServer { get; private set; }
    public bool IsClient { get; private set; }
    public int LocalClientId { get; private set; }
    public string BackendName => "MyCustomBackend";

    // Events
    public event MmNetworkMessageReceived OnMessageReceived;
    public event MmNetworkConnectionChanged OnClientConnected;
    public event MmNetworkConnectionChanged OnClientDisconnected;
    public event Action OnConnectedToServer;
    public event Action OnDisconnectedFromServer;

    // Your underlying network library
    private MyNetworkLibrary _network;

    public void Initialize()
    {
        _network = new MyNetworkLibrary();
        _network.OnDataReceived += HandleDataReceived;
        _network.OnConnected += HandleConnected;
    }

    public void Shutdown()
    {
        _network.OnDataReceived -= HandleDataReceived;
        _network.OnConnected -= HandleConnected;
        _network.Disconnect();
    }

    // Send methods
    public void SendToServer(byte[] data, MmReliability reliability = MmReliability.Reliable)
    {
        _network.SendToServer(data, reliability == MmReliability.Reliable);
    }

    public void SendToAllClients(byte[] data, MmReliability reliability = MmReliability.Reliable)
    {
        _network.BroadcastToAll(data, reliability == MmReliability.Reliable);
    }

    public void SendToClient(int clientId, byte[] data, MmReliability reliability = MmReliability.Reliable)
    {
        _network.SendToClient(clientId, data, reliability == MmReliability.Reliable);
    }

    public void SendToOtherClients(int excludeClientId, byte[] data, MmReliability reliability = MmReliability.Reliable)
    {
        foreach (int clientId in _network.ConnectedClients)
        {
            if (clientId != excludeClientId)
                _network.SendToClient(clientId, data, reliability == MmReliability.Reliable);
        }
    }

    // Receive handler - calls the event
    private void HandleDataReceived(byte[] data, int senderId)
    {
        OnMessageReceived?.Invoke(data, senderId);
    }

    private void HandleConnected()
    {
        IsConnected = true;
        OnConnectedToServer?.Invoke();
    }
}
```

### Step 2: Implement IMmGameObjectResolver

```csharp
using UnityEngine;
using MercuryMessaging.Network;

public class MyCustomResolver : IMmGameObjectResolver
{
    public string ResolverName => "MyCustomResolver";

    private Dictionary<uint, GameObject> _idToObject = new Dictionary<uint, GameObject>();
    private Dictionary<GameObject, uint> _objectToId = new Dictionary<GameObject, uint>();

    public void Register(GameObject go, uint networkId)
    {
        _idToObject[networkId] = go;
        _objectToId[go] = networkId;
    }

    public bool TryGetNetworkId(GameObject gameObject, out uint networkId)
    {
        return _objectToId.TryGetValue(gameObject, out networkId);
    }

    public bool TryGetGameObject(uint networkId, out GameObject gameObject)
    {
        return _idToObject.TryGetValue(networkId, out gameObject);
    }

    public uint GetNetworkId(GameObject gameObject)
    {
        return _objectToId.TryGetValue(gameObject, out uint id) ? id : 0;
    }

    public GameObject GetGameObject(uint networkId)
    {
        return _idToObject.TryGetValue(networkId, out GameObject go) ? go : null;
    }

    public bool TryGetRelayNode(uint networkId, out MmRelayNode relayNode)
    {
        if (TryGetGameObject(networkId, out GameObject go) && go != null)
        {
            relayNode = go.GetComponent<MmRelayNode>();
            return relayNode != null;
        }
        relayNode = null;
        return false;
    }
}
```

### Step 3: Configure MmNetworkBridge

```csharp
using UnityEngine;
using MercuryMessaging.Network;

public class NetworkSetup : MonoBehaviour
{
    void Start()
    {
        var backend = new MyCustomBackend();
        var resolver = new MyCustomResolver();

        MmNetworkBridge.Instance.Configure(backend, resolver);
        MmNetworkBridge.Instance.Initialize();
    }

    void OnDestroy()
    {
        MmNetworkBridge.Instance.Shutdown();
    }
}
```

---

## Network Filters

### MmNetworkFilter Enum

```csharp
public enum MmNetworkFilter
{
    Local,      // Only process locally, don't send over network
    Network,    // Only process from network, skip local
    All         // Process both local and network messages
}
```

### Using Network Filters with DSL

```csharp
// Local only - won't be sent over network
relay.Send("LocalEvent").ToDescendants().LocalOnly().Execute();

// Network only - sent over network, not processed locally
relay.Send("ServerCommand").ToDescendants().NetworkOnly().Execute();

// All - both local and network (default)
relay.Send("GlobalEvent").ToDescendants().Execute();
```

### Traditional API

```csharp
relay.MmInvoke(
    MmMethod.MessageString,
    "Hello",
    new MmMetadataBlock(
        MmLevelFilter.Child,
        MmActiveFilter.Active,
        MmSelectedFilter.All,
        MmNetworkFilter.Local  // Local only
    )
);
```

---

## Reliability Options

### MmReliability Enum

```csharp
public enum MmReliability
{
    Unreliable,  // May be dropped, arrives in order (UDP-like)
    Reliable     // Guaranteed delivery, in order (TCP-like)
}
```

### When to Use Each

| Use Case | Reliability |
|----------|-------------|
| Position updates | Unreliable |
| State changes | Reliable |
| Commands (fire, jump) | Reliable |
| Animation triggers | Unreliable |
| Chat messages | Reliable |

### Using Reliability

```csharp
// Send with reliability option
MmNetworkBridge.Instance.SendToAllClients(message, MmReliability.Unreliable);
```

---

## MmNetworkBridge API

### Registration

```csharp
// Register relay node for network message routing
MmNetworkBridge.Instance.RegisterRelayNode(networkId, relayNode);

// Unregister when destroyed
MmNetworkBridge.Instance.UnregisterRelayNode(networkId);

// Find registered relay node
if (MmNetworkBridge.Instance.TryGetRelayNode(networkId, out MmRelayNode node))
{
    // Found it
}
```

### Sending Messages

```csharp
// Auto-routing (client → server, server → all clients)
MmNetworkBridge.Instance.Send(message);

// Specific targets
MmNetworkBridge.Instance.SendToServer(message);
MmNetworkBridge.Instance.SendToAllClients(message);
MmNetworkBridge.Instance.SendToClient(clientId, message);
```

### Events

```csharp
MmNetworkBridge.Instance.OnMessageReceived += (MmMessage msg) =>
{
    Debug.Log($"Received: {msg.MmMessageType}");
};

MmNetworkBridge.Instance.OnMessageSent += (MmMessage msg) =>
{
    Debug.Log($"Sent: {msg.MmMessageType}");
};
```

### Status Properties

```csharp
bool connected = MmNetworkBridge.Instance.IsConnected;
bool isServer = MmNetworkBridge.Instance.IsServer;
bool isClient = MmNetworkBridge.Instance.IsClient;
string backend = MmNetworkBridge.Instance.BackendName;
```

---

## Message Flow Diagram

### Client to Server to Clients

```
[Client A]                    [Server]                   [Client B]
    │                            │                            │
    │  Send("Hello")             │                            │
    │  ───────────────────────►  │                            │
    │        Serialize           │                            │
    │        Send byte[]         │                            │
    │                            │  Deserialize               │
    │                            │  Route to MmRelayNode      │
    │                            │  Process message           │
    │                            │                            │
    │                            │  Forward to clients        │
    │                            │  ───────────────────────►  │
    │                            │                            │
    │                            │        Serialize           │
    │                            │        Send byte[]         │
    │                            │                            │
    │  ◄─────────────────────────│                            │
    │       Deserialize          │        Deserialize         │
    │       Route & Process      │        Route & Process     │
```

---

## Debugging Network Issues

### Enable Network Logging

```csharp
MmLogger.logNetwork = true;
```

### Common Issues and Solutions

| Issue | Possible Cause | Solution |
|-------|---------------|----------|
| "Not initialized" | Bridge not configured | Call `Configure()` then `Initialize()` |
| "Not connected" | Network not started | Start networking before sending |
| "No relay node found" | ID mismatch | Check resolver, verify registration |
| "Bad magic number" | Data corruption | Check binary format, version mismatch |
| Messages not received | NetworkFilter wrong | Check LocalOnly vs NetworkOnly |

### Diagnostic Script

```csharp
public class NetworkDiagnostics : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            var bridge = MmNetworkBridge.Instance;
            Debug.Log($"Network Status:");
            Debug.Log($"  Initialized: {bridge.IsInitialized}");
            Debug.Log($"  Connected: {bridge.IsConnected}");
            Debug.Log($"  Server: {bridge.IsServer}");
            Debug.Log($"  Client: {bridge.IsClient}");
            Debug.Log($"  Backend: {bridge.BackendName}");
        }
    }
}
```

---

## Custom Serializable Types

### Implementing IMmBinarySerializable

For zero-allocation serialization of custom types:

```csharp
using MercuryMessaging.Network;

public class MyCustomData : IMmBinarySerializable
{
    public int Score;
    public string PlayerName;
    public Vector3 Position;

    public void WriteTo(MmWriter writer)
    {
        writer.WriteInt(Score);
        writer.WriteString(PlayerName);
        writer.WriteVector3(Position);
    }

    public void ReadFrom(MmReader reader)
    {
        Score = reader.ReadInt();
        PlayerName = reader.ReadString();
        Position = reader.ReadVector3();
    }
}
```

### Register with MmTypeRegistry

```csharp
// At startup
MmTypeRegistry.Register<MyCustomData>(1001);

// Now you can send it
var data = new MyCustomData { Score = 100, PlayerName = "Player1", Position = Vector3.zero };
var msg = new MmMessageSerializable(data, MmMetadataBlock.Default);
relay.MmInvoke(msg);
```

---

## Backend Comparison

| Feature | FishNet | Fusion 2 |
|---------|---------|----------|
| Transport | Broadcasts | RPCs |
| Scene Objects | Path-based hash | Requires NetworkObject |
| Spawned Objects | NetworkObject.ObjectId | NetworkObject.Id.Raw |
| Free Tier | Yes | Limited |
| Peer-to-Peer | Via relay | Shared mode |
| Dedicated Server | Yes | Yes |

---

## Best Practices

1. **Initialize Early**: Configure MmNetworkBridge before any network activity
2. **Use Pooled Serialization**: For high message volume, use `SerializePooled()`
3. **Match Reliability to Need**: Use Unreliable for frequent updates
4. **Register All Relay Nodes**: Ensure all networked MmRelayNodes are registered
5. **Handle Disconnection**: Clean up registrations when objects are destroyed
6. **Test Both Roles**: Always test as both server and client

---

## Try This

Practice advanced networking concepts:

1. **Build a custom backend** - Create a simple LoopbackBackend that echoes messages locally without actual network transport. Verify serialization and deserialization work correctly.

2. **Implement unreliable position sync** - Create a position synchronization system that sends updates every frame using `MmReliability.Unreliable`. Compare the smoothness vs reliable delivery.

3. **Debug network routing** - Enable `MmLogger.logNetwork = true` and trace a message from client to server and back. Document the complete serialization → send → receive → deserialize flow.

4. **Create a custom serializable type** - Implement `IMmBinarySerializable` for a custom PlayerState class containing position, health, and inventory. Test round-trip serialization.

---

## Next Steps

- **[Tutorial 12: VR Behavioral Experiment](Tutorial-12-VR-Experiment)** - Network + VR
- **[Tutorial 6: FishNet Networking](Tutorial-6-FishNet-Networking)** - FishNet basics (review)
- **[Tutorial 7: Fusion 2 Networking](Tutorial-7-Fusion2-Networking)** - Fusion 2 basics (review)

---

*Tutorial 11 of 14 - MercuryMessaging Wiki*
