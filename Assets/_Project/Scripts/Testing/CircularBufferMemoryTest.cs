using System.Collections;
using UnityEngine;

namespace MercuryMessaging.Testing
{
    public class CircularBufferMemoryTest : MonoBehaviour
    {
        public MmRelayNode testRelay;
        public int messageCount = 10000;
        public int expectedMaxSize = 100;

        public IEnumerator Execute()
        {
            if (testRelay == null)
            {
                testRelay = gameObject.AddComponent<MmRelayNode>();
                testRelay.messageHistorySize = expectedMaxSize;
            }

            yield return null; // Let Awake run

            long memoryBefore = System.GC.GetTotalMemory(true);

            // Send many messages
            for (int i = 0; i < messageCount; i++)
            {
                testRelay.MmInvoke(new MmMessage(MmMethod.Initialize));
                if (i % 1000 == 0) yield return null;
            }

            long memoryAfter = System.GC.GetTotalMemory(true);
            long memoryDelta = (memoryAfter - memoryBefore) / 1024 / 1024; // MB

            // Check message history size
            bool sizeCheck = testRelay.messageInList.Count <= expectedMaxSize;
            bool memoryCheck = memoryDelta < 10; // Less than 10MB growth

            bool passed = sizeCheck && memoryCheck;
            string details = $"History: {testRelay.messageInList.Count}/{expectedMaxSize}, Memory: {memoryDelta}MB";

            var display = FindObjectOfType<TestResultDisplay>();
            display?.DisplayResult("QW-4: CircularBuffer", passed, 0f, details);
            display?.AppendLog($"  Messages sent: {messageCount}");
            display?.AppendLog($"  Buffer size: {testRelay.messageInList.Count} (max: {expectedMaxSize})");
            display?.AppendLog($"  Memory delta: {memoryDelta} MB");
            display?.AppendLog($"  Result: {(passed ? "PASS" : "FAIL")}");
        }
    }
}
