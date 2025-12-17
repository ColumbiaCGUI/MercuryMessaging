// Tutorial 11: Advanced Networking - Network Message Responder
// Demonstrates handling network-originated messages and checking IsDeserialized flag.

using UnityEngine;

namespace MercuryMessaging.Examples.Tutorial11
{
    /// <summary>
    /// Demonstrates handling messages and checking their network origin.
    /// Shows the difference between local and deserialized (network) messages.
    /// </summary>
    public class T11_NetworkResponder : MmBaseResponder
    {
        [Header("Debug")]
        [SerializeField] private bool logAllMessages = true;

        protected override void ReceivedMessage(MmMessageString message)
        {
            string origin = message.IsDeserialized ? "NETWORK" : "LOCAL";

            if (logAllMessages)
            {
                Debug.Log($"[T11_NetworkResponder] [{origin}] Received: {message.value}");
            }

            // Example: Only process local messages to avoid double-processing on host
            if (!message.IsDeserialized)
            {
                ProcessLocalMessage(message.value);
            }

            // Example: Only process network messages for specific logic
            if (message.IsDeserialized)
            {
                ProcessNetworkMessage(message.value);
            }
        }

        protected override void ReceivedMessage(MmMessageInt message)
        {
            string origin = message.IsDeserialized ? "NETWORK" : "LOCAL";

            if (logAllMessages)
            {
                Debug.Log($"[T11_NetworkResponder] [{origin}] Received int: {message.value}");
            }
        }

        protected override void ReceivedMessage(MmMessageTransform message)
        {
            string origin = message.IsDeserialized ? "NETWORK" : "LOCAL";

            if (logAllMessages)
            {
                Debug.Log($"[T11_NetworkResponder] [{origin}] Received transform: pos={message.MmTransform.Translation}");
            }

            // Apply network transform updates
            if (message.IsDeserialized)
            {
                transform.position = message.MmTransform.Translation;
                transform.rotation = message.MmTransform.Rotation;
            }
        }

        private void ProcessLocalMessage(string value)
        {
            Debug.Log($"[T11] Processing LOCAL message: {value}");
            // Local-only logic here (e.g., UI updates, sounds)
        }

        private void ProcessNetworkMessage(string value)
        {
            Debug.Log($"[T11] Processing NETWORK message: {value}");
            // Network-only logic here (e.g., sync state from remote)
        }
    }
}
