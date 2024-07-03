using UnityEditor;
using UnityEngine;

using MercuryMessaging;
using NewGraph;

[InitializeOnLoad]
public static class GameObjectCreationListener
{
    // public GraphWindow graphWindow;

    static GameObjectCreationListener()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private static void OnHierarchyChanged()
    {
        // This method is called whenever the hierarchy changes, including GameObject creation
        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<MmRelayNode>() == null)
        {
            // Check if the selected GameObject is newly created and does not already have the component
            MmRelayNode existingComponent = Selection.activeGameObject.GetComponent<MmRelayNode>();
            if (existingComponent == null)
            {
                // Add your component here if needed
                Selection.activeGameObject.AddComponent<MmRelayNode>();
                
                // Open the Graph Window
                // GraphWindowEditor.ShowWindow();
                GraphWindow.OpenWindow();
            }
        }
    }
}
