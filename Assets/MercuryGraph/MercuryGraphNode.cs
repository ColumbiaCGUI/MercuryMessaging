using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewGraph;

[Serializable, Node("#007F00FF", "Mercury")]
public class MercuryGraphNode : INode
{
    // The MercuryGraphable gameObject that this node represents.
    public MercuryGraphable mercuryGraphable;

    // The Port attribute creates visual connectable ports in the graph view to connect to other nodes.
    // it is important to also add the SerializeReference attribute, so that unity serializes this field as a reference.
    // [Port, SerializeReference]
    // public MercuryGraphNode output = null;

    // The PortList attribute instructs the graph view to visualize a dynamic list of ports. 
    [PortList, SerializeReference]
    public List<MercuryGraphNode> mercuryOutputs = new List<MercuryGraphNode>();

}
