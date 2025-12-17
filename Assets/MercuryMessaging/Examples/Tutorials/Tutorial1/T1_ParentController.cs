using UnityEngine;
using MercuryMessaging;

/// <summary>
/// Tutorial 1: Parent controller demonstrating DSL API message sending.
/// Shows both Tier 1 (auto-execute) and Tier 2 (fluent chain) methods.
///
/// Hierarchy Setup:
/// T1_Root (MmRelayNode + T1_ParentController)
///   └── T1_Child (MmRelayNode + T1_ChildResponder)
///
/// Keyboard Controls:
/// I - Initialize all children
/// 1 - Send string message
/// 2 - Send int message
/// R - Refresh all children
/// </summary>
public class T1_ParentController : MonoBehaviour
{
    private MmRelayNode relay;

    void Start()
    {
        relay = GetComponent<MmRelayNode>();

        // Initialize all children on start using Tier 1 auto-execute
        relay.BroadcastInitialize();
    }

    void Update()
    {
        // I - Initialize using Tier 1 (shortest syntax)
        if (Input.GetKeyDown(KeyCode.I))
        {
            relay.BroadcastInitialize();
            Debug.Log("[Parent] Sent BroadcastInitialize()");
        }

        // 1 - Send string message using Tier 2 fluent chain
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            relay.Send("Message from parent!").ToChildren().Execute();
            Debug.Log("[Parent] Sent string via .Send().ToChildren().Execute()");
        }

        // 2 - Send int message using Tier 2 fluent chain
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            relay.Send(100).ToChildren().Execute();
            Debug.Log("[Parent] Sent int via .Send().ToChildren().Execute()");
        }

        // R - Refresh using Tier 1 (auto-execute)
        if (Input.GetKeyDown(KeyCode.R))
        {
            relay.BroadcastRefresh();
            Debug.Log("[Parent] Sent BroadcastRefresh()");
        }

        // 3 - Demonstrate BroadcastValue (auto type detection)
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            relay.BroadcastValue(42);  // Auto-detects int
            Debug.Log("[Parent] Sent BroadcastValue(42) - auto-detects type");
        }
    }
}
