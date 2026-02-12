# Tutorial 6: Networking with FishNet

## Overview

MercuryMessaging provides seamless integration with [FishNet](https://fish-networking.gitbook.io/docs/), a powerful Unity networking solution. This tutorial shows how to send MercuryMessaging messages over the network to synchronize game state across clients.

## What You'll Learn

- Setting up FishNet with MercuryMessaging
- Configuring `MmNetworkBridge` and `FishNetBackend`
- Sending messages from server to clients and vice versa
- Testing with ParrelSync
- Understanding network IDs and routing

## Prerequisites

- Completed [Tutorial 1](Tutorial-1-Introduction) through [Tutorial 5](Tutorial-5-Fluent-DSL-API)
- FishNet installed via Package Manager
- Basic understanding of Unity networking concepts

---

## Architecture Overview

MercuryMessaging networking uses three key components:

```
┌─────────────────────────────────────────────────────────────┐
│                    Your Application Code                     │
│         relay.Send("Hello").ToDescendants().OverNetwork()   │
└─────────────────────────────┬───────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────┐
│                      MmNetworkBridge                         │
│    - Serializes/Deserializes messages (MmBinarySerializer)  │
│    - Resolves network IDs to GameObjects                    │
└─────────────────────────────┬───────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────┐
│                      FishNetBackend                          │
│    - Implements IMmNetworkBackend                           │
│    - Uses FishNet Broadcast system                          │
│    - Handles connection events                              │
└─────────────────────────────┬───────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────┐
│                         FishNet                              │
│    - Actual network transport (TCP/UDP)                     │
│    - Manages connections, lobbies, etc.                     │
└─────────────────────────────────────────────────────────────┘
```

---

## Step-by-Step Setup

### Step 1: Install FishNet

1. Open Package Manager (Window → Package Manager)
2. Click **+** → **Add package from git URL**
3. Enter: `https://github.com/FirstGearGames/FishNet.git`
4. Click **Add**

Or use the Asset Store version for automatic updates.

### Step 2: Add Define Symbol

1. Go to **Edit → Project Settings → Player**
2. Find **Scripting Define Symbols**
3. Add: `FISHNET_AVAILABLE`
4. Click **Apply**

### Step 3: Setup FishNet in Scene

1. Create an empty GameObject named "NetworkManager"
2. Add the `NetworkManager` component (from FishNet)
3. Configure your transport (Tugboat is recommended for LAN testing)

### Step 4: Setup MmNetworkBridge

Create a setup script to configure the bridge:

```csharp
using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Network;
using MercuryMessaging.Network.Backends;

public class NetworkSetup : MonoBehaviour
{
    void Awake()
    {
        // Configure the network bridge with FishNet backend
        var bridge = MmNetworkBridge.Instance;
        bridge.SetBackend(new FishNetBackend());
        bridge.SetResolver(new FishNetResolver());
        bridge.Initialize();

        Debug.Log("MercuryMessaging network bridge initialized with FishNet");
    }
}
```

Or use the included `FishNetBridgeSetup` component:

1. Add `FishNetBridgeSetup` component to your NetworkManager
2. It auto-configures everything on Awake

---

## Sending Network Messages

### Using the DSL (Recommended)

```csharp
using UnityEngine;
using MercuryMessaging;

public class NetworkController : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
    }

    // Send to all clients (server only)
    public void BroadcastToAllClients()
    {
        relay.Send("Hello from server!")
            .ToDescendants()
            .OverNetwork()  // This sends over the network!
            .Execute();
    }

    // Send to server (client only)
    public void SendToServer()
    {
        relay.Send("Hello from client!")
            .ToDescendants()
            .OverNetwork()
            .Execute();
    }

    // Local only (no network)
    public void LocalBroadcast()
    {
        relay.Send("Local message")
            .ToDescendants()
            .LocalOnly()  // Default - stays local
            .Execute();
    }
}
```

### Using Traditional API

```csharp
// Network message
relay.MmInvoke(
    MmMethod.MessageString,
    "Hello",
    new MmMetadataBlock(
        MmLevelFilter.SelfAndChildren,
        MmActiveFilter.Active,
        MmSelectedFilter.All,
        MmNetworkFilter.All  // MmNetworkFilter.All = send over network
    )
);

// Local only
relay.MmInvoke(
    MmMethod.MessageString,
    "Local",
    new MmMetadataBlock(
        MmLevelFilter.SelfAndChildren,
        MmActiveFilter.Active,
        MmSelectedFilter.All,
        MmNetworkFilter.Local  // MmNetworkFilter.Local = local only
    )
);
```

---

## Receiving Network Messages

Network messages are received exactly like local messages. The framework handles deserialization automatically:

```csharp
using UnityEngine;
using MercuryMessaging;

public class NetworkResponder : MmBaseResponder
{
    protected override void ReceivedMessage(MmMessageString message)
    {
        // Check if this came from the network
        if (message.IsDeserialized)
        {
            Debug.Log($"Received from network: {message.value}");
        }
        else
        {
            Debug.Log($"Received locally: {message.value}");
        }
    }

    protected override void ReceivedMessage(MmMessageInt message)
    {
        Debug.Log($"Received int: {message.value}");
    }

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("Initialized (possibly from network)");
    }
}
```

---

## Complete Example

### Hierarchy Setup

```
NetworkRoot (MmRelayNode + NetworkGameController)
  ├── Player (MmRelayNode + PlayerResponder)
  ├── Enemy (MmRelayNode + EnemyResponder)
  └── UI (MmRelayNode + UIResponder)
```

### NetworkGameController.cs (Server)

```csharp
using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Network;

public class NetworkGameController : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();

        // Subscribe to network events
        if (MmNetworkBridge.Instance.Backend != null)
        {
            MmNetworkBridge.Instance.Backend.OnClientConnected += OnClientConnected;
            MmNetworkBridge.Instance.Backend.OnClientDisconnected += OnClientDisconnected;
        }
    }

    void OnClientConnected(int clientId)
    {
        Debug.Log($"Client {clientId} connected");

        // Initialize the new client
        relay.BroadcastInitialize();
    }

    void OnClientDisconnected(int clientId)
    {
        Debug.Log($"Client {clientId} disconnected");
    }

    void Update()
    {
        // Server: Press S to sync game state
        if (Input.GetKeyDown(KeyCode.S) && MmNetworkBridge.Instance.Backend.IsServer)
        {
            SyncGameState();
        }
    }

    void SyncGameState()
    {
        // Send current score to all clients
        relay.Send(GetCurrentScore())
            .ToDescendants()
            .OverNetwork()
            .Execute();
    }

    int GetCurrentScore() => 100; // Your game logic
}
```

### PlayerResponder.cs (Client/Server)

```csharp
using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Network;

public class PlayerResponder : MmBaseResponder
{
    [SerializeField] private int score = 0;

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log("Player initialized");
    }

    protected override void ReceivedMessage(MmMessageInt message)
    {
        // Received score update from server
        score = message.value;
        Debug.Log($"Score updated: {score}");
    }

    void Update()
    {
        // Client: Press Space to request action
        if (Input.GetKeyDown(KeyCode.Space) && !MmNetworkBridge.Instance.Backend.IsServer)
        {
            RequestAction();
        }
    }

    void RequestAction()
    {
        // Send request to server
        this.Send("ActionRequest")
            .ToParents()
            .OverNetwork()
            .Execute();
    }
}
```

---

## Network IDs and Routing

MercuryMessaging uses **deterministic path-based IDs** for scene objects. This means:

- Objects at the same hierarchy path have the same network ID
- IDs are consistent across server and clients (if hierarchies match)
- No `NetworkObject` component needed on MercuryMessaging objects

### How It Works

```csharp
// FishNetResolver generates IDs from hierarchy path
// "/GameWorld/Player/Inventory" → hash → networkId

// Server sends to networkId 12345678
// Client receives, resolves 12345678 → "/GameWorld/Player/Inventory"
// Message routed to that MmRelayNode
```

### Important: Matching Hierarchies

For network messages to route correctly:
- Server and client must have matching GameObject hierarchies
- Object names and parent relationships must match
- Different hierarchy = different network ID = message not delivered

---

## Testing with ParrelSync

[ParrelSync](https://github.com/VeriorPies/ParrelSync) lets you run multiple Unity editors for local network testing.

### Setup

1. Install ParrelSync from git: `https://github.com/VeriorPies/ParrelSync.git`
2. Open **ParrelSync → Clones Manager**
3. Create a clone
4. Open the clone project

### Testing

1. **Main Editor**: Start as Server (Host)
2. **Clone**: Start as Client, connect to localhost
3. Test bidirectional messaging

### Example Test Flow

```
SERVER (Main Editor):
  Press S → Sends "Hello from server!"
  Log: "Sent to 1 client(s)"

CLIENT (Clone):
  Log: "Received from network: Hello from server!"
  Press Space → Sends "ActionRequest"

SERVER:
  Log: "Received from network: ActionRequest"
```

**Expected Output (server Console when client connects):**
```
Client 1 connected
Player initialized
```

---

## Common Mistakes

| Mistake | Solution |
|---------|----------|
| `FISHNET_AVAILABLE` not defined | Add to Scripting Define Symbols |
| Messages not received | Check `.OverNetwork()` filter is used |
| NetworkBridge not initialized | Add `FishNetBridgeSetup` or call `Initialize()` |
| Wrong objects receiving messages | Verify hierarchy paths match on server/client |
| `IsServer`/`IsClient` always false | FishNet not connected - check NetworkManager |

---

## Network Filters Reference

| Filter | Behavior |
|--------|----------|
| `.LocalOnly()` | Message stays local (default) |
| `.OverNetwork()` | Message is serialized and sent over network |
| `.NetworkOnly()` | Only send over network, skip local |

In traditional API:
- `MmNetworkFilter.Local` = Local only
- `MmNetworkFilter.Network` = Network only
- `MmNetworkFilter.All` = Both local and network

---

## Troubleshooting

### Messages not reaching clients

1. Verify `FishNetBridgeSetup` is in scene
2. Check `FISHNET_AVAILABLE` define is set
3. Confirm `.OverNetwork()` is used
4. Verify hierarchy paths match

### "Backend not initialized" error

```csharp
// Make sure bridge is initialized before sending
void Start()
{
    var bridge = MmNetworkBridge.Instance;
    if (!bridge.IsInitialized)
    {
        bridge.SetBackend(new FishNetBackend());
        bridge.SetResolver(new FishNetResolver());
        bridge.Initialize();
    }
}
```

### Checking connection status

```csharp
var backend = MmNetworkBridge.Instance.Backend;
Debug.Log($"Connected: {backend.IsConnected}");
Debug.Log($"Is Server: {backend.IsServer}");
Debug.Log($"Is Client: {backend.IsClient}");
Debug.Log($"Client ID: {backend.LocalClientId}");
```

---

## Try This

Practice FishNet networking:

1. **Send different message types** - Try sending int, float, and Vector3 messages over the network. Verify they serialize and deserialize correctly.

2. **Build a chat system** - Create a simple chat where clients can send string messages to the server, which broadcasts them to all clients.

3. **Track connected clients** - Modify `OnClientConnected` to maintain a list of connected client IDs. Display this list in the console.

4. **Test message ordering** - Send multiple messages rapidly and verify they arrive in order on the client.

---

## Next Steps

- **[Tutorial 7: Fusion 2 Networking](Tutorial-7-Fusion2-Networking)** - Alternative networking backend
- **[Tutorial 11: Advanced Networking](Tutorial-11-Advanced-Networking)** - Custom backends, serialization details
- **[Tutorial 4: Custom Messages](Tutorial-4-Custom-Messages)** - Network-ready custom message types

---

*Tutorial 6 of 14 - MercuryMessaging Wiki*
