# Network Backend Testing

This folder contains test scripts for validating MercuryMessaging network backends.

---

## Option A: Loopback Testing (No Network Required)

Test message serialization and deserialization without any networking setup.

### Quick Start

1. Create a new scene (File → New Scene)
2. Create an empty GameObject (right-click → Create Empty)
3. Add component: `NetworkBackendTestRunner`
4. Enter Play mode
5. Click "Run All Tests" button

### What It Tests

- All 13 message types serialize/deserialize correctly
- MmLoopbackBackend echo mode works
- Connection events fire correctly

### Expected Output

```
=== MercuryMessaging Network Backend Tests ===
Backend initialized: Loopback
Testing MmVoid...
  PASS: MmVoid
Testing MmInt...
  PASS: MmInt
... (12 message types) ...
=== Test Summary ===
Passed: 13
Failed: 0
All tests passed!
```

---

## Option B: FishNet Multiplayer Testing

Test actual network communication between two Unity instances using ParrelSync.

### Prerequisites

1. **Install FishNet** (one-time setup):
   - Window → Package Manager
   - Click `+` → Add package from git URL
   - Enter: `https://github.com/FirstGearGames/FishNet.git?path=Assets/FishNet`
   - Wait for import

2. **Add Scripting Define Symbol**:
   - Edit → Project Settings → Player
   - Find "Scripting Define Symbols"
   - Add: `FISHNET_AVAILABLE`
   - Click Apply

3. **Install ParrelSync** (one-time setup):
   - Window → Package Manager
   - Click `+` → Add package from git URL
   - Enter: `https://github.com/VeriorPies/ParrelSync.git?path=/ParrelSync`
   - Wait for import

### Scene Setup

1. Create a new scene
2. Add FishNet's NetworkManager:
   - Find prefab: `Packages/FishNet/Runtime/Prefabs/NetworkManager.prefab`
   - Drag into scene
3. Create empty GameObject, add: `FishNetTestManager`
4. Save scene

### Testing with ParrelSync

1. **Create Clone**:
   - ParrelSync → Clones Manager
   - Click "Create New Clone"
   - Wait for clone to be created

2. **Open Clone**:
   - Click "Open in New Editor" on the clone
   - Wait for second Unity editor to open

3. **Start Server** (in original editor):
   - Enter Play mode
   - In NetworkManager, click "Start Server"
   - Or use: `InstanceFinder.ServerManager.StartConnection()`

4. **Start Client** (in cloned editor):
   - Enter Play mode
   - In NetworkManager, click "Start Client"
   - Connect to: `localhost` or `127.0.0.1`

5. **Send Test Messages**:
   - Use the GUI to send String/Int/Vector3 messages
   - Observe messages received in both editors

### Expected Behavior (Verified 2025-12-01)

**Server → Client (String message):**
```
[FishNetTest] Bridge received MmString (NetId: 765583435): "Hello FishNet!" → Routed to 4 responder(s) ✓
[NetworkTestResponder] TestResponder1 received MmString: "Hello FishNet!" (total: 1, global: 1)
[NetworkTestResponder] TestResponder2 received MmString: "Hello FishNet!" (total: 1, global: 2)
[NetworkTestResponder] NestedResponder1 received MmString: "Hello FishNet!" (total: 1, global: 3)
[NetworkTestResponder] TestRoot received MmString: "Hello FishNet!" (total: 1, global: 4)
```

**Client → Server (Int message):**
```
[FishNetTest] Bridge received MmInt (NetId: 765583435): 42 → Routed to 4 responder(s) ✓
[NetworkTestResponder] TestResponder1 received MmInt: 42 (total: 1, global: 1)
[NetworkTestResponder] TestResponder2 received MmInt: 42 (total: 1, global: 2)
[NetworkTestResponder] NestedResponder1 received MmInt: 42 (total: 1, global: 3)
[NetworkTestResponder] TestRoot received MmInt: 42 (total: 1, global: 4)
```

**Key Observations:**
- Messages route through 1-3 relay hops depending on responder depth
- Same NetId preserved across network boundary
- Hierarchical routing works correctly over network

---

## Troubleshooting

### "FishNet not available"

- Ensure FishNet package is installed
- Add `FISHNET_AVAILABLE` to Scripting Define Symbols
- Reimport scripts if needed

### "No NetworkManager found"

- Add FishNet's NetworkManager prefab to scene
- Ensure it has Transport component configured

### Messages not being received

1. Check both instances are in Play mode
2. Verify server is started before client connects
3. Check Console for connection errors
4. Ensure firewall allows localhost connections

### ParrelSync issues

- Make sure original project is saved before creating clone
- If clone shows errors, delete and recreate it
- Don't modify files in the clone (changes sync from original)

---

## Files

| File | Purpose |
|------|---------|
| `NetworkBackendTestRunner.cs` | Loopback tests for all 13 message types |
| `FishNetTestManager.cs` | FishNet integration test with GUI |
| `README.md` | This file |

---

## Next Steps After Testing

Once tests pass, integrate FishNetBackend into your game:

```csharp
// In your network initialization code
var backend = new FishNetBackend();
var resolver = new FishNetResolver();

backend.Initialize();
backend.OnMessageReceived += (data, senderId) => {
    var message = MmBinarySerializer.Deserialize(data);
    // Route to MmRelayNode...
};
```

See `MmNetworkBridge.cs` for the full integration pattern.
