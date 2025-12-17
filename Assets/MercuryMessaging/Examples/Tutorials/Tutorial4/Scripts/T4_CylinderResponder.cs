// Copyright (c) 2017-2025, Columbia University
// Tutorial 4: Color Changing - Classic Responder Pattern
// Demonstrates receiving custom messages using MmInvoke override.
//
// This is the CLASSIC pattern using switch statement in MmInvoke().
// For the modern pattern, see T4_ModernCylinderResponder which uses
// MmExtendableResponder for cleaner handler registration.

using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Examples.Tutorials;

/// <summary>
/// Tutorial 4 classic cylinder responder demonstrating custom message handling.
/// Receives T4_ColorMessage and changes the cylinder's color.
/// </summary>
public class T4_CylinderResponder : MmBaseResponder
{
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
            Debug.Log($"[T4] {gameObject.name} color changed to: {col}");
        }
    }

    /// <summary>
    /// Override MmInvoke to handle custom message types.
    /// This is the CLASSIC pattern - see T4_ModernCylinderResponder for modern approach.
    /// </summary>
    public override void MmInvoke(MmMessage message)
    {
        // Check for our custom method
        if (message.MmMethod == (MmMethod)T4_Methods.ChangeColor)
        {
            // Cast to our specific message type
            var colorMessage = (T4_ColorMessage)message;
            ChangeColor(colorMessage.value);
        }
        else
        {
            // IMPORTANT: Always call base for unhandled methods!
            base.MmInvoke(message);
        }
    }
}
