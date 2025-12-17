# Tutorial 7: Networking with Photon Fusion 2

## Overview

MercuryMessaging supports [Photon Fusion 2](https://doc.photonengine.com/fusion/current/getting-started/fusion-intro), Photon's high-performance networking solution. This tutorial shows how to configure Fusion 2 with MercuryMessaging for synchronized multiplayer experiences.

## What You'll Learn

- Setting up Photon Fusion 2 with MercuryMessaging
- Configuring `MmNetworkBridge` and `Fusion2Backend`
- Understanding Fusion 2's RPC-based approach vs FishNet's Broadcasts
- Key differences from FishNet setup

## Prerequisites

- Completed [Tutorial 6: FishNet Networking](Tutorial-6-FishNet-Networking)
- Photon Fusion 2 installed
- Photon account with App ID

---

## FishNet vs Fusion 2

| Aspect | FishNet | Fusion 2 |
|--------|---------|----------|
| Transport | Broadcasts (global) | RPCs (object-based) |
| Scene Objects | No NetworkObject needed | Requires spawned NetworkObject |
| Bridge Component | None (uses InstanceFinder) | MmFusion2Bridge (NetworkBehaviour) |
| Define Symbol | `FISHNET_AVAILABLE` | `FUSION2_AVAILABLE` |
| ID Resolution | Path-based hash | NetworkObject.Id.Raw |

---

## Step-by-Step Setup

### Step 1: Install Photon Fusion 2

1. Download Fusion 2 SDK from [Photon Dashboard](https://dashboard.photonengine.com/)
2. Import the Unity package
3. Enter your App ID in Photon settings

### Step 2: Add Define Symbol

1. Go to **Edit → Project Settings → Player**
2. Find **Scripting Define Symbols**
3. Add: `FUSION2_AVAILABLE`
4. Click **Apply**

**Note:** If using assembly definitions, you can use `versionDefines` instead.

### Step 3: Setup Fusion in Scene

1. Create NetworkRunner GameObject
2. Add `NetworkRunner` component
3. Configure your connection settings (Shared/Server/Client mode)

### Step 4: Create MmFusion2Bridge Prefab

Unlike FishNet, Fusion 2 requires a spawned NetworkObject to handle RPCs:

**MmFusion2Bridge.cs** (included in MercuryMessaging):

```csharp
// This NetworkBehaviour handles the actual RPC calls
// It must be on a spawned NetworkObject

[RequireComponent(typeof(NetworkObject))]
public class MmFusion2Bridge : NetworkBehaviour
{
    // Singleton pattern for global access
    public static MmFusion2Bridge Instance { get; private set; }

    public override void Spawned()
    {
        Instance = this;
        // Connect to backend
        Fusion2Backend.Instance?.SetBridge(this);
    }

    // Server → Client RPC
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcBroadcastToClients(byte[] data)
    {
        Fusion2Backend.Instance?.HandleReceivedMessage(data, -1);
    }

    // Client → Server RPC
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSendToServer(byte[] data, RpcInfo info = default)
    {
        var clientId = info.Source.PlayerId;
        Fusion2Backend.Instance?.HandleReceivedMessage(data, clientId);
    }
}
```

### Step 5: Configure the Bridge

Create a setup script:

```csharp
using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Network;
using MercuryMessaging.Network.Backends;

public class Fusion2Setup : MonoBehaviour
{
    [SerializeField] private NetworkRunner runner;
    [SerializeField] private MmFusion2Bridge bridgePrefab;

    async void Start()
    {
        // Configure MercuryMessaging network bridge
        var bridge = MmNetworkBridge.Instance;
        bridge.SetBackend(new Fusion2Backend());
        bridge.SetResolver(new Fusion2Resolver());
        bridge.Initialize();

        // Start Fusion session
        await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = "MySession",
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        // Spawn the bridge object (host/server only)
        if (runner.IsServer)
        {
            runner.Spawn(bridgePrefab);
        }
    }
}
```

Or use the included `Fusion2BridgeSetup` component.

---

## Sending Network Messages

The DSL API is identical to FishNet:

```csharp
using UnityEngine;
using MercuryMessaging;

public class Fusion2Controller : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
    }

    // Send to all clients (server/host only)
    public void BroadcastToClients()
    {
        relay.Send("Hello from server!")
            .ToDescendants()
            .OverNetwork()
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
}
```

---

## Network ID Resolution

Fusion 2 uses `NetworkObject.Id.Raw` for network IDs instead of path-based hashes:

```csharp
// Fusion2Resolver uses NetworkObject IDs
public class Fusion2Resolver : IMmGameObjectResolver
{
    public bool TryGetNetworkId(GameObject go, out uint networkId)
    {
        var nob = go.GetComponent<NetworkObject>();
        if (nob != null && nob.IsValid && nob.Id.Raw > 0)
        {
            networkId = nob.Id.Raw;
            return true;
        }
        networkId = 0;
        return false;
    }

    public bool TryGetGameObject(uint networkId, out GameObject go)
    {
        var netId = new NetworkId { Raw = networkId };
        if (runner.TryFindObject(netId, out NetworkObject nob))
        {
            go = nob.gameObject;
            return true;
        }
        go = null;
        return false;
    }
}
```

### Important: NetworkObject Requirement

For Fusion 2, **MmRelayNodes that receive network messages must have a NetworkObject**:

```
CORRECT (Fusion 2):
  GameWorld (NetworkObject + MmRelayNode + Controller)
    ├─ Player (NetworkObject + MmRelayNode + PlayerResponder)
    └─ UI (NetworkObject + MmRelayNode + UIResponder)

WRONG (Fusion 2):
  GameWorld (MmRelayNode only)  ← No NetworkObject = can't receive network messages
    ├─ Player (MmRelayNode only)
    └─ UI (MmRelayNode only)
```

This is different from FishNet, which uses path-based IDs and doesn't require NetworkObjects.

---

## Complete Example

### Hierarchy Setup

```
NetworkManager (NetworkRunner + Fusion2Setup)
├── MmFusion2Bridge (spawned prefab with NetworkObject)
└── GameRoot (NetworkObject + MmRelayNode + GameController)
      ├── Player1 (NetworkObject + MmRelayNode + PlayerResponder)
      └── Player2 (NetworkObject + MmRelayNode + PlayerResponder)
```

### GameController.cs

```csharp
using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Network;

#if FUSION2_AVAILABLE
using Fusion;
#endif

public class GameController : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
    }

    void Update()
    {
#if FUSION2_AVAILABLE
        var backend = MmNetworkBridge.Instance.Backend;

        // Server: Press S to sync
        if (Input.GetKeyDown(KeyCode.S) && backend.IsServer)
        {
            relay.Send("GameSync")
                .ToDescendants()
                .OverNetwork()
                .Execute();
        }

        // Client: Press Space to request action
        if (Input.GetKeyDown(KeyCode.Space) && backend.IsClient && !backend.IsServer)
        {
            relay.Send("ActionRequest")
                .ToAncestors()
                .OverNetwork()
                .Execute();
        }
#endif
    }
}
```

### PlayerResponder.cs

```csharp
using UnityEngine;
using MercuryMessaging;

public class PlayerResponder : MmBaseResponder
{
    protected override void ReceivedMessage(MmMessageString message)
    {
        if (message.value == "GameSync")
        {
            Debug.Log($"[{name}] Received sync from server");
            // Update local state
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log($"[{name}] Player initialized");
    }
}
```

---

## Shared vs Client-Server Mode

Fusion 2 supports multiple game modes. MercuryMessaging works with all of them:

### Shared Mode (Recommended for Prototyping)
- All clients have state authority over their own objects
- Simpler setup, good for small games

### Client-Server Mode
- Server has state authority
- Clients request actions, server validates
- Better for competitive games

```csharp
// Check mode
var runner = NetworkRunner.GetRunnerForGameObject(gameObject);
bool isShared = runner.GameMode == GameMode.Shared;
```

---

## Common Mistakes

| Mistake | Solution |
|---------|----------|
| `FUSION2_AVAILABLE` not defined | Add to Scripting Define Symbols |
| MmFusion2Bridge not spawned | Host must spawn the bridge prefab |
| Messages not received | Verify NetworkObject on receiving MmRelayNode |
| "Bridge is null" error | Wait for bridge to spawn before sending |
| Wrong objects receiving | Check NetworkObject.Id matches on server/client |

---

## Comparison: When to Use Which?

| Use Case | Recommended |
|----------|-------------|
| LAN/localhost testing | FishNet |
| Quick prototyping | FishNet |
| Cloud-hosted servers | Fusion 2 |
| Competitive multiplayer | Fusion 2 |
| Existing Photon infrastructure | Fusion 2 |
| Need free tier | FishNet |

---

## Troubleshooting

### "Backend not connected"

```csharp
// Wait for Fusion to connect
IEnumerator WaitForConnection()
{
    var backend = MmNetworkBridge.Instance.Backend;
    while (!backend.IsConnected)
    {
        yield return new WaitForSeconds(0.1f);
    }
    Debug.Log("Connected!");
}
```

### "Bridge is null"

The MmFusion2Bridge must be spawned before sending messages:

```csharp
// Check if bridge is ready
if (MmFusion2Bridge.Instance == null)
{
    Debug.LogWarning("Bridge not spawned yet");
    return;
}
```

### Debugging network messages

```csharp
// Enable network logging
MmLogger.logNetwork = true;
```

---

## Try This

Practice Fusion 2 networking:

1. **Convert FishNet to Fusion 2** - Take the FishNet example from Tutorial 6 and convert it to use Fusion 2. Note the differences required (NetworkObject requirements).

2. **Test Shared vs Server mode** - Try both `GameMode.Shared` and `GameMode.Server`. Note how state authority differs.

3. **Add player spawning** - Modify the setup to spawn a player prefab for each connecting client, each with its own NetworkObject and MmRelayNode.

4. **Hybrid local/network** - Create a scenario where some messages stay local (UI updates) while others go over the network (game state).

---

## Next Steps

- **[Tutorial 8: Switch Nodes & FSM](Tutorial-8-Switch-Nodes-FSM)** - State machines over network
- **[Tutorial 11: Advanced Networking](Tutorial-11-Advanced-Networking)** - Custom backends, serialization format
- **[Photon Fusion 2 Documentation](https://doc.photonengine.com/fusion/current/getting-started/fusion-intro)** - Official Fusion docs

---

*Tutorial 7 of 14 - MercuryMessaging Wiki*
