using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Tutorial 4: Responder that handles custom ColorIntensityMessage.
/// Uses MmExtendableResponder for clean handler registration.
///
/// Attach to GameObjects with a Light component to control lighting.
/// Can also update Renderer materials if available.
/// </summary>
public class T4_LightResponder : MmExtendableResponder
{
    [Header("Target Components")]
    [Tooltip("Light component to control (auto-found if not set)")]
    [SerializeField] private Light targetLight;

    [Tooltip("Optional renderer for material color")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Default Settings")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private float defaultIntensity = 1.0f;

    public override void Awake()
    {
        base.Awake();

        // Auto-find Light component if not assigned
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }

        // Auto-find Renderer if not assigned
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        // Register custom handler for ColorIntensityMessage
        RegisterCustomHandler((MmMethod)T3_MyMethods.ChangeColor, OnColorIntensity);
    }

    /// <summary>
    /// Handler for custom ColorIntensityMessage.
    /// </summary>
    private void OnColorIntensity(MmMessage message)
    {
        var msg = (T4_ColorIntensityMessage)message;

        // Update Light component
        if (targetLight != null)
        {
            targetLight.color = msg.color;
            targetLight.intensity = msg.intensity;
        }

        // Update Renderer material (if available)
        if (targetRenderer != null)
        {
            // Apply color with intensity multiplier
            targetRenderer.material.color = msg.color * msg.intensity;
        }

        Debug.Log($"[{gameObject.name}] Color={msg.color}, Intensity={msg.intensity}");
    }

    /// <summary>
    /// Initialize to default values.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        if (targetLight != null)
        {
            targetLight.color = defaultColor;
            targetLight.intensity = defaultIntensity;
        }

        if (targetRenderer != null)
        {
            targetRenderer.material.color = defaultColor * defaultIntensity;
        }

        Debug.Log($"[{gameObject.name}] Initialized with default color/intensity");
    }

    /// <summary>
    /// SetActive controls whether the light is enabled.
    /// </summary>
    public override void SetActive(bool active)
    {
        base.SetActive(active);

        if (targetLight != null)
        {
            targetLight.enabled = active;
        }

        Debug.Log($"[{gameObject.name}] Light {(active ? "enabled" : "disabled")}");
    }
}
