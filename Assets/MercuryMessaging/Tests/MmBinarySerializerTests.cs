// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// Tests for MmBinarySerializer - validates serialization/deserialization
// of all MercuryMessaging message types.

using NUnit.Framework;
using UnityEngine;
using MercuryMessaging;
using MercuryMessaging.Network;

namespace MercuryMessaging.Tests
{
    [TestFixture]
    public class MmBinarySerializerTests
    {
        #region Basic Message Tests

        [Test]
        public void Serialize_VoidMessage_RoundTrips()
        {
            // Arrange
            var original = new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid,
                new MmMetadataBlock(MmLevelFilter.Child));
            original.NetId = 42;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            MmMessage result = MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(MmMethod.Initialize, result.MmMethod);
            Assert.AreEqual(MmMessageType.MmVoid, result.MmMessageType);
            Assert.AreEqual(42u, result.NetId);
            Assert.AreEqual(MmLevelFilter.Child, result.MetadataBlock.LevelFilter);
        }

        [Test]
        public void Serialize_IntMessage_RoundTrips()
        {
            // Arrange
            var original = new MmMessageInt(12345, MmMethod.MessageInt,
                new MmMetadataBlock(MmLevelFilter.SelfAndChildren));
            original.NetId = 100;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageInt)MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(12345, result.value);
            Assert.AreEqual(MmMethod.MessageInt, result.MmMethod);
            Assert.AreEqual(100u, result.NetId);
        }

        [Test]
        public void Serialize_BoolMessage_RoundTrips()
        {
            // Arrange: True
            var originalTrue = new MmMessageBool(true, MmMethod.SetActive);
            originalTrue.NetId = 1;

            // Act
            byte[] dataTrue = MmBinarySerializer.Serialize(originalTrue);
            var resultTrue = (MmMessageBool)MmBinarySerializer.Deserialize(dataTrue);

            // Assert
            Assert.IsTrue(resultTrue.value);
            Assert.AreEqual(MmMethod.SetActive, resultTrue.MmMethod);

            // Arrange: False
            var originalFalse = new MmMessageBool(false, MmMethod.SetActive);
            originalFalse.NetId = 2;

            // Act
            byte[] dataFalse = MmBinarySerializer.Serialize(originalFalse);
            var resultFalse = (MmMessageBool)MmBinarySerializer.Deserialize(dataFalse);

            // Assert
            Assert.IsFalse(resultFalse.value);
        }

        [Test]
        public void Serialize_FloatMessage_RoundTrips()
        {
            // Arrange
            var original = new MmMessageFloat(3.14159f, MmMethod.MessageFloat);
            original.NetId = 50;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageFloat)MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(3.14159f, result.value, 0.00001f);
        }

        [Test]
        public void Serialize_StringMessage_RoundTrips()
        {
            // Arrange
            var original = new MmMessageString("Hello, MercuryMessaging!", MmMethod.MessageString);
            original.NetId = 75;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageString)MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual("Hello, MercuryMessaging!", result.value);
        }

        [Test]
        public void Serialize_StringMessage_EmptyString_RoundTrips()
        {
            // Arrange
            var original = new MmMessageString("", MmMethod.MessageString);

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageString)MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual("", result.value);
        }

        [Test]
        public void Serialize_StringMessage_Unicode_RoundTrips()
        {
            // Arrange
            var original = new MmMessageString("Unicode: \u4e2d\u6587 \u00e9\u00e8\u00ea", MmMethod.MessageString);

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageString)MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual("Unicode: \u4e2d\u6587 \u00e9\u00e8\u00ea", result.value);
        }

        #endregion

        #region Vector/Transform Tests

        [Test]
        public void Serialize_Vector3Message_RoundTrips()
        {
            // Arrange
            var original = new MmMessageVector3(new Vector3(1.5f, 2.5f, 3.5f), MmMethod.MessageVector3);
            original.NetId = 10;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageVector3)MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(1.5f, result.value.x, 0.0001f);
            Assert.AreEqual(2.5f, result.value.y, 0.0001f);
            Assert.AreEqual(3.5f, result.value.z, 0.0001f);
        }

        [Test]
        public void Serialize_Vector4Message_RoundTrips()
        {
            // Arrange
            var original = new MmMessageVector4(new Vector4(1, 2, 3, 4), MmMethod.MessageVector4);
            original.NetId = 11;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageVector4)MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(1f, result.value.x);
            Assert.AreEqual(2f, result.value.y);
            Assert.AreEqual(3f, result.value.z);
            Assert.AreEqual(4f, result.value.w);
        }

        [Test]
        public void Serialize_QuaternionMessage_RoundTrips()
        {
            // Arrange
            var rotation = Quaternion.Euler(45, 90, 180);
            var original = new MmMessageQuaternion(rotation, MmMethod.MessageQuaternion);
            original.NetId = 12;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageQuaternion)MmBinarySerializer.Deserialize(data);

            // Assert (quaternion components)
            Assert.AreEqual(rotation.x, result.value.x, 0.0001f);
            Assert.AreEqual(rotation.y, result.value.y, 0.0001f);
            Assert.AreEqual(rotation.z, result.value.z, 0.0001f);
            Assert.AreEqual(rotation.w, result.value.w, 0.0001f);
        }

        [Test]
        public void Serialize_TransformMessage_RoundTrips()
        {
            // Arrange
            var transform = new MmTransform(
                new Vector3(10, 20, 30),  // Translation
                new Vector3(1, 1, 1),     // Scale
                Quaternion.identity       // Rotation
            );
            var original = new MmMessageTransform(transform, MmMethod.MessageTransform);
            original.NetId = 15;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageTransform)MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(10f, result.MmTransform.Translation.x, 0.0001f);
            Assert.AreEqual(20f, result.MmTransform.Translation.y, 0.0001f);
            Assert.AreEqual(30f, result.MmTransform.Translation.z, 0.0001f);
        }

        #endregion

        #region Byte Array Tests

        [Test]
        public void Serialize_ByteArrayMessage_RoundTrips()
        {
            // Arrange
            byte[] payload = { 0x01, 0x02, 0x03, 0x04, 0xFF, 0x00, 0xAB };
            var original = new MmMessageByteArray(payload, MmMethod.MessageByteArray);
            original.NetId = 20;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageByteArray)MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(payload.Length, result.value.Length);
            for (int i = 0; i < payload.Length; i++)
            {
                Assert.AreEqual(payload[i], result.value[i]);
            }
        }

        [Test]
        public void Serialize_ByteArrayMessage_EmptyArray_RoundTrips()
        {
            // Arrange
            var original = new MmMessageByteArray(new byte[0], MmMethod.MessageByteArray);

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageByteArray)MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.IsNotNull(result.value);
            Assert.AreEqual(0, result.value.Length);
        }

        #endregion

        #region Metadata Tests

        [Test]
        public void Serialize_PreservesAllMetadataFilters()
        {
            // Arrange
            var metadata = new MmMetadataBlock(
                MmLevelFilter.Parent,
                MmActiveFilter.All,
                MmSelectedFilter.Selected,
                MmNetworkFilter.Network,
                MmTag.Tag3);

            var original = new MmMessage(MmMethod.Refresh, MmMessageType.MmVoid, metadata);
            original.NetId = 999;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(MmLevelFilter.Parent, result.MetadataBlock.LevelFilter);
            Assert.AreEqual(MmActiveFilter.All, result.MetadataBlock.ActiveFilter);
            Assert.AreEqual(MmSelectedFilter.Selected, result.MetadataBlock.SelectedFilter);
            Assert.AreEqual(MmNetworkFilter.Network, result.MetadataBlock.NetworkFilter);
            Assert.AreEqual(MmTag.Tag3, result.MetadataBlock.Tag);
        }

        [Test]
        public void Serialize_PreservesMultipleTags()
        {
            // Arrange
            var tag = MmTag.Tag0 | MmTag.Tag2 | MmTag.Tag5;
            var metadata = new MmMetadataBlock(tag: tag);
            var original = new MmMessage(MmMethod.Initialize, MmMessageType.MmVoid, metadata);

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(tag, result.MetadataBlock.Tag);
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public void Deserialize_NullData_ThrowsArgumentException()
        {
            Assert.Throws<System.ArgumentException>(() =>
            {
                MmBinarySerializer.Deserialize(null);
            });
        }

        [Test]
        public void Deserialize_TooShortData_ThrowsArgumentException()
        {
            Assert.Throws<System.ArgumentException>(() =>
            {
                MmBinarySerializer.Deserialize(new byte[5]);
            });
        }

        [Test]
        public void Deserialize_InvalidMagic_ThrowsInvalidDataException()
        {
            // Arrange: Valid length but wrong magic number
            byte[] badData = new byte[MmBinarySerializer.HeaderSize + 10];

            Assert.Throws<System.IO.InvalidDataException>(() =>
            {
                MmBinarySerializer.Deserialize(badData);
            });
        }

        #endregion

        #region GameObject Message Tests

        [Test]
        public void Serialize_GameObjectMessage_PreservesNetworkId()
        {
            // Arrange
            var original = new MmMessageGameObject(null, MmMethod.MessageGameObject);
            original.NetId = 500;
            original.GameObjectNetId = 12345;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = (MmMessageGameObject)MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(500u, result.NetId);
            Assert.AreEqual(12345u, result.GameObjectNetId);
            // Note: Value (GameObject) is null because we didn't use a resolver
        }

        #endregion
    }
}
