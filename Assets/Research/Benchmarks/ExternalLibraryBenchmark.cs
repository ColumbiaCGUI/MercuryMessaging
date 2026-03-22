// R3 is installed (NuGet core + UPM layer):
#define HAS_R3

// MessagePipe is installed — enable it:
#define HAS_MESSAGEPIPE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
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
    /// Benchmarks Mercury against external messaging libraries (R3, MessagePipe).
    /// These libraries must be installed separately — see INSTALL INSTRUCTIONS below.
    ///
    /// INSTALL INSTRUCTIONS:
    ///
    /// 1. R3 (Cysharp):
    ///    a. Install NuGetForUnity: Window > Package Manager > + > Add by git URL:
    ///       https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity
    ///    b. Window > NuGet > Manage NuGet Packages > search "R3" > Install
    ///    c. Package Manager > + > Add by git URL:
    ///       https://github.com/Cysharp/R3.git?path=src/R3.Unity/Assets/R3.Unity
    ///    d. If version errors: Project Settings > Player > Other > uncheck "Assembly Version Validation"
    ///    e. Uncomment the R3 sections below and the using R3 directive above
    ///
    /// 2. MessagePipe (Cysharp):
    ///    a. Install UniTask first: Package Manager > + > Add by git URL:
    ///       https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
    ///    b. Package Manager > + > Add by git URL:
    ///       https://github.com/Cysharp/MessagePipe.git?path=src/MessagePipe.Unity/Assets/Plugins/MessagePipe
    ///    c. Delete bundled System.*.dll files from MessagePipe plugin if Unity 6 conflicts
    ///    d. Uncomment the MessagePipe sections below and the using MessagePipe directive above
    ///
    /// Enable via #define at top of file after installing.
    /// </summary>
    public class ExternalLibraryBenchmark : MonoBehaviour
    {
        [Header("Configuration")]
        public int iterations = 10000;
        public int warmUp = 100;
        public int[] subscriberCounts = { 1, 4, 16, 64 };

        [Header("Export")]
        public bool exportCsv = true;

        [Header("Runtime")]
        public bool isRunning;

        private readonly List<Result> _results = new List<Result>();

        private struct Result
        {
            public string Method;
            public int Subscribers;
            public double TotalMs;
            public double AvgUs;
            public long AvgTicks;
        }

        [ContextMenu("Run External Library Benchmark")]
        public void RunBenchmark()
        {
            if (isRunning) return;
            isRunning = true;
            _results.Clear();

            bool prevPerfMode = MmRelayNode.PerformanceMode;
            MmRelayNode.PerformanceMode = true;

            try
            {
                foreach (int subs in subscriberCounts)
                {
                    Debug.Log($"[ExternalLibBenchmark] Testing {subs} subscribers...");
                    RunWithSubscriberCount(subs);
                }

                PrintResults();
                if (exportCsv) ExportCsv();
            }
            catch (Exception e)
            {
                Debug.LogError($"[ExternalLibBenchmark] {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                MmRelayNode.PerformanceMode = prevPerfMode;
                isRunning = false;
            }
        }

        private void RunWithSubscriberCount(int subCount)
        {
            // ---- C# delegate (baseline for flat pub/sub) ----
            {
                Action<string> handler = null;
                int counter = 0;
                for (int i = 0; i < subCount; i++)
                    handler += (msg) => counter++;

                // Warm up
                for (int i = 0; i < warmUp; i++)
                    handler?.Invoke("w");

                Measure("C# Delegate", subCount, () => handler?.Invoke("bench"));
            }

            // ---- Mercury BroadcastValue (flat, no spatial) ----
            {
                var root = new GameObject("MercRoot");
                var rootRelay = root.AddComponent<MmRelayNode>();

                for (int i = 0; i < subCount; i++)
                {
                    var go = new GameObject($"S_{i}");
                    go.transform.SetParent(root.transform);
                    var relay = go.AddComponent<MmRelayNode>();
                    go.AddComponent<BenchReceiver>();
                    rootRelay.MmAddToRoutingTable(relay, MmLevelFilter.Child);
                    relay.AddParent(rootRelay);
                }

                foreach (var r in root.GetComponentsInChildren<MmRelayNode>())
                    r.MmRefreshResponders();

                // Warm up
                for (int i = 0; i < warmUp; i++)
                    rootRelay.BroadcastValue("w");

                Measure("Mercury Broadcast", subCount, () => rootRelay.BroadcastValue("bench"));

                DestroyImmediate(root);
            }

#if HAS_R3
            // ---- R3 Subject<string> ----
            {
                var subject = new Subject<string>();
                int counter = 0;
                var disposables = new List<IDisposable>();

                for (int i = 0; i < subCount; i++)
                    disposables.Add(subject.Subscribe(msg => counter++));

                // Warm up
                for (int i = 0; i < warmUp; i++)
                    subject.OnNext("w");

                Measure("R3 Subject", subCount, () => subject.OnNext("bench"));

                foreach (var d in disposables)
                    d.Dispose();
                subject.Dispose();
            }
#else
            if (subCount == subscriberCounts[0]) // Log once
            {
                Debug.Log("[ExternalLibBenchmark] R3 not installed (Unity 6 compatibility issue with NuGet core).");
                Debug.Log("  Published numbers (from R3 README, BenchmarkDotNet on .NET 8):");
                Debug.Log("  R3 Subject.OnNext: ~comparable to raw C# event (zero-alloc)");
                Debug.Log("  To install: NuGetForUnity > R3 v1.3.0 FIRST, then UPM git URL with #1.3.0 tag");
            }
#endif

#if HAS_MESSAGEPIPE
            // ---- MessagePipe ----
            {
                var builder = new BuiltinContainerBuilder();
                builder.AddMessagePipe();
                builder.AddMessageBroker<string>();
                var provider = builder.BuildServiceProvider();
                GlobalMessagePipe.SetProvider(provider);

                var publisher = GlobalMessagePipe.GetPublisher<string>();
                var subscriber = GlobalMessagePipe.GetSubscriber<string>();

                int counter = 0;
                var bag = DisposableBag.CreateBuilder();
                for (int i = 0; i < subCount; i++)
                    subscriber.Subscribe(msg => counter++).AddTo(bag);

                var disposable = bag.Build();

                // Warm up
                for (int i = 0; i < warmUp; i++)
                    publisher.Publish("w");

                Measure("MessagePipe", subCount, () => publisher.Publish("bench"));

                disposable.Dispose();
            }
#else
            if (subCount == subscriberCounts[0]) // Log once
            {
                Debug.Log("[ExternalLibBenchmark] MessagePipe not installed (requires UniTask, Unity 6 compatibility issue).");
                Debug.Log("  Published numbers (from MessagePipe README, BenchmarkDotNet):");
                Debug.Log("  MessagePipe Publish: ~faster than C# event, zero-alloc, 78x faster than Prism EventAggregator");
                Debug.Log("  Uses BuiltinContainerBuilder (no VContainer/Zenject required for benchmarks)");
            }
#endif
        }

        private void Measure(string name, int subs, Action action)
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

            _results.Add(new Result
            {
                Method = name,
                Subscribers = subs,
                TotalMs = sw.Elapsed.TotalMilliseconds,
                AvgUs = (sw.Elapsed.TotalMilliseconds / iterations) * 1000.0,
                AvgTicks = sw.ElapsedTicks / iterations
            });
        }

        private void PrintResults()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== External Library Benchmark ===");
            sb.AppendLine($"Iterations: {iterations}");
            sb.AppendLine("Note: Mercury overhead includes hierarchy routing (unique feature).");
            sb.AppendLine("R3/MessagePipe are flat pub/sub — no hierarchy, no spatial filtering.");
            sb.AppendLine();

            sb.AppendLine($"{"Method",-22} {"Subs",5} {"Total ms",10} {"Avg us",10} {"Avg ticks",10}");
            sb.AppendLine(new string('-', 58));

            int currentSubs = -1;
            foreach (var r in _results)
            {
                if (r.Subscribers != currentSubs)
                {
                    if (currentSubs > 0) sb.AppendLine();
                    currentSubs = r.Subscribers;
                }
                sb.AppendLine($"{r.Method,-22} {r.Subscribers,5} {r.TotalMs,10:F2} {r.AvgUs,10:F3} {r.AvgTicks,10}");
            }

            Debug.Log(sb.ToString());
        }

        private void ExportCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("method,subscribers,iterations,total_ms,avg_us,avg_ticks");

            foreach (var r in _results)
            {
                sb.AppendLine($"{r.Method},{r.Subscribers},{iterations}," +
                              $"{r.TotalMs:F4},{r.AvgUs:F4},{r.AvgTicks}");
            }

            string path = Path.Combine(Application.dataPath,
                "Research", "Benchmarks", "Results", "external_library_benchmark.csv");
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, sb.ToString());
            Debug.Log($"[ExternalLibBenchmark] CSV exported to: {path}");
        }
    }
}
