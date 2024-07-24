using System.Collections;
using System.Collections.Generic;
using MercuryMessaging;
using NewGraph;
using UnityEngine;

public class MmRelayNodeGraphable : MmRelayNode
{
    // The node view that represents this graphable in the graph view.
    public NodeController nodeController;
    // The position of the graph node in the graph view.
    public Vector2 nodePosition = new Vector2(0, 0);

    public bool testToggle = false;
}
