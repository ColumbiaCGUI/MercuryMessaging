using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Controls the facility mode (Day/Night). Used by both conditions.
    /// Provides UI buttons to toggle mode.
    /// DO NOT MODIFY THIS FILE.
    /// </summary>
    public class FacilityModeController : MonoBehaviour
    {
        [Header("Mercury (optional)")]
        public MmRelayNode facilityRelay;

        [Header("Events (optional)")]
        public UnityEngine.Events.UnityEvent<string> OnModeChanged;

        private string currentMode = "Day";

        public void ToggleMode()
        {
            currentMode = currentMode == "Day" ? "Night" : "Day";

            // Mercury path
            if (facilityRelay != null)
                facilityRelay.BroadcastSwitch(currentMode);

            // Events path
            OnModeChanged?.Invoke(currentMode);

            Debug.Log($"[Facility] Mode switched to: {currentMode}");
        }

        public void SetMode(string mode)
        {
            currentMode = mode;
            if (facilityRelay != null)
                facilityRelay.BroadcastSwitch(currentMode);
            OnModeChanged?.Invoke(currentMode);
        }

        public string GetCurrentMode() => currentMode;
    }
}
