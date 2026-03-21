using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Central monitoring dashboard that displays alerts from all subsystems.
    /// In Mercury: receives via ReceivedMessage (ancestor notification)
    /// In Events: receives via direct method call
    /// DO NOT MODIFY THIS FILE.
    /// </summary>
    public class CentralDashboard : MmBaseResponder
    {
        [Header("UI")]
        public TextMeshProUGUI alertLogText;
        public int maxLogEntries = 20;

        private Queue<string> alertLog = new Queue<string>();

        protected override void ReceivedMessage(MmMessage message)
        {
            base.ReceivedMessage(message);
            if (message is MmMessageString strMsg)
            {
                AddAlert(strMsg.value);
            }
        }

        public void AddAlert(string alert)
        {
            string timestamped = $"[{Time.time:F1}s] {alert}";
            alertLog.Enqueue(timestamped);
            while (alertLog.Count > maxLogEntries)
                alertLog.Dequeue();

            if (alertLogText != null)
                alertLogText.text = string.Join("\n", alertLog);

            Debug.Log($"[Dashboard] {timestamped}");
        }
    }
}
