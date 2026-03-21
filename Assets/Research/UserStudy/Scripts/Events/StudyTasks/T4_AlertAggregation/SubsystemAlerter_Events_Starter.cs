using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// T4: Alert Aggregation — Wire subsystem alerts to central dashboard.
    ///
    /// TASK: Implement RaiseAlert() to send alerts from this subsystem
    /// to the central monitoring dashboard.
    ///
    /// You need to:
    /// 1. Declare a reference to the CentralDashboard_Events component
    /// 2. Call AddAlert() on the dashboard with the alert data
    /// </summary>
    public class SubsystemAlerter_Events_Starter : MonoBehaviour
    {
        [Header("Subsystem Info")]
        public string subsystemName = "HVAC";

        // ============================================
        // YOUR CODE HERE
        // Declare a reference to the CentralDashboard_Events.
        // You'll need to wire this in the Inspector.
        // ============================================

        /// <summary>
        /// Called when this subsystem detects an alert condition.
        /// TODO: Send the alert to the central dashboard.
        /// </summary>
        public void RaiseAlert(string message, int severity)
        {
            string alertData = $"[{subsystemName}] SEV-{severity}: {message}";

            // ============================================
            // YOUR CODE HERE
            // Send alertData to the dashboard.
            // ============================================
        }
    }
}
