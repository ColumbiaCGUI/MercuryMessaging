// Copyright (c) 2017-2025, Columbia University
// DSL Tutorial: Color Responder
// Simple responder that changes color based on received messages

using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Simple responder for DSL tutorials.
/// Changes the material color based on received messages.
///
/// Message handlers:
/// - MessageBool: Red (true) or Blue (false)
/// - MessageInt: Gradient from Red (0) to Green (100)
/// - MessageString: "red", "green", "blue", "yellow" colors
/// - SetActive: Shows visual feedback
/// </summary>
public class ColorResponder : MmBaseResponder
{
    private Material material;
    private Color originalColor;

    public override void Awake()
    {
        base.Awake();

        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
            originalColor = material.color;
        }
    }

    protected override void ReceivedMessage(MmMessageBool message)
    {
        if (material == null) return;

        // True = Red, False = Blue
        material.color = message.value ? Color.red : Color.blue;
        Debug.Log("[ColorResponder " + gameObject.name + "] Bool: " + message.value + " -> " + (message.value ? "Red" : "Blue"));
    }

    protected override void ReceivedMessage(MmMessageInt message)
    {
        if (material == null) return;

        // Gradient from Red (0) to Green (100)
        float t = Mathf.Clamp01(message.value / 100f);
        material.color = Color.Lerp(Color.red, Color.green, t);
        Debug.Log("[ColorResponder " + gameObject.name + "] Int: " + message.value + " -> Gradient");
    }

    protected override void ReceivedMessage(MmMessageFloat message)
    {
        if (material == null) return;

        // Use float as hue value (0-1)
        float hue = Mathf.Repeat(message.value, 1f);
        material.color = Color.HSVToRGB(hue, 0.8f, 0.9f);
        Debug.Log("[ColorResponder " + gameObject.name + "] Float: " + message.value + " -> Hue");
    }

    protected override void ReceivedMessage(MmMessageString message)
    {
        if (material == null) return;

        // Named colors
        switch (message.value.ToLower())
        {
            case "red":
                material.color = Color.red;
                break;
            case "green":
                material.color = Color.green;
                break;
            case "blue":
                material.color = Color.blue;
                break;
            case "yellow":
                material.color = Color.yellow;
                break;
            case "white":
                material.color = Color.white;
                break;
            case "reset":
                material.color = originalColor;
                break;
            default:
                Debug.Log("[ColorResponder " + gameObject.name + "] Unknown color: " + message.value);
                return;
        }

        Debug.Log("[ColorResponder " + gameObject.name + "] String: " + message.value);
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);
        if (material == null) return;

        // Flash effect for SetActive
        material.color = active ? Color.white : Color.gray;
        Debug.Log("[ColorResponder " + gameObject.name + "] SetActive: " + active);
    }

    public override void Initialize()
    {
        base.Initialize();
        if (material == null) return;

        material.color = originalColor;
        Debug.Log("[ColorResponder " + gameObject.name + "] Initialize - Reset to original color");
    }

    public override void Refresh(System.Collections.Generic.List<MmTransform> transformList)
    {
        base.Refresh(transformList);
        Debug.Log("[ColorResponder " + gameObject.name + "] Refresh received");
    }
}
