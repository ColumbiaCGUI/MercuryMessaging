using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using MercuryMessaging;

namespace MercuryMessaging.Research.Benchmarks
{
    /// <summary>
    /// Measures sustained FPS under load comparing Mercury spatial filtering
    /// (Within()) vs UnityEvent with manual Vector3.Distance filtering.
    /// Configurable message rate, responder count, and test duration.
    /// </summary>
    public class FPSComparisonBenchmark : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Message rates to test (msg/sec)")]
        public int[] messageRates = { 100, 500, 1000 };

        [Tooltip("Responder counts to test")]
        public int[] responderCounts = { 50, 100 };

        [Tooltip("Duration of each measurement in seconds")]
        public float testDuration = 60f;

        [Tooltip("Spatial filter radius in meters")]
        public float filterRadius = 5f;

        [Tooltip("Spread radius for placing responders")]
        public float responderSpreadRadius = 20f;

        [Header("Runtime Info")]
        [Tooltip("True while benchmark is running")]
        public bool isRunning;

        [Tooltip("Current test description")]
        public string currentTest = "";

        private readonly List<FPSResult> _results = new List<FPSResult>();
        private GameObject _testRoot;

        private struct FPSResult
        {
            public string Method;
            public int MessageRate;
            public int ResponderCount;
            public float AvgFPS;
            public float MinFPS;
            public float MaxFPS;
            public float Duration;
            public int TotalMessagesSent;
        }

        [ContextMenu("Run FPS Benchmark")]
        public void RunFPSBenchmark()
        {
            if (isRunning)
            {
                Debug.LogWarning("[FPSComparisonBenchmark] Benchmark already running.");
                return;
            }

            StartCoroutine(RunAllTests());
        }

        private IEnumerator RunAllTests()
        {
            isRunning = true;
            _results.Clear();

            Debug.Log("[FPSComparisonBenchmark] Starting FPS comparison...");

            foreach (int responders in responderCounts)
            {
                foreach (int rate in messageRates)
                {
                    // Mercury Within() test
                    currentTest = $"Mercury Within() — {responders} resp, {rate} msg/s";
                    Debug.Log($"[FPSComparisonBenchmark] Running: {currentTest}");
                    yield return StartCoroutine(RunMercuryTest(responders, rate));

                    // Brief pause between tests for GC
                    yield return new WaitForSeconds(2f);
                    GC.Collect();
                    yield return new WaitForSeconds(1f);

                    // UnityEvent + Distance test
                    currentTest = $"UnityEvent+Distance — {responders} resp, {rate} msg/s";
                    Debug.Log($"[FPSComparisonBenchmark] Running: {currentTest}");
                    yield return StartCoroutine(RunUnityEventTest(responders, rate));

                    // Pause between test pairs
                    yield return new WaitForSeconds(2f);
                    GC.Collect();
                    yield return new WaitForSeconds(1f);
                }
            }

            PrintResults();
            isRunning = false;
            currentTest = "Complete";
        }

        private IEnumerator RunMercuryTest(int responderCount, int messageRate)
        {
            // Build Mercury hierarchy
            _testRoot = new GameObject("MercuryTestRoot");
            _testRoot.transform.position = Vector3.zero;
            var rootRelay = _testRoot.AddComponent<MmRelayNode>();

            // Create responders at random positions
            for (int i = 0; i < responderCount; i++)
            {
                var responder = new GameObject($"MercuryResp_{i}");
                responder.transform.SetParent(_testRoot.transform);
                responder.transform.position = UnityEngine.Random.insideUnitSphere * responderSpreadRadius;
                responder.AddComponent<MmRelayNode>();
                responder.AddComponent<BenchmarkFPSReceiver>();

                var parentRelay = _testRoot.GetComponent<MmRelayNode>();
                var childRelay = responder.GetComponent<MmRelayNode>();
                parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
                childRelay.AddParent(parentRelay);
            }

            rootRelay.MmRefreshResponders();

            // Wait a frame for setup
            yield return null;

            // Measure FPS
            float elapsed = 0f;
            int frameCount = 0;
            float minFrameTime = float.MaxValue;
            float maxFrameTime = 0f;
            float messageAccumulator = 0f;
            float messageInterval = 1f / messageRate;
            int totalSent = 0;

            // Warm up for 1 second
            float warmUp = 0f;
            while (warmUp < 1f)
            {
                rootRelay.Send("bench").ToAll().Within(filterRadius).Execute();
                warmUp += Time.unscaledDeltaTime;
                yield return null;
            }

            // Actual measurement
            while (elapsed < testDuration)
            {
                float dt = Time.unscaledDeltaTime;
                elapsed += dt;
                frameCount++;

                if (dt > 0)
                {
                    if (dt < minFrameTime) minFrameTime = dt;
                    if (dt > maxFrameTime) maxFrameTime = dt;
                }

                // Send messages at configured rate
                messageAccumulator += dt;
                int messagesToSend = Mathf.FloorToInt(messageAccumulator / messageInterval);
                for (int m = 0; m < messagesToSend; m++)
                {
                    rootRelay.Send("bench").ToAll().Within(filterRadius).Execute();
                    totalSent++;
                }
                messageAccumulator -= messagesToSend * messageInterval;

                yield return null;
            }

            float avgFPS = frameCount / elapsed;
            float minFPS = minFrameTime > 0 ? 1f / maxFrameTime : 0f; // min FPS = worst frame
            float maxFPS = maxFrameTime > 0 ? 1f / minFrameTime : 0f; // max FPS = best frame

            _results.Add(new FPSResult
            {
                Method = "Mercury Within()",
                MessageRate = messageRate,
                ResponderCount = responderCount,
                AvgFPS = avgFPS,
                MinFPS = minFPS,
                MaxFPS = maxFPS,
                Duration = elapsed,
                TotalMessagesSent = totalSent
            });

            // Cleanup
            DestroyImmediate(_testRoot);
            _testRoot = null;
        }

        private IEnumerator RunUnityEventTest(int responderCount, int messageRate)
        {
            // Build flat hierarchy with UnityEvents
            _testRoot = new GameObject("EventTestRoot");
            _testRoot.transform.position = Vector3.zero;

            var receivers = new List<EventBenchmarkReceiver>();

            for (int i = 0; i < responderCount; i++)
            {
                var responder = new GameObject($"EventResp_{i}");
                responder.transform.SetParent(_testRoot.transform);
                responder.transform.position = UnityEngine.Random.insideUnitSphere * responderSpreadRadius;
                var receiver = responder.AddComponent<EventBenchmarkReceiver>();
                receivers.Add(receiver);
            }

            // Wait a frame for setup
            yield return null;

            Transform senderTransform = _testRoot.transform;

            // Measure FPS
            float elapsed = 0f;
            int frameCount = 0;
            float minFrameTime = float.MaxValue;
            float maxFrameTime = 0f;
            float messageAccumulator = 0f;
            float messageInterval = 1f / messageRate;
            int totalSent = 0;
            float radiusSq = filterRadius * filterRadius;

            // Warm up for 1 second
            float warmUp = 0f;
            while (warmUp < 1f)
            {
                Vector3 senderPos = senderTransform.position;
                foreach (var recv in receivers)
                {
                    float distSq = (recv.transform.position - senderPos).sqrMagnitude;
                    if (distSq <= radiusSq)
                        recv.OnMessage("bench");
                }
                warmUp += Time.unscaledDeltaTime;
                yield return null;
            }

            // Actual measurement
            while (elapsed < testDuration)
            {
                float dt = Time.unscaledDeltaTime;
                elapsed += dt;
                frameCount++;

                if (dt > 0)
                {
                    if (dt < minFrameTime) minFrameTime = dt;
                    if (dt > maxFrameTime) maxFrameTime = dt;
                }

                // Send messages at configured rate with distance check
                messageAccumulator += dt;
                int messagesToSend = Mathf.FloorToInt(messageAccumulator / messageInterval);
                for (int m = 0; m < messagesToSend; m++)
                {
                    Vector3 senderPos = senderTransform.position;
                    foreach (var recv in receivers)
                    {
                        float distSq = (recv.transform.position - senderPos).sqrMagnitude;
                        if (distSq <= radiusSq)
                            recv.OnMessage("bench");
                    }
                    totalSent++;
                }
                messageAccumulator -= messagesToSend * messageInterval;

                yield return null;
            }

            float avgFPS = frameCount / elapsed;
            float minFPS = minFrameTime > 0 ? 1f / maxFrameTime : 0f;
            float maxFPS = maxFrameTime > 0 ? 1f / minFrameTime : 0f;

            _results.Add(new FPSResult
            {
                Method = "UnityEvent+Distance",
                MessageRate = messageRate,
                ResponderCount = responderCount,
                AvgFPS = avgFPS,
                MinFPS = minFPS,
                MaxFPS = maxFPS,
                Duration = elapsed,
                TotalMessagesSent = totalSent
            });

            // Cleanup
            DestroyImmediate(_testRoot);
            _testRoot = null;
        }

        private void PrintResults()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== FPS Comparison Benchmark Results ===");
            sb.AppendLine($"Test duration: {testDuration}s per measurement");
            sb.AppendLine($"Filter radius: {filterRadius}m");
            sb.AppendLine($"Responder spread: {responderSpreadRadius}m");
            sb.AppendLine();

            // Header
            sb.AppendLine($"{"Method",-22} {"Resp",5} {"Rate",6} {"Avg FPS",9} {"Min FPS",9} {"Max FPS",9} {"Msgs Sent",10}");
            sb.AppendLine(new string('-', 72));

            foreach (var r in _results)
            {
                sb.AppendLine($"{r.Method,-22} {r.ResponderCount,5} {r.MessageRate,6} {r.AvgFPS,9:F1} {r.MinFPS,9:F1} {r.MaxFPS,9:F1} {r.TotalMessagesSent,10}");
            }

            sb.AppendLine();

            // Paired comparisons
            sb.AppendLine("--- Paired Comparisons ---");
            for (int i = 0; i < _results.Count - 1; i += 2)
            {
                var mercury = _results[i];
                var events = _results[i + 1];
                float fpsRatio = events.AvgFPS > 0 ? mercury.AvgFPS / events.AvgFPS : 0f;
                sb.AppendLine($"  {mercury.ResponderCount} resp, {mercury.MessageRate} msg/s: " +
                              $"Mercury={mercury.AvgFPS:F1} FPS, Events={events.AvgFPS:F1} FPS " +
                              $"(ratio: {fpsRatio:F2}x)");
            }

            sb.AppendLine();
            Debug.Log(sb.ToString());
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            if (_testRoot != null)
            {
                DestroyImmediate(_testRoot);
                _testRoot = null;
            }
        }
    }

    /// <summary>
    /// Minimal Mercury responder for FPS benchmarks.
    /// </summary>
    internal class BenchmarkFPSReceiver : MmBaseResponder
    {
        protected override void ReceivedMessage(MmMessageString message) { }
    }

    /// <summary>
    /// Minimal receiver for UnityEvent/distance FPS benchmarks.
    /// </summary>
    internal class EventBenchmarkReceiver : MonoBehaviour
    {
        public void OnMessage(string message) { }
    }
}
