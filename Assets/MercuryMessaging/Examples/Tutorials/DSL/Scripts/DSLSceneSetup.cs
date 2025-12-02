// Copyright (c) 2017-2025, Columbia University
// DSL Tutorial: Scene Setup
// Creates the tutorial scene hierarchy at runtime

using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Automatically creates the DSL tutorial scene hierarchy.
///
/// Hierarchy created:
/// - DSLTutorial (this object + MmRelayNode + demo scripts)
///   - Child1 (Cube + ColorResponder)
///   - Child2 (Sphere + ColorResponder)
///   - Child3 (Capsule + ColorResponder)
///   - ChildGroup (MmRelayNode)
///     - Grandchild1 (Cube + ColorResponder)
///     - Grandchild2 (Cube + ColorResponder)
///
/// Add this script to an empty GameObject and press Play.
/// </summary>
public class DSLSceneSetup : MonoBehaviour
{
    [Header("Scene Setup")]
    [Tooltip("Automatically create tutorial hierarchy on Start")]
    public bool createOnStart = true;

    [Header("Visual Settings")]
    public float spacing = 2.5f;
    public float childScale = 0.8f;
    public float grandchildScale = 0.5f;

    void Start()
    {
        if (createOnStart)
        {
            SetupScene();
        }
    }

    [ContextMenu("Setup DSL Tutorial Scene")]
    public void SetupScene()
    {
        Debug.Log("[DSL Scene Setup] Creating tutorial hierarchy...");

        // Ensure this object has MmRelayNode
        var rootRelay = GetComponent<MmRelayNode>();
        if (rootRelay == null)
        {
            rootRelay = gameObject.AddComponent<MmRelayNode>();
            Debug.Log("Added MmRelayNode to root");
        }

        // Ensure demo scripts are attached
        if (GetComponent<DSLBasicDemo>() == null)
        {
            gameObject.AddComponent<DSLBasicDemo>();
            Debug.Log("Added DSLBasicDemo script");
        }
        if (GetComponent<DSLTemporalDemo>() == null)
        {
            gameObject.AddComponent<DSLTemporalDemo>();
            Debug.Log("Added DSLTemporalDemo script");
        }

        // Create direct children
        CreateColorChild("Child1_Cube", PrimitiveType.Cube, new Vector3(-spacing, 0, 0), Color.red);
        CreateColorChild("Child2_Sphere", PrimitiveType.Sphere, new Vector3(0, 0, 0), Color.green);
        CreateColorChild("Child3_Capsule", PrimitiveType.Capsule, new Vector3(spacing, 0, 0), Color.blue);

        // Create a child group with grandchildren
        var childGroup = CreateChildGroup();

        // Refresh the relay node to pick up new children
        rootRelay.MmRefreshResponders();

        Debug.Log("[DSL Scene Setup] Tutorial scene created successfully!");
        Debug.Log("Press 1-5 for basic demos, T/Y/U for temporal demos");
    }

    GameObject CreateColorChild(string name, PrimitiveType primitiveType, Vector3 position, Color color)
    {
        // Check if already exists
        var existing = transform.Find(name);
        if (existing != null)
        {
            Debug.Log("Child '" + name + "' already exists, skipping");
            return existing.gameObject;
        }

        // Create primitive
        var child = GameObject.CreatePrimitive(primitiveType);
        child.name = name;
        child.transform.SetParent(transform);
        child.transform.localPosition = position;
        child.transform.localScale = Vector3.one * childScale;

        // Set color
        var renderer = child.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = color;
        }

        // Add MercuryMessaging components
        child.AddComponent<MmRelayNode>();
        child.AddComponent<ColorResponder>();

        Debug.Log("Created child: " + name);
        return child;
    }

    GameObject CreateChildGroup()
    {
        var groupName = "ChildGroup";

        // Check if already exists
        var existing = transform.Find(groupName);
        if (existing != null)
        {
            Debug.Log("Child group already exists, skipping");
            return existing.gameObject;
        }

        // Create group container
        var group = new GameObject(groupName);
        group.transform.SetParent(transform);
        group.transform.localPosition = new Vector3(0, -spacing, 0);

        // Add relay node for group routing
        var groupRelay = group.AddComponent<MmRelayNode>();

        // Create grandchildren
        for (int i = 0; i < 3; i++)
        {
            var grandchild = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grandchild.name = "Grandchild" + (i + 1);
            grandchild.transform.SetParent(group.transform);
            grandchild.transform.localPosition = new Vector3((i - 1) * spacing * 0.6f, 0, 0);
            grandchild.transform.localScale = Vector3.one * grandchildScale;

            // Set color
            var renderer = grandchild.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                float hue = (float)i / 3f;
                renderer.material.color = Color.HSVToRGB(hue, 0.5f, 0.9f);
            }

            // Add components
            grandchild.AddComponent<MmRelayNode>();
            grandchild.AddComponent<ColorResponder>();

            Debug.Log("Created grandchild: " + grandchild.name);
        }

        // Refresh group relay
        groupRelay.MmRefreshResponders();

        Debug.Log("Created child group with grandchildren");
        return group;
    }

    [ContextMenu("Clear Tutorial Scene")]
    public void ClearScene()
    {
        // Destroy all children
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        Debug.Log("[DSL Scene Setup] Tutorial scene cleared");
    }
}
