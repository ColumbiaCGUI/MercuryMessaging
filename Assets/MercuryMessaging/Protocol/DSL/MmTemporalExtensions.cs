using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MercuryMessaging.Protocol.DSL
{
    /// <summary>
    /// Temporal extension methods for MmRelayNode providing time-based messaging patterns.
    /// Includes delayed execution, repeating messages, and async request/response patterns.
    ///
    /// Phase 3 - Task 3.3 (Async/Await Support) and Task 3.4 (Temporal Extensions)
    /// </summary>
    public static class MmTemporalExtensions
    {
        #region Delayed Execution (Task 3.4)

        /// <summary>
        /// Send a message after a delay.
        /// Returns a handle that can be used to cancel the delayed send.
        /// </summary>
        /// <param name="relay">The source relay node.</param>
        /// <param name="delay">Delay in seconds before sending.</param>
        /// <param name="method">The message method to send.</param>
        /// <returns>A handle to cancel the delayed operation.</returns>
        /// <example>
        /// var handle = relay.After(2f, MmMethod.Initialize);
        /// // Later: handle.Cancel();
        /// </example>
        public static MmTemporalHandle After(this MmRelayNode relay, float delay, MmMethod method)
        {
            return After(relay, delay, method, MmMetadataBlockHelper.Default);
        }

        /// <summary>
        /// Send a message after a delay with metadata.
        /// </summary>
        public static MmTemporalHandle After(this MmRelayNode relay, float delay, MmMethod method, MmMetadataBlock metadata)
        {
            var handle = new MmTemporalHandle();
            MmTemporalRunner.Instance.StartCoroutine(DelayedInvokeCoroutine(
                relay, delay, () => relay.MmInvoke(method, metadata), handle.Token
            ));
            return handle;
        }

        /// <summary>
        /// Send a boolean message after a delay.
        /// </summary>
        public static MmTemporalHandle After(this MmRelayNode relay, float delay, MmMethod method, bool value)
        {
            var handle = new MmTemporalHandle();
            MmTemporalRunner.Instance.StartCoroutine(DelayedInvokeCoroutine(
                relay, delay, () => relay.MmInvoke(method, value, MmMetadataBlockHelper.Default), handle.Token
            ));
            return handle;
        }

        /// <summary>
        /// Send an integer message after a delay.
        /// </summary>
        public static MmTemporalHandle After(this MmRelayNode relay, float delay, MmMethod method, int value)
        {
            var handle = new MmTemporalHandle();
            MmTemporalRunner.Instance.StartCoroutine(DelayedInvokeCoroutine(
                relay, delay, () => relay.MmInvoke(method, value, MmMetadataBlockHelper.Default), handle.Token
            ));
            return handle;
        }

        /// <summary>
        /// Send a float message after a delay.
        /// </summary>
        public static MmTemporalHandle After(this MmRelayNode relay, float delay, MmMethod method, float value)
        {
            var handle = new MmTemporalHandle();
            MmTemporalRunner.Instance.StartCoroutine(DelayedInvokeCoroutine(
                relay, delay, () => relay.MmInvoke(method, value, MmMetadataBlockHelper.Default), handle.Token
            ));
            return handle;
        }

        /// <summary>
        /// Send a string message after a delay.
        /// </summary>
        public static MmTemporalHandle After(this MmRelayNode relay, float delay, MmMethod method, string value)
        {
            var handle = new MmTemporalHandle();
            MmTemporalRunner.Instance.StartCoroutine(DelayedInvokeCoroutine(
                relay, delay, () => relay.MmInvoke(method, value, MmMetadataBlockHelper.Default), handle.Token
            ));
            return handle;
        }

        /// <summary>
        /// Send a message object after a delay.
        /// </summary>
        public static MmTemporalHandle After(this MmRelayNode relay, float delay, MmMessage message)
        {
            var handle = new MmTemporalHandle();
            MmTemporalRunner.Instance.StartCoroutine(DelayedInvokeCoroutine(
                relay, delay, () => relay.MmInvoke(message), handle.Token
            ));
            return handle;
        }

        private static IEnumerator DelayedInvokeCoroutine(MmRelayNode relay, float delay, Action action, CancellationToken token)
        {
            yield return new WaitForSeconds(delay);

            if (!token.IsCancellationRequested && relay != null)
            {
                action?.Invoke();
            }
        }

        #endregion

        #region Repeating Messages (Task 3.4)

        /// <summary>
        /// Send a message repeatedly at an interval.
        /// Returns a handle to stop the repeating messages.
        /// </summary>
        /// <param name="relay">The source relay node.</param>
        /// <param name="interval">Interval in seconds between sends.</param>
        /// <param name="method">The message method to send.</param>
        /// <param name="repeatCount">Optional max repeat count (0 = infinite).</param>
        /// <returns>A handle to cancel the repeating operation.</returns>
        /// <example>
        /// var handle = relay.Every(1f, MmMethod.Refresh);
        /// // Later: handle.Cancel();
        /// </example>
        public static MmTemporalHandle Every(this MmRelayNode relay, float interval, MmMethod method, int repeatCount = 0)
        {
            return Every(relay, interval, method, MmMetadataBlockHelper.Default, repeatCount);
        }

        /// <summary>
        /// Send a message repeatedly with metadata.
        /// </summary>
        public static MmTemporalHandle Every(this MmRelayNode relay, float interval, MmMethod method, MmMetadataBlock metadata, int repeatCount = 0)
        {
            var handle = new MmTemporalHandle();
            MmTemporalRunner.Instance.StartCoroutine(RepeatingInvokeCoroutine(
                relay, interval, () => relay.MmInvoke(method, metadata), repeatCount, handle.Token
            ));
            return handle;
        }

        /// <summary>
        /// Send a boolean message repeatedly.
        /// </summary>
        public static MmTemporalHandle Every(this MmRelayNode relay, float interval, MmMethod method, bool value, int repeatCount = 0)
        {
            var handle = new MmTemporalHandle();
            MmTemporalRunner.Instance.StartCoroutine(RepeatingInvokeCoroutine(
                relay, interval, () => relay.MmInvoke(method, value, MmMetadataBlockHelper.Default), repeatCount, handle.Token
            ));
            return handle;
        }

        /// <summary>
        /// Send an integer message repeatedly.
        /// </summary>
        public static MmTemporalHandle Every(this MmRelayNode relay, float interval, MmMethod method, int value, int repeatCount = 0)
        {
            var handle = new MmTemporalHandle();
            MmTemporalRunner.Instance.StartCoroutine(RepeatingInvokeCoroutine(
                relay, interval, () => relay.MmInvoke(method, value, MmMetadataBlockHelper.Default), repeatCount, handle.Token
            ));
            return handle;
        }

        /// <summary>
        /// Send a message repeatedly.
        /// </summary>
        public static MmTemporalHandle Every(this MmRelayNode relay, float interval, MmMessage message, int repeatCount = 0)
        {
            var handle = new MmTemporalHandle();
            MmTemporalRunner.Instance.StartCoroutine(RepeatingInvokeCoroutine(
                relay, interval, () => relay.MmInvoke(message), repeatCount, handle.Token
            ));
            return handle;
        }

        private static IEnumerator RepeatingInvokeCoroutine(MmRelayNode relay, float interval, Action action, int repeatCount, CancellationToken token)
        {
            var wait = new WaitForSeconds(interval);
            int count = 0;

            while (!token.IsCancellationRequested && relay != null)
            {
                yield return wait;

                if (token.IsCancellationRequested || relay == null)
                    break;

                action?.Invoke();

                if (repeatCount > 0)
                {
                    count++;
                    if (count >= repeatCount)
                        break;
                }
            }
        }

        #endregion

        #region Conditional Execution (Task 3.4)

        /// <summary>
        /// Send a message when a condition becomes true.
        /// Polls the condition each frame until it returns true.
        /// </summary>
        /// <param name="relay">The source relay node.</param>
        /// <param name="condition">The condition to check.</param>
        /// <param name="method">The message method to send when condition is met.</param>
        /// <param name="timeout">Optional timeout in seconds (0 = no timeout).</param>
        /// <returns>A handle to cancel the operation.</returns>
        /// <example>
        /// relay.When(() => player.IsReady, MmMethod.Initialize);
        /// </example>
        public static MmTemporalHandle When(this MmRelayNode relay, Func<bool> condition, MmMethod method, float timeout = 0f)
        {
            var handle = new MmTemporalHandle();
            MmTemporalRunner.Instance.StartCoroutine(ConditionalInvokeCoroutine(
                relay, condition, () => relay.MmInvoke(method, MmMetadataBlockHelper.Default), timeout, handle.Token
            ));
            return handle;
        }

        /// <summary>
        /// Send a message when a condition becomes true.
        /// </summary>
        public static MmTemporalHandle When(this MmRelayNode relay, Func<bool> condition, MmMessage message, float timeout = 0f)
        {
            var handle = new MmTemporalHandle();
            MmTemporalRunner.Instance.StartCoroutine(ConditionalInvokeCoroutine(
                relay, condition, () => relay.MmInvoke(message), timeout, handle.Token
            ));
            return handle;
        }

        /// <summary>
        /// Execute an action when a condition becomes true.
        /// </summary>
        public static MmTemporalHandle When(this MmRelayNode relay, Func<bool> condition, Action action, float timeout = 0f)
        {
            var handle = new MmTemporalHandle();
            MmTemporalRunner.Instance.StartCoroutine(ConditionalInvokeCoroutine(
                relay, condition, action, timeout, handle.Token
            ));
            return handle;
        }

        private static IEnumerator ConditionalInvokeCoroutine(MmRelayNode relay, Func<bool> condition, Action action, float timeout, CancellationToken token)
        {
            float elapsed = 0f;

            while (!token.IsCancellationRequested && relay != null)
            {
                if (condition != null && condition())
                {
                    action?.Invoke();
                    yield break;
                }

                yield return null;
                elapsed += Time.deltaTime;

                if (timeout > 0 && elapsed >= timeout)
                {
                    Debug.LogWarning("MmTemporalExtensions.When: Condition timed out");
                    yield break;
                }
            }
        }

        #endregion

        #region Async Request/Response (Task 3.3)

        /// <summary>
        /// Async request with response. Uses Task-based async pattern.
        /// Note: Prefer callback-based Query() for Unity performance.
        /// </summary>
        /// <typeparam name="T">Expected response message type.</typeparam>
        /// <param name="relay">The source relay node.</param>
        /// <param name="method">The query method.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The response message.</returns>
        /// <example>
        /// var response = await relay.RequestAsync&lt;MmMessageInt&gt;(MmMethod.MessageInt, timeout: 2f);
        /// Debug.Log($"Got value: {response.value}");
        /// </example>
        public static async Task<T> RequestAsync<T>(
            this MmRelayNode relay,
            MmMethod method,
            float timeout = 5f,
            CancellationToken cancellationToken = default) where T : MmMessage
        {
            return await RequestAsync<T>(relay, method, MmMetadataBlockHelper.Default, timeout, cancellationToken);
        }

        /// <summary>
        /// Async request with custom metadata.
        /// </summary>
        public static async Task<T> RequestAsync<T>(
            this MmRelayNode relay,
            MmMethod method,
            MmMetadataBlock metadata,
            float timeout = 5f,
            CancellationToken cancellationToken = default) where T : MmMessage
        {
            var tcs = new TaskCompletionSource<T>();
            int queryId = RegisterAsyncQuery<T>(tcs);

            // Set up timeout
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(timeout));

            // Register cancellation
            cts.Token.Register(() =>
            {
                UnregisterAsyncQuery(queryId);
                tcs.TrySetCanceled();
            });

            // Send the query (using pool for efficiency)
            var queryMessage = MmMessagePool.GetInt(queryId, method, metadata);
            relay.MmInvoke(queryMessage);
            MmMessagePool.Return(queryMessage);

            try
            {
                return await tcs.Task;
            }
            finally
            {
                cts.Dispose();
            }
        }

        /// <summary>
        /// Send an async response to a request.
        /// </summary>
        public static void RespondAsync<T>(this MmRelayNode relay, int queryId, T response) where T : MmMessage
        {
            ResolveAsyncQuery(queryId, response);
        }

        // Internal query management
        private static readonly Dictionary<int, object> _asyncQueries = new Dictionary<int, object>();
        private static int _asyncQueryId = 1;
        private static readonly object _queryLock = new object();

        private static int RegisterAsyncQuery<T>(TaskCompletionSource<T> tcs) where T : MmMessage
        {
            lock (_queryLock)
            {
                int id = _asyncQueryId++;
                _asyncQueries[id] = tcs;
                return id;
            }
        }

        private static void UnregisterAsyncQuery(int queryId)
        {
            lock (_queryLock)
            {
                _asyncQueries.Remove(queryId);
            }
        }

        private static void ResolveAsyncQuery<T>(int queryId, T response) where T : MmMessage
        {
            lock (_queryLock)
            {
                if (_asyncQueries.TryGetValue(queryId, out var obj) && obj is TaskCompletionSource<T> tcs)
                {
                    _asyncQueries.Remove(queryId);
                    tcs.TrySetResult(response);
                }
            }
        }

        #endregion

        #region Fluent Temporal Builder

        /// <summary>
        /// Start a temporal message builder for fluent delayed/repeating operations.
        /// </summary>
        /// <param name="relay">The source relay node.</param>
        /// <param name="method">The message method.</param>
        /// <returns>A temporal builder for chaining.</returns>
        /// <example>
        /// relay.Schedule(MmMethod.Refresh)
        ///     .ToDescendants()
        ///     .After(2f)
        ///     .Execute();
        /// </example>
        public static MmTemporalBuilder Schedule(this MmRelayNode relay, MmMethod method)
        {
            return new MmTemporalBuilder(relay, method, null);
        }

        /// <summary>
        /// Start a temporal message builder with a payload.
        /// </summary>
        public static MmTemporalBuilder Schedule(this MmRelayNode relay, MmMethod method, object payload)
        {
            return new MmTemporalBuilder(relay, method, payload);
        }

        #endregion
    }

    #region Support Types

    /// <summary>
    /// Handle for cancelling temporal operations.
    /// </summary>
    public class MmTemporalHandle
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Get the cancellation token for this handle.
        /// </summary>
        public CancellationToken Token => _cts.Token;

        /// <summary>
        /// Whether this operation has been cancelled.
        /// </summary>
        public bool IsCancelled => _cts.IsCancellationRequested;

        /// <summary>
        /// Cancel the temporal operation.
        /// </summary>
        public void Cancel()
        {
            _cts.Cancel();
        }
    }

    /// <summary>
    /// Fluent builder for temporal message operations.
    /// </summary>
    public struct MmTemporalBuilder
    {
        private readonly MmRelayNode _relay;
        private readonly MmMethod _method;
        private readonly object _payload;
        private MmMetadataBlock _metadata;
        private float _delay;
        private float _interval;
        private int _repeatCount;

        public MmTemporalBuilder(MmRelayNode relay, MmMethod method, object payload)
        {
            _relay = relay;
            _method = method;
            _payload = payload;
            _metadata = MmMetadataBlockHelper.Default;
            _delay = 0f;
            _interval = 0f;
            _repeatCount = 0;
        }

        /// <summary>
        /// Target children with the message.
        /// </summary>
        public MmTemporalBuilder ToChildren()
        {
            _metadata = new MmMetadataBlock(MmLevelFilter.Child, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Local);
            return this;
        }

        /// <summary>
        /// Target descendants with the message.
        /// </summary>
        public MmTemporalBuilder ToDescendants()
        {
            _metadata = new MmMetadataBlock(MmLevelFilter.Descendants, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Local);
            return this;
        }

        /// <summary>
        /// Target parents with the message.
        /// </summary>
        public MmTemporalBuilder ToParents()
        {
            _metadata = new MmMetadataBlock(MmLevelFilter.Parent, MmActiveFilter.All, MmSelectedFilter.All, MmNetworkFilter.Local);
            return this;
        }

        /// <summary>
        /// Set a delay before sending.
        /// </summary>
        public MmTemporalBuilder After(float seconds)
        {
            _delay = seconds;
            return this;
        }

        /// <summary>
        /// Make the message repeat at an interval.
        /// </summary>
        public MmTemporalBuilder Every(float seconds, int maxRepeats = 0)
        {
            _interval = seconds;
            _repeatCount = maxRepeats;
            return this;
        }

        /// <summary>
        /// Execute the temporal operation.
        /// </summary>
        /// <returns>A handle to cancel the operation.</returns>
        public MmTemporalHandle Execute()
        {
            if (_interval > 0)
            {
                // Repeating
                return _relay.Every(_interval, _method, _metadata, _repeatCount);
            }
            else if (_delay > 0)
            {
                // Delayed
                return _relay.After(_delay, _method, _metadata);
            }
            else
            {
                // Immediate - still return a handle for consistency
                _relay.MmInvoke(_method, _metadata);
                return new MmTemporalHandle();
            }
        }
    }

    /// <summary>
    /// MonoBehaviour that runs temporal coroutines.
    /// Auto-creates on first use and persists across scene loads.
    /// </summary>
    internal class MmTemporalRunner : MonoBehaviour
    {
        private static MmTemporalRunner _instance;

        public static MmTemporalRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[MmTemporalRunner]");
                    go.hideFlags = HideFlags.HideAndDontSave;
                    _instance = go.AddComponent<MmTemporalRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void OnDestroy()
        {
            _instance = null;
        }
    }

    #endregion
}
