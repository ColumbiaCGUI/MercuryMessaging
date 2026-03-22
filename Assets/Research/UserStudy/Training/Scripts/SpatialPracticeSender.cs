using UnityEngine;
using MercuryMessaging;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Training practice: Mercury spatial filtering sender.
    /// Participants fill in the TODO line to send a message using .Within().
    /// </summary>
    public class SpatialPracticeSender : MonoBehaviour
    {
        private MmRelayNode relay;

        [Header("Configuration")]
        [Tooltip("Radius for spatial filtering (meters)")]
        public float radius = 4f;

        void Start()
        {
            relay = GetComponent<MmRelayNode>();
        }

        void Update()
        {
            if (relay == null) return;

            // ============================================
            // YOUR CODE HERE (1 line)
            // Send "ping" to all objects within the configured radius.
            //
            // Use Mercury's fluent API with spatial filtering:
            //   relay.Send("ping").ToAll().Within(radius).Execute();
            // ============================================
        }
    }
}
