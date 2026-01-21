// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmBinarySerializerEdgeCaseTests.cs - Edge case tests for MmBinarySerializer
// Tests boundary conditions, large payloads, and special values

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Network;

namespace MercuryMessaging.Tests.Network
{
    /// <summary>
    /// Edge case tests for MmBinarySerializer.
    /// Tests boundary conditions, special values, and stress scenarios.
    /// </summary>
    [TestFixture]
    public class MmBinarySerializerEdgeCaseTests
    {
        #region Numeric Boundary Tests

        [Test]
        public void Serialize_IntMessage_MaxValue_RoundTrips()
        {
            var original = new MmMessageInt(int.MaxValue, MmMethod.MessageInt);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageInt)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(int.MaxValue, result.value);
        }

        [Test]
        public void Serialize_IntMessage_MinValue_RoundTrips()
        {
            var original = new MmMessageInt(int.MinValue, MmMethod.MessageInt);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageInt)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(int.MinValue, result.value);
        }

        [Test]
        public void Serialize_IntMessage_Zero_RoundTrips()
        {
            var original = new MmMessageInt(0, MmMethod.MessageInt);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageInt)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(0, result.value);
        }

        [Test]
        public void Serialize_FloatMessage_MaxValue_RoundTrips()
        {
            var original = new MmMessageFloat(float.MaxValue, MmMethod.MessageFloat);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageFloat)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(float.MaxValue, result.value);
        }

        [Test]
        public void Serialize_FloatMessage_MinValue_RoundTrips()
        {
            var original = new MmMessageFloat(float.MinValue, MmMethod.MessageFloat);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageFloat)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(float.MinValue, result.value);
        }

        [Test]
        public void Serialize_FloatMessage_Epsilon_RoundTrips()
        {
            var original = new MmMessageFloat(float.Epsilon, MmMethod.MessageFloat);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageFloat)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(float.Epsilon, result.value);
        }

        [Test]
        public void Serialize_FloatMessage_NaN_RoundTrips()
        {
            var original = new MmMessageFloat(float.NaN, MmMethod.MessageFloat);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageFloat)MmBinarySerializer.Deserialize(data);

            Assert.IsTrue(float.IsNaN(result.value));
        }

        [Test]
        public void Serialize_FloatMessage_PositiveInfinity_RoundTrips()
        {
            var original = new MmMessageFloat(float.PositiveInfinity, MmMethod.MessageFloat);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageFloat)MmBinarySerializer.Deserialize(data);

            Assert.IsTrue(float.IsPositiveInfinity(result.value));
        }

        [Test]
        public void Serialize_FloatMessage_NegativeInfinity_RoundTrips()
        {
            var original = new MmMessageFloat(float.NegativeInfinity, MmMethod.MessageFloat);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageFloat)MmBinarySerializer.Deserialize(data);

            Assert.IsTrue(float.IsNegativeInfinity(result.value));
        }

        #endregion

        #region String Edge Cases

        [Test]
        public void Serialize_StringMessage_LongString_RoundTrips()
        {
            // 10KB string
            string longString = new string('X', 10000);
            var original = new MmMessageString(longString, MmMethod.MessageString);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageString)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(longString, result.value);
            Assert.AreEqual(10000, result.value.Length);
        }

        [Test]
        public void Serialize_StringMessage_NullCharacter_RoundTrips()
        {
            // String with embedded null character
            string strWithNull = "Hello\0World";
            var original = new MmMessageString(strWithNull, MmMethod.MessageString);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageString)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(strWithNull, result.value);
            Assert.AreEqual(11, result.value.Length);
        }

        [Test]
        public void Serialize_StringMessage_Emoji_RoundTrips()
        {
            // Emoji and special Unicode
            string emoji = "Hello 🌍🚀 World!";
            var original = new MmMessageString(emoji, MmMethod.MessageString);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageString)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(emoji, result.value);
        }

        [Test]
        public void Serialize_StringMessage_Whitespace_RoundTrips()
        {
            string whitespace = "   \t\n\r   ";
            var original = new MmMessageString(whitespace, MmMethod.MessageString);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageString)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(whitespace, result.value);
        }

        #endregion

        #region Vector Edge Cases

        [Test]
        public void Serialize_Vector3Message_Zero_RoundTrips()
        {
            var original = new MmMessageVector3(Vector3.zero, MmMethod.MessageVector3);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageVector3)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(Vector3.zero, result.value);
        }

        [Test]
        public void Serialize_Vector3Message_One_RoundTrips()
        {
            var original = new MmMessageVector3(Vector3.one, MmMethod.MessageVector3);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageVector3)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(Vector3.one, result.value);
        }

        [Test]
        public void Serialize_Vector3Message_NegativeValues_RoundTrips()
        {
            var original = new MmMessageVector3(new Vector3(-100f, -200f, -300f), MmMethod.MessageVector3);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageVector3)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(-100f, result.value.x, 0.001f);
            Assert.AreEqual(-200f, result.value.y, 0.001f);
            Assert.AreEqual(-300f, result.value.z, 0.001f);
        }

        [Test]
        public void Serialize_QuaternionMessage_Identity_RoundTrips()
        {
            var original = new MmMessageQuaternion(Quaternion.identity, MmMethod.MessageQuaternion);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageQuaternion)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(Quaternion.identity.x, result.value.x, 0.0001f);
            Assert.AreEqual(Quaternion.identity.y, result.value.y, 0.0001f);
            Assert.AreEqual(Quaternion.identity.z, result.value.z, 0.0001f);
            Assert.AreEqual(Quaternion.identity.w, result.value.w, 0.0001f);
        }

        [Test]
        public void Serialize_QuaternionMessage_ExtremeRotation_RoundTrips()
        {
            // 360 degree rotation around each axis
            var rotation = Quaternion.Euler(360f, 360f, 360f);
            var original = new MmMessageQuaternion(rotation, MmMethod.MessageQuaternion);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageQuaternion)MmBinarySerializer.Deserialize(data);

            // Quaternion values should match (360 degrees wraps to identity)
            float dot = Quaternion.Dot(rotation, result.value);
            Assert.IsTrue(Mathf.Abs(dot) > 0.999f);
        }

        #endregion

        #region ByteArray Edge Cases

        [Test]
        public void Serialize_ByteArrayMessage_AllZeros_RoundTrips()
        {
            byte[] payload = new byte[100];
            var original = new MmMessageByteArray(payload, MmMethod.MessageByteArray);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageByteArray)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(100, result.byteArr.Length);
            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(0, result.byteArr[i]);
            }
        }

        [Test]
        public void Serialize_ByteArrayMessage_AllOnes_RoundTrips()
        {
            byte[] payload = new byte[100];
            for (int i = 0; i < 100; i++) payload[i] = 0xFF;

            var original = new MmMessageByteArray(payload, MmMethod.MessageByteArray);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageByteArray)MmBinarySerializer.Deserialize(data);

            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(0xFF, result.byteArr[i]);
            }
        }

        [Test]
        public void Serialize_ByteArrayMessage_LargePayload_RoundTrips()
        {
            // 64KB payload
            byte[] payload = new byte[65536];
            for (int i = 0; i < payload.Length; i++)
            {
                payload[i] = (byte)(i % 256);
            }

            var original = new MmMessageByteArray(payload, MmMethod.MessageByteArray);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageByteArray)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(65536, result.byteArr.Length);
            for (int i = 0; i < 100; i++) // Spot check
            {
                Assert.AreEqual((byte)(i % 256), result.byteArr[i]);
            }
        }

        #endregion

        #region Transform Edge Cases

        [Test]
        public void Serialize_TransformMessage_Identity_RoundTrips()
        {
            var transform = new MmTransform(
                Vector3.zero,
                Vector3.one,
                Quaternion.identity
            );
            var original = new MmMessageTransform(transform, MmMethod.MessageTransform);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageTransform)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(Vector3.zero, result.MmTransform.Translation);
            Assert.AreEqual(Vector3.one, result.MmTransform.Scale);
        }

        [Test]
        public void Serialize_TransformList_Empty_RoundTrips()
        {
            var transforms = new List<MmTransform>();
            var original = new MmMessageTransformList(transforms, MmMethod.MessageTransformList);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageTransformList)MmBinarySerializer.Deserialize(data);

            Assert.IsNotNull(result.transforms);
            Assert.AreEqual(0, result.transforms.Count);
        }

        [Test]
        public void Serialize_TransformList_ManyItems_RoundTrips()
        {
            var transforms = new List<MmTransform>();
            for (int i = 0; i < 50; i++)
            {
                transforms.Add(new MmTransform(
                    new Vector3(i, i * 2, i * 3),
                    Vector3.one,
                    Quaternion.identity
                ));
            }
            var original = new MmMessageTransformList(transforms, MmMethod.MessageTransformList);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageTransformList)MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(50, result.transforms.Count);
            Assert.AreEqual(10f, result.transforms[10].Translation.x, 0.001f);
        }

        #endregion

        #region NetId Edge Cases

        [Test]
        public void Serialize_Message_MaxNetId_RoundTrips()
        {
            // Use MmMessageBool since base MmMessage constructor sets MmMessageType to 0
            // which is not a valid serializable type
            var original = new MmMessageBool(true, MmMethod.Initialize);
            original.NetId = uint.MaxValue;

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(uint.MaxValue, result.NetId);
        }

        [Test]
        public void Serialize_Message_ZeroNetId_RoundTrips()
        {
            // Use MmMessageBool since base MmMessage constructor sets MmMessageType to 0
            // which is not a valid serializable type
            var original = new MmMessageBool(true, MmMethod.Initialize);
            original.NetId = 0;

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(0u, result.NetId);
        }

        #endregion

        #region Metadata Edge Cases

        [Test]
        public void Serialize_Message_AllTagsCombined_RoundTrips()
        {
            var allTags = MmTag.Tag0 | MmTag.Tag1 | MmTag.Tag2 | MmTag.Tag3 |
                          MmTag.Tag4 | MmTag.Tag5 | MmTag.Tag6 | MmTag.Tag7;

            var metadata = new MmMetadataBlock(allTags);
            // Use MmMessageBool since MmMessageType.MmVoid (0) has limited support
            var original = new MmMessageBool(true, MmMethod.Initialize, metadata);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(allTags, result.MetadataBlock.Tag);
        }

        [Test]
        public void Serialize_Message_AllFilters_RoundTrips()
        {
            var metadata = new MmMetadataBlock(
                MmTag.Tag7,
                MmLevelFilter.Parent | MmLevelFilter.Child,
                MmActiveFilter.All,
                MmSelectedFilter.Selected,
                MmNetworkFilter.All
            );
            // Use MmMessageBool since MmMessageType.MmVoid (0) has limited support
            var original = new MmMessageBool(true, MmMethod.Complete, metadata);

            byte[] data = MmBinarySerializer.Serialize(original);
            var result = MmBinarySerializer.Deserialize(data);

            Assert.AreEqual(MmTag.Tag7, result.MetadataBlock.Tag);
            Assert.AreEqual(MmLevelFilter.Parent | MmLevelFilter.Child, result.MetadataBlock.LevelFilter);
            Assert.AreEqual(MmActiveFilter.All, result.MetadataBlock.ActiveFilter);
            Assert.AreEqual(MmSelectedFilter.Selected, result.MetadataBlock.SelectedFilter);
            Assert.AreEqual(MmNetworkFilter.All, result.MetadataBlock.NetworkFilter);
        }

        #endregion

        #region Rapid Serialization Stress Tests

        [Test]
        public void Serialize_1000Messages_NoExceptions()
        {
            for (int i = 0; i < 1000; i++)
            {
                var msg = new MmMessageInt(i, MmMethod.MessageInt);
                byte[] data = MmBinarySerializer.Serialize(msg);
                var result = MmBinarySerializer.Deserialize(data);
                Assert.AreEqual(i, ((MmMessageInt)result).value);
            }
        }

        [Test]
        public void Serialize_MixedMessageTypes_AllSucceed()
        {
            // Note: We use MmMessageBool for the "void" case since base MmMessage
            // creates MmMessageType = 0 which is not handled by the serializer
            var messages = new MmMessage[]
            {
                new MmMessageBool(false, MmMethod.Initialize), // Replaces bare MmMessage
                new MmMessageInt(42, MmMethod.MessageInt),
                new MmMessageBool(true, MmMethod.SetActive),
                new MmMessageFloat(3.14f, MmMethod.MessageFloat),
                new MmMessageString("Hello", MmMethod.MessageString),
                new MmMessageVector3(Vector3.up, MmMethod.MessageVector3),
                new MmMessageVector4(Vector4.one, MmMethod.MessageVector4),
                new MmMessageQuaternion(Quaternion.identity, MmMethod.MessageQuaternion)
            };

            foreach (var msg in messages)
            {
                byte[] data = MmBinarySerializer.Serialize(msg);
                var result = MmBinarySerializer.Deserialize(data);
                Assert.AreEqual(msg.MmMessageType, result.MmMessageType);
            }
        }

        #endregion
    }
}
