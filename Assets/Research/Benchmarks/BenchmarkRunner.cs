using System.Collections;
using UnityEngine;

namespace MercuryMessaging.Research.Benchmarks
{
    /// <summary>
    /// Auto-runs all benchmarks in sequence on Play Mode start.
    /// Attach to a GameObject, enter Play Mode, wait for completion.
    /// </summary>
    public class BenchmarkRunner : MonoBehaviour
    {
        [Header("Which benchmarks to run")]
        public bool runUnified = true;
        public bool runSpatialFilter = true;
        public bool runExternal = true;
        public bool runScenario = true;
        public bool runThroughput = true;

        [Header("Override durations (0 = use defaults)")]
        [Tooltip("Override test duration for scenario/throughput (seconds). 0 = use default.")]
        public float overrideDuration = 0f;

        [Header("Status")]
        public string currentBenchmark = "Waiting...";
        public bool allComplete;

        private IEnumerator Start()
        {
            Debug.Log("========================================");
            Debug.Log("[BenchmarkRunner] Starting all benchmarks");
            Debug.Log("========================================");

            // 1. Unified (synchronous)
            if (runUnified)
            {
                currentBenchmark = "UnifiedBenchmark";
                Debug.Log("[BenchmarkRunner] >>> UnifiedBenchmark");
                var ub = gameObject.AddComponent<UnifiedBenchmark>();
                yield return null; // let component initialize
                ub.RunBenchmark();
                yield return new WaitWhile(() => ub.isRunning);
                Debug.Log("[BenchmarkRunner] <<< UnifiedBenchmark done");
                Destroy(ub);
                yield return null;
            }

            // 2. Spatial Filter (synchronous)
            if (runSpatialFilter)
            {
                currentBenchmark = "SpatialFilterBenchmark";
                Debug.Log("[BenchmarkRunner] >>> SpatialFilterBenchmark");
                var sf = gameObject.AddComponent<SpatialFilterBenchmark>();
                yield return null;
                sf.RunBenchmark();
                yield return new WaitWhile(() => sf.isRunning);
                Debug.Log("[BenchmarkRunner] <<< SpatialFilterBenchmark done");
                Destroy(sf);
                yield return null;
            }

            // 3. External Library (synchronous)
            if (runExternal)
            {
                currentBenchmark = "ExternalLibraryBenchmark";
                Debug.Log("[BenchmarkRunner] >>> ExternalLibraryBenchmark");
                var el = gameObject.AddComponent<ExternalLibraryBenchmark>();
                yield return null;
                el.RunBenchmark();
                yield return new WaitWhile(() => el.isRunning);
                Debug.Log("[BenchmarkRunner] <<< ExternalLibraryBenchmark done");
                Destroy(el);
                yield return null;
            }

            // 4. Scenario (coroutine-based, needs Play Mode)
            if (runScenario)
            {
                currentBenchmark = "ScenarioBenchmark";
                Debug.Log("[BenchmarkRunner] >>> ScenarioBenchmark");
                var sb = gameObject.AddComponent<ScenarioBenchmark>();
                yield return null;
                if (overrideDuration > 0) sb.testDuration = overrideDuration;
                sb.RunBenchmark();
                yield return new WaitWhile(() => sb.isRunning);
                Debug.Log("[BenchmarkRunner] <<< ScenarioBenchmark done");
                Destroy(sb);
                yield return null;
            }

            // 5. Throughput (coroutine-based, needs Play Mode)
            if (runThroughput)
            {
                currentBenchmark = "ThroughputBenchmark";
                Debug.Log("[BenchmarkRunner] >>> ThroughputBenchmark");
                var tb = gameObject.AddComponent<ThroughputBenchmark>();
                yield return null;
                if (overrideDuration > 0) tb.testDuration = overrideDuration;
                tb.RunBenchmark();
                yield return new WaitWhile(() => tb.isRunning);
                Debug.Log("[BenchmarkRunner] <<< ThroughputBenchmark done");
                Destroy(tb);
                yield return null;
            }

            currentBenchmark = "ALL COMPLETE";
            allComplete = true;
            Debug.Log("========================================");
            Debug.Log("[BenchmarkRunner] ALL BENCHMARKS COMPLETE");
            Debug.Log("Results in Assets/Research/Benchmarks/Results/");
            Debug.Log("========================================");
        }
    }
}
