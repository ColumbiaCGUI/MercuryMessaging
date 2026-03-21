using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MercuryMessaging.Research.UserStudy
{
    /// <summary>
    /// Automated correctness checking for study tasks.
    /// Run in Play Mode after participant submits to validate their solution.
    ///
    /// Each task has specific validation criteria:
    /// T1: All 4 panels receive joint data
    /// T2: Correct zone alerts at 3 test positions (>2m, 1-2m, <1m)
    /// T3: Night mode doesn't fire HVAC adjustments
    /// T4: Dashboard receives alerts from all 4 subsystems
    /// </summary>
    public class CorrectnessChecker : MonoBehaviour
    {
        [Header("T1 Validation")]
        public GameObject[] t1Panels; // 4 panels that should receive data

        [Header("T2 Validation")]
        public Transform t2Worker;
        public Transform[] t2TestPositions; // 3 positions: far (>2m), mid (1-2m), close (<1m)
        public GameObject[] t2Indicators; // 4 indicators

        [Header("T3 Validation")]
        public FacilityModeController t3ModeController;
        public TMPro.TextMeshProUGUI t3HvacStatus;

        [Header("T4 Validation")]
        public GameObject t4Dashboard;

        [Header("Output")]
        public TMPro.TextMeshProUGUI resultText;

        /// <summary>
        /// Validate T1: Check if all panels received data.
        /// Call this after entering Play Mode with the participant's solution.
        /// </summary>
        public void ValidateT1()
        {
            int panelsReceiving = 0;

            foreach (var panel in t1Panels)
            {
                if (panel == null) continue;

                // Check Mercury version
                var mmDisplay = panel.GetComponent<JointAngleDisplay>();
                if (mmDisplay != null)
                {
                    var text = mmDisplay.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (text != null && text.text != "" && text.text != "0.0°")
                    {
                        panelsReceiving++;
                        continue;
                    }
                }

                // Check Events version
                var evDisplay = panel.GetComponent<JointAngleDisplay_Events>();
                if (evDisplay != null)
                {
                    var text = evDisplay.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (text != null && text.text != "" && text.text != "0.0°")
                    {
                        panelsReceiving++;
                    }
                }
            }

            float score = (float)panelsReceiving / t1Panels.Length;
            string result = $"T1 Correctness: {panelsReceiving}/{t1Panels.Length} panels receiving data (score: {score:F2})";
            Debug.Log($"[CorrectnessChecker] {result}");
            if (resultText != null) resultText.text = result;
        }

        /// <summary>
        /// Validate T2: Check zone alerts at test positions.
        /// Moves worker to each test position and checks indicator responses.
        /// </summary>
        public void ValidateT2()
        {
            StartCoroutine(ValidateT2Coroutine());
        }

        IEnumerator ValidateT2Coroutine()
        {
            if (t2Worker == null || t2TestPositions.Length < 3)
            {
                Debug.LogError("[CorrectnessChecker] T2 validation requires worker and 3 test positions");
                yield break;
            }

            int correctResponses = 0;
            int totalChecks = 0;

            // Test position 0: Far (>2m) — should be CLEAR
            t2Worker.position = t2TestPositions[0].position;
            yield return new WaitForSeconds(1f);

            foreach (var indicator in t2Indicators)
            {
                if (indicator == null) continue;
                totalChecks++;
                var text = indicator.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text != null && text.text.ToUpper().Contains("CLEAR"))
                    correctResponses++;
            }

            // Test position 1: Mid (1-2m) — should be WARNING
            t2Worker.position = t2TestPositions[1].position;
            yield return new WaitForSeconds(1f);

            foreach (var indicator in t2Indicators)
            {
                if (indicator == null) continue;
                totalChecks++;
                var text = indicator.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text != null && text.text.ToUpper().Contains("WARNING"))
                    correctResponses++;
            }

            // Test position 2: Close (<1m) — should be EMERGENCY
            t2Worker.position = t2TestPositions[2].position;
            yield return new WaitForSeconds(1f);

            foreach (var indicator in t2Indicators)
            {
                if (indicator == null) continue;
                totalChecks++;
                var text = indicator.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text != null && text.text.ToUpper().Contains("EMERGENCY"))
                    correctResponses++;
            }

            float score = totalChecks > 0 ? (float)correctResponses / totalChecks : 0f;
            string result = $"T2 Correctness: {correctResponses}/{totalChecks} correct responses (score: {score:F2})";
            Debug.Log($"[CorrectnessChecker] {result}");
            if (resultText != null) resultText.text = result;
        }

        /// <summary>
        /// Validate T3: Check that Night mode suppresses HVAC adjustments.
        /// </summary>
        public void ValidateT3()
        {
            StartCoroutine(ValidateT3Coroutine());
        }

        IEnumerator ValidateT3Coroutine()
        {
            if (t3ModeController == null)
            {
                Debug.LogError("[CorrectnessChecker] T3 validation requires FacilityModeController");
                yield break;
            }

            // Switch to Night mode
            t3ModeController.SetMode("Night");
            yield return new WaitForSeconds(0.5f);

            // Record HVAC status
            string statusBefore = t3HvacStatus != null ? t3HvacStatus.text : "";

            // Wait for temperature simulator to fire
            yield return new WaitForSeconds(5f);

            string statusAfter = t3HvacStatus != null ? t3HvacStatus.text : "";

            // If status changed during Night mode, the bug is NOT fixed
            bool fixed_ = statusBefore == statusAfter;

            // Also verify Night mode shows energy-saving setpoint (18°C)
            bool correctSetpoint = statusAfter.Contains("18");

            float score = (fixed_ ? 0.5f : 0f) + (correctSetpoint ? 0.5f : 0f);
            string result = $"T3 Correctness: Bug fixed={fixed_}, Correct setpoint={correctSetpoint} (score: {score:F2})";
            Debug.Log($"[CorrectnessChecker] {result}");
            if (resultText != null) resultText.text = result;

            // Reset to Day mode
            t3ModeController.SetMode("Day");
        }

        /// <summary>
        /// Validate T4: Check that dashboard receives from all subsystems.
        /// </summary>
        public void ValidateT4()
        {
            StartCoroutine(ValidateT4Coroutine());
        }

        IEnumerator ValidateT4Coroutine()
        {
            // Wait for alerts to accumulate
            yield return new WaitForSeconds(10f);

            // Check dashboard log contains alerts from different subsystems
            string[] expectedSubsystems = { "HVAC", "Occupancy", "AirQuality", "Energy" };

            TMPro.TextMeshProUGUI logText = null;

            if (t4Dashboard != null)
            {
                var mmDash = t4Dashboard.GetComponent<CentralDashboard>();
                if (mmDash != null) logText = mmDash.alertLogText;

                var evDash = t4Dashboard.GetComponent<CentralDashboard_Events>();
                if (evDash != null) logText = evDash.alertLogText;
            }

            int subsystemsReporting = 0;
            if (logText != null)
            {
                string log = logText.text;
                foreach (var sub in expectedSubsystems)
                {
                    if (log.Contains(sub))
                        subsystemsReporting++;
                }
            }

            float score = (float)subsystemsReporting / expectedSubsystems.Length;
            string result = $"T4 Correctness: {subsystemsReporting}/{expectedSubsystems.Length} subsystems reporting (score: {score:F2})";
            Debug.Log($"[CorrectnessChecker] {result}");
            if (resultText != null) resultText.text = result;
        }
    }
}
