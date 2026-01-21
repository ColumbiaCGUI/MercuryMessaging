// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmMessagePoolTests.cs - Unit tests for message pooling system

using System.Collections.Generic;
using NUnit.Framework;
using MercuryMessaging.Network;

namespace MercuryMessaging.Tests.Network
{
    /// <summary>
    /// Unit tests for MmMessagePool and related classes.
    /// </summary>
    [TestFixture]
    public class MmMessagePoolTests
    {
        #region MmObjectPool Tests

        [Test]
        public void ObjectPool_Get_ReturnsNewObject_WhenEmpty()
        {
            var pool = new MmObjectPool<TestPoolable>();
            var obj = pool.Get();

            Assert.IsNotNull(obj);
            Assert.AreEqual(1, pool.TotalCreated);
        }

        [Test]
        public void ObjectPool_Get_ReturnsPooledObject_AfterReturn()
        {
            var pool = new MmObjectPool<TestPoolable>();
            var obj1 = pool.Get();
            pool.Return(obj1);
            var obj2 = pool.Get();

            Assert.AreSame(obj1, obj2);
            Assert.AreEqual(1, pool.TotalCreated);
            Assert.AreEqual(2, pool.TotalGets);
        }

        [Test]
        public void ObjectPool_Return_CallsResetAction()
        {
            bool resetCalled = false;
            var pool = new MmObjectPool<TestPoolable>(100, obj => resetCalled = true);

            var item = pool.Get();
            pool.Return(item);

            Assert.IsTrue(resetCalled);
        }

        [Test]
        public void ObjectPool_Prewarm_CreatesObjects()
        {
            var pool = new MmObjectPool<TestPoolable>();
            pool.Prewarm(10);

            Assert.AreEqual(10, pool.Count);
            Assert.AreEqual(10, pool.TotalCreated);
        }

        [Test]
        public void ObjectPool_Return_DiscardsExcess()
        {
            var pool = new MmObjectPool<TestPoolable>(maxSize: 5);
            pool.Prewarm(5);

            // Return extra objects
            for (int i = 0; i < 10; i++)
            {
                pool.Return(new TestPoolable());
            }

            Assert.AreEqual(5, pool.Count); // Still at max
        }

        [Test]
        public void ObjectPool_HitRate_CalculatesCorrectly()
        {
            var pool = new MmObjectPool<TestPoolable>();

            // First get creates new object
            var obj1 = pool.Get();
            pool.Return(obj1);

            // Second get reuses
            var obj2 = pool.Get();
            pool.Return(obj2);

            // Third get reuses
            var obj3 = pool.Get();

            // 3 gets, 1 created = 66% hit rate
            Assert.AreEqual(1, pool.TotalCreated);
            Assert.AreEqual(3, pool.TotalGets);
            Assert.AreEqual(1.0f - (1.0f / 3.0f), pool.HitRate, 0.01f);
        }

        [Test]
        public void ObjectPool_Clear_EmptiesPool()
        {
            var pool = new MmObjectPool<TestPoolable>();
            pool.Prewarm(10);
            pool.Clear();

            Assert.AreEqual(0, pool.Count);
        }

        #endregion

        #region MmByteArrayPool Tests

        [Test]
        public void ByteArrayPool_Get_ReturnsBucketSize()
        {
            var pool = new MmByteArrayPool();

            var arr1 = pool.Get(50);  // Should return 64-byte array
            var arr2 = pool.Get(100); // Should return 256-byte array
            var arr3 = pool.Get(500); // Should return 1024-byte array

            Assert.AreEqual(64, arr1.Length);
            Assert.AreEqual(256, arr2.Length);
            Assert.AreEqual(1024, arr3.Length);
        }

        [Test]
        public void ByteArrayPool_Get_ReturnsLargerForOversized()
        {
            var pool = new MmByteArrayPool();

            // Request larger than max bucket
            var arr = pool.Get(100000);

            Assert.AreEqual(100000, arr.Length);
        }

        [Test]
        public void ByteArrayPool_Return_PoolsCorrectBucket()
        {
            var pool = new MmByteArrayPool();

            var arr = pool.Get(50); // Returns 64-byte array
            pool.Return(arr);
            var arr2 = pool.Get(50);

            Assert.AreSame(arr, arr2);
        }

        [Test]
        public void ByteArrayPool_Return_ClearsData()
        {
            var pool = new MmByteArrayPool();

            var arr = pool.Get(64);
            arr[0] = 0xFF;
            arr[10] = 0xFF;
            pool.Return(arr);

            var arr2 = pool.Get(64);
            Assert.AreEqual(0, arr2[0]);
            Assert.AreEqual(0, arr2[10]);
        }

        [Test]
        public void ByteArrayPool_GetScoped_ReturnsOnDispose()
        {
            var pool = new MmByteArrayPool();
            byte[] capturedArray;

            using (var scoped = pool.GetScoped(100))
            {
                capturedArray = scoped.Array;
                Assert.AreEqual(256, capturedArray.Length);
            }

            // Should be returned to pool
            var arr2 = pool.Get(100);
            Assert.AreSame(capturedArray, arr2);
        }

        #endregion

        #region MmMessagePoolManager Tests

        [Test]
        public void PoolManager_Instance_ReturnsSingleton()
        {
            var instance1 = MmMessagePoolManager.Instance;
            var instance2 = MmMessagePoolManager.Instance;

            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void PoolManager_GetByteArray_ReturnsArray()
        {
            var arr = MmMessagePoolManager.Instance.GetByteArray(100);

            Assert.IsNotNull(arr);
            Assert.GreaterOrEqual(arr.Length, 100);
        }

        [Test]
        public void PoolManager_GetPooledMessage_ReturnsMessage()
        {
            var msg = MmMessagePoolManager.Instance.GetPooledMessage();

            Assert.IsNotNull(msg);
            Assert.IsTrue(msg.IsFromPool);
        }

        [Test]
        public void PoolManager_GetDeltaList_ReturnsList()
        {
            var list = MmMessagePoolManager.Instance.GetDeltaList();

            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void PoolManager_ReturnDeltaList_ClearsList()
        {
            var list = MmMessagePoolManager.Instance.GetDeltaList();
            list.Add(new MmStateDelta { DeltaId = 1 });
            list.Add(new MmStateDelta { DeltaId = 2 });

            MmMessagePoolManager.Instance.ReturnDeltaList(list);
            var list2 = MmMessagePoolManager.Instance.GetDeltaList();

            Assert.AreSame(list, list2);
            Assert.AreEqual(0, list2.Count);
        }

        [Test]
        public void PoolManager_Prewarm_PopulatesPools()
        {
            // Clear first
            MmMessagePoolManager.Instance.ClearAll();

            MmMessagePoolManager.Instance.Prewarm(5, 20, 10);

            Assert.AreEqual(20, MmMessagePoolManager.Instance.PooledMessagePool.Count);
            Assert.AreEqual(10, MmMessagePoolManager.Instance.DeltaListPool.Count);
        }

        [Test]
        public void PoolManager_GetAllStats_ReturnsStats()
        {
            var stats = MmMessagePoolManager.Instance.GetAllStats();

            Assert.IsNotNull(stats);
            Assert.IsTrue(stats.Contains("ByteArrayPool"));
            Assert.IsTrue(stats.Contains("MmPooledMessage"));
        }

        #endregion

        #region Helper Classes

        private class TestPoolable
        {
            public int Value { get; set; }
        }

        #endregion
    }
}
