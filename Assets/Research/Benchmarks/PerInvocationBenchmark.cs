using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using MercuryMessaging;
using Debug = UnityEngine.Debug;

namespace MercuryMessaging.Research.Benchmarks
{
    /// <summary>
    /// Measures per-invocation timing for different messaging approaches.
    /// Tests: Mercury DSL fluent chain, original MmInvoke, Unity SendMessage,
    /// Unity BroadcastMessage. Across hierarchy depths 1, 2, 4, 8.
    /// 10,000 invocations per measurement with warm-up.
    /// </summary>
    public class PerInvocationBenchmark : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Number of invocations per measurement")]
        public int invocationsPerMeasurement = 10000;

        [Tooltip("Warm-up invocations to skip (pool priming)")]
        public int warmUpInvocations = 100;

        [Tooltip("Hierarchy depths to test")]
        public int[] depths = { 1, 2, 4, 8 };

        [Header("Runtime Info")]
        [Tooltip("True while benchmark is running")]
        public bool isRunning;

        private readonly List<BenchmarkResult> _results = new List<BenchmarkResult>();

        private struct BenchmarkResult
        {
            public string Method;
            public int Depth;
            public int Invocations;
            public double TotalMs;
            public double AvgUs; // microseconds
            public long AvgTicks;
        }

        [ContextMenu("Run Benchmark")]
        public void RunBenchmark()
        {
            if (isRunning)
            {
                Debug.LogWarning("[PerInvocationBenchmark] Benchmark already running.");
                return;
            }

            isRunning = true;
            _results.Clear();

            try
            {
                foreach (int depth in depths)
                {
                    RunAtDepth(depth);
                }

                PrintResults();
            }
            catch (Exception e)
            {
                Debug.LogError($"[PerInvocationBenchmark] Error: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                isRunning = false;
            }
        }

        private void RunAtDepth(int depth)
        {
            // Build hierarchy programmatically
            GameObject root = BuildHierarchy(depth);
            MmRelayNode rootRelay = root.GetComponent<MmRelayNode>();
            var metadata = new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren);

            // Warm up pools
            for (int i = 0; i < warmUpInvocations; i++)
            {
                rootRelay.MmInvoke(MmMethod.Initialize, metadata);
            }

            // --- Mercury DSL fluent chain ---
            MeasureMethod("Mercury DSL", depth, () =>
            {
                rootRelay.Send("bench").ToDescendants().Execute();
            });

            // --- Original MmInvoke ---
            MeasureMethod("MmInvoke", depth, () =>
            {
                rootRelay.MmInvoke(MmMethod.MessageString, "bench", metadata);
            });

            // --- Unity SendMessage ---
            MeasureMethod("SendMessage", depth, () =>
            {
                root.SendMessage("OnBenchmarkPing", SendMessageOptions.DontRequireReceiver);
            });

            // --- Unity BroadcastMessage ---
            MeasureMethod("BroadcastMessage", depth, () =>
            {
                root.BroadcastMessage("OnBenchmarkPing", SendMessageOptions.DontRequireReceiver);
            });

            // Cleanup
            DestroyImmediate(root);
        }

        private void MeasureMethod(string methodName, int depth, Action action)
        {
            var stopwatch = new Stopwatch();

            // Try to suppress GC during measurement
            bool gcSuppressed = false;
            try
            {
                GC.TryStartNoGCRegion(16 * 1024 * 1024); // 16 MB
                gcSuppressed = true;
            }
            catch (InvalidOperationException)
            {
                // Already in no-GC region or not supported
            }

            stopwatch.Start();
            for (int i = 0; i < invocationsPerMeasurement; i++)
            {
                action();
            }
            stopwatch.Stop();

            if (gcSuppressed)
            {
                try
                {
                    GC.EndNoGCRegion();
                }
                catch (InvalidOperationException)
                {
                    // Region ended early due to allocation
                }
            }

            double totalMs = stopwatch.Elapsed.TotalMilliseconds;
            double avgUs = (totalMs / invocationsPerMeasurement) * 1000.0;
            long avgTicks = stopwatch.ElapsedTicks / invocationsPerMeasurement;

            _results.Add(new BenchmarkResult
            {
                Method = methodName,
                Depth = depth,
                Invocations = invocationsPerMeasurement,
                TotalMs = totalMs,
                AvgUs = avgUs,
                AvgTicks = avgTicks
            });
        }

        private GameObject BuildHierarchy(int depth)
        {
            GameObject root = new GameObject($"BenchRoot_d{depth}");
            root.AddComponent<MmRelayNode>();
            root.AddComponent<BenchmarkReceiver>();

            GameObject current = root;
            for (int d = 1; d < depth; d++)
            {
                GameObject child = new GameObject($"Level_{d}");
                child.transform.SetParent(current.transform);
                child.AddComponent<MmRelayNode>();
                child.AddComponent<BenchmarkReceiver>();

                var parentRelay = current.GetComponent<MmRelayNode>();
                var childRelay = child.GetComponent<MmRelayNode>();
                parentRelay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
                childRelay.AddParent(parentRelay);

                current = child;
            }

            // Refresh all relay nodes
            var allRelays = root.GetComponentsInChildren<MmRelayNode>();
            foreach (var relay in allRelays)
            {
                relay.MmRefreshResponders();
            }

            return root;
        }

        private void PrintResults()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== Per-Invocation Benchmark Results ===");
            sb.AppendLine($"Invocations per measurement: {invocationsPerMeasurement}");
            sb.AppendLine($"Warm-up invocations: {warmUpInvocations}");
            sb.AppendLine();

            // Header
            sb.AppendLine($"{"Method",-20} {"Depth",6} {"Total (ms)",12} {"Avg (us)",12} {"Avg (ticks)",12}");
            sb.AppendLine(new string('-', 62));

            foreach (var r in _results)
            {
                sb.AppendLine($"{r.Method,-20} {r.Depth,6} {r.TotalMs,12:F2} {r.AvgUs,12:F3} {r.AvgTicks,12}");
            }

            sb.AppendLine();

            // Summary per depth
            foreach (int depth in depths)
            {
                sb.AppendLine($"--- Depth {depth} Relative Performance ---");
                double? sendMessageAvg = null;
                foreach (var r in _results)
                {
                    if (r.Depth == depth && r.Method == "SendMessage")
                    {
                        sendMessageAvg = r.AvgUs;
                        break;
                    }
                }

                if (sendMessageAvg.HasValue && sendMessageAvg.Value > 0)
                {
                    foreach (var r in _results)
                    {
                        if (r.Depth == depth)
                        {
                            double ratio = r.AvgUs / sendMessageAvg.Value;
                            sb.AppendLine($"  {r.Method,-20} {ratio,6:F2}x vs SendMessage");
                        }
                    }
                }

                sb.AppendLine();
            }

            Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Dummy method for SendMessage/BroadcastMessage benchmarks.
        /// </summary>
        public void OnBenchmarkPing() { }
    }

    /// <summary>
    /// Minimal responder that receives benchmark messages without doing work.
    /// </summary>
    internal class BenchmarkReceiver : MmBaseResponder
    {
        protected override void ReceivedMessage(MmMessageString message) { }

        public override void Initialize() { }

        /// <summary>
        /// Dummy method for SendMessage/BroadcastMessage benchmarks.
        /// </summary>
        public void OnBenchmarkPing() { }
    }
}
