using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Central dashboard (Unity Events version).
    /// DO NOT MODIFY THIS FILE.
    /// </summary>
    public class CentralDashboard_Events : MonoBehaviour
    {
        [Header("UI")]
        public TextMeshProUGUI alertLogText;
        public int maxLogEntries = 20;

        private Queue<string> alertLog = new Queue<string>();

        public void AddAlert(string alert)
        {
            string timestamped = $"[{Time.time:F1}s] {alert}";
            alertLog.Enqueue(timestamped);
            while (alertLog.Count > maxLogEntries)
                alertLog.Dequeue();

            if (alertLogText != null)
                alertLogText.text = string.Join("\n", alertLog);

            Debug.Log($"[Dashboard-Events] {timestamped}");
        }
    }
}
