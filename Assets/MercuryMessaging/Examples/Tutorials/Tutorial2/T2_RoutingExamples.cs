using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Tutorial 2: Demonstrates all direction methods for message routing.
/// Shows ToChildren, ToParents, ToDescendants, ToAncestors, ToSiblings, ToAll.
///
/// Keyboard Controls:
/// C - ToChildren() - Direct children only
/// P - ToParents() - Direct parents only
/// D - ToDescendants() - All descendants recursively
/// A - ToAncestors() - All ancestors recursively
/// S - ToSiblings() - Same-level nodes
/// F - ToAll() - All directions (bidirectional)
/// </summary>
public class T2_RoutingExamples : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
    }

    void Update()
    {
        // C - Send to direct children only
        if (Input.GetKeyDown(KeyCode.C))
        {
            relay.Send("Hello children").ToChildren().Execute();
            Debug.Log("[Routing] Sent .ToChildren() - direct children only");
        }

        // P - Send to direct parents only
        if (Input.GetKeyDown(KeyCode.P))
        {
            relay.Send("Hello parent").ToParents().Execute();
            Debug.Log("[Routing] Sent .ToParents() - direct parents only");
        }

        // D - Send to all descendants recursively
        if (Input.GetKeyDown(KeyCode.D))
        {
            relay.Send("Hello descendants").ToDescendants().Execute();
            Debug.Log("[Routing] Sent .ToDescendants() - all children, grandchildren, etc.");
        }

        // A - Send to all ancestors recursively
        if (Input.GetKeyDown(KeyCode.A))
        {
            relay.Send("Hello ancestors").ToAncestors().Execute();
            Debug.Log("[Routing] Sent .ToAncestors() - all parents, grandparents, etc.");
        }

        // S - Send to siblings (same-level nodes)
        if (Input.GetKeyDown(KeyCode.S))
        {
            relay.Send("Hello siblings").ToSiblings().Execute();
            Debug.Log("[Routing] Sent .ToSiblings() - nodes sharing same parent");
        }

        // F - Send to all directions (bidirectional)
        if (Input.GetKeyDown(KeyCode.F))
        {
            relay.Send("Hello everyone").ToAll().Execute();
            Debug.Log("[Routing] Sent .ToAll() - all connected nodes");
        }
    }
}
