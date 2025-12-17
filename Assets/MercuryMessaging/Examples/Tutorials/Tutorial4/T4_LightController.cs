using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Tutorial 4: Controller that sends custom ColorIntensityMessage.
///
/// Hierarchy Setup:
/// T4_LightManager (MmRelayNode + T4_LightController)
///   └── Lights (MmRelayNode)
///         ├── Light1 (MmRelayNode + T4_LightResponder + Light)
///         ├── Light2 (MmRelayNode + T4_LightResponder + Light)
///         └── Light3 (MmRelayNode + T4_LightResponder + Light)
///
/// Keyboard Controls:
/// R - Set lights to red (intensity 2.0)
/// G - Set lights to green (intensity 1.5)
/// B - Set lights to blue (intensity 1.0)
/// W - Set lights to white (intensity 3.0)
/// 0 - Turn off lights (intensity 0)
/// I - Initialize all lights
/// </summary>
public class T4_LightController : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
        relay.BroadcastInitialize();
    }

    void Update()
    {
        // R - Red lights
        if (Input.GetKeyDown(KeyCode.R))
        {
            SetLightColor(Color.red, 2.0f);
        }

        // G - Green lights
        if (Input.GetKeyDown(KeyCode.G))
        {
            SetLightColor(Color.green, 1.5f);
        }

        // B - Blue lights
        if (Input.GetKeyDown(KeyCode.B))
        {
            SetLightColor(Color.blue, 1.0f);
        }

        // W - White lights
        if (Input.GetKeyDown(KeyCode.W))
        {
            SetLightColor(Color.white, 3.0f);
        }

        // 0 - Turn off
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SetLightColor(Color.black, 0f);
        }

        // I - Initialize
        if (Input.GetKeyDown(KeyCode.I))
        {
            relay.BroadcastInitialize();
            Debug.Log("[LightController] Re-initialized all lights");
        }
    }

    /// <summary>
    /// Sends a custom ColorIntensityMessage to all descendants.
    /// </summary>
    public void SetLightColor(Color color, float intensity)
    {
        // Create custom message with payload
        var message = new T4_ColorIntensityMessage(color, intensity)
        {
            // Set routing metadata
            MetadataBlock = new MmMetadataBlock(MmLevelFilter.Child)
        };

        // Send via relay node
        relay.MmInvoke(message);

        Debug.Log($"[LightController] Set color to {color} with intensity {intensity}");
    }

    /// <summary>
    /// Alternative: Use traditional MmInvoke with default metadata.
    /// </summary>
    public void SetLightColorSimple(Color color, float intensity)
    {
        var message = new T4_ColorIntensityMessage(color, intensity);
        relay.MmInvoke(message);
    }
}
