using UnityEngine;
using TMPro;

namespace MercuryMessaging.Testing
{
    /// <summary>
    /// UI helper for displaying test results.
    /// Manages result text, colors, and log output.
    /// </summary>
    public class TestResultDisplay : MonoBehaviour
    {
        [Header("Result Text Fields")]
        public TextMeshProUGUI qw4Result;
        public TextMeshProUGUI qw1HopResult;
        public TextMeshProUGUI qw1CycleResult;
        public TextMeshProUGUI qw2Result;

        [Header("Memory Display")]
        public TextMeshProUGUI currentMemoryText;
        public TextMeshProUGUI peakMemoryText;

        [Header("Log Display")]
        public TextMeshProUGUI logText;
        public UnityEngine.UI.ScrollRect logScrollRect;

        [Header("Colors")]
        public Color passColor = Color.green;
        public Color failColor = Color.red;

        private System.Text.StringBuilder logBuilder = new System.Text.StringBuilder();

        public void Initialize()
        {
            ClearResults();
            UpdateMemoryStats();
        }

        public void ClearResults()
        {
            SetResultText(qw4Result, "Pending...", Color.gray);
            SetResultText(qw1HopResult, "Pending...", Color.gray);
            SetResultText(qw1CycleResult, "Pending...", Color.gray);
            SetResultText(qw2Result, "Pending...", Color.gray);

            logBuilder.Clear();
            if (logText != null)
            {
                logText.text = "";
            }
        }

        public void DisplayResult(string testName, bool passed, float executionTime, string details)
        {
            Color resultColor = passed ? passColor : failColor;
            string status = passed ? "PASS" : "FAIL";
            string resultText = $"{status} - {details}";

            if (testName.Contains("CircularBuffer") || testName.Contains("QW-4"))
            {
                SetResultText(qw4Result, resultText, resultColor);
            }
            else if (testName.Contains("Hop") && testName.Contains("Limit"))
            {
                SetResultText(qw1HopResult, resultText, resultColor);
            }
            else if (testName.Contains("Cycle"))
            {
                SetResultText(qw1CycleResult, resultText, resultColor);
            }
            else if (testName.Contains("Lazy") || testName.Contains("QW-2"))
            {
                SetResultText(qw2Result, resultText, resultColor);
            }
        }

        public void AppendLog(string message)
        {
            logBuilder.AppendLine(message);
            if (logText != null)
            {
                logText.text = logBuilder.ToString();
            }

            // Scroll to bottom
            if (logScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                logScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public void UpdateMemoryStats()
        {
            long currentMemory = System.GC.GetTotalMemory(false) / 1024 / 1024;
            long peakMemory = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / 1024 / 1024;

            if (currentMemoryText != null)
            {
                currentMemoryText.text = $"Current: {currentMemory} MB";
            }

            if (peakMemoryText != null)
            {
                peakMemoryText.text = $"Peak: {peakMemory} MB";
            }
        }

        private void SetResultText(TextMeshProUGUI textField, string text, Color color)
        {
            if (textField != null)
            {
                textField.text = text;
                textField.color = color;
            }
        }

        private void Update()
        {
            // Update memory stats every second
            if (Time.frameCount % 60 == 0)
            {
                UpdateMemoryStats();
            }
        }
    }
}
