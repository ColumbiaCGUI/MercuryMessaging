using UnityEngine;
using TMPro;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Displays joint angle data on a UI panel.
    /// In Mercury condition: receives via MmBaseResponder.ReceivedMessage()
    /// In Events condition: receives via direct method call or UnityEvent.
    /// DO NOT MODIFY THIS FILE.
    /// </summary>
    public class JointAngleDisplay : MmBaseResponder
    {
        [Header("UI")]
        public TextMeshProUGUI angleText;
        public UnityEngine.UI.Image statusIndicator;

        [Header("Thresholds")]
        public float warningAngle = 60f;
        public Color normalColor = Color.green;
        public Color warningColor = Color.yellow;
        public Color dangerColor = Color.red;

        private float lastAngle;

        /// <summary>
        /// Mercury message handler — receives float values from parent relay.
        /// </summary>
        protected override void ReceivedMessage(MmMessage message)
        {
            base.ReceivedMessage(message);
            if (message is MmMessageFloat floatMsg)
            {
                UpdateDisplay(floatMsg.value);
            }
        }

        /// <summary>
        /// Direct update method — used by Unity Events condition.
        /// </summary>
        public void UpdateAngle(float angle)
        {
            UpdateDisplay(angle);
        }

        private void UpdateDisplay(float angle)
        {
            lastAngle = angle;
            if (angleText != null)
                angleText.text = $"{angle:F1}°";

            if (statusIndicator != null)
            {
                float absAngle = Mathf.Abs(angle);
                if (absAngle > warningAngle * 1.2f)
                    statusIndicator.color = dangerColor;
                else if (absAngle > warningAngle)
                    statusIndicator.color = warningColor;
                else
                    statusIndicator.color = normalColor;
            }
        }
    }
}
