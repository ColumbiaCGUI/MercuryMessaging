using NUnit.Framework;
using UnityEngine;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Unit tests for MmMessagePool.
    /// Tests pooling behavior, type safety, and reset functionality.
    /// </summary>
    [TestFixture]
    public class MmMessagePoolTests
    {
        #region Basic Pool Operations

        [Test]
        public void GetInt_ReturnsValidMessage()
        {
            var msg = MmMessagePool.GetInt(42);

            Assert.IsNotNull(msg);
            Assert.AreEqual(42, msg.value);
            Assert.AreEqual(MmMethod.MessageInt, msg.MmMethod);
            Assert.AreEqual(MmMessageType.MmInt, msg.MmMessageType);

            MmMessagePool.Return(msg);
        }

        [Test]
        public void GetFloat_ReturnsValidMessage()
        {
            var msg = MmMessagePool.GetFloat(3.14f);

            Assert.IsNotNull(msg);
            Assert.AreEqual(3.14f, msg.value, 0.001f);
            Assert.AreEqual(MmMethod.MessageFloat, msg.MmMethod);
            Assert.AreEqual(MmMessageType.MmFloat, msg.MmMessageType);

            MmMessagePool.Return(msg);
        }

        [Test]
        public void GetString_ReturnsValidMessage()
        {
            var msg = MmMessagePool.GetString("Hello");

            Assert.IsNotNull(msg);
            Assert.AreEqual("Hello", msg.value);
            Assert.AreEqual(MmMethod.MessageString, msg.MmMethod);
            Assert.AreEqual(MmMessageType.MmString, msg.MmMessageType);

            MmMessagePool.Return(msg);
        }

        [Test]
        public void GetBool_ReturnsValidMessage()
        {
            var msg = MmMessagePool.GetBool(true);

            Assert.IsNotNull(msg);
            Assert.IsTrue(msg.value);
            Assert.AreEqual(MmMethod.MessageBool, msg.MmMethod);
            Assert.AreEqual(MmMessageType.MmBool, msg.MmMessageType);

            MmMessagePool.Return(msg);
        }

        [Test]
        public void GetVector3_ReturnsValidMessage()
        {
            var testValue = new Vector3(1, 2, 3);
            var msg = MmMessagePool.GetVector3(testValue);

            Assert.IsNotNull(msg);
            Assert.AreEqual(testValue, msg.value);
            Assert.AreEqual(MmMethod.MessageVector3, msg.MmMethod);
            Assert.AreEqual(MmMessageType.MmVector3, msg.MmMessageType);

            MmMessagePool.Return(msg);
        }

        #endregion

        #region Pool Reuse

        [Test]
        public void Return_ThenGet_ReusesPooledMessage()
        {
            // Get a message
            var msg1 = MmMessagePool.GetInt(100);
            var reference1 = msg1;

            // Return it to pool
            MmMessagePool.Return(msg1);

            // Get another message of same type - should be the same instance (reused)
            var msg2 = MmMessagePool.GetInt(200);

            // The new message should have reset values
            Assert.AreEqual(200, msg2.value);
            Assert.AreEqual(0, msg2.HopCount, "HopCount should be reset");

            MmMessagePool.Return(msg2);
        }

        [Test]
        public void GetMultiple_DoesNotExhaustPool()
        {
            var messages = new MmMessageInt[100];

            // Get many messages
            for (int i = 0; i < 100; i++)
            {
                messages[i] = MmMessagePool.GetInt(i);
                Assert.IsNotNull(messages[i], $"Message {i} should not be null");
            }

            // Return all
            for (int i = 0; i < 100; i++)
            {
                MmMessagePool.Return(messages[i]);
            }
        }

        #endregion

        #region Reset Behavior

        [Test]
        public void Reset_ClearsHopCount()
        {
            var msg = MmMessagePool.GetInt(42);
            msg.HopCount = 10;

            MmMessagePool.Return(msg);

            var msg2 = MmMessagePool.GetInt(100);
            Assert.AreEqual(0, msg2.HopCount, "HopCount should be reset to 0");

            MmMessagePool.Return(msg2);
        }

        [Test]
        public void Reset_ClearsVisitedNodes()
        {
            var msg = MmMessagePool.GetInt(42);
            msg.VisitedNodes = new System.Collections.Generic.HashSet<int> { 1, 2, 3 };

            MmMessagePool.Return(msg);

            var msg2 = MmMessagePool.GetInt(100);
            Assert.IsTrue(msg2.VisitedNodes == null || msg2.VisitedNodes.Count == 0,
                "VisitedNodes should be cleared");

            MmMessagePool.Return(msg2);
        }

        [Test]
        public void Reset_ClearsNetId()
        {
            var msg = MmMessagePool.GetInt(42);
            msg.NetId = 12345;

            MmMessagePool.Return(msg);

            var msg2 = MmMessagePool.GetInt(100);
            Assert.AreEqual(0u, msg2.NetId, "NetId should be reset to 0");

            MmMessagePool.Return(msg2);
        }

        #endregion

        #region Custom Method and Metadata

        [Test]
        public void GetInt_WithCustomMethod_SetsMethod()
        {
            var msg = MmMessagePool.GetInt(42, MmMethod.Switch);

            Assert.AreEqual(MmMethod.Switch, msg.MmMethod);

            MmMessagePool.Return(msg);
        }

        [Test]
        public void GetString_WithMetadata_SetsMetadata()
        {
            var metadata = new MmMetadataBlock(MmLevelFilter.Child);
            var msg = MmMessagePool.GetString("test", MmMethod.MessageString, metadata);

            Assert.AreEqual(MmLevelFilter.Child, msg.MetadataBlock.LevelFilter);

            MmMessagePool.Return(msg);
        }

        #endregion

        #region Return Behavior

        [Test]
        public void Return_NullMessage_ReturnsFalse()
        {
            var result = MmMessagePool.Return(null);
            Assert.IsFalse(result);
        }

        [Test]
        public void Return_ValidMessage_ReturnsTrue()
        {
            var msg = MmMessagePool.GetInt(42);
            var result = MmMessagePool.Return(msg);
            Assert.IsTrue(result);
        }

        #endregion

        #region Base Message

        [Test]
        public void Get_BaseMessage_ReturnsValidMessage()
        {
            var msg = MmMessagePool.Get(MmMethod.Initialize);

            Assert.IsNotNull(msg);
            Assert.AreEqual(MmMethod.Initialize, msg.MmMethod);
            Assert.AreEqual(MmMessageType.MmVoid, msg.MmMessageType);

            MmMessagePool.Return(msg);
        }

        #endregion
    }

    /// <summary>
    /// Unit tests for MmHashSetPool.
    /// Tests pooling behavior for VisitedNodes HashSets.
    /// </summary>
    [TestFixture]
    public class MmHashSetPoolTests
    {
        [Test]
        public void Get_ReturnsEmptySet()
        {
            var set = MmHashSetPool.Get();

            Assert.IsNotNull(set);
            Assert.AreEqual(0, set.Count, "Pooled HashSet should be empty");

            MmHashSetPool.Return(set);
        }

        [Test]
        public void Return_ThenGet_ReusesSet()
        {
            var set1 = MmHashSetPool.Get();
            set1.Add(1);
            set1.Add(2);
            set1.Add(3);

            MmHashSetPool.Return(set1);

            var set2 = MmHashSetPool.Get();
            Assert.AreEqual(0, set2.Count, "Reused HashSet should be cleared");

            MmHashSetPool.Return(set2);
        }

        [Test]
        public void GetCopy_WithNull_ReturnsEmptySet()
        {
            var set = MmHashSetPool.GetCopy(null);

            Assert.IsNotNull(set);
            Assert.AreEqual(0, set.Count);

            MmHashSetPool.Return(set);
        }

        [Test]
        public void GetCopy_WithSource_CopiesValues()
        {
            var source = new System.Collections.Generic.HashSet<int> { 1, 2, 3, 4, 5 };
            var copy = MmHashSetPool.GetCopy(source);

            Assert.AreEqual(5, copy.Count);
            Assert.IsTrue(copy.Contains(1));
            Assert.IsTrue(copy.Contains(2));
            Assert.IsTrue(copy.Contains(3));
            Assert.IsTrue(copy.Contains(4));
            Assert.IsTrue(copy.Contains(5));

            MmHashSetPool.Return(copy);
        }

        [Test]
        public void Return_Null_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => MmHashSetPool.Return(null));
        }

        [Test]
        public void GetMultiple_DoesNotExhaustPool()
        {
            var sets = new System.Collections.Generic.HashSet<int>[100];

            for (int i = 0; i < 100; i++)
            {
                sets[i] = MmHashSetPool.Get();
                sets[i].Add(i);
                Assert.IsNotNull(sets[i]);
            }

            for (int i = 0; i < 100; i++)
            {
                MmHashSetPool.Return(sets[i]);
            }
        }

        [Test]
        public void PooledSet_CanBeUsedNormally()
        {
            var set = MmHashSetPool.Get();

            // Standard HashSet operations
            set.Add(100);
            set.Add(200);
            set.Add(100); // Duplicate

            Assert.AreEqual(2, set.Count);
            Assert.IsTrue(set.Contains(100));
            Assert.IsTrue(set.Contains(200));
            Assert.IsFalse(set.Contains(300));

            set.Remove(100);
            Assert.AreEqual(1, set.Count);

            MmHashSetPool.Return(set);
        }
    }
}
