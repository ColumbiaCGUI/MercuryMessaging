using System;
using System.Collections;
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
    /// Scenario-based benchmark matching actual study task patterns, then scaling
    /// up to find the crossover point where Mercury's overhead becomes measurable.
    ///
    /// T2 Pattern: 1 sender → N receivers with spatial filtering (fan-out)
    /// T4 Pattern: N senders → 1 receiver (many-to-one aggregation)
    ///
    /// At study scale (N=4), all methods should run identically at 60fps.
    /// The interesting question is WHERE the crossover is.
    /// </summary>
    public class ScenarioBenchmark : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Receiver counts to test (study scale first, then scale-up)")]
        public int[] receiverCounts = { 4, 16, 64, 256, 1024 };

        [Tooltip("Messages per frame (study sends every Update)")]
        public int messagesPerFrame = 1;

        [Tooltip("Duration per measurement (seconds)")]
        public float testDuration = 10f;

        [Tooltip("Number of runs per configuration for variance")]
        public int runs = 5;

        [Tooltip("Spatial filter radius for T2")]
        public float t2Radius = 2f;

        [Header("Export")]
        public bool exportCsv = true;

        [Header("Runtime")]
        public bool isRunning;
        public string currentTest = "";

        private readonly List<ScenarioResult> _results = new List<ScenarioResult>();
        private FrameResult _lastResult;

        private struct ScenarioResult
        {
            public string Scenario;
            public int N;
            public int Run;
            public float AvgFPS;
            public float FrameOverheadUs;
            public long MemoryDeltaBytes;
        }

        private struct FrameResult
        {
            public float avgFPS;
            public float avgOverheadUs;
            public long memoryDelta;
        }

        [ContextMenu("Run Scenario Benchmark")]
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

            Debug.Log("[ScenarioBenchmark] Starting scenario benchmarks...");
            Debug.Log($"  {receiverCounts.Length} scales x 4 methods x {runs} runs = {receiverCounts.Length * 4 * runs} measurements");

            foreach (int n in receiverCounts)
            {
                Debug.Log($"[ScenarioBenchmark] === Scale N={n} ===");

                for (int run = 0; run < runs; run++)
                {
                    currentTest = $"T2 Mercury N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT2Mercury(n, run));

                    currentTest = $"T2 Manual N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT2Manual(n, run));

                    currentTest = $"T4 Mercury N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT4Mercury(n, run));

                    currentTest = $"T4 Manual N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT4Manual(n, run));
                }
            }

            MmRelayNode.PerformanceMode = prevPerfMode;
            PrintResults();
            if (exportCsv) ExportCsv();
            isRunning = false;
            currentTest = "Complete";
        }

        // ================================================================
        // Coroutine-based measurement (yields per frame, measures work per frame)
        // ================================================================

        private IEnumerator Measure(float duration, Action perFrameWork)
        {
            // Warm up (60 frames)
            for (int i = 0; i < 60; i++)
            {
                perFrameWork();
                yield return null;
            }

            // GC before measurement
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long memBefore = GC.GetTotalMemory(true);

            var sw = new Stopwatch();
            float elapsed = 0f;
            int frames = 0;
            long totalOverheadTicks = 0;

            while (elapsed < duration)
            {
                sw.Restart();
                perFrameWork();
                sw.Stop();

                totalOverheadTicks += sw.ElapsedTicks;
                frames++;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            long memAfter = GC.GetTotalMemory(false);
            double avgOverheadUs = frames > 0
                ? (totalOverheadTicks * 1_000_000.0 / Stopwatch.Frequency) / frames
                : 0;

            _lastResult = new FrameResult
            {
                avgFPS = frames > 0 ? frames / elapsed : 0,
                avgOverheadUs = (float)avgOverheadUs,
                memoryDelta = memAfter - memBefore
            };

            // GC cleanup between tests
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            yield return null;
        }

        private void RecordResult(string scenario, int n, int run)
        {
            _results.Add(new ScenarioResult
            {
                Scenario = scenario,
                N = n,
                Run = run,
                AvgFPS = _lastResult.avgFPS,
                FrameOverheadUs = _lastResult.avgOverheadUs,
                MemoryDeltaBytes = _lastResult.memoryDelta
            });
        }

        // ================================================================
        // T2: Fan-out with spatial filtering (1 sender → N receivers)
        // ================================================================

        private IEnumerator RunT2Mercury(int n, int run)
        {
            var root = new GameObject("T2_Mercury");
            root.transform.position = Vector3.zero;
            var relay = root.AddComponent<MmRelayNode>();

            var rng = new System.Random(42 + run);
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject($"Ind_{i}");
                go.transform.SetParent(root.transform);
                float dist = (i % 2 == 0) ? (float)rng.NextDouble() * t2Radius : t2Radius + (float)rng.NextDouble() * 10f;
                float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                go.transform.position = new Vector3(Mathf.Cos(angle) * dist, 0, Mathf.Sin(angle) * dist);
                var childRelay = go.AddComponent<MmRelayNode>();
                go.AddComponent<ScenarioReceiver>();
                relay.MmAddToRoutingTable(childRelay, MmLevelFilter.Child);
                childRelay.AddParent(relay);
            }
            foreach (var r in root.GetComponentsInChildren<MmRelayNode>())
                r.MmRefreshResponders();
            yield return null;

            float rad = t2Radius;
            float emergRad = t2Radius * 0.5f;
            int mpf = messagesPerFrame;

            yield return StartCoroutine(Measure(testDuration, () =>
            {
                for (int m = 0; m < mpf; m++)
                {
                    relay.Send("warning").ToAll().Within(rad).Execute();
                    relay.Send("emergency").ToAll().Within(emergRad).Execute();
                }
            }));

            RecordResult("T2_Mercury", n, run);
            DestroyImmediate(root);
        }

        private IEnumerator RunT2Manual(int n, int run)
        {
            var root = new GameObject("T2_Manual");
            root.transform.position = Vector3.zero;

            var receivers = new List<ScenarioEventReceiver>();
            var rng = new System.Random(42 + run);
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject($"Ind_{i}");
                go.transform.SetParent(root.transform);
                float dist = (i % 2 == 0) ? (float)rng.NextDouble() * t2Radius : t2Radius + (float)rng.NextDouble() * 10f;
                float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                go.transform.position = new Vector3(Mathf.Cos(angle) * dist, 0, Mathf.Sin(angle) * dist);
                receivers.Add(go.AddComponent<ScenarioEventReceiver>());
            }
            yield return null;

            Vector3 senderPos = root.transform.position;
            float warningR = t2Radius;
            float emergencyR = t2Radius * 0.5f;
            int mpf = messagesPerFrame;

            yield return StartCoroutine(Measure(testDuration, () =>
            {
                for (int m = 0; m < mpf; m++)
                {
                    foreach (var recv in receivers)
                    {
                        float d = Vector3.Distance(senderPos, recv.transform.position);
                        if (d <= warningR) recv.HandleAlert("warning");
                        if (d <= emergencyR) recv.HandleAlert("emergency");
                    }
                }
            }));

            RecordResult("T2_Manual", n, run);
            DestroyImmediate(root);
        }

        // ================================================================
        // T4: Many-to-one aggregation (N senders → 1 dashboard)
        // ================================================================

        private IEnumerator RunT4Mercury(int n, int run)
        {
            var hub = new GameObject("T4_Hub");
            var hubRelay = hub.AddComponent<MmRelayNode>();

            var dashboard = new GameObject("Dashboard");
            dashboard.transform.SetParent(hub.transform);
            var dashRelay = dashboard.AddComponent<MmRelayNode>();
            dashboard.AddComponent<ScenarioReceiver>();
            hubRelay.MmAddToRoutingTable(dashRelay, MmLevelFilter.Child);
            dashRelay.AddParent(hubRelay);

            var senderRelays = new List<MmRelayNode>();
            for (int i = 0; i < n; i++)
            {
                var sub = new GameObject($"Sub_{i}");
                sub.transform.SetParent(dashboard.transform);
                var subRelay = sub.AddComponent<MmRelayNode>();
                sub.AddComponent<ScenarioReceiver>();
                dashRelay.MmAddToRoutingTable(subRelay, MmLevelFilter.Child);
                subRelay.AddParent(dashRelay);
                senderRelays.Add(subRelay);
            }
            foreach (var r in hub.GetComponentsInChildren<MmRelayNode>())
                r.MmRefreshResponders();
            yield return null;

            int senderCount = n;
            int idx = 0;

            yield return StartCoroutine(Measure(testDuration, () =>
            {
                senderRelays[idx % senderCount].NotifyValue($"alert_{idx}");
                idx++;
            }));

            RecordResult("T4_Mercury", n, run);
            DestroyImmediate(hub);
        }

        private IEnumerator RunT4Manual(int n, int run)
        {
            var hub = new GameObject("T4_Manual");
            var dashboard = new GameObject("Dashboard");
            dashboard.transform.SetParent(hub.transform);
            var dashRecv = dashboard.AddComponent<ScenarioEventReceiver>();
            yield return null;

            int idx = 0;

            yield return StartCoroutine(Measure(testDuration, () =>
            {
                dashRecv.HandleAlert($"alert_{idx}");
                idx++;
            }));

            RecordResult("T4_Manual", n, run);
            DestroyImmediate(hub);
        }

        // ================================================================
        // Output
        // ================================================================

        private void PrintResults()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== Scenario Benchmark Results ===");
            sb.AppendLine($"Duration: {testDuration}s, Runs: {runs}, MsgsPerFrame: {messagesPerFrame}");
            sb.AppendLine($"T2 radius: {t2Radius}m");
            sb.AppendLine();

            sb.AppendLine($"{"Scenario",-14} {"N",5} {"AvgFPS",12} {"Overhead_us",14} {"Mem_KB",10}");
            sb.AppendLine($"{"",14} {"",5} {"(mean +/- SD)",12} {"(mean +/- SD)",14} {"(mean)",10}");
            sb.AppendLine(new string('-', 60));

            var scenarios = new[] { "T2_Mercury", "T2_Manual", "T4_Mercury", "T4_Manual" };

            foreach (int n in receiverCounts)
            {
                foreach (string scenario in scenarios)
                {
                    var matching = new List<ScenarioResult>();
                    foreach (var r in _results)
                        if (r.Scenario == scenario && r.N == n)
                            matching.Add(r);

                    if (matching.Count == 0) continue;

                    float meanFPS = 0, meanOH = 0;
                    long meanMem = 0;
                    foreach (var r in matching) { meanFPS += r.AvgFPS; meanOH += r.FrameOverheadUs; meanMem += r.MemoryDeltaBytes; }
                    meanFPS /= matching.Count; meanOH /= matching.Count; meanMem /= matching.Count;

                    float sdFPS = 0, sdOH = 0;
                    foreach (var r in matching)
                    {
                        sdFPS += (r.AvgFPS - meanFPS) * (r.AvgFPS - meanFPS);
                        sdOH += (r.FrameOverheadUs - meanOH) * (r.FrameOverheadUs - meanOH);
                    }
                    sdFPS = matching.Count > 1 ? Mathf.Sqrt(sdFPS / (matching.Count - 1)) : 0;
                    sdOH = matching.Count > 1 ? Mathf.Sqrt(sdOH / (matching.Count - 1)) : 0;

                    sb.AppendLine($"{scenario,-14} {n,5} {meanFPS,6:F1}+/-{sdFPS,4:F1} {meanOH,7:F1}+/-{sdOH,5:F1} {meanMem / 1024f,10:F1}");
                }
                sb.AppendLine();
            }

            // Frame budget analysis
            sb.AppendLine("=== Frame Budget Analysis (16.6ms = 16600us = 60fps) ===");
            foreach (int n in receiverCounts)
            {
                float mercT2 = 0, manualT2 = 0;
                int cM = 0, cE = 0;
                foreach (var r in _results)
                {
                    if (r.N == n && r.Scenario == "T2_Mercury") { mercT2 += r.FrameOverheadUs; cM++; }
                    if (r.N == n && r.Scenario == "T2_Manual") { manualT2 += r.FrameOverheadUs; cE++; }
                }
                if (cM > 0 && cE > 0)
                {
                    mercT2 /= cM; manualT2 /= cE;
                    sb.AppendLine($"  N={n}: Mercury={mercT2:F1}us ({mercT2 / 16600f * 100:F3}%), Manual={manualT2:F1}us ({manualT2 / 16600f * 100:F3}%)");
                }
            }

            Debug.Log(sb.ToString());
        }

        private void ExportCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("scenario,n,run,avg_fps,frame_overhead_us,memory_delta_bytes");
            foreach (var r in _results)
                sb.AppendLine($"{r.Scenario},{r.N},{r.Run},{r.AvgFPS:F2},{r.FrameOverheadUs:F2},{r.MemoryDeltaBytes}");

            string path = Path.Combine(Application.dataPath, "Research", "Benchmarks", "Results", "scenario_benchmark.csv");
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, sb.ToString());
            Debug.Log($"[ScenarioBenchmark] CSV: {path}");
        }

        private void OnDestroy() { StopAllCoroutines(); }
    }

    internal class ScenarioReceiver : MmBaseResponder
    {
        public int counter;
        protected override void ReceivedMessage(MmMessageString message) { counter++; }
    }

    internal class ScenarioEventReceiver : MonoBehaviour
    {
        public int counter;
        public void HandleAlert(string msg) { counter++; }
    }
}
