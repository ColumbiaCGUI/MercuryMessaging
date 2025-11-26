using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using TMPro;

namespace MercuryMessaging.Tests.Performance
{
    /// <summary>
    /// Performance comparison harness for Traditional vs DSL API testing.
    /// Tracks metrics for both hierarchies side-by-side and calculates overhead.
    ///
    /// Usage:
    /// 1. Create two identical hierarchies (Traditional_Hierarchy and DSL_Hierarchy)
    /// 2. Add MessageGenerator to Traditional_Hierarchy root
    /// 3. Add MessageGenerator_DSL to DSL_Hierarchy root
    /// 4. Add this harness to the scene root
    /// 5. Link both generators to this harness
    /// 6. Run test to collect comparative metrics
    /// </summary>
    public class ComparisonTestHarness : MonoBehaviour
    {
        #region Configuration

        [Header("Test Configuration")]
        [Tooltip("Test duration in seconds")]
        [Range(10, 300)]
        public float testDuration = 60f;

        [Tooltip("Start test automatically on scene load")]
        public bool autoStart = false;

        [Header("Generator References")]
        [Tooltip("Traditional API message generator")]
        public MessageGenerator traditionalGenerator;

        [Tooltip("DSL API message generator")]
        public MessageGenerator_DSL dslGenerator;

        [Header("Export Settings")]
        [Tooltip("CSV export path (relative to project dev/ folder)")]
        public string exportPath = "performance-results/dsl_comparison_results.csv";

        [Tooltip("Also export to dev folder")]
        public bool exportToDevFolder = true;

        [Header("UI Display")]
        [Tooltip("TextMeshPro text component for real-time display (optional, auto-finds if not set)")]
        public TMP_Text displayText;

        [Tooltip("Update UI every N frames")]
        [Range(1, 60)]
        public int uiUpdateInterval = 10;

        #endregion

        #region Private Fields

        private bool _isRunning = false;
        private float _startTime;
        private int _frameCount;
        private List<ComparisonMetrics> _metricsHistory = new List<ComparisonMetrics>();

        // Traditional message tracking
        private int _traditionalMessagesSent = 0;
        private int _lastTraditionalCount = 0;
        private float _traditionalThroughput = 0f;
        private long _traditionalTotalTicks = 0;

        // DSL message tracking
        private int _dslMessagesSent = 0;
        private int _lastDSLCount = 0;
        private float _dslThroughput = 0f;
        private long _dslTotalTicks = 0;

        // Timing tracking
        private float _lastThroughputCheck = 0f;

        // Frame time tracking (shared)
        private float _minFrameTime = float.MaxValue;
        private float _maxFrameTime = float.MinValue;
        private float _totalFrameTime = 0f;

        // Memory tracking
        private long _startMemory = 0;
        private long _currentMemory = 0;
        private int _memorySampleCounter = 0;

        // UI update tracking
        private int _uiFrameCounter = 0;

        #endregion

        #region Data Structures

        [Serializable]
        public struct ComparisonMetrics
        {
            public float timestamp;
            public float frameTime;
            public long memoryBytes;
            public int traditionalMessagesSent;
            public int dslMessagesSent;
            public float traditionalThroughput;
            public float dslThroughput;
            public float overheadPercent; // (DSL - Traditional) / Traditional * 100
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Enable Performance Mode to disable debug overhead during testing
            MmRelayNode.PerformanceMode = true;
            Debug.Log("[ComparisonTestHarness] Performance Mode enabled - debug tracking disabled");

            // Find generators if not set
            if (traditionalGenerator == null)
            {
                traditionalGenerator = FindObjectOfType<MessageGenerator>();
                if (traditionalGenerator != null)
                {
                    Debug.Log($"[ComparisonTestHarness] Found traditional generator: {traditionalGenerator.gameObject.name}");
                }
            }

            if (dslGenerator == null)
            {
                dslGenerator = FindObjectOfType<MessageGenerator_DSL>();
                if (dslGenerator != null)
                {
                    Debug.Log($"[ComparisonTestHarness] Found DSL generator: {dslGenerator.gameObject.name}");
                }
            }

            // Validate setup
            if (traditionalGenerator == null || dslGenerator == null)
            {
                Debug.LogWarning("[ComparisonTestHarness] Missing generator references! Need both Traditional and DSL generators.");
            }

            // Auto-find TMP_Text if not assigned
            if (displayText == null)
            {
                displayText = FindObjectOfType<TMP_Text>();
                if (displayText != null)
                {
                    Debug.Log($"[ComparisonTestHarness] Found display text: {displayText.gameObject.name}");
                }
            }

            // Auto-start if enabled
            if (autoStart)
            {
                StartTest();
            }
        }

        private void OnDestroy()
        {
            // Export results if test was running
            if (_isRunning && _metricsHistory.Count > 0)
            {
                ExportResults();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Start the comparison test.
        /// </summary>
        public void StartTest()
        {
            if (_isRunning)
            {
                Debug.LogWarning("[ComparisonTestHarness] Test already running!");
                return;
            }

            // Ensure generators have matching configurations
            if (traditionalGenerator != null && dslGenerator != null)
            {
                if (traditionalGenerator.messagesPerSecond != dslGenerator.messagesPerSecond)
                {
                    Debug.LogWarning($"[ComparisonTestHarness] Message rates differ! Traditional: {traditionalGenerator.messagesPerSecond}, DSL: {dslGenerator.messagesPerSecond}");
                }
                if (traditionalGenerator.messageMethod != dslGenerator.messageMethod)
                {
                    Debug.LogWarning($"[ComparisonTestHarness] Message methods differ! Traditional: {traditionalGenerator.messageMethod}, DSL: {dslGenerator.messageMethod}");
                }
            }

            Debug.Log("[ComparisonTestHarness] Starting Traditional vs DSL comparison test...");
            StartCoroutine(RunTest());
        }

        /// <summary>
        /// Stop the test early and export results.
        /// </summary>
        public void StopTest()
        {
            if (!_isRunning)
            {
                Debug.LogWarning("[ComparisonTestHarness] No test running!");
                return;
            }

            _isRunning = false;
            Debug.Log("[ComparisonTestHarness] Test stopped manually.");
        }

        /// <summary>
        /// Track a traditional message sent with timing data.
        /// Called by MessageGenerator.
        /// </summary>
        /// <param name="elapsedTicks">Stopwatch ticks for this message send</param>
        public void OnTraditionalMessageSent(long elapsedTicks)
        {
            _traditionalMessagesSent++;
            _traditionalTotalTicks += elapsedTicks;
        }

        /// <summary>
        /// Track a DSL message sent with timing data.
        /// Called by MessageGenerator_DSL.
        /// </summary>
        /// <param name="elapsedTicks">Stopwatch ticks for this message send</param>
        public void OnDSLMessageSent(long elapsedTicks)
        {
            _dslMessagesSent++;
            _dslTotalTicks += elapsedTicks;
        }

        #endregion

        #region Test Execution

        private IEnumerator RunTest()
        {
            _isRunning = true;
            _startTime = Time.time;
            _frameCount = 0;
            _traditionalMessagesSent = 0;
            _dslMessagesSent = 0;
            _metricsHistory.Clear();

            // Record starting memory
            GC.Collect();
            yield return null;
            _startMemory = GC.GetTotalMemory(false);

            Debug.Log($"[ComparisonTestHarness] Starting memory: {_startMemory / 1024f / 1024f:F2} MB");
            Debug.Log($"[ComparisonTestHarness] Test duration: {testDuration}s");

            // Run test for specified duration
            while (_isRunning && (Time.time - _startTime) < testDuration)
            {
                // Collect frame metrics
                CollectFrameMetrics();

                // Update UI periodically
                _uiFrameCounter++;
                if (_uiFrameCounter >= uiUpdateInterval)
                {
                    UpdateUI();
                    _uiFrameCounter = 0;
                }

                _frameCount++;
                yield return null;
            }

            _isRunning = false;

            // Final statistics
            CalculateFinalStatistics();

            // Export results
            ExportResults();

            Debug.Log("[ComparisonTestHarness] Test complete!");
        }

        private void CollectFrameMetrics()
        {
            float currentTime = Time.time - _startTime;

            // Frame time
            float frameTime = Time.deltaTime * 1000f;
            _totalFrameTime += frameTime;
            _minFrameTime = Mathf.Min(_minFrameTime, frameTime);
            _maxFrameTime = Mathf.Max(_maxFrameTime, frameTime);

            // Memory (sample every 60 frames)
            _memorySampleCounter++;
            if (_memorySampleCounter >= 60)
            {
                _currentMemory = GC.GetTotalMemory(false);
                _memorySampleCounter = 0;
            }

            // Throughput (calculate every second)
            if (currentTime - _lastThroughputCheck >= 1f)
            {
                float deltaTime = currentTime - _lastThroughputCheck;
                _traditionalThroughput = (_traditionalMessagesSent - _lastTraditionalCount) / deltaTime;
                _dslThroughput = (_dslMessagesSent - _lastDSLCount) / deltaTime;

                _lastTraditionalCount = _traditionalMessagesSent;
                _lastDSLCount = _dslMessagesSent;
                _lastThroughputCheck = currentTime;
            }

            // Calculate overhead based on per-message timing (not throughput!)
            // This is the correct way to measure API overhead
            float overheadPercent = 0f;
            if (_traditionalMessagesSent > 0 && _dslMessagesSent > 0)
            {
                // Calculate average ticks per message for each API
                double avgTraditionalTicks = (double)_traditionalTotalTicks / _traditionalMessagesSent;
                double avgDslTicks = (double)_dslTotalTicks / _dslMessagesSent;

                // Overhead = (DSL time - Traditional time) / Traditional time * 100
                // Positive = DSL is slower, Negative = DSL is faster
                if (avgTraditionalTicks > 0)
                {
                    overheadPercent = (float)((avgDslTicks - avgTraditionalTicks) / avgTraditionalTicks * 100.0);
                }
            }

            // Store metrics
            _metricsHistory.Add(new ComparisonMetrics
            {
                timestamp = currentTime,
                frameTime = frameTime,
                memoryBytes = _currentMemory,
                traditionalMessagesSent = _traditionalMessagesSent,
                dslMessagesSent = _dslMessagesSent,
                traditionalThroughput = _traditionalThroughput,
                dslThroughput = _dslThroughput,
                overheadPercent = overheadPercent
            });
        }

        private void CalculateFinalStatistics()
        {
            if (_metricsHistory.Count == 0)
            {
                Debug.LogWarning("[ComparisonTestHarness] No metrics collected!");
                return;
            }

            float avgFrameTime = _totalFrameTime / _frameCount;
            float testDurationActual = Time.time - _startTime;
            long memoryGrowth = _currentMemory - _startMemory;

            // Calculate average overhead
            float totalOverhead = 0f;
            int overheadCount = 0;
            foreach (var m in _metricsHistory)
            {
                if (m.traditionalThroughput > 0 && m.dslThroughput > 0)
                {
                    totalOverhead += m.overheadPercent;
                    overheadCount++;
                }
            }
            float avgOverhead = overheadCount > 0 ? totalOverhead / overheadCount : 0f;

            // Calculate average throughputs
            float avgTraditionalThroughput = testDurationActual > 0 ? _traditionalMessagesSent / testDurationActual : 0f;
            float avgDSLThroughput = testDurationActual > 0 ? _dslMessagesSent / testDurationActual : 0f;

            Debug.Log("[ComparisonTestHarness] ========== COMPARISON RESULTS ==========");
            Debug.Log($"Test Duration: {testDurationActual:F2}s");
            Debug.Log($"Total Frames: {_frameCount}");
            Debug.Log("");
            Debug.Log("--- Frame Performance ---");
            Debug.Log($"Avg Frame Time: {avgFrameTime:F2}ms");
            Debug.Log($"FPS: {1000f / avgFrameTime:F1} avg");
            Debug.Log($"Min/Max: {_minFrameTime:F2}ms / {_maxFrameTime:F2}ms");
            Debug.Log("");
            Debug.Log("--- Message Throughput ---");
            Debug.Log($"Traditional: {_traditionalMessagesSent} total ({avgTraditionalThroughput:F1} msg/sec)");
            Debug.Log($"DSL:         {_dslMessagesSent} total ({avgDSLThroughput:F1} msg/sec)");
            Debug.Log("");
            Debug.Log("--- DSL Overhead ---");
            Debug.Log($"Average Overhead: {avgOverhead:F1}%");
            Debug.Log($"(Positive = DSL slower, Negative = DSL faster)");
            Debug.Log("");
            Debug.Log("--- Memory ---");
            Debug.Log($"Start: {_startMemory / 1024f / 1024f:F2} MB");
            Debug.Log($"End:   {_currentMemory / 1024f / 1024f:F2} MB");
            Debug.Log($"Growth: {memoryGrowth / 1024f / 1024f:F2} MB");
            Debug.Log("===========================================");
        }

        #endregion

        #region UI Update

        private void UpdateUI()
        {
            if (displayText == null) return;

            float currentTime = Time.time - _startTime;
            float progress = Mathf.Clamp01(currentTime / testDuration);
            float avgFrameTime = _frameCount > 0 ? _totalFrameTime / _frameCount : 0f;
            float currentFps = Time.deltaTime > 0 ? 1f / Time.deltaTime : 0f;

            // Calculate current overhead based on per-message timing
            float currentOverhead = 0f;
            double avgTraditionalNs = 0;
            double avgDslNs = 0;
            if (_traditionalMessagesSent > 0 && _dslMessagesSent > 0)
            {
                double avgTraditionalTicks = (double)_traditionalTotalTicks / _traditionalMessagesSent;
                double avgDslTicks = (double)_dslTotalTicks / _dslMessagesSent;
                // Convert ticks to nanoseconds
                double ticksPerNs = System.Diagnostics.Stopwatch.Frequency / 1_000_000_000.0;
                avgTraditionalNs = avgTraditionalTicks / ticksPerNs;
                avgDslNs = avgDslTicks / ticksPerNs;

                if (avgTraditionalTicks > 0)
                {
                    currentOverhead = (float)((avgDslTicks - avgTraditionalTicks) / avgTraditionalTicks * 100.0);
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<b>DSL Comparison Test</b>");
            sb.AppendLine($"Progress: {progress * 100f:F1}% ({currentTime:F1}s / {testDuration:F1}s)");
            sb.AppendLine();

            sb.AppendLine("<b>Frame Performance:</b>");
            sb.AppendLine($"  Current FPS: {currentFps:F1}");
            sb.AppendLine($"  Avg Frame Time: {avgFrameTime:F2}ms");
            sb.AppendLine();

            sb.AppendLine("<b>Traditional API:</b>");
            sb.AppendLine($"  Messages: {_traditionalMessagesSent}");
            sb.AppendLine($"  Avg Time: {avgTraditionalNs:F0} ns/msg");
            sb.AppendLine($"  Throughput: {_traditionalThroughput:F1} msg/sec");
            sb.AppendLine();

            sb.AppendLine("<b>DSL API:</b>");
            sb.AppendLine($"  Messages: {_dslMessagesSent}");
            sb.AppendLine($"  Avg Time: {avgDslNs:F0} ns/msg");
            sb.AppendLine($"  Throughput: {_dslThroughput:F1} msg/sec");
            sb.AppendLine();

            sb.AppendLine("<b>DSL Overhead:</b>");
            sb.AppendLine($"  {currentOverhead:F1}%");
            sb.AppendLine($"  ({(currentOverhead > 0 ? "DSL slower" : "DSL faster")})");
            sb.AppendLine();

            sb.AppendLine("<b>Memory:</b>");
            sb.AppendLine($"  Current: {_currentMemory / 1024f / 1024f:F2} MB");
            sb.AppendLine($"  Growth: {(_currentMemory - _startMemory) / 1024f / 1024f:F2} MB");

            displayText.text = sb.ToString();
        }

        #endregion

        #region Export

        private void ExportResults()
        {
            if (_metricsHistory.Count == 0)
            {
                Debug.LogWarning("[ComparisonTestHarness] No data to export!");
                return;
            }

            // Generate CSV content
            StringBuilder csv = new StringBuilder();

            // Header
            csv.AppendLine("timestamp,frame_time_ms,memory_bytes,memory_mb," +
                          "traditional_messages,dsl_messages," +
                          "traditional_throughput,dsl_throughput," +
                          "overhead_percent");

            // Data rows
            foreach (var metrics in _metricsHistory)
            {
                csv.AppendLine($"{metrics.timestamp:F3}," +
                             $"{metrics.frameTime:F3}," +
                             $"{metrics.memoryBytes}," +
                             $"{metrics.memoryBytes / 1024f / 1024f:F2}," +
                             $"{metrics.traditionalMessagesSent}," +
                             $"{metrics.dslMessagesSent}," +
                             $"{metrics.traditionalThroughput:F2}," +
                             $"{metrics.dslThroughput:F2}," +
                             $"{metrics.overheadPercent:F2}");
            }

            // Export to dev folder (moved out of Assets for build size optimization)
            try
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string resourcesPath = Path.Combine(projectRoot, "dev", exportPath);
                string directory = Path.GetDirectoryName(resourcesPath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(resourcesPath, csv.ToString());
                Debug.Log($"[ComparisonTestHarness] Exported to: {resourcesPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ComparisonTestHarness] Export failed: {e.Message}");
            }

            // Also export to dev folder if enabled
            if (exportToDevFolder)
            {
                try
                {
                    string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                    string devPath = Path.Combine(projectRoot, "dev", "active", "performance-analysis",
                                                  Path.GetFileName(exportPath));
                    string devDirectory = Path.GetDirectoryName(devPath);

                    if (!Directory.Exists(devDirectory))
                    {
                        Directory.CreateDirectory(devDirectory);
                    }

                    File.WriteAllText(devPath, csv.ToString());
                    Debug.Log($"[ComparisonTestHarness] Also exported to: {devPath}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ComparisonTestHarness] Dev folder export failed: {e.Message}");
                }
            }

            // Export summary
            ExportSummary();
        }

        private void ExportSummary()
        {
            float testDurationActual = Time.time - _startTime;
            float avgFrameTime = _frameCount > 0 ? _totalFrameTime / _frameCount : 0f;
            float avgTraditionalThroughput = testDurationActual > 0 ? _traditionalMessagesSent / testDurationActual : 0f;
            float avgDSLThroughput = testDurationActual > 0 ? _dslMessagesSent / testDurationActual : 0f;

            // Calculate average overhead
            float totalOverhead = 0f;
            int overheadCount = 0;
            foreach (var m in _metricsHistory)
            {
                if (m.traditionalThroughput > 0 && m.dslThroughput > 0)
                {
                    totalOverhead += m.overheadPercent;
                    overheadCount++;
                }
            }
            float avgOverhead = overheadCount > 0 ? totalOverhead / overheadCount : 0f;

            StringBuilder summary = new StringBuilder();
            summary.AppendLine("metric,value");
            summary.AppendLine($"test_duration_seconds,{testDurationActual:F2}");
            summary.AppendLine($"total_frames,{_frameCount}");
            summary.AppendLine($"avg_frame_time_ms,{avgFrameTime:F2}");
            summary.AppendLine($"avg_fps,{1000f / avgFrameTime:F1}");
            summary.AppendLine($"traditional_total_messages,{_traditionalMessagesSent}");
            summary.AppendLine($"dsl_total_messages,{_dslMessagesSent}");
            summary.AppendLine($"traditional_avg_throughput,{avgTraditionalThroughput:F2}");
            summary.AppendLine($"dsl_avg_throughput,{avgDSLThroughput:F2}");
            summary.AppendLine($"avg_overhead_percent,{avgOverhead:F2}");
            summary.AppendLine($"memory_start_mb,{_startMemory / 1024f / 1024f:F2}");
            summary.AppendLine($"memory_end_mb,{_currentMemory / 1024f / 1024f:F2}");
            summary.AppendLine($"memory_growth_mb,{(_currentMemory - _startMemory) / 1024f / 1024f:F2}");

            try
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string summaryPath = Path.Combine(projectRoot, "dev",
                    Path.GetDirectoryName(exportPath),
                    "dsl_comparison_summary.csv");
                File.WriteAllText(summaryPath, summary.ToString());
                Debug.Log($"[ComparisonTestHarness] Summary exported to: {summaryPath}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ComparisonTestHarness] Summary export failed: {e.Message}");
            }
        }

        #endregion

        #region Editor Buttons

        [ContextMenu("Start Test")]
        private void StartTestFromMenu()
        {
            StartTest();
        }

        [ContextMenu("Stop Test")]
        private void StopTestFromMenu()
        {
            StopTest();
        }

        [ContextMenu("Export Results Now")]
        private void ExportResultsFromMenu()
        {
            ExportResults();
        }

        #endregion
    }
}
