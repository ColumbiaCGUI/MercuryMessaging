// using System;
// using System.Collections;
// using System.Collections.Generic;
// using GraphViewBase;
// using MercuryMessaging;
// using NewGraph;
// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine;

// public class MercuryGraphController : MonoBehaviour
// {
//     public static MercuryGraphController instance;

//     [Header("Graph Settings and References (DO change these.)")]
//     public bool automaticGraphRendering = true;
//     public ScriptableGraphModel graphModel;

//     [Header("Graph Controller (DON'T change these!)")]
//     public GraphWindow graphWindow;
//     public GraphController graphController;

//     [InitializeOnLoadMethod]
//     private static void Initialize()
//     {
//         ScriptableInspectorController.OnRefreshButtonClicked += (graphWindow) => OnRefreshButtonClicked();
//         ScriptableInspectorController.OnApplyButtonClicked += (graphWindow) => OnApplyButtonClicked();
//         // EditorApplication.hierarchyChanged += OnHierarchyChanged;
//         // EditorApplication.projectChanged += OnProjectChanged;
//         // EditorApplication.update += OnEditorUpdate;
//         Undo.postprocessModifications += ctx => OnPostProcessModifications(ctx);
//         Undo.undoRedoPerformed += OnUndoRedoPerformed;
//     }

//     void Awake()
//     {
//         instance = this;
//     }

//     void Start()
//     {
//         graphWindow = GraphWindow.OpenWindow();
//         graphController = graphWindow.graphController;
//         RenderGraph();
//     }

//     void OnDisable()
//     {
//         ClearGraph();
//     }

//     private static void OnApplyButtonClicked()
//     {
//         // Debug.Log("MercuryGraphController: Apply Button clicked!");

//         // If in play mode, we save the node positions and listeners
//         if (Application.isPlaying)
//         {
//             if (instance == null) return;
//             // Debug.Log("MercuryGraphController: In Play Mode: Saving node positions and listeners.");
//             instance.SaveNodePositions();
//             instance.SaveMercuryGraphableListeners();
//         }
//         else
//         {
//             // If in edit mode, we change the listeners according to the saved changes
//             // Debug.Log("MercuryGraphController: In Edit Mode: Overwriting listeners.");
//             OverwriteMercuryGraphableListeners();
//         }
//     }
//     private static void OnRefreshButtonClicked()
//     {
//         // Debug.Log("MercuryGraphController: Refresh Button clicked!");
//         if (instance == null) return;
//         // instance.ClearGraph();
//         instance.RenderGraph();
//     }

//     private static void OnHierarchyChanged()
//     {
//         Debug.Log("MercuryGraphController: OnHierarchyChanged");
//         if (instance == null) return;
//         if (!instance.automaticGraphRendering) return;
//         instance.ClearGraph();
//         instance.RenderGraph();
//     }

//     private static void OnProjectChanged()
//     {
//         Debug.Log("MercuryGraphController: OnProjectChanged");
//         if (instance == null) return;
//         if (!instance.automaticGraphRendering) return;
//         // instance.ClearGraph();
//         instance.RenderGraph();
//     }

//     private static void OnEditorUpdate()
//     {
//         Debug.Log("MercuryGraphController: OnEditorUpdate");
//         if (instance != null) return;
//         if (!instance.automaticGraphRendering) return;
//         if (EditorApplication.isCompiling)
//         {
//             // instance.ClearGraph();
//             instance.RenderGraph();
//         }
//     }

//     private static UndoPropertyModification[] OnPostProcessModifications(UndoPropertyModification[] modifications)
//     {
//         if (instance == null) return modifications;
//         if (!instance.automaticGraphRendering) return modifications;

//         bool graphableModified = false;
//         foreach (var modificaton in modifications)
//         {
//             // Debug.Log("Post process modification: " + modificaton.currentValue.target);
//             if (modificaton.currentValue.target is MmRelayNode)
//             {
//                 graphableModified = true;
//                 break;
//             }
//         }
//         if (graphableModified)
//         {
//             // instance.ClearGraph();
//             instance.RenderGraph();
//             // Debug.Log("MercuryGraphController: OnPostProcessModifications");
//         }
//         return modifications;
//     }
//     private static void OnUndoRedoPerformed()
//     {
//         // Debug.Log("MercuryGraphController: OnUndoRedoPerformed");
//         if (instance == null) return;
//         if (!instance.automaticGraphRendering) return;
//         // instance.ClearGraph();
//         instance.RenderGraph();
//     }

//     public void RenderGraph()
//     {
//         instance.ClearGraph();

//         // Get all graphables in scene
//         List<MmRelayNode> graphables = GetMercuryGraphables();
//         // Debug.Log(graphables.Count + " graphables found in Hierarchy.");

//         // Create nodes for each graphable (Line them up for now)
//         // Save the nodeView objects created
//         List<NodeView> nodeViews = new List<NodeView>();
//         // Vector2 position = new Vector2(0, 0);
//         foreach (MmRelayNode graphable in graphables)
//         {
//             // Debug.Log("Creating " + graphable.gameObject.name);

//             // Create a new node
//             Type nodeType = typeof(MercuryGraphNode);

//             if (PlayerPrefs.HasKey(graphable.gameObject.name + "_nodePosition_x"))
//             {
//                 graphable.nodePosition.x = PlayerPrefs.GetFloat(graphable.gameObject.name + "_nodePosition_x");
//                 graphable.nodePosition.y = PlayerPrefs.GetFloat(graphable.gameObject.name + "_nodePosition_y");
//                 // Debug.Log("Loaded position of " + graphable.gameObject.name + " at " + graphable.nodePosition);
//             }
//             NodeController nodeController = CreateNewNode(graphable.nodePosition, graphable, nodeType);
//             graphable.nodeController = nodeController;

//             // Move to next position
//             // position.x -= 200;
//             // position.y -= 200;
//         }

//         // Link the nodes
//         foreach (MmRelayNode graphable in graphables)
//         {
//             MercuryGraphNode mercuryGraphNode = graphable.nodeController.nodeItem.nodeData as MercuryGraphNode;
//             // Debug.Log("MercuryGraphController: Calling GetMmMessageResponders for " + graphable.gameObject.name);
//             List<MmRelayNode> responders = GetMmMessageResponders(graphable);
//             foreach (MmRelayNode responder in responders)
//             {
//                 // Debug.Log("Linking " + graphable.gameObject.name + " to " + responder.gameObject.name);
//                 if(responder.nodeController != null)
//                 {
//                     MercuryGraphNode mercurySignalListenerNode = responder.nodeController.nodeItem.nodeData as MercuryGraphNode;
//                     mercuryGraphNode.mercuryOutputs.Add(mercurySignalListenerNode);
//                 }
                
//             }
//         }

//         // Reload the graph
//         // graphController.graphView.Unbind();
//         graphController.Reload();
//         SaveNodePositions();
//         // Debug.Log("Graph rendered.");
//     }

//     public void ClearGraph()
//     {
//         SaveNodePositions();

//         // Debug.Log("Clearing graph...");
//         graphController.inspector.Clear();
//         graphController.graphView.ClearView();
//         graphController.dataToViewLookup.Clear();
//         graphController.inspector.SetSelectedNodeInfoActive(active: false);

//         UnityEngine.Object graphData = ScriptableObject.CreateInstance<ScriptableGraphModel>();
//         IGraphModelData graphModel = graphData as IGraphModelData;
//         graphModel.CreateSerializedObject();
//         graphController.graphData = graphModel;
//         if (graphController.graphData.SerializedGraphData == null)
//         {
//             graphController.graphData.CreateSerializedObject();
//         }

//         int counter = 0;
//         graphController.ForEachNode((node) =>
//         {
//             graphController.graphView.SetSelected(node as NodeView, true, false);
//             counter++;
//         });
//         graphController.OnDelete();
//         // Debug.Log("Deleted " + counter + " nodes.");
//     }

//     public static List<MmRelayNode> GetMercuryGraphables()
//     {
//         List<MmRelayNode> graphables = new List<MmRelayNode>();
//         if (Application.isPlaying)
//         {
//             MmRelayNode[] graphableArray = FindObjectsOfType<MmRelayNode>();
//             graphables = new List<MmRelayNode>(graphableArray);
//         }
//         else
//         {
//             GameObject[] rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
//             foreach (GameObject rootGameObject in rootGameObjects)
//             {
//                 MmRelayNode[] graphableArray = rootGameObject.GetComponentsInChildren<MmRelayNode>();
//                 graphables.AddRange(graphableArray);
//             }
//         }
//         return graphables;
//     }
//     public static List<MmRelayNode> GetMmMessageResponders(MmRelayNode graphable)
//     {
//         List<MmRelayNode> responders = new List<MmRelayNode>();
//         List<MmRoutingTableItem> mmRoutingTableItems = graphable.RoutingTable.GetMmRoutingTableItems(
//             MmRoutingTable.ListFilter.All,
//             MmLevelFilterHelper.SelfAndChildren);
//         // We only want the children (listeners) of the graphable
//         List<MmRoutingTableItem> mmRoutingTableItemsChildren = new List<MmRoutingTableItem>();
//         foreach (MmRoutingTableItem item in mmRoutingTableItems)
//         {
//             if (item.Level == MmLevelFilter.Child)
//             {
//                 mmRoutingTableItemsChildren.Add(item);
//             }
//         }

//         foreach (MmRoutingTableItem item in mmRoutingTableItemsChildren)
//         {
//             if (item.Responder is MmRelayNode)
//             {
//                 responders.Add((MmRelayNode)item.Responder);
//             }
//             else if (item.Responder is MmBaseResponder)
//             {
//                 MmRelayNode graphableResponder = item.Responder.gameObject.GetComponent<MmRelayNode>();
//                 if (graphableResponder != null)
//                 {
//                     responders.Add(graphableResponder);
//                 }
            
//             }
//         }
//         // Debug.Log("MercuryGraphController: Found " + responders.Count + " responders for " + graphable.gameObject.name);
//         return responders;
//     }

//     // Modified from GraphController.CreateNewNode()
//     public NodeController CreateNewNode(Vector2 position, MmRelayNode graphable, Type nodeType)
//     {
//         // Create a node and add it to the graph
//         INode node = Activator.CreateInstance(nodeType) as INode;
//         NodeModel nodeModel = graphController.graphData.AddNode(node, false);
//         if (node is MercuryGraphNode mercuryGraphNode)
//         {
//             mercuryGraphNode.mercuryGraphable = graphable;
//             mercuryGraphNode.testToggle = graphable.testToggle;
//         }

//         // Add its data
//         nodeModel.SetData(graphController.graphData.GetLastAddedNodeProperty(false));
//         // Debug.Log(graphable.gameObject.name);
//         if (graphable != null)
//         {
//             nodeModel.SetName(graphable.gameObject.name);
//         }

//         // Create a node controller and view
//         NodeController nodeController = new NodeController(nodeModel, graphController, position);
//         graphController.graphView.AddElement(nodeController.nodeView);
//         nodeController.Initialize();

//         // Select the new node
//         graphController.graphView.SetSelected(nodeController.nodeView);

//         // Store the data for data to node lookup later..?
//         // graphController.dataToViewLookup.Add(node, nodeController.nodeView);
//         return nodeController;
//     }

//     public void SaveNodePositions()
//     {
//         // Save positions of the graph nodes
//         List<MmRelayNode> graphables = GetMercuryGraphables();
//         foreach (MmRelayNode graphable in graphables)
//         {
//             if (graphable.nodeController != null)
//             {
//                 graphable.nodePosition = graphable.nodeController.nodeItem.GetPosition();
//                 PlayerPrefs.SetFloat(graphable.gameObject.name + "_nodePosition_x", graphable.nodePosition.x);
//                 PlayerPrefs.SetFloat(graphable.gameObject.name + "_nodePosition_y", graphable.nodePosition.y);
//                 // Debug.Log("Saved position of " + graphable.gameObject.name + " at " + graphable.nodePosition);
//             }
//         }
//     }
//     public void SaveMercuryGraphableListeners()
//     {
//         // Save the listeners of each graphable in Playerprefs
//         List<MmRelayNode> graphables = GetMercuryGraphables();
//         foreach (MmRelayNode graphable in graphables)
//         {
//             if (graphable.nodeController != null)
//             {
//                 List<MmRelayNode> listeners = GetMmMessageResponders(graphable);
//                 if (listeners.Count > 0)
//                 {
//                     string listenerNames = "";
//                     foreach (MmRelayNode listener in listeners)
//                     {
//                         listenerNames += listener.gameObject.name + ",";
//                     }
//                     PlayerPrefs.SetString(graphable.gameObject.name + "_listeners", listenerNames);
//                     // Debug.Log("Saved listeners of " + graphable.gameObject.name + ": " + listenerNames);
//                 }
//             }
//         }
//     }
//     public static void OverwriteMercuryGraphableListeners()
//     {
//         // Overwrite the listeners of each graphable
//         List<MmRelayNode> graphables = GetMercuryGraphables();
//         // Debug.Log("Overwriting Listeners of " + graphables.Count + " graphables.");
//         foreach (MmRelayNode graphable in graphables)
//         {
//             graphable.RoutingTable.Clear();
//             string listenerNames = PlayerPrefs.GetString(graphable.gameObject.name + "_listeners");
//             if (listenerNames != "")
//             {
//                 string[] listenerNameArray = listenerNames.Split(',');
//                 List<MmRelayNode> listeners = new List<MmRelayNode>();
//                 foreach (string listenerName in listenerNameArray)
//                 {
//                     if (listenerName != "")
//                     {
//                         MmRelayNode listener = GameObject.Find(listenerName).GetComponent<MmRelayNode>();
//                         // MmRoutingTableItem newRoutingTableItem = new MmRoutingTableItem(
//                         //     listener.gameObject.name,
//                         //     listener,
//                         //     false);
//                         // if (graphable.doNotModifyRoutingTable)
//                         // {
//                         //     graphable.MmRespondersToAdd.Enqueue(newRoutingTableItem);
//                         // }
//                         // else
//                         // {
//                         //     graphable.RoutingTable.Add(newRoutingTableItem);
//                         // }
//                         graphable.MmAddToRoutingTable(listener, listener.gameObject.name);
//                     }
//                 }
//                 // Debug.Log("Overwrote listeners of " + graphable.gameObject.name + ": " + listenerNames);
//             }
//         }
//     }
// }
