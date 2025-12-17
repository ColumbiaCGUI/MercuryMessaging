using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Tutorial 2: Button responder demonstrating child-to-parent notification.
/// When activated (clicked), notifies parent MenuController via ToParents().
///
/// Usage:
/// - Responds to SetActive(true) by sending notification to parent
/// - Each button has a unique clickMessage to identify which was clicked
/// </summary>
public class T2_ButtonResponder : MmBaseResponder
{
    [Tooltip("Message sent to parent when this button is activated")]
    public string clickMessage = "ButtonClicked";

    private MmRelayNode relay;

    new void Start()
    {
        relay = GetComponent<MmRelayNode>();
    }

    /// <summary>
    /// Called when SetActive message is received.
    /// When activated (true), notify parent of click.
    /// </summary>
    public override void SetActive(bool active)
    {
        base.SetActive(active);

        if (active && relay != null)
        {
            // Notify parent that this button was clicked
            relay.Send(clickMessage).ToParents().Execute();
            Debug.Log($"[{gameObject.name}] Sent '{clickMessage}' to parent");
        }
    }

    /// <summary>
    /// Alternative: Direct click handler for UI Button OnClick events.
    /// </summary>
    public void OnClick()
    {
        if (relay != null)
        {
            relay.Send(clickMessage).ToParents().Execute();
            Debug.Log($"[{gameObject.name}] Button clicked, notifying parent");
        }
    }
}
