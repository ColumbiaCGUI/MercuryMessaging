using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    public class SubsystemAlerter_Events_Solution : MonoBehaviour
    {
        public string subsystemName = "HVAC";

        [SerializeField] private CentralDashboard_Events dashboard;

        public void RaiseAlert(string message, int severity)
        {
            string alertData = $"[{subsystemName}] SEV-{severity}: {message}";
            if (dashboard != null)
                dashboard.AddAlert(alertData);
        }
    }
}
