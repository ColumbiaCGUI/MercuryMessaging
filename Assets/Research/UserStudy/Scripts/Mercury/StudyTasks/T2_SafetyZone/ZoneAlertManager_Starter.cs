using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// T2: Safety Zone Alerts — Send proximity-based alerts using spatial filtering.
    ///
    /// TASK: Use Mercury's spatial filtering to send alerts ONLY to objects
    /// within specific distance thresholds:
    ///   - "warning" to objects within 2 meters
    ///   - "emergency" to objects within 1 meter
    ///
    /// HINT: Mercury has a .Within(radius) spatial filter.
    /// You need TWO lines of Mercury fluent chain code.
    /// </summary>
    public class ZoneAlertManager_Starter : MonoBehaviour
    {
        private MmRelayNode relay;

        [Header("Configuration")]
        [Tooltip("The worker whose proximity triggers alerts")]
        public Transform workerTransform;

        void Start()
        {
            relay = GetComponent<MmRelayNode>();
        }

        void Update()
        {
            if (workerTransform == null) return;

            // ============================================
            // YOUR CODE HERE (2 lines of Mercury fluent code)
            // Line 1: Send "warning" to all descendants within 2 meters
            // Line 2: Send "emergency" to all descendants within 1 meter
            //
            // Use Mercury's fluent API with spatial filtering:
            //   relay.Send("message").ToDescendants().Within(radius).Execute();
            // ============================================
        }
    }
}
