using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// T4: Alert Aggregation — Solution.
    ///
    /// FIX: RaiseAlert() uses relay.NotifyValue() to send the alert
    /// upward through the hierarchy to the central dashboard ancestor.
    /// </summary>
    public class SubsystemAlerter_Solution : MonoBehaviour
    {
        private MmRelayNode relay;

        [Header("Subsystem Info")]
        public string subsystemName = "HVAC";

        void Start()
        {
            relay = GetComponent<MmRelayNode>();
        }

        public void RaiseAlert(string message, int severity)
        {
            string alertData = $"[{subsystemName}] SEV-{severity}: {message}";
            relay.NotifyValue(alertData);
        }
    }
}
