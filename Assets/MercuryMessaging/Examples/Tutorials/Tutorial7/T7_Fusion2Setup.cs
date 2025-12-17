using UnityEngine;
using System.Collections;

#if FUSION2_AVAILABLE
using Fusion;
using Fusion.Sockets;
using MercuryMessaging;
using MercuryMessaging.Network;
using MercuryMessaging.Network.Backends;
#endif

/// <summary>
/// Tutorial 7: Network bridge setup for Photon Fusion 2.
/// Initializes MmNetworkBridge with Fusion2Backend.
///
/// Prerequisites:
/// 1. Photon Fusion 2 SDK installed
/// 2. FUSION2_AVAILABLE added to Scripting Define Symbols
/// 3. NetworkRunner configured in scene
/// 4. Valid Photon App ID configured
///
/// Usage: Add this component to your NetworkRunner GameObject.
/// Assign the MmFusion2Bridge prefab reference.
/// </summary>
public class T7_Fusion2Setup : MonoBehaviour
{
#if FUSION2_AVAILABLE
    [Header("Fusion References")]
    [Tooltip("Reference to the NetworkRunner")]
    [SerializeField] private NetworkRunner runner;

    [Tooltip("Prefab containing MmFusion2Bridge component")]
    [SerializeField] private MmFusion2Bridge bridgePrefab;

    [Header("Configuration")]
    [Tooltip("Session name for Fusion connection")]
    [SerializeField] private string sessionName = "MercurySession";

    [Tooltip("Game mode for Fusion (Shared is easiest for testing)")]
    [SerializeField] private GameMode gameMode = GameMode.Shared;

    [Tooltip("Auto-start connection on Start")]
    public bool autoStart = true;

    async void Start()
    {
        if (runner == null)
        {
            runner = GetComponent<NetworkRunner>();
        }

        if (autoStart)
        {
            await StartFusionSession();
        }
    }

    /// <summary>
    /// Initialize MercuryMessaging network bridge and start Fusion session.
    /// </summary>
    public async System.Threading.Tasks.Task StartFusionSession()
    {
        // Configure MercuryMessaging network bridge
        var bridge = MmNetworkBridge.Instance;
        bridge.Configure(new Fusion2Backend(), new Fusion2Resolver());
        bridge.Initialize();

        Debug.Log("[T7_Fusion2Setup] MercuryMessaging bridge initialized");

        // Start Fusion session
        // Note: For scene management, add a NetworkSceneManagerDefault component
        // to your NetworkRunner GameObject, or use a custom INetworkSceneManager
        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            SessionName = sessionName
        });

        if (result.Ok)
        {
            Debug.Log($"[T7_Fusion2Setup] Fusion session started: {sessionName}");

            // Spawn the bridge object (host/server only)
            if (runner.IsServer || runner.IsSharedModeMasterClient)
            {
                SpawnBridge();
            }
        }
        else
        {
            Debug.LogError($"[T7_Fusion2Setup] Failed to start Fusion: {result.ShutdownReason}");
        }
    }

    void SpawnBridge()
    {
        if (bridgePrefab != null)
        {
            runner.Spawn(bridgePrefab);
            Debug.Log("[T7_Fusion2Setup] MmFusion2Bridge spawned");
        }
        else
        {
            Debug.LogWarning("[T7_Fusion2Setup] Bridge prefab not assigned!");
        }
    }

    /// <summary>
    /// Check if bridge is ready for network messages.
    /// </summary>
    public bool IsBridgeReady => MmFusion2Bridge.Instance != null;

    /// <summary>
    /// Wait for bridge to be ready.
    /// </summary>
    public IEnumerator WaitForBridge()
    {
        while (!IsBridgeReady)
        {
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("[T7_Fusion2Setup] Bridge is ready");
    }

    /// <summary>
    /// Log current network status.
    /// </summary>
    public void LogNetworkStatus()
    {
        var backend = MmNetworkBridge.Instance?.Backend;
        if (backend != null)
        {
            Debug.Log($"[T7_Fusion2Setup] Connected: {backend.IsConnected}");
            Debug.Log($"[T7_Fusion2Setup] Is Server: {backend.IsServer}");
            Debug.Log($"[T7_Fusion2Setup] Is Client: {backend.IsClient}");
            Debug.Log($"[T7_Fusion2Setup] Bridge Ready: {IsBridgeReady}");
        }
        else
        {
            Debug.LogWarning("[T7_Fusion2Setup] Backend not initialized");
        }
    }
#else
    void Awake()
    {
        Debug.LogWarning(
            "[T7_Fusion2Setup] Photon Fusion 2 not available.\n" +
            "To use this tutorial:\n" +
            "1. Download Fusion 2 SDK from Photon Dashboard\n" +
            "2. Import the Unity package\n" +
            "3. Add 'FUSION2_AVAILABLE' to Scripting Define Symbols\n" +
            "   (Edit > Project Settings > Player > Scripting Define Symbols)\n" +
            "4. Configure your Photon App ID"
        );
    }
#endif
}
