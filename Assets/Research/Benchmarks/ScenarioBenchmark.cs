// Uncomment to enable optional library comparisons:
// #define HAS_R3
// #define HAS_MESSAGEPIPE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using MercuryMessaging;
using Debug = UnityEngine.Debug;

#if HAS_R3
using R3;
#endif

#if HAS_MESSAGEPIPE
using MessagePipe;
#endif

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
    ///
    /// Methods compared:
    ///   T2: Mercury DSL (Within), MmInvoke (broadcast, no spatial filter),
    ///       Manual (Vector3.Distance), Delegate (Action+distance), UnityEvent+distance,
    ///       R3 Subject+distance (#if HAS_R3), MessagePipe+distance (#if HAS_MESSAGEPIPE)
    ///   T4: Mercury DSL (NotifyValue), MmInvoke (old API, Parent filter),
    ///       Manual (direct call), Delegate, UnityEvent
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

        // ----------------------------------------------------------------
        // All scenario names in order (T2 then T4)
        // ----------------------------------------------------------------
        private static readonly string[] T2Scenarios =
        {
            "T2_Mercury",
            "T2_MmInvoke",
            "T2_Manual",
            "T2_Delegate",
            "T2_UnityEvent",
#if HAS_R3
            "T2_R3",
#endif
#if HAS_MESSAGEPIPE
            "T2_MessagePipe",
#endif
        };

        private static readonly string[] T4Scenarios =
        {
            "T4_Mercury",
            "T4_MmInvoke",
            "T4_Manual",
            "T4_Delegate",
            "T4_UnityEvent",
        };

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

            int totalScenarios = T2Scenarios.Length + T4Scenarios.Length;
            Debug.Log("[ScenarioBenchmark] Starting scenario benchmarks...");
            Debug.Log($"  {receiverCounts.Length} scales x {totalScenarios} methods x {runs} runs = {receiverCounts.Length * totalScenarios * runs} measurements");

            foreach (int n in receiverCounts)
            {
                Debug.Log($"[ScenarioBenchmark] === Scale N={n} ===");

                for (int run = 0; run < runs; run++)
                {
                    // ---- T2 scenarios ----
                    currentTest = $"T2_Mercury N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT2Mercury(n, run));

                    currentTest = $"T2_MmInvoke N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT2MmInvoke(n, run));

                    currentTest = $"T2_Manual N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT2Manual(n, run));

                    currentTest = $"T2_Delegate N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT2Delegate(n, run));

                    currentTest = $"T2_UnityEvent N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT2UnityEvent(n, run));

#if HAS_R3
                    currentTest = $"T2_R3 N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT2R3(n, run));
#endif

#if HAS_MESSAGEPIPE
                    currentTest = $"T2_MessagePipe N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT2MessagePipe(n, run));
#endif

                    // ---- T4 scenarios ----
                    currentTest = $"T4_Mercury N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT4Mercury(n, run));

                    currentTest = $"T4_MmInvoke N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT4MmInvoke(n, run));

                    currentTest = $"T4_Manual N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT4Manual(n, run));

                    currentTest = $"T4_Delegate N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT4Delegate(n, run));

                    currentTest = $"T4_UnityEvent N={n} run {run + 1}/{runs}";
                    yield return StartCoroutine(RunT4UnityEvent(n, run));
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

        /// <summary>
        /// T2_Mercury: Mercury DSL with Within() spatial filter.
        /// relay.Send("warning").ToAll().Within(radius).Execute()
        /// The relay node co-located with the sender; Within() measures from its position.
        /// </summary>
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
                float dist = (i % 2 == 0)
                    ? (float)rng.NextDouble() * t2Radius
                    : t2Radius + (float)rng.NextDouble() * 10f;
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

        /// <summary>
        /// T2_MmInvoke: Mercury old API — relay.MmInvoke with MmMetadataBlock.
        /// No spatial filter: broadcasts to all children unconditionally.
        /// This isolates the Mercury framework overhead WITHOUT the Within() predicate cost.
        /// </summary>
        private IEnumerator RunT2MmInvoke(int n, int run)
        {
            var root = new GameObject("T2_MmInvoke");
            root.transform.position = Vector3.zero;
            var relay = root.AddComponent<MmRelayNode>();

            var rng = new System.Random(42 + run);
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject($"Ind_{i}");
                go.transform.SetParent(root.transform);
                float dist = (i % 2 == 0)
                    ? (float)rng.NextDouble() * t2Radius
                    : t2Radius + (float)rng.NextDouble() * 10f;
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

            var metadata = new MmMetadataBlock(MmLevelFilterHelper.SelfAndChildren);
            int mpf = messagesPerFrame;

            yield return StartCoroutine(Measure(testDuration, () =>
            {
                for (int m = 0; m < mpf; m++)
                {
                    relay.MmInvoke(MmMethod.MessageString, "warning", metadata);
                    relay.MmInvoke(MmMethod.MessageString, "emergency", metadata);
                }
            }));

            RecordResult("T2_MmInvoke", n, run);
            DestroyImmediate(root);
        }

        /// <summary>
        /// T2_Manual: Direct iteration with Vector3.Distance check per receiver.
        /// Baseline for T2 — raw C# cost with no messaging overhead.
        /// </summary>
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
                float dist = (i % 2 == 0)
                    ? (float)rng.NextDouble() * t2Radius
                    : t2Radius + (float)rng.NextDouble() * 10f;
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
                        if (d <= warningR)   recv.HandleAlert("warning");
                        if (d <= emergencyR) recv.HandleAlert("emergency");
                    }
                }
            }));

            RecordResult("T2_Manual", n, run);
            DestroyImmediate(root);
        }

        /// <summary>
        /// T2_Delegate: C# Action&lt;string&gt; multicast delegate per receiver,
        /// with manual distance check before invoking.
        /// </summary>
        private IEnumerator RunT2Delegate(int n, int run)
        {
            var root = new GameObject("T2_Delegate");
            root.transform.position = Vector3.zero;

            // Each receiver registers its own Action handler; we keep refs to
            // (position, delegate) so we can do the distance gate ourselves.
            var entries = new List<(Transform xf, Action<string> handler)>();
            var rng = new System.Random(42 + run);
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject($"Ind_{i}");
                go.transform.SetParent(root.transform);
                float dist = (i % 2 == 0)
                    ? (float)rng.NextDouble() * t2Radius
                    : t2Radius + (float)rng.NextDouble() * 10f;
                float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                go.transform.position = new Vector3(Mathf.Cos(angle) * dist, 0, Mathf.Sin(angle) * dist);
                var recv = go.AddComponent<ScenarioEventReceiver>();
                entries.Add((go.transform, recv.HandleAlert));
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
                    foreach (var (xf, handler) in entries)
                    {
                        float d = Vector3.Distance(senderPos, xf.position);
                        if (d <= warningR)   handler("warning");
                        if (d <= emergencyR) handler("emergency");
                    }
                }
            }));

            RecordResult("T2_Delegate", n, run);
            DestroyImmediate(root);
        }

        /// <summary>
        /// T2_UnityEvent: UnityEvent&lt;string&gt; per receiver with manual distance check.
        /// </summary>
        private IEnumerator RunT2UnityEvent(int n, int run)
        {
            var root = new GameObject("T2_UnityEvent");
            root.transform.position = Vector3.zero;

            var entries = new List<(Transform xf, UnityEvent<string> evt)>();
            var rng = new System.Random(42 + run);
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject($"Ind_{i}");
                go.transform.SetParent(root.transform);
                float dist = (i % 2 == 0)
                    ? (float)rng.NextDouble() * t2Radius
                    : t2Radius + (float)rng.NextDouble() * 10f;
                float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                go.transform.position = new Vector3(Mathf.Cos(angle) * dist, 0, Mathf.Sin(angle) * dist);
                var recv = go.AddComponent<ScenarioEventReceiver>();
                var evt = new UnityEvent<string>();
                evt.AddListener(recv.HandleAlert);
                entries.Add((go.transform, evt));
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
                    foreach (var (xf, evt) in entries)
                    {
                        float d = Vector3.Distance(senderPos, xf.position);
                        if (d <= warningR)   evt.Invoke("warning");
                        if (d <= emergencyR) evt.Invoke("emergency");
                    }
                }
            }));

            RecordResult("T2_UnityEvent", n, run);
            DestroyImmediate(root);
        }

#if HAS_R3
        /// <summary>
        /// T2_R3: R3 Subject&lt;string&gt; publish/subscribe with distance check per receiver.
        /// Each receiver subscribes; sender publishes after checking distance.
        /// </summary>
        private IEnumerator RunT2R3(int n, int run)
        {
            var root = new GameObject("T2_R3");
            root.transform.position = Vector3.zero;

            // Two subjects — one per alert level.
            var warningSubject   = new Subject<string>();
            var emergencySubject = new Subject<string>();

            var entries = new List<(Transform xf, IDisposable warnSub, IDisposable emergSub)>();
            var rng = new System.Random(42 + run);
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject($"Ind_{i}");
                go.transform.SetParent(root.transform);
                float dist = (i % 2 == 0)
                    ? (float)rng.NextDouble() * t2Radius
                    : t2Radius + (float)rng.NextDouble() * 10f;
                float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                go.transform.position = new Vector3(Mathf.Cos(angle) * dist, 0, Mathf.Sin(angle) * dist);
                var recv = go.AddComponent<ScenarioEventReceiver>();
                var warnSub  = warningSubject.Subscribe(msg  => recv.HandleAlert(msg));
                var emergSub = emergencySubject.Subscribe(msg => recv.HandleAlert(msg));
                entries.Add((go.transform, warnSub, emergSub));
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
                    // Distance-gate: only publish to subjects whose receivers are in range.
                    // Because R3 is a broadcast, we iterate and publish only for in-range
                    // receivers by using per-receiver subjects instead of a shared one.
                    foreach (var (xf, warnSub, emergSub) in entries)
                    {
                        float d = Vector3.Distance(senderPos, xf.position);
                        if (d <= warningR)   warningSubject.OnNext("warning");
                        if (d <= emergencyR) emergencySubject.OnNext("emergency");
                    }
                }
            }));

            RecordResult("T2_R3", n, run);

            foreach (var (_, warnSub, emergSub) in entries) { warnSub.Dispose(); emergSub.Dispose(); }
            warningSubject.Dispose();
            emergencySubject.Dispose();
            DestroyImmediate(root);
        }
#endif

#if HAS_MESSAGEPIPE
        /// <summary>
        /// T2_MessagePipe: MessagePipe IPublisher/ISubscriber with distance check per receiver.
        /// </summary>
        private IEnumerator RunT2MessagePipe(int n, int run)
        {
            var root = new GameObject("T2_MessagePipe");
            root.transform.position = Vector3.zero;

            // MessagePipe requires a DI container; use the global provider if configured,
            // or build a local one for the benchmark.
            var builder = new BuiltinContainerBuilder();
            builder.AddMessagePipe();
            var provider = builder.BuildServiceProvider();

            var warningPub   = provider.GetRequiredService<IPublisher<string>>();
            var emergencyPub = provider.GetRequiredService<IPublisher<string>>();

            var entries = new List<(Transform xf, IDisposable sub)>();
            var bag = DisposableBag.CreateBuilder();
            var rng = new System.Random(42 + run);
            for (int i = 0; i < n; i++)
            {
                var go = new GameObject($"Ind_{i}");
                go.transform.SetParent(root.transform);
                float dist = (i % 2 == 0)
                    ? (float)rng.NextDouble() * t2Radius
                    : t2Radius + (float)rng.NextDouble() * 10f;
                float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                go.transform.position = new Vector3(Mathf.Cos(angle) * dist, 0, Mathf.Sin(angle) * dist);
                var recv = go.AddComponent<ScenarioEventReceiver>();
                var sub = provider.GetRequiredService<ISubscriber<string>>()
                    .Subscribe(msg => recv.HandleAlert(msg)).AddTo(bag);
                entries.Add((go.transform, sub));
            }
            var allSubs = bag.Build();
            yield return null;

            Vector3 senderPos = root.transform.position;
            float warningR = t2Radius;
            float emergencyR = t2Radius * 0.5f;
            int mpf = messagesPerFrame;

            yield return StartCoroutine(Measure(testDuration, () =>
            {
                for (int m = 0; m < mpf; m++)
                {
                    foreach (var (xf, _) in entries)
                    {
                        float d = Vector3.Distance(senderPos, xf.position);
                        if (d <= warningR)   warningPub.Publish("warning");
                        if (d <= emergencyR) emergencyPub.Publish("emergency");
                    }
                }
            }));

            RecordResult("T2_MessagePipe", n, run);

            allSubs.Dispose();
            DestroyImmediate(root);
        }
#endif

        // ================================================================
        // T4: Many-to-one aggregation (N senders → 1 dashboard)
        // ================================================================

        /// <summary>
        /// T4_Mercury: Mercury DSL — each subsystem relay calls NotifyValue() to
        /// propagate the alert UP the hierarchy to the parent dashboard.
        /// relay.NotifyValue(alertData)
        /// </summary>
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

        /// <summary>
        /// T4_MmInvoke: Mercury old API — relay.MmInvoke(MmMethod.MessageString, value, metadata)
        /// using MmLevelFilter.Parent so the message travels UP to the dashboard.
        /// </summary>
        private IEnumerator RunT4MmInvoke(int n, int run)
        {
            var hub = new GameObject("T4_MmInvoke");
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

            // Parent filter so message travels up to the dashboard receiver
            var metadata = new MmMetadataBlock(MmLevelFilterHelper.SelfAndParents);
            int senderCount = n;
            int idx = 0;

            yield return StartCoroutine(Measure(testDuration, () =>
            {
                senderRelays[idx % senderCount].MmInvoke(MmMethod.MessageString, $"alert_{idx}", metadata);
                idx++;
            }));

            RecordResult("T4_MmInvoke", n, run);
            DestroyImmediate(hub);
        }

        /// <summary>
        /// T4_Manual: Direct method call from sender to dashboard reference.
        /// Baseline — no framework overhead whatsoever.
        /// </summary>
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

        /// <summary>
        /// T4_Delegate: C# Action&lt;string&gt; delegate — each subsystem holds a reference
        /// to the dashboard's handler and invokes it directly.
        /// </summary>
        private IEnumerator RunT4Delegate(int n, int run)
        {
            var hub = new GameObject("T4_Delegate");
            var dashboard = new GameObject("Dashboard");
            dashboard.transform.SetParent(hub.transform);
            var dashRecv = dashboard.AddComponent<ScenarioEventReceiver>();

            // Wire each subsystem to the same dashboard handler via delegate
            Action<string> onAlert = dashRecv.HandleAlert;

            var senders = new List<Action<string>>();
            for (int i = 0; i < n; i++)
            {
                var sub = new GameObject($"Sub_{i}");
                sub.transform.SetParent(dashboard.transform);
                // Each sender captures the delegate (simulates what a field reference would do)
                senders.Add(onAlert);
            }
            yield return null;

            int senderCount = n;
            int idx = 0;

            yield return StartCoroutine(Measure(testDuration, () =>
            {
                senders[idx % senderCount].Invoke($"alert_{idx}");
                idx++;
            }));

            RecordResult("T4_Delegate", n, run);
            DestroyImmediate(hub);
        }

        /// <summary>
        /// T4_UnityEvent: UnityEvent&lt;string&gt; — each subsystem has its own event
        /// wired to the dashboard handler, and invokes it.
        /// </summary>
        private IEnumerator RunT4UnityEvent(int n, int run)
        {
            var hub = new GameObject("T4_UnityEvent");
            var dashboard = new GameObject("Dashboard");
            dashboard.transform.SetParent(hub.transform);
            var dashRecv = dashboard.AddComponent<ScenarioEventReceiver>();

            var senderEvents = new List<UnityEvent<string>>();
            for (int i = 0; i < n; i++)
            {
                var sub = new GameObject($"Sub_{i}");
                sub.transform.SetParent(dashboard.transform);
                var evt = new UnityEvent<string>();
                evt.AddListener(dashRecv.HandleAlert);
                senderEvents.Add(evt);
            }
            yield return null;

            int senderCount = n;
            int idx = 0;

            yield return StartCoroutine(Measure(testDuration, () =>
            {
                senderEvents[idx % senderCount].Invoke($"alert_{idx}");
                idx++;
            }));

            RecordResult("T4_UnityEvent", n, run);
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

            // Collect all scenario names actually present in results
            var allScenarios = new List<string>();
            foreach (string s in T2Scenarios) allScenarios.Add(s);
            foreach (string s in T4Scenarios) allScenarios.Add(s);

            sb.AppendLine($"{"Scenario",-20} {"N",5} {"AvgFPS",14} {"Overhead_us",16} {"Mem_KB",10}");
            sb.AppendLine($"{"",20} {"",5} {"(mean+/-SD)",14} {"(mean+/-SD)",16} {"(mean)",10}");
            sb.AppendLine(new string('-', 70));

            foreach (int n in receiverCounts)
            {
                foreach (string scenario in allScenarios)
                {
                    var matching = new List<ScenarioResult>();
                    foreach (var r in _results)
                        if (r.Scenario == scenario && r.N == n)
                            matching.Add(r);

                    if (matching.Count == 0) continue;

                    float meanFPS = 0, meanOH = 0;
                    long meanMem = 0;
                    foreach (var r in matching)
                    {
                        meanFPS += r.AvgFPS;
                        meanOH  += r.FrameOverheadUs;
                        meanMem += r.MemoryDeltaBytes;
                    }
                    meanFPS /= matching.Count;
                    meanOH  /= matching.Count;
                    meanMem /= matching.Count;

                    float sdFPS = 0, sdOH = 0;
                    foreach (var r in matching)
                    {
                        sdFPS += (r.AvgFPS - meanFPS) * (r.AvgFPS - meanFPS);
                        sdOH  += (r.FrameOverheadUs - meanOH) * (r.FrameOverheadUs - meanOH);
                    }
                    sdFPS = matching.Count > 1 ? Mathf.Sqrt(sdFPS / (matching.Count - 1)) : 0;
                    sdOH  = matching.Count > 1 ? Mathf.Sqrt(sdOH  / (matching.Count - 1)) : 0;

                    sb.AppendLine($"{scenario,-20} {n,5} {meanFPS,6:F1}+/-{sdFPS,4:F1} {meanOH,8:F1}+/-{sdOH,5:F1} {meanMem / 1024f,10:F1}");
                }
                sb.AppendLine();
            }

            // Frame budget analysis — compare each T2 method against T2_Manual baseline
            sb.AppendLine("=== T2 Frame Budget Analysis (16600 us = 60fps) ===");
            foreach (int n in receiverCounts)
            {
                var t2Means = new Dictionary<string, float>();
                foreach (string scenario in T2Scenarios)
                {
                    float sum = 0; int cnt = 0;
                    foreach (var r in _results)
                        if (r.N == n && r.Scenario == scenario) { sum += r.FrameOverheadUs; cnt++; }
                    if (cnt > 0) t2Means[scenario] = sum / cnt;
                }

                if (t2Means.Count == 0) continue;
                sb.Append($"  N={n}:");
                foreach (var kv in t2Means)
                    sb.Append($"  {kv.Key}={kv.Value:F1}us ({kv.Value / 16600f * 100:F3}%)");
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("=== T4 Frame Budget Analysis (16600 us = 60fps) ===");
            foreach (int n in receiverCounts)
            {
                var t4Means = new Dictionary<string, float>();
                foreach (string scenario in T4Scenarios)
                {
                    float sum = 0; int cnt = 0;
                    foreach (var r in _results)
                        if (r.N == n && r.Scenario == scenario) { sum += r.FrameOverheadUs; cnt++; }
                    if (cnt > 0) t4Means[scenario] = sum / cnt;
                }

                if (t4Means.Count == 0) continue;
                sb.Append($"  N={n}:");
                foreach (var kv in t4Means)
                    sb.Append($"  {kv.Key}={kv.Value:F1}us ({kv.Value / 16600f * 100:F3}%)");
                sb.AppendLine();
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
            Debug.Log($"[ScenarioBenchmark] CSV exported: {path}");
        }

        private void OnDestroy() { StopAllCoroutines(); }
    }

    // ====================================================================
    // Helper receivers
    // ====================================================================

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
