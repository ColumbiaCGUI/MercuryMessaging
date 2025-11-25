using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MercuryMessaging.Tests.Performance
{
    /// <summary>
    /// Generates messages at a specified rate for performance testing.
    /// </summary>
    public class MessageGenerator : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Messages per second to generate")]
        [Range(1, 1000)]
        public int messagesPerSecond = 100;

        [Tooltip("Message method to send")]
        public MmMethod messageMethod = MmMethod.MessageString;

        [Tooltip("Level filter for messages")]
        public MmLevelFilter levelFilter = MmLevelFilterHelper.SelfAndChildren;

        [Tooltip("Active filter for messages")]
        public MmActiveFilter activeFilter = MmActiveFilter.Active;

        [Tooltip("Tag for messages")]
        public MmTag messageTag = MmTagHelper.Everything;

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
                    SendTestMessage();
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
                SendTestMessage();
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
                    Debug.LogError("[MessageGenerator] No MmRelayNode found! Cannot send messages.");
                    enabled = false;
                    return;
                }
            }

            // Find test harnesses if not set
            if (testHarness == null)
            {
                testHarness = FindObjectOfType<PerformanceTestHarness>();
            }
            if (comparisonHarness == null)
            {
                comparisonHarness = FindObjectOfType<ComparisonTestHarness>();
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
                Debug.LogWarning("[MessageGenerator] Already generating messages!");
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
            Debug.Log($"[MessageGenerator] Started generating {messagesPerSecond} messages/second ({mode} mode)");
        }

        /// <summary>
        /// Stop generating messages.
        /// </summary>
        public void StopGenerating()
        {
            if (!_isGenerating)
            {
                Debug.LogWarning("[MessageGenerator] Not generating messages!");
                return;
            }

            _isGenerating = false;
            Debug.Log($"[MessageGenerator] Stopped. Total messages sent: {totalMessagesSent}");
        }

        private IEnumerator GenerateMessages()
        {
            while (_isGenerating)
            {
                // Send message
                SendTestMessage();

                // Wait for interval
                yield return new WaitForSeconds(_messageInterval);
            }
        }

        private void SendTestMessage()
        {
            if (relayNode == null) return;

            _messageCounter++;

            // Create metadata
            MmMetadataBlock metadata = new MmMetadataBlock(
                messageTag,
                levelFilter,
                activeFilter,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            );

            // Start timing the API call
            _sendTimer.Restart();

            // Send appropriate message type
            switch (messageMethod)
            {
                case MmMethod.MessageString:
                    relayNode.MmInvoke(MmMethod.MessageString, $"Test_{_messageCounter}", metadata);
                    break;

                case MmMethod.MessageInt:
                    relayNode.MmInvoke(MmMethod.MessageInt, _messageCounter, metadata);
                    break;

                case MmMethod.MessageFloat:
                    relayNode.MmInvoke(MmMethod.MessageFloat, (float)_messageCounter, metadata);
                    break;

                case MmMethod.MessageBool:
                    relayNode.MmInvoke(MmMethod.MessageBool, _messageCounter % 2 == 0, metadata);
                    break;

                case MmMethod.Initialize:
                    relayNode.MmInvoke(MmMethod.Initialize, metadata);
                    break;

                case MmMethod.Refresh:
                    relayNode.MmInvoke(MmMethod.Refresh, metadata);
                    break;

                default:
                    // For other methods, send as NoOp
                    relayNode.MmInvoke(MmMethod.NoOp, metadata);
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
                comparisonHarness.OnTraditionalMessageSent(elapsedTicks);
            }
        }

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
