using UnityEngine;

#if FISHNET_AVAILABLE
using MercuryMessaging;
using MercuryMessaging.Network;
using MercuryMessaging.Network.Backends;
#endif

/// <summary>
/// Tutorial 6: Network bridge setup for FishNet.
/// Initializes MmNetworkBridge with FishNetBackend.
///
/// Prerequisites:
/// 1. FishNet installed via Package Manager
/// 2. FISHNET_AVAILABLE added to Scripting Define Symbols
/// 3. FishNet NetworkManager in scene
///
/// Usage: Add this component to your NetworkManager GameObject.
/// </summary>
public class T6_NetworkSetup : MonoBehaviour
{
#if FISHNET_AVAILABLE
    [Header("Configuration")]
    [Tooltip("Auto-initialize on Awake")]
    public bool initializeOnAwake = true;

    void Awake()
    {
        if (initializeOnAwake)
        {
            InitializeNetworkBridge();
        }
    }

    /// <summary>
    /// Initialize the MmNetworkBridge with FishNet backend.
    /// Call this manually if initializeOnAwake is false.
    /// </summary>
    public void InitializeNetworkBridge()
    {
        var bridge = MmNetworkBridge.Instance;
        bridge.Configure(new FishNetBackend(), new FishNetResolver());
        bridge.Initialize();

        Debug.Log("[T6_NetworkSetup] MercuryMessaging network bridge initialized with FishNet");
    }

    /// <summary>
    /// Check current network status.
    /// </summary>
    public void LogNetworkStatus()
    {
        var backend = MmNetworkBridge.Instance.Backend;
        if (backend != null)
        {
            Debug.Log($"[T6_NetworkSetup] Connected: {backend.IsConnected}");
            Debug.Log($"[T6_NetworkSetup] Is Server: {backend.IsServer}");
            Debug.Log($"[T6_NetworkSetup] Is Client: {backend.IsClient}");
        }
        else
        {
            Debug.LogWarning("[T6_NetworkSetup] Backend not initialized");
        }
    }
#else
    void Awake()
    {
        Debug.LogWarning(
            "[T6_NetworkSetup] FishNet not available.\n" +
            "To use this tutorial:\n" +
            "1. Install FishNet via Package Manager\n" +
            "2. Add 'FISHNET_AVAILABLE' to Scripting Define Symbols\n" +
            "   (Edit > Project Settings > Player > Scripting Define Symbols)"
        );
    }
#endif
}
