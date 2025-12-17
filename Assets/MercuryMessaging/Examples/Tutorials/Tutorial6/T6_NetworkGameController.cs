using UnityEngine;

#if FISHNET_AVAILABLE
using MercuryMessaging;
using MercuryMessaging.Network;
#endif

/// <summary>
/// Tutorial 6: Server-side network game controller.
/// Demonstrates sending messages from server to all clients.
///
/// Hierarchy Setup:
/// NetworkRoot (MmRelayNode + T6_NetworkGameController)
///   ├── Player (MmRelayNode + T6_PlayerResponder)
///   └── UI (MmRelayNode + T6_UIResponder)
///
/// Keyboard Controls (Server Only):
/// S - Sync game state to all clients
/// I - Initialize all responders
/// R - Refresh all responders
/// </summary>
public class T6_NetworkGameController : MonoBehaviour
{
#if FISHNET_AVAILABLE
    private MmRelayNode relay;
    private int currentScore = 0;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();

        // Subscribe to network events
        if (MmNetworkBridge.Instance.Backend != null)
        {
            MmNetworkBridge.Instance.Backend.OnClientConnected += OnClientConnected;
            MmNetworkBridge.Instance.Backend.OnClientDisconnected += OnClientDisconnected;
        }

        Debug.Log("[T6_NetworkGameController] Started. Press S to sync (server only)");
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (MmNetworkBridge.Instance?.Backend != null)
        {
            MmNetworkBridge.Instance.Backend.OnClientConnected -= OnClientConnected;
            MmNetworkBridge.Instance.Backend.OnClientDisconnected -= OnClientDisconnected;
        }
    }

    void OnClientConnected(int clientId)
    {
        Debug.Log($"[T6_NetworkGameController] Client {clientId} connected");

        // Send current state to new client
        if (MmNetworkBridge.Instance.Backend.IsServer)
        {
            relay.BroadcastInitialize();
            SyncGameState();
        }
    }

    void OnClientDisconnected(int clientId)
    {
        Debug.Log($"[T6_NetworkGameController] Client {clientId} disconnected");
    }

    void Update()
    {
        // Only server can control
        if (MmNetworkBridge.Instance?.Backend?.IsServer != true) return;

        // S - Sync game state to all clients
        if (Input.GetKeyDown(KeyCode.S))
        {
            currentScore += 10;
            SyncGameState();
        }

        // I - Initialize all (over network)
        if (Input.GetKeyDown(KeyCode.I))
        {
            relay.Send(MmMethod.Initialize)
                .ToDescendants()
                .OverNetwork()
                .Execute();
            Debug.Log("[T6_NetworkGameController] Sent Initialize over network");
        }

        // R - Refresh all (over network)
        if (Input.GetKeyDown(KeyCode.R))
        {
            relay.Send(MmMethod.Refresh)
                .ToDescendants()
                .OverNetwork()
                .Execute();
            Debug.Log("[T6_NetworkGameController] Sent Refresh over network");
        }
    }

    void SyncGameState()
    {
        // Send current score to all clients
        relay.Send(currentScore)
            .ToDescendants()
            .OverNetwork()
            .Execute();
        Debug.Log($"[T6_NetworkGameController] Synced score: {currentScore}");
    }

    /// <summary>
    /// Called by T6_PlayerResponder when player requests an action.
    /// </summary>
    public void OnPlayerAction(string action)
    {
        Debug.Log($"[T6_NetworkGameController] Received action from player: {action}");

        // Process action (server-side logic)
        if (action == "ActionRequest")
        {
            currentScore += 5;
            SyncGameState();
        }
    }
#else
    void Awake()
    {
        Debug.LogWarning(
            "[T6_NetworkGameController] FishNet not available.\n" +
            "See T6_NetworkSetup for setup instructions."
        );
    }
#endif
}
