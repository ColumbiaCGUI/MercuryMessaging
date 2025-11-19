using UnityEngine;

namespace MercuryMessaging.Testing
{
    /// <summary>
    /// Configuration helper for Quick Win validation tests.
    /// Provides inspector fields for adjusting test parameters.
    /// </summary>
    public class QuickWinConfigHelper : MonoBehaviour
    {
        [Header("CircularBuffer Test")]
        [Tooltip("Number of messages to send in CircularBuffer test")]
        public int circularBufferMessageCount = 10000;

        [Tooltip("Expected maximum buffer size")]
        public int expectedBufferSize = 100;

        [Tooltip("Maximum acceptable memory growth (MB)")]
        public int maxMemoryDeltaMB = 10;

        [Header("Hop Limit Test")]
        [Tooltip("Depth of node chain to create")]
        public int hopLimitChainDepth = 50;

        [Tooltip("Maximum hops before message is dropped")]
        public int maxHops = 25;

        [Header("Cycle Detection Test")]
        [Tooltip("Enable cycle detection for test")]
        public bool enableCycleDetection = true;

        [Header("Lazy Copy Test")]
        [Tooltip("Number of iterations for performance test")]
        public int lazyCopyIterations = 1000;

        [Tooltip("Expected performance improvement percentage")]
        [Range(0f, 100f)]
        public float expectedImprovement = 20f;

        private void Awake()
        {
            // Apply configuration to test components
            var circularBufferTest = FindObjectOfType<CircularBufferMemoryTest>();
            if (circularBufferTest != null)
            {
                circularBufferTest.messageCount = circularBufferMessageCount;
                circularBufferTest.expectedMaxSize = expectedBufferSize;
            }

            var hopLimitTest = FindObjectOfType<HopLimitTest>();
            if (hopLimitTest != null)
            {
                hopLimitTest.chainDepth = hopLimitChainDepth;
                hopLimitTest.maxHops = maxHops;
            }

            var lazyCopyTest = FindObjectOfType<LazyCopyPerformanceTest>();
            if (lazyCopyTest != null)
            {
                lazyCopyTest.testIterations = lazyCopyIterations;
            }
        }
    }
}
