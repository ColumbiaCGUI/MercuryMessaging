using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
#if UNITY_TEXTMESHPRO
using TMPro;
#endif

namespace MercuryMessaging.Tests.Performance
{
    /// <summary>
    /// Automated performance testing harness for MercuryMessaging framework.
    /// Collects quantitative metrics for Quick Win optimizations analysis.
    /// </summary>
    public class PerformanceTestHarness : MonoBehaviour
    {
        #region Configuration

        [Header("Test Configuration")]
        [Tooltip("Test scenario to run")]
        public TestScenario testScenario = TestScenario.Medium;

        [Tooltip("Number of responders (overrides scenario default)")]
        [Range(10, 200)]
        public int responderCount = 50;

        [Tooltip("Hierarchy depth (overrides scenario default)")]
        [Range(3, 10)]
        public int hierarchyDepth = 5;

        [Tooltip("Messages per second to send")]
        [Range(10, 1000)]
        public int messageVolume = 500;

        [Tooltip("Test duration in seconds")]
        [Range(10, 300)]
        public float testDuration = 60f;

        [Tooltip("Start test automatically on scene load")]
        public bool autoStart = false;

        [Header("Export Settings")]
        [Tooltip("CSV export path (relative to project dev/ folder)")]
        public string exportPath = "performance-results/test_results.csv";

        [Tooltip("Also export to dev/active/performance-analysis/")]
        public bool exportToDevFolder = true;

        [Header("UI Display")]
        [Tooltip("TextMeshPro text component for real-time display (optional)")]
#if UNITY_TEXTMESHPRO
        public TMP_Text displayText;
#else
        public UnityEngine.UI.Text displayText;
#endif

        [Tooltip("Update UI every N frames (reduces overhead)")]
        [Range(1, 60)]
        public int uiUpdateInterval = 10;

        [Header("References")]
        [Tooltip("Relay node to monitor")]
        public MmRelayNode relayNode;

        [Tooltip("Routing table to monitor for cache metrics")]
        public MmRoutingTable routingTable;

        #endregion

        #region Enums

        public enum TestScenario
        {
            Small,      // 10 responders, 3 levels, 100 msg/sec
            Medium,     // 50 responders, 5 levels, 500 msg/sec
            Large       // 100+ responders, 7-10 levels, 1000 msg/sec
        }

        #endregion

        #region Private Fields

        private bool _isRunning = false;
        private float _startTime;
        private int _frameCount;
        private List<FrameMetrics> _metricsHistory = new List<FrameMetrics>();

        // Message tracking
        private int _messagesSent = 0;
        private int _lastMessageCount = 0;
        private float _lastThroughputCheck = 0f;
        private float _currentThroughput = 0f;

        // Memory tracking
        private long _startMemory = 0;
        private long _currentMemory = 0;
        private int _memorySampleCounter = 0;

        // Cache tracking
        private float _lastCacheHitRate = 0f;

        // Frame time tracking
        private float _minFrameTime = float.MaxValue;
        private float _maxFrameTime = float.MinValue;
        private float _totalFrameTime = 0f;

        // UI update tracking
        private int _uiFrameCounter = 0;

        #endregion

        #region Data Structures

        [Serializable]
        public struct FrameMetrics
        {
            public float timestamp;
            public float frameTime;
            public long memoryBytes;
            public float throughput;
            public float cacheHitRate;
            public float avgHopCount;
            public int messagesSent;
        }

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Enable Performance Mode to disable debug overhead during testing
            MmRelayNode.PerformanceMode = true;
            Debug.Log("[PerformanceTestHarness] Performance Mode enabled - debug tracking disabled");

            // Apply scenario defaults if needed
            ApplyScenarioDefaults();

            // Find references if not set
            if (relayNode == null)
            {
                relayNode = GetComponent<MmRelayNode>();
                if (relayNode == null)
                {
                    Debug.LogWarning("[PerformanceTestHarness] No MmRelayNode found. Some metrics unavailable.");
                }
            }

            if (routingTable == null && relayNode != null)
            {
                routingTable = relayNode.RoutingTable;
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
        /// Start the performance test.
        /// </summary>
        public void StartTest()
        {
            if (_isRunning)
            {
                Debug.LogWarning("[PerformanceTestHarness] Test already running!");
                return;
            }

            Debug.Log($"[PerformanceTestHarness] Starting test: {testScenario} " +
                     $"({responderCount} responders, {hierarchyDepth} levels, {messageVolume} msg/sec, {testDuration}s)");

            StartCoroutine(RunTest());
        }

        /// <summary>
        /// Stop the test early and export results.
        /// </summary>
        public void StopTest()
        {
            if (!_isRunning)
            {
                Debug.LogWarning("[PerformanceTestHarness] No test running!");
                return;
            }

            _isRunning = false;
            Debug.Log("[PerformanceTestHarness] Test stopped manually.");
        }

        /// <summary>
        /// Track a sent message (call this from message sender).
        /// </summary>
        public void OnMessageSent()
        {
            _messagesSent++;
        }

        #endregion

        #region Test Execution

        private IEnumerator RunTest()
        {
            _isRunning = true;
            _startTime = Time.time;
            _frameCount = 0;
            _messagesSent = 0;
            _metricsHistory.Clear();

            // Record starting memory
            GC.Collect();
            yield return null;
            _startMemory = GC.GetTotalMemory(false);

            Debug.Log($"[PerformanceTestHarness] Starting memory: {_startMemory / 1024f / 1024f:F2} MB");

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

            Debug.Log("[PerformanceTestHarness] Test complete!");
        }

        private void CollectFrameMetrics()
        {
            float currentTime = Time.time - _startTime;

            // Frame time
            float frameTime = Time.deltaTime * 1000f; // Convert to milliseconds
            _totalFrameTime += frameTime;
            _minFrameTime = Mathf.Min(_minFrameTime, frameTime);
            _maxFrameTime = Mathf.Max(_maxFrameTime, frameTime);

            // Memory (sample every 60 frames to reduce GC.GetTotalMemory overhead)
            _memorySampleCounter++;
            if (_memorySampleCounter >= 60)
            {
                _currentMemory = GC.GetTotalMemory(false);
                _memorySampleCounter = 0;
            }

            // Throughput (calculate every second)
            if (currentTime - _lastThroughputCheck >= 1f)
            {
                _currentThroughput = (_messagesSent - _lastMessageCount) / (currentTime - _lastThroughputCheck);
                _lastMessageCount = _messagesSent;
                _lastThroughputCheck = currentTime;
            }

            // Cache hit rate
            if (routingTable != null)
            {
                _lastCacheHitRate = routingTable.CacheHitRate;
            }

            // Average hop count (would need instrumentation in MmMessage)
            float avgHopCount = 0f; // Placeholder - requires message history inspection

            // Store metrics
            _metricsHistory.Add(new FrameMetrics
            {
                timestamp = currentTime,
                frameTime = frameTime,
                memoryBytes = _currentMemory,
                throughput = _currentThroughput,
                cacheHitRate = _lastCacheHitRate,
                avgHopCount = avgHopCount,
                messagesSent = _messagesSent
            });
        }

        private void CalculateFinalStatistics()
        {
            if (_metricsHistory.Count == 0)
            {
                Debug.LogWarning("[PerformanceTestHarness] No metrics collected!");
                return;
            }

            float avgFrameTime = _totalFrameTime / _frameCount;
            float testDurationActual = Time.time - _startTime;
            long memoryGrowth = _currentMemory - _startMemory;

            Debug.Log("[PerformanceTestHarness] === FINAL STATISTICS ===");
            Debug.Log($"Test Duration: {testDurationActual:F2}s");
            Debug.Log($"Total Frames: {_frameCount}");
            Debug.Log($"Total Messages: {_messagesSent}");
            Debug.Log($"Frame Time: avg={avgFrameTime:F2}ms, min={_minFrameTime:F2}ms, max={_maxFrameTime:F2}ms");
            Debug.Log($"FPS: avg={1000f / avgFrameTime:F1}, min={1000f / _maxFrameTime:F1}, max={1000f / _minFrameTime:F1}");
            Debug.Log($"Memory: start={_startMemory / 1024f / 1024f:F2}MB, end={_currentMemory / 1024f / 1024f:F2}MB, growth={memoryGrowth / 1024f / 1024f:F2}MB");
            Debug.Log($"Message Throughput: {_messagesSent / testDurationActual:F1} msg/sec average");

            if (routingTable != null)
            {
                Debug.Log($"Cache Hit Rate: {_lastCacheHitRate * 100f:F1}%");
            }

            Debug.Log("=============================");
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

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>Performance Test: {testScenario}</b>");
            sb.AppendLine($"Progress: {progress * 100f:F1}% ({currentTime:F1}s / {testDuration:F1}s)");
            sb.AppendLine();
            sb.AppendLine($"<b>Frame Performance:</b>");
            sb.AppendLine($"  Current FPS: {currentFps:F1}");
            sb.AppendLine($"  Avg Frame Time: {avgFrameTime:F2}ms");
            sb.AppendLine($"  Min/Max: {_minFrameTime:F2}ms / {_maxFrameTime:F2}ms");
            sb.AppendLine();
            sb.AppendLine($"<b>Memory:</b>");
            sb.AppendLine($"  Current: {_currentMemory / 1024f / 1024f:F2} MB");
            sb.AppendLine($"  Growth: {(_currentMemory - _startMemory) / 1024f / 1024f:F2} MB");
            sb.AppendLine();
            sb.AppendLine($"<b>Messages:</b>");
            sb.AppendLine($"  Total Sent: {_messagesSent}");
            sb.AppendLine($"  Throughput: {_currentThroughput:F1} msg/sec");

            if (routingTable != null)
            {
                sb.AppendLine();
                sb.AppendLine($"<b>Cache Performance:</b>");
                sb.AppendLine($"  Hit Rate: {_lastCacheHitRate * 100f:F1}%");
            }

            displayText.text = sb.ToString();
        }

        #endregion

        #region Export

        private void ExportResults()
        {
            if (_metricsHistory.Count == 0)
            {
                Debug.LogWarning("[PerformanceTestHarness] No data to export!");
                return;
            }

            // Generate CSV content
            StringBuilder csv = new StringBuilder();

            // Header
            csv.AppendLine("timestamp,frame_time_ms,memory_bytes,memory_mb,throughput_msg_sec,cache_hit_rate,avg_hop_count,messages_sent");

            // Data rows
            foreach (var metrics in _metricsHistory)
            {
                csv.AppendLine($"{metrics.timestamp:F3}," +
                             $"{metrics.frameTime:F3}," +
                             $"{metrics.memoryBytes}," +
                             $"{metrics.memoryBytes / 1024f / 1024f:F2}," +
                             $"{metrics.throughput:F2}," +
                             $"{metrics.cacheHitRate:F4}," +
                             $"{metrics.avgHopCount:F2}," +
                             $"{metrics.messagesSent}");
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
                Debug.Log($"[PerformanceTestHarness] Exported to: {resourcesPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[PerformanceTestHarness] Export failed: {e.Message}");
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
                    Debug.Log($"[PerformanceTestHarness] Also exported to: {devPath}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[PerformanceTestHarness] Dev folder export failed: {e.Message}");
                }
            }
        }

        #endregion

        #region Helper Methods

        private void ApplyScenarioDefaults()
        {
            // Apply defaults based on scenario (can be overridden via inspector)
            switch (testScenario)
            {
                case TestScenario.Small:
                    if (responderCount == 50) responderCount = 10; // Default was 50
                    if (hierarchyDepth == 5) hierarchyDepth = 3;
                    if (messageVolume == 500) messageVolume = 100;
                    break;

                case TestScenario.Medium:
                    // Use inspector defaults (50, 5, 500)
                    break;

                case TestScenario.Large:
                    if (responderCount == 50) responderCount = 100;
                    if (hierarchyDepth == 5) hierarchyDepth = 7;
                    if (messageVolume == 500) messageVolume = 1000;
                    break;
            }
        }

        #endregion

        #region Editor Buttons (for testing in Editor)

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
