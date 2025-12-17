// Copyright (c) 2017-2025, Columbia University
// All rights reserved.
//
// MmCompression.cs - Compression utilities for network messages
// Provides bandwidth reduction through data compression

using System;
using System.IO;
using System.IO.Compression;

namespace MercuryMessaging.Network
{
    /// <summary>
    /// Compression algorithm selection.
    /// </summary>
    public enum MmCompressionType : byte
    {
        /// <summary>
        /// No compression.
        /// </summary>
        None = 0,

        /// <summary>
        /// GZip compression (good compression, slower).
        /// </summary>
        GZip = 1,

        /// <summary>
        /// Deflate compression (good balance of speed and compression).
        /// </summary>
        Deflate = 2
    }

    /// <summary>
    /// Compression level selection.
    /// </summary>
    public enum MmCompressionLevel : byte
    {
        /// <summary>
        /// Fastest compression (less compression ratio).
        /// </summary>
        Fastest = 0,

        /// <summary>
        /// Optimal balance of speed and compression.
        /// </summary>
        Optimal = 1,

        /// <summary>
        /// No compression (just framing).
        /// </summary>
        NoCompression = 2
    }

    /// <summary>
    /// Configuration for compression behavior.
    /// </summary>
    public class MmCompressionConfig
    {
        /// <summary>
        /// Compression algorithm to use.
        /// </summary>
        public MmCompressionType Type = MmCompressionType.Deflate;

        /// <summary>
        /// Compression level.
        /// </summary>
        public MmCompressionLevel Level = MmCompressionLevel.Optimal;

        /// <summary>
        /// Minimum payload size to compress (smaller payloads have overhead).
        /// </summary>
        public int MinSizeToCompress = 128;

        /// <summary>
        /// Maximum compression ratio - if compressed isn't smaller enough, skip.
        /// </summary>
        public float MinCompressionRatio = 0.9f;

        /// <summary>
        /// Default configuration (optimal balance).
        /// </summary>
        public static MmCompressionConfig Default => new MmCompressionConfig();

        /// <summary>
        /// Fast configuration (prioritize speed).
        /// </summary>
        public static MmCompressionConfig Fast => new MmCompressionConfig
        {
            Level = MmCompressionLevel.Fastest,
            MinSizeToCompress = 256
        };

        /// <summary>
        /// Disabled configuration (no compression).
        /// </summary>
        public static MmCompressionConfig Disabled => new MmCompressionConfig
        {
            Type = MmCompressionType.None
        };
    }

    /// <summary>
    /// Provides compression and decompression for network data.
    /// </summary>
    public static class MmCompression
    {
        /// <summary>
        /// Magic header for compressed packets.
        /// </summary>
        public const byte CompressedPacketMarker = 0xC0;

        /// <summary>
        /// Magic header for uncompressed packets.
        /// </summary>
        public const byte UncompressedPacketMarker = 0x00;

        /// <summary>
        /// Compress data using the specified configuration.
        /// Returns original data if compression isn't beneficial.
        /// </summary>
        public static byte[] Compress(byte[] data, MmCompressionConfig config)
        {
            if (config == null) config = MmCompressionConfig.Default;

            // Skip compression for small payloads or if disabled
            if (config.Type == MmCompressionType.None || data.Length < config.MinSizeToCompress)
            {
                return WrapUncompressed(data);
            }

            byte[] compressed = CompressInternal(data, config);

            // Check if compression is beneficial
            if (compressed.Length >= data.Length * config.MinCompressionRatio)
            {
                return WrapUncompressed(data);
            }

            return WrapCompressed(compressed, config.Type, data.Length);
        }

        /// <summary>
        /// Decompress data, automatically detecting compression.
        /// </summary>
        public static byte[] Decompress(byte[] data)
        {
            if (data == null || data.Length < 2)
            {
                throw new ArgumentException("Invalid compressed data");
            }

            byte marker = data[0];

            if (marker == UncompressedPacketMarker)
            {
                // Uncompressed - just unwrap
                byte[] result = new byte[data.Length - 1];
                Array.Copy(data, 1, result, 0, result.Length);
                return result;
            }
            else if (marker == CompressedPacketMarker)
            {
                // Compressed - read header and decompress
                if (data.Length < 6)
                {
                    throw new ArgumentException("Invalid compressed packet header");
                }

                MmCompressionType type = (MmCompressionType)data[1];
                int originalLength = BitConverter.ToInt32(data, 2);

                byte[] compressedData = new byte[data.Length - 6];
                Array.Copy(data, 6, compressedData, 0, compressedData.Length);

                return DecompressInternal(compressedData, type, originalLength);
            }
            else
            {
                throw new ArgumentException($"Unknown compression marker: 0x{marker:X2}");
            }
        }

        /// <summary>
        /// Check if data is compressed.
        /// </summary>
        public static bool IsCompressed(byte[] data)
        {
            return data != null && data.Length > 0 && data[0] == CompressedPacketMarker;
        }

        /// <summary>
        /// Get compression ratio for the given data.
        /// </summary>
        public static float GetCompressionRatio(byte[] original, byte[] compressed)
        {
            if (original == null || original.Length == 0) return 1.0f;
            return (float)compressed.Length / original.Length;
        }

        private static byte[] WrapUncompressed(byte[] data)
        {
            byte[] result = new byte[data.Length + 1];
            result[0] = UncompressedPacketMarker;
            Array.Copy(data, 0, result, 1, data.Length);
            return result;
        }

        private static byte[] WrapCompressed(byte[] compressed, MmCompressionType type, int originalLength)
        {
            byte[] result = new byte[compressed.Length + 6];
            result[0] = CompressedPacketMarker;
            result[1] = (byte)type;
            byte[] lengthBytes = BitConverter.GetBytes(originalLength);
            Array.Copy(lengthBytes, 0, result, 2, 4);
            Array.Copy(compressed, 0, result, 6, compressed.Length);
            return result;
        }

        private static byte[] CompressInternal(byte[] data, MmCompressionConfig config)
        {
            CompressionLevel level = config.Level switch
            {
                MmCompressionLevel.Fastest => System.IO.Compression.CompressionLevel.Fastest,
                MmCompressionLevel.Optimal => System.IO.Compression.CompressionLevel.Optimal,
                _ => System.IO.Compression.CompressionLevel.NoCompression
            };

            using (var outputStream = new MemoryStream())
            {
                Stream compressionStream = config.Type switch
                {
                    MmCompressionType.GZip => new GZipStream(outputStream, level, leaveOpen: true),
                    MmCompressionType.Deflate => new DeflateStream(outputStream, level, leaveOpen: true),
                    _ => throw new ArgumentException($"Unsupported compression type: {config.Type}")
                };

                using (compressionStream)
                {
                    compressionStream.Write(data, 0, data.Length);
                }

                return outputStream.ToArray();
            }
        }

        private static byte[] DecompressInternal(byte[] data, MmCompressionType type, int originalLength)
        {
            using (var inputStream = new MemoryStream(data))
            {
                Stream decompressionStream = type switch
                {
                    MmCompressionType.GZip => new GZipStream(inputStream, CompressionMode.Decompress),
                    MmCompressionType.Deflate => new DeflateStream(inputStream, CompressionMode.Decompress),
                    _ => throw new ArgumentException($"Unsupported compression type: {type}")
                };

                using (decompressionStream)
                using (var outputStream = new MemoryStream(originalLength))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = decompressionStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                    }
                    return outputStream.ToArray();
                }
            }
        }
    }

    /// <summary>
    /// Bandwidth statistics tracker for network optimization.
    /// </summary>
    public class MmBandwidthStats
    {
        /// <summary>
        /// Total bytes sent (uncompressed).
        /// </summary>
        public long TotalBytesSentUncompressed { get; private set; }

        /// <summary>
        /// Total bytes sent (compressed/actual).
        /// </summary>
        public long TotalBytesSentCompressed { get; private set; }

        /// <summary>
        /// Total bytes received (compressed/actual).
        /// </summary>
        public long TotalBytesReceived { get; private set; }

        /// <summary>
        /// Total packets sent.
        /// </summary>
        public long PacketsSent { get; private set; }

        /// <summary>
        /// Total packets received.
        /// </summary>
        public long PacketsReceived { get; private set; }

        /// <summary>
        /// Average compression ratio (lower is better).
        /// </summary>
        public float AverageCompressionRatio =>
            TotalBytesSentUncompressed > 0
                ? (float)TotalBytesSentCompressed / TotalBytesSentUncompressed
                : 1.0f;

        /// <summary>
        /// Bytes saved by compression.
        /// </summary>
        public long BytesSaved => TotalBytesSentUncompressed - TotalBytesSentCompressed;

        /// <summary>
        /// Track a sent packet.
        /// </summary>
        public void TrackSent(int uncompressedSize, int compressedSize)
        {
            TotalBytesSentUncompressed += uncompressedSize;
            TotalBytesSentCompressed += compressedSize;
            PacketsSent++;
        }

        /// <summary>
        /// Track a received packet.
        /// </summary>
        public void TrackReceived(int size)
        {
            TotalBytesReceived += size;
            PacketsReceived++;
        }

        /// <summary>
        /// Reset all statistics.
        /// </summary>
        public void Reset()
        {
            TotalBytesSentUncompressed = 0;
            TotalBytesSentCompressed = 0;
            TotalBytesReceived = 0;
            PacketsSent = 0;
            PacketsReceived = 0;
        }

        /// <summary>
        /// Get formatted statistics string.
        /// </summary>
        public override string ToString()
        {
            return $"Sent: {PacketsSent} packets ({FormatBytes(TotalBytesSentCompressed)} / {FormatBytes(TotalBytesSentUncompressed)} uncompressed), " +
                   $"Received: {PacketsReceived} packets ({FormatBytes(TotalBytesReceived)}), " +
                   $"Compression: {AverageCompressionRatio:P1}, Saved: {FormatBytes(BytesSaved)}";
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes}B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1}KB";
            return $"{bytes / (1024.0 * 1024.0):F1}MB";
        }
    }
}
