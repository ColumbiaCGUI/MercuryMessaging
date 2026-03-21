using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// T4: Alert Aggregation — Wire subsystem alerts to central dashboard.
    ///
    /// TASK: Implement RaiseAlert() to send alerts from this subsystem
    /// UP through the hierarchy to the central dashboard.
    ///
    /// The dashboard is an ancestor of this node in the Mercury hierarchy.
    /// You need ONE line of Mercury code to notify ancestors.
    ///
    /// HINT: Mercury has methods to send messages upward to parent/ancestor nodes.
    /// </summary>
    public class SubsystemAlerter_Starter : MonoBehaviour
    {
        private MmRelayNode relay;

        [Header("Subsystem Info")]
        public string subsystemName = "HVAC";

        void Start()
        {
            relay = GetComponent<MmRelayNode>();
        }

        /// <summary>
        /// Called when this subsystem detects an alert condition.
        /// TODO: Send the alert message upward to the dashboard.
        /// </summary>
        public void RaiseAlert(string message, int severity)
        {
            string alertData = $"[{subsystemName}] SEV-{severity}: {message}";

            // ============================================
            // YOUR CODE HERE (1 line of Mercury code)
            // Send alertData UP to ancestor nodes (the dashboard).
            // ============================================
        }
    }
}
