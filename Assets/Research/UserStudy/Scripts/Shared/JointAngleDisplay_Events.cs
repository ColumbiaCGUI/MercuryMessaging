using UnityEngine;
using TMPro;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Displays joint angle data on a UI panel (Unity Events version).
    /// Receives data via direct method call.
    /// DO NOT MODIFY THIS FILE.
    /// </summary>
    public class JointAngleDisplay_Events : MonoBehaviour
    {
        [Header("UI")]
        public TextMeshProUGUI angleText;
        public UnityEngine.UI.Image statusIndicator;

        [Header("Thresholds")]
        public float warningAngle = 60f;
        public Color normalColor = Color.green;
        public Color warningColor = Color.yellow;
        public Color dangerColor = Color.red;

        public void UpdateAngle(float angle)
        {
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
