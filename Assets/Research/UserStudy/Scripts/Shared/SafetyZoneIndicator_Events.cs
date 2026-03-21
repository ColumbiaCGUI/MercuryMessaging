using UnityEngine;
using TMPro;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Safety zone indicator (Unity Events version).
    /// DO NOT MODIFY THIS FILE.
    /// </summary>
    public class SafetyZoneIndicator_Events : MonoBehaviour
    {
        [Header("UI")]
        public TextMeshProUGUI alertText;
        public Renderer indicatorRenderer;

        public Color normalColor = Color.green;
        public Color warningColor = Color.yellow;
        public Color emergencyColor = Color.red;

        public void HandleAlert(string alertType)
        {
            Color color;
            string text;

            switch (alertType.ToLower())
            {
                case "emergency":
                    color = emergencyColor;
                    text = "EMERGENCY";
                    break;
                case "warning":
                    color = warningColor;
                    text = "WARNING";
                    break;
                default:
                    color = normalColor;
                    text = "CLEAR";
                    break;
            }

            if (indicatorRenderer != null)
                indicatorRenderer.material.color = color;
            if (alertText != null)
                alertText.text = text;
        }
    }
}
