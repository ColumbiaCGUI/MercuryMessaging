// Copyright (c) 2017-2025, Columbia University
// Tutorial 4: Color Changing - Classic Pattern Example
// Demonstrates sending custom color messages with keyboard input.
//
// Keyboard Controls:
// - Press '1': Send Red to children (with Tag0), Blue to parents
// - Press '2': Send Green to all
// - Press '3': Send Blue to children, Red to parents

using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Examples.Tutorials;

/// <summary>
/// Tutorial 4 classic sphere handler demonstrating custom message sending.
/// Uses T4_ColorMessage to change colors of other objects in hierarchy.
/// </summary>
public class T4_SphereHandler : MmBaseResponder
{
    new void Update()
    {
        // Press '1': Red to children (with Tag0), Blue to parents
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // Send red to children with Tag0 filter
            GetRelayNode().MmInvoke(
                new T4_ColorMessage(
                    Color.red,
                    new MmMetadataBlock(MmTag.Tag0, MmLevelFilter.Child)));

            // Send blue to parents
            GetRelayNode().MmInvoke(
                new T4_ColorMessage(
                    Color.blue,
                    new MmMetadataBlock(MmLevelFilter.Parent)));

            Debug.Log("[T4] Key '1': Red to children (Tag0), Blue to parents");
        }
        // Press '2': Green to all
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GetRelayNode().MmInvoke(
                new T4_ColorMessage(
                    Color.green,
                    new MmMetadataBlock(MmLevelFilter.Child)));

            GetRelayNode().MmInvoke(
                new T4_ColorMessage(
                    Color.green,
                    new MmMetadataBlock(MmLevelFilter.Parent)));

            Debug.Log("[T4] Key '2': Green to all");
        }
        // Press '3': Blue to children, Red to parents
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GetRelayNode().MmInvoke(
                new T4_ColorMessage(
                    Color.blue,
                    new MmMetadataBlock(MmLevelFilter.Child)));

            GetRelayNode().MmInvoke(
                new T4_ColorMessage(
                    Color.red,
                    new MmMetadataBlock(MmLevelFilter.Parent)));

            Debug.Log("[T4] Key '3': Blue to children, Red to parents");
        }
    }
}
