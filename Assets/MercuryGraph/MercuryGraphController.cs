using System;
using System.Collections;
using System.Collections.Generic;
using GraphViewBase;
using NewGraph;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

public class MercuryGraphController : MonoBehaviour
{
    public static MercuryGraphController instance;

    [Header("Graph Settings and References (DO change these.)")]
    public bool automaticGraphRendering = true;
    public ScriptableGraphModel graphModel;

    [Header("Graph Controller (DON'T change these!)")]
    public GraphWindow graphWindow;
    public GraphController graphController;

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        ScriptableInspectorController.OnRefreshButtonClicked += (graphWindow) => OnRefreshButtonClicked();
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        EditorApplication.projectChanged += OnProjectChanged;
        // EditorApplication.update += OnEditorUpdate;
        Undo.postprocessModifications += ctx => OnPostProcessModifications(ctx);
        Undo.undoRedoPerformed += OnUndoRedoPerformed;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        graphWindow = GraphWindow.OpenWindow();
        graphController = graphWindow.graphController;
        RenderGraph();
    }

    void OnDisable()
    {
        ClearGraph();
    }

    private static void OnRefreshButtonClicked()
    {
        Debug.Log("MercuryGraphController: Refresh Button clicked!");
        if (instance == null) return;
        // instance.ClearGraph();
        instance.RenderGraph();
    }
    private static void OnHierarchyChanged()
    {
        Debug.Log("MercuryGraphController: OnHierarchyChanged");
        if (instance == null) return;
        if (!instance.automaticGraphRendering) return;
        // instance.ClearGraph();
        instance.RenderGraph();
    }
    private static void OnProjectChanged()
    {
        Debug.Log("MercuryGraphController: OnProjectChanged");
        if (instance == null) return;
        if (!instance.automaticGraphRendering) return;
        // instance.ClearGraph();
        instance.RenderGraph();
    }
    private static void OnEditorUpdate()
    {
        Debug.Log("MercuryGraphController: OnEditorUpdate");
        if (instance != null) return;
        if (!instance.automaticGraphRendering) return;
        if (EditorApplication.isCompiling)
        {
            // instance.ClearGraph();
            instance.RenderGraph();
        }
    }
    private static UndoPropertyModification[] OnPostProcessModifications(UndoPropertyModification[] modifications)
    {
        if (instance == null) return modifications;
        if (!instance.automaticGraphRendering) return modifications;

        bool graphableModified = false;
        foreach (var modificaton in modifications)
        {
            // Debug.Log("Post process modification: " + modificaton.currentValue.target);
            if (modificaton.currentValue.target is MercuryGraphable)
            {
                graphableModified = true;
                break;
            }
        }
        if (graphableModified)
        {
            // instance.ClearGraph();
            instance.RenderGraph();
            Debug.Log("MercuryGraphController: OnPostProcessModifications");
        }
        return modifications;
    }
    private static void OnUndoRedoPerformed()
    {
        Debug.Log("MercuryGraphController: OnUndoRedoPerformed");
        if (instance == null) return;
        if (!instance.automaticGraphRendering) return;
        // instance.ClearGraph();
        instance.RenderGraph();
    }

    public void RenderGraph()
    {
        instance.ClearGraph();

        // Get all graphables in scene
        List<MercuryGraphable> graphables = GetMercuryGraphables();
        Debug.Log(graphables.Count + " graphables found in Hierarchy.");

        // Create nodes for each graphable (Line them up for now)
        // Save the nodeView objects created
        List<NodeView> nodeViews = new List<NodeView>();
        // Vector2 position = new Vector2(0, 0);
        foreach (MercuryGraphable graphable in graphables)
        {
            Debug.Log("Creating " + graphable.gameObject.name);

            // Create a new node
            Type nodeType = typeof(MercuryGraphNode);

            if (PlayerPrefs.HasKey(graphable.gameObject.name + "_nodePosition_x"))
            {
                graphable.nodePosition.x = PlayerPrefs.GetFloat(graphable.gameObject.name + "_nodePosition_x");
                graphable.nodePosition.y = PlayerPrefs.GetFloat(graphable.gameObject.name + "_nodePosition_y");
                Debug.Log("Loaded position of " + graphable.gameObject.name + " at " + graphable.nodePosition);
            }
            NodeController nodeController = CreateNewNode(graphable.nodePosition, graphable, nodeType);
            graphable.nodeController = nodeController;

            // Move to next position
            // position.x -= 200;
            // position.y -= 200;
        }

        // Link the nodes
        foreach (MercuryGraphable graphable in graphables)
        {
            MercuryGraphNode mercuryGraphNode = graphable.nodeController.nodeItem.nodeData as MercuryGraphNode;
            foreach (MercuryGraphable mercurySignalListener in graphable.mercurySignalListeners)
            {
                Debug.Log("Linking " + graphable.gameObject.name + " to " + mercurySignalListener.gameObject.name);
                MercuryGraphNode mercurySignalListenerNode = mercurySignalListener.nodeController.nodeItem.nodeData as MercuryGraphNode;
                mercuryGraphNode.mercuryOutputs.Add(mercurySignalListenerNode);
            }
        }

        // Reload the graph
        // graphController.graphView.Unbind();
        graphController.Reload();
        SaveNodePositions();
        Debug.Log("Graph rendered.");
    }
    public void ClearGraph()
    {
        SaveNodePositions();

        Debug.Log("Clearing graph...");
        graphController.inspector.Clear();
        graphController.graphView.ClearView();
        graphController.dataToViewLookup.Clear();
        graphController.inspector.SetSelectedNodeInfoActive(active: false);

        UnityEngine.Object graphData = ScriptableObject.CreateInstance<ScriptableGraphModel>();
        IGraphModelData graphModel = graphData as IGraphModelData;
        graphModel.CreateSerializedObject();
        graphController.graphData = graphModel;
        if (graphController.graphData.SerializedGraphData == null)
        {
            graphController.graphData.CreateSerializedObject();
        }

        int counter = 0;
        graphController.ForEachNode((node) =>
        {
            graphController.graphView.SetSelected(node as NodeView, true, false);
            counter++;
        });
        graphController.OnDelete();
        Debug.Log("Deleted " + counter + " nodes.");
    }
    public void SaveNodePositions()
    {
        // Save positions of the graph nodes
        List<MercuryGraphable> graphables = GetMercuryGraphables();
        foreach (MercuryGraphable graphable in graphables)
        {
            if (graphable.nodeController != null)
            {
                graphable.nodePosition = graphable.nodeController.nodeItem.GetPosition();
                PlayerPrefs.SetFloat(graphable.gameObject.name + "_nodePosition_x", graphable.nodePosition.x);
                PlayerPrefs.SetFloat(graphable.gameObject.name + "_nodePosition_y", graphable.nodePosition.y);
                Debug.Log("Saved position of " + graphable.gameObject.name + " at " + graphable.nodePosition);
            }
        }
    }

    public List<MercuryGraphable> GetMercuryGraphables()
    {
        MercuryGraphable[] graphableArray = FindObjectsOfType<MercuryGraphable>();
        List<MercuryGraphable> graphables = new List<MercuryGraphable>(graphableArray);
        return graphables;
    }

    // Modified from GraphController.CreateNewNode()
    public NodeController CreateNewNode(Vector2 position, MercuryGraphable graphable, Type nodeType)
    {
        // Create a node and add it to the graph
        INode node = Activator.CreateInstance(nodeType) as INode;
        NodeModel nodeModel = graphController.graphData.AddNode(node, false);
        if (node is MercuryGraphNode mercuryGraphNode)
        {
            mercuryGraphNode.mercuryGraphable = graphable;
            mercuryGraphNode.testToggle = graphable.testToggle;
        }

        // Add its data
        nodeModel.SetData(graphController.graphData.GetLastAddedNodeProperty(false));
        // Debug.Log(graphable.gameObject.name);
        if (graphable != null)
        {
            nodeModel.SetName(graphable.gameObject.name);
        }

        // Create a node controller and view
        NodeController nodeController = new NodeController(nodeModel, graphController, position);
        graphController.graphView.AddElement(nodeController.nodeView);
        nodeController.Initialize();

        // Select the new node
        graphController.graphView.SetSelected(nodeController.nodeView);

        // Store the data for data to node lookup later..?
        // graphController.dataToViewLookup.Add(node, nodeController.nodeView);
        return nodeController;
    }
}
