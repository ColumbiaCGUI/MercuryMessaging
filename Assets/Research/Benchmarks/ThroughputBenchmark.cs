using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using MercuryMessaging;

namespace MercuryMessaging.Research.Benchmarks
{
    /// <summary>
    /// Sustained FPS under continuous messaging load.
    /// Compares Mercury spatial filtering vs manual Vector3.Distance.
    /// Coroutine-based measurement over configurable duration.
    /// </summary>
    public class ThroughputBenchmark : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Message rates to test (msg/sec)")]
        public int[] messageRates = { 100, 500, 1000 };

        [Tooltip("Receiver counts to test")]
        public int[] receiverCounts = { 50, 100 };

        [Tooltip("Duration per measurement (seconds)")]
        public float testDuration = 30f;

        [Tooltip("Spatial filter radius")]
        public float radius = 5f;

        [Tooltip("Placement range for receivers")]
        public float placementRange = 20f;

        [Header("Export")]
        public bool exportCsv = true;

        [Header("Runtime")]
        public bool isRunning;
        public string currentTest = "";

        private readonly List<Result> _results = new List<Result>();
        private GameObject _testRoot;

        private struct Result
        {
            public string Method;
            public int MessageRate;
            public int ReceiverCount;
            public float AvgFPS;
            public float MinFPS;
            public float MaxFPS;
            public int TotalSent;
            public float Duration;
        }

        [ContextMenu("Run Throughput Benchmark")]
        public void RunBenchmark()
        {
            if (isRunning) return;
            StartCoroutine(RunAll());
        }

        private IEnumerator RunAll()
        {
            isRunning = true;
            _results.Clear();

            bool prevPerfMode = MmRelayNode.PerformanceMode;
            MmRelayNode.PerformanceMode = true;

            Debug.Log("[ThroughputBenchmark] Starting...");

            foreach (int receivers in receiverCounts)
            {
                foreach (int rate in messageRates)
                {
                    // Mercury Within()
                    currentTest = $"Mercury {receivers}r {rate}msg/s";
                    Debug.Log($"[ThroughputBenchmark] {currentTest}");
                    yield return StartCoroutine(RunMercury(receivers, rate));
                    yield return Pause();

                    // Manual Distance
                    currentTest = $"Manual {receivers}r {rate}msg/s";
                    Debug.Log($"[ThroughputBenchmark] {currentTest}");
                    yield return StartCoroutine(RunManual(receivers, rate));
                    yield return Pause();
                }
            }

            MmRelayNode.PerformanceMode = prevPerfMode;
            PrintResults();
            if (exportCsv) ExportCsv();
            isRunning = false;
            currentTest = "Complete";
        }

        private IEnumerator Pause()
        {
            yield return new WaitForSeconds(1f);
            GC.Collect();
            yield return new WaitForSeconds(1f);
        }

        private IEnumerator RunMercury(int receiverCount, int messageRate)
        {
            var rng = new System.Random(42);
            _testRoot = new GameObject("MercuryRoot");
            _testRoot.transform.position = Vector3.zero;
            var rootRelay = _testRoot.AddComponent<MmRelayNode>();

            for (int i = 0; i < receiverCount; i++)
            {
                var go = new GameObject($"R_{i}");
                go.transform.SetParent(_testRoot.transform);
                go.transform.position = new Vector3(
                    (float)(rng.NextDouble() * 2 - 1) * placementRange,
                    (float)(rng.NextDouble() * 2 - 1) * placementRange,
                    (float)(rng.NextDouble() * 2 - 1) * placementRange);
                var relay = go.AddComponent<MmRelayNode>();
                go.AddComponent<ThroughputReceiver>();
                rootRelay.MmAddToRoutingTable(relay, MmLevelFilter.Child);
                relay.AddParent(rootRelay);
            }

            foreach (var r in _testRoot.GetComponentsInChildren<MmRelayNode>())
                r.MmRefreshResponders();

            yield return null;

            // Warm up
            for (int i = 0; i < 10; i++)
            {
                rootRelay.Send("w").ToAll().Within(radius).Execute();
                yield return null;
            }

            // Measure
            var result = new FPSMeasurement();
            float interval = 1f / messageRate;
            float accum = 0f;
            int totalSent = 0;

            while (result.elapsed < testDuration)
            {
                float dt = Time.unscaledDeltaTime;
                result.AddFrame(dt);

                accum += dt;
                int toSend = Mathf.FloorToInt(accum / interval);
                for (int m = 0; m < toSend; m++)
                {
                    rootRelay.Send("t").ToAll().Within(radius).Execute();
                    totalSent++;
                }
                accum -= toSend * interval;
                yield return null;
            }

            _results.Add(new Result
            {
                Method = "Mercury Within()",
                MessageRate = messageRate,
                ReceiverCount = receiverCount,
                AvgFPS = result.AvgFPS,
                MinFPS = result.MinFPS,
                MaxFPS = result.MaxFPS,
                TotalSent = totalSent,
                Duration = result.elapsed
            });

            DestroyImmediate(_testRoot);
            _testRoot = null;
        }

        private IEnumerator RunManual(int receiverCount, int messageRate)
        {
            var rng = new System.Random(42); // Same seed = same positions
            _testRoot = new GameObject("ManualRoot");
            _testRoot.transform.position = Vector3.zero;

            var receivers = new List<ThroughputEventReceiver>();
            for (int i = 0; i < receiverCount; i++)
            {
                var go = new GameObject($"R_{i}");
                go.transform.SetParent(_testRoot.transform);
                go.transform.position = new Vector3(
                    (float)(rng.NextDouble() * 2 - 1) * placementRange,
                    (float)(rng.NextDouble() * 2 - 1) * placementRange,
                    (float)(rng.NextDouble() * 2 - 1) * placementRange);
                receivers.Add(go.AddComponent<ThroughputEventReceiver>());
            }

            yield return null;

            Vector3 senderPos = _testRoot.transform.position;
            float r = radius;

            // Warm up
            for (int i = 0; i < 10; i++)
            {
                foreach (var recv in receivers)
                {
                    if (Vector3.Distance(senderPos, recv.transform.position) <= r)
                        recv.HandleMessage("w");
                }
                yield return null;
            }

            // Measure
            var result = new FPSMeasurement();
            float interval = 1f / messageRate;
            float accum = 0f;
            int totalSent = 0;

            while (result.elapsed < testDuration)
            {
                float dt = Time.unscaledDeltaTime;
                result.AddFrame(dt);

                accum += dt;
                int toSend = Mathf.FloorToInt(accum / interval);
                for (int m = 0; m < toSend; m++)
                {
                    // Use Vector3.Distance — matching what study participants write
                    foreach (var recv in receivers)
                    {
                        if (Vector3.Distance(senderPos, recv.transform.position) <= r)
                            recv.HandleMessage("t");
                    }
                    totalSent++;
                }
                accum -= toSend * interval;
                yield return null;
            }

            _results.Add(new Result
            {
                Method = "Manual Distance",
                MessageRate = messageRate,
                ReceiverCount = receiverCount,
                AvgFPS = result.AvgFPS,
                MinFPS = result.MinFPS,
                MaxFPS = result.MaxFPS,
                TotalSent = totalSent,
                Duration = result.elapsed
            });

            DestroyImmediate(_testRoot);
            _testRoot = null;
        }

        private void PrintResults()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== Throughput Benchmark ===");
            sb.AppendLine($"Duration: {testDuration}s, Radius: {radius}m");
            sb.AppendLine();

            sb.AppendLine($"{"Method",-22} {"Recv",5} {"Rate",6} {"AvgFPS",8} {"MinFPS",8} {"MaxFPS",8} {"Sent",8}");
            sb.AppendLine(new string('-', 68));

            foreach (var r in _results)
            {
                sb.AppendLine($"{r.Method,-22} {r.ReceiverCount,5} {r.MessageRate,6} " +
                              $"{r.AvgFPS,8:F1} {r.MinFPS,8:F1} {r.MaxFPS,8:F1} {r.TotalSent,8}");
            }

            // Paired comparisons
            sb.AppendLine();
            sb.AppendLine("--- Paired FPS Ratios ---");
            for (int i = 0; i < _results.Count - 1; i += 2)
            {
                var merc = _results[i];
                var manual = _results[i + 1];
                float ratio = manual.AvgFPS > 0 ? merc.AvgFPS / manual.AvgFPS : 0f;
                sb.AppendLine($"  {merc.ReceiverCount}r {merc.MessageRate}msg/s: " +
                              $"Mercury={merc.AvgFPS:F1} Manual={manual.AvgFPS:F1} " +
                              $"ratio={ratio:F2}x");
            }

            Debug.Log(sb.ToString());
        }

        private void ExportCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("method,receiver_count,message_rate,avg_fps,min_fps,max_fps,total_sent,duration_s");

            foreach (var r in _results)
            {
                sb.AppendLine($"{r.Method},{r.ReceiverCount},{r.MessageRate}," +
                              $"{r.AvgFPS:F2},{r.MinFPS:F2},{r.MaxFPS:F2}," +
                              $"{r.TotalSent},{r.Duration:F2}");
            }

            string path = Path.Combine(Application.dataPath,
                "Research", "Benchmarks", "Results", "throughput_benchmark.csv");
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, sb.ToString());
            Debug.Log($"[ThroughputBenchmark] CSV exported to: {path}");
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            if (_testRoot != null) DestroyImmediate(_testRoot);
        }

        private struct FPSMeasurement
        {
            public float elapsed;
            public int frames;
            public float minDt;
            public float maxDt;

            public void AddFrame(float dt)
            {
                elapsed += dt;
                frames++;
                if (dt > 0)
                {
                    if (minDt <= 0 || dt < minDt) minDt = dt;
                    if (dt > maxDt) maxDt = dt;
                }
            }

            public float AvgFPS => elapsed > 0 ? frames / elapsed : 0;
            public float MinFPS => maxDt > 0 ? 1f / maxDt : 0;
            public float MaxFPS => minDt > 0 ? 1f / minDt : 0;
        }
    }

    internal class ThroughputReceiver : MmBaseResponder
    {
        public int counter;
        protected override void ReceivedMessage(MmMessageString message) { counter++; }
    }

    internal class ThroughputEventReceiver : MonoBehaviour
    {
        public int counter;
        public void HandleMessage(string msg) { counter++; }
    }
}
