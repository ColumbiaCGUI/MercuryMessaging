// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Performance benchmark tests for MmExtendableResponder

using NUnit.Framework;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.TestTools;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Performance benchmarks for MmExtendableResponder custom method handling.
    /// Validates that performance targets are met for fast and slow paths.
    /// </summary>
    [TestFixture]
    public class MmExtendableResponderPerformanceTests
    {
        private const int WARMUP_ITERATIONS = 100;
        private const int TEST_ITERATIONS = 10000;

        private GameObject baselineObject;
        private GameObject extendableObject;
        private MmBaseResponder baselineResponder;
        private MmExtendableResponder extendableResponder;
        private TestExtendableResponder testExtendableResponder;

        #region Test Setup

        [SetUp]
        public void SetUp()
        {
            // Create baseline responder (MmBaseResponder)
            baselineObject = new GameObject("BaselineObject");
            baselineResponder = baselineObject.AddComponent<MmBaseResponder>();

            // Create extendable responder (MmExtendableResponder)
            extendableObject = new GameObject("ExtendableObject");
            testExtendableResponder = extendableObject.AddComponent<TestExtendableResponder>();
            testExtendableResponder.RegisterHandler((MmMethod)1000, msg => { /* Simple handler */ });
            testExtendableResponder.RegisterHandler((MmMethod)1001, msg => { /* Simple handler */ });
            testExtendableResponder.RegisterHandler((MmMethod)1002, msg => { /* Simple handler */ });
            extendableResponder = testExtendableResponder;
        }

        [TearDown]
        public void TearDown()
        {
            if (baselineObject != null)
                UnityEngine.Object.DestroyImmediate(baselineObject);
            if (extendableObject != null)
                UnityEngine.Object.DestroyImmediate(extendableObject);
        }

        private class TestExtendableResponder : MmExtendableResponder
        {
            public void RegisterHandler(MmMethod method, System.Action<MmMessage> handler)
            {
                RegisterCustomHandler(method, handler);
            }
        }

        #endregion

        #region Benchmark Tests

        [Test]
        [Category("Performance")]
        public void Benchmark_Baseline_StandardMethod()
        {
            // Benchmark MmBaseResponder with standard method (SetActive)
            var message = new MmMessageBool { MmMethod = MmMethod.SetActive, value = true };

            // Warmup
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
                baselineResponder.MmInvoke(message);

            // Actual benchmark
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < TEST_ITERATIONS; i++)
                baselineResponder.MmInvoke(message);
            stopwatch.Stop();

            double avgNanoseconds = (stopwatch.Elapsed.TotalMilliseconds * 1_000_000) / TEST_ITERATIONS;
            UnityEngine.Debug.Log($"[Baseline] Avg dispatch time: {avgNanoseconds:F2} ns per message ({TEST_ITERATIONS} iterations)");

            // Assert baseline is reasonable (< 2000ns in Unity Editor, accounting for GC, profiling, and Editor overhead)
            // Production builds are typically 2-5x faster. Use PerformanceTestHarness for accurate production metrics.
            Assert.Less(avgNanoseconds, 2000,
                $"Baseline dispatch should be < 2000ns, got {avgNanoseconds:F2}ns");
        }

        [Test]
        [Category("Performance")]
        public void Benchmark_FastPath_StandardMethod()
        {
            // Benchmark MmExtendableResponder with standard method (fast path)
            var message = new MmMessageBool { MmMethod = MmMethod.SetActive, value = true };

            // Warmup
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
                extendableResponder.MmInvoke(message);

            // Actual benchmark
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < TEST_ITERATIONS; i++)
                extendableResponder.MmInvoke(message);
            stopwatch.Stop();

            double avgNanoseconds = (stopwatch.Elapsed.TotalMilliseconds * 1_000_000) / TEST_ITERATIONS;
            UnityEngine.Debug.Log($"[Fast Path] Avg dispatch time: {avgNanoseconds:F2} ns per message ({TEST_ITERATIONS} iterations)");

            // Assert fast path is within acceptable overhead vs baseline (< 2500ns in Unity Editor)
            // Production builds are typically 2-5x faster. Use PerformanceTestHarness for accurate production metrics.
            Assert.Less(avgNanoseconds, 2500,
                $"Fast path dispatch should be < 2500ns, got {avgNanoseconds:F2}ns");
        }

        [Test]
        [Category("Performance")]
        public void Benchmark_SlowPath_CustomMethod()
        {
            // Benchmark MmExtendableResponder with custom method (slow path)
            var message = new MmMessage { MmMethod = (MmMethod)1000 };

            // Warmup
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
                extendableResponder.MmInvoke(message);

            // Actual benchmark
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < TEST_ITERATIONS; i++)
                extendableResponder.MmInvoke(message);
            stopwatch.Stop();

            double avgNanoseconds = (stopwatch.Elapsed.TotalMilliseconds * 1_000_000) / TEST_ITERATIONS;
            UnityEngine.Debug.Log($"[Slow Path] Avg dispatch time: {avgNanoseconds:F2} ns per message ({TEST_ITERATIONS} iterations)");

            // Assert slow path is reasonable (< 1500ns, accounting for Unity Editor overhead)
            Assert.Less(avgNanoseconds, 1500,
                $"Slow path dispatch should be < 1500ns, got {avgNanoseconds:F2}ns");
        }

        [Test]
        [Category("Performance")]
        public void Benchmark_SlowPath_MultipleCustomMethods()
        {
            // Benchmark with multiple different custom methods (tests dictionary lookup)
            var messages = new MmMessage[]
            {
                new MmMessage { MmMethod = (MmMethod)1000 },
                new MmMessage { MmMethod = (MmMethod)1001 },
                new MmMessage { MmMethod = (MmMethod)1002 },
            };

            // Warmup
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
                extendableResponder.MmInvoke(messages[i % 3]);

            // Actual benchmark
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < TEST_ITERATIONS; i++)
                extendableResponder.MmInvoke(messages[i % 3]);
            stopwatch.Stop();

            double avgNanoseconds = (stopwatch.Elapsed.TotalMilliseconds * 1_000_000) / TEST_ITERATIONS;
            UnityEngine.Debug.Log($"[Slow Path Multi] Avg dispatch time: {avgNanoseconds:F2} ns per message ({TEST_ITERATIONS} iterations)");

            // Assert slow path with varying methods is still reasonable
            Assert.Less(avgNanoseconds, 1000,
                $"Slow path (multi) dispatch should be < 1000ns, got {avgNanoseconds:F2}ns");
        }

        [Test]
        [Category("Performance")]
        public void Benchmark_MemoryOverhead()
        {
            // Measure memory overhead of registered handlers
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            long beforeMemory = System.GC.GetTotalMemory(false);

            // Create 100 responders with 3 handlers each
            var responders = new GameObject[100];
            for (int i = 0; i < 100; i++)
            {
                responders[i] = new GameObject($"Responder{i}");
                var responder = responders[i].AddComponent<TestExtendableResponder>();
                responder.RegisterHandler((MmMethod)1000, msg => { });
                responder.RegisterHandler((MmMethod)1001, msg => { });
                responder.RegisterHandler((MmMethod)1002, msg => { });
            }

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            long afterMemory = System.GC.GetTotalMemory(false);
            long memoryDelta = afterMemory - beforeMemory;
            double memoryPerResponder = memoryDelta / 100.0;

            UnityEngine.Debug.Log($"[Memory] Total overhead: {memoryDelta / 1024.0:F2} KB ({memoryPerResponder:F2} bytes per responder)");

            // Cleanup
            foreach (var obj in responders)
                UnityEngine.Object.DestroyImmediate(obj);

            // Assert memory overhead is reasonable (< 2 KB per responder with 3 handlers)
            // Note: Each responder now includes an auto-added MmRelayNode via [RequireComponent]
            Assert.Less(memoryPerResponder, 2048,
                $"Memory per responder should be < 2 KB, got {memoryPerResponder:F2} bytes");
        }

        [Test]
        [Category("Performance")]
        public void Benchmark_Comparison_Summary()
        {
            // Run all benchmarks and print comparison table
            var baselineMessage = new MmMessageBool { MmMethod = MmMethod.SetActive, value = true };
            var customMessage = new MmMessage { MmMethod = (MmMethod)1000 };

            // Warmup all
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
            {
                baselineResponder.MmInvoke(baselineMessage);
                extendableResponder.MmInvoke(baselineMessage);
                extendableResponder.MmInvoke(customMessage);
            }

            // Benchmark baseline
            var swBaseline = Stopwatch.StartNew();
            for (int i = 0; i < TEST_ITERATIONS; i++)
                baselineResponder.MmInvoke(baselineMessage);
            swBaseline.Stop();
            double baselineNs = (swBaseline.Elapsed.TotalMilliseconds * 1_000_000) / TEST_ITERATIONS;

            // Benchmark fast path
            var swFast = Stopwatch.StartNew();
            for (int i = 0; i < TEST_ITERATIONS; i++)
                extendableResponder.MmInvoke(baselineMessage);
            swFast.Stop();
            double fastNs = (swFast.Elapsed.TotalMilliseconds * 1_000_000) / TEST_ITERATIONS;

            // Benchmark slow path
            var swSlow = Stopwatch.StartNew();
            for (int i = 0; i < TEST_ITERATIONS; i++)
                extendableResponder.MmInvoke(customMessage);
            swSlow.Stop();
            double slowNs = (swSlow.Elapsed.TotalMilliseconds * 1_000_000) / TEST_ITERATIONS;

            // Print comparison table
            UnityEngine.Debug.Log("\n" +
                "=================================================================\n" +
                "MmExtendableResponder Performance Comparison\n" +
                "=================================================================\n" +
                $"Baseline (MmBaseResponder):           {baselineNs:F2} ns/msg\n" +
                $"Fast Path (Standard Methods 0-999):   {fastNs:F2} ns/msg  ({(fastNs / baselineNs * 100):F1}% of baseline)\n" +
                $"Slow Path (Custom Methods 1000+):     {slowNs:F2} ns/msg  ({(slowNs / baselineNs):F1}x slower)\n" +
                "=================================================================\n" +
                $"Iterations: {TEST_ITERATIONS:N0}\n" +
                "Target: Fast path < 2500ns, Slow path < 3000ns (Unity Editor overhead)\n" +
                "Production builds are typically 2-5x faster.\n" +
                "=================================================================\n");

            // Assert all targets met (adjusted for Unity Editor overhead, GC, profiling)
            // Production builds should validate with PerformanceTestHarness for accurate metrics
            Assert.Less(fastNs, 2500, "Fast path target not met");
            Assert.Less(slowNs, 3000, "Slow path target not met");
        }

        #endregion

        #region Real-World Scenario Tests

        [Test]
        [Category("Performance")]
        public void Benchmark_RealWorld_MixedMessages()
        {
            // Simulate real-world usage: 70% standard methods, 30% custom methods
            var messages = new MmMessage[10];
            messages[0] = new MmMessageBool { MmMethod = MmMethod.SetActive, value = true };
            messages[1] = new MmMessage { MmMethod = MmMethod.Initialize };
            messages[2] = new MmMessage { MmMethod = MmMethod.Initialize }; // Use Initialize instead of Refresh (requires MmMessageTransformList)
            messages[3] = new MmMessageString { MmMethod = MmMethod.MessageString, value = "test" };
            messages[4] = new MmMessageInt { MmMethod = MmMethod.MessageInt, value = 42 };
            messages[5] = new MmMessageFloat { MmMethod = MmMethod.MessageFloat, value = 3.14f };
            messages[6] = new MmMessageBool { MmMethod = MmMethod.MessageBool, value = false };
            messages[7] = new MmMessage { MmMethod = (MmMethod)1000 }; // Custom
            messages[8] = new MmMessage { MmMethod = (MmMethod)1001 }; // Custom
            messages[9] = new MmMessage { MmMethod = (MmMethod)1002 }; // Custom

            // Warmup
            for (int i = 0; i < WARMUP_ITERATIONS; i++)
                extendableResponder.MmInvoke(messages[i % 10]);

            // Actual benchmark
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < TEST_ITERATIONS; i++)
                extendableResponder.MmInvoke(messages[i % 10]);
            stopwatch.Stop();

            double avgNanoseconds = (stopwatch.Elapsed.TotalMilliseconds * 1_000_000) / TEST_ITERATIONS;
            UnityEngine.Debug.Log($"[Real-World Mix] Avg dispatch time: {avgNanoseconds:F2} ns per message ({TEST_ITERATIONS} iterations)");

            // Assert mixed workload is reasonable (adjusted for Unity Editor overhead)
            Assert.Less(avgNanoseconds, 1200,
                $"Real-world mixed dispatch should be < 1200ns, got {avgNanoseconds:F2}ns");
        }

        #endregion
    }
}
