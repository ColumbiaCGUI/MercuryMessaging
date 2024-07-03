using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GraphWindowEditor : EditorWindow
{
    [MenuItem("Window/GraphWindow")]
    public static void ShowWindow()
    {
        GetWindow<GraphWindowEditor>("GraphWindow");
    }

    private void OnGUI()
    {
        GUILayout.Label("This is a graph window", EditorStyles.boldLabel);
    }
}
