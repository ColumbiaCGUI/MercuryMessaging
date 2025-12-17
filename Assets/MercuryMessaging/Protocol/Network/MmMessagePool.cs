// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmMessagePool.cs - Object pooling for network messages
// Reduces GC pressure by reusing message objects and byte arrays

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Network
{
    /// <summary>
    /// Generic object pool for reusable objects.
    /// Thread-safe implementation.
    /// </summary>
    public class MmObjectPool<T> where T : class, new()
    {
        private readonly Stack<T> _pool = new Stack<T>();
        private readonly object _lock = new object();
        private readonly int _maxSize;
        private readonly Action<T> _resetAction;

        /// <summary>
        /// Number of objects currently in the pool.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _pool.Count;
                }
            }
        }

        /// <summary>
        /// Total objects created by this pool.
        /// </summary>
        public int TotalCreated { get; private set; }

        /// <summary>
        /// Total get operations.
        /// </summary>
        public int TotalGets { get; private set; }

        /// <summary>
        /// Total return operations.
        /// </summary>
        public int TotalReturns { get; private set; }

        /// <summary>
        /// Hit rate (percentage of gets served from pool).
        /// </summary>
        public float HitRate => TotalGets > 0 ? 1.0f - (float)TotalCreated / TotalGets : 0f;

        /// <summary>
        /// Create a new object pool.
        /// </summary>
        /// <param name="maxSize">Maximum pool size (excess returns are discarded)</param>
        /// <param name="resetAction">Action to reset object state before reuse</param>
        public MmObjectPool(int maxSize = 100, Action<T> resetAction = null)
        {
            _maxSize = maxSize;
            _resetAction = resetAction;
        }

        /// <summary>
        /// Prewarm the pool with objects.
        /// </summary>
        public void Prewarm(int count)
        {
            lock (_lock)
            {
                for (int i = 0; i < count && _pool.Count < _maxSize; i++)
                {
                    _pool.Push(new T());
                    TotalCreated++;
                }
            }
        }

        /// <summary>
        /// Get an object from the pool.
        /// </summary>
        public T Get()
        {
            TotalGets++;

            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    var obj = _pool.Pop();
                    // Mark poolable objects as from pool
                    if (obj is IMmPoolable poolable)
                    {
                        poolable.Reset(); // This sets IsFromPool = true for MmPooledMessage
                    }
                    return obj;
                }
            }

            TotalCreated++;
            var newObj = new T();
            // Mark new poolable objects as from pool (they will be returned to pool later)
            if (newObj is IMmPoolable newPoolable)
            {
                newPoolable.Reset(); // This sets IsFromPool = true for MmPooledMessage
            }
            return newObj;
        }

        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null) return;

            TotalReturns++;
            _resetAction?.Invoke(obj);

            lock (_lock)
            {
                if (_pool.Count < _maxSize)
                {
                    _pool.Push(obj);
                }
                // Excess objects are discarded (GC will collect them)
            }
        }

        /// <summary>
        /// Clear the pool.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _pool.Clear();
            }
        }

        /// <summary>
        /// Get pool statistics.
        /// </summary>
        public string GetStats()
        {
            return $"Pool<{typeof(T).Name}>: Count={Count}/{_maxSize}, Created={TotalCreated}, Gets={TotalGets}, Returns={TotalReturns}, HitRate={HitRate:P1}";
        }
    }

    /// <summary>
    /// Pool for byte arrays of various sizes.
    /// Uses bucketed allocation for efficient memory reuse.
    /// </summary>
    public class MmByteArrayPool
    {
        // Bucket sizes: 64, 256, 1024, 4096, 16384, 65536
        private static readonly int[] BucketSizes = { 64, 256, 1024, 4096, 16384, 65536 };
        private readonly Stack<byte[]>[] _buckets;
        private readonly object[] _locks;
        private readonly int _maxArraysPerBucket;

        /// <summary>
        /// Total arrays created.
        /// </summary>
        public int TotalCreated { get; private set; }

        /// <summary>
        /// Total get operations.
        /// </summary>
        public int TotalGets { get; private set; }

        /// <summary>
        /// Total return operations.
        /// </summary>
        public int TotalReturns { get; private set; }

        /// <summary>
        /// Create a new byte array pool.
        /// </summary>
        public MmByteArrayPool(int maxArraysPerBucket = 50)
        {
            _maxArraysPerBucket = maxArraysPerBucket;
            _buckets = new Stack<byte[]>[BucketSizes.Length];
            _locks = new object[BucketSizes.Length];

            for (int i = 0; i < BucketSizes.Length; i++)
            {
                _buckets[i] = new Stack<byte[]>();
                _locks[i] = new object();
            }
        }

        /// <summary>
        /// Get a byte array of at least the specified size.
        /// </summary>
        public byte[] Get(int minSize)
        {
            TotalGets++;

            int bucketIndex = GetBucketIndex(minSize);
            if (bucketIndex < 0)
            {
                // Too large for pooling, allocate directly
                TotalCreated++;
                return new byte[minSize];
            }

            lock (_locks[bucketIndex])
            {
                if (_buckets[bucketIndex].Count > 0)
                {
                    return _buckets[bucketIndex].Pop();
                }
            }

            TotalCreated++;
            return new byte[BucketSizes[bucketIndex]];
        }

        /// <summary>
        /// Return a byte array to the pool.
        /// </summary>
        public void Return(byte[] array)
        {
            if (array == null) return;

            TotalReturns++;

            int bucketIndex = GetExactBucketIndex(array.Length);
            if (bucketIndex < 0)
            {
                // Not a pooled size, let GC collect it
                return;
            }

            // Clear sensitive data
            Array.Clear(array, 0, array.Length);

            lock (_locks[bucketIndex])
            {
                if (_buckets[bucketIndex].Count < _maxArraysPerBucket)
                {
                    _buckets[bucketIndex].Push(array);
                }
            }
        }

        /// <summary>
        /// Get a scoped array that returns itself when disposed.
        /// </summary>
        public ScopedArray GetScoped(int minSize)
        {
            return new ScopedArray(this, Get(minSize));
        }

        /// <summary>
        /// Clear all buckets.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _buckets.Length; i++)
            {
                lock (_locks[i])
                {
                    _buckets[i].Clear();
                }
            }
        }

        private int GetBucketIndex(int size)
        {
            for (int i = 0; i < BucketSizes.Length; i++)
            {
                if (BucketSizes[i] >= size)
                {
                    return i;
                }
            }
            return -1; // Too large
        }

        private int GetExactBucketIndex(int size)
        {
            for (int i = 0; i < BucketSizes.Length; i++)
            {
                if (BucketSizes[i] == size)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get pool statistics.
        /// </summary>
        public string GetStats()
        {
            var stats = new System.Text.StringBuilder();
            stats.AppendLine($"ByteArrayPool: Created={TotalCreated}, Gets={TotalGets}, Returns={TotalReturns}");
            for (int i = 0; i < BucketSizes.Length; i++)
            {
                lock (_locks[i])
                {
                    stats.AppendLine($"  Bucket[{BucketSizes[i]}]: {_buckets[i].Count}/{_maxArraysPerBucket}");
                }
            }
            return stats.ToString();
        }

        /// <summary>
        /// Scoped array that returns to pool on dispose.
        /// </summary>
        public struct ScopedArray : IDisposable
        {
            private readonly MmByteArrayPool _pool;
            public byte[] Array { get; }

            public ScopedArray(MmByteArrayPool pool, byte[] array)
            {
                _pool = pool;
                Array = array;
            }

            public void Dispose()
            {
                _pool.Return(Array);
            }
        }
    }

    /// <summary>
    /// Poolable message wrapper that tracks pool origin.
    /// </summary>
    public interface IMmPoolable
    {
        /// <summary>
        /// Reset object state for reuse.
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// Pooled wrapper for MmMessage objects.
    /// </summary>
    public class MmPooledMessage : IMmPoolable
    {
        public MmMessage Message { get; set; }
        public byte[] SerializedData { get; set; }
        public bool IsFromPool { get; set; }

        public void Reset()
        {
            Message = null;
            SerializedData = null;
            IsFromPool = true;
        }
    }

    /// <summary>
    /// Central message pool manager.
    /// Provides unified access to all network-related pools.
    /// </summary>
    public class MmMessagePoolManager
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static MmMessagePoolManager Instance { get; } = new MmMessagePoolManager();

        /// <summary>
        /// Pool for byte arrays.
        /// </summary>
        public MmByteArrayPool ByteArrayPool { get; } = new MmByteArrayPool();

        /// <summary>
        /// Pool for pooled message wrappers.
        /// </summary>
        public MmObjectPool<MmPooledMessage> PooledMessagePool { get; }

        /// <summary>
        /// Pool for list of MmStateDelta.
        /// </summary>
        public MmObjectPool<List<MmStateDelta>> DeltaListPool { get; }

        /// <summary>
        /// Pool for list of MmQueuedMessage.
        /// </summary>
        public MmObjectPool<List<MmQueuedMessage>> QueuedMessageListPool { get; }

        private MmMessagePoolManager()
        {
            PooledMessagePool = new MmObjectPool<MmPooledMessage>(100, msg => msg.Reset());
            DeltaListPool = new MmObjectPool<List<MmStateDelta>>(50, list => list.Clear());
            QueuedMessageListPool = new MmObjectPool<List<MmQueuedMessage>>(50, list => list.Clear());
        }

        /// <summary>
        /// Get a byte array from the pool.
        /// </summary>
        public byte[] GetByteArray(int minSize)
        {
            return ByteArrayPool.Get(minSize);
        }

        /// <summary>
        /// Return a byte array to the pool.
        /// </summary>
        public void ReturnByteArray(byte[] array)
        {
            ByteArrayPool.Return(array);
        }

        /// <summary>
        /// Get a pooled message wrapper.
        /// </summary>
        public MmPooledMessage GetPooledMessage()
        {
            return PooledMessagePool.Get();
        }

        /// <summary>
        /// Return a pooled message wrapper.
        /// </summary>
        public void ReturnPooledMessage(MmPooledMessage msg)
        {
            PooledMessagePool.Return(msg);
        }

        /// <summary>
        /// Get a delta list from the pool.
        /// </summary>
        public List<MmStateDelta> GetDeltaList()
        {
            return DeltaListPool.Get();
        }

        /// <summary>
        /// Return a delta list to the pool.
        /// </summary>
        public void ReturnDeltaList(List<MmStateDelta> list)
        {
            DeltaListPool.Return(list);
        }

        /// <summary>
        /// Clear all pools.
        /// </summary>
        public void ClearAll()
        {
            ByteArrayPool.Clear();
            PooledMessagePool.Clear();
            DeltaListPool.Clear();
            QueuedMessageListPool.Clear();
        }

        /// <summary>
        /// Get comprehensive statistics.
        /// </summary>
        public string GetAllStats()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== MmMessagePoolManager Statistics ===");
            sb.AppendLine(ByteArrayPool.GetStats());
            sb.AppendLine(PooledMessagePool.GetStats());
            sb.AppendLine(DeltaListPool.GetStats());
            sb.AppendLine(QueuedMessageListPool.GetStats());
            return sb.ToString();
        }

        /// <summary>
        /// Prewarm pools for expected usage.
        /// Call during initialization.
        /// </summary>
        public void Prewarm(int byteArraysPerBucket = 10, int pooledMessages = 50, int deltaLists = 20)
        {
            PooledMessagePool.Prewarm(pooledMessages);
            DeltaListPool.Prewarm(deltaLists);
            QueuedMessageListPool.Prewarm(20);
        }
    }

    /// <summary>
    /// Extension methods for pooled operations.
    /// </summary>
    public static class MmPoolExtensions
    {
        /// <summary>
        /// Serialize message using pooled byte array.
        /// </summary>
        public static MmPooledMessage SerializePooled(this MmMessage message)
        {
            var pooled = MmMessagePoolManager.Instance.GetPooledMessage();
            pooled.Message = message;
            pooled.SerializedData = MmBinarySerializer.Serialize(message);
            return pooled;
        }

        /// <summary>
        /// Return pooled message and its data.
        /// </summary>
        public static void ReturnToPool(this MmPooledMessage pooled)
        {
            if (pooled.SerializedData != null)
            {
                MmMessagePoolManager.Instance.ReturnByteArray(pooled.SerializedData);
            }
            MmMessagePoolManager.Instance.ReturnPooledMessage(pooled);
        }
    }
}
