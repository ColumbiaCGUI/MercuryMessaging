// Tutorial 4: Color Changing - Modern Pattern Example
// Keyboard input handler that sends color change messages
//
// This is the MODERN pattern using corrected method IDs (>= 1000)

using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Modern method IDs for Tutorial 4 (>= 1000 as required by framework)
/// </summary>
public enum T4_ModernMethods
{
    UpdateColor = 1000  // Fixed from legacy value of 100
}

/// <summary>
/// Message type IDs for Tutorial 4
/// </summary>
public enum T4_ModernMsgTypes
{
    Color = 1100
}

/// <summary>
/// Modern Tutorial 4 sphere handler using corrected method IDs.
/// Sends color change messages based on keyboard input.
///
/// Keyboard Controls:
/// - Press '1': Send Red to children, Blue to parents
/// - Press '2': Send Green to all
/// - Press '3': Send Blue to children, Red to parents
/// </summary>
public class T4_ModernSphereHandler : MmBaseResponder
{
    void Update()
    {
        // Press '1' key: Red for children, Blue for parents
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // Send red color to children
            GetRelayNode().MmInvoke(
                new T4_ColorMessage(
                    new Color(1, 0, 0, 1),                               // Red
                    (MmMethod)T4_ModernMethods.UpdateColor,              // Method 1000
                    (MmMessageType)T4_ModernMsgTypes.Color,              // Message type
                    new MmMetadataBlock(MmLevelFilter.Child)));          // To children

            // Send blue color to parents
            GetRelayNode().MmInvoke(
                new T4_ColorMessage(
                    new Color(0, 0, 1, 1),                               // Blue
                    (MmMethod)T4_ModernMethods.UpdateColor,
                    (MmMessageType)T4_ModernMsgTypes.Color,
                    new MmMetadataBlock(MmLevelFilter.Parent)));         // To parents

            Debug.Log("[Modern] Key '1' pressed: Red to children, Blue to parents");
        }
        // Press '2' key: Green for all
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // Send green to children
            GetRelayNode().MmInvoke(
                new T4_ColorMessage(
                    new Color(0, 1, 0, 1),                               // Green
                    (MmMethod)T4_ModernMethods.UpdateColor,
                    (MmMessageType)T4_ModernMsgTypes.Color,
                    new MmMetadataBlock(MmLevelFilter.Child)));

            // Send green to parents
            GetRelayNode().MmInvoke(
                new T4_ColorMessage(
                    new Color(0, 1, 0, 1),                               // Green
                    (MmMethod)T4_ModernMethods.UpdateColor,
                    (MmMessageType)T4_ModernMsgTypes.Color,
                    new MmMetadataBlock(MmLevelFilter.Parent)));

            Debug.Log("[Modern] Key '2' pressed: Green to all");
        }
        // Press '3' key: Blue for children, Red for parents
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // Send blue to children
            GetRelayNode().MmInvoke(
                new T4_ColorMessage(
                    new Color(0, 0, 1, 1),                               // Blue
                    (MmMethod)T4_ModernMethods.UpdateColor,
                    (MmMessageType)T4_ModernMsgTypes.Color,
                    new MmMetadataBlock(MmLevelFilter.Child)));

            // Send red to parents
            GetRelayNode().MmInvoke(
                new T4_ColorMessage(
                    new Color(1, 0, 0, 1),                               // Red
                    (MmMethod)T4_ModernMethods.UpdateColor,
                    (MmMessageType)T4_ModernMsgTypes.Color,
                    new MmMetadataBlock(MmLevelFilter.Parent)));

            Debug.Log("[Modern] Key '3' pressed: Blue to children, Red to parents");
        }
    }
}
