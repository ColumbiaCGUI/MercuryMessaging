// Copyright (c) 2017-2025, Columbia University
// Tutorial 4: Color Changing - Modern Pattern Example
// Demonstrates MmExtendableResponder with registration-based custom method handling.
//
// This is the MODERN pattern that eliminates boilerplate switch statements
// and prevents common errors like forgetting to call base.MmInvoke().
//
// Key improvements over the classic pattern (T4_CylinderResponder):
// - No switch statement boilerplate
// - Can't forget base.MmInvoke() call (MmExtendableResponder handles it)
// - Clearer separation of concerns
// - Handler registration happens in Awake()

using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Examples.Tutorials;

/// <summary>
/// Modern Tutorial 4 cylinder responder using MmExtendableResponder.
/// Receives T4_ColorMessage and updates the material color.
/// </summary>
public class T4_ModernCylinderResponder : MmExtendableResponder
{
    /// <summary>
    /// Register custom method handlers during Awake.
    /// This is called before Start() and before any messages are received.
    /// </summary>
    public override void Awake()
    {
        base.Awake(); // IMPORTANT: Always call base.Awake()

        // Register handler for ChangeColor custom method (ID 1000)
        RegisterCustomHandler((MmMethod)T4_Methods.ChangeColor, OnColorChange);
    }

    /// <summary>
    /// Handler for color change messages.
    /// This method is called automatically when a T4_ColorMessage is received.
    /// </summary>
    /// <param name="message">The received message (cast to T4_ColorMessage to access color data)</param>
    private void OnColorChange(MmMessage message)
    {
        // Cast to specific message type to access color payload
        var colorMessage = (T4_ColorMessage)message;

        // Apply the color change
        ChangeColor(colorMessage.value);

        Debug.Log($"[Modern T4] {gameObject.name} received color: {colorMessage.value}");
    }

    /// <summary>
    /// Changes the cylinder's material color.
    /// </summary>
    /// <param name="col">The new color to apply</param>
    public void ChangeColor(Color col)
    {
        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = col;
        }
    }
}
