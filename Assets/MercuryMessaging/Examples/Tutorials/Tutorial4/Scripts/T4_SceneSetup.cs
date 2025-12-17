// Copyright (c) 2017-2025, Columbia University
// Tutorial 4: Custom Messages - Scene Setup
// Creates the tutorial scene hierarchy at runtime.
//
// Usage: Add this script to an empty GameObject and press Play.
// Keyboard Controls:
// - Press '1': Red to children, Blue to parents
// - Press '2': Green to all
// - Press '3': Blue to children, Red to parents

using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Examples.Tutorials;

/// <summary>
/// Automatically creates the Tutorial 4 (Custom Messages) scene hierarchy.
///
/// Hierarchy created:
/// - Tutorial4Root (this object + MmRelayNode + T4_SphereHandler)
///   - ParentCylinder (MmRelayNode + T4_CylinderResponder) - receives blue from children
///   - ChildCylinder1 (MmRelayNode + T4_CylinderResponder) - receives red from parent
///   - ChildCylinder2 (MmRelayNode + T4_CylinderResponder + Tag0) - receives tagged messages
///
/// Press Play, then press 1/2/3 to send color messages.
/// </summary>
public class T4_SceneSetup : MonoBehaviour
{
    [Header("Scene Setup")]
    [Tooltip("Automatically create tutorial hierarchy on Start")]
    public bool createOnStart = true;

    [Tooltip("Use modern handler pattern (MmExtendableResponder)")]
    public bool useModernPattern = true;

    [Header("Visual Settings")]
    public float spacing = 2.5f;
    public float cylinderScale = 0.8f;

    void Start()
    {
        if (createOnStart)
        {
            SetupScene();
        }
    }

    [ContextMenu("Setup Tutorial 4 Scene")]
    public void SetupScene()
    {
        Debug.Log("[T4 Scene Setup] Creating Custom Messages tutorial hierarchy...");

        // Ensure root has MmRelayNode and handler
        var rootRelay = GetComponent<MmRelayNode>();
        if (rootRelay == null)
        {
            rootRelay = gameObject.AddComponent<MmRelayNode>();
            Debug.Log("Added MmRelayNode to root");
        }

        // Add handler (sends color messages on keyboard input)
        if (useModernPattern)
        {
            if (GetComponent<T4_ModernSphereHandler>() == null)
            {
                gameObject.AddComponent<T4_ModernSphereHandler>();
                Debug.Log("Added T4_ModernSphereHandler to root");
            }
        }
        else
        {
            if (GetComponent<T4_SphereHandler>() == null)
            {
                gameObject.AddComponent<T4_SphereHandler>();
                Debug.Log("Added T4_SphereHandler to root");
            }
        }

        // Create parent cylinder (above root)
        CreateCylinder("ParentCylinder", new Vector3(0, spacing, 0), Color.gray, MmTagHelper.Everything, true);

        // Create child cylinders (below root)
        CreateCylinder("ChildCylinder1", new Vector3(-spacing, -spacing, 0), Color.white, MmTagHelper.Everything, false);
        CreateCylinder("ChildCylinder2_Tag0", new Vector3(spacing, -spacing, 0), Color.white, MmTag.Tag0, false);

        // Refresh responders after creating hierarchy
        rootRelay.MmRefreshResponders();

        Debug.Log("[T4 Scene Setup] Tutorial scene created successfully!");
        Debug.Log("Press 1/2/3 to send color messages:");
        Debug.Log("  1 = Red to children (Tag0 only), Blue to parents");
        Debug.Log("  2 = Green to all");
        Debug.Log("  3 = Blue to children, Red to parents");
    }

    GameObject CreateCylinder(string name, Vector3 position, Color color, MmTag tag, bool isParent)
    {
        // Check if already exists
        var existing = isParent ? transform.parent?.Find(name) : transform.Find(name);
        if (existing != null)
        {
            Debug.Log($"Cylinder '{name}' already exists, skipping");
            return existing.gameObject;
        }

        // Create cylinder primitive
        var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = name;
        cylinder.transform.localPosition = position;
        cylinder.transform.localScale = Vector3.one * cylinderScale;

        // Set up parent/child relationship
        if (isParent)
        {
            // This cylinder is a parent - put root under it
            if (transform.parent == null)
            {
                // Create parent relationship
                cylinder.transform.position = transform.position + position;
                transform.SetParent(cylinder.transform);
                transform.localPosition = Vector3.zero;
            }
        }
        else
        {
            // This cylinder is a child
            cylinder.transform.SetParent(transform);
            cylinder.transform.localPosition = position;
        }

        // Set initial color
        var renderer = cylinder.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = color;
        }

        // Add MercuryMessaging components
        var relay = cylinder.AddComponent<MmRelayNode>();

        // Add responder based on pattern choice
        if (useModernPattern)
        {
            var responder = cylinder.AddComponent<T4_ModernCylinderResponder>();
            responder.Tag = tag;
            responder.TagCheckEnabled = (tag != MmTagHelper.Everything);
        }
        else
        {
            var responder = cylinder.AddComponent<T4_CylinderResponder>();
            responder.Tag = tag;
            responder.TagCheckEnabled = (tag != MmTagHelper.Everything);
        }

        // Set up routing table entries
        if (!isParent)
        {
            // Child cylinders: register with root
            var rootRelay = GetComponent<MmRelayNode>();
            if (rootRelay != null)
            {
                rootRelay.MmAddToRoutingTable(relay, MmLevelFilter.Child);
                relay.AddParent(rootRelay);
            }
        }
        else
        {
            // Parent cylinder: register root as child
            var rootRelay = GetComponent<MmRelayNode>();
            if (rootRelay != null)
            {
                relay.MmAddToRoutingTable(rootRelay, MmLevelFilter.Child);
                rootRelay.AddParent(relay);
            }
        }

        Debug.Log($"Created cylinder: {name} (Tag: {tag})");
        return cylinder;
    }

    [ContextMenu("Clear Tutorial Scene")]
    public void ClearScene()
    {
        // Destroy all children
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Unparent from any parent cylinder
        if (transform.parent != null)
        {
            var parent = transform.parent.gameObject;
            transform.SetParent(null);
            DestroyImmediate(parent);
        }

        Debug.Log("[T4 Scene Setup] Tutorial scene cleared");
    }
}
