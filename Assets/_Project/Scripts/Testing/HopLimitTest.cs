using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Testing
{
    public class HopLimitTest : MonoBehaviour
    {
        public int chainDepth = 50;
        public int maxHops = 25;

        private List<GameObject> chainNodes = new List<GameObject>();

        public IEnumerator Execute()
        {
            // Create chain
            for (int i = 0; i < chainDepth; i++)
            {
                var node = new GameObject($"ChainNode_{i}");
                var relay = node.AddComponent<MmRelayNode>();
                var responder = node.AddComponent<TestCounterResponder>();

                relay.maxMessageHops = maxHops;

                if (i > 0)
                {
                    node.transform.SetParent(chainNodes[i - 1].transform);
                }

                chainNodes.Add(node);
            }

            yield return null; // Let hierarchy settle

            // Send message from root
            TestCounterResponder.resetCount = 0;
            chainNodes[0].GetComponent<MmRelayNode>().MmInvoke(
                new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid,
                new MmMetadataBlock(MmLevelFilter.SelfAndChildren))
            );

            yield return new WaitForSeconds(0.1f);

            bool passed = TestCounterResponder.resetCount <= maxHops + 5; // Allow some tolerance
            string details = $"Received by {TestCounterResponder.resetCount}/{chainDepth} nodes (limit: {maxHops})";

            var display = FindObjectOfType<TestResultDisplay>();
            display?.DisplayResult("QW-1: Hop Limits", passed, 0f, details);
            display?.AppendLog($"  Chain depth: {chainDepth}");
            display?.AppendLog($"  Max hops: {maxHops}");
            display?.AppendLog($"  Messages received: {TestCounterResponder.resetCount}");
            display?.AppendLog($"  Result: {(passed ? "PASS" : "FAIL")}");

            // Cleanup
            foreach (var node in chainNodes)
            {
                if (node != null) Destroy(node);
            }
            chainNodes.Clear();
        }

        private class TestCounterResponder : MmBaseResponder
        {
            public static int resetCount = 0;
            protected override void ReceivedInitialize()
            {
                resetCount++;
            }
        }
    }
}
