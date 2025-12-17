using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Tutorial 1: Traditional API example (for comparison with DSL).
/// Shows the verbose but explicit way to send messages.
///
/// Compare with T1_ParentController.cs which uses the recommended DSL API.
///
/// Keyboard Controls:
/// Space - Send SetActive message
/// S - Send string message with full metadata
/// </summary>
public class T1_TraditionalApiExample : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();
    }

    void Update()
    {
        // Space - Send SetActive (toggle) using traditional API
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Traditional API: requires full MmMetadataBlock
            relay.MmInvoke(MmMethod.MessageString, "Hello from parent!");
            Debug.Log("[Traditional] Sent MmInvoke(MmMethod.MessageString, ...)");
        }

        // S - Full traditional API with explicit metadata
        if (Input.GetKeyDown(KeyCode.S))
        {
            var metadata = new MmMetadataBlock(
                MmLevelFilter.Child,
                MmActiveFilter.Active,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            );
            relay.MmInvoke(MmMethod.MessageString, "Message with full metadata", metadata);
            Debug.Log("[Traditional] Sent with explicit MmMetadataBlock");
        }

        // Compare: This is 7 lines vs DSL's 1 line:
        // relay.Send("Hello").ToChildren().Active().Execute();
    }
}
