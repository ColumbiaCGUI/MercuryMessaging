using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Tests.Performance
{
    /// <summary>
    /// Simple test responder for performance testing.
    /// Receives messages and optionally logs them for validation.
    /// </summary>
    public class TestResponder : MmBaseResponder
    {
        [Header("Configuration")]
        [Tooltip("Log received messages (creates overhead)")]
        public bool logMessages = false;

        [Tooltip("Count received messages")]
        public bool countMessages = true;

        [Header("Statistics")]
        [Tooltip("Total messages received")]
        public int messagesReceived = 0;

        [Tooltip("Messages by type")]
        public int initializeCount = 0;
        public int refreshCount = 0;
        public int setActiveCount = 0;
        public int stringMessageCount = 0;
        public int intMessageCount = 0;
        public int floatMessageCount = 0;
        public int boolMessageCount = 0;
        public int otherMessageCount = 0;

        public override void Initialize()
        {
            if (countMessages)
            {
                messagesReceived++;
                initializeCount++;
            }

            if (logMessages)
            {
                Debug.Log($"[TestResponder:{name}] Received Initialize");
            }

            base.Initialize();
        }

        public override void Refresh(List<MmTransform> transformList)
        {
            if (countMessages)
            {
                messagesReceived++;
                refreshCount++;
            }

            if (logMessages)
            {
                Debug.Log($"[TestResponder:{name}] Received Refresh");
            }

            base.Refresh(transformList);
        }

        public override void SetActive(bool active)
        {
            if (countMessages)
            {
                messagesReceived++;
                setActiveCount++;
            }

            if (logMessages)
            {
                Debug.Log($"[TestResponder:{name}] Received SetActive: {active}");
            }

            base.SetActive(active);
        }

        protected override void ReceivedMessage(MmMessageString message)
        {
            if (countMessages)
            {
                messagesReceived++;
                stringMessageCount++;
            }

            if (logMessages)
            {
                Debug.Log($"[TestResponder:{name}] Received String: {message.value}");
            }

            base.ReceivedMessage(message);
        }

        protected override void ReceivedMessage(MmMessageInt message)
        {
            if (countMessages)
            {
                messagesReceived++;
                intMessageCount++;
            }

            if (logMessages)
            {
                Debug.Log($"[TestResponder:{name}] Received Int: {message.value}");
            }

            base.ReceivedMessage(message);
        }

        protected override void ReceivedMessage(MmMessageFloat message)
        {
            if (countMessages)
            {
                messagesReceived++;
                floatMessageCount++;
            }

            if (logMessages)
            {
                Debug.Log($"[TestResponder:{name}] Received Float: {message.value}");
            }

            base.ReceivedMessage(message);
        }

        protected override void ReceivedMessage(MmMessageBool message)
        {
            if (countMessages)
            {
                messagesReceived++;
                boolMessageCount++;
            }

            if (logMessages)
            {
                Debug.Log($"[TestResponder:{name}] Received Bool: {message.value}");
            }

            base.ReceivedMessage(message);
        }

        public override void MmInvoke(MmMessage message)
        {
            // Track other message types
            if (countMessages)
            {
                bool isTrackedType = message.MmMethod == MmMethod.Initialize ||
                                   message.MmMethod == MmMethod.Refresh ||
                                   message.MmMethod == MmMethod.SetActive ||
                                   message.MmMethod == MmMethod.MessageString ||
                                   message.MmMethod == MmMethod.MessageInt ||
                                   message.MmMethod == MmMethod.MessageFloat ||
                                   message.MmMethod == MmMethod.MessageBool;

                if (!isTrackedType)
                {
                    messagesReceived++;
                    otherMessageCount++;

                    if (logMessages)
                    {
                        Debug.Log($"[TestResponder:{name}] Received Other: {message.MmMethod}");
                    }
                }
            }

            base.MmInvoke(message);
        }

        /// <summary>
        /// Reset message counters.
        /// </summary>
        [ContextMenu("Reset Counters")]
        public void ResetCounters()
        {
            messagesReceived = 0;
            initializeCount = 0;
            refreshCount = 0;
            setActiveCount = 0;
            stringMessageCount = 0;
            intMessageCount = 0;
            floatMessageCount = 0;
            boolMessageCount = 0;
            otherMessageCount = 0;
        }

        /// <summary>
        /// Print current statistics to console.
        /// </summary>
        [ContextMenu("Print Statistics")]
        public void PrintStatistics()
        {
            Debug.Log($"[TestResponder:{name}] === Message Statistics ===");
            Debug.Log($"Total Messages: {messagesReceived}");
            Debug.Log($"  Initialize: {initializeCount}");
            Debug.Log($"  Refresh: {refreshCount}");
            Debug.Log($"  SetActive: {setActiveCount}");
            Debug.Log($"  String: {stringMessageCount}");
            Debug.Log($"  Int: {intMessageCount}");
            Debug.Log($"  Float: {floatMessageCount}");
            Debug.Log($"  Bool: {boolMessageCount}");
            Debug.Log($"  Other: {otherMessageCount}");
            Debug.Log("============================");
        }
    }
}
