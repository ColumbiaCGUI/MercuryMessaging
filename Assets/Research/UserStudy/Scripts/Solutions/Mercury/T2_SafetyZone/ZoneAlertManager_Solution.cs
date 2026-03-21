using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// T2: Safety Zone Alerts — SOLUTION
    /// Sends proximity-based alerts using Mercury spatial filtering.
    /// Uses .ToAll().Within(radius) so the distance is measured from THIS relay node
    /// (the Worker) to all reachable nodes (including siblings = the indicators).
    /// </summary>
    public class ZoneAlertManager_Solution : MonoBehaviour
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
            if (workerTransform == null || relay == null) return;
            relay.Send("warning").ToAll().Within(2f).Execute();
            relay.Send("emergency").ToAll().Within(1f).Execute();
        }
    }
}
