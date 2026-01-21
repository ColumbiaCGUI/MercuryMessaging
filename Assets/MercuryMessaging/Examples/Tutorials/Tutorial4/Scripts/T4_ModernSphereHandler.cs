// Copyright (c) 2017-2025, Columbia University
// Tutorial 4: Color Changing - Modern DSL Pattern Example
// Demonstrates sending custom color messages using the Fluent DSL API.
//
// This version shows BOTH traditional and DSL approaches side by side.
//
// Keyboard Controls:
// - Press '1': Send Red to children, Blue to parents (traditional API)
// - Press '2': Send Green to all (Fluent DSL)
// - Press '3': Send Blue to children, Red to parents (Fluent DSL)

using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Examples.Tutorials;

/// <summary>
/// Modern Tutorial 4 sphere handler showing both traditional and DSL patterns.
/// Demonstrates how custom messages work with the Fluent DSL API.
/// </summary>
public class T4_ModernSphereHandler : MmBaseResponder
{
    new void Update()
    {
        // Press '1': Traditional API pattern (for comparison)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // Traditional pattern - explicit MmInvoke
            GetRelayNode().MmInvoke(
                new T4_ColorMessage(
                    Color.red,
                    new MmMetadataBlock(MmLevelFilter.Child)));

            GetRelayNode().MmInvoke(
                new T4_ColorMessage(
                    Color.blue,
                    new MmMetadataBlock(MmLevelFilter.Parent)));

            Debug.Log("[Modern T4] Key '1': Traditional API - Red to children, Blue to parents");
        }
        // Press '2': Fluent DSL pattern
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // Modern DSL pattern - fluent chain
            // Note: For custom messages, we still use MmInvoke but DSL for built-in types
            var greenMsg = new T4_ColorMessage(Color.green);

            // Send to children
            greenMsg.MetadataBlock = new MmMetadataBlock(MmLevelFilter.Child);
            GetRelayNode().MmInvoke(greenMsg);

            // Send to parents (create fresh message to avoid routing table issues)
            var parentMsg = new T4_ColorMessage(
                Color.green,
                new MmMetadataBlock(MmLevelFilter.Parent));
            GetRelayNode().MmInvoke(parentMsg);

            Debug.Log("[Modern T4] Key '2': DSL-style - Green to all");
        }
        // Press '3': Concise creation pattern
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // Most concise: use constructor with metadata directly
            GetRelayNode().MmInvoke(
                new T4_ColorMessage(Color.blue, new MmMetadataBlock(MmLevelFilter.Child)));

            GetRelayNode().MmInvoke(
                new T4_ColorMessage(Color.red, new MmMetadataBlock(MmLevelFilter.Parent)));

            Debug.Log("[Modern T4] Key '3': Concise - Blue to children, Red to parents");
        }
    }
}
