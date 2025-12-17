// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmCompressionTests.cs - Unit tests for compression utilities

using NUnit.Framework;
using MercuryMessaging.Network;

namespace MercuryMessaging.Tests.Network
{
    /// <summary>
    /// Unit tests for MmCompression and related classes.
    /// </summary>
    [TestFixture]
    public class MmCompressionTests
    {
        #region Compression Tests

        [Test]
        public void Compression_Compress_ReturnsValidData()
        {
            byte[] data = CreateTestData(1000);
            var config = MmCompressionConfig.Default;

            byte[] compressed = MmCompression.Compress(data, config);

            Assert.IsNotNull(compressed);
            Assert.Greater(compressed.Length, 0);
        }

        [Test]
        public void Compression_Decompress_RestoresOriginal()
        {
            byte[] original = CreateTestData(1000);
            var config = MmCompressionConfig.Default;

            byte[] compressed = MmCompression.Compress(original, config);
            byte[] decompressed = MmCompression.Decompress(compressed);

            Assert.AreEqual(original.Length, decompressed.Length);
            for (int i = 0; i < original.Length; i++)
            {
                Assert.AreEqual(original[i], decompressed[i], $"Mismatch at index {i}");
            }
        }

        [Test]
        public void Compression_SmallPayload_SkipsCompression()
        {
            byte[] data = new byte[50]; // Below MinSizeToCompress
            var config = MmCompressionConfig.Default; // MinSizeToCompress = 128

            byte[] result = MmCompression.Compress(data, config);

            Assert.IsFalse(MmCompression.IsCompressed(result));
            Assert.AreEqual(data.Length + 1, result.Length); // +1 for marker
        }

        [Test]
        public void Compression_Disabled_SkipsCompression()
        {
            byte[] data = CreateTestData(1000);
            var config = MmCompressionConfig.Disabled;

            byte[] result = MmCompression.Compress(data, config);

            Assert.IsFalse(MmCompression.IsCompressed(result));
        }

        [Test]
        public void Compression_GZip_CompressesAndDecompresses()
        {
            byte[] original = CreateCompressibleData(5000);
            var config = new MmCompressionConfig
            {
                Type = MmCompressionType.GZip,
                Level = MmCompressionLevel.Optimal
            };

            byte[] compressed = MmCompression.Compress(original, config);
            byte[] decompressed = MmCompression.Decompress(compressed);

            Assert.AreEqual(original.Length, decompressed.Length);
            Assert.Less(compressed.Length, original.Length); // Should be smaller
        }

        [Test]
        public void Compression_Deflate_CompressesAndDecompresses()
        {
            byte[] original = CreateCompressibleData(5000);
            var config = new MmCompressionConfig
            {
                Type = MmCompressionType.Deflate,
                Level = MmCompressionLevel.Optimal
            };

            byte[] compressed = MmCompression.Compress(original, config);
            byte[] decompressed = MmCompression.Decompress(compressed);

            Assert.AreEqual(original.Length, decompressed.Length);
            Assert.Less(compressed.Length, original.Length);
        }

        [Test]
        public void Compression_IsCompressed_DetectsCompressedData()
        {
            byte[] original = CreateCompressibleData(2000);
            var config = MmCompressionConfig.Default;

            byte[] compressed = MmCompression.Compress(original, config);

            Assert.IsTrue(MmCompression.IsCompressed(compressed));
        }

        [Test]
        public void Compression_IsCompressed_DetectsUncompressedData()
        {
            byte[] data = new byte[50];
            var config = MmCompressionConfig.Default;

            byte[] result = MmCompression.Compress(data, config);

            Assert.IsFalse(MmCompression.IsCompressed(result));
        }

        [Test]
        public void Compression_GetCompressionRatio_CalculatesCorrectly()
        {
            byte[] original = new byte[1000];
            byte[] compressed = new byte[500];

            float ratio = MmCompression.GetCompressionRatio(original, compressed);

            Assert.AreEqual(0.5f, ratio, 0.001f);
        }

        [Test]
        public void Compression_Fastest_StillCompresses()
        {
            byte[] original = CreateCompressibleData(5000);
            var config = MmCompressionConfig.Fast;

            byte[] compressed = MmCompression.Compress(original, config);
            byte[] decompressed = MmCompression.Decompress(compressed);

            Assert.AreEqual(original.Length, decompressed.Length);
        }

        [Test]
        public void Compression_IncompressibleData_SkipsCompression()
        {
            // Random data is hard to compress
            byte[] random = CreateRandomData(500);
            var config = new MmCompressionConfig
            {
                Type = MmCompressionType.Deflate,
                MinCompressionRatio = 0.8f // Must achieve 20% reduction
            };

            byte[] result = MmCompression.Compress(random, config);

            // Should fall back to uncompressed since random data doesn't compress well
            // Result should still decompress correctly
            byte[] decompressed = MmCompression.Decompress(result);
            Assert.AreEqual(random.Length, decompressed.Length);
        }

        #endregion

        #region Bandwidth Stats Tests

        [Test]
        public void BandwidthStats_TrackSent_UpdatesStats()
        {
            var stats = new MmBandwidthStats();

            stats.TrackSent(1000, 500);
            stats.TrackSent(2000, 1000);

            Assert.AreEqual(3000, stats.TotalBytesSentUncompressed);
            Assert.AreEqual(1500, stats.TotalBytesSentCompressed);
            Assert.AreEqual(2, stats.PacketsSent);
        }

        [Test]
        public void BandwidthStats_TrackReceived_UpdatesStats()
        {
            var stats = new MmBandwidthStats();

            stats.TrackReceived(500);
            stats.TrackReceived(750);

            Assert.AreEqual(1250, stats.TotalBytesReceived);
            Assert.AreEqual(2, stats.PacketsReceived);
        }

        [Test]
        public void BandwidthStats_AverageCompressionRatio_CalculatesCorrectly()
        {
            var stats = new MmBandwidthStats();

            stats.TrackSent(1000, 500); // 50% compression

            Assert.AreEqual(0.5f, stats.AverageCompressionRatio, 0.001f);
        }

        [Test]
        public void BandwidthStats_BytesSaved_CalculatesCorrectly()
        {
            var stats = new MmBandwidthStats();

            stats.TrackSent(1000, 400);
            stats.TrackSent(2000, 800);

            Assert.AreEqual(1800, stats.BytesSaved);
        }

        [Test]
        public void BandwidthStats_Reset_ClearsAllStats()
        {
            var stats = new MmBandwidthStats();

            stats.TrackSent(1000, 500);
            stats.TrackReceived(750);
            stats.Reset();

            Assert.AreEqual(0, stats.TotalBytesSentUncompressed);
            Assert.AreEqual(0, stats.TotalBytesSentCompressed);
            Assert.AreEqual(0, stats.TotalBytesReceived);
            Assert.AreEqual(0, stats.PacketsSent);
            Assert.AreEqual(0, stats.PacketsReceived);
        }

        [Test]
        public void BandwidthStats_ToString_ReturnsFormattedString()
        {
            var stats = new MmBandwidthStats();
            stats.TrackSent(1000, 500);
            stats.TrackReceived(750);

            string result = stats.ToString();

            Assert.IsTrue(result.Contains("Sent:"));
            Assert.IsTrue(result.Contains("Received:"));
            Assert.IsTrue(result.Contains("Compression:"));
        }

        #endregion

        #region Priority Queue Tests

        [Test]
        public void PriorityQueue_Enqueue_IncreasesCount()
        {
            var queue = new MmMessagePriorityQueue();

            queue.Enqueue(new byte[] { 1 }, MmMessagePriority.Normal, 0, MmReliability.Reliable, 0);
            queue.Enqueue(new byte[] { 2 }, MmMessagePriority.Normal, 0, MmReliability.Reliable, 0);

            Assert.AreEqual(2, queue.Count);
        }

        [Test]
        public void PriorityQueue_TryDequeue_ReturnsHighestPriority()
        {
            var queue = new MmMessagePriorityQueue();

            queue.Enqueue(new byte[] { 1 }, MmMessagePriority.Low, 0, MmReliability.Reliable, 0);
            queue.Enqueue(new byte[] { 2 }, MmMessagePriority.Critical, 0, MmReliability.Reliable, 1);
            queue.Enqueue(new byte[] { 3 }, MmMessagePriority.Normal, 0, MmReliability.Reliable, 2);

            Assert.IsTrue(queue.TryDequeue(out var msg));
            Assert.AreEqual(MmMessagePriority.Critical, msg.Priority);
        }

        [Test]
        public void PriorityQueue_TryDequeue_ReturnsFIFOWithinPriority()
        {
            var queue = new MmMessagePriorityQueue();

            queue.Enqueue(new byte[] { 1 }, MmMessagePriority.Normal, 0, MmReliability.Reliable, 0);
            queue.Enqueue(new byte[] { 2 }, MmMessagePriority.Normal, 0, MmReliability.Reliable, 1);
            queue.Enqueue(new byte[] { 3 }, MmMessagePriority.Normal, 0, MmReliability.Reliable, 2);

            queue.TryDequeue(out var msg1);
            queue.TryDequeue(out var msg2);
            queue.TryDequeue(out var msg3);

            Assert.AreEqual(0, msg1.QueuedTime);
            Assert.AreEqual(1, msg2.QueuedTime);
            Assert.AreEqual(2, msg3.QueuedTime);
        }

        [Test]
        public void PriorityQueue_TryDequeue_EmptyQueue_ReturnsFalse()
        {
            var queue = new MmMessagePriorityQueue();

            Assert.IsFalse(queue.TryDequeue(out _));
        }

        [Test]
        public void PriorityQueue_TotalBytes_SumsPayloads()
        {
            var queue = new MmMessagePriorityQueue();

            queue.Enqueue(new byte[100], MmMessagePriority.Normal, 0, MmReliability.Reliable, 0);
            queue.Enqueue(new byte[200], MmMessagePriority.Normal, 0, MmReliability.Reliable, 1);

            Assert.AreEqual(300, queue.TotalBytes);
        }

        [Test]
        public void PriorityQueue_DequeueBatch_RespectsLimit()
        {
            var queue = new MmMessagePriorityQueue();

            queue.Enqueue(new byte[100], MmMessagePriority.Normal, 0, MmReliability.Reliable, 0);
            queue.Enqueue(new byte[100], MmMessagePriority.Normal, 0, MmReliability.Reliable, 1);
            queue.Enqueue(new byte[100], MmMessagePriority.Normal, 0, MmReliability.Reliable, 2);

            var batch = queue.DequeueBatch(150);

            Assert.AreEqual(1, batch.Count); // Only first fits within limit
            Assert.AreEqual(2, queue.Count); // Two remaining
        }

        [Test]
        public void PriorityQueue_DropLowPriority_RemovesLowPriorityMessages()
        {
            var queue = new MmMessagePriorityQueue();

            queue.Enqueue(new byte[] { 1 }, MmMessagePriority.Low, 0, MmReliability.Reliable, 0);
            queue.Enqueue(new byte[] { 2 }, MmMessagePriority.Low, 0, MmReliability.Reliable, 1);
            queue.Enqueue(new byte[] { 3 }, MmMessagePriority.High, 0, MmReliability.Reliable, 2);
            queue.Enqueue(new byte[] { 4 }, MmMessagePriority.Critical, 0, MmReliability.Reliable, 3);

            int dropped = queue.DropLowPriority(2);

            Assert.AreEqual(2, dropped); // Two low priority dropped
            Assert.AreEqual(2, queue.Count); // High and Critical remain
        }

        [Test]
        public void PriorityQueue_Clear_RemovesAllMessages()
        {
            var queue = new MmMessagePriorityQueue();

            queue.Enqueue(new byte[] { 1 }, MmMessagePriority.Normal, 0, MmReliability.Reliable, 0);
            queue.Enqueue(new byte[] { 2 }, MmMessagePriority.High, 0, MmReliability.Reliable, 1);
            queue.Clear();

            Assert.AreEqual(0, queue.Count);
        }

        #endregion

        #region Helper Methods

        private byte[] CreateTestData(int size)
        {
            byte[] data = new byte[size];
            for (int i = 0; i < size; i++)
            {
                data[i] = (byte)(i % 256);
            }
            return data;
        }

        private byte[] CreateCompressibleData(int size)
        {
            // Create data with repeating patterns (compresses well)
            byte[] data = new byte[size];
            for (int i = 0; i < size; i++)
            {
                data[i] = (byte)(i % 10);
            }
            return data;
        }

        private byte[] CreateRandomData(int size)
        {
            byte[] data = new byte[size];
            var random = new System.Random(42);
            random.NextBytes(data);
            return data;
        }

        #endregion
    }
}
