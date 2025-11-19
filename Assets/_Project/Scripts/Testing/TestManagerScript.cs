using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MercuryMessaging.Testing
{
    /// <summary>
    /// Main test orchestrator for Quick Win validation.
    /// Manages test execution, UI updates, and result reporting.
    /// </summary>
    public class TestManagerScript : MonoBehaviour
    {
        [Header("Test Components")]
        public CircularBufferMemoryTest circularBufferTest;
        public HopLimitTest hopLimitTest;
        public CycleDetectionTest cycleDetectionTest;
        public LazyCopyPerformanceTest lazyCopyTest;

        [Header("UI References")]
        public Button runCircularBufferBtn;
        public Button runHopLimitBtn;
        public Button runCycleDetectionBtn;
        public Button runLazyCopyBtn;
        public Button runAllTestsBtn;

        [Header("Result Display")]
        public TextResultDisplay resultDisplay;

        [Header("Test Status")]
        public bool isRunningTests = false;

        private void Start()
        {
            // Wire up button events
            runCircularBufferBtn?.onClick.AddListener(RunCircularBufferTest);
            runHopLimitBtn?.onClick.AddListener(RunHopLimitTest);
            runCycleDetectionBtn?.onClick.AddListener(RunCycleDetectionTest);
            runLazyCopyBtn?.onClick.AddListener(RunLazyCopyTest);
            runAllTestsBtn?.onClick.AddListener(RunAllTests);

            resultDisplay?.Initialize();
        }

        #region Individual Test Methods

        public void RunCircularBufferTest()
        {
            if (isRunningTests)
            {
                Debug.LogWarning("Tests are already running. Please wait.");
                return;
            }

            StartCoroutine(ExecuteTest("CircularBuffer", circularBufferTest?.Execute));
        }

        public void RunHopLimitTest()
        {
            if (isRunningTests)
            {
                Debug.LogWarning("Tests are already running. Please wait.");
                return;
            }

            StartCoroutine(ExecuteTest("HopLimit", hopLimitTest?.Execute));
        }

        public void RunCycleDetectionTest()
        {
            if (isRunningTests)
            {
                Debug.LogWarning("Tests are already running. Please wait.");
                return;
            }

            StartCoroutine(ExecuteTest("CycleDetection", cycleDetectionTest?.Execute));
        }

        public void RunLazyCopyTest()
        {
            if (isRunningTests)
            {
                Debug.LogWarning("Tests are already running. Please wait.");
                return;
            }

            StartCoroutine(ExecuteTest("LazyCopy", lazyCopyTest?.Execute));
        }

        #endregion

        #region Run All Tests

        public void RunAllTests()
        {
            if (isRunningTests)
            {
                Debug.LogWarning("Tests are already running. Please wait.");
                return;
            }

            StartCoroutine(ExecuteAllTests());
        }

        private IEnumerator ExecuteAllTests()
        {
            isRunningTests = true;
            resultDisplay?.ClearResults();
            resultDisplay?.AppendLog("=== Starting All Quick Win Validation Tests ===");

            // Record start time
            float totalStartTime = Time.realtimeSinceStartup;

            // Run tests sequentially
            yield return ExecuteTest("QW-4: CircularBuffer", circularBufferTest?.Execute);
            yield return new WaitForSeconds(0.5f);

            yield return ExecuteTest("QW-1: Hop Limits", hopLimitTest?.Execute);
            yield return new WaitForSeconds(0.5f);

            yield return ExecuteTest("QW-1: Cycle Detection", cycleDetectionTest?.Execute);
            yield return new WaitForSeconds(0.5f);

            yield return ExecuteTest("QW-2: Lazy Copying", lazyCopyTest?.Execute);

            // Calculate total time
            float totalTime = Time.realtimeSinceStartup - totalStartTime;

            resultDisplay?.AppendLog($"\n=== All Tests Complete in {totalTime:F2}s ===");
            isRunningTests = false;
        }

        #endregion

        #region Test Execution Helper

        private IEnumerator ExecuteTest(string testName, System.Func<IEnumerator> testMethod)
        {
            if (testMethod == null)
            {
                resultDisplay?.DisplayResult(testName, false, 0f, "Test not assigned");
                resultDisplay?.AppendLog($"[ERROR] {testName}: Test component not assigned");
                yield break;
            }

            resultDisplay?.AppendLog($"\n[START] {testName}...");
            float startTime = Time.realtimeSinceStartup;

            // Execute the test
            yield return testMethod();

            float duration = Time.realtimeSinceStartup - startTime;

            // Results will be displayed by individual test components
            resultDisplay?.AppendLog($"[DONE] {testName} completed in {duration:F2}s");
        }

        #endregion

        #region Unity Callbacks

        private void OnDestroy()
        {
            // Cleanup button listeners
            runCircularBufferBtn?.onClick.RemoveListener(RunCircularBufferTest);
            runHopLimitBtn?.onClick.RemoveListener(RunHopLimitTest);
            runCycleDetectionBtn?.onClick.RemoveListener(RunCycleDetectionTest);
            runLazyCopyBtn?.onClick.RemoveListener(RunLazyCopyTest);
            runAllTestsBtn?.onClick.RemoveListener(RunAllTests);
        }

        #endregion
    }
}
