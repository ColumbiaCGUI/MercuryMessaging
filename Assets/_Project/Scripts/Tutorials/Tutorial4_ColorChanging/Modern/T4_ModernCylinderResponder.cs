// Tutorial 4: Color Changing - Modern Pattern Example
// Demonstrates MmExtendableResponder with registration-based custom method handling
//
// This is the MODERN pattern that eliminates boilerplate switch statements
// and prevents common errors like forgetting to call base.MmInvoke().

using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Modern Tutorial 4 cylinder responder using MmExtendableResponder.
/// Receives color change messages and updates the material color.
///
/// Key improvements over legacy pattern:
/// - No switch statement boilerplate
/// - Can't forget base.MmInvoke() call
/// - Clearer separation of concerns
/// - Handler registration happens in Awake()
/// </summary>
public class T4_ModernCylinderResponder : MmExtendableResponder
{
    /// <summary>
    /// Register custom method handlers during Awake.
    /// This is called before Start() and before any messages are received.
    /// </summary>
    protected override void Awake()
    {
        base.Awake(); // IMPORTANT: Always call base.Awake()

        // Register handler for UpdateColor custom method
        // Method ID 1000 (>= 1000 for custom methods)
        RegisterCustomHandler((MmMethod)T4_ModernMethods.UpdateColor, OnUpdateColor);
    }

    /// <summary>
    /// Handler for color update messages.
    /// This method is called automatically when a message with method UpdateColor is received.
    /// </summary>
    /// <param name="message">The received message (cast to T4_ColorMessage to access color data)</param>
    private void OnUpdateColor(MmMessage message)
    {
        // Cast to specific message type
        var colorMessage = (T4_ColorMessage)message;

        // Apply the color change
        ChangeColor(colorMessage.value);

        // Optional: Log for debugging
        Debug.Log($"[Modern] {gameObject.name} received color update: {colorMessage.value}");
    }

    /// <summary>
    /// Changes the cylinder's material color.
    /// </summary>
    /// <param name="col">The new color to apply</param>
    public void ChangeColor(Color col)
    {
        GetComponent<MeshRenderer>().material.color = col;
    }
}
