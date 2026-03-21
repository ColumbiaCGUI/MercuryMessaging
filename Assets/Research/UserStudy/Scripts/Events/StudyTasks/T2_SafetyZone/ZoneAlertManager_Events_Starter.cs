using UnityEngine;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// T2: Safety Zone Alerts — Send proximity-based alerts using distance checks.
    ///
    /// TASK: Implement distance-based safety zone alerts:
    ///   - Objects within 2 meters should receive a "warning" alert
    ///   - Objects within 1 meter should receive an "emergency" alert
    ///   - Objects beyond 2 meters should show "CLEAR"
    ///
    /// You need to:
    /// 1. Declare references to the alert indicator objects
    /// 2. Calculate distance from worker to each indicator
    /// 3. Call the appropriate alert method based on distance
    /// </summary>
    public class ZoneAlertManager_Events_Starter : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The worker whose proximity triggers alerts")]
        public Transform workerTransform;

        // ============================================
        // YOUR CODE HERE
        // Declare references to SafetyZoneIndicator_Events components.
        // There are 4 indicator objects in the scene.
        // ============================================

        void Update()
        {
            if (workerTransform == null) return;

            // ============================================
            // YOUR CODE HERE
            // For each indicator:
            // 1. Calculate distance from workerTransform to the indicator
            // 2. If distance <= 1m, call HandleAlert("emergency")
            // 3. Else if distance <= 2m, call HandleAlert("warning")
            // 4. Else call HandleAlert("clear")
            //
            // Use Vector3.Distance(a, b) for distance calculation.
            // ============================================
        }
    }
}
