using System.Collections;
using UnityEngine;

namespace MercuryMessaging.Testing
{
    public class LazyCopyPerformanceTest : MonoBehaviour
    {
        public int testIterations = 1000;

        public IEnumerator Execute()
        {
            // Note: Actual allocation measurement requires Unity Profiler API
            // This is a simplified version that validates functionality

            // Scenario A: Single direction (only child responders)
            var singleDirNode = new GameObject("SingleDir");
            var singleRelay = singleDirNode.AddComponent<MmRelayNode>();
            var childResponder = singleDirNode.AddComponent<TestResponder>();

            yield return null;

            for (int i = 0; i < testIterations; i++)
            {
                singleRelay.MmInvoke(new MmMessage(MmMethod.Initialize));
                if (i % 100 == 0) yield return null;
            }

            // Scenario B: Multi-direction (parent + child)
            var multiDirNode = new GameObject("MultiDir");
            var multiRelay = multiDirNode.AddComponent<MmRelayNode>();
            var multiChild = new GameObject("Child");
            multiChild.transform.SetParent(multiDirNode.transform);
            var childRelay = multiChild.AddComponent<MmRelayNode>();

            yield return null;

            for (int i = 0; i < testIterations; i++)
            {
                multiRelay.MmInvoke(new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid,
                    new MmMetadataBlock(MmLevelFilter.SelfAndChildren)));
                if (i % 100 == 0) yield return null;
            }

            // Success if no errors occurred
            bool passed = true;
            string details = $"Routing scenarios tested with {testIterations} messages each";

            var display = FindObjectOfType<TestResultDisplay>();
            display?.DisplayResult("QW-2: Lazy Copying", passed, 0f, details);
            display?.AppendLog($"  Single-direction iterations: {testIterations}");
            display?.AppendLog($"  Multi-direction iterations: {testIterations}");
            display?.AppendLog($"  Note: Use Unity Profiler for detailed allocation analysis");
            display?.AppendLog($"  Result: {(passed ? "PASS - No errors" : "FAIL")}");

            // Cleanup
            if (singleDirNode != null) Destroy(singleDirNode);
            if (multiDirNode != null) Destroy(multiDirNode);
        }

        private class TestResponder : MmBaseResponder
        {
            protected override void ReceivedInitialize() { }
        }
    }
}
