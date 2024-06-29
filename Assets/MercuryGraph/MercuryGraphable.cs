using System.Collections;
using System.Collections.Generic;
using NewGraph;
using UnityEngine;

public class MercuryGraphable : MonoBehaviour
{
    // The node view that represents this graphable in the graph view.
    public NodeController nodeController;

    // A list of graphables that listen to this graphable's signals.
    public List<MercuryGraphable> mercurySignalListeners = new List<MercuryGraphable>();
}
