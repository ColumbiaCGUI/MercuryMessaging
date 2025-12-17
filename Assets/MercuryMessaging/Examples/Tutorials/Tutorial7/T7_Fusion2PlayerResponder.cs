using UnityEngine;
using System.Collections.Generic;

#if FUSION2_AVAILABLE
using Fusion;
using MercuryMessaging;
using MercuryMessaging.Network;
#endif

/// <summary>
/// Tutorial 7: Player responder for Fusion 2 networking.
///
/// IMPORTANT: This GameObject must have a NetworkObject component
/// to receive network messages in Fusion 2.
///
/// Demonstrates:
/// - Receiving network messages with Fusion 2
/// - Checking message origin with IsDeserialized
/// - NetworkObject requirement for Fusion 2
/// </summary>
public class T7_Fusion2PlayerResponder :
#if FUSION2_AVAILABLE
    MmBaseResponder
#else
    MonoBehaviour
#endif
{
#if FUSION2_AVAILABLE
    [Header("Player State")]
    [SerializeField] private int score = 0;
    [SerializeField] private string playerName = "Player";

    new void Start()
    {
        // Verify NetworkObject is present (required for Fusion 2)
        if (GetComponent<NetworkObject>() == null)
        {
            Debug.LogWarning(
                $"[{playerName}] Missing NetworkObject component!\n" +
                "Fusion 2 requires NetworkObject on MmRelayNodes that receive network messages."
            );
        }
    }

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
            Debug.Log($"[{playerName}] Score from network: {score}");
        }
        else
        {
            Debug.Log($"[{playerName}] Score local: {score}");
        }
    }

    /// <summary>
    /// Receives string messages.
    /// </summary>
    protected override void ReceivedMessage(MmMessageString message)
    {
        string source = message.IsDeserialized ? "network" : "local";
        Debug.Log($"[{playerName}] Received from {source}: {message.value}");

        // Handle game sync message
        if (message.value == "GameSync")
        {
            Debug.Log($"[{playerName}] Processing game sync");
        }
    }

    public int GetScore() => score;
#else
    void Awake()
    {
        Debug.LogWarning(
            "[T7_Fusion2PlayerResponder] Photon Fusion 2 not available.\n" +
            "See T7_Fusion2Setup for setup instructions."
        );
    }
#endif
}
