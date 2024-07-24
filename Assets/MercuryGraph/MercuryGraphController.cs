using System;
using System.Collections;
using System.Collections.Generic;
using GraphViewBase;
using MercuryMessaging;
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
        ScriptableInspectorController.OnApplyButtonClicked += (graphWindow) => OnApplyButtonClicked();
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

    private static void OnApplyButtonClicked()
    {
        Debug.Log("MercuryGraphController: Apply Button clicked!");

        // If in play mode, we save the node positions and listeners
        if (Application.isPlaying)
        {
            if (instance == null) return;
            Debug.Log("MercuryGraphController: In Play Mode: Saving node positions and listeners.");
            instance.SaveNodePositions();
            instance.SaveMercuryGraphableListeners();
        }
        else
        {
            // If in edit mode, we change the listeners according to the saved changes
            Debug.Log("MercuryGraphController: In Edit Mode: Overwriting listeners.");
            OverwriteMercuryGraphableListeners();
        }
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
            if (modificaton.currentValue.target is MmRelayNodeGraphable)
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
        List<MmRelayNodeGraphable> graphables = GetMercuryGraphables();
        Debug.Log(graphables.Count + " graphables found in Hierarchy.");

        // Create nodes for each graphable (Line them up for now)
        // Save the nodeView objects created
        List<NodeView> nodeViews = new List<NodeView>();
        // Vector2 position = new Vector2(0, 0);
        foreach (MmRelayNodeGraphable graphable in graphables)
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
        foreach (MmRelayNodeGraphable graphable in graphables)
        {
            MercuryGraphNode mercuryGraphNode = graphable.nodeController.nodeItem.nodeData as MercuryGraphNode;
            // Debug.Log("MercuryGraphController: Calling GetMmMessageResponders for " + graphable.gameObject.name);
            List<MmRelayNodeGraphable> responders = GetMmMessageResponders(graphable);
            foreach (MmRelayNodeGraphable responder in responders)
            {
                Debug.Log("Linking " + graphable.gameObject.name + " to " + responder.gameObject.name);
                MercuryGraphNode mercurySignalListenerNode = responder.nodeController.nodeItem.nodeData as MercuryGraphNode;
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

    public static List<MmRelayNodeGraphable> GetMercuryGraphables()
    {
        List<MmRelayNodeGraphable> graphables = new List<MmRelayNodeGraphable>();
        if (Application.isPlaying)
        {
            MmRelayNodeGraphable[] graphableArray = FindObjectsOfType<MmRelayNodeGraphable>();
            graphables = new List<MmRelayNodeGraphable>(graphableArray);
        }
        else
        {
            GameObject[] rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                MmRelayNodeGraphable[] graphableArray = rootGameObject.GetComponentsInChildren<MmRelayNodeGraphable>();
                graphables.AddRange(graphableArray);
            }
        }
        return graphables;
    }
    public static List<MmRelayNodeGraphable> GetMmMessageResponders(MmRelayNodeGraphable graphable)
    {
        List<MmRelayNodeGraphable> responders = new List<MmRelayNodeGraphable>();
        List<MmRoutingTableItem> mmRoutingTableItems = graphable.RoutingTable.GetMmRoutingTableItems(
            MmRoutingTable.ListFilter.All,
            MmLevelFilterHelper.SelfAndChildren);
        // We only want the children (listeners) of the graphable
        List<MmRoutingTableItem> mmRoutingTableItemsChildren = new List<MmRoutingTableItem>();
        foreach (MmRoutingTableItem item in mmRoutingTableItems)
        {
            if (item.Level == MmLevelFilter.Child)
            {
                mmRoutingTableItemsChildren.Add(item);
            }
        }

        foreach (MmRoutingTableItem item in mmRoutingTableItemsChildren)
        {
            if (item.Responder is MmRelayNodeGraphable)
            {
                responders.Add((MmRelayNodeGraphable)item.Responder);
            }
        }
        Debug.Log("MercuryGraphController: Found " + responders.Count + " responders for " + graphable.gameObject.name);
        return responders;
    }

    // Modified from GraphController.CreateNewNode()
    public NodeController CreateNewNode(Vector2 position, MmRelayNodeGraphable graphable, Type nodeType)
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

    public void SaveNodePositions()
    {
        // Save positions of the graph nodes
        List<MmRelayNodeGraphable> graphables = GetMercuryGraphables();
        foreach (MmRelayNodeGraphable graphable in graphables)
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
    public void SaveMercuryGraphableListeners()
    {
        // Save the listeners of each graphable in Playerprefs
        List<MmRelayNodeGraphable> graphables = GetMercuryGraphables();
        foreach (MmRelayNodeGraphable graphable in graphables)
        {
            if (graphable.nodeController != null)
            {
                List<MmRelayNodeGraphable> listeners = GetMmMessageResponders(graphable);
                if (listeners.Count > 0)
                {
                    string listenerNames = "";
                    foreach (MmRelayNodeGraphable listener in listeners)
                    {
                        listenerNames += listener.gameObject.name + ",";
                    }
                    PlayerPrefs.SetString(graphable.gameObject.name + "_listeners", listenerNames);
                    Debug.Log("Saved listeners of " + graphable.gameObject.name + ": " + listenerNames);
                }
            }
        }
    }
    public static void OverwriteMercuryGraphableListeners()
    {
        // Overwrite the listeners of each graphable
        List<MmRelayNodeGraphable> graphables = GetMercuryGraphables();
        Debug.Log("Overwriting Listeners of " + graphables.Count + " graphables.");
        foreach (MmRelayNodeGraphable graphable in graphables)
        {
            graphable.RoutingTable.Clear();
            string listenerNames = PlayerPrefs.GetString(graphable.gameObject.name + "_listeners");
            if (listenerNames != "")
            {
                string[] listenerNameArray = listenerNames.Split(',');
                List<MmRelayNodeGraphable> listeners = new List<MmRelayNodeGraphable>();
                foreach (string listenerName in listenerNameArray)
                {
                    if (listenerName != "")
                    {
                        MmRelayNodeGraphable listener = GameObject.Find(listenerName).GetComponent<MmRelayNodeGraphable>();
                        // MmRoutingTableItem newRoutingTableItem = new MmRoutingTableItem(
                        //     listener.gameObject.name,
                        //     listener,
                        //     false);
                        // if (graphable.doNotModifyRoutingTable)
                        // {
                        //     graphable.MmRespondersToAdd.Enqueue(newRoutingTableItem);
                        // }
                        // else
                        // {
                        //     graphable.RoutingTable.Add(newRoutingTableItem);
                        // }
                        graphable.MmAddToRoutingTable(listener, listener.gameObject.name);
                    }
                }
                Debug.Log("Overwrote listeners of " + graphable.gameObject.name + ": " + listenerNames);
            }
        }
    }
}
