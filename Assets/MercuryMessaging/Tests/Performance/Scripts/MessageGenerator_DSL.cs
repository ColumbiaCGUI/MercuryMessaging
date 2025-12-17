using System.Collections;
using System.Diagnostics;

using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MercuryMessaging.Tests.Performance
{
    /// <summary>
    /// DSL version of MessageGenerator for performance comparison testing.
    /// Uses the Fluent DSL API instead of traditional MmInvoke calls.
    ///
    /// This component generates messages using the DSL patterns:
    /// - relay.Broadcast(method, value) for descendant routing
    /// - relay.Send(value).ToChildren().Execute() for child routing
    ///
    /// Compare with MessageGenerator.cs which uses traditional MmInvoke API.
    /// </summary>
    public class MessageGenerator_DSL : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Messages per second to generate")]
        [Range(1, 1000)]
        public int messagesPerSecond = 100;

        [Tooltip("Message method to send")]
        public MmMethod messageMethod = MmMethod.MessageString;

        [Tooltip("Level filter for messages (maps to DSL routing method)")]
        public MmLevelFilter levelFilter = MmLevelFilterHelper.SelfAndChildren;

        [Tooltip("Active filter for messages (must match traditional generator for fair comparison)")]
        public MmActiveFilter activeFilter = MmActiveFilter.Active;

        [Tooltip("Start generating automatically on scene load")]
        public bool autoStart = true;

        [Tooltip("Burst mode - send all messages immediately without rate limiting (for stress testing)")]
        public bool burstMode = false;

        [Tooltip("Use frame-based generation instead of coroutine (better performance, more accurate)")]
        public bool useFrameBasedGeneration = true;

        [Header("References")]
        [Tooltip("Relay node to send messages through")]
        public MmRelayNode relayNode;

        [Tooltip("Performance test harness to notify (optional)")]
        public PerformanceTestHarness testHarness;

        [Tooltip("Comparison test harness to notify (optional)")]
        public ComparisonTestHarness comparisonHarness;

        [Header("Runtime Info")]
        [Tooltip("Total messages sent this session")]
        public int totalMessagesSent = 0;

        private bool _isGenerating = false;
        private float _messageInterval = 0f;
        private int _messageCounter = 0;
        private float _messageAccumulator = 0f;

        // Per-message timing for accurate overhead calculation
        private Stopwatch _sendTimer = new Stopwatch();
        private long _totalSendTicks = 0;
        private int _timedMessageCount = 0;

        /// <summary>
        /// Get average ticks per message for performance comparison.
        /// </summary>
        public double AverageTicksPerMessage => _timedMessageCount > 0 ? (double)_totalSendTicks / _timedMessageCount : 0;

        /// <summary>
        /// Get average nanoseconds per message.
        /// </summary>
        public double AverageNanosecondsPerMessage => AverageTicksPerMessage * (1_000_000_000.0 / Stopwatch.Frequency);

        private void Update()
        {
            // Frame-based message generation (more accurate than coroutine)
            if (!_isGenerating || !useFrameBasedGeneration) return;

            // Burst mode - send as many messages as possible this frame
            if (burstMode)
            {
                // Send multiple messages per frame
                for (int i = 0; i < messagesPerSecond / 60; i++)
                {
                    SendTestMessage_DSL();
                }
                return;
            }

            // Accumulate time and send messages when threshold reached
            _messageAccumulator += Time.deltaTime;

            // Calculate how many messages should be sent this frame
            int messagesToSend = Mathf.FloorToInt(_messageAccumulator / _messageInterval);

            // Send accumulated messages
            for (int i = 0; i < messagesToSend; i++)
            {
                SendTestMessage_DSL();
            }

            // Subtract sent message time from accumulator
            _messageAccumulator -= messagesToSend * _messageInterval;
        }

        private void Start()
        {
            // Find relay node if not set
            if (relayNode == null)
            {
                relayNode = GetComponent<MmRelayNode>();
                if (relayNode == null)
                {
                    relayNode = GetComponentInParent<MmRelayNode>();
                }

                if (relayNode == null)
                {
                    Debug.LogError("[MessageGenerator_DSL] No MmRelayNode found! Cannot send messages.");
                    enabled = false;
                    return;
                }
            }

            // Find test harnesses if not set
            if (testHarness == null)
            {
                testHarness = FindFirstObjectByType<PerformanceTestHarness>();
            }
            if (comparisonHarness == null)
            {
                comparisonHarness = FindFirstObjectByType<ComparisonTestHarness>();
            }

            // Calculate interval
            _messageInterval = 1f / messagesPerSecond;

            // Auto-start if enabled
            if (autoStart)
            {
                StartGenerating();
            }
        }

        /// <summary>
        /// Start generating messages.
        /// </summary>
        public void StartGenerating()
        {
            if (_isGenerating)
            {
                Debug.LogWarning("[MessageGenerator_DSL] Already generating messages!");
                return;
            }

            _isGenerating = true;
            totalMessagesSent = 0;
            _messageCounter = 0;
            _messageAccumulator = 0f;
            _totalSendTicks = 0;
            _timedMessageCount = 0;

            // Use coroutine only if frame-based generation is disabled
            if (!useFrameBasedGeneration)
            {
                StartCoroutine(GenerateMessages());
            }

            string mode = burstMode ? "BURST" : useFrameBasedGeneration ? "frame-based" : "coroutine";
            Debug.Log($"[MessageGenerator_DSL] Started generating {messagesPerSecond} messages/second ({mode} mode) using DSL API");
        }

        /// <summary>
        /// Stop generating messages.
        /// </summary>
        public void StopGenerating()
        {
            if (!_isGenerating)
            {
                Debug.LogWarning("[MessageGenerator_DSL] Not generating messages!");
                return;
            }

            _isGenerating = false;
            Debug.Log($"[MessageGenerator_DSL] Stopped. Total messages sent: {totalMessagesSent}");
        }

        private IEnumerator GenerateMessages()
        {
            while (_isGenerating)
            {
                // Send message
                SendTestMessage_DSL();

                // Wait for interval
                yield return new WaitForSeconds(_messageInterval);
            }
        }

        /// <summary>
        /// Send test message using DSL API.
        /// This is the key difference from MessageGenerator - uses Fluent DSL.
        /// </summary>
        private void SendTestMessage_DSL()
        {
            if (relayNode == null) return;

            _messageCounter++;

            // Start timing the API call
            _sendTimer.Restart();

            // Use DSL API based on level filter and message method
            switch (messageMethod)
            {
                case MmMethod.MessageString:
                    SendDSL_String($"Test_{_messageCounter}");
                    break;

                case MmMethod.MessageInt:
                    SendDSL_Int(_messageCounter);
                    break;

                case MmMethod.MessageFloat:
                    SendDSL_Float((float)_messageCounter);
                    break;

                case MmMethod.MessageBool:
                    SendDSL_Bool(_messageCounter % 2 == 0);
                    break;

                case MmMethod.Initialize:
                    SendDSL_Initialize();
                    break;

                case MmMethod.Refresh:
                    SendDSL_Refresh();
                    break;

                default:
                    // For other methods, use NoOp via fluent API
                    relayNode.Send(MmMethod.NoOp).ToDescendants().Execute();
                    break;
            }

            // Stop timing and record
            _sendTimer.Stop();
            long elapsedTicks = _sendTimer.ElapsedTicks;
            _totalSendTicks += elapsedTicks;
            _timedMessageCount++;

            totalMessagesSent++;

            // Notify test harnesses
            if (testHarness != null)
            {
                testHarness.OnMessageSent();
            }
            if (comparisonHarness != null)
            {
                comparisonHarness.OnDSLMessageSent(elapsedTicks);
            }
        }

        #region DSL Send Methods (by Level Filter)

        private void SendDSL_String(string value)
        {
            // Choose DSL method based on level filter
            // levelFilter is set by inspector to match traditional generator
            switch (levelFilter)
            {
                case MmLevelFilter.Child:
                    relayNode.Send(value).ToChildren().Execute();
                    break;
                case MmLevelFilter.Parent:
                    relayNode.Send(value).ToParents().Execute();
                    break;
                default:
                    // SelfAndChildren, Descendants, etc. - use fluent API fast path
                    relayNode.Send(value).ToDescendants().Execute();
                    break;
            }
        }

        private void SendDSL_Int(int value)
        {
            switch (levelFilter)
            {
                case MmLevelFilter.Child:
                    relayNode.Send(value).ToChildren().Execute();
                    break;
                case MmLevelFilter.Parent:
                    relayNode.Send(value).ToParents().Execute();
                    break;
                default:
                    relayNode.Send(value).ToDescendants().Execute();
                    break;
            }
        }

        private void SendDSL_Float(float value)
        {
            switch (levelFilter)
            {
                case MmLevelFilter.Child:
                    relayNode.Send(value).ToChildren().Execute();
                    break;
                case MmLevelFilter.Parent:
                    relayNode.Send(value).ToParents().Execute();
                    break;
                default:
                    relayNode.Send(value).ToDescendants().Execute();
                    break;
            }
        }

        private void SendDSL_Bool(bool value)
        {
            switch (levelFilter)
            {
                case MmLevelFilter.Child:
                    relayNode.Send(value).ToChildren().Execute();
                    break;
                case MmLevelFilter.Parent:
                    relayNode.Send(value).ToParents().Execute();
                    break;
                default:
                    relayNode.Send(value).ToDescendants().Execute();
                    break;
            }
        }

        private void SendDSL_Initialize()
        {
            switch (levelFilter)
            {
                case MmLevelFilter.Child:
                    relayNode.Initialize().ToChildren().Execute();
                    break;
                case MmLevelFilter.Parent:
                    relayNode.Initialize().ToParents().Execute();
                    break;
                default:
                    relayNode.BroadcastInitialize();
                    break;
            }
        }

        private void SendDSL_Refresh()
        {
            switch (levelFilter)
            {
                case MmLevelFilter.Child:
                    relayNode.Refresh().ToChildren().Execute();
                    break;
                case MmLevelFilter.Parent:
                    relayNode.Refresh().ToParents().Execute();
                    break;
                default:
                    relayNode.BroadcastRefresh();
                    break;
            }
        }

        #endregion

        [ContextMenu("Start")]
        private void StartFromMenu()
        {
            StartGenerating();
        }

        [ContextMenu("Stop")]
        private void StopFromMenu()
        {
            StopGenerating();
        }
    }
}
