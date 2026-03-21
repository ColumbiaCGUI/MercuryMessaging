using UnityEngine;
using UnityEngine.Events;

namespace MercuryMessaging.Research.UserStudy
{
    public class HvacController_Events_Solution : MonoBehaviour
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
                isActive = false;
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
        /// FIX: Guard against processing adjustments when not in Day mode.
        /// </summary>
        public void OnTemperatureRequested(float requestedTemp)
        {
            if (!isActive) return;  // FIX: ignore events during Night mode
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
