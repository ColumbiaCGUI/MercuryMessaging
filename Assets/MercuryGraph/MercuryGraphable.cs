using System.Collections;
using System.Collections.Generic;
using NewGraph;
using UnityEngine;

public class MercuryGraphable : MonoBehaviour
{
    // The node view that represents this graphable in the graph view.
    public NodeController nodeController;
    // The position of the graph node in the graph view.
    public Vector2 nodePosition = new Vector2(0, 0);

    // A list of graphables that listen to this graphable's signals.
    public List<MercuryGraphable> mercurySignalListeners = new List<MercuryGraphable>();

    public bool testToggle = false;
}
