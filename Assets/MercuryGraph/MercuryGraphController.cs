using System;
using System.Collections;
using System.Collections.Generic;
using NewGraph;
using UnityEngine;

public class MercuryGraphController : MonoBehaviour
{
    [Header("Graph Settings and References (DO change these.)")]
    public ScriptableGraphModel graphModel;

    [Header("Graph Controller (DON'T change these!)")]
    public GraphWindow graphWindow;
    public GraphController graphController;

    void Start()
    {
        graphWindow = GraphWindow.OpenWindow();
        graphController = graphWindow.graphController;
        RenderGraph();
    }

    void OnDisable()
    {
        // Clear the graph
        int counter = 0;
        graphController.ForEachNode((node) =>
        {
            graphController.graphView.SetSelected(node as NodeView, true, false);
            counter++;
        });
        graphController.OnDelete();
        Debug.Log("Deleted " + counter + " nodes.");
    }

    public void RenderGraph()
    {
        // Get all graphables in scene
        List<MercuryGraphable> graphables = GetMercuryGraphables();

        // Create nodes for each graphable (Line them up for now)
        // Save the nodeView objects created
        List<NodeView> nodeViews = new List<NodeView>();
        Vector2 position = new Vector2(0, 0);
        foreach (MercuryGraphable graphable in graphables)
        {
            Debug.Log("Creating " + graphable.gameObject.name);

            // Create a new node
            Type nodeType = typeof(MercuryGraphNode);
            NodeController nodeController = CreateNewNode(position, graphable, nodeType);
            graphable.nodeController = nodeController;

            // Move to next position
            position.x -= 200;
            position.y -= 200;
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
        graphController.Reload();
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
        }

        // Add its data
        nodeModel.SetData(graphController.graphData.GetLastAddedNodeProperty(false));
        Debug.Log(graphable.gameObject.name);
        nodeModel.SetName(graphable.gameObject.name);

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
