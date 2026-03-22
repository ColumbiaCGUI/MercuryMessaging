using UnityEngine;
using TMPro;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Visual indicator that responds to safety zone alerts.
    /// Changes color and shows alert text based on received messages.
    /// In Mercury: receives via ReceivedMessage
    /// In Events: receives via direct method call
    /// DO NOT MODIFY THIS FILE.
    /// </summary>
    public class SafetyZoneIndicator : MmBaseResponder
    {
        [Header("UI")]
        public TextMeshProUGUI alertText;
        public Renderer indicatorRenderer;

        [Header("Colors")]
        public Color normalColor = Color.green;
        public Color warningColor = Color.yellow;
        public Color emergencyColor = Color.red;

        public enum AlertLevel { Normal, Warning, Emergency }
        private AlertLevel currentLevel = AlertLevel.Normal;
        private float alertCooldown;

        public override void Update()
        {
            // Reset to normal if no alerts received recently
            alertCooldown -= Time.deltaTime;
            if (alertCooldown <= 0f)
            {
                SetAlertLevel(AlertLevel.Normal);
            }
        }

        protected override void ReceivedMessage(MmMessage message)
        {
            base.ReceivedMessage(message);
            if (message is MmMessageString strMsg)
            {
                HandleAlert(strMsg.value);
            }
        }

        public void HandleAlert(string alertType)
        {
            alertCooldown = 0.5f;
            switch (alertType.ToLower())
            {
                case "emergency":
                    SetAlertLevel(AlertLevel.Emergency);
                    break;
                case "warning":
                    SetAlertLevel(AlertLevel.Warning);
                    break;
            }
        }

        private void SetAlertLevel(AlertLevel level)
        {
            if (level <= currentLevel && level != AlertLevel.Normal) return;
            currentLevel = level;

            Color color = level switch
            {
                AlertLevel.Emergency => emergencyColor,
                AlertLevel.Warning => warningColor,
                _ => normalColor
            };

            if (indicatorRenderer != null)
                indicatorRenderer.material.color = color;
            if (alertText != null)
                alertText.text = level == AlertLevel.Normal ? "CLEAR" : level.ToString().ToUpper();
        }
    }
}
