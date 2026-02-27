// Suppress MM analyzer warnings - test code intentionally uses patterns that trigger warnings
#pragma warning disable MM002, MM005, MM006, MM008, MM014, MM015

using NUnit.Framework;
using UnityEngine;
using MercuryMessaging.Network;

namespace MercuryMessaging.Tests
{
    /// <summary>
    /// Tests for MmBinarySerializer metadata serialization fixes:
    /// - Wave 1.3: 8-bit LevelFilter (was 4-bit) supporting Descendants, Ancestors, Cousins, Custom
    /// - Wave 1.4: IsDeserialized set to true after binary deserialization
    /// - Wave 4.9: Pooled serialization format with NullTypeId sentinel
    /// </summary>
    [TestFixture]
    public class MmBinarySerializerMetadataTests
    {
        #region Wave 1.3 - 8-bit LevelFilter Round-Trip Tests

        [Test]
        public void Serialize_Descendants_LevelFilter_RoundTrips()
        {
            // Arrange - Descendants (value 32) requires 8-bit LevelFilter
            var metadata = new MmMetadataBlock(
                MmLevelFilter.Descendants,
                MmActiveFilter.Active,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            );
            var original = new MmMessageBool(true, MmMethod.Initialize, metadata);
            original.NetId = 1;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(MmLevelFilter.Descendants, result.MetadataBlock.LevelFilter);
        }

        [Test]
        public void Serialize_Ancestors_LevelFilter_RoundTrips()
        {
            // Arrange - Ancestors (value 64) requires 8-bit LevelFilter
            var metadata = new MmMetadataBlock(
                MmLevelFilter.Ancestors,
                MmActiveFilter.Active,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            );
            var original = new MmMessageBool(true, MmMethod.Initialize, metadata);
            original.NetId = 1;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(MmLevelFilter.Ancestors, result.MetadataBlock.LevelFilter);
        }

        [Test]
        public void Serialize_Custom_LevelFilter_RoundTrips()
        {
            // Arrange - Custom (value 128) requires full 8-bit LevelFilter
            var metadata = new MmMetadataBlock(
                MmLevelFilter.Custom,
                MmActiveFilter.Active,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            );
            var original = new MmMessageBool(true, MmMethod.Initialize, metadata);
            original.NetId = 1;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(MmLevelFilter.Custom, result.MetadataBlock.LevelFilter);
        }

        [Test]
        public void Serialize_CombinedHighBitFilters_RoundTrips()
        {
            // Arrange - Self (1) | Descendants (32) = 33, requires 8-bit LevelFilter
            var combinedFilter = MmLevelFilter.Self | MmLevelFilter.Descendants;
            var metadata = new MmMetadataBlock(
                combinedFilter,
                MmActiveFilter.Active,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            );
            var original = new MmMessageBool(true, MmMethod.Initialize, metadata);
            original.NetId = 1;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(combinedFilter, result.MetadataBlock.LevelFilter);
        }

        [Test]
        public void Serialize_AllMetadataFields_Preserved()
        {
            // Arrange - all metadata fields set to non-default values
            var metadata = new MmMetadataBlock(
                MmLevelFilter.Ancestors,
                MmActiveFilter.All,
                MmSelectedFilter.Selected,
                MmNetworkFilter.All,
                MmTag.Tag3
            );
            var original = new MmMessageBool(true, MmMethod.Initialize, metadata);
            original.NetId = 1;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.AreEqual(MmLevelFilter.Ancestors, result.MetadataBlock.LevelFilter);
            Assert.AreEqual(MmActiveFilter.All, result.MetadataBlock.ActiveFilter);
            Assert.AreEqual(MmSelectedFilter.Selected, result.MetadataBlock.SelectedFilter);
            Assert.AreEqual(MmNetworkFilter.All, result.MetadataBlock.NetworkFilter);
            Assert.AreEqual(MmTag.Tag3, result.MetadataBlock.Tag);
        }

        #endregion

        #region Wave 1.4 - IsDeserialized Flag Tests

        [Test]
        public void Deserialize_SetsIsDeserialized_True()
        {
            // Arrange
            var original = new MmMessageBool(true, MmMethod.Initialize);
            original.NetId = 1;

            // Act
            byte[] data = MmBinarySerializer.Serialize(original);
            var result = MmBinarySerializer.Deserialize(data);

            // Assert
            Assert.IsTrue(result.IsDeserialized);
        }

        [Test]
        public void DeserializePooled_SetsIsDeserialized_True()
        {
            // Arrange
            var original = new MmMessageBool(true, MmMethod.Initialize);
            original.NetId = 1;

            // Act
            byte[] data = MmBinarySerializer.SerializePooled(original);
            var result = MmBinarySerializer.DeserializePooled(data);

            // Assert
            Assert.IsTrue(result.IsDeserialized);
        }

        #endregion

        #region Wave 4.9 - Pooled Serialization Round-Trip Tests

        [Test]
        public void SerializePooled_Descendants_LevelFilter_RoundTrips()
        {
            // Arrange - Descendants (value 32) via pooled path
            var metadata = new MmMetadataBlock(
                MmLevelFilter.Descendants,
                MmActiveFilter.Active,
                MmSelectedFilter.All,
                MmNetworkFilter.Local
            );
            var original = new MmMessageBool(true, MmMethod.Initialize, metadata);
            original.NetId = 1;

            // Act
            byte[] data = MmBinarySerializer.SerializePooled(original);
            var result = MmBinarySerializer.DeserializePooled(data);

            // Assert
            Assert.AreEqual(MmLevelFilter.Descendants, result.MetadataBlock.LevelFilter);
        }

        #endregion
    }
}
