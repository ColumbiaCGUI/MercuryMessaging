using System.Collections;
using UnityEngine;

namespace MercuryMessaging.Testing
{
    public class CycleDetectionTest : MonoBehaviour
    {
        private GameObject nodeA, nodeB, nodeC;

        public IEnumerator Execute()
        {
            // Create circular graph: A → B → C → A
            nodeA = new GameObject("NodeA");
            nodeB = new GameObject("NodeB");
            nodeC = new GameObject("NodeC");

            var relayA = nodeA.AddComponent<MmRelayNode>();
            var relayB = nodeB.AddComponent<MmRelayNode>();
            var relayC = nodeC.AddComponent<MmRelayNode>();

            relayA.enableCycleDetection = true;
            relayB.enableCycleDetection = true;
            relayC.enableCycleDetection = true;

            // Set up circular references manually in routing table
            relayA.RoutingTable.Add(new MmRoutingTableItem(relayB, MmLevelFilter.Child));
            relayB.RoutingTable.Add(new MmRoutingTableItem(relayC, MmLevelFilter.Child));
            relayC.RoutingTable.Add(new MmRoutingTableItem(relayA, MmLevelFilter.Child));

            yield return null;

            // Send message - should detect cycle
            var message = new MmMessage(MmMethod.Initialize);
            relayA.MmInvoke(message);

            yield return new WaitForSeconds(0.1f);

            // If we get here without freezing, cycle detection worked
            bool passed = message.VisitedNodes != null && message.VisitedNodes.Count >= 1;
            string details = $"Visited {message.VisitedNodes?.Count ?? 0} nodes, no infinite loop";

            var display = FindObjectOfType<TestResultDisplay>();
            display?.DisplayResult("QW-1: Cycle Detection", passed, 0f, details);
            display?.AppendLog($"  Cycle detection enabled: {relayA.enableCycleDetection}");
            display?.AppendLog($"  Nodes visited: {message.VisitedNodes?.Count ?? 0}");
            display?.AppendLog($"  Result: {(passed ? "PASS - No freeze" : "FAIL")}");

            // Cleanup
            if (nodeA != null) Destroy(nodeA);
            if (nodeB != null) Destroy(nodeB);
            if (nodeC != null) Destroy(nodeC);
        }
    }
}
