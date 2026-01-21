// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Network Performance Benchmark
// Compares latency and throughput between FishNet and Fusion 2 backends

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Network;
using Debug = UnityEngine.Debug;

namespace MercuryMessaging.Tests.Network
{
    /// <summary>
    /// Performance benchmark for comparing network backends (FishNet vs Fusion 2).
    ///
    /// Metrics measured:
    /// - Serialization time (MmBinarySerializer)
    /// - Loopback round-trip time (local echo)
    /// - Message throughput (messages/second)
    /// - Memory allocation per message
    ///
    /// Usage:
    /// 1. Add to a GameObject in any scene
    /// 2. Press Play
    /// 3. Use GUI buttons to run benchmarks
    /// 4. Results logged to console and displayed in GUI
    /// </summary>
    public class NetworkPerformanceBenchmark : MonoBehaviour
    {
        [Header("Benchmark Configuration")]
        [Tooltip("Number of messages to send per benchmark iteration")]
        public int messagesPerIteration = 1000;

        [Tooltip("Number of iterations for averaging results")]
        public int iterations = 5;

        [Tooltip("Warmup iterations before measuring")]
        public int warmupIterations = 2;

        [Header("Results")]
        [SerializeField] private List<BenchmarkResult> results = new List<BenchmarkResult>();

        private MmLoopbackBackend _loopbackBackend;
        private int _messagesReceived;
        private Stopwatch _stopwatch = new Stopwatch();
        private bool _isBenchmarking;

        [Serializable]
        public class BenchmarkResult
        {
            public string testName;
            public string backendName;
            public double avgTimeMs;
            public double minTimeMs;
            public double maxTimeMs;
            public double throughputMsgPerSec;
            public int messageCount;
            public long memoryAllocatedBytes;
            public string timestamp;

            public override string ToString()
            {
                return $"{testName} ({backendName}): {avgTimeMs:F3}ms avg, {throughputMsgPerSec:F0} msg/sec";
            }
        }

        #region Unity Lifecycle

        private void Start()
        {
            InitializeLoopbackBackend();
        }

        private void OnDestroy()
        {
            _loopbackBackend?.Shutdown();
        }

        #endregion

        #region Initialization

        private void InitializeLoopbackBackend()
        {
            _loopbackBackend = new MmLoopbackBackend
            {
                Mode = MmLoopbackBackend.LoopbackMode.Echo,
                UseMessageQueue = false
            };
            _loopbackBackend.OnMessageReceived += OnLoopbackMessageReceived;
            _loopbackBackend.Initialize();

            Debug.Log("[Benchmark] Loopback backend initialized for benchmarking");
        }

        private void OnLoopbackMessageReceived(byte[] data, int senderId)
        {
            _messagesReceived++;
        }

        #endregion

        #region Benchmark Tests

        /// <summary>
        /// Run all benchmarks sequentially.
        /// </summary>
        public void RunAllBenchmarks()
        {
            if (_isBenchmarking)
            {
                Debug.LogWarning("[Benchmark] Already running");
                return;
            }

            StartCoroutine(RunAllBenchmarksCoroutine());
        }

        private IEnumerator RunAllBenchmarksCoroutine()
        {
            _isBenchmarking = true;
            results.Clear();

            Debug.Log("=== Starting Network Performance Benchmarks ===");
            Debug.Log($"Configuration: {messagesPerIteration} messages x {iterations} iterations (+ {warmupIterations} warmup)");
            Debug.Log("");

            // Serialization benchmarks
            yield return StartCoroutine(BenchmarkSerialization());
            yield return null;

            // Loopback throughput benchmarks
            yield return StartCoroutine(BenchmarkLoopbackThroughput());
            yield return null;

            // Message type comparison
            yield return StartCoroutine(BenchmarkMessageTypes());
            yield return null;

            // Print summary
            PrintSummary();

            _isBenchmarking = false;
        }

        /// <summary>
        /// Benchmark serialization/deserialization performance.
        /// </summary>
        public IEnumerator BenchmarkSerialization()
        {
            Debug.Log("--- Serialization Benchmark ---");

            var messageTypes = new (string name, Func<MmMessage> create)[]
            {
                ("MmVoid", () => new MmMessage(MmMetadataBlockHelper.Default, MmMessageType.MmVoid)),
                ("MmInt", () => new MmMessageInt(42, MmMethod.MessageInt, MmMetadataBlockHelper.Default)),
                ("MmString", () => new MmMessageString("Hello Network Benchmark!", MmMethod.MessageString, MmMetadataBlockHelper.Default)),
                ("MmVector3", () => new MmMessageVector3(Vector3.one, MmMethod.MessageVector3, MmMetadataBlockHelper.Default)),
                ("MmTransform", () => new MmMessageTransform(new MmTransform(Vector3.one, Vector3.one, Quaternion.identity), MmMethod.MessageTransform, MmMetadataBlockHelper.Default)),
            };

            foreach (var (name, create) in messageTypes)
            {
                var times = new List<double>();
                var msg = create();

                // Warmup
                for (int w = 0; w < warmupIterations; w++)
                {
                    for (int i = 0; i < messagesPerIteration; i++)
                    {
                        var data = MmBinarySerializer.Serialize(msg);
                        MmBinarySerializer.Deserialize(data);
                    }
                }

                // Measure
                long memBefore = GC.GetTotalMemory(true);

                for (int iter = 0; iter < iterations; iter++)
                {
                    _stopwatch.Restart();

                    for (int i = 0; i < messagesPerIteration; i++)
                    {
                        var data = MmBinarySerializer.Serialize(msg);
                        MmBinarySerializer.Deserialize(data);
                    }

                    _stopwatch.Stop();
                    times.Add(_stopwatch.Elapsed.TotalMilliseconds);
                }

                long memAfter = GC.GetTotalMemory(false);

                var result = new BenchmarkResult
                {
                    testName = $"Serialize_{name}",
                    backendName = "MmBinarySerializer",
                    avgTimeMs = Average(times),
                    minTimeMs = Min(times),
                    maxTimeMs = Max(times),
                    throughputMsgPerSec = messagesPerIteration / (Average(times) / 1000.0),
                    messageCount = messagesPerIteration * iterations,
                    memoryAllocatedBytes = memAfter - memBefore,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                results.Add(result);
                Debug.Log($"  {result}");

                yield return null;
            }
        }

        /// <summary>
        /// Benchmark loopback round-trip throughput.
        /// </summary>
        public IEnumerator BenchmarkLoopbackThroughput()
        {
            Debug.Log("--- Loopback Throughput Benchmark ---");

            var times = new List<double>();

            // Warmup
            for (int w = 0; w < warmupIterations; w++)
            {
                _messagesReceived = 0;
                for (int i = 0; i < messagesPerIteration; i++)
                {
                    var msg = new MmMessageInt(i, MmMethod.MessageInt, MmMetadataBlockHelper.Default);
                    var data = MmBinarySerializer.Serialize(msg);
                    _loopbackBackend.SendToServer(data);
                }
            }

            // Measure
            long memBefore = GC.GetTotalMemory(true);

            for (int iter = 0; iter < iterations; iter++)
            {
                _messagesReceived = 0;
                _stopwatch.Restart();

                for (int i = 0; i < messagesPerIteration; i++)
                {
                    var msg = new MmMessageInt(i, MmMethod.MessageInt, MmMetadataBlockHelper.Default);
                    var data = MmBinarySerializer.Serialize(msg);
                    _loopbackBackend.SendToServer(data);
                }

                _stopwatch.Stop();
                times.Add(_stopwatch.Elapsed.TotalMilliseconds);
            }

            long memAfter = GC.GetTotalMemory(false);

            var result = new BenchmarkResult
            {
                testName = "Loopback_Throughput",
                backendName = "MmLoopbackBackend",
                avgTimeMs = Average(times),
                minTimeMs = Min(times),
                maxTimeMs = Max(times),
                throughputMsgPerSec = messagesPerIteration / (Average(times) / 1000.0),
                messageCount = messagesPerIteration * iterations,
                memoryAllocatedBytes = memAfter - memBefore,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            results.Add(result);
            Debug.Log($"  {result}");

            yield return null;
        }

        /// <summary>
        /// Compare performance across different message types.
        /// </summary>
        public IEnumerator BenchmarkMessageTypes()
        {
            Debug.Log("--- Message Type Comparison ---");

            var messages = new (string name, MmMessage msg)[]
            {
                ("Small (MmVoid)", new MmMessage(MmMetadataBlockHelper.Default, MmMessageType.MmVoid)),
                ("Medium (MmString_16)", new MmMessageString("1234567890123456", MmMethod.MessageString, MmMetadataBlockHelper.Default)),
                ("Large (MmString_256)", new MmMessageString(new string('X', 256), MmMethod.MessageString, MmMetadataBlockHelper.Default)),
                ("Complex (MmTransform)", new MmMessageTransform(new MmTransform(Vector3.one, Vector3.one, Quaternion.Euler(45, 90, 0)), MmMethod.MessageTransform, MmMetadataBlockHelper.Default)),
            };

            foreach (var (name, msg) in messages)
            {
                var times = new List<double>();
                var data = MmBinarySerializer.Serialize(msg);
                int payloadSize = data.Length;

                // Warmup
                for (int w = 0; w < warmupIterations; w++)
                {
                    _messagesReceived = 0;
                    for (int i = 0; i < messagesPerIteration; i++)
                    {
                        _loopbackBackend.SendToServer(data);
                    }
                }

                // Measure
                for (int iter = 0; iter < iterations; iter++)
                {
                    _messagesReceived = 0;
                    _stopwatch.Restart();

                    for (int i = 0; i < messagesPerIteration; i++)
                    {
                        _loopbackBackend.SendToServer(data);
                    }

                    _stopwatch.Stop();
                    times.Add(_stopwatch.Elapsed.TotalMilliseconds);
                }

                var result = new BenchmarkResult
                {
                    testName = $"MsgType_{name}",
                    backendName = $"Loopback ({payloadSize} bytes)",
                    avgTimeMs = Average(times),
                    minTimeMs = Min(times),
                    maxTimeMs = Max(times),
                    throughputMsgPerSec = messagesPerIteration / (Average(times) / 1000.0),
                    messageCount = messagesPerIteration * iterations,
                    memoryAllocatedBytes = payloadSize * messagesPerIteration * iterations,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                results.Add(result);
                Debug.Log($"  {result}");

                yield return null;
            }
        }

        #endregion

        #region Helpers

        private double Average(List<double> values)
        {
            if (values.Count == 0) return 0;
            double sum = 0;
            foreach (var v in values) sum += v;
            return sum / values.Count;
        }

        private double Min(List<double> values)
        {
            if (values.Count == 0) return 0;
            double min = double.MaxValue;
            foreach (var v in values) if (v < min) min = v;
            return min;
        }

        private double Max(List<double> values)
        {
            if (values.Count == 0) return 0;
            double max = double.MinValue;
            foreach (var v in values) if (v > max) max = v;
            return max;
        }

        private void PrintSummary()
        {
            Debug.Log("");
            Debug.Log("=== Benchmark Summary ===");
            foreach (var result in results)
            {
                Debug.Log($"  {result}");
            }
            Debug.Log("");
            Debug.Log($"Total tests: {results.Count}");
            Debug.Log("=========================");
        }

        /// <summary>
        /// Clear all benchmark results.
        /// </summary>
        public void ClearResults()
        {
            results.Clear();
            Debug.Log("[Benchmark] Results cleared");
        }

        /// <summary>
        /// Export results to CSV format.
        /// </summary>
        public string ExportToCsv()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("TestName,Backend,AvgTimeMs,MinTimeMs,MaxTimeMs,ThroughputMsgPerSec,MessageCount,MemoryBytes,Timestamp");

            foreach (var r in results)
            {
                sb.AppendLine($"{r.testName},{r.backendName},{r.avgTimeMs:F3},{r.minTimeMs:F3},{r.maxTimeMs:F3},{r.throughputMsgPerSec:F0},{r.messageCount},{r.memoryAllocatedBytes},{r.timestamp}");
            }

            return sb.ToString();
        }

        #endregion

        #region GUI

        private Vector2 _scrollPos;

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 500));

            GUILayout.Label("Network Performance Benchmark", GUI.skin.box);

            // Configuration
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Configuration:");
            GUILayout.Label($"  Messages per iteration: {messagesPerIteration}");
            GUILayout.Label($"  Iterations: {iterations}");
            GUILayout.Label($"  Warmup: {warmupIterations}");
            GUILayout.EndVertical();

            GUILayout.Space(5);

            // Controls
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Benchmarks:");

            GUI.enabled = !_isBenchmarking;

            if (GUILayout.Button("Run All Benchmarks"))
            {
                RunAllBenchmarks();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Serialization"))
            {
                StartCoroutine(BenchmarkSerialization());
            }
            if (GUILayout.Button("Loopback"))
            {
                StartCoroutine(BenchmarkLoopbackThroughput());
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Message Types"))
            {
                StartCoroutine(BenchmarkMessageTypes());
            }

            GUI.enabled = true;

            if (GUILayout.Button("Clear Results"))
            {
                ClearResults();
            }

            if (GUILayout.Button("Export CSV to Console"))
            {
                Debug.Log(ExportToCsv());
            }

            GUILayout.EndVertical();

            GUILayout.Space(5);

            // Status
            if (_isBenchmarking)
            {
                GUILayout.Label("RUNNING BENCHMARK...", GUI.skin.box);
            }

            // Results
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Results ({results.Count} tests):");
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(200));

            foreach (var result in results)
            {
                GUILayout.Label($"{result.testName}:");
                GUILayout.Label($"  {result.avgTimeMs:F2}ms avg | {result.throughputMsgPerSec:F0} msg/sec");
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndArea();
        }

        #endregion
    }
}
