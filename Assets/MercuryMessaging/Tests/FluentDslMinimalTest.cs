using UnityEngine;
using MercuryMessaging.Protocol;
using MercuryMessaging.Protocol.DSL;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Minimal test to verify fluent DSL compiles correctly.
    /// </summary>
    public class FluentDslMinimalTest : MonoBehaviour
    {
        private MmRelayNode relay;

        void Start()
        {
            relay = GetComponent<MmRelayNode>();
            if (relay == null)
            {
                Debug.LogError("FluentDslMinimalTest requires MmRelayNode");
                return;
            }

            // Test basic message sending
            TestBasicMessages();
        }

        void TestBasicMessages()
        {
            // Test string message
            relay.Send("Hello World").Execute();

            // Test boolean message
            relay.Send(true).Execute();

            // Test integer message
            relay.Send(42).Execute();

            // Test float message
            relay.Send(3.14f).Execute();

            // Test with routing
            relay.Send("To Children").ToChildren().Execute();

            // Test with filters
            relay.Send("Active Only").Active().Execute();

            // Test with tags
            relay.Send("Tagged").WithTag(MmTag.Tag0).Execute();

            // Test commands
            relay.Initialize().Execute();
            relay.Refresh().Execute();
            relay.SetActive(true).Execute();
            relay.Switch("State1").Execute();
            relay.Complete().Execute();

            // Test broadcasting
            relay.Broadcast("Broadcast Message").Execute();

            Debug.Log("All fluent DSL tests compiled and executed successfully!");
        }
    }
}