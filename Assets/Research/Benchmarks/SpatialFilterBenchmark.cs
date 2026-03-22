using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using MercuryMessaging;
using Debug = UnityEngine.Debug;

namespace MercuryMessaging.Research.Benchmarks
{
    /// <summary>
    /// Benchmarks Mercury's .Within() spatial filtering against the manual
    /// Vector3.Distance loop that study participants write in the Events condition.
    /// Uses FIXED receiver positions for reproducibility.
    /// </summary>
    public class SpatialFilterBenchmark : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Invocations per measurement")]
        public int iterations = 10000;

        [Tooltip("Warm-up invocations")]
        public int warmUp = 100;

        [Tooltip("Receiver counts to test")]
        public int[] receiverCounts = { 10, 50, 100 };

        [Tooltip("Spatial filter radius")]
        public float radius = 5f;

        [Tooltip("Max placement range for receivers")]
        public float placementRange = 20f;

        [Tooltip("Runs per configuration for mean +/- SD")]
        public int runs = 3;

        [Header("Export")]
        public bool exportCsv = true;

        [Header("Runtime")]
        public bool isRunning;

        private readonly List<Result> _results = new List<Result>();

        private struct Result
        {
            public string Method;
            public int ReceiverCount;
            public int ReceiversInRange;
            public int Run;
            public int Iterations;
            public double TotalMs;
            public double AvgUs;
            public long AvgTicks;
            public long MemoryDeltaBytes;
        }

        [ContextMenu("Run Spatial Filter Benchmark")]
        public void RunBenchmark()
        {
            if (isRunning) return;
            isRunning = true;
            _results.Clear();

            bool prevPerfMode = MmRelayNode.PerformanceMode;
            MmRelayNode.PerformanceMode = true;

            try
            {
                foreach (int count in receiverCounts)
                {
                    Debug.Log($"[SpatialFilterBenchmark] Testing {count} receivers...");
                    RunWithReceiverCount(count);
                }

                PrintResults();
                if (exportCsv) ExportCsv();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SpatialFilterBenchmark] {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                MmRelayNode.PerformanceMode = prevPerfMode;
                isRunning = false;
            }
        }

        private void RunWithReceiverCount(int count)
        {
            // Use deterministic seeded positions for reproducibility
            var rng = new System.Random(42);

            // Build Mercury hierarchy
            var root = new GameObject("SpatialRoot");
            root.transform.position = Vector3.zero;
            var rootRelay = root.AddComponent<MmRelayNode>();

            // Build Events receivers list
            var eventReceivers = new List<SpatialEventReceiver>();

            int inRangeCount = 0;

            for (int i = 0; i < count; i++)
            {
                // Deterministic position using seeded RNG
                float x = (float)(rng.NextDouble() * 2 - 1) * placementRange;
                float y = (float)(rng.NextDouble() * 2 - 1) * placementRange;
                float z = (float)(rng.NextDouble() * 2 - 1) * placementRange;
                Vector3 pos = new Vector3(x, y, z);

                var go = new GameObject($"Recv_{i}");
                go.transform.SetParent(root.transform);
                go.transform.position = pos;

                // Mercury components
                var relay = go.AddComponent<MmRelayNode>();
                go.AddComponent<SpatialMercuryReceiver>();
                rootRelay.MmAddToRoutingTable(relay, MmLevelFilter.Child);
                relay.AddParent(rootRelay);

                // Events receiver
                var er = go.AddComponent<SpatialEventReceiver>();
                eventReceivers.Add(er);

                if (Vector3.Distance(Vector3.zero, pos) <= radius)
                    inRangeCount++;
            }

            foreach (var r in root.GetComponentsInChildren<MmRelayNode>())
                r.MmRefreshResponders();

            Debug.Log($"  {inRangeCount}/{count} receivers within {radius}m radius");

            // Warm up Mercury pools
            for (int i = 0; i < warmUp; i++)
                rootRelay.Send("w").ToAll().Within(radius).Execute();

            // ---- Mercury WITHOUT spatial filter (isolates framework overhead) ----
            Measure("Mercury NoFilter", count, count, () =>
            {
                rootRelay.Send("alert").ToChildren().Execute();
            });

            // ---- Mercury WITH Within() spatial filter ----
            Measure("Mercury Within()", count, inRangeCount, () =>
            {
                rootRelay.Send("alert").ToAll().Within(radius).Execute();
            });

            // The delta between "Mercury NoFilter" and "Mercury Within()" is the
            // ACTUAL spatial filtering cost, isolated from framework overhead.

            // ---- Manual Vector3.Distance (what study participants write) ----
            Vector3 senderPos = root.transform.position;
            Measure("Manual Distance", count, inRangeCount, () =>
            {
                foreach (var recv in eventReceivers)
                {
                    float dist = Vector3.Distance(senderPos, recv.transform.position);
                    if (dist <= radius)
                        recv.HandleAlert("alert");
                }
            });

            // ---- Optimized sqrMagnitude (what an expert would write) ----
            float radiusSq = radius * radius;
            Measure("Optimized sqrMag", count, inRangeCount, () =>
            {
                foreach (var recv in eventReceivers)
                {
                    float distSq = (recv.transform.position - senderPos).sqrMagnitude;
                    if (distSq <= radiusSq)
                        recv.HandleAlert("alert");
                }
            });

            DestroyImmediate(root);
        }

        private void Measure(string name, int receiverCount, int inRange, Action action)
        {
            for (int run = 0; run < runs; run++)
            {
                // Memory before
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                long memBefore = GC.GetTotalMemory(true);

                var sw = new Stopwatch();

                bool gcSuppressed = false;
                try
                {
                    GC.TryStartNoGCRegion(32 * 1024 * 1024);
                    gcSuppressed = true;
                }
                catch (InvalidOperationException) { }

                sw.Start();
                for (int i = 0; i < iterations; i++)
                    action();
                sw.Stop();

                if (gcSuppressed)
                {
                    try { GC.EndNoGCRegion(); }
                    catch (InvalidOperationException) { }
                }

                long memAfter = GC.GetTotalMemory(false);

                double totalMs = sw.Elapsed.TotalMilliseconds;
                double avgUs = (totalMs / iterations) * 1000.0;
                long avgTicks = sw.ElapsedTicks / iterations;

                _results.Add(new Result
                {
                    Method = name,
                    ReceiverCount = receiverCount,
                    ReceiversInRange = inRange,
                    Run = run,
                    Iterations = iterations,
                    TotalMs = totalMs,
                    AvgUs = avgUs,
                    AvgTicks = avgTicks,
                    MemoryDeltaBytes = memAfter - memBefore
                });
            }
        }

        private void PrintResults()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== Spatial Filter Benchmark ===");
            sb.AppendLine($"Iterations: {iterations}, Radius: {radius}m, Range: {placementRange}m");
            sb.AppendLine();

            sb.AppendLine($"{"Method",-22} {"N",4} {"InRange",8} {"Total ms",10} {"Avg us",10} {"Avg ticks",10}");
            sb.AppendLine(new string('-', 66));

            int currentCount = -1;
            foreach (var r in _results)
            {
                if (r.ReceiverCount != currentCount)
                {
                    if (currentCount > 0) sb.AppendLine();
                    currentCount = r.ReceiverCount;
                }
                sb.AppendLine($"{r.Method,-22} {r.ReceiverCount,4} {r.ReceiversInRange,8} " +
                              $"{r.TotalMs,10:F2} {r.AvgUs,10:F3} {r.AvgTicks,10}");
            }

            // Ratios
            sb.AppendLine();
            sb.AppendLine("=== Mercury vs Manual Distance Ratio ===");
            foreach (int n in receiverCounts)
            {
                double? mercuryAvg = null, manualAvg = null;
                foreach (var r in _results)
                {
                    if (r.ReceiverCount == n && r.Method == "Mercury Within()") mercuryAvg = r.AvgUs;
                    if (r.ReceiverCount == n && r.Method == "Manual Distance") manualAvg = r.AvgUs;
                }
                if (mercuryAvg.HasValue && manualAvg.HasValue && manualAvg.Value > 0)
                {
                    double ratio = mercuryAvg.Value / manualAvg.Value;
                    sb.AppendLine($"  N={n}: Mercury is {ratio:F2}x vs manual Vector3.Distance");
                }
            }

            Debug.Log(sb.ToString());
        }

        private void ExportCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("method,receiver_count,in_range,run,iterations,total_ms,avg_us,avg_ticks,memory_delta_bytes");

            foreach (var r in _results)
            {
                sb.AppendLine($"{r.Method},{r.ReceiverCount},{r.ReceiversInRange}," +
                              $"{r.Run},{r.Iterations},{r.TotalMs:F4},{r.AvgUs:F4},{r.AvgTicks},{r.MemoryDeltaBytes}");
            }

            string path = Path.Combine(Application.dataPath,
                "Research", "Benchmarks", "Results", "spatial_filter_benchmark.csv");
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, sb.ToString());
            Debug.Log($"[SpatialFilterBenchmark] CSV exported to: {path}");
        }
    }

    internal class SpatialMercuryReceiver : MmBaseResponder
    {
        public int counter;
        protected override void ReceivedMessage(MmMessageString message) { counter++; }
    }

    internal class SpatialEventReceiver : MonoBehaviour
    {
        public int counter;
        public void HandleAlert(string msg) { counter++; }
    }
}
