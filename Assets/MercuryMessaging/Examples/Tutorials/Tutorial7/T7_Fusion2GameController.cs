using UnityEngine;

#if FUSION2_AVAILABLE
using Fusion;
using MercuryMessaging;
using MercuryMessaging.Network;
using MercuryMessaging.Network.Backends;
#endif

/// <summary>
/// Tutorial 7: Game controller for Fusion 2 networking.
///
/// IMPORTANT: For Fusion 2, this GameObject must have a NetworkObject component
/// to receive network messages. This is different from FishNet.
///
/// Hierarchy Setup (Fusion 2 requires NetworkObject on each MmRelayNode):
/// GameRoot (NetworkObject + MmRelayNode + T7_Fusion2GameController)
///   ├── Player1 (NetworkObject + MmRelayNode + T7_Fusion2PlayerResponder)
///   └── Player2 (NetworkObject + MmRelayNode + T7_Fusion2PlayerResponder)
///
/// Keyboard Controls:
/// S - Sync game state (server only)
/// I - Initialize all (over network)
/// Space - Request action (client only)
/// </summary>
public class T7_Fusion2GameController : MonoBehaviour
{
#if FUSION2_AVAILABLE
    private MmRelayNode relay;
    private int currentScore = 0;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();

        // Verify NetworkObject is present (required for Fusion 2)
        if (GetComponent<NetworkObject>() == null)
        {
            Debug.LogWarning(
                "[T7_Fusion2GameController] Missing NetworkObject component!\n" +
                "Fusion 2 requires NetworkObject on MmRelayNodes that receive network messages."
            );
        }

        Debug.Log("[T7_Fusion2GameController] Started. Press S to sync (server only)");
    }

    void Update()
    {
        var backend = MmNetworkBridge.Instance?.Backend;
        if (backend == null || !backend.IsConnected) return;

        // Check if MmFusion2Bridge is ready
        if (MmFusion2Bridge.Instance == null)
        {
            // Bridge not spawned yet
            return;
        }

        // S - Server: Sync game state to all clients
        if (Input.GetKeyDown(KeyCode.S) && backend.IsServer)
        {
            currentScore += 10;
            SyncGameState();
        }

        // I - Initialize all (over network)
        if (Input.GetKeyDown(KeyCode.I) && backend.IsServer)
        {
            relay.Send(MmMethod.Initialize)
                .ToDescendants()
                .OverNetwork()
                .Execute();
            Debug.Log("[T7_Fusion2GameController] Sent Initialize over network");
        }

        // Space - Client: Request action from server
        if (Input.GetKeyDown(KeyCode.Space) && backend.IsClient && !backend.IsServer)
        {
            RequestAction();
        }
    }

    void SyncGameState()
    {
        relay.Send(currentScore)
            .ToDescendants()
            .OverNetwork()
            .Execute();
        Debug.Log($"[T7_Fusion2GameController] Synced score: {currentScore}");
    }

    void RequestAction()
    {
        relay.Send("ActionRequest")
            .ToParents()
            .OverNetwork()
            .Execute();
        Debug.Log("[T7_Fusion2GameController] Sent ActionRequest to server");
    }

    /// <summary>
    /// Handle action requests from clients.
    /// </summary>
    public void OnActionRequest(string action)
    {
        if (MmNetworkBridge.Instance?.Backend?.IsServer != true) return;

        Debug.Log($"[T7_Fusion2GameController] Processing action: {action}");
        currentScore += 5;
        SyncGameState();
    }
#else
    void Awake()
    {
        Debug.LogWarning(
            "[T7_Fusion2GameController] Photon Fusion 2 not available.\n" +
            "See T7_Fusion2Setup for setup instructions."
        );
    }
#endif
}
