using UnityEngine;
using System.Collections.Generic;

#if FISHNET_AVAILABLE
using MercuryMessaging;
using MercuryMessaging.Network;
#endif

/// <summary>
/// Tutorial 6: Player responder for network messages.
/// Works on both server and client.
///
/// Demonstrates:
/// - Receiving network messages (score updates from server)
/// - Sending network messages (action requests to server)
/// - Checking IsDeserialized to know message origin
///
/// Keyboard Controls (Client Only):
/// Space - Send action request to server
/// </summary>
public class T6_PlayerResponder :
#if FISHNET_AVAILABLE
    MmBaseResponder
#else
    MonoBehaviour
#endif
{
#if FISHNET_AVAILABLE
    [Header("Player State")]
    [SerializeField] private int score = 0;
    [SerializeField] private string playerName = "Player";

    public override void Initialize()
    {
        base.Initialize();
        Debug.Log($"[{playerName}] Initialized");
    }

    public override void Refresh(List<MmTransform> transformList)
    {
        Debug.Log($"[{playerName}] Refreshed");
    }

    /// <summary>
    /// Receives score updates from server.
    /// </summary>
    protected override void ReceivedMessage(MmMessageInt message)
    {
        score = message.value;

        if (message.IsDeserialized)
        {
            Debug.Log($"[{playerName}] Score updated from network: {score}");
        }
        else
        {
            Debug.Log($"[{playerName}] Score updated locally: {score}");
        }
    }

    /// <summary>
    /// Receives string messages (e.g., status updates).
    /// </summary>
    protected override void ReceivedMessage(MmMessageString message)
    {
        if (message.IsDeserialized)
        {
            Debug.Log($"[{playerName}] Received from network: {message.value}");
        }
        else
        {
            Debug.Log($"[{playerName}] Received locally: {message.value}");
        }
    }

    new void Update()
    {
        // Only clients can request actions
        var backend = MmNetworkBridge.Instance?.Backend;
        if (backend == null) return;

        // Check if we're a client (not server/host)
        bool isClientOnly = backend.IsClient && !backend.IsServer;

        // Space - Send action request to server
        if (Input.GetKeyDown(KeyCode.Space) && isClientOnly)
        {
            SendActionRequest();
        }
    }

    void SendActionRequest()
    {
        // Send request to parent (server) over network
        this.Send("ActionRequest")
            .ToParents()
            .OverNetwork()
            .Execute();

        Debug.Log($"[{playerName}] Sent ActionRequest to server");
    }

    public int GetScore() => score;
#else
    void Awake()
    {
        Debug.LogWarning(
            "[T6_PlayerResponder] FishNet not available.\n" +
            "See T6_NetworkSetup for setup instructions."
        );
    }
#endif
}
