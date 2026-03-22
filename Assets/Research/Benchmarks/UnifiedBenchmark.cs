using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using MercuryMessaging;
using Debug = UnityEngine.Debug;

namespace MercuryMessaging.Research.Benchmarks
{
    /// <summary>
    /// Comprehensive per-invocation microbenchmark comparing all messaging approaches.
    /// Tests: Direct call, C# delegate, UnityEvent, SendMessage, BroadcastMessage,
    /// Mercury MmInvoke (old CHI 2018 API), Mercury DSL fluent chain,
    /// Mercury DSL with Within() spatial filter.
    /// Fan-out: 1, 4, 16, 64 receivers.
    /// </summary>
    public class UnifiedBenchmark : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Invocations per measurement")]
        public int iterations = 10000;

        [Tooltip("Warm-up invocations (skipped)")]
        public int warmUp = 100;

        [Tooltip("Fan-out counts to test")]
        public int[] fanOuts = { 1, 4, 16, 64 };

        [Tooltip("Spatial filter radius for Within() test")]
        public float withinRadius = 5f;

        [Header("Export")]
        [Tooltip("Export CSV to Assets/Research/Benchmarks/Results/")]
        public bool exportCsv = true;

        [Header("Runtime")]
        public bool isRunning;

        private readonly List<Result> _results = new List<Result>();

        private struct Result
        {
            public string Method;
            public int FanOut;
            public int Iterations;
            public double TotalMs;
            public double AvgUs;
            public long AvgTicks;
        }

        [ContextMenu("Run Unified Benchmark")]
        public void RunBenchmark()
        {
            if (isRunning) return;
            isRunning = true;
            _results.Clear();

            // Ensure PerformanceMode is on for clean measurement
            bool prevPerfMode = MmRelayNode.PerformanceMode;
            MmRelayNode.PerformanceMode = true;

            try
            {
                foreach (int fanOut in fanOuts)
                {
                    Debug.Log($"[UnifiedBenchmark] Testing fan-out={fanOut}...");
                    RunFanOut(fanOut);
                }

                PrintResults();
                if (exportCsv) ExportCsv();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnifiedBenchmark] {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                MmRelayNode.PerformanceMode = prevPerfMode;
                isRunning = false;
            }
        }

        private void RunFanOut(int fanOut)
        {
            // ---- Build test hierarchy ----
            var root = new GameObject("BenchRoot");
            var rootRelay = root.AddComponent<MmRelayNode>();

            var receivers = new BenchReceiver[fanOut];
            var eventReceivers = new EventReceiver[fanOut];
            var unityEvent = new UnityEvent<string>();

            // C# delegate
            Action<string> csharpDelegate = null;

            for (int i = 0; i < fanOut; i++)
            {
                var go = new GameObject($"Recv_{i}");
                go.transform.SetParent(root.transform);
                // Place at known positions for spatial filtering test
                go.transform.position = new Vector3(i * 0.5f, 0, 0);

                // Mercury components
                var relay = go.AddComponent<MmRelayNode>();
                var recv = go.AddComponent<BenchReceiver>();
                receivers[i] = recv;

                // Register in Mercury routing table
                rootRelay.MmAddToRoutingTable(relay, MmLevelFilter.Child);
                relay.AddParent(rootRelay);

                // Event receiver (plain MonoBehaviour)
                var er = go.AddComponent<EventReceiver>();
                eventReceivers[i] = er;

                // UnityEvent listener
                int idx = i;
                unityEvent.AddListener((msg) => eventReceivers[idx].counter++);

                // C# delegate
                csharpDelegate += (msg) => eventReceivers[i].counter++;
            }

            // Refresh all relay nodes
            foreach (var r in root.GetComponentsInChildren<MmRelayNode>())
                r.MmRefreshResponders();

            var metadata = new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren);

            // Warm up
            for (int i = 0; i < warmUp; i++)
            {
                rootRelay.MmInvoke(MmMethod.MessageString, "w", metadata);
            }

            // ---- 1. Direct method call ----
            Measure("Direct Call", fanOut, () =>
            {
                for (int i = 0; i < fanOut; i++)
                    eventReceivers[i].HandleMessage("bench");
            });

            // ---- 2. C# delegate ----
            Measure("C# Delegate", fanOut, () =>
            {
                csharpDelegate?.Invoke("bench");
            });

            // ---- 3. UnityEvent ----
            Measure("UnityEvent", fanOut, () =>
            {
                unityEvent.Invoke("bench");
            });

            // ---- 4. SendMessage (only hits root, not children — documents this limitation) ----
            Measure("SendMessage", fanOut, () =>
            {
                root.SendMessage("HandleMessage", "bench", SendMessageOptions.DontRequireReceiver);
            });

            // ---- 5. BroadcastMessage (propagates down Transform hierarchy) ----
            Measure("BroadcastMessage", fanOut, () =>
            {
                root.BroadcastMessage("HandleMessage", "bench", SendMessageOptions.DontRequireReceiver);
            });

            // ---- 6. Mercury MmInvoke (old CHI 2018 API) ----
            Measure("Mercury MmInvoke", fanOut, () =>
            {
                rootRelay.MmInvoke(MmMethod.MessageString, "bench", metadata);
            });

            // ---- 7. Mercury DSL fluent chain ----
            Measure("Mercury DSL", fanOut, () =>
            {
                rootRelay.Send("bench").ToChildren().Execute();
            });

            // ---- 8. Mercury DSL + Within() spatial filter ----
            Measure("Mercury Within()", fanOut, () =>
            {
                rootRelay.Send("bench").ToAll().Within(withinRadius).Execute();
            });

            // Cleanup
            DestroyImmediate(root);
        }

        private void Measure(string name, int fanOut, Action action)
        {
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

            double totalMs = sw.Elapsed.TotalMilliseconds;
            double avgUs = (totalMs / iterations) * 1000.0;
            long avgTicks = sw.ElapsedTicks / iterations;

            _results.Add(new Result
            {
                Method = name,
                FanOut = fanOut,
                Iterations = iterations,
                TotalMs = totalMs,
                AvgUs = avgUs,
                AvgTicks = avgTicks
            });
        }

        private void PrintResults()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== Unified Invocation Benchmark ===");
            sb.AppendLine($"Iterations: {iterations}, Warm-up: {warmUp}");
            sb.AppendLine($"Within() radius: {withinRadius}m");
            sb.AppendLine();

            sb.AppendLine($"{"Method",-22} {"Fan",4} {"Total ms",10} {"Avg us",10} {"Avg ticks",10}");
            sb.AppendLine(new string('-', 58));

            int currentFanOut = -1;
            foreach (var r in _results)
            {
                if (r.FanOut != currentFanOut)
                {
                    if (currentFanOut > 0) sb.AppendLine();
                    currentFanOut = r.FanOut;
                    sb.AppendLine($"--- Fan-out: {currentFanOut} receivers ---");
                }
                sb.AppendLine($"{r.Method,-22} {r.FanOut,4} {r.TotalMs,10:F2} {r.AvgUs,10:F3} {r.AvgTicks,10}");
            }

            // Relative to direct call per fan-out
            sb.AppendLine();
            sb.AppendLine("=== Relative to Direct Call ===");
            foreach (int fo in fanOuts)
            {
                double? directAvg = null;
                foreach (var r in _results)
                    if (r.FanOut == fo && r.Method == "Direct Call") { directAvg = r.AvgUs; break; }

                if (!directAvg.HasValue || directAvg.Value <= 0) continue;

                sb.AppendLine($"Fan-out {fo}:");
                foreach (var r in _results)
                {
                    if (r.FanOut == fo)
                    {
                        double ratio = r.AvgUs / directAvg.Value;
                        sb.AppendLine($"  {r.Method,-22} {ratio,8:F1}x");
                    }
                }
            }

            Debug.Log(sb.ToString());
        }

        private void ExportCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("method,fan_out,iterations,total_ms,avg_us,avg_ticks");

            foreach (var r in _results)
            {
                sb.AppendLine($"{r.Method},{r.FanOut},{r.Iterations}," +
                              $"{r.TotalMs:F4},{r.AvgUs:F4},{r.AvgTicks}");
            }

            string path = Path.Combine(Application.dataPath,
                "Research", "Benchmarks", "Results", "unified_benchmark.csv");
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, sb.ToString());
            Debug.Log($"[UnifiedBenchmark] CSV exported to: {path}");
        }

        public void HandleMessage(string msg) { } // for SendMessage
    }

    internal class BenchReceiver : MmBaseResponder
    {
        public int counter;
        protected override void ReceivedMessage(MmMessageString message) { counter++; }
        public override void Initialize() { counter++; }
        public void HandleMessage(string msg) { counter++; }
    }

    internal class EventReceiver : MonoBehaviour
    {
        public int counter;
        public void HandleMessage(string msg) { counter++; }
    }
}
