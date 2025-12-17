// Tutorial 11: Advanced Networking - Loopback Backend Demo
// This script demonstrates using MmLoopbackBackend for local message routing
// that simulates network behavior without actual network transport.

using UnityEngine;
using MercuryMessaging.Network;

namespace MercuryMessaging.Examples.Tutorial11
{
    /// <summary>
    /// Demonstrates the MmLoopbackBackend for testing network message flow locally.
    /// Useful for testing serialization/deserialization without network overhead.
    /// </summary>
    public class T11_LoopbackDemo : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MmRelayNode relayNode;

        [Header("Settings")]
        [SerializeField] private bool enableDebugLogging = true;

        private MmLoopbackBackend _loopbackBackend;

        private void Awake()
        {
            if (relayNode == null)
                relayNode = GetComponent<MmRelayNode>();
        }

        private void Start()
        {
            // Create a loopback backend for local testing
            _loopbackBackend = new MmLoopbackBackend();

            if (enableDebugLogging)
            {
                Debug.Log("[T11] Loopback backend initialized.");
            }

            Debug.Log("[T11] Press SPACE to send a test message.");
            Debug.Log("[T11] Press 'S' to test with network filter.");
            Debug.Log("[T11] Press 'I' to initialize all children.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SendTestMessage();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                SendNetworkFilteredMessage();
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                relayNode.BroadcastInitialize();
                Debug.Log("[T11] Sent BroadcastInitialize()");
            }
        }

        private void SendTestMessage()
        {
            Debug.Log("[T11] Sending test message...");

            // Send a simple string message
            relayNode.Send("Hello from Loopback!")
                .ToDescendants()
                .Execute();
        }

        private void SendNetworkFilteredMessage()
        {
            Debug.Log("[T11] Sending network-filtered message...");

            // Send with network filter - OverNetwork() sends to network peers
            relayNode.Send("Network test message")
                .ToDescendants()
                .OverNetwork()
                .Execute();
        }
    }
}
