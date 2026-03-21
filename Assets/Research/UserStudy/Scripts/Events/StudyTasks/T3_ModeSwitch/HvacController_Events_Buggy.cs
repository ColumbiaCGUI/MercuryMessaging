using UnityEngine;
using UnityEngine.Events;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// HVAC Controller (Unity Events version).
    ///
    /// BUG: When the facility switches to Night mode, the HVAC daytime
    /// adjustment routine still fires. Temperature adjustment events
    /// continue to be processed even during Night mode.
    ///
    /// TASK: Find and fix the bug so that HVAC adjustments only occur
    /// during Day mode. When in Night mode, the HVAC should enter
    /// energy-saving defaults (setpoint = 18°C) and ignore adjustment events.
    ///
    /// HINT: Look at how OnTemperatureRequested handles the mode check.
    /// </summary>
    public class HvacController_Events_Buggy : MonoBehaviour
    {
        [Header("HVAC Settings")]
        public float daySetpoint = 22f;
        public float nightSetpoint = 18f;

        [Header("Status")]
        public TMPro.TextMeshProUGUI statusText;

        [Header("Events")]
        public UnityEvent<string> OnHvacStatusChanged;

        private float currentSetpoint;
        private string currentMode = "Day";
        private bool isActive = true;

        void Start()
        {
            currentSetpoint = daySetpoint;
        }

        public void OnModeChanged(string newMode)
        {
            currentMode = newMode;
            if (currentMode == "Night")
            {
                currentSetpoint = nightSetpoint;
                isActive = false;  // Set but never checked!
            }
            else
            {
                currentSetpoint = daySetpoint;
                isActive = true;
            }
            UpdateStatus();
        }

        /// <summary>
        /// Called by TemperatureSimulator's UnityEvent.
        /// BUG: Processes temperature adjustments regardless of mode.
        /// </summary>
        public void OnTemperatureRequested(float requestedTemp)
        {
            // BUG: Missing check for isActive or currentMode
            currentSetpoint = requestedTemp;
            Debug.Log($"[HVAC-Events] Adjusted setpoint to {currentSetpoint}°C (mode: {currentMode})");
            UpdateStatus();
            OnHvacStatusChanged?.Invoke($"HVAC: {currentSetpoint}°C");
        }

        private void UpdateStatus()
        {
            if (statusText != null)
                statusText.text = $"HVAC: {currentSetpoint:F1}°C ({currentMode})";
        }
    }
}
