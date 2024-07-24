using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewGraph;

[Serializable, Node("#007F00FF", "Mercury")]
public class MercuryGraphNode : INode
{
    // The MercuryGraphable gameObject that this node represents.
    [GraphDisplay(displayType = DisplayType.BothViews, editability = Editability.None, createGroup = false)]
    public MmRelayNodeGraphable mercuryGraphable;

    // Test toggle for the node.
    [GraphDisplay(displayType = DisplayType.BothViews, editability = Editability.None, createGroup = false)]
    public bool testToggle = false;

    // The PortList attribute instructs the graph view to visualize a dynamic list of ports. 
    [PortList, SerializeReference]
    public List<MercuryGraphNode> mercuryOutputs = new List<MercuryGraphNode>();
}
